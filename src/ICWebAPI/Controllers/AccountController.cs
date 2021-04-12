using ICWebAPI.Extensions;
using ICWebAPI.Models;
using ICWebAPI.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using ICWebAPI.Data;
using Microsoft.EntityFrameworkCore;

namespace ICWebAPI.Controllers
{
    [Route("[controller]")]
    public class AccountController : MainController
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly AppSettings _appSettings;
        private readonly ICMemoryContext _context;
        private readonly AppTokenSettings _appTokenSettingsSettings;

        public AccountController(UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, IOptions<AppSettings> appSettings, ICMemoryContext context, IOptions<AppTokenSettings> appTokenSettingsSettings)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _appTokenSettingsSettings = appTokenSettingsSettings.Value;
            _appSettings = appSettings.Value;
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginUser loginUser)
        {
            if (!ModelState.IsValid) return CustomResponse(ModelState);

            var result = await _signInManager.PasswordSignInAsync(loginUser.Email, loginUser.Password, false, true);

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

            if (!await _userManager.IsEmailConfirmedAsync(await _userManager.FindByEmailAsync(loginUser.Email)))
            {
                AddError("O e-mail não foi confirmado, confirme primeiro");
                return CustomResponse();
            }

            return CustomResponse(await GetJwt(loginUser.Email));
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

            var token = await GetRefreshToken(Guid.Parse(refreshToken));

            if (token is not null) return CustomResponse(await GetJwt(token.Username));

            AddError("Refresh Token expirado");
            return CustomResponse();
        }

        private async Task<UserResponseLogin> GetJwt(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            var claims = await _userManager.GetClaimsAsync(user);

            var identityClaims = TokenService.GetClaimsUser(claims, user, await _userManager.GetRolesAsync(user));
            var encodedToken = TokenService.GenerateToken(_appSettings, identityClaims);

            var refreshToken = await GerarRefreshToken(email);

            return GetResponseToken(encodedToken, user, claims, refreshToken);
        }

        private UserResponseLogin GetResponseToken(string encodedToken, IdentityUser user, IEnumerable<Claim> claims, RefreshToken refreshToken)
        {
            return new UserResponseLogin
            {
                AccessToken = encodedToken,
                Created = DateTime.UtcNow.ToString("dd/MM/yyyy HH:mm:ss"),
                Expiration = DateTime.UtcNow.AddSeconds(_appSettings.Expiracao).ToString("dd/MM/yyyy HH:mm:ss"),
                ExpiresIn = TimeSpan.FromSeconds(_appSettings.Expiracao).TotalSeconds,
                UsuarioToken = new UserToken
                {
                    Id = user.Id,
                    Email = user.Email,
                    Claims = claims.Select(c => new UserClaim { Type = c.Type, Value = c.Value })
                },
                RefreshToken = refreshToken.Token,
            };
        }

        private async Task<RefreshToken> GerarRefreshToken(string email)
        {
            var refreshToken = new RefreshToken
            {
                Username = email,
                ExpirationDate = DateTime.UtcNow.AddSeconds(_appTokenSettingsSettings.RefreshTokenExpiration)
            };

            _context.RefreshTokens.RemoveRange(_context.RefreshTokens.Where(u => u.Username == email));
            await _context.RefreshTokens.AddAsync(refreshToken);

            await _context.SaveChangesAsync();

            return refreshToken;
        }

        private async Task<RefreshToken> GetRefreshToken(Guid refreshToken)
        {
            var token = await _context.RefreshTokens.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Token == refreshToken);

            return token != null && token.ExpirationDate.ToLocalTime() > DateTime.Now
                ? token
                : null;
        }
    }
}
