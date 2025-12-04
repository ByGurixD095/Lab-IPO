using AppComida.Domain;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AppComida.Presentation
{
    /// <summary>
    /// Lógica de negocio para la vista de gestión de pedidos.
    /// Maneja el listado, filtrado y las operaciones CRUD sobre las comandas.
    /// </summary>
    public partial class PedidosPage : Page
    {
        #region Campos y Estado

        private List<Pedido> _allPedidosDb;

        private List<Product> _externalProducts;
        private List<Client> _externalClients;

        public ObservableCollection<Pedido> PedidosList { get; set; }

        private Pedido _selectedPedido;

        #endregion

        #region Constructor e Inicialización

        public PedidosPage()
        {
            InitializeComponent();

            PedidosList = new ObservableCollection<Pedido>();
            OrderList.ItemsSource = PedidosList;

            _allPedidosDb = new List<Pedido>();
            _externalProducts = new List<Product>();
            _externalClients = new List<Client>();
        }

        /// <summary>
        /// Método de "Inyección de Dependencias Manual".
        /// MainWindow llama a este método cada vez que se muestra la vista para asegurar que
        /// trabajamos con los datos más recientes (clientes nuevos, productos modificados, etc).
        /// </summary>
        public void SetContext(List<Pedido> pedidos, List<Product> productos, List<Client> clientes)
        {
            _allPedidosDb = pedidos;
            _externalProducts = productos;
            _externalClients = clientes;

            RefreshList(TxtSearch.Text.Trim());
        }

        public List<Pedido> GetPedidos()
        {
            return _allPedidosDb;
        }

        #endregion

        #region Lógica de Listado y Filtrado

        private void RefreshList(string filter = "")
        {
            PedidosList.Clear();
            if (_allPedidosDb == null) return;

            // Filtrado multicriterio
            var filteredData = string.IsNullOrWhiteSpace(filter)
                ? _allPedidosDb
                : _allPedidosDb.Where(p =>
                    (p.NombreCliente != null && p.NombreCliente.ToLower().Contains(filter.ToLower())) ||
                    (p.Id != null && p.Id.Contains(filter)) ||
                    (p.Estado != null && p.Estado.ToLower().Contains(filter.ToLower()))
                  );

            // Reconstrucción de la ObservableCollection
            foreach (var pedido in filteredData)
            {
                PedidosList.Add(pedido);
            }
        }

        private void OrderList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (OrderList.SelectedItem is Pedido pedido)
            {
                _selectedPedido = pedido;
                ShowDetailPanel(pedido);
            }
            else
            {
                // Si se pierde la selección, ocultamos el panel de detalles
                PanelDetail.Visibility = Visibility.Collapsed;
                _selectedPedido = null;
            }
        }

        #endregion

        #region Panel de Detalles y Edición

        private void ShowDetailPanel(Pedido p)
        {
            PanelDetail.Visibility = Visibility.Visible;

            // Binding manual de datos al panel lateral
            TxtDetailId.Text = $"#{p.Id}";
            TxtDetailClientName.Text = p.NombreCliente;
            TxtDetailClientId.Text = p.CustomerIdDisplay;
            TxtDetailTotal.Text = $"{p.Total:N2}€";

            // Selección segura de ComboBoxes
            CmbDetailStatus.Text = p.Estado;
            CmbDetailDelivery.Text = p.TipoEntrega;

            // Forzamos el refresco de la lista de items interna
            LstDetailItems.ItemsSource = null;
            LstDetailItems.ItemsSource = new List<string>(p.Items);
        }

        private void BtnSaveDetail_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPedido == null) return;

            var confirm = new ConfirmWindow("¿Deseas guardar los cambios de estado del pedido?", "Confirmar Edición", ConfirmType.Question);
            confirm.Owner = Window.GetWindow(this);

            if (confirm.ShowDialog() == true)
            {
                // Persistencia en memoria
                _selectedPedido.Estado = CmbDetailStatus.Text;
                _selectedPedido.TipoEntrega = CmbDetailDelivery.Text;

                RefreshList(TxtSearch.Text.Trim());

                // Mantenemos la selección para no perder el foco visual
                OrderList.SelectedItem = _selectedPedido;
            }
        }

        private void BtnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPedido == null) return;

            var confirm = new ConfirmWindow($"¿Estás seguro de ELIMINAR el pedido #{_selectedPedido.Id}?\nEsta acción no se puede deshacer.", "Eliminar Pedido", ConfirmType.Danger);
            confirm.Owner = Window.GetWindow(this);

            if (confirm.ShowDialog() == true)
            {
                _allPedidosDb.Remove(_selectedPedido);
                PedidosList.Remove(_selectedPedido);
                PanelDetail.Visibility = Visibility.Collapsed;
                _selectedPedido = null;
            }
        }

        private void BtnCloseDetail_Click(object sender, RoutedEventArgs e)
        {
            PanelDetail.Visibility = Visibility.Collapsed;
            OrderList.SelectedItem = null;
        }

        #endregion

        #region Gestión de Items del Pedido

        private void BtnDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            // Recuperamos el item desde el Tag del botón
            if (sender is Button btn && btn.Tag is string itemNombre)
            {
                _selectedPedido.Items.Remove(itemNombre);

                // Lógica de recálculo de precio
                if (_externalProducts != null)
                {
                    var productData = _externalProducts.FirstOrDefault(p => p.Name == itemNombre);
                    if (productData != null)
                    {
                        _selectedPedido.Total -= (decimal)productData.Price;
                        if (_selectedPedido.Total < 0) _selectedPedido.Total = 0;
                    }
                }

                // Refrescamos la vista
                ShowDetailPanel(_selectedPedido);
            }
        }

        private void BtnAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPedido == null) return;

            // Validación previa
            if (_externalProducts == null || !_externalProducts.Any())
            {
                MessageBox.Show("No hay productos disponibles en el catálogo para añadir.", "Catálogo Vacío", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            #region Construcción de UI Dinámica

            var dialog = new Window
            {
                Title = "Añadir Producto Extra",
                Width = 350,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = Window.GetWindow(this),
                WindowStyle = WindowStyle.ToolWindow,
                Background = Brushes.White
            };

            var stack = new StackPanel { Margin = new Thickness(20) };

            var cmbProducts = new ComboBox
            {
                ItemsSource = _externalProducts,
                DisplayMemberPath = "Name",
                Margin = new Thickness(0, 0, 0, 15),
                Height = 30,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            cmbProducts.SelectedIndex = 0;

            var btnConfirm = new Button
            {
                Content = "Añadir al Pedido",
                Height = 35,
                IsDefault = true,
                Background = (Brush)new BrushConverter().ConvertFrom("#4CAF50"),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold
            };

            // Evento local para capturar la selección
            btnConfirm.Click += (s, args) =>
            {
                if (cmbProducts.SelectedItem is Product p)
                {
                    _selectedPedido.Items.Add(p.Name);
                    _selectedPedido.Total += (decimal)p.Price;

                    dialog.DialogResult = true;
                    dialog.Close();
                }
            };

            stack.Children.Add(new TextBlock { Text = "Selecciona el producto:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 5) });
            stack.Children.Add(cmbProducts);
            stack.Children.Add(btnConfirm);

            dialog.Content = stack;

            #endregion

            if (dialog.ShowDialog() == true)
            {
                ShowDetailPanel(_selectedPedido);
            }
        }

        #endregion

        #region Acciones Globales

        private void BtnFilter_Click(object sender, RoutedEventArgs e) => RefreshList(TxtSearch.Text.Trim());

        private void BtnClear_Click(object sender, RoutedEventArgs e)
        {
            TxtSearch.Text = string.Empty;
            RefreshList();
        }

        private void TxtSearch_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                RefreshList(TxtSearch.Text.Trim());
        }

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            // Creamos la ventana de nuevo pedido pasándole las referencias actualizadas
            var newOrderWindow = new NewOrderWindow(_externalProducts, _externalClients);
            newOrderWindow.Owner = Window.GetWindow(this);

            if (newOrderWindow.ShowDialog() == true)
            {
                var nuevoPedido = newOrderWindow.CreatedOrder;
                if (nuevoPedido != null)
                {
                    Client c = nuevoPedido.ClienteVinculado;
                    if (c != null && c.Fidelizacion != null)
                    {
                        c.Fidelizacion.PuntosAcumulados += (nuevoPedido.Total > 20) ? 3 : 0;
                    }

                    // Añadimos al principio de la lista
                    _allPedidosDb.Insert(0, nuevoPedido);
                    PedidosList.Insert(0, nuevoPedido);

                    // Auto-selección del nuevo pedido
                    OrderList.SelectedItem = nuevoPedido;
                }
            }
        }

        #endregion
    }
}