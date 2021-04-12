using System;

namespace ICWebAPI.Models
{
    public class UserResponseLogin
    {
        public string AccessToken { get; set; }
        public string Created { get; set; }
        public string Expiration { get; set; }
        public double ExpiresIn { get; set; }
        public UserToken UsuarioToken { get; set; }
        public Guid RefreshToken { get; set; }
    }
}