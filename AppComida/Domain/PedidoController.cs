using AppComida.Persistence;
using System.Collections.Generic;
using System.Linq;

namespace AppComida.Domain
{
    public class PedidoController
    {
        private DataAgent _agent;
        private List<Pedido> _pedidosDb;
        private List<Client> _clientesDb;

        public PedidoController()
        {
            _agent = new DataAgent();
            LoadData();
        }

        private void LoadData()
        {
            _pedidosDb = _agent.LoadPedidos();
            if (_pedidosDb == null) _pedidosDb = new List<Pedido>();

            _clientesDb = _agent.LoadClients();
            if (_clientesDb == null) _clientesDb = new List<Client>();

            foreach (var pedido in _pedidosDb)
            {
                if (pedido.ClienteId > 0)
                {
                    var clienteReal = _clientesDb.FirstOrDefault(c => c.Id == pedido.ClienteId);
                    if (clienteReal != null)
                    {
                        pedido.ClienteVinculado = clienteReal;
                    }
                }
            }
        }

        public List<Pedido> GetAllPedidos()
        {
            return _pedidosDb;
        }

        public List<Pedido> GetPedidosByStatus(string status)
        {
            return _pedidosDb.Where(p => p.Estado == status).ToList();
        }
    }
}