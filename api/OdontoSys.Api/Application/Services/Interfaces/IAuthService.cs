using OdontoSys.Api.Application;
using OdontoSys.Api.Application.DTOs;

namespace OdontoSys.Api.Application.Services.Interfaces;

public interface IAuthService
{
    Task<ApplicationResult<string>> Register(RegisterDTO dto);
    Task<ApplicationResult<string>> Login(LoginDTO dto);
    Task<ApplicationResult<string>> ChangePassword(string userId, ChangePasswordDTO dto);
    Task<ApplicationResult<string>> ResetPasswordByAdmin(AdminResetPasswordDTO dto);
}