using AutoMapper;
using wahaha.API.Models.DTO;
using wahaha.API.Models.Pagination;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Handlers.Tasks;

public sealed record GetCheckInHistoryRequest(Guid TaskId, Guid UserId, int PageNumber, int PageSize);

public sealed class GetCheckInHistoryHandler : IRequestHandler<GetCheckInHistoryRequest, PagedResult<CheckInCycleDto>>
{
    private readonly ITaskRepository _taskRepo;
    private readonly ITaskCheckInCycleRepository _cycleRepo;
    private readonly IMapper _mapper;

    public GetCheckInHistoryHandler(ITaskRepository taskRepo, ITaskCheckInCycleRepository cycleRepo, IMapper mapper)
    {
        _taskRepo = taskRepo;
        _cycleRepo = cycleRepo;
        _mapper = mapper;
    }

    public async Task<HandlerResult<PagedResult<CheckInCycleDto>>> HandleAsync(GetCheckInHistoryRequest request, CancellationToken ct = default)
    {
        var task = await _taskRepo.GetByIdAsync(request.TaskId);
        if (task == null || task.UserId != request.UserId)
            return HandlerResult<PagedResult<CheckInCycleDto>>.NotFound($"Task with ID {request.TaskId} was not found.");

        var pageNumber = request.PageNumber < 1 ? 1 : request.PageNumber;
        var pageSize = request.PageSize < 1 ? 30 : Math.Min(request.PageSize, 100);
        var (items, total) = await _cycleRepo.GetByTaskIdAsync(request.TaskId, pageNumber, pageSize);
        var dtos = _mapper.Map<List<CheckInCycleDto>>(items);
        return HandlerResult<PagedResult<CheckInCycleDto>>.Ok(new PagedResult<CheckInCycleDto>
        {
            Data = dtos,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalCount = total,
        });
    }
}
