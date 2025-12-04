using AppComida.Domain;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace AppComida.Presentation
{
    /// <summary>
    /// Lógica de presentación para la ficha del usuario.
    /// Se encarga de formatear los datos brutos del modelo para su visualización y gestionar la carga de recursos gráficos.
    /// </summary>
    public partial class UserProfilePage : Page
    {
        public UserProfilePage(User user)
        {
            InitializeComponent();
            CargarDatos(user);
        }

        private void CargarDatos(User user)
        {
            if (user == null) return;

            // Mapeo directo de propiedades de texto
            TxtFirstName.Text = user.firstname;
            TxtEmail.Text = user.email;

            // Formateo de fecha para lectura
            TxtLastAccess.Text = user.last_access.ToString("dd MMM, HH:mm tt");

            // Tratamiento de datos para UI
            string safeLast = (user.lastname ?? "Usuario").Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];
            TxtLastName.Text = safeLast;

            // Decorador visual para el username
            TxtUsername.Text = "@" + user.username;

            SetProfileImage(user.image);
        }

        /// <summary>
        /// Gestiona la carga de la imagen de perfil soportando tanto rutas absolutas (ficheros locales)
        /// como recursos embebidos en la aplicación (Assets).
        /// </summary>
        private void SetProfileImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            try
            {
                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();

                bitmap.CacheOption = BitmapCacheOption.OnLoad;

                if (File.Exists(imagePath))
                {
                    bitmap.UriSource = new Uri(imagePath, UriKind.Absolute);
                }
                else
                {

                    string relativePath = imagePath.Replace('\\', '/').TrimStart('/');
                    bitmap.UriSource = new Uri($"pack://application:,,,/{relativePath}", UriKind.Absolute);
                }

                bitmap.EndInit();
                ProfileImageBrush.ImageSource = bitmap;
            }
            catch (Exception)
            {
            }
        }
    }
}