using System.Windows;
using System.Windows.Input;

namespace AppComida.Presentation
{
    /// <summary>
    /// Ventana modal genérica para solicitar un dato de texto al usuario.
    /// Sustituye al InputBox para mantener la consistencia visual.
    /// </summary>
    public partial class InputWindow : Window
    {
        #region Propiedades Públicas

        // Propiedad de solo lectura externa para recuperar el valor introducido tras el cierre
        public string ResponseText { get; private set; }

        #endregion

        #region Constructor e Inicialización

        public InputWindow(string message, string title, string defaultValue = "")
        {
            InitializeComponent();

            // Configuración dinámica de textos según el contexto de llamada
            TxtTitle.Text = title;
            TxtMessage.Text = message;
            TxtInput.Text = defaultValue;

            // Suscripción a eventos de ciclo de vida
            this.Loaded += Window_Loaded;
            this.MouseLeftButtonDown += Window_MouseLeftButtonDown;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            TxtInput.Focus();
            TxtInput.SelectAll();
        }

        #endregion

        #region Eventos de Interfaz

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            ResponseText = TxtInput.Text;

            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        #endregion
    }
}