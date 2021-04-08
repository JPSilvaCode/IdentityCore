﻿using ICWebAPI.Extensions;
using ICWebAPI.Models;
using ICWebAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ICWebAPI.Controllers
{
    [Route("[controller]")]
    public class AccountController : MainController
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppSettings _appSettings;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IOptions<AppSettings> appSettings, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _appSettings = appSettings.Value;
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
                "Por favor, confirme sua conta clicando aqui <a href=\"" + Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token }, Request.Scheme) + "\">here</a>");

            var userReturn = new User
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email
            };

            return Created(Url.Action("GetUserById", "Account", new { id = user.Id }, Request.Scheme), userReturn);
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
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginUser loginUser)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var result = await _signInManager.PasswordSignInAsync(loginUser.Email, loginUser.Password, false, true);

            if (result.Succeeded)
                return CustomResponse(await GetJwt(loginUser.Email));

            if (!await _userManager.IsEmailConfirmedAsync(await _userManager.FindByEmailAsync(loginUser.Email)))
            {
                AddError("O e-mail não foi confirmado, confirme primeiro");
                return CustomResponse();
            }

            if (result.IsLockedOut)
            {
                AddError("Usuário temporariamente bloqueado por tentativas inválidas");
                return CustomResponse();
            }

            AddError("Usuário ou Senha incorretos");
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

        private async Task<UserResponseLogin> GetJwt(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);

            var identityClaims = TokenService.GetClaimsUser(claims, user, await _userManager.GetRolesAsync(user));
            var encodedToken = TokenService.GenerateToken(_appSettings, identityClaims);

            return GetResponseToken(encodedToken, user, claims);
        }

        private UserResponseLogin GetResponseToken(string encodedToken, IdentityUser user, IEnumerable<Claim> claims)
        {
            return new UserResponseLogin
            {
                AccessToken = encodedToken,
                ExpiresIn = TimeSpan.FromHours(_appSettings.ExpiracaoHoras).TotalSeconds,
                UsuarioToken = new UserToken
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new UserClaim { Type = c.Type, Value = c.Value })
                }
            };
        }
    }
}
