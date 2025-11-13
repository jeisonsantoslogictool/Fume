using fume.api.Data;
using fume.api.Helpers;
using fume.shared.DTOs;
using fume.shared.Enttities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace fume.api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("/api/subcategories")]
    public class SubCategoriesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMemoryCache _cache;

        public SubCategoriesController(DataContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] PaginationDTO pagination)
        {
            var queryable = _context.SubCategories
                .AsNoTracking() // No rastrear cambios = más rápido
                .Include(x => x.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.Name.ToLower().Contains(pagination.Filter.ToLower()));
            }

            var tempResult = await queryable
                .OrderBy(x => x.Category.Name)
                .ThenBy(x => x.Name)
                .Paginate(pagination)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.CategoryId,
                    CategoryId_FK = x.Category.Id,
                    CategoryName = x.Category.Name,
                    x.ImageUrl,
                    ImageLength = x.Image != null ? x.Image.Length : 0,
                    ProductSubCategoriesNumber = x.ProductSubCategories != null ? x.ProductSubCategories.Count : 0
                })
                .ToListAsync();

            var result = tempResult.Select(x => new SubCategory
            {
                Id = x.Id,
                Name = x.Name,
                CategoryId = x.CategoryId,
                Category = new Category
                {
                    Id = x.CategoryId_FK,
                    Name = x.CategoryName,
                    Image = null
                },
                Image = null,
                ImageUrl = x.ImageUrl,
                HasImage = x.ImageLength > 0,
                ProductSubCategories = new List<ProductSubCategory>(new ProductSubCategory[x.ProductSubCategoriesNumber])
            }).ToList();

            return Ok(result);
        }

        [HttpGet("totalPages")]
        public async Task<ActionResult> GetPages([FromQuery] PaginationDTO pagination)
        {
            var queryable = _context.SubCategories
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

        [HttpGet("count")]
        [AllowAnonymous]
        public async Task<ActionResult> GetCount()
        {
            var count = await _context.SubCategories.AsNoTracking().CountAsync();
            return Ok(count);
        }

        [HttpGet("{id:int}")]
        [AllowAnonymous]
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
        [AllowAnonymous]
        public async Task<ActionResult> GetByCategory(int categoryId)
        {
            // Verificar cache usando el mismo timestamp de categorías
            var timestamp = _cache.Get<DateTime?>("categories_cache_timestamp");
            if (timestamp == null)
            {
                _cache.Set("categories_cache_timestamp", DateTime.UtcNow);
                timestamp = DateTime.UtcNow;
            }

            var cacheKey = $"subcategories_bycategory_{categoryId}_{timestamp:yyyyMMddHHmmss}";

            if (_cache.TryGetValue(cacheKey, out List<SubCategory>? cachedSubCategories))
            {
                return Ok(cachedSubCategories);
            }

            // Dos pasos: primero traer solo la longitud de la imagen, luego calcular HasImage en memoria
            var tempResult = await _context.SubCategories
                .AsNoTracking()
                .Where(x => x.CategoryId == categoryId)
                .OrderBy(x => x.Name)
                .Select(x => new
                {
                    x.Id,
                    x.Name,
                    x.CategoryId,
                    x.ImageUrl,
                    ImageLength = x.Image != null ? x.Image.Length : 0,
                    ProductSubCategoriesNumber = x.ProductSubCategories != null ? x.ProductSubCategories.Count : 0
                })
                .ToListAsync();

            // Mapear a SubCategory con HasImage calculado en memoria
            var result = tempResult.Select(x => new SubCategory
            {
                Id = x.Id,
                Name = x.Name,
                CategoryId = x.CategoryId,
                ImageUrl = x.ImageUrl,
                Image = null,
                HasImage = x.ImageLength > 0,
                ProductSubCategories = new List<ProductSubCategory>(new ProductSubCategory[x.ProductSubCategoriesNumber])
            }).ToList();

            // Guardar en cache por 10 minutos
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                SlidingExpiration = TimeSpan.FromMinutes(5)
            };
            _cache.Set(cacheKey, result, cacheOptions);

            return Ok(result);
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

                subCategory.HasImage = subCategory.Image != null && subCategory.Image.Length > 0;
                subCategory.ProductSubCategories = new List<ProductSubCategory>();

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

                subCategory.HasImage = subCategory.Image != null && subCategory.Image.Length > 0;
                if (subCategory.ProductSubCategories == null)
                {
                    subCategory.ProductSubCategories = new List<ProductSubCategory>();
                }

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
