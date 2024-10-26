using TaskBlaster.TaskManagement.API.Services.Interfaces;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models.Dtos;
using TaskBlaster.TaskManagement.Models.InputModels;

namespace TaskBlaster.TaskManagement.API.Services.Implementations;

public class UserService(IUserRepository userRepository) : IUserService
{
    public async Task CreateUserIfNotExistsAsync(UserInputModel inputModel) => await userRepository.CreateUserIfNotExists(inputModel);

    public async Task<IEnumerable<UserDto>> GetAllUsersAsync() => await userRepository.GetAllUsers();

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(userId, 1);
        return await userRepository.GetUserByIdAsync(userId);
    }
}