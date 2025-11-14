using fume.shared.Enttities;
using Microsoft.JSInterop;
using System.Text.Json;

namespace Fume.Web.Services
{
    public class CatalogCacheService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string CATEGORIES_KEY = "fume_categories";
        private const string SUBCATEGORIES_PREFIX = "fume_subcat_";
        private const string PRODUCTS_PREFIX = "fume_prod_";
        private const string USER_ID_KEY = "fume_user_id";
        private const string CACHE_VERSION_KEY = "fume_cache_version";
        private const string CACHE_VERSION = "2.0"; // v2.0: URLs dinámicas incluidas en responses

        // Eventos para notificar cambios
        public event Action? OnCacheCleared;

        public CatalogCacheService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        // Inicializar y verificar versión del cache
        public async Task InitializeAsync()
        {
            try
            {
                var storedVersion = await GetFromLocalStorage<string>(CACHE_VERSION_KEY);
                if (storedVersion != CACHE_VERSION)
                {
                    // Cache antiguo, limpiar todo
                    await ClearAll();
                    await SetToLocalStorage(CACHE_VERSION_KEY, CACHE_VERSION);
                }
            }
            catch
            {
                // Si hay error, limpiar cache
                await ClearAll();
            }
        }

        // Categorías
        public async Task<List<Category>?> GetCategoriesAsync()
        {
            return await GetFromLocalStorage<List<Category>>(CATEGORIES_KEY);
        }

        public async Task SetCategoriesAsync(List<Category> categories)
        {
            await SetToLocalStorage(CATEGORIES_KEY, categories);
        }

        // Subcategorías
        public async Task<List<SubCategory>?> GetSubCategoriesAsync(int categoryId)
        {
            return await GetFromLocalStorage<List<SubCategory>>($"{SUBCATEGORIES_PREFIX}{categoryId}");
        }

        public async Task SetSubCategoriesAsync(int categoryId, List<SubCategory> subCategories)
        {
            await SetToLocalStorage($"{SUBCATEGORIES_PREFIX}{categoryId}", subCategories);
        }

        // Productos
        public async Task<List<Product>?> GetProductsAsync(int subCategoryId)
        {
            return await GetFromLocalStorage<List<Product>>($"{PRODUCTS_PREFIX}{subCategoryId}");
        }

        public async Task SetProductsAsync(int subCategoryId, List<Product> products)
        {
            await SetToLocalStorage($"{PRODUCTS_PREFIX}{subCategoryId}", products);
        }

        // Limpiar cache cuando cambia el usuario
        public async Task CheckAndClearIfUserChangedAsync(string? newUserId)
        {
            var currentUserId = await GetFromLocalStorage<string>(USER_ID_KEY);
            if (currentUserId != newUserId)
            {
                await ClearAll();
                await SetToLocalStorage(USER_ID_KEY, newUserId ?? "");
            }
        }

        // Limpiar todo el cache
        public async Task ClearAll()
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.clear");
                OnCacheCleared?.Invoke();
            }
            catch
            {
                // Silenciar errores si localStorage no está disponible
            }
        }

        // Limpiar solo productos
        public async Task ClearProducts()
        {
            try
            {
                // Obtener todas las keys de localStorage
                var keys = await GetAllLocalStorageKeys();
                foreach (var key in keys)
                {
                    if (key.StartsWith(PRODUCTS_PREFIX))
                    {
                        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
                    }
                }
            }
            catch
            {
                // Silenciar errores
            }
        }

        // Métodos auxiliares privados
        private async Task<T?> GetFromLocalStorage<T>(string key)
        {
            try
            {
                var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
                return string.IsNullOrEmpty(json) ? default : JsonSerializer.Deserialize<T>(json);
            }
            catch
            {
                return default;
            }
        }

        private async Task SetToLocalStorage<T>(string key, T value)
        {
            try
            {
                var json = JsonSerializer.Serialize(value);
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
            }
            catch
            {
                // Silenciar errores si localStorage está lleno o no disponible
            }
        }

        private async Task<List<string>> GetAllLocalStorageKeys()
        {
            try
            {
                var length = await _jsRuntime.InvokeAsync<int>("eval", "localStorage.length");
                var keys = new List<string>();
                for (int i = 0; i < length; i++)
                {
                    var key = await _jsRuntime.InvokeAsync<string>("localStorage.key", i);
                    if (!string.IsNullOrEmpty(key))
                    {
                        keys.Add(key);
                    }
                }
                return keys;
            }
            catch
            {
                return new List<string>();
            }
        }
    }
}
