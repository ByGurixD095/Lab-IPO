using System;
using System.Xml.Serialization;

namespace AppComida.Domain
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public double Price { get; set; }
        public string Ingredients { get; set; }
        public string Allergens { get; set; }

        [XmlElement("Image")]
        public string ImagePath { get; set; }

        public bool IsAvailable { get; set; }
    }
}