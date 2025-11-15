namespace Fume.Web.Helpers
{
    public static class ImageUrlHelper
    {
        private const string FallbackImage = "/images/no-image.png";

        public static string GetCategoryImageUrl(fume.shared.Enttities.Category category)
        {
            // Usar la URL directamente si está disponible
            if (!string.IsNullOrEmpty(category.ImageUrl))
            {
                return category.ImageUrl;
            }

            // Fallback: si tiene imagen pero no URL (datos legacy)
            if (category.HasImage)
            {
                // Esto se puede eliminar después de migrar todas las imágenes
                return $"data:image/png;base64,{category.ImageString}";
            }

            return FallbackImage;
        }

        public static string GetSubCategoryImageUrl(fume.shared.Enttities.SubCategory subCategory)
        {
            // Usar la URL directamente si está disponible
            if (!string.IsNullOrEmpty(subCategory.ImageUrl))
            {
                return subCategory.ImageUrl;
            }

            // Fallback: si tiene imagen pero no URL (datos legacy)
            if (subCategory.HasImage)
            {
                // Esto se puede eliminar después de migrar todas las imágenes
                return $"data:image/png;base64,{subCategory.ImageString}";
            }

            return FallbackImage;
        }

        public static string GetProductImageUrl(fume.shared.Enttities.ProductImage productImage)
        {
            // Usar la URL directamente si está disponible
            if (!string.IsNullOrEmpty(productImage.ImageUrl))
            {
                return productImage.ImageUrl;
            }

            // Fallback: si tiene imagen en bytes (datos legacy)
            if (productImage.Imagefile != null && productImage.Imagefile.Length > 0)
            {
                // Esto se puede eliminar después de migrar todas las imágenes
                var base64 = Convert.ToBase64String(productImage.Imagefile);
                return $"data:image/png;base64,{base64}";
            }

            return FallbackImage;
        }

        public static string GetUserPhotoUrl(string userId, bool hasPhoto)
        {
            if (hasPhoto)
            {
                // Generar URL relativa al endpoint de la API
                // El HttpClient configurado en Program.cs agregará el BaseAddress automáticamente
                return $"https://localhost:7181/api/accounts/photo/{userId}";
            }

            return string.Empty; // No devolvemos fallback, dejamos que el componente maneje las iniciales
        }
    }
}
