using System.Collections.Generic;
using System.Xml.Serialization;

namespace AppComida.Domain
{
    // Wrapper necesario para deserializar la lista raíz del XML
    [XmlRoot("Clientes")]
    public class ClientListWrapper
    {
        [XmlElement("Cliente")]
        public List<Client> Clients { get; set; } = new List<Client>();
    }

    /// <summary>
    /// Entidad principal del cliente. Contiene sub-clases para organizar la información compleja.
    /// </summary>
    public class Client
    {
        [XmlAttribute("Id")]
        public int Id { get; set; }

        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Foto { get; set; }
        public string Nivel { get; set; }

        // Objetos de valor (Value Objects) para agrupar datos relacionados
        [XmlElement("Contactos")]
        public ContactInfo Contacto { get; set; } = new ContactInfo();

        [XmlArray("Direcciones")]
        [XmlArrayItem("Direccion")]
        public List<Address> Direcciones { get; set; } = new List<Address>();

        [XmlArray("Salud")]
        [XmlArrayItem("Alergia")]
        public List<string> Alergias { get; set; } = new List<string>();

        [XmlElement("Preferencias")]
        public Preferences Preferencias { get; set; } = new Preferences();

        [XmlElement("Fidelizacion")]
        public Loyalty Fidelizacion { get; set; } = new Loyalty();

        [XmlArray("HistorialPedidos")]
        [XmlArrayItem("RefPedido")]
        public List<OrderRef> Historial { get; set; } = new List<OrderRef>();

        #region Propiedades Auxiliares para UI (Binding)

        [XmlIgnore]
        public string NombreCompleto => $"{Nombre} {Apellidos}";

        [XmlIgnore]
        public string DireccionPrincipal => (Direcciones != null && Direcciones.Count > 0)
            ? Direcciones[0].Calle
            : "Sin dirección registrada";

        #endregion
    }

    // --- Clases auxiliares para estructura del XML ---

    public class ContactInfo
    {
        public string Telefono { get; set; }
        public string Email { get; set; }
    }

    public class Address
    {
        [XmlAttribute("Principal")]
        public bool EsPrincipal { get; set; }

        [XmlText]
        public string Calle { get; set; }
    }

    public class Preferences
    {
        public string FormaPago { get; set; }
    }

    public class Loyalty
    {
        public int PuntosAcumulados { get; set; }
        public int PuntosCanjeados { get; set; }
    }

    public class OrderRef
    {
        [XmlAttribute("Id")]
        public string Id { get; set; }

        [XmlAttribute("Fecha")]
        public string Fecha { get; set; }

        [XmlAttribute("Total")]
        public decimal Total { get; set; }

        [XmlAttribute("Estado")]
        public string Estado { get; set; }
    }
}