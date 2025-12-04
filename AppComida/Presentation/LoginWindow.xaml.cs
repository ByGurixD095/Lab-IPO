using AppComida.Domain;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AppComida.Presentation
{
    /// <summary>
    /// Lógica de interacción para la pantalla de inicio de sesión.
    /// Gestiona la validación visual de campos y la llamada asíncrona al controlador de autenticación.
    /// </summary>
    public partial class LoginWindow : Window
    {
        #region Campos Privados

        // Estilos cacheados para evitar crearlos en cada validación
        private readonly Brush _defaultBorder = Brushes.Transparent;
        private readonly Brush _errorBorder = (Brush)new BrushConverter().ConvertFrom("#D32F2F");

        private LoginController _logController;

        #endregion

        #region Constructor e Inicialización

        public LoginWindow()
        {
            InitializeComponent();
            _logController = new LoginController();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckCapsLock();
            txtUser.Focus();
        }

        #endregion

        #region Gestión de Ventana

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        #endregion

        #region Lógica de Entrada

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            CheckCapsLock();
        }

        private void Input_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                BtnLogin_Click(sender, e);
            }
        }

        private void CheckCapsLock()
        {
            bool isCapsLock = Keyboard.IsKeyToggled(Key.CapsLock);

            // Solo mostramos el aviso si el usuario está escribiendo en ese campo específico
            if (badgeCapsLockUser != null)
            {
                bool userHasFocus = txtUser.IsKeyboardFocused;
                badgeCapsLockUser.Visibility = (isCapsLock && userHasFocus) ? Visibility.Visible : Visibility.Collapsed;
            }

            if (badgeCapsLockPass != null)
            {
                bool passHasFocus = txtPass.IsKeyboardFocused || txtPassVisible.IsKeyboardFocused;
                badgeCapsLockPass.Visibility = (isCapsLock && passHasFocus) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        #endregion

        #region Gestión Visual de Placeholders y Errores

        private void txtUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblUserPlaceholder.Visibility = (txtUser.Text.Length == 0) ? Visibility.Visible : Visibility.Collapsed;
            ClearErrors();
        }

        private void txtPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            lblPassPlaceholder.Visibility = (txtPass.Password.Length == 0) ? Visibility.Visible : Visibility.Collapsed;
            ClearErrors();
        }

        private void txtPassVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblPassPlaceholder.Visibility = (txtPassVisible.Text.Length == 0) ? Visibility.Visible : Visibility.Collapsed;
            ClearErrors();
        }

        /// <summary>
        /// Limpia los estados de error visuales en cuanto el usuario empieza a corregir.
        /// </summary>
        private void ClearErrors()
        {
            if (lblUserError.Visibility == Visibility.Visible || txtUser.BorderBrush == _errorBorder)
            {
                lblUserError.Visibility = Visibility.Collapsed;
                txtUser.BorderBrush = _defaultBorder;
            }

            if (lblPassError.Visibility == Visibility.Visible || txtPass.BorderBrush == _errorBorder)
            {
                lblPassError.Visibility = Visibility.Collapsed;
                txtPass.BorderBrush = _defaultBorder;
                txtPassVisible.BorderBrush = _defaultBorder;
            }

            // Reseteamos el mensaje de error genérico al placeholder original
            if (lblPassPlaceholder.Text != "Introduce tu clave")
            {
                lblPassPlaceholder.Text = "Introduce tu clave";
                lblPassPlaceholder.Foreground = (Brush)FindResource("PlaceholderColor");
            }
        }

        private void BtnTogglePass_Click(object sender, RoutedEventArgs e)
        {
            // Alternancia entre PasswordBox (seguro) y TextBox (visible)
            if (txtPass.Visibility == Visibility.Visible)
            {
                // Mostrar contraseña
                txtPassVisible.Text = txtPass.Password;
                txtPass.Visibility = Visibility.Collapsed;
                txtPassVisible.Visibility = Visibility.Visible;

                btnTogglePass.Content = "🚫"; // Icono de ocultar

                // Actualizar estado del placeholder
                lblPassPlaceholder.Visibility = (txtPassVisible.Text.Length == 0) ? Visibility.Visible : Visibility.Collapsed;
                txtPassVisible.Focus();
            }
            else
            {
                // Ocultar contraseña
                txtPass.Password = txtPassVisible.Text;
                txtPassVisible.Visibility = Visibility.Collapsed;
                txtPass.Visibility = Visibility.Visible;

                btnTogglePass.Content = "👁️"; // Icono de ver

                lblPassPlaceholder.Visibility = (txtPass.Password.Length == 0) ? Visibility.Visible : Visibility.Collapsed;
                txtPass.Focus();
            }
        }

        #endregion

        #region Lógica de Autenticación

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // Unificamos el valor de la contraseña dependiendo del modo visible/oculto
            string finalPass = (txtPass.Visibility == Visibility.Visible) ? txtPass.Password : txtPassVisible.Text;
            string username = txtUser.Text.Trim();

            ClearErrors();

            // 1. Validación local
            if (string.IsNullOrWhiteSpace(username)) { ShowInputError(true); return; }
            if (finalPass.Length < 1) { ShowInputError(false); return; }

            // 2. Feedback visual
            SetLoadingState(true);

            // Pequeño delay artificial para que la animación de carga se vea fluida y no sea un parpadeo
            await Task.Delay(200);

            // 3. Proceso de Login en segundo plano para no congelar la UI
            User userLogged = null;

            try
            {
                await Task.Run(() => {
                    userLogged = _logController.ValidateLogin(username, finalPass);
                });
            }
            catch (Exception)
            {
            }

            if (userLogged != null)
            {
                // --- LOGIN CORRECTO ---
                if (btnLoader.Children.Count > 1 && btnLoader.Children[1] is TextBlock txtLoader)
                {
                    txtLoader.Text = "Accediendo...";
                }

                await Task.Delay(300); 

                MainWindow main = new MainWindow(userLogged);
                main.Show();
                this.Close();
            }
            else
            {
                // --- ERROR DE CREDENCIALES ---
                SetLoadingState(false);
                ShowGenericLoginError();
            }
        }

        #endregion

        #region Métodos Auxiliares de UI

        private void ShowInputError(bool isUserError)
        {
            if (isUserError)
            {
                lblUserError.Visibility = Visibility.Visible;
                txtUser.BorderBrush = _errorBorder;
                txtUser.Focus();
            }
            else
            {
                lblPassError.Visibility = Visibility.Visible;
                txtPass.BorderBrush = _errorBorder;
                txtPassVisible.BorderBrush = _errorBorder;

                if (txtPass.Visibility == Visibility.Visible) txtPass.Focus();
                else txtPassVisible.Focus();
            }
        }

        private void ShowGenericLoginError()
        {
            // Limpiamos campos de contraseña por seguridad
            txtPass.Password = "";
            txtPassVisible.Text = "";

            // Usamos el placeholder para mostrar el error
            lblPassPlaceholder.Text = "Usuario o contraseña incorrectos";
            lblPassPlaceholder.Foreground = (Brush)FindResource("ErrorColor");
            lblPassPlaceholder.Visibility = Visibility.Visible;

            txtUser.BorderBrush = _errorBorder;
            txtPass.BorderBrush = _errorBorder;
            txtPassVisible.BorderBrush = _errorBorder;

            if (txtPass.Visibility == Visibility.Visible) txtPass.Focus();
            else txtPassVisible.Focus();
        }

        private void SetLoadingState(bool isLoading)
        {
            if (isLoading)
            {
                btnLogin.IsEnabled = false;
                btnText.Visibility = Visibility.Collapsed;
                btnLoader.Visibility = Visibility.Visible;
                Cursor = Cursors.Wait;
            }
            else
            {
                btnLogin.IsEnabled = true;
                btnText.Visibility = Visibility.Visible;
                btnLoader.Visibility = Visibility.Collapsed;

                // Restaurar texto original
                if (btnLoader.Children.Count > 1 && btnLoader.Children[1] is TextBlock txtLoader)
                {
                    txtLoader.Text = "Verificando...";
                }

                Cursor = Cursors.Arrow;
            }
        }

        #endregion
    }
}