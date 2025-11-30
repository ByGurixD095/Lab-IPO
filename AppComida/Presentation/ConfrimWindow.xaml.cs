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
            string pathData = "";
            string colorHex = "#FF6F00";

            switch (type)
            {
                case ConfirmType.Question:
                    pathData = "M13,14H11V10H13M13,18H11V16H13M1,21H23L12,2L1,21Z";
                    colorHex = "#FF6F00";
                    break;
                case ConfirmType.Warning:
                    pathData = "M12,2L1,21H23M12,6L19.53,19H4.47M11,10V14H13V10M11,16V18H13V16";
                    colorHex = "#FFA000";
                    break;
                case ConfirmType.Danger:
                    pathData = "M13,13H11V7H13M13,17H11V15H13M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z";
                    colorHex = "#D32F2F";
                    break;
                case ConfirmType.Info:
                    pathData = "M11,9H13V7H11M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M11,17H13V11H11V17Z";
                    colorHex = "#0288D1";
                    break;
                case ConfirmType.Success:
                    pathData = "M12 2C6.5 2 2 6.5 2 12S6.5 22 12 22 22 17.5 22 12 17.5 2 12 2M10 17L5 12L6.41 10.59L10 14.17L17.59 6.58L19 8L10 17Z";
                    colorHex = "#388E3C";
                    break;
            }
            try
            {
                if (this.FindName("IconPath") is System.Windows.Shapes.Path iconPath)
                {
                    iconPath.Data = Geometry.Parse(pathData);
                    iconPath.Fill = (SolidColorBrush)new BrushConverter().ConvertFrom(colorHex);
                }
            }
            catch (Exception) {}
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