using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace AuthenticationBase
{
    public class AuthOptions
    {
        public const string Security = "Security";

        public string Issuer { get; set; }
        public string Audience { get; set; }
        public string Secret { get; set; }
        public int MaxAccessFailedCount { get; set; }
        public int? LifeTime { get; set; }

        public SymmetricSecurityKey GetSymmetricSecurityKey()
        {
            return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Secret));
        }
    }
}
