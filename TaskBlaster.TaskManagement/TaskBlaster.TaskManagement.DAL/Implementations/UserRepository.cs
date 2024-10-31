using Microsoft.EntityFrameworkCore;
using TaskBlaster.TaskManagement.DAL.Data;
using TaskBlaster.TaskManagement.DAL.Entities;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models.Dtos;
using TaskBlaster.TaskManagement.Models.InputModels;
using Task = System.Threading.Tasks.Task;

namespace TaskBlaster.TaskManagement.DAL.Implementations;

public class UserRepository(TaskBlasterDbContext dbContext) : IUserRepository
{
    private readonly TaskBlasterDbContext _dbContext = dbContext;
    public async Task CreateUserIfNotExists(UserInputModel inputModel)
    {
        if (await _dbContext.Users.AnyAsync(u => u.FullName == inputModel.FullName && u.EmailAddress == inputModel.EmailAddress))
        {
            return;
        }

        var newUser = new User
        {
            FullName = inputModel.FullName,
            EmailAddress = inputModel.EmailAddress,
            ProfileImageUrl = inputModel.ProfileImageUrl,
            CreatedAt = DateTime.UtcNow
        };
        await _dbContext.AddAsync(newUser);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<UserDto>> GetAllUsers()
    {
        return await _dbContext.Users.Select(u => new UserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            EmailAddress = u.EmailAddress,
            ProfileImageUrl = u.ProfileImageUrl
        }).ToListAsync();
    }

    public async Task<UserDto?> GetUserByIdAsync(int userId)
    {
        return await _dbContext.Users
            .Select(u => new UserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                EmailAddress = u.EmailAddress,
                ProfileImageUrl = u.ProfileImageUrl
            })
            .FirstOrDefaultAsync(u => u.Id == userId);
    }
    public async Task<UserDto?> GetUserByNameAsync(string userName)
    {
        return await _dbContext.Users.Where(u => u.FullName == userName).Select(u => new UserDto
        {
            Id = u.Id,
            FullName = u.FullName,
            EmailAddress = u.EmailAddress,
        }).FirstOrDefaultAsync();
    }

    public async Task<bool> DoesUserExistAsync(string user)
    {
        return await _dbContext.Users.AnyAsync(u => u.FullName == user);
    }

    public async Task<bool> DoesUserExistAsync(int userId)
    {
        return await _dbContext.Users.AnyAsync(u => u.Id == userId);
    }
}