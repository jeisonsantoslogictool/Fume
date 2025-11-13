using fume.shared.Enttities;

namespace Fume.Web.Services
{
    public class CatalogCacheService
    {
        // Cache de categorías
        private List<Category>? _categories;

        // Cache de subcategorías por categoría
        private Dictionary<int, List<SubCategory>> _subCategoriesByCategory = new();

        // Cache de productos por subcategoría
        private Dictionary<int, List<Product>> _productsBySubCategory = new();

        // Usuario actual (para limpiar cache al cambiar de usuario)
        private string? _currentUserId;

        // Eventos para notificar cambios
        public event Action? OnCacheCleared;

        // Categorías
        public List<Category>? Categories
        {
            get => _categories;
            set => _categories = value;
        }

        // Subcategorías
        public bool TryGetSubCategories(int categoryId, out List<SubCategory>? subCategories)
        {
            return _subCategoriesByCategory.TryGetValue(categoryId, out subCategories);
        }

        public void SetSubCategories(int categoryId, List<SubCategory> subCategories)
        {
            _subCategoriesByCategory[categoryId] = subCategories;
        }

        // Productos
        public bool TryGetProducts(int subCategoryId, out List<Product>? products)
        {
            return _productsBySubCategory.TryGetValue(subCategoryId, out products);
        }

        public void SetProducts(int subCategoryId, List<Product> products)
        {
            _productsBySubCategory[subCategoryId] = products;
        }

        // Limpiar cache cuando cambia el usuario
        public void CheckAndClearIfUserChanged(string? newUserId)
        {
            if (_currentUserId != newUserId)
            {
                ClearAll();
                _currentUserId = newUserId;
            }
        }

        // Limpiar todo el cache
        public void ClearAll()
        {
            _categories = null;
            _subCategoriesByCategory.Clear();
            _productsBySubCategory.Clear();
            OnCacheCleared?.Invoke();
        }

        // Limpiar solo productos
        public void ClearProducts()
        {
            _productsBySubCategory.Clear();
        }

        // Limpiar solo subcategorías
        public void ClearSubCategories()
        {
            _subCategoriesByCategory.Clear();
        }
    }
}
