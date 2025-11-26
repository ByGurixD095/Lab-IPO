using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace TPVComida
{
    public partial class LoginView : Window
    {
        // Colores para feedback visual
        private readonly Brush _defaultBorder = Brushes.Transparent;
        private readonly Brush _focusBorder = (Brush)new BrushConverter().ConvertFrom("#00B0FF");
        private readonly Brush _errorBorder = (Brush)new BrushConverter().ConvertFrom("#D32F2F");

        public LoginView()
        {
            InitializeComponent();
        }

        // --- GESTIÓN DE VENTANA ---

        private void Border_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        // --- IPO: AVISO BLOQ MAYÚS Y FOCO ---

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckCapsLock();
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            CheckCapsLock();
        }

        private void Input_StateChanged(object sender, RoutedEventArgs e)
        {
            CheckCapsLock();
        }

        private void CheckCapsLock()
        {
            bool isCapsLock = Keyboard.IsKeyToggled(Key.CapsLock);

            // Lógica Usuario: Solo visible si Mayús activado Y el foco está en txtUser
            if (badgeCapsLockUser != null)
            {
                bool userHasFocus = txtUser.IsKeyboardFocused;
                badgeCapsLockUser.Visibility = (isCapsLock && userHasFocus) ? Visibility.Visible : Visibility.Collapsed;
            }

            // Lógica Password: Solo visible si Mayús activado Y el foco está en txtPass O txtPassVisible
            if (badgeCapsLockPass != null)
            {
                bool passHasFocus = txtPass.IsKeyboardFocused || txtPassVisible.IsKeyboardFocused;
                badgeCapsLockPass.Visibility = (isCapsLock && passHasFocus) ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        // --- LOGICA DE PLACEHOLDERS (Corrección Visual) ---

        private void txtUser_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Ocultar placeholder si hay texto
            lblUserPlaceholder.Visibility = (txtUser.Text.Length == 0) ? Visibility.Visible : Visibility.Collapsed;

            // Limpiamos errores visuales al escribir
            if (lblUserError.Visibility == Visibility.Visible)
            {
                lblUserError.Visibility = Visibility.Collapsed;
                txtUser.BorderBrush = _defaultBorder;
            }
        }

        private void txtPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            // Ocultar placeholder si hay contraseña
            lblPassPlaceholder.Visibility = (txtPass.Password.Length == 0) ? Visibility.Visible : Visibility.Collapsed;

            // Limpiamos errores visuales al escribir
            if (lblPassError.Visibility == Visibility.Visible)
            {
                lblPassError.Visibility = Visibility.Collapsed;
                txtPass.BorderBrush = _defaultBorder;
                txtPassVisible.BorderBrush = _defaultBorder;
            }
        }

        private void txtPassVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Ocultar placeholder si hay texto visible
            lblPassPlaceholder.Visibility = (txtPassVisible.Text.Length == 0) ? Visibility.Visible : Visibility.Collapsed;

            // Limpiamos errores visuales al escribir
            if (lblPassError.Visibility == Visibility.Visible)
            {
                lblPassError.Visibility = Visibility.Collapsed;
                txtPass.BorderBrush = _defaultBorder;
                txtPassVisible.BorderBrush = _defaultBorder;
            }
        }

        // --- LÓGICA DEL OJO (TOGGLE) ---

        private void BtnTogglePass_Click(object sender, RoutedEventArgs e)
        {
            if (txtPass.Visibility == Visibility.Visible)
            {
                // MOSTRAR: De Puntos -> Texto
                txtPassVisible.Text = txtPass.Password;
                txtPass.Visibility = Visibility.Collapsed;
                txtPassVisible.Visibility = Visibility.Visible;
                btnTogglePass.Content = "🚫";

                // Asegurar que el placeholder respete el texto copiado
                lblPassPlaceholder.Visibility = (txtPassVisible.Text.Length == 0) ? Visibility.Visible : Visibility.Collapsed;

                // Forzar foco para que el badge de mayúsculas se actualice correctamente
                txtPassVisible.Focus();
            }
            else
            {
                // OCULTAR: De Texto -> Puntos
                txtPass.Password = txtPassVisible.Text;
                txtPassVisible.Visibility = Visibility.Collapsed;
                txtPass.Visibility = Visibility.Visible;
                btnTogglePass.Content = "👁️";

                // Asegurar que el placeholder respete el password copiado
                lblPassPlaceholder.Visibility = (txtPass.Password.Length == 0) ? Visibility.Visible : Visibility.Collapsed;

                // Forzar foco para que el badge de mayúsculas se actualice correctamente
                txtPass.Focus();
            }
        }

        // --- LOGIN CLICK (VALIDACIÓN SOLO AL PULSAR) ---

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            // 1. Sincronizar contraseña final dependiendo de qué caja esté visible
            string finalPass = (txtPass.Visibility == Visibility.Visible) ? txtPass.Password : txtPassVisible.Text;

            // 2. Validación
            bool userValid = !string.IsNullOrWhiteSpace(txtUser.Text);
            bool passValid = finalPass.Length >= 4;

            if (!userValid)
            {
                lblUserError.Visibility = Visibility.Visible;
                txtUser.BorderBrush = _errorBorder;
                txtUser.Focus();
                return;
            }

            if (!passValid)
            {
                lblPassError.Visibility = Visibility.Visible;
                txtPass.BorderBrush = _errorBorder;
                txtPassVisible.BorderBrush = _errorBorder;

                // Enfocar la caja que esté visible
                if (txtPass.Visibility == Visibility.Visible) txtPass.Focus();
                else txtPassVisible.Focus();

                return;
            }

            // 3. Estado LOADING
            SetLoadingState(true);

            await Task.Delay(2000); // Simular BBDD

            SetLoadingState(false);

            MessageBox.Show($"Bienvenido, {txtUser.Text}", "Sesión Iniciada", MessageBoxButton.OK, MessageBoxImage.Information);
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
                Cursor = Cursors.Arrow;
            }
        }
    }
}