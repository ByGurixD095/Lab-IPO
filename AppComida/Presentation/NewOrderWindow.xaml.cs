using AppComida.Domain;
using AppComida.Persistence;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AppComida.Presentation
{
    public partial class NewOrderWindow : Window
    {
        public Pedido CreatedOrder { get; private set; }
        public ObservableCollection<Product> CartItems { get; set; }

        // Listas en memoria que nos pasan desde fuera
        private List<Product> _availableProducts;
        private List<Client> _availableClients;

        // Constructor modificado: OBLIGA a pasar las listas actuales
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
            // 1. Cargar Clientes pasados por parámetro
            CmbClientes.ItemsSource = _availableClients;
            if (_availableClients.Any()) CmbClientes.SelectedIndex = 0;

            CmbEntrega.SelectedIndex = 0;

            // 2. Cargar Productos pasados por parámetro
            var categories = _availableProducts.Select(p => p.Category).Distinct().ToList();

            TabsMenu.Items.Clear(); // Limpiamos por si acaso

            foreach (var cat in categories)
            {
                var tab = new TabItem { Header = cat };
                var scrollView = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
                var wrapPanel = new WrapPanel { Margin = new Thickness(10) };

                // Filtramos de la lista en memoria
                foreach (var prod in _availableProducts.Where(p => p.Category == cat))
                {
                    wrapPanel.Children.Add(CreateProductCard(prod));
                }

                scrollView.Content = wrapPanel;
                tab.Content = scrollView;
                TabsMenu.Items.Add(tab);
            }

            if (TabsMenu.Items.Count > 0) TabsMenu.SelectedIndex = 0;
        }

        private Button CreateProductCard(Product p)
        {
            var btn = new Button
            {
                Width = 140,
                Height = 100,
                Margin = new Thickness(5),
                Background = Brushes.White,
                BorderBrush = (Brush)new BrushConverter().ConvertFrom("#E0E0E0"),
                BorderThickness = new Thickness(1),
                Cursor = System.Windows.Input.Cursors.Hand,
                Tag = p
            };

            var stack = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
            stack.Children.Add(new TextBlock { Text = p.Name, TextWrapping = TextWrapping.Wrap, FontWeight = FontWeights.Bold, TextAlignment = TextAlignment.Center, Margin = new Thickness(5) });
            stack.Children.Add(new TextBlock { Text = $"{p.Price:C}", Foreground = (Brush)FindResource("TextSecondary"), TextAlignment = TextAlignment.Center });

            btn.Content = stack;
            btn.Click += (s, e) => { CartItems.Add(p); RecalculateTotal(); };
            return btn;
        }

        private void BtnRemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is Product p)
            {
                CartItems.Remove(p);
                RecalculateTotal();
            }
        }

        private void RecalculateTotal()
        {
            TxtTotal.Text = $"{CartItems.Sum(p => p.Price):C}";
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            if (CartItems.Count == 0)
            {
                MessageBox.Show("El carrito está vacío.");
                return;
            }

            var cliente = CmbClientes.SelectedItem as Client;
            var entrega = (CmbEntrega.SelectedItem as ComboBoxItem)?.Content.ToString();

            CreatedOrder = new Pedido
            {
                Id = new Random().Next(2000, 9999).ToString(),
                ClienteId = cliente != null ? cliente.Id : 0,
                NombreCliente = cliente != null ? cliente.NombreCompleto : "Cliente Mostrador",
                TipoEntrega = entrega,
                Fecha = DateTime.Now.ToString("yyyy-MM-dd"),
                Hora = DateTime.Now.ToString("HH:mm"),
                Estado = "Pendiente",
                Total = (decimal)CartItems.Sum(p => p.Price),
                Items = CartItems.Select(p => p.Name).ToList(),

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
    }
}