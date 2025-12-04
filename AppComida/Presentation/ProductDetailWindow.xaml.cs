using AppComida.Domain;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace AppComida.Presentation
{
    /// <summary>
    /// Ventana modal polimórfica: Sirve tanto para ver detalles, editar existentes o crear nuevos productos.
    /// Controla la visibilidad de los campos según el estado (_isEditMode).
    /// </summary>
    public partial class ProductDetailWindow : Window
    {
        #region Estado y Propiedades

        private bool _isEditMode = false;
        private bool _isCreationMode = false;
        private Product _productCurrent;

        // Propiedades para comunicar el resultado a la ventana padre
        public bool ActionEdit { get; private set; } = false;
        public bool ActionDelete { get; private set; } = false;
        public Product ProductResult { get; private set; }

        #endregion

        #region Constructores

        // Constructor A: Modo Ver/Editar producto existente
        public ProductDetailWindow(Product product)
        {
            InitializeComponent();
            _productCurrent = product;
            _isCreationMode = false;
            LoadData();
        }

        // Constructor B: Modo Crear nuevo producto
        public ProductDetailWindow(bool isNewProduct)
        {
            InitializeComponent();
            _isCreationMode = isNewProduct;
            if (_isCreationMode)
                SetupCreationMode();
        }

        #endregion

        #region Lógica de Visualización y Carga

        private void SetupCreationMode()
        {
            this.Title = "Alta de Nuevo Producto";

            if (txtMain != null) txtMain.Text = "Crear Producto";

            // Cambiamos el icono del botón principal
            if (iconMain != null && Application.Current.Resources.Contains("IconSave"))
            {
                iconMain.Data = (Geometry)Application.Current.Resources["IconSave"];
            }

            ToggleEditMode(true);

            // Reseteo de formulario
            InputName.Text = "";
            InputPrice.Text = "";
            InputIngredientes.Text = "";
            ComboCategory.SelectedIndex = 0;

            InputImage.Text = "/Assets/Images/default_food.jpg";
            CargarImagen(InputImage.Text);
        }

        private void LoadData()
        {
            if (_productCurrent == null) return;

            // 1. Mapeo a controles de Lectura
            TxtNombre.Text = _productCurrent.Name;
            TxtPrecio.Text = _productCurrent.Price.ToString("F2");
            TxtIngredientes.Text = _productCurrent.Ingredients;
            if (TxtCategoria != null) TxtCategoria.Text = _productCurrent.Category;

            // 2. Mapeo a controles de Edición
            InputName.Text = _productCurrent.Name;
            InputPrice.Text = _productCurrent.Price.ToString();
            InputIngredientes.Text = _productCurrent.Ingredients;
            InputImage.Text = _productCurrent.ImagePath;

            // Carga visual de la imagen
            CargarImagen(_productCurrent.ImagePath);

            // Mock de alérgenos
            string[] alergenosMock = { "Gluten", "Lácteos", "Huevo" };
            ListAlergenos.ItemsSource = alergenosMock;
        }

        /// <summary>
        /// Carga segura de imágenes manejando rutas absolutas (web) y relativas (recursos).
        /// </summary>
        private void CargarImagen(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            try
            {
                Uri imageUri;
                if (path.StartsWith("http"))
                {
                    imageUri = new Uri(path, UriKind.Absolute);
                }
                else
                {
                    imageUri = new Uri(path, UriKind.Relative);
                }

                BitmapImage bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = imageUri;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;

                bitmap.EndInit();

                ImgProducto.Source = bitmap;
            }
            catch (Exception)
            {
                // Fallback silencioso pereza hacer más
            }
        }

        private void ToggleEditMode(bool enableEdit)
        {
            _isEditMode = enableEdit;

            // Alternancia de visibilidad
            Visibility viewVis = enableEdit ? Visibility.Collapsed : Visibility.Visible;
            Visibility editVis = enableEdit ? Visibility.Visible : Visibility.Collapsed;

            // Panel Lectura
            TxtNombre.Visibility = viewVis;
            TxtPrecio.Visibility = viewVis;
            PillCategoria.Visibility = viewVis;
            TxtIngredientes.Visibility = viewVis;
            ListAlergenos.Visibility = viewVis;

            // Panel Escritura
            InputName.Visibility = editVis;
            PanelEditPrice.Visibility = editVis;
            PanelEditCategoria.Visibility = editVis;
            InputIngredientes.Visibility = editVis;
            InputAlergenos.Visibility = editVis;
            PanelEditImage.Visibility = editVis;

            // Actualización de textos e iconos del botón principal si no estamos creando
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

        #endregion

        #region Eventos de Botones

        private void BtnMainAction_Click(object sender, RoutedEventArgs e)
        {
            if (_isCreationMode)
            {
                // LÓGICA DE CREACIÓN
                ProductResult = new Product
                {
                    Name = InputName.Text,
                    Price = double.TryParse(InputPrice.Text, out double p) ? p : 0,
                    Ingredients = InputIngredientes.Text,
                    Category = (ComboCategory.SelectedItem as ComboBoxItem)?.Content.ToString() ?? "Platos",
                    SubCategory = "Todo",
                    IsAvailable = true,
                    ImagePath = string.IsNullOrEmpty(InputImage.Text) ? "/Assets/Images/default_food.png" : InputImage.Text
                };

                var confirm = new ConfirmWindow(
                    "Producto añadido correctamente al catálogo.",
                    "Éxito",
                    ConfirmType.Success);
                confirm.Owner = this;
                confirm.ShowDialog();

                ActionEdit = true;
                this.DialogResult = true;
                this.Close();
            }
            else
            {
                // LÓGICA DE EDICIÓN
                if (!_isEditMode)
                {
                    // Si estamos en modo ver, pasamos a modo editar
                    ToggleEditMode(true);
                }
                else
                {
                    // Si estamos editando, guardamos
                    var confirm = new ConfirmWindow(
                        "¿Deseas guardar los cambios realizados en la ficha?",
                        "Confirmar Guardado",
                        ConfirmType.Question);
                    confirm.Owner = this;

                    if (confirm.ShowDialog() == true)
                    {
                        // Actualizamos el objeto referencia directamente
                        _productCurrent.Name = InputName.Text;
                        _productCurrent.Price = double.TryParse(InputPrice.Text, out double p) ? p : 0;
                        _productCurrent.Ingredients = InputIngredientes.Text;
                        _productCurrent.ImagePath = InputImage.Text;

                        // Refrescamos visualmente
                        CargarImagen(_productCurrent.ImagePath);

                        ActionEdit = true;
                        ToggleEditMode(false);
                        LoadData();
                    }
                }
            }
        }

        private void BtnSecondaryAction_Click(object sender, RoutedEventArgs e)
        {
            if (_isCreationMode)
            {
                // Cancelar creación
                this.Close();
            }
            else if (_isEditMode)
            {
                // Cancelar edición
                LoadData();
                ToggleEditMode(false);
            }
            else
            {
                var confirm = new ConfirmWindow(
                    $"¿Estás seguro de eliminar '{_productCurrent?.Name}'?\nNo podrás recuperar este ítem.",
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

        #endregion
    }
}