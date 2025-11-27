using AppComida.Domain;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;           // Necesario para ImageBrush
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;   // Necesario para BitmapImage
using System.Windows.Shapes;          // Necesario para la clase base Shape
using System.Windows.Threading;

namespace AppComida.Presentation
{
    public partial class MainWindow : Window
    {
        private readonly User _userLogged;
        private DispatcherTimer _clockTimer;

        // La lógica P/Invoke ha sido eliminada.

        public MainWindow(User user)
        {
            InitializeComponent();
            _userLogged = user;
            CargarDatosUsuario(user);
            InicializarReloj();
        }

        // --- GESTIÓN DE LA VENTANA ---

        // Método de arrastre de la ventana
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Solo arrastrar si es el botón izquierdo del ratón
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void BtnSalir_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // La inicialización del reloj ya ocurre en el constructor
        }

        // --- LÓGICA DE USUARIO Y PERFIL ---

        /// <summary>
        /// Método auxiliar para asignar la imagen de avatar por defecto.
        /// </summary>
        private void SetDefaultAvatar()
        {
            // Creamos un Grid contenedor con gradiente sutil
            Grid container = new Grid();

            // Fondo con gradiente radial que simula profundidad
            RadialGradientBrush backgroundBrush = new RadialGradientBrush();
            backgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#546E7A"), 0.0));
            backgroundBrush.GradientStops.Add(new GradientStop((Color)ColorConverter.ConvertFromString("#37474F"), 1.0));
            container.Background = backgroundBrush;

            // Creamos el Path con el icono de usuario (mismo del XAML)
            Path userIcon = new Path
            {
                Data = Geometry.Parse("M12,4A4,4 0 0,1 16,8A4,4 0 0,1 12,12A4,4 0 0,1 8,8A4,4 0 0,1 12,4M12,14C16.42,14 20,15.79 20,18V20H4V18C4,15.79 7.58,14 12,14Z"),
                // Volvemos al gris claro del código original para evitar el 'brillo blanco'.
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#B0BEC5")),
                Stretch = Stretch.Uniform,
                Width = 20, // Mantenemos el tamaño
                Height = 20, // Mantenemos el tamaño
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = 0.9
            };

            // **IMPORTANTE: ELIMINAMOS EL DropShadowEffect** para quitar el difuminado (el halo naranja)
            userIcon.Effect = null; // Se elimina la sombra

            container.Children.Add(userIcon);

            // Creamos un VisualBrush a partir del Grid
            VisualBrush defaultBrush = new VisualBrush(container)
            {
                Stretch = Stretch.Fill
            };

            // Asignamos el pincel a la Elipse
            AvatarEllipse.Fill = defaultBrush;
        }

        public void CargarDatosUsuario(User usuarioLogeado)
        {
            // 1. Apellido seguro (Evita NRE)
            string primerApellido = string.IsNullOrEmpty(usuarioLogeado.lastname)
                ? ""
                : usuarioLogeado.lastname.Split(null as char[], StringSplitOptions.RemoveEmptyEntries)[0];

            // 2. Carga de datos de texto
            LblNombreUsuario.Text = $"{usuarioLogeado.firstname} {primerApellido}";
            LblRolUsuario.Text = "Administrador";

            // 3. Carga de la imagen de usuario
            if (!string.IsNullOrEmpty(usuarioLogeado.image))
            {
                try
                {
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(usuarioLogeado.image, UriKind.RelativeOrAbsolute);

                    // CRÍTICO: Esto fuerza la carga inmediata y lanza la excepción aquí
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;

                    bitmap.EndInit();

                    ImageBrush imageBrush = new ImageBrush(bitmap)
                    {
                        Stretch = Stretch.UniformToFill
                    };
                    AvatarEllipse.Fill = imageBrush;
                    return;
                }
                catch (Exception ex)
                {
                    // Ahora SÍ captura DirectoryNotFoundException y otros errores
                    System.Diagnostics.Debug.WriteLine($"Error al cargar la imagen: {ex.Message}");
                }
            }

            // Si la ruta es nula/vacía O la carga falló:
            SetDefaultAvatar();
        }

        // --- LÓGICA DE FECHA ---

        private void InicializarReloj()
        {
            _clockTimer = new DispatcherTimer(DispatcherPriority.Normal);
            _clockTimer.Interval = TimeSpan.FromMinutes(1);
            _clockTimer.Tick += UpdateClock;
            _clockTimer.Start();
            UpdateClock(null, null); // Llamada inicial para cargar la fecha inmediatamente
        }

        private void UpdateClock(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;
            CultureInfo culture = CultureInfo.GetCultureInfo("es-ES");

            // 1. Día de la semana (MIÉRCOLES)
            LblDayOfWeek.Text = now.ToString("dddd", culture).ToUpper();

            // 2. Día del mes (27) - Corresponde al texto grande
            LblDayOfMonth.Text = now.ToString("dd");

            // 3. Mes (NOV) - Corresponde al texto pequeño
            LblMonth.Text = now.ToString("MMM", culture).ToUpper();
        }

        // --- LÓGICA DE NAVEGACIÓN ---

        private void NavButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton radio && radio.IsChecked == true)
            {
                string targetViewTag = radio.Tag?.ToString();

                // 1. Lógica de Contenido
                // [Inferencia] Aquí iría la lógica para cargar el UserControl (Vista) correspondiente.

                // 2. Lógica de Header
                UpdateHeader(radio.Content.ToString(), targetViewTag);
            }
        }

        private void UpdateHeader(string title, string tag)
        {
            // Comprobación de nulidad para evitar excepciones si el evento se dispara antes de tiempo
            // Esto también soluciona el error de ejecución si los nombres XAML son incorrectos.
            if (LblHeaderTitle == null || LblHeaderSubtitle == null)
            {
                return;
            }

            string subtitle;

            // Determinar subtítulos basados en el Tag (Switch statement compatible con versiones antiguas)
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

            LblHeaderTitle.Text = title;
            LblHeaderSubtitle.Text = subtitle;
        }

    }
}