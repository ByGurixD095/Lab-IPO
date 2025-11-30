using AppComida.Persistence;
using System.Collections.Generic;
using System.Linq;

namespace AppComida.Domain
{
    public class ClientController
    {
        private DataAgent _agent;
        private List<Client> _clientsDb; 

        public ClientController()
        {
            _agent = new DataAgent();
            LoadData();
        }

        private void LoadData()
        {
            _clientsDb = _agent.LoadClients();

            if (_clientsDb == null) _clientsDb = new List<Client>();
        }

        public List<Client> GetAllClients()
        {
            return _clientsDb;
        }
    }
}