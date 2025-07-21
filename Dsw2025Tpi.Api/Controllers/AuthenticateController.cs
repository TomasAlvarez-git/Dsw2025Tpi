using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Dsw2025Tpi.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthenticateController : ControllerBase
    {
        //Estos dos primeros servicios son para la autenticación y autorización de usuarios
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtTokenService _jwtTokenService;

        public AuthenticateController(UserManager<IdentityUser> userManager,
                                      SignInManager<IdentityUser> signInManager,
                                      JwtTokenService jwtTokenService,
                                      RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _roleManager = roleManager;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(request.Username);
                if (user == null)
                {
                    return Unauthorized("Usuario o contraseña incorrectos");
                }

                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded)
                {
                    return Unauthorized("Usuario o contraseña incorrectos");
                }

                // Obtener el rol del usuario
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault() ?? "Customer"; // Valor por defecto

                var token = _jwtTokenService.GenerateToken(request.Username, userRole);

                return Ok(new { token });
            }
            catch (ArgumentException ae)
            {
                // Errores relacionados con argumentos inválidos (por ejemplo, usuario corrupto)
                return BadRequest($"Error de entrada: {ae.Message}");
            }
            catch (Exception ex)
            {
                // Errores inesperados
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        [HttpPost("registerAdmin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email))
                    return BadRequest("El nombre de usuario y el email son obligatorios.");

                var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingUserByEmail != null)
                    return BadRequest("Ya existe un usuario registrado con ese email.");

                var existingUserByUsername = await _userManager.FindByNameAsync(model.Username);
                if (existingUserByUsername != null)
                    return BadRequest("El nombre de usuario ya está en uso.");

                var user = new IdentityUser { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                if (!roleResult.Succeeded)
                    return BadRequest(roleResult.Errors);

                return Ok("Usuario registrado correctamente con rol de administrador.");
            }
            catch (Exception ex)
            {
                // Podés loguear el error si tenés un logger, por ejemplo: _logger.LogError(ex, "Error al registrar admin.");
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        [HttpPost("registerCustomer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email))
                    return BadRequest("El nombre de usuario y el email son obligatorios.");

                var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingUserByEmail != null)
                    return BadRequest("Ya existe un usuario registrado con ese email.");

                var existingUserByUsername = await _userManager.FindByNameAsync(model.Username);
                if (existingUserByUsername != null)
                    return BadRequest("El nombre de usuario ya está en uso.");

                var user = new IdentityUser { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
                if (!roleResult.Succeeded)
                    return BadRequest(roleResult.Errors);

                return Ok("Usuario registrado correctamente con rol de cliente.");
            }
            catch (Exception ex)
            {
                // Podés loguear el error si usás ILogger
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

    }
}
