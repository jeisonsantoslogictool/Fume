using fume.api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace fume.api.Controllers
{
    [ApiController]
    [Route("/api/images")]
    public class ImagesController : ControllerBase
    {
        private readonly DataContext _context;

        public ImagesController(DataContext context)
        {
            _context = context;
        }

        [HttpGet("categories/{id:int}")]
        [ResponseCache(Duration = 3600)] // Cache por 1 hora
        public async Task<IActionResult> GetCategoryImage(int id)
        {
            var category = await _context.categories
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (category?.Image == null || category.Image.Length == 0)
            {
                return NotFound();
            }

            return File(category.Image, "image/png");
        }

        [HttpGet("subcategories/{id:int}")]
        [ResponseCache(Duration = 3600)] // Cache por 1 hora
        public async Task<IActionResult> GetSubCategoryImage(int id)
        {
            var subCategory = await _context.SubCategories
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subCategory?.Image == null || subCategory.Image.Length == 0)
            {
                return NotFound();
            }

            return File(subCategory.Image, "image/png");
        }

        [HttpGet("products/{id:int}")]
        [ResponseCache(Duration = 3600)] // Cache por 1 hora
        public async Task<IActionResult> GetProductImage(int id)
        {
            var productImage = await _context.ProductImages
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (productImage?.Imagefile == null || productImage.Imagefile.Length == 0)
            {
                return NotFound();
            }

            return File(productImage.Imagefile, "image/png");
        }
    }
}
