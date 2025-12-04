using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AppComida.Presentation
{
    /// <summary>
    /// Enumerado para tipificar los distintos estados de feedback.
    /// </summary>
    public enum ConfirmType
    {
        Question, // Naranja: Preguntas de sí/no
        Warning,  // Ámbar: Advertencias no bloqueantes
        Danger,   // Rojo: Acciones destructivas (borrar)
        Info,     // Azul: Información general
        Success   // Verde: Confirmación de éxito
    }

    /// <summary>
    /// Ventana modal personalizada para reemplazar el MessageBox nativo de Windows.
    /// Permite mantener la estética de la aplicación.
    /// </summary>
    public partial class ConfirmWindow : Window
    {
        #region Constructor e Inicialización

        public ConfirmWindow(string message, string title = "Confirmación", ConfirmType type = ConfirmType.Question)
        {
            InitializeComponent();

            // Inyección de contenido dinámico
            TxtMessage.Text = message;
            TxtTitle.Text = title;

            // Aplicamos el estilo visual según el tipo de mensaje
            ApplyTheme(type);
        }

        #endregion

        #region Lógica de Tematización

        private void ApplyTheme(ConfirmType type)
        {
            string iconResourceKey = "IconQuestion";
            string colorHex = "#FF6F00";

            switch (type)
            {
                case ConfirmType.Question:
                    iconResourceKey = "IconQuestion";
                    colorHex = "#FF6F00";
                    break;
                case ConfirmType.Warning:
                    iconResourceKey = "IconAlert";
                    colorHex = "#FFA000";
                    break;
                case ConfirmType.Danger:
                    iconResourceKey = "IconDanger";
                    colorHex = "#D32F2F";
                    break;
                case ConfirmType.Info:
                    iconResourceKey = "IconInfo";
                    colorHex = "#0288D1";
                    break;
                case ConfirmType.Success:
                    iconResourceKey = "IconSuccess";
                    colorHex = "#388E3C";
                    break;
            }

            try
            {
                //Cargamos icono
                if (Application.Current.Resources.Contains(iconResourceKey))
                {
                    IconPath.Data = (Geometry)Application.Current.Resources[iconResourceKey];
                }

                // Aplicamos el color
                IconPath.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(colorHex);
            }
            catch (Exception)
            {
                // No me apaetece tener que poner solucion a la ventana de avisos que no me da la vida para más
                // Además, si lo dejo así sin toquetear nada evito que la app crashee
            }
        }

        #endregion

        #region Eventos de Interfaz

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            // DialogResult devuelve true al padre (ShowDialog)
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                this.DragMove();
        }

        #endregion
    }
}