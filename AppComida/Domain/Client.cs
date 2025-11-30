using System.Collections.Generic;
using System.Xml.Serialization;

namespace AppComida.Domain
{
    // Raíz para la lista
    [XmlRoot("Clientes")]
    public class ClientListWrapper
    {
        [XmlElement("Cliente")]
        public List<Client> Clients { get; set; } = new List<Client>();
    }

    public class Client
    {
        [XmlAttribute("Id")]
        public int Id { get; set; }

        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Foto { get; set; }
        public string Nivel { get; set; } // Oro, Plata, Bronce

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

        [XmlElement("HistorialPedidos")]
        public string HistorialPlaceholder { get; set; }

        // Helpers para UI
        [XmlIgnore]
        public string NombreCompleto => $"{Nombre} {Apellidos}";

        [XmlIgnore]
        public string DireccionPrincipal => Direcciones != null && Direcciones.Count > 0 ? Direcciones[0].Calle : "Sin dirección";
    }

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
}