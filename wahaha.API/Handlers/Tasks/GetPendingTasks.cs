using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Tasks;

public sealed record GetPendingTasksRequest(Guid UserId);

public sealed class GetPendingTasksHandler : IRequestHandler<GetPendingTasksRequest, IEnumerable<TaskDto>>
{
    private readonly ITaskRepository _taskRepo;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPendingTasksHandler> _logger;

    public GetPendingTasksHandler(ITaskRepository taskRepo, IMapper mapper, ILogger<GetPendingTasksHandler> logger)
    {
        _taskRepo = taskRepo;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<IEnumerable<TaskDto>>> HandleAsync(GetPendingTasksRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching pending tasks for user {UserId}", request.UserId);
        var tasks = await _taskRepo.GetPendingByUserAsync(request.UserId);
        return HandlerResult<IEnumerable<TaskDto>>.Ok(_mapper.Map<IEnumerable<TaskDto>>(tasks));
    }
}
