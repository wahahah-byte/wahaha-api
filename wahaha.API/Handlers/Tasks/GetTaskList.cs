using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Filters;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;
using wahaha.API.Services.Interfaces;

namespace wahaha.API.Handlers.Tasks;

public sealed record GetTaskListRequest(Guid UserId, DateTime ClientToday, TaskFilterParams Filters);

public sealed class GetTaskListHandler : IRequestHandler<GetTaskListRequest, PagedResult<TaskDto>>
{
    private readonly ITaskRepository _taskRepo;
    private readonly ITaskPenaltyService _penalty;
    private readonly IMapper _mapper;
    private readonly ILogger<GetTaskListHandler> _logger;

    public GetTaskListHandler(ITaskRepository taskRepo, ITaskPenaltyService penalty, IMapper mapper, ILogger<GetTaskListHandler> logger)
    {
        _taskRepo = taskRepo;
        _penalty = penalty;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<HandlerResult<PagedResult<TaskDto>>> HandleAsync(GetTaskListRequest request, CancellationToken ct = default)
    {
        _logger.LogDebug("Fetching tasks for user {UserId}", request.UserId);
        request.Filters.UserId = request.UserId;
        var result = await _taskRepo.GetFilteredAsync(request.Filters);
        var taskList = result.Data.ToList();
        await _penalty.ApplyAndPersistAsync(taskList, request.ClientToday);
        var dtos = _mapper.Map<List<TaskDto>>(taskList);
        for (var i = 0; i < taskList.Count; i++)
        {
            var streak = taskList[i].Streaks.FirstOrDefault();
            if (streak != null)
            {
                dtos[i].CurrentStreakCount = streak.CurrentCount;
                dtos[i].LongestStreakCount = streak.LongestCount;
            }
        }
        return HandlerResult<PagedResult<TaskDto>>.Ok(new PagedResult<TaskDto>
        {
            Data = dtos,
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalCount = result.TotalCount,
        });
    }
}
