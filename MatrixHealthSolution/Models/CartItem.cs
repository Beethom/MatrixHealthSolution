namespace MatrixHealthSolution.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string? ImagePath { get; set; }

        // ✅ add this
        public int Quantity { get; set; } = 1;

        public decimal LineTotal => Price * Quantity;
    }
}
