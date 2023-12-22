using fume.api.Data;
using fume.shared.Enttities;
using Microsoft.EntityFrameworkCore;

namespace fume.api.Helpers
{
    public class FIleStorage : IFileStorage
    {
        private readonly string connectionString;
        private readonly DataContext _context;

        public FIleStorage(IConfiguration configuration, DataContext context)
        {
            connectionString = "DockerConnection";
            _context = context;
        }
        public Task RemoveFileAsync(string path, string nombreContenedor)
        {
            throw new NotImplementedException();
        }

        public async Task<string> SaveFileAsync(byte[] content, string extention, string containerName)
        {
            try
            {
                if (content == null || string.IsNullOrEmpty(extention))
                {
                    throw new ArgumentNullException(nameof(content), "El contenido o la extensión no pueden ser nulos o vacíos.");
                }

                // Crea una entidad ProductImage para representar la imagen
                var productImage = new ProductImage
                {
                    Imagefile = content
                };

                // Asocia el ProductImage a un Producto existente (podrías necesitar una lógica más específica aquí)
                var firstProduct = await _context.Products.FirstOrDefaultAsync();

                if (firstProduct != null)
                {
                    // Asigna el Id del primer producto a ProductId en ProductImage
                    productImage.ProductId = firstProduct.Id;
                }

                // Asocia la entidad ProductImage con la entidad Product
                productImage.Product = firstProduct;

                // Agrega la entidad a la base de datos
                _context.ProductImages.Add(productImage);

                // Guarda los cambios en la base de datos
                await _context.SaveChangesAsync();

                // Obtén el Id del producto asociado a la imagen después de la inserción
                int productId = productImage.ProductId;

                // Devuelve el Id del producto (o cualquier identificador único)
                return productId.ToString();
            }
            catch (Exception ex)
            {
                // Maneja la excepción (puedes registrarla, lanzarla nuevamente, etc.)
                Console.WriteLine($"Error al guardar la imagen en la base de datos: {ex.Message}");
                throw;
            }
        }    }
}
