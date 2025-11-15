using fume.api.Data;
using fume.shared.DTOs;
using fume.shared.Enttities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fume.api.Controllers
{
    [ApiController]
    [Route("/api/suggestions")]
    public class SuggestionsController : ControllerBase
    {
        private readonly DataContext _context;

        public SuggestionsController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var suggestions = await _context.Suggestions
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();

            return Ok(suggestions);
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAsync(int id)
        {
            var suggestion = await _context.Suggestions.FindAsync(id);

            if (suggestion == null)
            {
                return NotFound();
            }

            return Ok(suggestion);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] SuggestionDTO suggestionDTO)
        {
            try
            {
                var suggestion = new Suggestion
                {
                    Name = suggestionDTO.Name,
                    Email = suggestionDTO.Email,
                    Message = suggestionDTO.Message,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                };

                _context.Add(suggestion);
                await _context.SaveChangesAsync();

                return Ok(suggestion);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id:int}/mark-read")]
        public async Task<IActionResult> MarkAsReadAsync(int id)
        {
            var suggestion = await _context.Suggestions.FindAsync(id);

            if (suggestion == null)
            {
                return NotFound();
            }

            suggestion.IsRead = true;
            _context.Update(suggestion);
            await _context.SaveChangesAsync();

            return Ok(suggestion);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var suggestion = await _context.Suggestions.FindAsync(id);

            if (suggestion == null)
            {
                return NotFound();
            }

            _context.Remove(suggestion);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
