using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;
using OdontoSys.Api.Domain.Entities;
using OdontoSys.Api.Domain.Interfaces;
using OdontoSys.Api.Application.Services.Interfaces;

namespace OdontoSys.Api.Controllers;

[Authorize(Roles = "Admin")]
public class UsuarioController : ApiControllerBase
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly IGenericRepository<Funcionario> _funcionarioRepo;
    private readonly IAuthService _authService;

    public UsuarioController(
        UserManager<IdentityUser> userManager, 
        IGenericRepository<Funcionario> funcionarioRepo,
        IAuthService authService)
    {
        _userManager = userManager;
        _funcionarioRepo = funcionarioRepo;
        _authService = authService;
    }
    
    [HttpGet("listar")]
    [ProducesResponseType(typeof(ApplicationResult<IEnumerable<object>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarTodos()
    {
        var users = await _userManager.Users.ToListAsync();
        var listaRetorno = new List<object>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var funcionario = (await _funcionarioRepo.FindAsync(f => f.UserId == user.Id)).FirstOrDefault();

            listaRetorno.Add(new 
            {
                Id = user.Id,
                Email = user.Email,
                Roles = roles,
                VinculadoA = funcionario != null ? new { funcionario.IdFuncionario, funcionario.Nome } : null
            });
        }

        return StatusCode(200, ApplicationResult<object>.Success(listaRetorno));
    }
    
    [HttpPost("criar-admin-sistema")]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CriarUsuarioAdmin([FromBody] RegisterDTO dto)
    {
        var userExists = await _userManager.FindByEmailAsync(dto.Email);
        if (userExists != null)
            return StatusCode(400, ApplicationResult<string>.Failure("Usuário já existe!", 400));

        var user = new IdentityUser { UserName = dto.Email, Email = dto.Email, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
            return StatusCode(500, ApplicationResult<string>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)), 500));
        
        await _userManager.AddToRoleAsync(user, "Admin");
        return StatusCode(201, ApplicationResult<string>.Success(user.Id, 201, "Admin criado (sem vínculo RH)."));
    }

    [HttpPost("criar-vinculado")]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CriarUsuarioVinculado([FromBody] CriarUsuarioVinculadoDTO dto)
    {
        var funcionario = await _funcionarioRepo.GetByIdAsync(dto.IdFuncionario);
        if (funcionario == null)
            return StatusCode(404, ApplicationResult<string>.Failure("Funcionário não encontrado.", 404));

        if (!string.IsNullOrEmpty(funcionario.UserId))
            return StatusCode(409, ApplicationResult<string>.Failure("Este funcionário já possui um usuário vinculado.", 409));

        var userExists = await _userManager.FindByEmailAsync(dto.EmailLogin);
        if (userExists != null)
            return StatusCode(400, ApplicationResult<string>.Failure("Email de login já está em uso.", 400));

        var user = new IdentityUser { UserName = dto.EmailLogin, Email = dto.EmailLogin, EmailConfirmed = true };
        var result = await _userManager.CreateAsync(user, dto.Senha);

        if (!result.Succeeded)
            return StatusCode(500, ApplicationResult<string>.Failure(string.Join(", ", result.Errors.Select(e => e.Description)), 500));

        await _userManager.AddToRoleAsync(user, dto.Perfil);

        funcionario.UserId = user.Id;
        await _funcionarioRepo.UpdateAsync(funcionario);

        return StatusCode(201, ApplicationResult<string>.Success(user.Id, 201, $"Usuário criado e vinculado a {funcionario.Nome}."));
    }

    [HttpDelete("{idUsuario}")]
    public async Task<IActionResult> DeletarUsuario(string idUsuario)
    {
        var user = await _userManager.FindByIdAsync(idUsuario);
        if (user == null) return StatusCode(404, ApplicationResult<string>.Failure("Usuário não encontrado.", 404));
        
        var funcionario = (await _funcionarioRepo.FindAsync(f => f.UserId == idUsuario)).FirstOrDefault();
        if (funcionario != null)
        {
            funcionario.UserId = null;
            await _funcionarioRepo.UpdateAsync(funcionario);
        }

        await _userManager.DeleteAsync(user);
        return StatusCode(200, ApplicationResult<string>.Success("Usuário removido e desvinculado."));
    }

    [HttpPost("reset-senha-admin")]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApplicationResult<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetarSenhaAdmin([FromBody] AdminResetPasswordDTO dto)
    {
        var result = await _authService.ResetPasswordByAdmin(dto);
        return StatusCode(result.StatusCode, result);
    }
}