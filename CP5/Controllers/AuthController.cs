using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;

namespace CP5.Controllers;

public sealed class LoginDto
{
    [Required]
    public string Usuario { get; set; } = string.Empty;

    [Required]
    public string Senha { get; set; } = string.Empty;
}

/// <summary>
/// Controller de autenticação do ArenaSync.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _configuration = configuration;
        _environment = environment;
    }

    /// <summary>
    /// Realiza login e gera um token JWT.
    /// </summary>
    /// <remarks>
    /// Endpoint de demonstração disponível apenas
    /// no ambiente Development.
    /// </remarks>
    /// <param name="dto">Credenciais do usuário.</param>
    /// <returns>Token JWT válido por duas horas.</returns>
    [AllowAnonymous]
    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Realiza login",
        Description = "Valida as credenciais e retorna um JWT.",
        OperationId = "Login")]
    [SwaggerResponse(200, "Login realizado com sucesso")]
    [SwaggerResponse(400, "Dados inválidos")]
    [SwaggerResponse(401, "Usuário ou senha incorretos")]
    public IActionResult Login([FromBody] LoginDto dto)
    {
        // Autenticação de demonstração: somente Development
        if (!_environment.IsDevelopment())
        {
            return NotFound();
        }

        var usuarioConfigurado =
            _configuration["DemoLogin:Usuario"];

        var senhaConfigurada =
            _configuration["DemoLogin:Senha"];

        if (string.IsNullOrWhiteSpace(usuarioConfigurado) ||
            string.IsNullOrWhiteSpace(senhaConfigurada))
        {
            return StatusCode(
                503,
                new { message = "Login não configurado." });
        }

        var usuarioValido = string.Equals(
            dto.Usuario,
            usuarioConfigurado,
            StringComparison.Ordinal);

        var senhaValida = CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(dto.Senha ?? "")),
            SHA256.HashData(
                Encoding.UTF8.GetBytes(senhaConfigurada)));

        if (!usuarioValido || !senhaValida)
        {
            return Unauthorized(new
            {
                message = "Usuário ou senha incorretos."
            });
        }

        var jwtKey = _configuration["Jwt:Key"]!;

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, dto.Usuario),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.Name, dto.Usuario)
        };

        var expiration = DateTime.UtcNow.AddHours(2);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: expiration,
            signingCredentials: credentials);

        var tokenString =
            new JwtSecurityTokenHandler().WriteToken(token);

        return Ok(new
        {
            message = "Login realizado com sucesso.",
            token = tokenString,
            tokenType = "Bearer",
            expiresAtUtc = expiration
        });
    }
}