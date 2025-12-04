using AppComida.Persistence;
using System.Collections.Generic;

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
            if (_productsDb == null) _productsDb = new List<Product>();
        }

        public List<Product> GetAllProducts()
        {
            return _productsDb;
        }
    }
}