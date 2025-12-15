using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Domain.Entities;
using OdontoSys.Api.Domain.Interfaces;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Application.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IConfiguration _configuration;
    private readonly IGenericRepository<Funcionario> _funcionarioRepo;

    public AuthService(
        UserManager<IdentityUser> userManager, 
        IConfiguration configuration,
        IGenericRepository<Funcionario> funcionarioRepo)
    {
        _userManager = userManager;
        _configuration = configuration;
        _funcionarioRepo = funcionarioRepo;
    }

    public async Task<ApplicationResult<string>> Register(RegisterDTO dto)
    {
        return await CreateUserInternal(dto.Email, dto.Password, "Secretaria");
    }
    
    private async Task<ApplicationResult<string>> CreateUserInternal(string email, string password, string role)
    {
        var userExists = await _userManager.FindByEmailAsync(email);
        if (userExists != null)
            return ApplicationResult<string>.Failure("Usuário já existe!", 409);

        var user = new IdentityUser
        {
            UserName = email,
            Email = email,
            SecurityStamp = Guid.NewGuid().ToString(),
            EmailConfirmed = true
        };

        var result = await _userManager.CreateAsync(user, password);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApplicationResult<string>.Failure($"Falha ao criar usuário Identity: {errors}", 500);
        }

        await _userManager.AddToRoleAsync(user, role);

        return ApplicationResult<string>.Success(user.Id, 201, "Usuário criado.");
    }

    public async Task<ApplicationResult<string>> Login(LoginDTO dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        
        if (user != null && await _userManager.CheckPasswordAsync(user, dto.Password))
        {
            var funcionarios = await _funcionarioRepo.FindAsync(f => f.Email == dto.Email);
            var funcionario = funcionarios.FirstOrDefault();
            
            if (funcionario != null && !funcionario.Ativo)
            {
                return ApplicationResult<string>.Failure("Usuário desativado. Entre em contato com o administrador.", 403);
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("UsuarioId", user.Id),
                new Claim(ClaimTypes.NameIdentifier, user.Id)
            };

            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                expires: DateTime.Now.AddHours(8),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            return ApplicationResult<string>.Success(
                new JwtSecurityTokenHandler().WriteToken(token), 
                200, 
                "Login realizado com sucesso");
        }

        return ApplicationResult<string>.Failure("Email ou senha inválidos.", 401);
    }

    public async Task<ApplicationResult<string>> ChangePassword(string userId, ChangePasswordDTO dto)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return ApplicationResult<string>.Failure("Usuário não encontrado.", 404);
        
        var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApplicationResult<string>.Failure($"Erro ao alterar senha: {errors}", 400);
        }

        return ApplicationResult<string>.Success("Senha alterada com sucesso.", 200);
    }

    public async Task<ApplicationResult<string>> ResetPasswordByAdmin(AdminResetPasswordDTO dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return ApplicationResult<string>.Failure("Usuário Identity não encontrado para este email.", 404);

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        
        var result = await _userManager.ResetPasswordAsync(user, token, dto.NovaSenha);

        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            return ApplicationResult<string>.Failure($"Erro ao resetar: {errors}", 400);
        }

        return ApplicationResult<string>.Success("Senha redefinida com sucesso pelo administrador.", 200);
    }
}