
using fume.shared.Enttities;

namespace Fume.Web.Services
{
    public class CatalogNavigationService
    {
        public event Action? OnCategoriesRequested;
        public event Action<Category>? OnCategorySelected;
        public event Action<SubCategory>? OnSubCategorySelected;

        public void NavigateToCategories()
        {
            OnCategoriesRequested?.Invoke();
        }

        public void NavigateToCategory(Category category)
        {
            OnCategorySelected?.Invoke(category);
        }

        public void NavigateToSubCategory(SubCategory subCategory)
        {
            OnSubCategorySelected?.Invoke(subCategory);
        }
    }
}
