using AppComida.Domain;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AppComida.Presentation
{
    public partial class PedidosPage : Page
    {
        private List<Pedido> _allPedidosDb;

        // Referencias a las listas vivas (sincronizadas con MainWindow)
        private List<Product> _externalProducts;
        private List<Client> _externalClients;

        public ObservableCollection<Pedido> PedidosList { get; set; }
        private Pedido _selectedPedido;

        public PedidosPage()
        {
            InitializeComponent();
            PedidosList = new ObservableCollection<Pedido>();
            OrderList.ItemsSource = PedidosList;

            // Inicializar vacío, MainWindow inyectará los datos al navegar aquí
            _allPedidosDb = new List<Pedido>();
            _externalProducts = new List<Product>();
            _externalClients = new List<Client>();
        }

        // --- MÉTODOS DE SINCRONIZACIÓN (Llamados por MainWindow) ---
        public void SetContext(List<Pedido> pedidos, List<Product> productos, List<Client> clientes)
        {
            _allPedidosDb = pedidos;
            _externalProducts = productos; // Aquí recibimos el catálogo actualizado de productos
            _externalClients = clientes;
            RefreshList(TxtSearch.Text.Trim());
        }

        public List<Pedido> GetPedidos()
        {
            return _allPedidosDb;
        }

        // --- LÓGICA PRINCIPAL ---

        private void RefreshList(string filter = "")
        {
            PedidosList.Clear();
            if (_allPedidosDb == null) return;

            var filteredData = string.IsNullOrWhiteSpace(filter)
                ? _allPedidosDb
                : _allPedidosDb.Where(p =>
                    (p.NombreCliente != null && p.NombreCliente.ToLower().Contains(filter.ToLower())) ||
                    (p.Id != null && p.Id.Contains(filter)) ||
                    (p.Estado != null && p.Estado.ToLower().Contains(filter.ToLower()))
                  );

            foreach (var pedido in filteredData) PedidosList.Add(pedido);
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
                PanelDetail.Visibility = Visibility.Collapsed;
                _selectedPedido = null;
            }
        }

        private void ShowDetailPanel(Pedido p)
        {
            PanelDetail.Visibility = Visibility.Visible;
            TxtDetailId.Text = $"#{p.Id}";
            TxtDetailClientName.Text = p.NombreCliente;
            TxtDetailClientId.Text = p.CustomerIdDisplay;
            TxtDetailTotal.Text = $"{p.Total:N2}€";
            CmbDetailStatus.Text = p.Estado;
            CmbDetailDelivery.Text = p.TipoEntrega;

            // Refrescamos la lista visual de items
            LstDetailItems.ItemsSource = null;
            LstDetailItems.ItemsSource = new List<string>(p.Items);
        }

        // --- BOTONES DE ACCIÓN ---

        private void BtnSaveDetail_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPedido == null) return;
            var confirm = new ConfirmWindow("¿Guardar los cambios del pedido?", "Confirmar Edición", ConfirmType.Question);
            confirm.Owner = Window.GetWindow(this);

            if (confirm.ShowDialog() == true)
            {
                _selectedPedido.Estado = CmbDetailStatus.Text;
                _selectedPedido.TipoEntrega = CmbDetailDelivery.Text;
                RefreshList(TxtSearch.Text.Trim());
                OrderList.SelectedItem = _selectedPedido;
            }
        }

        private void BtnDeleteOrder_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPedido == null) return;
            var confirm = new ConfirmWindow($"¿Estás seguro de ELIMINAR el pedido #{_selectedPedido.Id}?", "Eliminar Pedido", ConfirmType.Danger);
            confirm.Owner = Window.GetWindow(this);

            if (confirm.ShowDialog() == true)
            {
                _allPedidosDb.Remove(_selectedPedido);
                PedidosList.Remove(_selectedPedido);
                PanelDetail.Visibility = Visibility.Collapsed;
                _selectedPedido = null;
            }
        }

        // Eliminar un item individual de la lista
        private void BtnDeleteItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string itemNombre)
            {
                _selectedPedido.Items.Remove(itemNombre);
                // Restar precio buscando en el catálogo actual
                if (_externalProducts != null)
                {
                    var productData = _externalProducts.FirstOrDefault(p => p.Name == itemNombre);
                    if (productData != null)
                    {
                        _selectedPedido.Total -= (decimal)productData.Price;
                        if (_selectedPedido.Total < 0) _selectedPedido.Total = 0;
                    }
                }
                ShowDetailPanel(_selectedPedido);
            }
        }

        // --- NUEVO: AÑADIR PRODUCTO A UN PEDIDO EXISTENTE ---
        // Asocia este evento a tu nuevo botón en el XAML
        private void BtnAddItem_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedPedido == null) return;
            if (_externalProducts == null || !_externalProducts.Any())
            {
                MessageBox.Show("No hay productos disponibles en el catálogo.");
                return;
            }

            // 1. Creamos una ventana de diálogo dinámica (al vuelo)
            var dialog = new Window
            {
                Title = "Añadir Producto",
                Width = 350,
                Height = 180,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize,
                Owner = Window.GetWindow(this),
                WindowStyle = WindowStyle.ToolWindow,
                Background = Brushes.White
            };

            var stack = new StackPanel { Margin = new Thickness(20) };

            // 2. ComboBox para seleccionar el producto
            var cmbProducts = new ComboBox
            {
                ItemsSource = _externalProducts,
                DisplayMemberPath = "Name", // Muestra el nombre del producto
                Margin = new Thickness(0, 0, 0, 15),
                Height = 30,
                VerticalContentAlignment = VerticalAlignment.Center
            };
            cmbProducts.SelectedIndex = 0;

            // 3. Botón de confirmar
            var btnConfirm = new Button
            {
                Content = "Añadir al Pedido",
                Height = 35,
                IsDefault = true, // Se activa con Enter
                Background = (Brush)new BrushConverter().ConvertFrom("#4CAF50"),
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold
            };

            // Lógica al pulsar confirmar
            btnConfirm.Click += (s, args) =>
            {
                if (cmbProducts.SelectedItem is Product p)
                {
                    // Añadimos el nombre a la lista del pedido
                    _selectedPedido.Items.Add(p.Name);
                    // Sumamos el precio al total
                    _selectedPedido.Total += (decimal)p.Price;

                    dialog.DialogResult = true;
                    dialog.Close();
                }
            };

            stack.Children.Add(new TextBlock { Text = "Selecciona el producto a añadir:", FontWeight = FontWeights.SemiBold, Margin = new Thickness(0, 0, 0, 5) });
            stack.Children.Add(cmbProducts);
            stack.Children.Add(btnConfirm);

            dialog.Content = stack;

            // 4. Mostramos la ventana y si se acepta, refrescamos el panel
            if (dialog.ShowDialog() == true)
            {
                ShowDetailPanel(_selectedPedido);
            }
        }
        // ----------------------------------------------------

        private void BtnCloseDetail_Click(object sender, RoutedEventArgs e) { PanelDetail.Visibility = Visibility.Collapsed; OrderList.SelectedItem = null; }
        private void BtnFilter_Click(object sender, RoutedEventArgs e) => RefreshList(TxtSearch.Text.Trim());
        private void BtnClear_Click(object sender, RoutedEventArgs e) { TxtSearch.Text = string.Empty; RefreshList(); }
        private void TxtSearch_KeyUp(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) RefreshList(TxtSearch.Text.Trim()); }

        private void BtnAddOrder_Click(object sender, RoutedEventArgs e)
        {
            // Pasamos las listas vivas para que el nuevo pedido use datos actualizados
            var newOrderWindow = new NewOrderWindow(_externalProducts, _externalClients);
            newOrderWindow.Owner = Window.GetWindow(this);

            if (newOrderWindow.ShowDialog() == true)
            {
                var nuevoPedido = newOrderWindow.CreatedOrder;
                if (nuevoPedido != null)
                {
                    _allPedidosDb.Insert(0, nuevoPedido);
                    PedidosList.Insert(0, nuevoPedido);
                    OrderList.SelectedItem = nuevoPedido;
                }
            }
        }
    }
}