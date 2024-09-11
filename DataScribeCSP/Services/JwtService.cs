using DataScribeCSP.Models;
using DataScribeCSP.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DataScribeCSP.Services
{
    public class JwtService : IJwtService
    {
        private readonly JwtOptions _jwtOptions;
        private IConfiguration _config;

        public JwtService(IOptions<JwtOptions> jwtOptions, IConfiguration config)
        {
            _jwtOptions = jwtOptions.Value;
            _config = config;
        }

        public string GenerateToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var authClaims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var sectoken = new JwtSecurityToken(
                _jwtOptions.Issuer,
              _jwtOptions.Audience,
              claims: new List<Claim>
              {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
              },
              null,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials
              );

            var token = new JwtSecurityTokenHandler().WriteToken(sectoken);

            return token;
        }
    }


    public interface IJwtService
    {
        string GenerateToken(User user);
    }
}
