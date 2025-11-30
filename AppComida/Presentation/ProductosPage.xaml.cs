using AppComida.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace AppComida.Presentation
{
    public partial class ProductosPage : Page
    {
        private ProductController _controller;
        private List<Product> _productsDb;
        public ObservableCollection<Product> Productos { get; set; }

        // Filtros por defecto
        private string _filtroCategoria = "Platos";
        private string _filtroSubCategoria = "Todo";

        public ProductosPage()
        {
            InitializeComponent();
            _controller = new ProductController();
            Productos = new ObservableCollection<Product>();
            ProductList.ItemsSource = Productos;
            CargarDatos();
        }

        private void CargarDatos()
        {
            _productsDb = _controller.GetAllProducts();
            if (_productsDb == null) _productsDb = new List<Product>();
            AplicarFiltros();
        }

        // Detecta cuando cambia un radio button de los filtros
        private void Filtro_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag != null)
            {
                string seleccion = rb.Tag.ToString();
                if (rb.GroupName == "MainCat") _filtroCategoria = seleccion;
                else if (rb.GroupName == "SubCat") _filtroSubCategoria = seleccion;
                AplicarFiltros();
            }
        }

        // Lógica de filtrado con LINQ
        private void AplicarFiltros()
        {
            if (_productsDb == null) return;

            var consulta = _productsDb.AsEnumerable();

            if (!string.IsNullOrEmpty(_filtroCategoria))
                consulta = consulta.Where(p => p.Category != null && p.Category.Equals(_filtroCategoria, StringComparison.OrdinalIgnoreCase));

            if (_filtroSubCategoria != "Todo")
                consulta = consulta.Where(p => p.SubCategory != null && p.SubCategory.Equals(_filtroSubCategoria, StringComparison.OrdinalIgnoreCase));

            // Limpio la lista visible y añado los filtrados
            Productos.Clear();
            foreach (var p in consulta) Productos.Add(p);
        }

        // Truco para abrir el menú contextual con click izquierdo en el botón
        private void BtnOpciones_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;
                btn.ContextMenu.Closed += (s, args) => btn.IsChecked = false;
            }
        }

        private void OpVer_Click(object sender, RoutedEventArgs e)
        {
            AbrirFichaProducto(sender);
        }

        private void OpEditar_Click(object sender, RoutedEventArgs e)
        {
            AbrirFichaProducto(sender);
        }

        // Abro la ventana de detalle, busco el ID por el tag
        private void AbrirFichaProducto(object sender)
        {
            int id = ObtenerId(sender);
            var prod = _productsDb.FirstOrDefault(p => p.Id == id);

            if (prod != null)
            {
                ProductDetailWindow detalle = new ProductDetailWindow(prod);
                detalle.Owner = Window.GetWindow(this);

                detalle.ShowDialog();

                // Compruebo qué ha pasado al cerrar la ventana
                if (detalle.ActionDelete)
                {
                    EliminarProducto(prod);
                }
                else if (detalle.ActionEdit)
                {
                    AplicarFiltros();
                }
            }
        }

        private void OpEliminar_Click(object sender, RoutedEventArgs e)
        {
            int id = ObtenerId(sender);
            var prod = _productsDb.FirstOrDefault(p => p.Id == id);

            if (prod != null)
            {
                ConfirmWindow confirm = new ConfirmWindow(
                   $"¿Seguro que quieres eliminar '{prod.Name}' del catálogo?\nEsta acción no se puede deshacer.",
                   "Eliminar Producto",
                   ConfirmType.Danger);

                confirm.Owner = Window.GetWindow(this);

                if (confirm.ShowDialog() == true)
                {
                    EliminarProducto(prod);
                }
            }
        }

        // Botón grande de añadir
        private void BtnAnadir_Click(object sender, RoutedEventArgs e)
        {
            // Creo un producto dummy para poder abrir la ventana
            var nuevoProd = new Product
            {
                // Calculo el ID sumando 1 al máximo (un poco chapuza pero funciona)
                Id = _productsDb.Any() ? _productsDb.Max(p => p.Id) + 1 : 1,
                Name = "NUEVO (Click Editar)",
                Category = "Platos",
                SubCategory = "Todo",
                Price = 0,
                IsAvailable = true,
                Ingredients = "",
                Allergens = "",
                ImagePath = ""
            };

            // Reutilizo la ventana de detalle
            ProductDetailWindow detalle = new ProductDetailWindow(nuevoProd);
            detalle.Owner = Window.GetWindow(this);

            detalle.ShowDialog();

            // Si le dio a guardar, lo añado a la lista de verdad
            if (detalle.ActionEdit)
            {
                _productsDb.Add(nuevoProd);
                AplicarFiltros();
            }
        }

        private void EliminarProducto(Product prod)
        {
            // Borro de la lista total y de la observable
            _productsDb.Remove(prod);
            Productos.Remove(prod);
            AplicarFiltros();
        }

        // Helper para sacar el ID del Tag del elemento que lanza el evento
        private int ObtenerId(object sender)
        {
            if (sender is MenuItem item && item.Tag != null && int.TryParse(item.Tag.ToString(), out int id))
                return id;
            if (sender is FrameworkElement fe && fe.Tag != null && int.TryParse(fe.Tag.ToString(), out int id2))
                return id2;
            return 0;
        }
    }
} 