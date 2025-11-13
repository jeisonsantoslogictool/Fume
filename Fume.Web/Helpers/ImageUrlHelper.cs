namespace Fume.Web.Helpers
{
    public static class ImageUrlHelper
    {
        private const string BaseUrl = "https://localhost:7181";

        public static string GetCategoryImageUrl(fume.shared.Enttities.Category category)
        {
            // Solo generar URL si tiene imagen
            if (category.HasImage && category.Id > 0)
            {
                return $"{BaseUrl}/api/images/categories/{category.Id}?v={category.ImageModifiedAt}";
            }

            return "/images/no-image.png";
        }

        public static string GetSubCategoryImageUrl(fume.shared.Enttities.SubCategory subCategory)
        {
            // Solo generar URL si tiene imagen
            if (subCategory.HasImage && subCategory.Id > 0)
            {
                return $"{BaseUrl}/api/images/subcategories/{subCategory.Id}?v={subCategory.ImageModifiedAt}";
            }

            return "/images/no-image.png";
        }

        public static string GetProductImageUrl(fume.shared.Enttities.ProductImage productImage)
        {
            // Para productos, siempre intentamos cargar la URL si tiene ID
            if (productImage.Id > 0)
            {
                return $"{BaseUrl}/api/images/products/{productImage.Id}";
            }

            return "/images/no-image.png";
        }
    }
}
