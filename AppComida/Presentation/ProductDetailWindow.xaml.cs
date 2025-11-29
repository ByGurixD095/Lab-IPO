using AppComida.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes; // Necesario para los iconos

namespace AppComida.Presentation
{
    public partial class ProductDetailWindow : Window
    {
        private Product _producto;
        private bool _isEditMode = false;

        // Propiedades para comunicar al padre qué hacer
        public bool ActionDelete { get; private set; } = false;
        public bool ActionEdit { get; private set; } = false;

        public ProductDetailWindow(Product producto)
        {
            InitializeComponent();
            _producto = producto;
            CargarDatos();
        }

        private void CargarDatos()
        {
            if (_producto == null) return;

            // 1. Textos (Modo Lectura)
            TxtNombre.Text = _producto.Name;
            TxtPrecio.Text = $"{_producto.Price:0.00} €";
            TxtIngredientes.Text = string.IsNullOrEmpty(_producto.Ingredients) ? "Sin información de ingredientes." : _producto.Ingredients;
            TxtCategoria.Text = string.IsNullOrEmpty(_producto.SubCategory) ? _producto.Category : _producto.SubCategory;

            // 2. Imagen
            if (!string.IsNullOrEmpty(_producto.ImagePath))
            {
                CargarImagenSegura(_producto.ImagePath);
            }
            else
            {
                ImgProducto.Source = null;
            }

            // 3. Alérgenos
            RefreshAllergensList();
        }

        private void RefreshAllergensList()
        {
            if (!string.IsNullOrEmpty(_producto.Allergens))
            {
                var listaAlergenos = _producto.Allergens
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim())
                    .ToList();
                ListAlergenos.ItemsSource = listaAlergenos;
            }
            else
            {
                ListAlergenos.ItemsSource = new List<string> { "Sin alérgenos registrados" };
            }
        }

        private void CargarImagenSegura(string ruta)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ruta)) return;
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;

                if (ruta.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    bitmap.UriSource = new Uri(ruta, UriKind.Absolute);
                }
                else
                {
                    string cleanPath = ruta.Replace('\\', '/');

                    // CORRECCIÓN 1: Usamos System.IO.Path explícitamente para evitar confusión con el Path de dibujo
                    if (System.IO.Path.IsPathRooted(cleanPath) && File.Exists(cleanPath))
                    {
                        bitmap.UriSource = new Uri(cleanPath, UriKind.Absolute);
                    }
                    else
                    {
                        if (!cleanPath.StartsWith("/")) cleanPath = "/" + cleanPath;
                        bitmap.UriSource = new Uri($"pack://application:,,,{cleanPath}", UriKind.Absolute);
                    }
                }
                bitmap.EndInit();
                ImgProducto.Source = bitmap;
            }
            catch
            {
                // Fallback local
                try
                {
                    if (!ruta.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
                        // CORRECCIÓN 2: System.IO.Path explícito
                        string rutaFisica = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ruta.Replace('/', '\\').TrimStart('\\'));
                        if (File.Exists(rutaFisica))
                        {
                            var bmp = new BitmapImage();
                            bmp.BeginInit();
                            bmp.CacheOption = BitmapCacheOption.OnLoad;
                            bmp.UriSource = new Uri(rutaFisica, UriKind.Absolute);
                            bmp.EndInit();
                            ImgProducto.Source = bmp;
                        }
                    }
                }
                catch { }
            }
        }


        // --- LÓGICA DE EDICIÓN (TOGGLE) ---

        private void BtnMainAction_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditMode)
            {
                EnterEditMode();
            }
            else
            {
                SaveChanges();
            }
        }

        private void BtnSecondaryAction_Click(object sender, RoutedEventArgs e)
        {
            if (!_isEditMode)
            {
                ConfirmWindow confirm = new ConfirmWindow(
                    "¿Estás seguro de que deseas eliminar este producto?\nNo podrás deshacer esta acción.",
                    "Eliminar Producto",
                    ConfirmType.Danger);
                confirm.Owner = this;

                if (confirm.ShowDialog() == true)
                {
                    ActionDelete = true;
                    this.DialogResult = true;
                    this.Close();
                }
            }
            else
            {
                ExitEditMode();
            }
        }

        private void EnterEditMode()
        {
            _isEditMode = true;

            InputName.Text = _producto.Name;
            InputPrice.Text = _producto.Price.ToString();
            InputIngredientes.Text = _producto.Ingredients;
            InputAlergenos.Text = _producto.Allergens;
            InputSubCategory.Text = _producto.SubCategory;
            InputImage.Text = _producto.ImagePath;

            foreach (ComboBoxItem item in ComboCategory.Items)
            {
                if (item.Content.ToString() == _producto.Category)
                {
                    ComboCategory.SelectedItem = item;
                    break;
                }
            }

            ToggleVisibility(Visibility.Collapsed, Visibility.Visible);
            UpdateButtonsState(true);
        }

        private void SaveChanges()
        {
            if (string.IsNullOrWhiteSpace(InputName.Text))
            {
                MessageBox.Show("El nombre no puede estar vacío.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!double.TryParse(InputPrice.Text, out double newPrice))
            {
                MessageBox.Show("El precio debe ser un número válido.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _producto.Name = InputName.Text;
            _producto.Price = newPrice;
            _producto.Ingredients = InputIngredientes.Text;
            _producto.Allergens = InputAlergenos.Text;
            _producto.SubCategory = InputSubCategory.Text;
            _producto.Category = (ComboCategory.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Platos";
            _producto.ImagePath = InputImage.Text;

            ActionEdit = true;
            CargarDatos();
            ExitEditMode();

            // Opcional: Cerrar para refrescar el padre inmediatamente
            this.DialogResult = true;
            this.Close();
        }

        private void ExitEditMode()
        {
            _isEditMode = false;
            ToggleVisibility(Visibility.Visible, Visibility.Collapsed);
            UpdateButtonsState(false);
        }

        private void ToggleVisibility(Visibility readMode, Visibility editMode)
        {
            TxtNombre.Visibility = readMode;
            TxtPrecio.Visibility = readMode;
            TxtIngredientes.Visibility = readMode;
            ListAlergenos.Visibility = readMode;
            PillCategoria.Visibility = readMode;

            InputName.Visibility = editMode;
            PanelEditPrice.Visibility = editMode;
            InputIngredientes.Visibility = editMode;
            InputAlergenos.Visibility = editMode;
            PanelEditCategoria.Visibility = editMode;
            PanelEditImage.Visibility = editMode;
        }

        private void UpdateButtonsState(bool isEditing)
        {
            if (isEditing)
            {
                SetButtonContent(BtnMainAction, "Guardar Cambios", "IconSave", "#4CAF50");
                SetButtonContent(BtnSecondaryAction, "", "IconCancel", "#9E9E9E");
                BtnSecondaryAction.ToolTip = "Cancelar edición";
            }
            else
            {
                SetButtonContent(BtnMainAction, "Editar Producto", "IconPencil", "#FF6F00");
                SetButtonContent(BtnSecondaryAction, "", "IconTrash", "#D32F2F");
                BtnSecondaryAction.ToolTip = "Eliminar permanentemente";
            }
        }

        private void SetButtonContent(Button btn, string text, string iconResourceKey, string colorHex)
        {
            var border = btn.Template.FindName("bdr", btn) as Border;
            if (border != null) border.Background = (Brush)new BrushConverter().ConvertFrom(colorHex);

            var txtBlock = btn.Template.FindName("txt", btn) as TextBlock;
            if (txtBlock != null) txtBlock.Text = text;

            // CORRECCIÓN 3: Especificamos que es un Path de dibujo (System.Windows.Shapes)
            var path = btn.Template.FindName("icon", btn) as System.Windows.Shapes.Path;
            if (path != null) path.Data = (Geometry)FindResource(iconResourceKey);
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }
    }
}