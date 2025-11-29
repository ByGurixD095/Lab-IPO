using AppComida.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq; // Necesario para leer XML directamente

namespace AppComida.Presentation
{
    public partial class ProductosPage : Page
    {
        // Colección observable para la interfaz
        public ObservableCollection<Product> Productos { get; set; }

        public ProductosPage()
        {
            InitializeComponent();

            // Inicializamos la colección vacía
            Productos = new ObservableCollection<Product>();

            // Cargamos los productos directamente desde el XML
            CargarProductosDesdeXml();

            // Vinculamos la lista a la interfaz
            ProductList.ItemsSource = Productos;
        }

        private void CargarProductosDesdeXml()
        {
            try
            {
                // 1. Buscamos el archivo products.xml subiendo niveles (igual que hace el DataAgent)
                string rutaXml = BuscarArchivo("products.xml");

                if (string.IsNullOrEmpty(rutaXml))
                {
                    MessageBox.Show("No se encontró el archivo 'products.xml' en la carpeta Data.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 2. Leemos el XML usando LINQ to XML (XDocument)
                XDocument doc = XDocument.Load(rutaXml);

                // 3. Convertimos cada nodo <Product> en un objeto Product
                var lista = doc.Descendants("Product").Select(p => new Product
                {
                    Id = int.Parse(p.Element("Id")?.Value ?? "0"),
                    Name = p.Element("Name")?.Value ?? "Sin nombre",
                    Category = p.Element("Category")?.Value ?? "",
                    SubCategory = p.Element("SubCategory")?.Value ?? "",
                    Price = double.Parse(p.Element("Price")?.Value ?? "0", System.Globalization.CultureInfo.InvariantCulture),
                    Ingredients = p.Element("Ingredients")?.Value ?? "",
                    Allergens = p.Element("Allergens")?.Value ?? "",
                    ImagePath = p.Element("Image")?.Value ?? "", // Mapeamos <Image> del XML a ImagePath
                    IsAvailable = bool.Parse(p.Element("IsAvailable")?.Value ?? "true")
                }).ToList();

                // 4. Añadimos a la colección visual
                foreach (var prod in lista)
                {
                    Productos.Add(prod);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error al leer productos: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Método auxiliar para encontrar el archivo (copiado de la lógica del DataAgent para que funcione aquí)
        private string BuscarArchivo(string nombreArchivo)
        {
            string directorioActual = AppDomain.CurrentDomain.BaseDirectory;
            for (int i = 0; i < 6; i++)
            {
                string rutaPotencial = System.IO.Path.Combine(directorioActual, "Data", nombreArchivo); // Carpeta 'Data'
                if (File.Exists(rutaPotencial)) return rutaPotencial;

                // Intento alternativo minúsculas por si acaso
                string rutaPotencialMin = System.IO.Path.Combine(directorioActual, "data", nombreArchivo);
                if (File.Exists(rutaPotencialMin)) return rutaPotencialMin;

                var padre = Directory.GetParent(directorioActual);
                if (padre == null) break;
                directorioActual = padre.FullName;
            }
            return null;
        }

        private void BtnGestionar_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag != null)
            {
                string productId = btn.Tag.ToString();

                MessageBox.Show(
                    $"Gestión del producto ID: {productId}\n\nAquí iría la lógica para Editar o Borrar.",
                    "Administración",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
        }
    }
}