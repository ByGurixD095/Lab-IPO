using AppComida.Domain;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AppComida.Presentation
{
    public partial class MainWindow : Window
    {
        private readonly User _userLogged;
        private readonly LoginController _controller;

        private UserProfilePage _profilePage;
        private PedidosPage _pedidosPage;
        private ProductosPage _productosPage;
        private ClientesPage _clientesPage;

        // LISTAS MAESTRAS: La única fuente de verdad mientras la app corre
        private List<Product> _masterProductos;
        private List<Client> _masterClientes;
        private List<Pedido> _masterPedidos;

        public MainWindow(User user)
        {
            InitializeComponent();
            _userLogged = user;
            _controller = new LoginController();

            // 1. Cargar datos iniciales una sola vez
            CargarDatosInicialesGlobales();

            CargarDatosUsuario(user);
            setDate(null, null);
            InitializeToggleButtons();
            InicializarVistas();
        }

        private void CargarDatosInicialesGlobales()
        {
            _masterProductos = new ProductController().GetAllProducts() ?? new List<Product>();
            _masterClientes = new ClientController().GetAllClients() ?? new List<Client>();
            _masterPedidos = new PedidoController().GetAllPedidos() ?? new List<Pedido>();
        }

        private void InitializeToggleButtons()
        {
            if (MenuSalir != null)
            {
                MenuSalir.Opened += (s, e) => ToggleSalir.IsChecked = true;
                MenuSalir.Closed += (s, e) => ToggleSalir.IsChecked = false;
            }
            if (MenuAyuda != null)
            {
                MenuAyuda.Opened += (s, e) => ToggleAyuda.IsChecked = true;
                MenuAyuda.Closed += (s, e) => ToggleAyuda.IsChecked = false;
            }
        }

        private void InicializarVistas()
        {
            _profilePage = new UserProfilePage(_userLogged);
            _pedidosPage = new PedidosPage();
            _productosPage = new ProductosPage();
            _clientesPage = new ClientesPage();

            if (FrameProfile != null) FrameProfile.Navigate(_profilePage);
            if (FramePedidos != null) FramePedidos.Navigate(_pedidosPage);
            if (FrameProductos != null) FrameProductos.Navigate(_productosPage);
            if (FrameClientes != null) FrameClientes.Navigate(_clientesPage);

            // Inyectar datos iniciales
            _productosPage.SetProductos(_masterProductos);
            _clientesPage.SetClientes(_masterClientes);
            _pedidosPage.SetContext(_masterPedidos, _masterProductos, _masterClientes);
        }

        // === SINCRONIZACIÓN Y NAVEGACIÓN ===

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void SincronizarDatosGlobales()
        {
            // Antes de cambiar de vista, recuperamos el estado más reciente de cada página
            if (_productosPage != null) _masterProductos = _productosPage.GetProductos();
            if (_clientesPage != null) _masterClientes = _clientesPage.GetClientes();
            if (_pedidosPage != null) _masterPedidos = _pedidosPage.GetPedidos();
        }

        private void NavigateToView(string viewTag)
        {
            SincronizarDatosGlobales(); // Guardar estado actual

            if (FrameProfile != null) FrameProfile.Visibility = Visibility.Collapsed;
            if (FramePedidos != null) FramePedidos.Visibility = Visibility.Collapsed;
            if (FrameProductos != null) FrameProductos.Visibility = Visibility.Collapsed;
            if (FrameClientes != null) FrameClientes.Visibility = Visibility.Collapsed;

            switch (viewTag)
            {
                case "PedidosView":
                    // Pasamos todo actualizado a Pedidos
                    _pedidosPage.SetContext(_masterPedidos, _masterProductos, _masterClientes);
                    if (FramePedidos != null) FramePedidos.Visibility = Visibility.Visible;
                    break;

                case "ClientesView":
                    // Pasamos clientes y pedidos actualizados a ClientesPage
                    _clientesPage.SetClientes(_masterClientes);
                    _clientesPage.UpdateOrders(_masterPedidos);
                    if (FrameClientes != null) FrameClientes.Visibility = Visibility.Visible;
                    break;

                case "ProductosView":
                    _productosPage.SetProductos(_masterProductos);
                    if (FrameProductos != null) FrameProductos.Visibility = Visibility.Visible;
                    break;

                default:
                    if (FrameProfile != null) FrameProfile.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void NavButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radio && radio.IsChecked == true)
            {
                string targetViewTag = radio.CommandParameter?.ToString();
                if (!string.IsNullOrEmpty(targetViewTag))
                {
                    NavigateToView(targetViewTag);
                    UpdateHeader(radio.Content.ToString(), targetViewTag);
                }
            }
        }

        private void UpdateHeader(string title, string tag)
        {
            if (LblHeaderTitle == null || LblHeaderSubtitle == null) return;
            string subtitle;
            if (tag.Contains(" ")) subtitle = tag;
            else
            {
                switch (tag)
                {
                    case "PedidosView": subtitle = "Gestión de comandas y estado de mesas"; break;
                    case "ProductosView": subtitle = "Catálogo, precios e inventario"; break;
                    case "ClientesView": subtitle = "Base de datos y gestión de clientes"; break;
                    default: subtitle = "Área de gestión del TPV"; break;
                }
            }
            LblHeaderTitle.Text = title;
            LblHeaderSubtitle.Text = subtitle;
        }

        // UI Helpers
        private void ToggleAyuda_Click(object sender, RoutedEventArgs e) { if (ToggleAyuda.ContextMenu != null) { ToggleAyuda.IsChecked = true; ToggleAyuda.ContextMenu.PlacementTarget = ToggleAyuda; ToggleAyuda.ContextMenu.Placement = PlacementMode.Top; ToggleAyuda.ContextMenu.HorizontalOffset = 5; ToggleAyuda.ContextMenu.IsOpen = true; } }
        private void MenuTemaClaro_Click(object sender, RoutedEventArgs e) => CambiarTema("/Themes/TemaClaro.xaml");
        private void MenuTemaOscuro_Click(object sender, RoutedEventArgs e) => CambiarTema("/Themes/TemaOscuro.xaml");
        private void CambiarTema(string rutaTema) { try { var uri = new Uri(rutaTema, UriKind.Relative); ResourceDictionary nuevoTema = Application.LoadComponent(uri) as ResourceDictionary; if (nuevoTema != null) { var diccionarios = Application.Current.Resources.MergedDictionaries; diccionarios.Clear(); diccionarios.Add(nuevoTema); diccionarios.Add(new ResourceDictionary { Source = new Uri("/Themes/Icons.xaml", UriKind.Relative) }); diccionarios.Add(new ResourceDictionary { Source = new Uri("/Themes/ControlStyles.xaml", UriKind.Relative) }); diccionarios.Add(new ResourceDictionary { Source = new Uri("/Themes/ProductStyles.xaml", UriKind.Relative) }); diccionarios.Add(new ResourceDictionary { Source = new Uri("/Themes/ClientStyles.xaml", UriKind.Relative) }); diccionarios.Add(new ResourceDictionary { Source = new Uri("/Themes/OrderStyles.xaml", UriKind.Relative) }); } } catch (Exception ex) { MessageBox.Show($"Error cargando tema: {ex.Message}"); } }
        private void MenuEsp_Click(object sender, RoutedEventArgs e) { ConfirmWindow confirm = new ConfirmWindow("Tampoco me pagan tanto como para implementar el idioma", "Casi crack", ConfirmType.Info); confirm.Owner = this; confirm.ShowDialog(); }
        private void MenuEng_Click(object sender, RoutedEventArgs e) { ConfirmWindow confirm = new ConfirmWindow("Tampoco me pagan tanto como para implementar el idioma", "Casi crack", ConfirmType.Info); confirm.Owner = this; confirm.ShowDialog(); }
        private void MenuItemPerfil_Click(object sender, RoutedEventArgs e) { DesmarcarNavegacion(); UpdateHeader("Mi Perfil", "Datos del usuario y estadísticas"); NavigateToView("UserProfile"); }
        private void MenuItemAcercaDe_Click(object sender, RoutedEventArgs e) { ConfirmWindow info = new ConfirmWindow("Trabajo IPO 2025-2026.\nDiseño 'Floating Modern' corrección v2.", "Acerca de", ConfirmType.Info); info.Owner = this; info.ShowDialog(); }
        private void DesmarcarNavegacion() { if (BtnPedidos != null) BtnPedidos.IsChecked = false; if (BtnProductos != null) BtnProductos.IsChecked = false; if (BtnClientes != null) BtnClientes.IsChecked = false; }
        private void GuardarHoraSalida() { if (_userLogged != null) _controller.RegisterExit(_userLogged.username); }
        private void ToggleSalir_Click(object sender, RoutedEventArgs e) { if (ToggleSalir.ContextMenu != null) { ToggleSalir.IsChecked = true; ToggleSalir.ContextMenu.PlacementTarget = ToggleSalir; ToggleSalir.ContextMenu.Placement = PlacementMode.Top; ToggleSalir.ContextMenu.HorizontalOffset = 5; ToggleSalir.ContextMenu.IsOpen = true; } }
        private void MenuItemCerrarSesion_Click(object sender, RoutedEventArgs e) { ConfirmWindow confirm = new ConfirmWindow("¿Estás seguro de que deseas cerrar la sesión?\nSe perderán los datos no guardados.", "Cerrar Sesión", ConfirmType.Question); confirm.Owner = this; if (confirm.ShowDialog() == true) { GuardarHoraSalida(); LoginWindow loginWindow = new LoginWindow(); loginWindow.Show(); this.Close(); } }
        private void MenuItemSalirApp_Click(object sender, RoutedEventArgs e) { ConfirmWindow confirm = new ConfirmWindow("La aplicación se cerrará por completo.\n¿Deseas apagar el sistema?", "Salir de la App", ConfirmType.Danger); confirm.Owner = this; if (confirm.ShowDialog() == true) { GuardarHoraSalida(); Application.Current.Shutdown(); } }
        public void CargarDatosUsuario(User usuarioLogeado) { if (usuarioLogeado == null) return; string primerApellido = string.IsNullOrEmpty(usuarioLogeado.lastname) ? "" : usuarioLogeado.lastname.Split(null as char[], StringSplitOptions.RemoveEmptyEntries)[0]; if (LblNombreUsuario != null) LblNombreUsuario.Text = $"{usuarioLogeado.firstname} {primerApellido}"; if (LblRolUsuario != null) LblRolUsuario.Text = "Administrador"; if (!string.IsNullOrEmpty(usuarioLogeado.image) && AvatarEllipse != null) { try { BitmapImage bitmap = new BitmapImage(); bitmap.BeginInit(); string imagePath = usuarioLogeado.image.Replace('\\', '/'); Uri imageUri = new Uri($"pack://application:,,/{imagePath}", UriKind.Absolute); bitmap.UriSource = imageUri; bitmap.CacheOption = BitmapCacheOption.OnLoad; bitmap.EndInit(); ImageBrush imageBrush = new ImageBrush(bitmap) { Stretch = Stretch.UniformToFill }; AvatarEllipse.Fill = imageBrush; } catch { } } }
        private void setDate(object sender, EventArgs e) { DateTime now = DateTime.Now; CultureInfo culture = CultureInfo.GetCultureInfo("es-ES"); if (LblDayOfWeek != null) LblDayOfWeek.Text = now.ToString("dddd", culture).ToUpper(); if (LblDayOfMonth != null) LblDayOfMonth.Text = now.ToString("dd"); if (LblMonth != null) LblMonth.Text = now.ToString("MMM", culture).ToUpper(); }
    }
}