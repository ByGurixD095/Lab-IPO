using AppComida.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives; // Necesario para ToggleButton y ContextMenu

namespace AppComida.Presentation
{
    public partial class ProductosPage : Page
    {
        private ProductController _controller;
        private List<Product> _todosLosProductos;
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
            _todosLosProductos = _controller.GetAllProducts();
            if (_todosLosProductos == null) _todosLosProductos = new List<Product>();
            AplicarFiltros();
        }

        // --- SECCIÓN 1: LÓGICA DE FILTRADOS ---

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
            if (_todosLosProductos == null) return;

            var consulta = _todosLosProductos.AsEnumerable();

            if (!string.IsNullOrEmpty(_filtroCategoria))
                consulta = consulta.Where(p => p.Category != null && p.Category.Equals(_filtroCategoria, StringComparison.OrdinalIgnoreCase));

            if (_filtroSubCategoria != "Todo")
                consulta = consulta.Where(p => p.SubCategory != null && p.SubCategory.Equals(_filtroSubCategoria, StringComparison.OrdinalIgnoreCase));

            Productos.Clear();
            foreach (var p in consulta) Productos.Add(p);
        }

        // --- SECCIÓN 2: LÓGICA DEL MENÚ DE OPCIONES ---

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

        // Opción A: VER DETALLE
        private void OpVer_Click(object sender, RoutedEventArgs e)
        {
            AbrirFichaProducto(sender);
        }

        // Opción B: EDITAR
        private void OpEditar_Click(object sender, RoutedEventArgs e)
        {
            AbrirFichaProducto(sender);
        }

        // Lógica común para abrir la ventana
        private void AbrirFichaProducto(object sender)
        {
            int id = ObtenerId(sender);
            var prod = _todosLosProductos.FirstOrDefault(p => p.Id == id);

            if (prod != null)
            {
                ProductDetailWindow detalle = new ProductDetailWindow(prod);
                detalle.Owner = Window.GetWindow(this);

                detalle.ShowDialog();

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

        // Opción C: ELIMINAR RÁPIDO
        private void OpEliminar_Click(object sender, RoutedEventArgs e)
        {
            int id = ObtenerId(sender);
            var prod = _todosLosProductos.FirstOrDefault(p => p.Id == id);

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

        // --- SECCIÓN 3: BOTÓN AÑADIR PRODUCTO (Nuevo) ---

        private void BtnAnadir_Click(object sender, RoutedEventArgs e)
        {
            // 1. Crear un producto vacío / placeholder
            var nuevoProd = new Product
            {
                // Calcular ID nuevo (Max actual + 1)
                Id = _todosLosProductos.Any() ? _todosLosProductos.Max(p => p.Id) + 1 : 1,
                Name = "NUEVO (Click Editar)",
                Category = "Platos",
                SubCategory = "Todo",
                Price = 0,
                IsAvailable = true,
                Ingredients = "",
                Allergens = "",
                ImagePath = ""
            };

            // 2. Abrir la ventana de detalle reutilizada
            ProductDetailWindow detalle = new ProductDetailWindow(nuevoProd);
            detalle.Owner = Window.GetWindow(this);

            // 3. Esperar a que el usuario edite y guarde
            detalle.ShowDialog();

            // 4. Si el usuario pulsó "Guardar Cambios" dentro de la ventana:
            if (detalle.ActionEdit)
            {
                // Añadimos a la lista maestra
                _todosLosProductos.Add(nuevoProd);

                // Refrescamos la vista
                AplicarFiltros();

                // Opcional: Scrollear al nuevo elemento (requiere buscarlo en la UI, se puede omitir por simplicidad)
            }
        }

        private void EliminarProducto(Product prod)
        {
            // _controller.DeleteProduct(prod.Id);
            _todosLosProductos.Remove(prod);
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