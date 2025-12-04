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
    /// <summary>
    /// Controlador de la vista de catálogo.
    /// Gestiona el filtrado dinámico (Categoría/Subcategoría) y la navegación hacia el detalle del producto.
    /// </summary>
    public partial class ProductosPage : Page
    {
        #region Campos y Propiedades

        private List<Product> _productsDb;

        public ObservableCollection<Product> Productos { get; set; }

        // Estado de los filtros activos
        private string _filtroCategoria = "Platos";
        private string _filtroSubCategoria = "Todo";

        #endregion

        #region Inicialización

        public ProductosPage()
        {
            InitializeComponent();

            Productos = new ObservableCollection<Product>();
            ProductList.ItemsSource = Productos;

            _productsDb = new List<Product>();
        }

        /// <summary>
        /// Recibe la lista maestra de productos desde la ventana principal.
        /// Se llama al inicio y cada vez que se navega a esta vista.
        /// </summary>
        public void SetProductos(List<Product> productosExternos)
        {
            _productsDb = productosExternos;

            AplicarFiltros();
        }

        // Permite extraer los cambios realizados para guardarlos en MainWindow
        public List<Product> GetProductos()
        {
            return _productsDb;
        }

        #endregion

        #region Lógica de Filtrado

        private void Filtro_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag != null)
            {
                string seleccion = rb.Tag.ToString();

                // Identificamos si es filtro principal o secundario por el grupo
                if (rb.GroupName == "MainCat")
                    _filtroCategoria = seleccion;
                else if (rb.GroupName == "SubCat")
                    _filtroSubCategoria = seleccion;

                AplicarFiltros();
            }
        }

        private void AplicarFiltros()
        {
            if (_productsDb == null) return;

            // Construcción de la consulta
            var consulta = _productsDb.AsEnumerable();

            // 1. Filtro por Categoría Principal
            if (!string.IsNullOrEmpty(_filtroCategoria))
                consulta = consulta.Where(p => p.Category != null && p.Category.Equals(_filtroCategoria, StringComparison.OrdinalIgnoreCase));

            // 2. Filtro por Subcategoría
            if (_filtroSubCategoria != "Todo")
                consulta = consulta.Where(p => p.SubCategory != null && p.SubCategory.Equals(_filtroSubCategoria, StringComparison.OrdinalIgnoreCase));

            // Actualización de la UI
            Productos.Clear();
            foreach (var p in consulta)
            {
                Productos.Add(p);
            }
        }

        #endregion

        #region Gestión de Acciones (CRUD)

        private void BtnOpciones_Click(object sender, RoutedEventArgs e)
        {
            if (sender is ToggleButton btn && btn.ContextMenu != null)
            {
                btn.ContextMenu.PlacementTarget = btn;
                btn.ContextMenu.Placement = PlacementMode.Bottom;
                btn.ContextMenu.IsOpen = true;

                // Reset del toggle al cerrar menú
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
                // Abrimos la ventana modal en modo edición
                ProductDetailWindow detalle = new ProductDetailWindow(prod);
                detalle.Owner = Window.GetWindow(this);
                detalle.ShowDialog();

                // Procesamos el resultado
                if (detalle.ActionDelete)
                {
                    EliminarProducto(prod);
                }
                else if (detalle.ActionEdit)
                {
                    // Si se editó, simplemente refrescamos los filtros para actualizar la vista
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
                ConfirmWindow confirm = new ConfirmWindow($"¿Seguro que deseas eliminar '{prod.Name}' del catálogo?", "Eliminar Producto", ConfirmType.Danger);
                confirm.Owner = Window.GetWindow(this);

                if (confirm.ShowDialog() == true)
                {
                    EliminarProducto(prod);
                }
            }
        }

        private void BtnAnadir_Click(object sender, RoutedEventArgs e)
        {
            // Modo creación
            ProductDetailWindow detalle = new ProductDetailWindow(true);
            detalle.Owner = Window.GetWindow(this);
            detalle.ShowDialog();

            if (detalle.ActionEdit && detalle.ProductResult != null)
            {
                var nuevoProd = detalle.ProductResult;

                // Generación ID
                nuevoProd.Id = _productsDb.Any() ? _productsDb.Max(p => p.Id) + 1 : 1;

                _productsDb.Add(nuevoProd);
                AplicarFiltros();
            }
        }

        private void EliminarProducto(Product prod)
        {
            _productsDb.Remove(prod);
            Productos.Remove(prod);
        }

        private int ObtenerId(object sender)
        {
            // Extrae el ID guardado en el Tag del control que lanzó el evento
            if (sender is MenuItem item && item.Tag != null && int.TryParse(item.Tag.ToString(), out int id)) return id;
            if (sender is FrameworkElement fe && fe.Tag != null && int.TryParse(fe.Tag.ToString(), out int id2)) return id2;
            return 0;
        }

        #endregion
    }
}