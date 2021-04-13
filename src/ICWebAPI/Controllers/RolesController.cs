using ICWebAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace ICWebAPI.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [Authorize(Roles = "Admin")]
    public class RolesController : MainController
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<IdentityUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetRoleById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null) return NotFound();

            return Ok(role);
        }

        [HttpGet("{name}")]
        public async Task<IActionResult> GetRoleByName(string name)
        {
            var role = await _roleManager.FindByNameAsync(name);

            if (role == null) return NotFound();

            return Ok(role);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleManager.Roles.ToListAsync();

            if (roles == null) return NotFound();

            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> Post(RegisterRole registerRole)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var result = await _roleManager.CreateAsync(new IdentityRole { Name = registerRole.Name });

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    AddError(error.Description);

                return CustomResponse();
            }

            var role = await _roleManager.FindByNameAsync(registerRole.Name);

            return Created(Url.Action("GetRoleById", "Roles", new { id = role.Id }, Request.Scheme), role);
        }

        [HttpPut]
        public async Task<IActionResult> Put(ChangeRole changeRole)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var role = await _roleManager.FindByNameAsync(changeRole.OldName);

            if (role == null) return NotFound();

            role.Name = changeRole.NewName;

            var result = await _roleManager.UpdateAsync(role);

            if (result.Succeeded) return Created(Url.Action("GetRoleById", "Roles", new { id = role.Id }, Request.Scheme), role);

            foreach (var error in result.Errors)
                AddError(error.Description);

            return CustomResponse();

        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteById(string id)
        {
            var role = await _roleManager.FindByIdAsync(id);

            if (role == null) return NotFound();

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded) return Ok();

            foreach (var error in result.Errors)
                AddError(error.Description);

            return CustomResponse();
        }

        [HttpDelete("{name}")]
        public async Task<IActionResult> DeleteByName(string name)
        {
            var role = await _roleManager.FindByNameAsync(name);

            if (role == null) return NotFound();

            var result = await _roleManager.DeleteAsync(role);

            if (result.Succeeded) return Ok();

            foreach (var error in result.Errors)
                AddError(error.Description);

            return CustomResponse();
        }

        [HttpPost]
        [Route("ManageUsersInRole")]
        public async Task<IActionResult> ManageUsersInRole(UsersInRole usersInRole)
        {
            var role = await _roleManager.FindByIdAsync(usersInRole.RoleId);

            if (role == null) return NotFound();

            foreach (var user in usersInRole.EnrolledUsers)
            {
                var appUser = await _userManager.FindByIdAsync(user);

                if (appUser == null)
                {
                    AddError($"User: {user} does not exists");
                    continue;
                }

                if (await _userManager.IsInRoleAsync(appUser, role.Name)) continue;

                var result = await _userManager.AddToRoleAsync(appUser, role.Name);

                if (!result.Succeeded) AddError($"User: {user} could not be added to role");
            }

            foreach (var user in usersInRole.RemovedUsers)
            {
                var appUser = await _userManager.FindByIdAsync(user);

                if (appUser == null)
                {
                    AddError($"User: {user} does not exists");
                    continue;
                }

                var result = await _userManager.RemoveFromRoleAsync(appUser, role.Name);

                if (!result.Succeeded) AddError($"User: {user} could not be removed from role");
            }

            return !IsOperationValid() ? CustomResponse() : Ok();
        }
    }
}
