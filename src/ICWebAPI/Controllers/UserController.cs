using ICWebAPI.Models;
using ICWebAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ICWebAPI.Controllers
{
    [Route("[controller]")]
    public class UserController : MainController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailSender;

        public UserController(UserManager<IdentityUser> userManager, IEmailService emailSender, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _roleManager = roleManager;
        }

        [HttpGet]
        [Route("users")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUsers()
        {
            var users = await _userManager.Users.ToListAsync();

            var usersReturn = (from user in users
                               select new
                               {
                                   user.Id,
                                   user.UserName,
                                   user.Email
                               }).ToList().Select(p => new User
                               {
                                   Id = p.Id,
                                   UserName = p.UserName,
                                   Email = p.Email
                               });

            return Ok(usersReturn);
        }

        [HttpGet]
        [Route("user/{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id.Equals(id.ToString()));

            if (user == null) return NotFound();

            var userReturn = new User
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };

            return Ok(userReturn);
        }

        [HttpGet]
        [Route("user/{username}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetUserByName(string username)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.UserName.Equals(username));

            if (user == null) return NotFound();

            var userReturn = new User
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };

            return Ok(userReturn);
        }

        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public async Task<ActionResult> Register(RegisterUser registerUser)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var user = new IdentityUser
            {
                UserName = registerUser.Email,
                Email = registerUser.Email
            };

            var result = await _userManager.CreateAsync(user, registerUser.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    AddError(error.Description);

                return CustomResponse();
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            await _emailSender.SendEmailAsync(user.Email,
                "Confirme sua conta",
                "Por favor, confirme sua conta clicando aqui <a href=\"" + Url.Action("ConfirmEmail", "User", new { userId = user.Id, token }, Request.Scheme) + "\">here</a>");

            var userReturn = new User
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };

            return Created(Url.Action("GetUserById", "User", new { id = user.Id }, Request.Scheme), userReturn);
        }

        [HttpGet]
        [Route("ConfirmEmail")]
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId = "", string token = "")
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                AddError("ID de usuário e código são obrigatórios");
                return CustomResponse();
            }

            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id.Equals(userId));

            if (user == null) return NotFound();

            IdentityResult result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                var userReturn = new User
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email
                };

                return Ok(userReturn);
            }

            foreach (var error in result.Errors)
            {
                AddError(error.Description);
            }

            return CustomResponse();
        }

        [HttpPost]
        [Route("ChangePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePassword model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _userManager.ChangePasswordAsync(await _userManager.GetUserAsync(User), model.OldPassword, model.NewPassword);

            if (result.Succeeded) return Ok();

            foreach (var error in result.Errors)
                AddError(error.Description);

            return CustomResponse();
        }

        [HttpDelete]
        [Route("user/{id:guid}")]
        [Authorize]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);

            if (user == null) return NotFound();

            var result = await _userManager.DeleteAsync(user);

            if (result.Succeeded) return Ok();

            foreach (var error in result.Errors)
                AddError(error.Description);

            return CustomResponse();
        }

        [HttpGet]
        [Route("user/{id:guid}/roles")]
        public async Task<IActionResult> GetRolesTouser(string id)
        {
            var appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            var rolesUser = await _userManager.GetRolesAsync(appUser);

            if (rolesUser == null) return NotFound();

            return Ok(rolesUser);
        }

        [HttpPost]
        [Route("user/{id:guid}/role/{role}")]
        public async Task<IActionResult> AssignRoleToUser(string id, string role)
        {
            var appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            var roles = await _roleManager.Roles.ToListAsync();

            if (!roles.Any(r => r.Name.Equals(role)))
            {
                AddError($"Roles '{role}' não existe no sistema");
                return CustomResponse();
            }

            var currentRolesUser = await _userManager.GetRolesAsync(appUser);

            if (currentRolesUser.Any(r => r.Equals(role)))
            {
                AddError($"Role '{role}' já existe para o usuário");
                return CustomResponse();
            }

            var addResult = await _userManager.AddToRoleAsync(appUser, role);

            if (addResult.Succeeded) return Ok();

            foreach (var error in addResult.Errors)
                AddError(error.Description);

            return CustomResponse();
        }

        [HttpPost]
        [Route("user/{id:guid}/roles")]
        public async Task<IActionResult> AssignRolesToUser([FromRoute] string id, [FromBody] string[] rolesToAssign)
        {
            var appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            var rolesNotExists = rolesToAssign.Except(_roleManager.Roles.Select(x => x.Name)).ToArray();

            if (rolesNotExists.Any())
            {
                AddError($"Roles '{string.Join(",", rolesNotExists)}' não existem no sistema");
                return CustomResponse();
            }

            var currentRolesUser = await _userManager.GetRolesAsync(appUser);

            var removeResult = await _userManager.RemoveFromRolesAsync(appUser, currentRolesUser.ToArray());

            if (!removeResult.Succeeded)
            {
                foreach (var error in removeResult.Errors)
                    AddError(error.Description);

                return CustomResponse();
            }

            var addResult = await _userManager.AddToRolesAsync(appUser, rolesToAssign);

            if (addResult.Succeeded) return Ok();

            foreach (var error in addResult.Errors)
                AddError(error.Description);

            return CustomResponse();
        }

        [HttpDelete]
        [Route("user/{id:guid}/role/{role}")]
        public async Task<IActionResult> RemoveRoleToUser(string id, string role)
        {
            var appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            var roles = await _roleManager.Roles.ToListAsync();

            if (!roles.Any(r => r.Name.Equals(role)))
            {
                AddError($"Roles '{role}' não existe no sistema");
                return CustomResponse();
            }

            var currentRolesUser = await _userManager.GetRolesAsync(appUser);

            if (!currentRolesUser.Any(r => r.Equals(role)))
            {
                AddError($"Role '{role}' não existe para o usuário");
                return CustomResponse();
            }

            var addResult = await _userManager.RemoveFromRoleAsync(appUser, role);

            if (addResult.Succeeded) return Ok();

            foreach (var error in addResult.Errors)
                AddError(error.Description);

            return CustomResponse();
        }

        [HttpGet]
        [Route("user/{id:guid}/claims")]
        public async Task<IActionResult> GetClaimsTouser(string id)
        {
            var appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            var claimsUser = await _userManager.GetClaimsAsync(appUser);

            if (claimsUser == null) return NotFound();

            IList<ClaimBinding> claims = null;
            foreach (var claimUser in claimsUser)
            {
                claims ??= new List<ClaimBinding>();
                claims.Add(new ClaimBinding { Type = claimUser.Type, Value = claimUser.Value });
            }

            return Ok(claims);
        }

        [HttpPost]
        [Route("user/{id:guid}/claim/{type}/{value}")]
        public async Task<IActionResult> AssignClaimToUser(string id, string type, string value)
        {
            var appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            var claimsUser = await _userManager.GetClaimsAsync(appUser);
            if (claimsUser.Any(c => c.Type == type))
                await _userManager.RemoveClaimAsync(appUser, claimsUser.SingleOrDefault(c => c.Type == type));

            await _userManager.AddClaimAsync(appUser, new Claim(type, value));

            return Ok();
        }

        [HttpPost]
        [Route("user/{id:guid}/claims")]
        public async Task<IActionResult> AssignClaimsToUser([FromRoute] string id, [FromBody] List<ClaimBinding> claimsBinding)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            foreach (var claimBinding in claimsBinding)
            {
                var claimsUser = await _userManager.GetClaimsAsync(appUser);
                if (claimsUser.Any(c => c.Type == claimBinding.Type))
                    await _userManager.RemoveClaimAsync(appUser, claimsUser.SingleOrDefault(c => c.Type == claimBinding.Type));

                await _userManager.AddClaimAsync(appUser, new Claim(claimBinding.Type, claimBinding.Value));
            }

            return Ok();
        }

        [HttpDelete]
        [Route("user/{id:guid}/claims")]
        public async Task<IActionResult> RemoveClaimsFromUser([FromRoute] string id, [FromBody] List<ClaimBinding> claimsBinding)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            foreach (var claimBinding in claimsBinding)
            {
                var claimsUser = await _userManager.GetClaimsAsync(appUser);
                if (claimsUser.Any(c => c.Type == claimBinding.Type))
                    await _userManager.RemoveClaimAsync(appUser, claimsUser.SingleOrDefault(c => c.Type == claimBinding.Type));
            }

            return Ok();
        }

        [HttpDelete]
        [Route("user/{id:guid}/claim/{type}")]
        public async Task<IActionResult> RemoveClaimFromUser(string id, string type)
        {
            var appUser = await _userManager.FindByIdAsync(id);

            if (appUser == null) return NotFound();

            var claimsUser = await _userManager.GetClaimsAsync(appUser);
            if (claimsUser.Any(c => c.Type == type))
                await _userManager.RemoveClaimAsync(appUser, claimsUser.SingleOrDefault(c => c.Type == type));

            return Ok();
        }
    }
}
