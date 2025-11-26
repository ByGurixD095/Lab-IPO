using AppComida;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Contexts;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

namespace AppComida.Presentation
{
    public class UserData
    {
        public string username { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public DateTime last_access { get; set; }
        public string image { get; set; }
        public string salt { get; set; }
        public string digest { get; set; }
    }

    public partial class LoginView : Window
    {
        // Colores
        private readonly Brush _defaultBorder = Brushes.Transparent;
        private readonly Brush _errorBorder = (Brush)new BrushConverter().ConvertFrom("#D32F2F");

        // Diccionario de usuarios
        private Dictionary<string, UserData> _usersDb;

        public LoginView()
        {
            InitializeComponent();
            LoadUsers();
        }

        // --- CARGA DE DATOS XML ---
        private void LoadUsers()
        {
            _usersDb = new Dictionary<string, UserData>();
            string finalPath = null;
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;

            for (int i = 0; i < 6; i++)
            {
                string potentialPath = Path.Combine(currentDir, "data", "users.xml");
                if (File.Exists(potentialPath))
                {
                    finalPath = potentialPath;
                    break;
                }
                DirectoryInfo parentDir = Directory.GetParent(currentDir);
                if (parentDir == null) break;
                currentDir = parentDir.FullName;
            }

            try
            {
                if (finalPath != null && File.Exists(finalPath))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<UserData>));
                    using (StreamReader reader = new StreamReader(finalPath))
                    {
                        List<UserData> usersList = (List<UserData>)serializer.Deserialize(reader);
                        if (usersList != null)
                        {
                            foreach (var u in usersList)
                            {
                                if (!string.IsNullOrEmpty(u.username) && !_usersDb.ContainsKey(u.username))
                                {
                                    _usersDb.Add(u.username, u);
                                }
                            }
                        }
                    }
                }
                else
                {
                    MessageBox.Show("No se ha podido localizar el archivo 'data/users.xml'", "Error de Datos", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error leyendo XML: {ex.Message}", "Error Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            ClearErrors(); // Importante: Aquí se resetea el placeholder rojo si el usuario escribe
        }

        private void txtPassVisible_TextChanged(object sender, TextChangedEventArgs e)
        {
            lblPassPlaceholder.Visibility = (txtPassVisible.Text.Length == 0) ? Visibility.Visible : Visibility.Collapsed;
            ClearErrors();
        }

        private void ClearErrors()
        {
            // 1. Limpiar bordes rojos de Usuario
            if (lblUserError.Visibility == Visibility.Visible || txtUser.BorderBrush == _errorBorder)
            {
                lblUserError.Visibility = Visibility.Collapsed;
                txtUser.BorderBrush = _defaultBorder;
            }

            // 2. Limpiar bordes rojos de Contraseña
            if (lblPassError.Visibility == Visibility.Visible || txtPass.BorderBrush == _errorBorder)
            {
                lblPassError.Visibility = Visibility.Collapsed;
                txtPass.BorderBrush = _defaultBorder;
                txtPassVisible.BorderBrush = _defaultBorder;
            }

            // 3. RESETEAR EL PLACEHOLDER DE ERROR A NORMAL
            // Si el texto del placeholder ha cambiado, lo devolvemos al original y gris
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

        // --- VALIDACIÓN CRIPTO ---

        private bool ValidateCredentials(string username, string password, out UserData userFound)
        {
            userFound = null;
            if (_usersDb == null || !_usersDb.ContainsKey(username)) return false;

            UserData storedUser = _usersDb[username];
            string salt = storedUser.salt;
            string storedDigest = storedUser.digest;
            string calculatedDigest = CalculateMD5(password + salt);

            if (calculatedDigest.Trim().Equals(storedDigest.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                userFound = storedUser;
                return true;
            }
            return false;
        }

        private string CalculateMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }
                return sb.ToString();
            }
        }

        // --- LOGIN CLICK ---

        private async void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            string finalPass = (txtPass.Visibility == Visibility.Visible) ? txtPass.Password : txtPassVisible.Text;
            string username = txtUser.Text.Trim();

            // Reseteamos cualquier error visual previo
            ClearErrors();

            // 1. Validación UI (Campos vacíos)
            bool userValid = !string.IsNullOrWhiteSpace(username);
            bool passValid = finalPass.Length >= 1;

            if (!userValid)
            {
                ShowInputError(true);
                return;
            }
            if (!passValid)
            {
                ShowInputError(false);
                return;
            }

            // 2. Loading
            SetLoadingState(true);
            await Task.Delay(500);

            // 3. Validación Real
            UserData userLogged;
            bool isAuthenticated = ValidateCredentials(username, finalPass, out userLogged);

            SetLoadingState(false);

            if (isAuthenticated)
            {
                MainWindow main = new MainWindow();
                main.Show();
                this.Close();
            }
            else
            {
                // ERROR DE CREDENCIALES
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

        // AQUÍ ESTÁ EL CAMBIO CLAVE: Error en el Placeholder
        private void ShowGenericLoginError()
        {
            // 1. Limpiar campos de contraseña (esto hace visible el placeholder)
            txtPass.Password = "";
            txtPassVisible.Text = "";

            // 2. CAMBIAR el texto del Placeholder a Error y ponerlo ROJO
            lblPassPlaceholder.Text = "Usuario o contraseña incorrectos";
            lblPassPlaceholder.Foreground = (Brush)FindResource("ErrorColor"); // Rojo definido en XAML
            lblPassPlaceholder.Visibility = Visibility.Visible;

            // 3. Poner bordes rojos en ambos inputs (seguridad)
            txtUser.BorderBrush = _errorBorder;
            txtPass.BorderBrush = _errorBorder;
            txtPassVisible.BorderBrush = _errorBorder;

            // 4. Dar foco a la contraseña
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
                Cursor = Cursors.Arrow;
            }
        }
    }
}