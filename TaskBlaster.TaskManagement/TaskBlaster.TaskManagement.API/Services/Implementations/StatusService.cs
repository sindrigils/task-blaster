using TaskBlaster.TaskManagement.API.Services.Interfaces;
using TaskBlaster.TaskManagement.DAL.Interfaces;
using TaskBlaster.TaskManagement.Models.Dtos;

namespace TaskBlaster.TaskManagement.API.Services.Implementations;

public class StatusService(IStatusRepository statusRepository) : IStatusService
{
    public async Task<IEnumerable<StatusDto>> GetAllStatusesAsync() => await statusRepository.GetAllStatusesAsync();
}