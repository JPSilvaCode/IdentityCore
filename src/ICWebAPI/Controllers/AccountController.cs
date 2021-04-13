using ICWebAPI.Models;
using ICWebAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace ICWebAPI.Controllers
{
    [Route("[controller]")]
    public class AccountController : MainController
    {
        private readonly AuthenticationService _authenticationService;

        public AccountController(AuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginUser loginUser)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var result = await _authenticationService.SignInManager.PasswordSignInAsync(loginUser.Email, loginUser.Password, false, true);

            if (!result.Succeeded)
            {
                AddError("Usuário ou Senha incorretos");
                return CustomResponse();
            }

            if (result.IsLockedOut)
            {
                AddError("Usuário temporariamente bloqueado por tentativas inválidas");
                return CustomResponse();
            }

            if (!await _authenticationService.UserManager.IsEmailConfirmedAsync(await _authenticationService.UserManager.FindByEmailAsync(loginUser.Email)))
            {
                AddError("O e-mail não foi confirmado, confirme primeiro");
                return CustomResponse();
            }

            return CustomResponse(await _authenticationService.GetJwt(loginUser.Email));
        }

        [HttpPost]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken(string refreshToken)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                AddError("Refresh Token inválido");
                return CustomResponse();
            }

            var token = await _authenticationService.GetRefreshToken(Guid.Parse(refreshToken));

            if (token is not null) return CustomResponse(await _authenticationService.GetJwt(token.Username));

            AddError("Refresh Token expirado");
            return CustomResponse();
        }
    }
}
