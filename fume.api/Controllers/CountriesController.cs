using fume.api.Data;
using fume.shared.Enttities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fume.api.Controllers
{
    [ApiController]
    [Route("/api/countries")]
    public class CountriesController : ControllerBase
    {
        private readonly DataContext _Context;
        public CountriesController(DataContext context)
        {
            _Context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            return Ok(await  _Context.Countries.ToListAsync());
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetAsync( int id )
        {
            var country = await _Context.Countries.FirstOrDefaultAsync(x => x.Id == id);
            if(country == null )
            {
                return NotFound();
            }

            return Ok();
        }
        [HttpPost]
        public async Task<ActionResult> PostAsync(Country country)
        {
            _Context.Add(country);
           await _Context.SaveChangesAsync();
            return Ok(country);
        }

        [HttpPut]
        public async Task<ActionResult> PutAsync(Country country)
        {
            _Context.Update(country);
            await _Context.SaveChangesAsync();
            return Ok(country);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var country = await _Context.Countries.FirstOrDefaultAsync(x => x.Id == id);
            if (country == null)
            {
                return NotFound();

            }

            _Context.Remove(country);
            await _Context.SaveChangesAsync();
            return NoContent();
        }
    }
}
