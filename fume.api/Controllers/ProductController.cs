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
    public class ProductController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IFileStorage _fileStorage;

        public ProductController(DataContext context, IFileStorage fileStorage)
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
                .Include(x => x.ProductSubCategories)
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
                .Include(x => x.ProductSubCategories!)
                .ThenInclude(x => x.SubCategory)
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
                    ProductSubCategories = new List<ProductSubCategory>(),
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
                    newProduct.ProductCategories.Add(new ProductCategory { ProductId = newProduct.Id, CategoryId = productCategoryId, Category = category! });
                }

                // Asocia las subcategorías al producto
                if (productDTO.ProductSubCategoryIds != null && productDTO.ProductSubCategoryIds.Any())
                {
                    foreach (var productSubCategoryId in productDTO.ProductSubCategoryIds)
                    {
                        var subCategory = await _context.SubCategories.FirstOrDefaultAsync(x => x.Id == productSubCategoryId);
                        if (subCategory != null)
                        {
                            newProduct.ProductSubCategories!.Add(new ProductSubCategory { ProductId = newProduct.Id, SubCategoryId = productSubCategoryId, SubCategory = subCategory });
                        }
                    }
                }

                // Guarda los cambios finales en el producto (con categorías, subcategorías e imágenes asociadas)
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
        public async Task<ActionResult> PutAsync(ProductDTO productDTO)
        {
            try
            {
                var product = await _context.Products
                    .Include(x => x.ProductCategories)
                    .Include(x => x.ProductSubCategories)
                    .FirstOrDefaultAsync(x => x.Id == productDTO.Id);
                if (product == null)
                {
                    return NotFound();
                }

                product.Name = productDTO.Name;
                product.Description = productDTO.Description;
                product.Price = productDTO.Price;
                product.Stock = productDTO.Stock;
                product.ProductCategories = productDTO.ProductCategoryIds!.Select(x => new ProductCategory { ProductId = product.Id, CategoryId = x }).ToList();

                // Actualiza las subcategorías
                if (productDTO.ProductSubCategoryIds != null)
                {
                    product.ProductSubCategories = productDTO.ProductSubCategoryIds.Select(x => new ProductSubCategory { ProductId = product.Id, SubCategoryId = x }).ToList();
                }
                else
                {
                    product.ProductSubCategories = new List<ProductSubCategory>();
                }

                _context.Update(product);
                await _context.SaveChangesAsync();
                return Ok(productDTO);
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException!.Message.Contains("duplicate"))
                {
                    return BadRequest("Ya existe una ciudad con el mismo nombre.");
                }

                return BadRequest(dbUpdateException.Message);
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }


        [HttpGet("subcategory/{subcategoryId:int}")]
        public async Task<ActionResult> GetBySubcategoryAsync(int subcategoryId)
        {
            var products = await _context.Products
                .Include(x => x.ProductImages)
                .Include(x => x.ProductCategories)
                .Include(x => x.ProductSubCategories)
                .Where(x => x.ProductSubCategories!.Any(ps => ps.SubCategoryId == subcategoryId))
                .OrderBy(x => x.Name)
                .ToListAsync();

            return Ok(products);
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
