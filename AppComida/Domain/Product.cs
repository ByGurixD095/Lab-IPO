using System.Xml.Serialization;

namespace AppComida.Domain
{
    /// <summary>
    /// Representa un item del menú (plato o bebida).
    /// Incluye etiquetas XML para la correcta serialización de la base de datos de productos.
    /// </summary>
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public double Price { get; set; }

        // Información detallada para la ficha técnica
        public string Ingredients { get; set; }
        public string Allergens { get; set; }

        [XmlElement("Image")]
        public string ImagePath { get; set; }

        // Control de stock lógico
        public bool IsAvailable { get; set; }
    }
}