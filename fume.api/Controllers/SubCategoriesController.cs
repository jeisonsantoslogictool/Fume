using fume.api.Data;
using fume.api.Helpers;
using fume.shared.DTOs;
using fume.shared.Enttities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fume.api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("/api/subcategories")]
    public class SubCategoriesController : ControllerBase
    {
        private readonly DataContext _context;

        public SubCategoriesController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] PaginationDTO pagination)
        {
            var queryable = _context.SubCategories
                .Include(x => x.Category)
                .Include(x => x.ProductSubCategories)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.Name.ToLower().Contains(pagination.Filter.ToLower()));
            }

            return Ok(await queryable
                .OrderBy(x => x.Category.Name)
                .ThenBy(x => x.Name)
                .Paginate(pagination)
                .ToListAsync());
        }

        [HttpGet("totalPages")]
        public async Task<ActionResult> GetPages([FromQuery] PaginationDTO pagination)
        {
            var queryable = _context.SubCategories
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.Name.ToLower().Contains(pagination.Filter.ToLower()));
            }

            double count = await queryable.CountAsync();
            double totalPages = Math.Ceiling(count / pagination.RecordsNumber);
            return Ok(totalPages);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult> Get(int id)
        {
            var subCategory = await _context.SubCategories
                .Include(x => x.Category)
                .Include(x => x.ProductSubCategories)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (subCategory is null)
            {
                return NotFound();
            }

            return Ok(subCategory);
        }

        [HttpGet("bycategory/{categoryId:int}")]
        public async Task<ActionResult> GetByCategory(int categoryId)
        {
            var subCategories = await _context.SubCategories
                .Include(x => x.ProductSubCategories)
                .Where(x => x.CategoryId == categoryId)
                .OrderBy(x => x.Name)
                .ToListAsync();

            return Ok(subCategories);
        }

        [HttpPost]
        public async Task<ActionResult> Post(SubCategoryDTO subCategoryDTO)
        {
            // Validar que el nombre no sea nulo o vacío
            if (string.IsNullOrWhiteSpace(subCategoryDTO.Name))
            {
                return BadRequest("El nombre de la subcategoría es obligatorio.");
            }

            // Validar longitud máxima
            if (subCategoryDTO.Name.Length > 100)
            {
                return BadRequest("El nombre de la subcategoría no puede exceder 100 caracteres.");
            }

            // Validar que se haya seleccionado una categoría
            if (subCategoryDTO.CategoryId <= 0)
            {
                return BadRequest("Debe seleccionar una categoría válida.");
            }

            // Validar que la categoría exista
            var categoryExists = await _context.categories.AnyAsync(c => c.Id == subCategoryDTO.CategoryId);
            if (!categoryExists)
            {
                return BadRequest("La categoría seleccionada no existe.");
            }

            // Validar que haya una imagen
            if (string.IsNullOrEmpty(subCategoryDTO.ImageString))
            {
                return BadRequest("La imagen de la subcategoría es obligatoria.");
            }

            var subCategory = new SubCategory
            {
                Name = subCategoryDTO.Name,
                CategoryId = subCategoryDTO.CategoryId
            };

            if (!string.IsNullOrEmpty(subCategoryDTO.ImageString))
            {
                subCategory.Image = Convert.FromBase64String(subCategoryDTO.ImageString);
            }

            _context.Add(subCategory);
            try
            {
                await _context.SaveChangesAsync();
                return Ok(subCategory);
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException!.Message.Contains("duplicate"))
                {
                    return BadRequest("Ya existe una subcategoria con el mismo nombre en esta categoria.");
                }
                else
                {
                    return BadRequest(dbUpdateException.InnerException.Message);
                }
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpPut]
        public async Task<ActionResult> Put(SubCategoryDTO subCategoryDTO)
        {
            // Validar que el nombre no sea nulo o vacío
            if (string.IsNullOrWhiteSpace(subCategoryDTO.Name))
            {
                return BadRequest("El nombre de la subcategoría es obligatorio.");
            }

            // Validar longitud máxima
            if (subCategoryDTO.Name.Length > 100)
            {
                return BadRequest("El nombre de la subcategoría no puede exceder 100 caracteres.");
            }

            // Validar que se haya seleccionado una categoría
            if (subCategoryDTO.CategoryId <= 0)
            {
                return BadRequest("Debe seleccionar una categoría válida.");
            }

            // Validar que la categoría exista
            var categoryExists = await _context.categories.AnyAsync(c => c.Id == subCategoryDTO.CategoryId);
            if (!categoryExists)
            {
                return BadRequest("La categoría seleccionada no existe.");
            }

            var subCategory = await _context.SubCategories.FindAsync(subCategoryDTO.Id);
            if (subCategory == null)
            {
                return NotFound();
            }

            subCategory.Name = subCategoryDTO.Name;
            subCategory.CategoryId = subCategoryDTO.CategoryId;

            if (!string.IsNullOrEmpty(subCategoryDTO.ImageString))
            {
                subCategory.Image = Convert.FromBase64String(subCategoryDTO.ImageString);
            }

            _context.Update(subCategory);
            try
            {
                await _context.SaveChangesAsync();
                return Ok(subCategory);
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException!.Message.Contains("duplicate"))
                {
                    return BadRequest("Ya existe una subcategoria con el mismo nombre en esta categoria.");
                }
                else
                {
                    return BadRequest(dbUpdateException.InnerException.Message);
                }
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var subCategory = await _context.SubCategories.FirstOrDefaultAsync(x => x.Id == id);
            if (subCategory == null)
            {
                return NotFound();
            }

            _context.Remove(subCategory);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
