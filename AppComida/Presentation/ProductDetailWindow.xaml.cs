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
using System.Windows.Shapes;

namespace AppComida.Presentation
{
    public partial class ProductDetailWindow : Window
    {
        private Product _producto;
        private bool _isEditMode = false;

        // Flags para que la ventana padre sepa si se ha tocado algo
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

            // Pongo los datos en los TextBlocks
            TxtNombre.Text = _producto.Name;
            TxtPrecio.Text = $"{_producto.Price:0.00} €";
            TxtIngredientes.Text = string.IsNullOrEmpty(_producto.Ingredients) ? "Sin información de ingredientes." : _producto.Ingredients;
            TxtCategoria.Text = string.IsNullOrEmpty(_producto.SubCategory) ? _producto.Category : _producto.SubCategory;

            // Intento cargar la imagen si tiene ruta
            if (!string.IsNullOrEmpty(_producto.ImagePath))
            {
                CargarImagenSegura(_producto.ImagePath);
            }
            else
            {
                ImgProducto.Source = null;
            }

            // Muestro la lista de alérgenos bonita
            RefreshAllergensList();
        }

        private void RefreshAllergensList()
        {
            if (!string.IsNullOrEmpty(_producto.Allergens))
            {
                // Separo por comas o punto y coma por si acaso
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

        // Método tocho para cargar imágenes evitando bloqueos y errores de rutas
        private void CargarImagenSegura(string ruta)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ruta)) return;
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Para que no bloquee el fichero

                if (ruta.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                {
                    bitmap.UriSource = new Uri(ruta, UriKind.Absolute);
                }
                else
                {
                    // Apaño para las barras de directorios
                    string cleanPath = ruta.Replace('\\', '/');
                    if (System.IO.Path.IsPathRooted(cleanPath) && File.Exists(cleanPath))
                    {
                        bitmap.UriSource = new Uri(cleanPath, UriKind.Absolute);
                    }
                    else
                    {
                        // Si es ruta relativa dentro del proyecto
                        if (!cleanPath.StartsWith("/")) cleanPath = "/" + cleanPath;
                        bitmap.UriSource = new Uri($"pack://application:,,,{cleanPath}", UriKind.Absolute);
                    }
                }
                bitmap.EndInit();
                ImgProducto.Source = bitmap;
            }
            catch
            {
                // Si falla lo de arriba, intento buscarlo en local a lo bruto
                // Aqui no pongo ventana de error porque si falla al cargar la ventana queda feo que salten popups
                try
                {
                    if (!ruta.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    {
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
                catch { } // Si falla aquí ya me rindo
            }
        }


        // Botón principal
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

        // Botón secundario: borrar o cancelar edición
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

            //inputs con lo que ya tiene el producto
            InputName.Text = _producto.Name;
            InputPrice.Text = _producto.Price.ToString();
            InputIngredientes.Text = _producto.Ingredients;
            InputAlergenos.Text = _producto.Allergens;
            InputSubCategory.Text = _producto.SubCategory;
            InputImage.Text = _producto.ImagePath;

            // categoría correcta en el combo
            foreach (ComboBoxItem item in ComboCategory.Items)
            {
                if (item.Content.ToString() == _producto.Category)
                {
                    ComboCategory.SelectedItem = item;
                    break;
                }
            }

            //visibilidad de paneles
            ToggleVisibility(Visibility.Collapsed, Visibility.Visible);
            UpdateButtonsState(true);
        }

        private void SaveChanges()
        {
            // Validaciones básicas con la ventana de Aviso (Warning)
            if (string.IsNullOrWhiteSpace(InputName.Text))
            {
                var alert = new ConfirmWindow("El nombre no puede estar vacío.", "Faltan datos", ConfirmType.Warning);
                alert.Owner = this;
                alert.ShowDialog();
                return;
            }

            // Si el precio no es numero, otro aviso
            if (!double.TryParse(InputPrice.Text, out double newPrice))
            {
                var alert = new ConfirmWindow("El precio tiene que ser un número válido.", "Error de formato", ConfirmType.Warning);
                alert.Owner = this;
                alert.ShowDialog();
                return;
            }

            try
            {
                // Actualizar producto
                _producto.Name = InputName.Text;
                _producto.Price = newPrice;
                _producto.Ingredients = InputIngredientes.Text;
                _producto.Allergens = InputAlergenos.Text;
                _producto.SubCategory = InputSubCategory.Text;
                _producto.Category = (ComboCategory.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Platos";
                _producto.ImagePath = InputImage.Text;

                // Marco que hemos editado para que el padre actualice la lista
                ActionEdit = true;
                CargarDatos();
                ExitEditMode();

                // Ventana de éxito.
                var success = new ConfirmWindow("El producto se ha guardado correctamente.", "Operación Exitosa", ConfirmType.Success);
                success.Owner = this; // Esto hace que salga centrada sobre esta ventana
                success.ShowDialog();

                // Ahora sí, cerramos el chiringuito devolviendo true
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                // Si explota algo raro, saco la ventana roja
                var error = new ConfirmWindow("Ha ocurrido un error al guardar: " + ex.Message, "Error Crítico", ConfirmType.Danger);
                error.Owner = this;
                error.ShowDialog();
            }
        }

        private void ExitEditMode()
        {
            _isEditMode = false;
            ToggleVisibility(Visibility.Visible, Visibility.Collapsed);
            UpdateButtonsState(false);
        }

        // Helper para mostrar/ocultar cosas rápido
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

        // Cambio colores e iconos de los botones dinámicamente
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

        // Método auxiliar para no repetir código cambiando estilos de botón
        private void SetButtonContent(Button btn, string text, string iconResourceKey, string colorHex)
        {
            var border = btn.Template.FindName("bdr", btn) as Border;
            if (border != null) border.Background = (Brush)new BrushConverter().ConvertFrom(colorHex);

            var txtBlock = btn.Template.FindName("txt", btn) as TextBlock;
            if (txtBlock != null) txtBlock.Text = text;

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