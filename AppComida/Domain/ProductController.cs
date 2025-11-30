using AppComida.Persistence;
using System.Collections.Generic;
using System.Linq;

namespace AppComida.Domain
{
    public class ProductController
    {
        private DataAgent _agent;
        private List<Product> _productsDb;

        public ProductController()
        {
            _agent = new DataAgent();
            LoadData();
        }

        private void LoadData()
        {
            _productsDb = _agent.LoadProducts();
        }

        public List<Product> GetAllProducts()
        {
            return _productsDb;
        }

        public List<Product> GetProductsByCategory(string category)
        {
            if (_productsDb == null) return new List<Product>();

            return _productsDb
                .Where(p => p.Category.Equals(category, System.StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }
}