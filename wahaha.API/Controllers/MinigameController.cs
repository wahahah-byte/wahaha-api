using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using wahaha.API.Models.Domain;
using wahaha.API.Models.DTOs;
using wahaha.API.Repositories.Interfaces;

namespace wahaha.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MinigamesController : ControllerBase
{
    private readonly IMinigameRepository _minigameRepository;
    private readonly IMapper _mapper;

    public MinigamesController(IMinigameRepository minigameRepository, IMapper mapper)
    {
        _minigameRepository = minigameRepository;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MinigameDto>>> GetAll()
    {
        var games = await _minigameRepository.GetAllAsync();
        return Ok(_mapper.Map<IEnumerable<MinigameDto>>(games));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<MinigameDto>> GetById(int id)
    {
        var game = await _minigameRepository.GetByIdAsync(id);

        if (game == null)
            return NotFound($"Minigame with ID {id} was not found.");

        return Ok(_mapper.Map<MinigameDto>(game));
    }

    [HttpGet("unlocked/{userLevel}")]
    public async Task<ActionResult<IEnumerable<MinigameDto>>> GetUnlocked(int userLevel)
    {
        var games = await _minigameRepository.GetUnlockedAsync(userLevel);
        return Ok(_mapper.Map<IEnumerable<MinigameDto>>(games));
    }

    [HttpGet("difficulty/{difficulty}")]
    public async Task<ActionResult<IEnumerable<MinigameDto>>> GetByDifficulty(Difficulty difficulty)
    {
        var games = await _minigameRepository.GetByDifficultyAsync(difficulty);
        return Ok(_mapper.Map<IEnumerable<MinigameDto>>(games));
    }

    [HttpPost]
    public async Task<ActionResult<MinigameDto>> Create(CreateMinigameDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var game = _mapper.Map<Minigame>(dto);
        var created = await _minigameRepository.CreateAsync(game);

        return CreatedAtAction(nameof(GetById), new { id = created.GameId }, _mapper.Map<MinigameDto>(created));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateMinigameDto dto)
    {
        if (id != dto.GameId)
            return BadRequest("Game ID in the URL does not match the request body.");

        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var game = await _minigameRepository.GetByIdAsync(id);

        if (game == null)
            return NotFound($"Minigame with ID {id} was not found.");

        _mapper.Map(dto, game);
        await _minigameRepository.UpdateAsync(game);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _minigameRepository.DeleteAsync(id);

        if (!success)
            return NotFound($"Minigame with ID {id} was not found.");

        return NoContent();
    }
}
