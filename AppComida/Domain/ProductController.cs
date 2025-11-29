using AppComida.Persistence;
using System.Collections.Generic;
using System.Linq;

namespace AppComida.Domain
{
    public class ProductController
    {
        private DataAgent _agent;
        private List<Product> _productsCache;

        public ProductController()
        {
            _agent = new DataAgent();
            LoadData();
        }

        private void LoadData()
        {
            _productsCache = _agent.LoadProducts();
        }

        public List<Product> GetAllProducts()
        {
            // Devolvemos todos los productos disponibles
            return _productsCache;
        }

        public List<Product> GetProductsByCategory(string category)
        {
            if (_productsCache == null) return new List<Product>();

            return _productsCache
                .Where(p => p.Category.Equals(category, System.StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}