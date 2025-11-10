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
    [Route("/api/temporalSales")]
    public class TemporalSalesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public TemporalSalesController(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var email = User.Identity!.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var user = await _userHelper.GetUserAsync(email);
            if (user == null)
            {
                return Unauthorized();
            }

            var temporalSales = await _context.TemporalSales
                .Include(ts => ts.Product)
                .ThenInclude(p => p!.ProductImages)
                .Where(ts => ts.UserId == user.Id)
                .OrderByDescending(ts => ts.Id)
                .ToListAsync();

            return Ok(temporalSales);
        }

        [HttpGet("count")]
        public async Task<IActionResult> GetCountAsync()
        {
            var email = User.Identity!.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Ok(0);
            }

            var user = await _userHelper.GetUserAsync(email);
            if (user == null)
            {
                return Ok(0);
            }

            var count = await _context.TemporalSales
                .Where(ts => ts.UserId == user.Id)
                .SumAsync(ts => ts.Quantity);

            return Ok(count);
        }

        [HttpPost]
        public async Task<IActionResult> PostAsync([FromBody] TemporalSaleDTO temporalSaleDTO)
        {
            try
            {
                var email = User.Identity!.Name;
                if (string.IsNullOrEmpty(email))
                {
                    return Unauthorized();
                }

                var user = await _userHelper.GetUserAsync(email);
                if (user == null)
                {
                    return Unauthorized();
                }

                // Verificar que el producto existe y está disponible
                var product = await _context.Products.FindAsync(temporalSaleDTO.ProductId);
                if (product == null)
                {
                    return NotFound("Producto no encontrado");
                }

                // Verificar stock disponible
                var currentQuantityInCart = await _context.TemporalSales
                    .Where(ts => ts.UserId == user.Id && ts.ProductId == temporalSaleDTO.ProductId)
                    .SumAsync(ts => ts.Quantity);

                var requestedQuantity = temporalSaleDTO.Quantity;
                var totalQuantity = currentQuantityInCart + requestedQuantity;

                if (totalQuantity > product.Stock)
                {
                    return BadRequest($"No hay suficiente stock. Disponible: {product.Stock}, En carrito: {currentQuantityInCart}");
                }

                // Verificar si ya existe el producto en el carrito
                var existingTemporalSale = await _context.TemporalSales
                    .FirstOrDefaultAsync(ts => ts.UserId == user.Id && ts.ProductId == temporalSaleDTO.ProductId);

                if (existingTemporalSale != null)
                {
                    // Si ya existe, incrementar la cantidad
                    existingTemporalSale.Quantity += requestedQuantity;
                    existingTemporalSale.Remarks = temporalSaleDTO.Remarks ?? existingTemporalSale.Remarks;
                    _context.Update(existingTemporalSale);
                }
                else
                {
                    // Si no existe, crear nuevo
                    var temporalSale = new TemporalSale
                    {
                        UserId = user.Id,
                        ProductId = temporalSaleDTO.ProductId,
                        Quantity = requestedQuantity,
                        Remarks = temporalSaleDTO.Remarks
                    };

                    _context.Add(temporalSale);
                }

                await _context.SaveChangesAsync();

                // Retornar el conteo actualizado
                var newCount = await _context.TemporalSales
                    .Where(ts => ts.UserId == user.Id)
                    .SumAsync(ts => ts.Quantity);

                return Ok(new { count = newCount });
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutAsync(int id, [FromBody] TemporalSaleDTO temporalSaleDTO)
        {
            var email = User.Identity!.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var user = await _userHelper.GetUserAsync(email);
            if (user == null)
            {
                return Unauthorized();
            }

            var temporalSale = await _context.TemporalSales
                .Include(ts => ts.Product)
                .FirstOrDefaultAsync(ts => ts.Id == id && ts.UserId == user.Id);

            if (temporalSale == null)
            {
                return NotFound();
            }

            // Verificar stock disponible
            if (temporalSaleDTO.Quantity > temporalSale.Product!.Stock)
            {
                return BadRequest($"No hay suficiente stock. Disponible: {temporalSale.Product.Stock}");
            }

            temporalSale.Quantity = temporalSaleDTO.Quantity;
            temporalSale.Remarks = temporalSaleDTO.Remarks;

            _context.Update(temporalSale);
            await _context.SaveChangesAsync();

            return Ok(temporalSale);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var email = User.Identity!.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var user = await _userHelper.GetUserAsync(email);
            if (user == null)
            {
                return Unauthorized();
            }

            var temporalSale = await _context.TemporalSales
                .FirstOrDefaultAsync(ts => ts.Id == id && ts.UserId == user.Id);

            if (temporalSale == null)
            {
                return NotFound();
            }

            _context.Remove(temporalSale);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("clear")]
        public async Task<IActionResult> ClearAsync()
        {
            var email = User.Identity!.Name;
            if (string.IsNullOrEmpty(email))
            {
                return Unauthorized();
            }

            var user = await _userHelper.GetUserAsync(email);
            if (user == null)
            {
                return Unauthorized();
            }

            var temporalSales = await _context.TemporalSales
                .Where(ts => ts.UserId == user.Id)
                .ToListAsync();

            _context.RemoveRange(temporalSales);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
