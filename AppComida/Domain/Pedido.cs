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
        // Propiedades mapeadas al XML de Pedidos
        [XmlAttribute("Id")]
        public string Id { get; set; }

        public int ClienteId { get; set; } // FK lógica hacia Client

        public string NombreCliente { get; set; } // Redundancia para optimizar lectura
        public string TipoEntrega { get; set; } 
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public string Estado { get; set; } 
        public decimal Total { get; set; }

        [XmlArray("Detalle")]
        [XmlArrayItem("Item")]
        public List<string> Items { get; set; } = new List<string>();

        // Referencia al objeto Cliente real (no se guarda en XML, se rellena en tiempo de ejecución)
        [XmlIgnore]
        public Client ClienteVinculado { get; set; }

        #region Helpers para la Vista (MVVM)
        
        // Exponemos propiedades de solo lectura para facilitar el Binding en la interfaz
        
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

        // Lógica visual para mostrar iconos según el tipo de entrega
        [XmlIgnore]
        public string DeliveryIcon
        {
            get
            {
                if (string.IsNullOrEmpty(TipoEntrega)) return "❓";
                if (TipoEntrega.Contains("Domicilio")) return "🛵"; 
                if (TipoEntrega.Contains("Mesa")) return "🍽️";      
                if (TipoEntrega.Contains("Recoger")) return "🥡";   
                return "📦";
            }
        }
        #endregion
    }
}