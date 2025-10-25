namespace fume.shared.Enttities
{
    public class ProductSubCategory
    {
        public int Id { get; set; }

        public Product Products { get; set; } = null!;

        public int ProductId { get; set; }

        public SubCategory SubCategory { get; set; } = null!;

        public int SubCategoryId { get; set; }
    }
}
