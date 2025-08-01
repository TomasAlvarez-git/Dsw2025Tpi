using Dsw2025Tpi.Application.Dtos;
using Dsw2025Tpi.Application.Exceptions;
using Dsw2025Tpi.Application.Services;
using Dsw2025Tpi.Data;
using Dsw2025Tpi.Domain.Entities;
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
        private readonly Dsw2025TpiContext _context;

        // Constructor con inyección de dependencias
        public AuthenticateController(UserManager<IdentityUser> userManager,
                                      SignInManager<IdentityUser> signInManager,
                                      JwtTokenService jwtTokenService,
                                      RoleManager<IdentityRole> roleManager,
                                      Dsw2025TpiContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtTokenService = jwtTokenService;
            _roleManager = roleManager;
            _context = context;
        }

        // Endpoint para login de usuario (POST /api/auth/login)
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel request)
        {

            // Buscar el usuario por nombre
            var user = await _userManager.FindByNameAsync(request.Username) ?? throw new UnauthorizedException("Usuario o contraseña incorrectos");

            // Verificar la contraseña del usuario
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                throw new UnauthorizedException("Usuario o contraseña incorrectos");
            }

            // Obtener los roles asociados al usuario
            var roles = await _userManager.GetRolesAsync(user);
            var userRole = roles.FirstOrDefault() ?? "Customer"; // Si no tiene roles, se asigna 'Customer'

            // Generar token JWT para el usuario
            var token = _jwtTokenService.GenerateToken(request.Username, userRole);

            // Devolver el token al cliente
            return Ok(new { token });

        }

        // Endpoint para registrar un usuario administrador (POST /api/auth/registerAdmin)
        [HttpPost("registerAdmin")]
        public async Task<IActionResult> RegisterAdmin([FromBody] RegisterModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email))
                throw new BadRequestException("El nombre de usuario y el email son obligatorios.");

            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
                throw new BadRequestException("Ya existe un usuario registrado con ese email.");

            var existingUserByUsername = await _userManager.FindByNameAsync(model.Username);
            if (existingUserByUsername != null)
                throw new BadRequestException("El nombre de usuario ya está en uso.");

            var user = new IdentityUser { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Admin");

            if (!roleResult.Succeeded)
            {
                var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            return Ok("Usuario registrado correctamente con rol de administrador.");
        }

        // Endpoint para registrar un usuario cliente (POST /api/auth/registerCustomer)
        [HttpPost("registerCustomer")]
        public async Task<IActionResult> RegisterCustomer([FromBody] RegisterModel model)
        {
            // Validación básica de datos requeridos
            if (string.IsNullOrWhiteSpace(model.Username) || string.IsNullOrWhiteSpace(model.Email))
                throw new BadRequestException("El nombre de usuario y el email son obligatorios.");

            // Verifica si el email ya está registrado
            var existingUserByEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingUserByEmail != null)
                throw new BadRequestException("Ya existe un usuario registrado con ese email."); // ← corregido mensaje y tipo de excepción

            // Verifica si el nombre de usuario ya está en uso
            var existingUserByUsername = await _userManager.FindByNameAsync(model.Username);
            if (existingUserByUsername != null)
                throw new BadRequestException("El nombre de usuario ya está en uso.");

            // Crear el usuario con los datos recibidos
            var user = new IdentityUser { UserName = model.Username, Email = model.Email };
            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            // Asignar el rol "Customer" al nuevo usuario
            var roleResult = await _userManager.AddToRoleAsync(user, "Customer");
            if (!roleResult.Succeeded)
            {
                var errors = string.Join("; ", roleResult.Errors.Select(e => e.Description));
                throw new BadRequestException(errors);
            }

            return Ok("Usuario registrado correctamente con rol de cliente.");
        }
    }
}

