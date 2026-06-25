using System.Collections.Generic;
using System.Linq;

namespace P2FixAnAppDotNetCode.Models
{
    /// <summary>
    /// The Cart class
    /// </summary>
    public class Cart : ICart
    {
        /// <summary>
        /// Private backing field that persists the cart lines for the lifetime of this Cart instance
        /// </summary>
        private readonly List<CartLine> _cartLines = new List<CartLine>();

        /// <summary>
        /// Read-only property for display only
        /// </summary>
        public IEnumerable<CartLine> Lines => _cartLines;


        /// <summary>
        /// Adds a product in the cart or increments its quantity if already present
        /// </summary>
        public void AddItem(Product product, int quantity)
        {
            CartLine existingLine = _cartLines.FirstOrDefault(l => l.Product.Id == product.Id);

            if (existingLine == null)
            {
                _cartLines.Add(new CartLine
                {
                    OrderLineId = _cartLines.Count + 1,
                    Product = product,
                    Quantity = quantity
                });
            }
            else
            {
                existingLine.Quantity += quantity;
            }
        }

        /// <summary>
        /// Removes a product from the cart
        /// </summary>
        public void RemoveLine(Product product) =>
            _cartLines.RemoveAll(l => l.Product.Id == product.Id);

        /// <summary>
        /// Get total value of a cart
        /// </summary>
        public double GetTotalValue()
        {
            return _cartLines.Sum(l => l.Product.Price * l.Quantity);
        }

        /// <summary>
        /// Get average value of a cart
        /// </summary>
        public double GetAverageValue()
        {
            if (!_cartLines.Any())
                return 0.0;

            int totalQuantity = _cartLines.Sum(l => l.Quantity);
            return GetTotalValue() / totalQuantity;
        }

        /// <summary>
        /// Looks after a given product in the cart and returns if it finds it
        /// </summary>
        public Product FindProductInCartLines(int productId)
        {
            return _cartLines.FirstOrDefault(l => l.Product.Id == productId)?.Product;
        }

        /// <summary>
        /// Get a specific cartline by its index
        /// </summary>
        public CartLine GetCartLineByIndex(int index)
        {
            return Lines.ToArray()[index];
        }

        /// <summary>
        /// Clears the cart of all added products
        /// </summary>
        public void Clear() => _cartLines.Clear();
    }

    public class CartLine
    {
        public int OrderLineId { get; set; }
        public Product Product { get; set; }
        public int Quantity { get; set; }
    }
}
