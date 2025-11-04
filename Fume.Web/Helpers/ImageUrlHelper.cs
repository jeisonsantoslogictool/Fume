namespace Fume.Web.Helpers
{
    public static class ImageUrlHelper
    {
        private const string BaseUrl = "https://localhost:7181";

        public static string GetCategoryImageUrl(fume.shared.Enttities.Category category)
        {
           
            if (category.Id > 0)
            {
                return $"{BaseUrl}/api/images/categories/{category.Id}";
            }

         
            return "/images/no-image.png";
        }

        public static string GetSubCategoryImageUrl(fume.shared.Enttities.SubCategory subCategory)
        {
            
            if (subCategory.Id > 0)
            {
                return $"{BaseUrl}/api/images/subcategories/{subCategory.Id}";
            }

         
            return "/images/no-image.png";
        }

        public static string GetProductImageUrl(fume.shared.Enttities.ProductImage productImage)
        {
           
            if (productImage.Id > 0)
            {
                return $"{BaseUrl}/api/images/products/{productImage.Id}";
            }

            
            return "/images/no-image.png";
        }
    }
}
