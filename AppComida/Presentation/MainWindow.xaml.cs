using AppComida.Domain;
using System;
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

        public MainWindow(User user)
        {
            InitializeComponent();
            _userLogged = user;
            _controller = new LoginController();

            CargarDatosUsuario(user);
            setDate(null, null);

            // Iniciar eventos de los toggle buttons
            InitializeToggleButtons();

            // Iniciar las pages de las funcionalidades
            InicializarVistas();
        }

        // Inicalizar vetana principal app
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
            if (FrameProfile != null) FrameProfile.Navigate(new UserProfilePage(_userLogged));

            if (FramePedidos != null) FramePedidos.Navigate(new PedidosPage());

            if (FrameProductos != null) FrameProductos.Navigate(new ProductosPage());

            if (FrameClientes != null) FrameClientes.Navigate(new ClientesPage());
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        // Help Button actions
        private void ToggleAyuda_Click(object sender, RoutedEventArgs e)
        {
            if (ToggleAyuda.ContextMenu != null)
            {
                ToggleAyuda.IsChecked = true;
                ToggleAyuda.ContextMenu.PlacementTarget = ToggleAyuda;
                ToggleAyuda.ContextMenu.Placement = PlacementMode.Top;
                ToggleAyuda.ContextMenu.HorizontalOffset = 5;
                ToggleAyuda.ContextMenu.IsOpen = true;
            }
        }

        private void MenuTemaClaro_Click(object sender, RoutedEventArgs e)
        {
            CambiarTema("/Themes/TemaClaro.xaml");
        }

        private void MenuTemaOscuro_Click(object sender, RoutedEventArgs e)
        {
            CambiarTema("/Themes/TemaOscuro.xaml");
        }

        private void CambiarTema(string rutaTema)
        {
            try
            {
                // 1. Cargar el nuevo diccionario de colores 
                var uri = new Uri(rutaTema, UriKind.Relative);
                ResourceDictionary nuevoTema = Application.LoadComponent(uri) as ResourceDictionary;

                if (nuevoTema != null)
                {
                    // 2. Limpiar todo
                    var diccionarios = Application.Current.Resources.MergedDictionaries;
                    diccionarios.Clear();

                    // 3. Añadir el  tema de colores
                    diccionarios.Add(nuevoTema);

                    // 4. RESTAURAR LOS ESTILOS ESENCIALES
                    diccionarios.Add(new ResourceDictionary { Source = new Uri("/Themes/Icons.xaml", UriKind.Relative) });
                    diccionarios.Add(new ResourceDictionary { Source = new Uri("/Themes/ControlStyles.xaml", UriKind.Relative) });
                    diccionarios.Add(new ResourceDictionary { Source = new Uri("/Themes/ProductStyles.xaml", UriKind.Relative) });
                    diccionarios.Add(new ResourceDictionary { Source = new Uri("/Themes/ClientStyles.xaml", UriKind.Relative) });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cargando tema: {ex.Message}\n\nRuta intentada: {rutaTema}");
            }
        }

        private void MenuEsp_Click(object sender, RoutedEventArgs e)
        {
            ConfirmWindow confirm = new ConfirmWindow(
               "Tampoco me pagan tanto como para implementar el idioma",
               "Casi crack",
               ConfirmType.Info);
            confirm.Owner = this;
            confirm.ShowDialog();
        }

        private void MenuEng_Click(object sender, RoutedEventArgs e)
        {
            ConfirmWindow confirm = new ConfirmWindow(
              "Tampoco me pagan tanto como para implementar el idioma",
              "Casi crack",
              ConfirmType.Info);
            confirm.Owner = this;
            confirm.ShowDialog();
        }

        private void MenuItemPerfil_Click(object sender, RoutedEventArgs e)
        {
            DesmarcarNavegacion();
            UpdateHeader("Mi Perfil", "Datos del usuario y estadísticas");
            NavigateToView("UserProfile");
        }

        private void MenuItemAcercaDe_Click(object sender, RoutedEventArgs e)
        {
            ConfirmWindow info = new ConfirmWindow(
                "Trabajo IPO 2025-2026.\nDiseño 'Floating Modern' corrección v2.",
                "Acerca de",
                ConfirmType.Info);

            info.Owner = this;
            info.ShowDialog();
        }

        private void DesmarcarNavegacion()
        {
            if (BtnPedidos != null) BtnPedidos.IsChecked = false;
            if (BtnProductos != null) BtnProductos.IsChecked = false;
            if (BtnClientes != null) BtnClientes.IsChecked = false;
        }

        private void GuardarHoraSalida()
        {
            if (_userLogged != null)
            {
                _controller.RegisterExit(_userLogged.username);
            }
        }

        // Salir Button 

        private void ToggleSalir_Click(object sender, RoutedEventArgs e)
        {
            if (ToggleSalir.ContextMenu != null)
            {
                ToggleSalir.IsChecked = true;
                ToggleSalir.ContextMenu.PlacementTarget = ToggleSalir;
                ToggleSalir.ContextMenu.Placement = PlacementMode.Top;
                ToggleSalir.ContextMenu.HorizontalOffset = 5;
                ToggleSalir.ContextMenu.IsOpen = true;
            }
        }

        private void MenuItemCerrarSesion_Click(object sender, RoutedEventArgs e)
        {
            ConfirmWindow confirm = new ConfirmWindow(
                "¿Estás seguro de que deseas cerrar la sesión?\nSe perderán los datos no guardados.",
                "Cerrar Sesión",
                ConfirmType.Question);
            confirm.Owner = this;

            if (confirm.ShowDialog() == true)
            {
                GuardarHoraSalida();
                LoginWindow loginWindow = new LoginWindow();
                loginWindow.Show();
                this.Close();
            }
        }

        private void MenuItemSalirApp_Click(object sender, RoutedEventArgs e)
        {
            ConfirmWindow confirm = new ConfirmWindow(
                "La aplicación se cerrará por completo.\n¿Deseas apagar el sistema?",
                "Salir de la App",
                ConfirmType.Danger);
            confirm.Owner = this;

            if (confirm.ShowDialog() == true)
            {
                GuardarHoraSalida();
                Application.Current.Shutdown();
            }
        }

        // General Purpose 
        public void CargarDatosUsuario(User usuarioLogeado)
        {
            if (usuarioLogeado == null) return;

            string primerApellido = string.IsNullOrEmpty(usuarioLogeado.lastname)
                ? ""
                : usuarioLogeado.lastname.Split(null as char[], StringSplitOptions.RemoveEmptyEntries)[0];

            if (LblNombreUsuario != null)
                LblNombreUsuario.Text = $"{usuarioLogeado.firstname} {primerApellido}";

            if (LblRolUsuario != null)
                LblRolUsuario.Text = "Administrador";

            if (!string.IsNullOrEmpty(usuarioLogeado.image) && AvatarEllipse != null)
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    string imagePath = usuarioLogeado.image.Replace('\\', '/');
                    Uri imageUri = new Uri($"pack://application:,,/{imagePath}", UriKind.Absolute);
                    bitmap.UriSource = imageUri;
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();

                    ImageBrush imageBrush = new ImageBrush(bitmap)
                    {
                        Stretch = Stretch.UniformToFill
                    };
                    AvatarEllipse.Fill = imageBrush;
                }
                catch { }
            }
        }

        private void setDate(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            CultureInfo culture = CultureInfo.GetCultureInfo("es-ES");

            if (LblDayOfWeek != null) LblDayOfWeek.Text = now.ToString("dddd", culture).ToUpper();
            if (LblDayOfMonth != null) LblDayOfMonth.Text = now.ToString("dd");
            if (LblMonth != null) LblMonth.Text = now.ToString("MMM", culture).ToUpper();
        }

        // Change page view
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

        // NUEVA LÓGICA: ALTERNAR VISIBILIDAD
        private void NavigateToView(string viewTag)
        {
            // 1. Ocultar todos
            if (FrameProfile != null) FrameProfile.Visibility = Visibility.Collapsed;
            if (FramePedidos != null) FramePedidos.Visibility = Visibility.Collapsed;
            if (FrameProductos != null) FrameProductos.Visibility = Visibility.Collapsed;
            if (FrameClientes != null) FrameClientes.Visibility = Visibility.Collapsed;

            // 2. Mostrar el seleccionado
            switch (viewTag)
            {
                case "PedidosView":
                    if (FramePedidos != null) FramePedidos.Visibility = Visibility.Visible;
                    break;
                case "ProductosView":
                    if (FrameProductos != null) FrameProductos.Visibility = Visibility.Visible;
                    break;
                case "ClientesView":
                    if (FrameClientes != null) FrameClientes.Visibility = Visibility.Visible;
                    break;
                default:
                    if (FrameProfile != null) FrameProfile.Visibility = Visibility.Visible;
                    break;
            }
        }

        private void UpdateHeader(string title, string tag)
        {
            if (LblHeaderTitle == null || LblHeaderSubtitle == null) return;

            string subtitle;

            if (tag.Contains(" "))
            {
                subtitle = tag;
            }
            else
            {
                switch (tag)
                {
                    case "PedidosView":
                        subtitle = "Gestión de comandas y estado de mesas";
                        break;
                    case "ProductosView":
                        subtitle = "Catálogo, precios e inventario";
                        break;
                    case "ClientesView":
                        subtitle = "Base de datos y gestión de clientes";
                        break;
                    default:
                        subtitle = "Área de gestión del TPV";
                        break;
                }
            }

            LblHeaderTitle.Text = title;
            LblHeaderSubtitle.Text = subtitle;
        }
    }
}