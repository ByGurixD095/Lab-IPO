using System.Collections.Generic;
using System.Xml.Serialization;

namespace AppComida.Domain
{
    [XmlRoot("Pedidos")]
    public class PedidoListWrapper
    {
        [XmlElement("Pedido")]
        public List<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }

    public class Pedido
    {
        // ==========================================
        // PROPIEDADES DE DATOS (Mapeo XML)
        // ==========================================
        // Estas propiedades coinciden EXACTAMENTE con tu archivo pedidos.xml

        [XmlAttribute("Id")]
        public string Id { get; set; }

        public int ClienteId { get; set; }

        // El XML tiene <NombreCliente>, así que usamos ese nombre aquí
        public string NombreCliente { get; set; }

        public string TipoEntrega { get; set; } // "Mesa 5", "Domicilio", etc.
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public string Estado { get; set; } // "Pendiente", "En Cocina", etc.
        public decimal Total { get; set; }

        [XmlArray("Detalle")]
        [XmlArrayItem("Item")]
        public List<string> Items { get; set; } = new List<string>();

        [XmlIgnore]
        public Client ClienteVinculado { get; set; }


        [XmlIgnore]
        public string Status => Estado;

        [XmlIgnore]
        public string CustomerName => NombreCliente;

        [XmlIgnore]
        public string DeliveryType => TipoEntrega; 

        [XmlIgnore]
        public string Time => Hora;

        [XmlIgnore]
        public string IdDisplay => $"#{Id}";

        [XmlIgnore]
        public string CustomerIdDisplay
        {
            get
            {
                if (ClienteVinculado != null) return $"ID: C-{ClienteVinculado.Id:D4}";
                if (ClienteId > 0) return $"ID: C-{ClienteId:D4}";
                return "Invitado";
            }
        }

        [XmlIgnore]
        public string TotalDisplay => $"{Total:C}";

        [XmlIgnore]
        public string DeliveryIcon
        {
            get
            {
                if (string.IsNullOrEmpty(TipoEntrega)) return "❓";
                if (TipoEntrega.Contains("Domicilio")) return "🛵"; // Moto
                if (TipoEntrega.Contains("Mesa")) return "🍽️";      // Plato
                if (TipoEntrega.Contains("Recoger")) return "🥡";   // Take away
                return "📦";
            }
        }
    }
}