using Microsoft.EntityFrameworkCore;
using TaskBlaster.TaskManagement.DAL.Data;
using TaskBlaster.TaskManagement.DAL.Entities;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models.Dtos;
using TaskBlaster.TaskManagement.Models.InputModels;
using Task = System.Threading.Tasks.Task;

namespace TaskBlaster.TaskManagement.DAL.Implementations;

public class TagRepository(TaskBlasterDbContext dbContext) : ITagRepository
{
    private readonly TaskBlasterDbContext _dbContext = dbContext;
    public async Task<int> CreateNewTagAsync(TagInputModel inputModel)
    {
        var tag = new Tag
        {
            Name = inputModel.Name,
            Description = inputModel.Description
        };

        await _dbContext.AddAsync(tag);
        await _dbContext.SaveChangesAsync();
        return tag.Id;
    }

    public async Task<IEnumerable<TagDto>> GetAllTagsAsync()
    {
        return await _dbContext.Tags.Select(t => new TagDto
        {
            Id = t.Id,
            Name = t.Name,
            Description = t.Description
        }).ToListAsync();
    }
}