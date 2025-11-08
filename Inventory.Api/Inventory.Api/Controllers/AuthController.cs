using Inventory.Api.DTOs;
using Inventory.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Inventory.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        /// <summary>
        /// Registrar un nuevo usuario
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto dto)
        {
            try
            {
                // Verificar si el email ya existe
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                {
                    return BadRequest(new { message = "El email ya está registrado" });
                }

                // Crear el usuario
                var usuario = new Usuario
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    Nombre = dto.Nombre,
                    Apellido = dto.Apellido,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _userManager.CreateAsync(usuario, dto.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return BadRequest(new { message = "Error al crear usuario", errors });
                }

                // Generar token JWT
                var token = GenerateJwtToken(usuario);

                return Ok(new AuthResponseDto
                {
                    Token = token.Token,
                    Email = usuario.Email!,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    UserId = usuario.Id,
                    Expiration = token.Expiration
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Iniciar sesión
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
        {
            try
            {
                // Buscar usuario por email
                var usuario = await _userManager.FindByEmailAsync(dto.Email);
                if (usuario == null)
                {
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                // Verificar contraseña
                var result = await _signInManager.CheckPasswordSignInAsync(usuario, dto.Password, false);
                if (!result.Succeeded)
                {
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                // Generar token JWT
                var token = GenerateJwtToken(usuario);

                return Ok(new AuthResponseDto
                {
                    Token = token.Token,
                    Email = usuario.Email!,
                    Nombre = usuario.Nombre,
                    Apellido = usuario.Apellido,
                    UserId = usuario.Id,
                    Expiration = token.Expiration
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno del servidor", error = ex.Message });
            }
        }

        /// <summary>
        /// Generar token JWT
        /// </summary>
        private (string Token, DateTime Expiration) GenerateJwtToken(Usuario usuario)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"];
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expirationMinutes = int.Parse(jwtSettings["ExpirationInMinutes"]!);
            var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("nombre", usuario.Nombre),
                new Claim("apellido", usuario.Apellido)
            };

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiration,
                signingCredentials: credentials
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return (tokenString, expiration);
        }
    }
}

