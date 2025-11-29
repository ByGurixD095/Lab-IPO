using AppComida.Domain;
using AppComida.Presentation;
using System;
using System.Threading.Tasks; // Necesario para Task.Delay
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace AppComida.Presentation
{
    public partial class LoginWindow : Window
    {
        // Colores
        private readonly Brush _defaultBorder = Brushes.Transparent;
        private readonly Brush _errorBorder = (Brush)new BrushConverter().ConvertFrom("#D32F2F");
        private LoginController _logController;

        public LoginWindow()
        {
            InitializeComponent();
            _logController = new LoginController();
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

        // --- INPUT & EVENTOS ---

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            CheckCapsLock();
            txtUser.Focus();
        }

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

        private void Input_StateChanged(object sender, RoutedEventArgs e)
        {
            CheckCapsLock();
        }

        private void CheckCapsLock()
        {
            bool isCapsLock = Keyboard.IsKeyToggled(Key.CapsLock);

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

        // --- LÓGICA DE PLACEHOLDERS Y ERRORES ---

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

            if (lblPassPlaceholder.Text != "Introduce tu clave")
            {
                lblPassPlaceholder.Text = "Introduce tu clave";
                lblPassPlaceholder.Foreground = (Brush)FindResource("PlaceholderColor");
            }
        }

        // --- LÓGICA DEL OJO (TOGGLE) ---

        private void BtnTogglePass_Click(object sender, RoutedEventArgs e)
        {
            if (txtPass.Visibility == Visibility.Visible)
            {
                txtPassVisible.Text = txtPass.Password;
                txtPass.Visibility = Visibility.Collapsed;
                txtPassVisible.Visibility = Visibility.Visible;
                btnTogglePass.Content = "🚫";
                lblPassPlaceholder.Visibility = (txtPassVisible.Text.Length == 0) ? Visibility.Visible : Visibility.Collapsed;
                txtPassVisible.Focus();
            }
            else
            {
                txtPass.Password = txtPassVisible.Text;
                txtPassVisible.Visibility = Visibility.Collapsed;
                txtPass.Visibility = Visibility.Visible;
                btnTogglePass.Content = "👁️";
                lblPassPlaceholder.Visibility = (txtPass.Password.Length == 0) ? Visibility.Visible : Visibility.Collapsed;
                txtPass.Focus();
            }
        }

        // --- LOGIN CLICK (LÓGICA NUEVA DE PRE-CARGA) ---

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string finalPass = (txtPass.Visibility == Visibility.Visible) ? txtPass.Password : txtPassVisible.Text;
            string username = txtUser.Text.Trim();

            ClearErrors();

            // 1. Validación de campos vacíos (Rápida)
            bool userValid = !string.IsNullOrWhiteSpace(username);
            bool passValid = finalPass.Length >= 1;

            if (!userValid) { ShowInputError(true); return; }
            if (!passValid) { ShowInputError(false); return; }

            // 2. Activar Estado de "Verificando..."
            SetLoadingState(true);

            // Pequeña pausa (50ms) para asegurar que la UI se renderiza antes de seguir
            await Task.Delay(50);

            // 3. Validación de Credenciales
            // Usamos Task.Run para no bloquear la UI durante la verificación
            User userLogged = null;
            await Task.Run(() => {
                userLogged = _logController.ValidateLogin(username, finalPass);
            });

            if (userLogged != null)
            {
                // --- ÉXITO: INICIAMOS LA FALSA ILUSIÓN ---

                // A. Cambiamos el texto para dar feedback de progreso
                if (btnLoader.Children.Count > 1 && btnLoader.Children[1] is TextBlock txtLoader)
                {
                    txtLoader.Text = "Cargando sistema...";
                }

                // Permitimos que la UI se actualice con el nuevo texto
                await Task.Delay(50);

                // B. Instanciamos MainWindow (CARGA PESADA REAL)
                // Al hacer el 'new', se ejecuta 'InicializarVistas' en MainWindow, 
                // lo que carga Productos, Clientes y Pedidos desde los XMLs.
                // El spinner seguirá girando mientras el procesador trabaja aquí.
                MainWindow main = new MainWindow(userLogged);

                // C. Cuando el constructor termina, todo está listo. Mostramos.
                main.Show();
                this.Close();
            }
            else
            {
                // ERROR DE CREDENCIALES
                SetLoadingState(false);
                ShowGenericLoginError();
            }
        }

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
                if (txtPass.Visibility == Visibility.Visible) txtPass.Focus(); else txtPassVisible.Focus();
            }
        }

        private void ShowGenericLoginError()
        {
            txtPass.Password = "";
            txtPassVisible.Text = "";

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

                // Restaurar texto original por si hubo error tras "Cargando sistema..."
                if (btnLoader.Children.Count > 1 && btnLoader.Children[1] is TextBlock txtLoader)
                {
                    txtLoader.Text = "Verificando...";
                }

                Cursor = Cursors.Arrow;
            }
        }
    }
}