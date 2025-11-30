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

        private void AplicarFiltros()
        {
            if (_productsDb == null) return;

            var consulta = _productsDb.AsEnumerable();

            if (!string.IsNullOrEmpty(_filtroCategoria))
                consulta = consulta.Where(p => p.Category != null && p.Category.Equals(_filtroCategoria, StringComparison.OrdinalIgnoreCase));

            if (_filtroSubCategoria != "Todo")
                consulta = consulta.Where(p => p.SubCategory != null && p.SubCategory.Equals(_filtroSubCategoria, StringComparison.OrdinalIgnoreCase));

            Productos.Clear();
            foreach (var p in consulta) Productos.Add(p);
        }

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

        private void OpVer_Click(object sender, RoutedEventArgs e) => AbrirFichaProducto(sender);
        private void OpEditar_Click(object sender, RoutedEventArgs e) => AbrirFichaProducto(sender);

        private void AbrirFichaProducto(object sender)
        {
            int id = ObtenerId(sender);
            var prod = _productsDb.FirstOrDefault(p => p.Id == id);

            if (prod != null)
            {
                // Constructor para VER/EDITAR
                ProductDetailWindow detalle = new ProductDetailWindow(prod);
                detalle.Owner = Window.GetWindow(this);
                detalle.ShowDialog();

                if (detalle.ActionDelete)
                {
                    EliminarProducto(prod);
                }
                else if (detalle.ActionEdit)
                {
                    // Si se editó, refrescamos la lista
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

        // --- BOTÓN AÑADIR (MODIFICADO) ---
        private void BtnAnadir_Click(object sender, RoutedEventArgs e)
        {
            // Constructor para CREAR (pasamos true)
            ProductDetailWindow detalle = new ProductDetailWindow(true);
            detalle.Owner = Window.GetWindow(this);
            detalle.ShowDialog();

            // Verificamos si se guardó correctamente
            if (detalle.ActionEdit && detalle.ProductResult != null)
            {
                // Asignamos un ID temporal si es necesario
                var nuevoProd = detalle.ProductResult;
                nuevoProd.Id = _productsDb.Any() ? _productsDb.Max(p => p.Id) + 1 : 1;

                _productsDb.Add(nuevoProd);
                AplicarFiltros();
            }
        }

        private void EliminarProducto(Product prod)
        {
            _productsDb.Remove(prod);
            Productos.Remove(prod);
            AplicarFiltros();
        }

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