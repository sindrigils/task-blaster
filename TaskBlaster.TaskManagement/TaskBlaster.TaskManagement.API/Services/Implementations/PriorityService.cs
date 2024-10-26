using TaskBlaster.TaskManagement.API.Services.Interfaces;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models.Dtos;

namespace TaskBlaster.TaskManagement.API.Services.Implementations;

public class PriorityService(IPriorityRepository priorityRepository) : IPriorityService
{
    public async Task<IEnumerable<PriorityDto>> GetAllPrioritiesAsync() => await priorityRepository.GetAllPrioritiesAsync();
}