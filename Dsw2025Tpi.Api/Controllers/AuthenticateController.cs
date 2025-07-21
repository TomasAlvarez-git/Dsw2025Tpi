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
        // Servicios para manejar usuarios, inicio de sesión, roles y generación de tokens JWT
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JwtTokenService _jwtTokenService;

        // Constructor con inyección de dependencias
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

        // Endpoint para login de usuario (POST /api/auth/login)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {
            try
            {
                // Buscar el usuario por nombre
                var user = await _userManager.FindByNameAsync(request.Username);
                if (user == null)
                {
                    return Unauthorized("Usuario o contraseña incorrectos");
                }

                // Verificar la contraseña del usuario
                var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
                if (!result.Succeeded)
                {
                    return Unauthorized("Usuario o contraseña incorrectos");
                }

                // Obtener los roles asociados al usuario
                var roles = await _userManager.GetRolesAsync(user);
                var userRole = roles.FirstOrDefault() ?? "Customer"; // Si no tiene roles, se asigna 'Customer'

                // Generar token JWT para el usuario
                var token = _jwtTokenService.GenerateToken(request.Username, userRole);

                // Devolver el token al cliente
                return Ok(new { token });
            }
            catch (ArgumentException ae)
            {
                // Manejo de errores por argumentos inválidos
                return BadRequest($"Error de entrada: {ae.Message}");
            }
            catch (Exception ex)
            {
                // Manejo de errores inesperados
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // Endpoint para registrar un usuario administrador (POST /api/auth/registerAdmin)
        [HttpPost("registerAdmin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            try
            {
                // Validación básica de datos requeridos
                if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email))
                    return BadRequest("El nombre de usuario y el email son obligatorios.");

                // Verifica si el email ya está registrado
                var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingUserByEmail != null)
                    return BadRequest("Ya existe un usuario registrado con ese email.");

                // Verifica si el nombre de usuario ya está en uso
                var existingUserByUsername = await _userManager.FindByNameAsync(model.Username);
                if (existingUserByUsername != null)
                    return BadRequest("El nombre de usuario ya está en uso.");

                // Crear el usuario con los datos recibidos
                var user = new IdentityUser { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                    return BadRequest(result.Errors); // Devuelve errores si falla la creación

                // Asignar el rol "Admin" al nuevo usuario
                var roleResult = await _userManager.AddToRoleAsync(user, "Admin");
                if (!roleResult.Succeeded)
                    return BadRequest(roleResult.Errors);

                return Ok("Usuario registrado correctamente con rol de administrador.");
            }
            catch (Exception ex)
            {
                // Manejo de errores inesperados
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // Endpoint para registrar un usuario cliente (POST /api/auth/registerCustomer)
        [HttpPost("registerCustomer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterModel model)
        {
            try
            {
                // Validación básica de datos requeridos
                if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email))
                    return BadRequest("El nombre de usuario y el email son obligatorios.");

                // Verifica si el email ya está registrado
                var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
                if (existingUserByEmail != null)
                    return BadRequest("Ya existe un usuario registrado con ese email.");

                // Verifica si el nombre de usuario ya está en uso
                var existingUserByUsername = await _userManager.FindByNameAsync(model.Username);
                if (existingUserByUsername != null)
                    return BadRequest("El nombre de usuario ya está en uso.");

                // Crear el usuario con los datos recibidos
                var user = new IdentityUser { UserName = model.Username, Email = model.Email };
                var result = await _userManager.CreateAsync(user, model.Password);

                if (!result.Succeeded)
                    return BadRequest(result.Errors); // Devuelve errores si falla la creación

                // Asignar el rol "Customer" al nuevo usuario
                var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
                if (!roleResult.Succeeded)
                    return BadRequest(roleResult.Errors);

                return Ok("Usuario registrado correctamente con rol de cliente.");
            }
            catch (Exception ex)
            {
                // Manejo de errores inesperados
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}

