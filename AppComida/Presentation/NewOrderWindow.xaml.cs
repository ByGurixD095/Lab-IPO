using AppComida.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AppComida.Presentation
{
    /// <summary>
    /// Ventana modal para la creación de nuevos pedidos.
    /// Genera dinámicamente la interfaz de productos basada en las categorías disponibles en memoria.
    /// </summary>
    public partial class NewOrderWindow : Window
    {
        #region Propiedades Públicas

        // Propiedad que leerá la ventana padre (MainWindow/PedidosPage) al cerrarse este diálogo
        public Pedido CreatedOrder { get; private set; }

        // Colección observable para el carrito
        public ObservableCollection<Product> CartItems { get; set; }

        #endregion

        #region Estado Interno

        // Referencias a los datos maestros
        private List<Product> _availableProducts;
        private List<Client> _availableClients;

        #endregion

        #region Constructor e Inicialización

        public NewOrderWindow(List<Product> products, List<Client> clients)
        {
            InitializeComponent();

            _availableProducts = products ?? new List<Product>();
            _availableClients = clients ?? new List<Client>();

            CartItems = new ObservableCollection<Product>();
            LstCart.ItemsSource = CartItems;

            LoadDataFromMemory();
        }

        private void LoadDataFromMemory()
        {
            // 1. Configuración del ComboBox de Clientes
            CmbClientes.ItemsSource = _availableClients;

            // Pre-seleccionar el primer cliente para agilizar la toma de nota
            if (_availableClients.Any())
                CmbClientes.SelectedIndex = 0;

            CmbEntrega.SelectedIndex = 0;

            // 2. Generación dinámica del menú de productos
            RenderProductMenu();
        }

        #endregion

        #region Generación Dinámica de UI

        /// <summary>
        /// Crea pestañas y botones en tiempo de ejecución basándose en las categorías de los productos.
        /// </summary>
        private void RenderProductMenu()
        {
            var categories = _availableProducts.Select(p => p.Category).Distinct().ToList();

            TabsMenu.Items.Clear();

            foreach (var cat in categories)
            {
                var tab = new TabItem { Header = cat };

                var scrollView = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
                var wrapPanel = new WrapPanel { Margin = new Thickness(10) };

                // Filtramos productos de esta categoría específica
                var productsInCat = _availableProducts.Where(p => p.Category == cat);

                foreach (var prod in productsInCat)
                {
                    wrapPanel.Children.Add(CreateProductButton(prod));
                }

                scrollView.Content = wrapPanel;
                tab.Content = scrollView;
                TabsMenu.Items.Add(tab);
            }

            // Seleccionar primera pestaña por defecto
            if (TabsMenu.Items.Count > 0)
                TabsMenu.SelectedIndex = 0;
        }

        private Button CreateProductButton(Product p)
        {
            // Creación manual del botón
            var btn = new Button
            {
                Width = 140,
                Height = 100,
                Margin = new Thickness(5),
                Background = Brushes.White,
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#E0E0E0"),
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = p // Guardamos la referencia del objeto completo en el Tag
            };

            // Estructura visual interna del botón
            var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            stack.Children.Add(new TextBlock
            {
                Text = p.Name,
                TextWrapping = TextWrapping.Wrap,
                FontWeight = FontWeights.Bold,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(5)
            });

            stack.Children.Add(new TextBlock
            {
                Text = $"{p.Price:C}",
                Foreground = (Brush)FindResource("TextSecondary"),
                TextAlignment = TextAlignment.Center
            });

            btn.Content = stack;

            // Evento anónimo para añadir al carrito
            btn.Click += (s, e) => {
                CartItems.Add(p);
                RecalculateTotal();
            };

            return btn;
        }

        #endregion

        #region Gestión del Carrito

        private void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            // Recuperamos el objeto desde el Tag del botón de borrar
            if (sender is Button btn && btn.Tag is Product p)
            {
                CartItems.Remove(p);
                RecalculateTotal();
            }
        }

        private void RecalculateTotal()
        {
            // Actualización visual
            TxtTotal.Text = $"{CartItems.Sum(p => p.Price):C}";
        }

        #endregion

        #region Finalización del Pedido

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (CartItems.Count == 0)
            {
                MessageBox.Show("No se puede crear una comanda vacía.", "Carrito Vacío", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Recopilación de datos del formulario
            var cliente = CmbClientes.SelectedItem as Client;

            // Extracción segura del ComboBoxItem
            var entrega = (CmbEntrega.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Construcción del objeto final
            CreatedOrder = new Pedido
            {
                // Generación de ID simulada
                Id = new Random().Next(2000, 9999).ToString(),

                ClienteId = cliente != null ? cliente.Id : 0,
                NombreCliente = cliente != null ? cliente.NombreCompleto : "Cliente Mostrador",
                TipoEntrega = entrega,

                // Fechas actuales del sistema
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                Hora = DateTime.Now.ToString("HH:mm"),
                Estado = "Pendiente",

                Total = (decimal)CartItems.Sum(p => p.Price),
                Items = CartItems.Select(p => p.Name).ToList(),

                // Referencia en memoria para evitar recargas
                ClienteVinculado = cliente
            };

            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        #endregion
    }
}