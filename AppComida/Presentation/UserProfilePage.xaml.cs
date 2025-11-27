using AppComida.Domain;
using System;
using System.IO; // Necesario para File.Exists
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AppComida.Presentation
{
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

            // Datos de texto
            TxtFirstName.Text = user.firstname;
                

            // Generación de datos simulados si faltan
            string safeName = (user.firstname ?? "Usuario").ToLower().Replace(" ", "");
            string safeLast = (user.lastname ?? "Prueba").Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)[0];

            TxtUsername.Text = "@" + user.username;
            TxtEmail.Text = user.email;
            TxtLastAccess.Text = DateTime.Now.ToString("dd MMM, HH:mm tt");
            TxtLastName.Text = safeLast;

            // --- LÓGICA SEGURA DE IMAGEN ---
            // Solo intentamos cargar si hay una ruta escrita
            if (!string.IsNullOrEmpty(user.image))
            {
                try
                {
                    // 1. Verificar si es una ruta absoluta en disco y si existe
                    if (File.Exists(user.image))
                    {
                        var bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(user.image, UriKind.Absolute);
                        bitmap.CacheOption = BitmapCacheOption.OnLoad;
                        bitmap.EndInit();
                        ProfileImageBrush.ImageSource = bitmap;
                    }
                    else
                    {
                        // 2. Si no es absoluta, intentar como recurso relativo (pack URI)
                        // Esto evita el FileNotFoundException de rutas relativas simples
                        try
                        {
                            // Asumimos que la ruta relativa empieza en Assets, ej: "Assets/Images/..."
                            string relativePath = user.image.Replace('\\', '/').TrimStart('/');
                            var uri = new Uri($"pack://application:,,,/{relativePath}", UriKind.Absolute);

                            // Prueba de carga segura
                            var bitmap = new BitmapImage(uri);
                            ProfileImageBrush.ImageSource = bitmap;
                        }
                        catch
                        {
                            // Si falla también como recurso, no hacemos nada.
                            // Se mostrará el círculo gris por defecto.
                        }
                    }
                }
                catch
                {
                    // Ante cualquier otro error imprevisto, no rompemos la app.
                    // Se queda la imagen vacía (placeholder gris).
                }
            }
        }
    }
}