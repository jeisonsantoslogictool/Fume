using fume.api.Data;
using fume.api.Helpers;
using fume.shared.DTOs;
using fume.shared.Enttities;
using fume.shared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fume.api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("/api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly DataContext _context;

        public CategoriesController(DataContext context)
        {
            _context = context;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> Get([FromQuery] PaginationDTO pagination)
        {
            try
            {
                var queryable = _context.categories
               .AsNoTracking() // No rastrear cambios = más rápido
               .AsQueryable();

                if (!string.IsNullOrWhiteSpace(pagination.Filter))
                {
                    queryable = queryable.Where(x => x.Name.ToLower().Contains(pagination.Filter.ToLower()));
                }

                var tempResult = await queryable
                    .OrderBy(x => x.Name)
                    .Paginate(pagination)
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.ImageUrl,
                        ImageLength = x.Image != null ? x.Image.Length : 0, // Solo traer la longitud
                        SubCategoriesNumber = x.SubCategories != null ? x.SubCategories.Count : 0,
                        ProductCategoriesNumber = x.ProductCategories != null ? x.ProductCategories.Count : 0
                    })
                    .ToListAsync();

                // Mapear a Category con HasImage calculado en memoria
                var result = tempResult.Select(x => new Category
                {
                    Id = x.Id,
                    Name = x.Name,
                    ImageUrl = x.ImageUrl,
                    Image = null,
                    HasImage = x.ImageLength > 0,
                    SubCategories = new List<SubCategory>(new SubCategory[x.SubCategoriesNumber]), // Para que el contador funcione
                    ProductCategories = new List<ProductCategory>(new ProductCategory[x.ProductCategoriesNumber]) // Para que el contador funcione
                }).ToList();

                return Ok(result);
            }
            catch (Exception ex )
            {

                throw;
            }
       
        }

        [HttpGet("full")]
        [AllowAnonymous]
        public async Task<ActionResult> GetFull([FromQuery] PaginationDTO pagination)
        {
            try
            {
                var queryable = _context.categories
                    .AsNoTracking()
                    .Include(x => x.SubCategories)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(pagination.Filter))
                {
                    queryable = queryable.Where(x => x.Name.ToLower().Contains(pagination.Filter.ToLower()));
                }

                var categories = await queryable
                    .OrderBy(x => x.Name)
                    .Paginate(pagination)
                    .ToListAsync();

                // Limpiar las imágenes para no enviar los bytes
                foreach (var category in categories)
                {
                    category.Image = null;
                    category.HasImage = false; // No necesitamos las imágenes de categorías en el menu

                    if (category.SubCategories != null)
                    {
                        foreach (var subCategory in category.SubCategories)
                        {
                            subCategory.Image = null;
                            subCategory.HasImage = false;
                            subCategory.ProductSubCategories = null; // No necesitamos productos en el menu
                        }
                    }
                }

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("totalPages")]
        public async Task<ActionResult> GetPages([FromQuery] PaginationDTO pagination)
        {
            var queryable = _context.categories
                .AsNoTracking() // No rastrear cambios = más rápido
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
            var category = await _context.categories
                .Include(x => x.SubCategories)
                .ThenInclude(x => x.ProductSubCategories)
                .Include(x => x.ProductCategories)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (category is null)
            {
                return NotFound();
            }

            category.HasImage = category.Image != null && category.Image.Length > 0;

            if (category.SubCategories != null)
            {
                foreach (var subCategory in category.SubCategories)
                {
                    subCategory.HasImage = subCategory.Image != null && subCategory.Image.Length > 0;
                }
            }

            return Ok(category);
        }


        [HttpPost]
        public async Task<ActionResult> Post(Category category)
        {
            // Validar que el nombre no sea nulo o vacío
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                return BadRequest("El nombre de la categoría es obligatorio.");
            }

            // Validar longitud máxima
            if (category.Name.Length > 100)
            {
                return BadRequest("El nombre de la categoría no puede exceder 100 caracteres.");
            }

            // Validar que haya una imagen
            if (category.Image == null || category.Image.Length == 0)
            {
                return BadRequest("La imagen de la categoría es obligatoria.");
            }

            _context.Add(category);
            try
            {
                await _context.SaveChangesAsync();

                category.HasImage = category.Image != null && category.Image.Length > 0;
                category.SubCategories = new List<SubCategory>();
                category.ProductCategories = new List<ProductCategory>();

                return Ok(category);
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException!.Message.Contains("duplicate"))
                {
                    return BadRequest("Ya existe un registro con el mismo nombre.");
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
        public async Task<ActionResult> Put(CategoryDTO categoryDTO)
        {
            // Validar que el nombre no sea nulo o vacío
            if (string.IsNullOrWhiteSpace(categoryDTO.Name))
            {
                return BadRequest("El nombre de la categoría es obligatorio.");
            }

            // Validar longitud máxima
            if (categoryDTO.Name.Length > 100)
            {
                return BadRequest("El nombre de la categoría no puede exceder 100 caracteres.");
            }

            try
            {
                var existingCategory = await _context.categories.FindAsync(categoryDTO.Id);
                if (existingCategory == null)
                {
                    return NotFound();
                }

                // Solo actualizar los campos permitidos
                existingCategory.Name = categoryDTO.Name;

                // Si hay una imagen en el DTO, convertirla de Base64
                if (!string.IsNullOrEmpty(categoryDTO.ImageString))
                {
                    existingCategory.Image = Convert.FromBase64String(categoryDTO.ImageString);
                }

                _context.Update(existingCategory);
                await _context.SaveChangesAsync();

                existingCategory.HasImage = existingCategory.Image != null && existingCategory.Image.Length > 0;
                if (existingCategory.SubCategories == null)
                {
                    existingCategory.SubCategories = new List<SubCategory>();
                }
                if (existingCategory.ProductCategories == null)
                {
                    existingCategory.ProductCategories = new List<ProductCategory>();
                }

                return Ok(existingCategory);
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException!.Message.Contains("duplicate"))
                {
                    return BadRequest("Ya existe un registro con el mismo nombre.");
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
            var category = await _context.categories.FirstOrDefaultAsync(x => x.Id == id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Remove(category);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}

