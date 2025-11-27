using AppComida.Domain;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace AppComida.Presentation
{
    public partial class MainWindow : Window
    {
        private readonly User _userLogged;
        private DispatcherTimer _clockTimer;

        public MainWindow() : this(new User
        {
            firstname = "Admin",
            lastname = "Prueba",
            image = "" 
        })
        {
        }
        public MainWindow(User user)
        {
            InitializeComponent();
            _userLogged = user;
            CargarDatosUsuario(user);
            setDate(null, null);
        
            // --- GESTIÓN DE ESTADOS DE TOGGLE BUTTONS ---

            // Toggle Salir (Gestión de apertura/cierre)
            if (MenuSalir != null)
            {
                MenuSalir.Opened += (s, e) => ToggleSalir.IsChecked = true;
                MenuSalir.Closed += (s, e) => ToggleSalir.IsChecked = false;
            }

            // Toggle Ayuda (Gestión de apertura/cierre)
            if (MenuAyuda != null)
            {
                MenuAyuda.Opened += (s, e) => ToggleAyuda.IsChecked = true;
                MenuAyuda.Closed += (s, e) => ToggleAyuda.IsChecked = false;
            }

            // NAVEGACIÓN INICIAL
            if (MainFrame != null)
            {
                MainFrame.Navigate(new UserProfilePage(_userLogged));
            }
        }

        // --- GESTIÓN DE VENTANA ---
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        // ==========================================
        //        SECCIÓN MENÚ AYUDA / AJUSTES
        // ==========================================

        // ESTE ES EL MÉTODO QUE FALTABA O NO SE ENCONTRABA
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
                var uri = new Uri(rutaTema, UriKind.Relative);

                ResourceDictionary nuevoTema = Application.LoadComponent(uri) as ResourceDictionary;

                if (nuevoTema != null)
                {
                    Application.Current.Resources.MergedDictionaries.Clear();
                    Application.Current.Resources.MergedDictionaries.Add(nuevoTema);
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
        }

        private void MenuEng_Click(object sender, RoutedEventArgs e)
        {
            ConfirmWindow confirm = new ConfirmWindow(
              "Tampoco me pagan tanto como para implementar el idioma",
              "Casi crack",
              ConfirmType.Info);
            confirm.Owner = this;
        }

        private void MenuItemPerfil_Click(object sender, RoutedEventArgs e)
        {
            // Navegar a la página de perfil
            DesmarcarNavegacion();

            UpdateHeader("Mi Perfil", "Datos del usuario y estadísticas");

            if (MainFrame != null)
            {
                MainFrame.Navigate(new UserProfilePage(_userLogged));
            }
        }

        // ESTE TAMBIÉN DABA ERROR
        private void MenuItemAcercaDe_Click(object sender, RoutedEventArgs e)
        {
            ConfirmWindow info = new ConfirmWindow(
                "Trabajo IPO 2025-2026.",
                "Acerca de",
                ConfirmType.Info);

            info.Owner = this;
            info.ShowDialog();
        }

        private void DesmarcarNavegacion()
        {
            // Lógica opcional para desmarcar visualmente los botones laterales si fuera necesario
        }


        // ==========================================
        //           SECCIÓN MENÚ SALIR
        // ==========================================

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
                Application.Current.Shutdown();
            }
        }

        // ==========================================
        //           LÓGICA GENERAL
        // ==========================================

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

        private void NavButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radio && radio.IsChecked == true)
            {
                string targetViewTag = radio.Tag?.ToString();
                NavigateToView(targetViewTag);
                UpdateHeader(radio.Content.ToString(), targetViewTag);
            }
        }

        private void NavigateToView(string viewTag)
        {
            if (MainFrame == null) return;

            switch (viewTag)
            {
                case "PedidosView":
                    MainFrame.Navigate(new PedidosPage());
                    break;
                case "ProductosView":
                    MainFrame.Navigate(new ProductosPage());
                    break;
                case "ClientesView":
                    MainFrame.Navigate(new ClientesPage());
                    break;
                default:
                    MainFrame.Navigate(new UserProfilePage(_userLogged));
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