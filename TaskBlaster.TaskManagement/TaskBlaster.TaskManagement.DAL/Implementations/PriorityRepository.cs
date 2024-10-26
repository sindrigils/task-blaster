using Microsoft.EntityFrameworkCore;
using TaskBlaster.TaskManagement.DAL.Data;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models.Dtos;

namespace TaskBlaster.TaskManagement.DAL.Implementations;

public class PriorityRepository(TaskBlasterDbContext dbContext) : IPriorityRepository
{
    private readonly TaskBlasterDbContext _dbContext = dbContext;
    public async Task<IEnumerable<PriorityDto>> GetAllPrioritiesAsync()
    {
        return await _dbContext.Priorities.Select(p => new PriorityDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description
        }).ToListAsync();
    }

}