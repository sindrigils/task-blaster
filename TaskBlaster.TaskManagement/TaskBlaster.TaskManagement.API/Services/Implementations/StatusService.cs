using TaskBlaster.TaskManagement.API.Services.Interfaces;
using TaskBlaster.TaskManagement.Models.Dtos;

namespace TaskBlaster.TaskManagement.API.Services.Implementations;

public class StatusService : IStatusService
{
    public Task<IEnumerable<StatusDto>> GetAllStatusesAsync()
    {
        throw new NotImplementedException();
    }
}