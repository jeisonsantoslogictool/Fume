using CurrieTechnologies.Razor.SweetAlert2;
using fume.shared.DTOs;
using fume.shared.Enttities;
using Fume.Web.Repositories;

namespace Fume.Web.Services
{
    public class CartService
    {
        private readonly IRepository _repository;
        private readonly SweetAlertService _sweetAlertService;
        private int _itemCount = 0;
        private bool _isInitialized = false;

        // Evento que notifica cuando el carrito cambia
        public event Action? OnChange;

        public int ItemCount => _itemCount;

        public CartService(IRepository repository, SweetAlertService sweetAlertService)
        {
            _repository = repository;
            _sweetAlertService = sweetAlertService;
        }

        /// <summary>
        /// Inicializa el contador del carrito para el usuario autenticado
        /// </summary>
        public async Task InitializeAsync()
        {
            if (_isInitialized)
            {
                return;
            }

            await RefreshCountAsync();
            _isInitialized = true;
        }

        /// <summary>
        /// Refresca el contador desde la API
        /// </summary>
        public async Task RefreshCountAsync()
        {
            var response = await _repository.Get<int>("/api/temporalSales/count");
            if (!response.Error)
            {
                _itemCount = response.Response;
                NotifyStateChanged();
            }
        }

        /// <summary>
        /// Agrega un producto al carrito
        /// </summary>
        public async Task<bool> AddToCartAsync(int productId, float quantity = 1, string? remarks = null)
        {
            var temporalSaleDTO = new TemporalSaleDTO
            {
                ProductId = productId,
                Quantity = quantity,
                Remarks = remarks
            };

            var response = await _repository.post("/api/temporalSales", temporalSaleDTO);

            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                await _sweetAlertService.FireAsync(new SweetAlertOptions
                {
                    Icon = SweetAlertIcon.Error,
                    Title = "Error",
                    Text = message,
                    Toast = true,
                    Position = SweetAlertPosition.BottomEnd,
                    ShowConfirmButton = false,
                    Timer = 5000,
                    TimerProgressBar = true
                });
                return false;
            }

            // Refrescar el contador desde la API
            await RefreshCountAsync();

            // Mostrar notificación de éxito
            await _sweetAlertService.FireAsync(new SweetAlertOptions
            {
                Icon = SweetAlertIcon.Success,
                Text = "Producto agregado al carrito",
                Toast = true,
                Position = SweetAlertPosition.BottomEnd,
                ShowConfirmButton = false,
                Timer = 3000,
                TimerProgressBar = true
            });

            return true;
        }

        /// <summary>
        /// Obtiene todos los items del carrito
        /// </summary>
        public async Task<List<TemporalSale>?> GetCartItemsAsync()
        {
            var response = await _repository.Get<List<TemporalSale>>("/api/temporalSales");

            if (response.Error)
            {
                return null;
            }

            return response.Response;
        }

        /// <summary>
        /// Actualiza la cantidad de un item en el carrito
        /// </summary>
        public async Task<bool> UpdateQuantityAsync(int temporalSaleId, float newQuantity)
        {
            var temporalSaleDTO = new TemporalSaleDTO
            {
                Id = temporalSaleId,
                Quantity = newQuantity
            };

            var response = await _repository.Put($"/api/temporalSales/{temporalSaleId}", temporalSaleDTO);

            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                await _sweetAlertService.FireAsync(new SweetAlertOptions
                {
                    Icon = SweetAlertIcon.Error,
                    Title = "Error",
                    Text = message
                });
                return false;
            }

            await RefreshCountAsync();
            return true;
        }

        /// <summary>
        /// Elimina un item del carrito
        /// </summary>
        public async Task<bool> RemoveFromCartAsync(int temporalSaleId)
        {
            var response = await _repository.Delete($"/api/temporalSales/{temporalSaleId}");

            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                await _sweetAlertService.FireAsync(new SweetAlertOptions
                {
                    Icon = SweetAlertIcon.Error,
                    Title = "Error",
                    Text = message
                });
                return false;
            }

            await RefreshCountAsync();

            await _sweetAlertService.FireAsync(new SweetAlertOptions
            {
                Icon = SweetAlertIcon.Success,
                Text = "Producto eliminado del carrito",
                Toast = true,
                Position = SweetAlertPosition.BottomEnd,
                ShowConfirmButton = false,
                Timer = 3000,
                TimerProgressBar = true
            });

            return true;
        }

        /// <summary>
        /// Vacía todo el carrito
        /// </summary>
        public async Task<bool> ClearCartAsync()
        {
            var response = await _repository.post("/api/temporalSales/clear", new { });

            if (response.Error)
            {
                var message = await response.GetErrorMessageAsync();
                await _sweetAlertService.FireAsync(new SweetAlertOptions
                {
                    Icon = SweetAlertIcon.Error,
                    Title = "Error",
                    Text = message
                });
                return false;
            }

            _itemCount = 0;
            NotifyStateChanged();
            return true;
        }

        /// <summary>
        /// Resetea el servicio (útil al hacer logout)
        /// </summary>
        public void Reset()
        {
            _itemCount = 0;
            _isInitialized = false;
            NotifyStateChanged();
        }

        private void NotifyStateChanged() => OnChange?.Invoke();
    }
}
