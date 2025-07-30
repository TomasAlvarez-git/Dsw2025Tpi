using Dsw2025Tpi.Application.Exceptions;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Dsw2025Tpi.Application.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _config;

        // Constructor que recibe la configuración de la aplicación (por ejemplo, appsettings.json)
        public JwtTokenService(IConfiguration config)
        {
            _config = config;
        }

        // Método para generar un token JWT a partir de un nombre de usuario y un rol
        public string GenerateToken(string username, string role)
        {
            // Obtiene la sección "Jwt" de la configuración para acceder a datos como clave, issuer, audiencia, etc.
            var jwtConfig = _config.GetSection("Jwt");

            // Obtiene la clave secreta para firmar el token. Si no está configurada, lanza excepción
            var keyText = jwtConfig["Key"] ?? throw new NotFoundException("Jwt Key");

            // Crea una clave simétrica usando la clave secreta (como bytes UTF8)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyText));

            // Define las credenciales de firma del token usando HMAC SHA256
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Define las reclamaciones (claims) que irán dentro del token: el usuario, un ID único y el rol
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),        // Sujeto del token: el username
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()), // ID único para evitar reutilización
                new Claim(ClaimTypes.Role, role)                          // Rol del usuario
            };

            // Crea el token JWT con los parámetros: issuer, audience, claims, fecha de expiración y credenciales
            var token = new JwtSecurityToken(
                issuer: jwtConfig["Issuer"],
                audience: jwtConfig["Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(double.Parse(jwtConfig["ExpireInMinutes"] ?? "60")),
                signingCredentials: creds
            );

            // Serializa el token a una cadena compacta y la retorna
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
