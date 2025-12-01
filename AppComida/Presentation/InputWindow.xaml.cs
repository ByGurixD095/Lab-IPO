using System.Windows;

namespace AppComida.Presentation
{
    public partial class InputWindow : Window
    {
        public string ResponseText { get; private set; }

        public InputWindow(string message, string title, string defaultValue = "")
        {
            InitializeComponent();
            TxtTitle.Text = title;
            TxtMessage.Text = message;
            TxtInput.Text = defaultValue;

            Loaded += (s, e) => { TxtInput.Focus(); TxtInput.SelectAll(); };

            MouseLeftButtonDown += (s, e) => DragMove();
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            ResponseText = TxtInput.Text;
            DialogResult = true;
            Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}