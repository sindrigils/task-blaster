using TaskBlaster.TaskManagement.API.Services.Interfaces;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models.Dtos;
using TaskBlaster.TaskManagement.Models.InputModels;

namespace TaskBlaster.TaskManagement.API.Services.Implementations;

public class TagService(ITagRepository tagRepository) : ITagService
{
    public async Task<IEnumerable<TagDto>> GetAllTagsAsync() => await tagRepository.GetAllTagsAsync();

    public async Task<int> CreateNewTagAsync(TagInputModel inputModel) => await tagRepository.CreateNewTagAsync(inputModel);
}