using Microsoft.EntityFrameworkCore;
using TaskBlaster.TaskManagement.DAL.Data;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models.Dtos;

namespace TaskBlaster.TaskManagement.DAL.Implementations;

public class StatusRepository(TaskBlasterDbContext dbContext) : IStatusRepository
{
    private readonly TaskBlasterDbContext _dbContext = dbContext;
    public async Task<IEnumerable<StatusDto>> GetAllStatusesAsync()
    {
        return await _dbContext.Statuses.Select(s => new StatusDto
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description
        }).ToListAsync();
    }
    public async Task<bool> DoesStatusExistAsync(int? id)
    {
        return await _dbContext.Statuses.AnyAsync(s => s.Id == id);
    }

}