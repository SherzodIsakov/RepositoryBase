using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AuthenticationBase
{
    public class AppSecurity
    {
        private readonly AuthOptions _authOptions;

        public AppSecurity(IOptions<AuthOptions> authOptions)
        {
            _authOptions = authOptions.Value;
        }

        /// <summary>
        /// Возвращает токен для указанных <paramref name="userId"/> и <paramref name="userSessionId"/>
        /// </summary>
        /// <param name="userId">Id пользователя в системе</param>
        /// <param name="userSessionId">Id текущей сессии пользователя в системе</param>
        /// <param name="from">Дата и время начала действия токена</param>
        /// <param name="additionalClaims"></param>
        /// <returns></returns>
        public string GetToken(string userId, DateTime from, IEnumerable<string> roles, IDictionary<string, string> additionalClaims = null)
        {
            var userIdentity = GetIdentity(userId, roles, additionalClaims);

            return GetJwtToken(from, userIdentity.Claims);
        }

        /// <summary>
        /// Получает объект идентификации пользователя, содержащий все утверждения (claim) о нём. 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="userSessionId"></param>
        /// <param name="additionalClaims"></param>
        /// <returns></returns>
        private ClaimsIdentity GetIdentity(string userId, IEnumerable<string> roles, IDictionary<string, string> additionalClaims)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, userId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            claims.AddRange(roles.Select(userRole => new Claim(ClaimTypes.Role, userRole)));

            if (additionalClaims != null)
            {
                claims.AddRange(additionalClaims.Select(x => new Claim(x.Key, x.Value)));
            }

            var claimsIdentity = new ClaimsIdentity(claims, "Token");
            return claimsIdentity;
        }

        private string GetJwtToken(DateTime from, IEnumerable<Claim> claims)
        {
            var expires = from.Add(TimeSpan.FromMinutes(_authOptions.LifeTime ?? 1440));

            var authLoginKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_authOptions.Secret));

            var token = new JwtSecurityToken(
                issuer: _authOptions.Issuer,
                audience: _authOptions.Audience,
                expires: expires,
                claims: claims,
                signingCredentials: new SigningCredentials(authLoginKey, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

}
