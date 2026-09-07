using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SIGTI.Application.Common.Interfaces.Services;
using SIGTI.Domain.Entities;
using SIGTI.Infrastructure.Authentication;

namespace SIGTI.Infrastructure.Services
{
    public sealed class JwtTokenGenerator : IJwtTokenGenerator
    {
        private readonly JwtOptions _options;

        public JwtTokenGenerator(IOptions<JwtOptions> options)
        {
            _options = options.Value;
        }

        public (string Token, DateTime ExpiresAt) GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_options.SecretKey)
            );
            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );
            var expiresAt = DateTime.UtcNow.AddMinutes(
                _options.ExpirationMinutes
            );

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(JwtRegisteredClaimNames.Email, user.Email.Value),
                new(JwtRegisteredClaimNames.Name, user.Name),
                new(ClaimTypes.Role, user.Role.ToString()),
                new("departmentId", user.DepartmentId.ToString()),
            };

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _options.Issuer,
                audience: _options.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(
                tokenDescriptor
            );
            return (tokenString, expiresAt);
        }
    }
}
