using System.ComponentModel.DataAnnotations;

namespace fume.shared.DTOs
{
    public class TemporalSaleDTO
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar un producto")]
        public int ProductId { get; set; }

        [Range(0.01, float.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero")]
        public float Quantity { get; set; } = 1;

        public string? Remarks { get; set; }
    }
}
