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
    [Route("/api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IFileStorage _fileStorage;

        public ProductsController(DataContext context, IFileStorage fileStorage)
        {
            _context = context;
            _fileStorage = fileStorage;
        }

        [HttpGet]
        public async Task<ActionResult> Get([FromQuery] PaginationDTO pagination)
        {
            var queryable = _context.Products
                .Include(x => x.ProductImages)
                .Include(x => x.ProductCategories)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(pagination.Filter))
            {
                queryable = queryable.Where(x => x.Name.ToLower().Contains(pagination.Filter.ToLower()));
            }

            return Ok(await queryable
                .OrderBy(x => x.Name)
                .Paginate(pagination)
                .ToListAsync());
        }


        [HttpGet("totalPages")]
        public async Task<ActionResult> GetPages([FromQuery] PaginationDTO pagination)
        {
            var queryable = _context.Products
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
        public async Task<IActionResult> GetAsync(int id)
        {
            var product = await _context.Products
                .Include(x => x.ProductImages)
                .Include(x => x.ProductCategories!)
                .ThenInclude(x => x.Category)
                .FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult> PostAsync(ProductDTO productDTO)
        {
            try
            {
                // Crea un nuevo producto
                Product newProduct = new()
                {
                    Name = productDTO.Name,
                    Description = productDTO.Description,
                    Price = productDTO.Price,
                    Stock = productDTO.Stock,
                    ProductCategories = new List<ProductCategory>(),
                    ProductImages = new List<ProductImage>()
                };

                // Guarda el nuevo producto en la base de datos
                _context.Add(newProduct);
                await _context.SaveChangesAsync();

                // Asocia las imágenes al producto
                foreach (var productImage in productDTO.ProductImages!)
                {
                    var photoProduct = Convert.FromBase64String(productImage);
                    var savedImage = await _fileStorage.SaveFileAsync(photoProduct, ".jpg", "products");

                    // Asocia la imagen al producto
                    newProduct.ProductImages.Add(new ProductImage { Imagefile = Convert.FromBase64String(productImage), ProductId = newProduct.Id });
                    await _context.SaveChangesAsync();
                }

                // Asocia las categorías al producto
                foreach (var productCategoryId in productDTO.ProductCategoryIds!)
                {
                    var category = await _context.categories.FirstOrDefaultAsync(x => x.Id == productCategoryId);
                    newProduct.ProductCategories.Add(new ProductCategory { Category = category! });
                }

                // Guarda los cambios finales en el producto (con categorías e imágenes asociadas)
                await _context.SaveChangesAsync();

                return Ok(productDTO);
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException!.Message.Contains("duplicate"))
                {
                    return BadRequest("Ya existe un producto con el mismo nombre.");
                }

                return BadRequest(dbUpdateException.Message);
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
          
        }

        [HttpPut]
        public async Task<ActionResult> PutAsync(Product product)
        {
            try
            {
                _context.Update(product);
                await _context.SaveChangesAsync();
                return Ok(product);
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException!.Message.Contains("duplicate"))
                {
                    return BadRequest("Ya existe un producto con el mismo nombre.");
                }

                return BadRequest(dbUpdateException.Message);
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}
