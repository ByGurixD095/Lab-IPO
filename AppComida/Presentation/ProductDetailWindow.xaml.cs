using AppComida.Domain;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging; // Necesario para BitmapImage

namespace AppComida.Presentation
{
    public partial class ProductDetailWindow : Window
    {
        private bool _isEditMode = false;
        private bool _isCreationMode = false;
        private Product _productCurrent;

        public bool ActionEdit { get; private set; } = false;
        public bool ActionDelete { get; private set; } = false;
        public Product ProductResult { get; private set; }

        // CONSTRUCTOR 1: Ver/Editar
        public ProductDetailWindow(Product product)
        {
            InitializeComponent();
            _productCurrent = product;
            _isCreationMode = false;
            LoadData();
        }

        // CONSTRUCTOR 2: Crear
        public ProductDetailWindow(bool isNewProduct)
        {
            InitializeComponent();
            _isCreationMode = isNewProduct;
            if (_isCreationMode) SetupCreationMode();
        }

        private void SetupCreationMode()
        {
            this.Title = "Nuevo Producto";

            if (txtMain != null) txtMain.Text = "Crear Producto";

            if (iconMain != null && Application.Current.Resources.Contains("IconSave"))
            {
                iconMain.Data = (Geometry)Application.Current.Resources["IconSave"];
            }

            ToggleEditMode(true);

            // Limpiar campos
            InputName.Text = "";
            InputPrice.Text = "";
            InputIngredientes.Text = "";
            ComboCategory.SelectedIndex = 0;

            // [CORRECCIÓN] Cargar imagen por defecto en modo creación
            InputImage.Text = "/Assets/Images/default_food.png"; // Ruta placeholder
            CargarImagen(InputImage.Text);
        }

        private void LoadData()
        {
            if (_productCurrent == null) return;

            // Datos Lectura
            TxtNombre.Text = _productCurrent.Name;
            TxtPrecio.Text = _productCurrent.Price.ToString("F2");
            TxtIngredientes.Text = _productCurrent.Ingredients;
            if (TxtCategoria != null) TxtCategoria.Text = _productCurrent.Category;

            // Datos Edición
            InputName.Text = _productCurrent.Name;
            InputPrice.Text = _productCurrent.Price.ToString();
            InputIngredientes.Text = _productCurrent.Ingredients;
            InputImage.Text = _productCurrent.ImagePath; // Rellenar input URL oculto

            // [CORRECCIÓN] CARGAR LA IMAGEN VISUALMENTE
            CargarImagen(_productCurrent.ImagePath);

            string[] alergenosMock = { "Gluten", "Lácteos" };
            ListAlergenos.ItemsSource = alergenosMock;
        }

        // [NUEVO MÉTODO] Lógica segura para cargar imágenes
        private void CargarImagen(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                // Detectar si es web (http) o local
                Uri imageUri;
                if (path.StartsWith("http"))
                {
                    imageUri = new Uri(path, UriKind.Absolute);
                }
                else
                {
                    // Asumimos ruta relativa local
                    imageUri = new Uri(path, UriKind.Relative);
                }

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = imageUri;
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Importante para liberar fichero
                bitmap.EndInit();

                ImgProducto.Source = bitmap;
            }
            catch (Exception)
            {
                // Si falla (ej: ruta no existe), no hacemos nada o ponemos una por defecto
                // ImgProducto.Source = null; 
            }
        }

        private void BtnMainAction_Click(object sender, RoutedEventArgs e)
        {
            if (_isCreationMode)
            {
                ProductResult = new Product
                {
                    Name = InputName.Text,
                    Price = double.TryParse(InputPrice.Text, out double p) ? p : 0,
                    Ingredients = InputIngredientes.Text,
                    Category = (ComboCategory.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Platos",
                    SubCategory = "Todo",
                    IsAvailable = true,
                    // Usamos lo que haya escrito en el input oculto de imagen
                    ImagePath = string.IsNullOrEmpty(InputImage.Text) ? "/Assets/Images/default_food.png" : InputImage.Text
                };

                var confirm = new ConfirmWindow(
                    "El producto se ha creado correctamente y añadido al catálogo.",
                    "Producto Creado",
                    ConfirmType.Success);
                confirm.Owner = this;
                confirm.ShowDialog();

                ActionEdit = true;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                if (!_isEditMode)
                {
                    ToggleEditMode(true);
                }
                else
                {
                    var confirm = new ConfirmWindow(
                        "¿Deseas guardar los cambios realizados?",
                        "Guardar Cambios",
                        ConfirmType.Question);
                    confirm.Owner = this;

                    if (confirm.ShowDialog() == true)
                    {
                        _productCurrent.Name = InputName.Text;
                        _productCurrent.Price = double.TryParse(InputPrice.Text, out double p) ? p : 0;
                        _productCurrent.Ingredients = InputIngredientes.Text;
                        _productCurrent.ImagePath = InputImage.Text; // Guardar nueva imagen si cambió

                        // [CORRECCIÓN] Actualizar la imagen visualmente al guardar
                        CargarImagen(_productCurrent.ImagePath);

                        ActionEdit = true;
                        ToggleEditMode(false);
                        LoadData();
                    }
                }
            }
        }

        private void ToggleEditMode(bool enableEdit)
        {
            _isEditMode = enableEdit;

            Visibility viewVis = enableEdit ? Visibility.Collapsed : Visibility.Visible;
            Visibility editVis = enableEdit ? Visibility.Visible : Visibility.Collapsed;

            TxtNombre.Visibility = viewVis;
            TxtPrecio.Visibility = viewVis;
            PillCategoria.Visibility = viewVis;
            TxtIngredientes.Visibility = viewVis;
            ListAlergenos.Visibility = viewVis;

            InputName.Visibility = editVis;
            PanelEditPrice.Visibility = editVis;
            PanelEditCategoria.Visibility = editVis;
            InputIngredientes.Visibility = editVis;
            InputAlergenos.Visibility = editVis;
            PanelEditImage.Visibility = editVis; // El input de URL de imagen se muestra al editar

            if (!_isCreationMode)
            {
                if (txtMain != null)
                {
                    txtMain.Text = enableEdit ? "Guardar Cambios" : "Editar Producto";
                }

                if (iconMain != null)
                {
                    string key = enableEdit ? "IconSave" : "IconPencil";
                    if (Application.Current.Resources.Contains(key))
                        iconMain.Data = (Geometry)Application.Current.Resources[key];
                }
            }
        }

        private void BtnSecondaryAction_Click(object sender, RoutedEventArgs e)
        {
            if (_isCreationMode)
            {
                this.Close();
            }
            else if (_isEditMode)
            {
                // Al cancelar edición, recargamos la imagen original por si la cambiaron en el input
                LoadData();
                ToggleEditMode(false);
            }
            else
            {
                var confirm = new ConfirmWindow(
                    $"¿Seguro que quieres eliminar '{_productCurrent?.Name}'?\nEsta acción no se puede deshacer.",
                    "Eliminar Producto",
                    ConfirmType.Danger);
                confirm.Owner = this;

                if (confirm.ShowDialog() == true)
                {
                    ActionDelete = true;
                    this.Close();
                }
            }
        }

        private void BtnCerrar_Click(object sender, RoutedEventArgs e) => this.Close();

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) this.DragMove();
        }
    }
}