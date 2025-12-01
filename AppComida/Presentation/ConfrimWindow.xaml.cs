using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace AppComida.Presentation
{
    public enum ConfirmType
    {
        Question,
        Warning,
        Danger,
        Info,
        Success
    }

    public partial class ConfirmWindow : Window
    {
        public ConfirmWindow(string message, string title = "Confirmación", ConfirmType type = ConfirmType.Question)
        {
            InitializeComponent();
            TxtMessage.Text = message;
            TxtTitle.Text = title;
            ApplyTheme(type);
        }

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
                    colorHex = "#D32F2F"; // Rojo
                    break;
                case ConfirmType.Info:
                    iconResourceKey = "IconInfo";
                    colorHex = "#0288D1"; // Azul
                    break;
                case ConfirmType.Success:
                    iconResourceKey = "IconSuccess";
                    colorHex = "#388E3C"; // Verde
                    break;
            }

            try
            {
                if (Application.Current.Resources.Contains(iconResourceKey))
                {
                    IconPath.Data = (Geometry)Application.Current.Resources[iconResourceKey];
                }

                // 2. Aplicar el color
                IconPath.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(colorHex);
            }
            catch (Exception)
            {

            }
        }

        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
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
    }
}