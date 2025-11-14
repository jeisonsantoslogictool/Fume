using fume.api.Data;
using fume.api.Helpers;
using fume.shared.DTOs;
using fume.shared.Enttities;
using fume.shared.Enums;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace fume.api.Controllers
{
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Route("/api/categories")]
    public class CategoriesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IMemoryCache _cache;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CategoriesController(DataContext context, IMemoryCache cache, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _cache = cache;
            _httpContextAccessor = httpContextAccessor;
        }

        private string GetImageUrl(string path, int id, long? timestamp = null)
        {
            var request = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request?.Scheme}://{request?.Host}";
            var vParam = timestamp.HasValue ? $"?v={timestamp}" : "";
            return $"{baseUrl}/api/images/{path}/{id}{vParam}";
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
                        x.Icon,
                        x.ImageUrl,
                        x.ImageModifiedAt,
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
                    Icon = x.Icon,
                    ImageUrl = x.ImageUrl,
                    ImageModifiedAt = x.ImageModifiedAt,
                    Image = null,
                    HasImage = x.ImageLength > 0,
                    SubCategories = new List<SubCategory>(new SubCategory[x.SubCategoriesNumber]), // Para que el contador funcione
                    ProductCategories = new List<ProductCategory>(new ProductCategory[x.ProductCategoriesNumber]) // Para que el contador funcione
                }).ToList();

                // Generar URLs dinámicas para las imágenes
                PopulateImageUrls(result);

                return Ok(result);
            }
            catch (Exception ex )
            {

                throw;
            }
       
        }

        [HttpGet("light")]
        [AllowAnonymous]
        public async Task<ActionResult> GetLight([FromQuery] PaginationDTO pagination)
        {
            try
            {
                // Verificar cache
                var timestamp = _cache.Get<DateTime?>("categories_cache_timestamp");
                if (timestamp == null)
                {
                    _cache.Set("categories_cache_timestamp", DateTime.UtcNow);
                    timestamp = DateTime.UtcNow;
                }

                var cacheKey = $"categories_light_{pagination.Filter ?? "all"}_{pagination.Page}_{pagination.RecordsNumber}_{timestamp:yyyyMMddHHmmss}";

                if (_cache.TryGetValue(cacheKey, out List<Category>? cachedCategories))
                {
                    return Ok(cachedCategories);
                }

                var queryable = _context.categories
                    .AsNoTracking()
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(pagination.Filter))
                {
                    queryable = queryable.Where(x => x.Name.ToLower().Contains(pagination.Filter.ToLower()));
                }

                // Dos pasos: primero traer datos con conteo, luego mapear
                var tempResult = await queryable
                    .OrderBy(x => x.Name)
                    .Paginate(pagination)
                    .Select(x => new
                    {
                        x.Id,
                        x.Name,
                        x.Icon,
                        x.ImageUrl,
                        x.ImageModifiedAt,
                        ImageLength = x.Image != null ? x.Image.Length : 0,
                        SubCategoriesCount = x.SubCategories != null ? x.SubCategories.Count : 0
                    })
                    .ToListAsync();

                // Mapear a Category con SubCategories inicializada para que SubCategoriesNumber funcione
                var categories = tempResult.Select(x => new Category
                {
                    Id = x.Id,
                    Name = x.Name,
                    Icon = x.Icon,
                    ImageUrl = x.ImageUrl,
                    HasImage = x.ImageLength > 0,
                    ImageModifiedAt = x.ImageModifiedAt,
                    Image = null,
                    SubCategories = new List<SubCategory>(new SubCategory[x.SubCategoriesCount]) // Para que el contador funcione
                }).ToList();

                // Generar URLs dinámicas para las imágenes
                PopulateImageUrls(categories);

                // Guardar en cache por 10 minutos
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                };
                _cache.Set(cacheKey, categories, cacheOptions);

                return Ok(categories);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("full")]
        [AllowAnonymous]
        public async Task<ActionResult> GetFull([FromQuery] PaginationDTO pagination)
        {
            try
            {
                // Verificar si el cache sigue siendo válido (no ha sido invalidado)
                var timestamp = _cache.Get<DateTime?>("categories_cache_timestamp");
                if (timestamp == null)
                {
                    // Inicializar timestamp si no existe
                    _cache.Set("categories_cache_timestamp", DateTime.UtcNow);
                    timestamp = DateTime.UtcNow;
                }

                // Crear clave de cache basada en filtro, paginación y timestamp
                var cacheKey = $"categories_full_{pagination.Filter ?? "all"}_{pagination.Page}_{pagination.RecordsNumber}_{timestamp:yyyyMMddHHmmss}";

                // Intentar obtener del cache
                if (_cache.TryGetValue(cacheKey, out List<Category>? cachedCategories))
                {
                    // Generar URLs para los datos cacheados también
                    PopulateImageUrls(cachedCategories);
                    return Ok(cachedCategories);
                }

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

                // Limpiar las imágenes para no enviar los bytes y generar URLs
                foreach (var category in categories)
                {
                    category.HasImage = category.Image != null && category.Image.Length > 0;
                    category.Image = null; // Limpiar bytes de imagen

                    if (category.SubCategories != null)
                    {
                        foreach (var subCategory in category.SubCategories)
                        {
                            subCategory.HasImage = subCategory.Image != null && subCategory.Image.Length > 0;
                            subCategory.Image = null;
                            subCategory.ProductSubCategories = null; // No necesitamos productos en el menu
                        }
                    }
                }

                // Generar URLs
                PopulateImageUrls(categories);

                // Guardar en cache por 10 minutos
                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                };
                _cache.Set(cacheKey, categories, cacheOptions);

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
            category.Image = null; // Limpiar bytes

            if (category.SubCategories != null)
            {
                foreach (var subCategory in category.SubCategories)
                {
                    subCategory.HasImage = subCategory.Image != null && subCategory.Image.Length > 0;
                    subCategory.Image = null; // Limpiar bytes
                }
            }

            // Generar URLs dinámicas
            PopulateImageUrls(new List<Category> { category });

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

                // Invalidar cache de categorías
                InvalidateCategoriesCache();

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
                existingCategory.Icon = categoryDTO.Icon;

                // Si hay una imagen en el DTO, convertirla de Base64
                if (!string.IsNullOrEmpty(categoryDTO.ImageString))
                {
                    existingCategory.Image = Convert.FromBase64String(categoryDTO.ImageString);
                }

                _context.Update(existingCategory);
                await _context.SaveChangesAsync();

                // Invalidar cache de categorías
                InvalidateCategoriesCache();

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

            // Invalidar cache de categorías
            InvalidateCategoriesCache();

            return NoContent();
        }

        // Método para invalidar todas las entradas de cache relacionadas con categorías
        private void InvalidateCategoriesCache()
        {
            // Actualizar el timestamp invalida efectivamente todas las claves de cache anteriores
            // ya que las nuevas solicitudes usarán un timestamp diferente en la clave
            _cache.Set("categories_cache_timestamp", DateTime.UtcNow);
        }

        private void PopulateImageUrls(List<Category> categories)
        {
            foreach (var category in categories)
            {
                // Generar URL si no tiene y tiene imagen
                if (string.IsNullOrEmpty(category.ImageUrl) && category.HasImage)
                {
                    category.ImageUrl = GetImageUrl("categories", category.Id, category.ImageModifiedAt);
                }

                if (category.SubCategories != null)
                {
                    foreach (var subCategory in category.SubCategories)
                    {
                        // Saltar subcategorías nulas (placeholders para conteo)
                        if (subCategory == null) continue;

                        // Generar URL si no tiene y tiene imagen
                        if (string.IsNullOrEmpty(subCategory.ImageUrl) && subCategory.HasImage)
                        {
                            subCategory.ImageUrl = GetImageUrl("subcategories", subCategory.Id, subCategory.ImageModifiedAt);
                        }
                    }
                }
            }
        }
    }
}

