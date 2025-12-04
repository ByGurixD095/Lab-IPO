using AppComida.Domain;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AppComida.Presentation
{
    /// <summary>
    /// Lógica de interacción para la gestión de clientes.
    /// Maneja el CRUD básico, visualización de historial y sistema de puntos de fidelización.
    /// </summary>
    public partial class ClientesPage : Page
    {
        #region Campos y Propiedades

        // Referencia a la "Single Source of Truth" que viene del MainWindow
        private List<Client> _allClients;

        // Referencia a los pedidos globales para calcular el historial dinámicamente
        private List<Pedido> _externalPedidos;

        // Colección observable para el DataBinding de la lista lateral (WPF actualiza la UI automáticamente)
        public ObservableCollection<Client> ClientsList { get; set; }

        private Client _selectedClient;

        // Flags de estado para controlar la interfaz
        private bool _isEditMode = false;
        private bool _isCreationMode = false;

        // Estilos para validación visual
        private readonly Brush _errorBorder = (Brush)new BrushConverter().ConvertFrom("#D32F2F");
        private readonly Brush _errorBackground = (Brush)new BrushConverter().ConvertFrom("#FFCDD2");
        private Brush _defaultBorder;
        private Brush _defaultBackground;

        #endregion

        #region Constructor e Inicialización

        public ClientesPage()
        {
            InitializeComponent();

            // Inicialización de colecciones
            ClientsList = new ObservableCollection<Client>();
            _allClients = new List<Client>();

            // Guardamos estilos por defecto para resetear validaciones
            _defaultBorder = InputNombre.BorderBrush;
            _defaultBackground = InputNombre.Background;

            // Binding manual
            LstClients.ItemsSource = ClientsList;
        }

        /// <summary>
        /// Método llamado por MainWindow para inyectar los datos cargados.
        /// </summary>
        public void SetClientes(List<Client> clientes)
        {
            _allClients = clientes;
            RefreshList(TxtSearch.Text);
        }

        /// <summary>
        /// Permite al padre recuperar los datos modificados antes de navegar.
        /// </summary>
        public List<Client> GetClientes()
        {
            return _allClients;
        }

        /// <summary>
        /// Actualiza la referencia de pedidos para mostrar el historial de compras en la ficha.
        /// Se llama cada vez que entramos en la vista para asegurar datos frescos.
        /// </summary>
        public void UpdateOrders(List<Pedido> pedidos)
        {
            _externalPedidos = pedidos;

            if (_selectedClient != null && PanelDetail.Visibility == Visibility.Visible)
            {
                PopulateDetailView(_selectedClient);
            }
        }

        #endregion

        #region Lógica de Listado y Filtrado

        private void RefreshList(string filter = "")
        {
            ClientsList.Clear();

            // Filtro case-insensitive por nombre o teléfono
            var filtered = string.IsNullOrWhiteSpace(filter)
                ? _allClients
                : _allClients.Where(c => c.NombreCompleto.ToLower().Contains(filter.ToLower()) ||
                                         (c.Contacto != null && c.Contacto.Telefono != null && c.Contacto.Telefono.Contains(filter)));

            foreach (var c in filtered)
            {
                ClientsList.Add(c);
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshList(TxtSearch.Text);
        }

        private void LstClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isEditMode || _isCreationMode)
                ToggleEditMode(false);

            if (LstClients.SelectedItem is Client client)
            {
                _selectedClient = client;
                PopulateDetailView(client);
                PanelDetail.Visibility = Visibility.Visible;
            }
            else
            {
                // Si no hay selección y no estamos creando uno nuevo, ocultamos el panel
                if (!_isCreationMode)
                    PanelDetail.Visibility = Visibility.Hidden;
            }
        }

        #endregion

        #region Visualización de Detalles (Read Mode)

        private void PopulateDetailView(Client client)
        {
            string telefonoStr = client.Contacto?.Telefono ?? "No registrado";
            string emailStr = client.Contacto?.Email ?? "No registrado";
            string direccionStr = (client.Direcciones != null && client.Direcciones.Any()) ? client.Direcciones.First().Calle : "Sin dirección";
            string alergiasStr = (client.Alergias != null && client.Alergias.Any()) ? string.Join(", ", client.Alergias) : "Ninguna";

            // Header y Estado
            TxtNombreHeader.Text = client.NombreCompleto;
            TxtNivel.Text = string.IsNullOrEmpty(client.Nivel) ? "ESTÁNDAR" : client.Nivel.ToUpper();

            // Color coding del nivel
            if (client.Nivel == "Oro") TxtNivel.Foreground = Brushes.Goldenrod;
            else if (client.Nivel == "Plata") TxtNivel.Foreground = Brushes.Gray;
            else TxtNivel.Foreground = Brushes.Orange;

            // Datos de contacto
            TxtTelefono.Text = telefonoStr;
            TxtEmail.Text = emailStr;
            TxtDireccion.Text = direccionStr;
            TxtAlergias.Text = alergiasStr;

            // Fidelización
            int puntos = client.Fidelizacion != null ? client.Fidelizacion.PuntosAcumulados : 0;
            TxtPuntos.Text = puntos + " Pts";
            int canjeados = client.Fidelizacion != null ? client.Fidelizacion.PuntosCanjeados : 0;
            TxtPuntosCanjeados.Text = $"Canjeados: {canjeados}";

            BadgeAlergias.Visibility = (alergiasStr == "Ninguna" || string.IsNullOrWhiteSpace(alergiasStr))
                ? Visibility.Collapsed
                : Visibility.Visible;

            // Lógica del Historial
            System.Collections.IEnumerable fuenteDatos = null;

            if (_externalPedidos != null)
            {
                var pedidosDelCliente = _externalPedidos.Where(p => p.NombreCliente == client.NombreCompleto).ToList();
                if (pedidosDelCliente.Any()) fuenteDatos = pedidosDelCliente;
            }
            else if (client.Historial != null && client.Historial.Any())
            {
                fuenteDatos = client.Historial;
            }

            if (fuenteDatos != null)
            {
                LstHistorial.ItemsSource = null;
                LstHistorial.ItemsSource = fuenteDatos;
                LstHistorial.Visibility = Visibility.Visible;
                TxtNoPedidos.Visibility = Visibility.Collapsed;
            }
            else
            {
                LstHistorial.ItemsSource = null;
                LstHistorial.Visibility = Visibility.Collapsed;
                TxtNoPedidos.Visibility = Visibility.Visible;
            }

            // Precarga de campos de edición
            InputNombre.Text = client.Nombre;
            InputApellidos.Text = client.Apellidos;
            InputTelefono.Text = client.Contacto?.Telefono ?? "";
            InputEmail.Text = client.Contacto?.Email ?? "";
            InputDireccion.Text = (client.Direcciones != null && client.Direcciones.Any()) ? client.Direcciones.First().Calle : "";
            InputAlergias.Text = (client.Alergias != null && client.Alergias.Any()) ? string.Join(", ", client.Alergias) : "";
            InputPuntos.Text = puntos.ToString();
        }

        #endregion

        #region Gestión de Edición y CRUD

        private void ToggleEditMode(bool enable)
        {
            _isEditMode = enable;

            Visibility readVis = enable ? Visibility.Collapsed : Visibility.Visible;
            Visibility writeVis = enable ? Visibility.Visible : Visibility.Collapsed;

            TxtNombreHeader.Visibility = readVis;
            TxtTelefono.Visibility = readVis;
            TxtEmail.Visibility = readVis;
            TxtDireccion.Visibility = readVis;
            TxtAlergias.Visibility = readVis;
            TxtPuntos.Visibility = readVis;
            TxtPuntosCanjeados.Visibility = readVis;
            BtnEdit.Visibility = readVis;
            BtnDelete.Visibility = readVis;

            // Elementos de edición
            PanelEditHeader.Visibility = writeVis;
            InputTelefono.Visibility = writeVis;
            InputEmail.Visibility = writeVis;
            InputDireccion.Visibility = writeVis;
            InputAlergias.Visibility = writeVis;
            InputPuntos.Visibility = writeVis;
            PanelActions.Visibility = writeVis;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            LstClients.SelectedItem = null;

            _selectedClient = new Client
            {
                Contacto = new ContactInfo(),
                Direcciones = new List<Address>(),
                Alergias = new List<string>(),
                Fidelizacion = new Loyalty(),
                Preferencias = new Preferences(),
                Historial = new List<OrderRef>()
            };

            _isCreationMode = true;

            // Limpieza de formulario
            InputNombre.Text = "";
            InputApellidos.Text = "";
            InputTelefono.Text = "";
            InputEmail.Text = "";
            InputDireccion.Text = "";
            InputAlergias.Text = "";
            InputPuntos.Text = "0";

            // Reset de validaciones
            InputNombre.BorderBrush = _defaultBorder;
            InputNombre.Background = _defaultBackground;
            LblErrorNombre.Visibility = Visibility.Collapsed;

            PanelDetail.Visibility = Visibility.Visible;
            ToggleEditMode(true);

            BtnEdit.Visibility = Visibility.Collapsed;
            BtnDelete.Visibility = Visibility.Collapsed;
            InputNombre.Focus();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient == null) return;

            _isCreationMode = false;
            // Reset de estilos de error
            InputNombre.BorderBrush = _defaultBorder;
            InputNombre.Background = _defaultBackground;
            LblErrorNombre.Visibility = Visibility.Collapsed;

            ToggleEditMode(true);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient == null) return;

            var confirm = new ConfirmWindow("¿Seguro que deseas eliminar este cliente?\nEsta acción es irreversible.", "Confirmar Eliminación", ConfirmType.Danger);
            confirm.Owner = Window.GetWindow(this);

            if (confirm.ShowDialog() == true)
            {
                _allClients.Remove(_selectedClient);
                RefreshList();
                PanelDetail.Visibility = Visibility.Hidden;
                _selectedClient = null;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            // Validación básica
            if (string.IsNullOrWhiteSpace(InputNombre.Text))
            {
                LblErrorNombre.Visibility = Visibility.Visible;
                InputNombre.BorderBrush = _errorBorder;
                InputNombre.Background = _errorBackground;
                return;
            }

            var confirm = new ConfirmWindow("¿Deseas guardar los cambios en la ficha?", "Guardar Cambios", ConfirmType.Question);
            confirm.Owner = Window.GetWindow(this);

            if (confirm.ShowDialog() == true)
            {
                _selectedClient.Nombre = InputNombre.Text;
                _selectedClient.Apellidos = InputApellidos.Text;

                if (_selectedClient.Contacto == null) _selectedClient.Contacto = new ContactInfo();
                _selectedClient.Contacto.Telefono = InputTelefono.Text;
                _selectedClient.Contacto.Email = InputEmail.Text;

                // Gestión simplificada de direcciones 
                if (_selectedClient.Direcciones == null) _selectedClient.Direcciones = new List<Address>();
                _selectedClient.Direcciones.Clear();
                if (!string.IsNullOrWhiteSpace(InputDireccion.Text))
                    _selectedClient.Direcciones.Add(new Address { Calle = InputDireccion.Text, EsPrincipal = true });

                // Gestión de alergias
                if (_selectedClient.Alergias == null) _selectedClient.Alergias = new List<string>();
                _selectedClient.Alergias.Clear();
                if (!string.IsNullOrWhiteSpace(InputAlergias.Text))
                    _selectedClient.Alergias.AddRange(InputAlergias.Text.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)));

                // Puntos y cálculo de nivel
                if (_selectedClient.Fidelizacion == null) _selectedClient.Fidelizacion = new Loyalty();
                int puntosFinales = 0;
                if (int.TryParse(InputPuntos.Text, out int newPoints)) puntosFinales = newPoints;

                _selectedClient.Fidelizacion.PuntosAcumulados = puntosFinales;

                // Recalculamos nivel automáticamente según puntos
                if (puntosFinales >= 2000) _selectedClient.Nivel = "Oro";
                else if (puntosFinales >= 800) _selectedClient.Nivel = "Plata";
                else _selectedClient.Nivel = "Bronce";

                // Si es nuevo, generamos ID y lo añadimos a la lista maestra
                if (_isCreationMode)
                {
                    int newId = _allClients.Any() ? _allClients.Max(c => c.Id) + 1 : 1;
                    _selectedClient.Id = newId;
                    _allClients.Add(_selectedClient);

                    new ConfirmWindow("Cliente registrado correctamente en el sistema.", "Éxito", ConfirmType.Success) { Owner = Window.GetWindow(this) }.ShowDialog();
                }

                // Finalización
                _isCreationMode = false;
                ToggleEditMode(false);
                RefreshList(TxtSearch.Text);
                PopulateDetailView(_selectedClient);

                // Re-seleccionar para no perder el foco visual
                if (!ClientsList.Contains(_selectedClient)) LstClients.SelectedItem = _selectedClient;
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (_isCreationMode)
            {
                PanelDetail.Visibility = Visibility.Hidden;
                _isCreationMode = false;
                _selectedClient = null;
                LstClients.SelectedItem = null;
            }
            else
            {
                // Revertimos cambios visuales recargando del objeto original
                PopulateDetailView(_selectedClient);
            }
            ToggleEditMode(false);
        }

        private void InputNombre_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Eliminamos feedback de error en cuanto el usuario escribe algo
            if (LblErrorNombre.Visibility == Visibility.Visible)
            {
                LblErrorNombre.Visibility = Visibility.Collapsed;
                InputNombre.BorderBrush = _defaultBorder;
                InputNombre.Background = _defaultBackground;
            }
        }

        #endregion

        #region Sistema Canjeo de pts

        private void BtnCanjearPuntos_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient == null || _selectedClient.Fidelizacion == null) return;

            var inputWindow = new InputWindow($"Saldo actual: {_selectedClient.Fidelizacion.PuntosAcumulados}\n¿Cuántos puntos desea canjear?", "Canjear Puntos", "0");
            inputWindow.Owner = Window.GetWindow(this);

            if (inputWindow.ShowDialog() == true)
            {
                if (int.TryParse(inputWindow.ResponseText, out int puntosACanjear) && puntosACanjear > 0)
                {
                    if (_selectedClient.Fidelizacion.PuntosAcumulados >= puntosACanjear)
                    {
                        _selectedClient.Fidelizacion.PuntosAcumulados -= puntosACanjear;
                        _selectedClient.Fidelizacion.PuntosCanjeados += puntosACanjear;

                        PopulateDetailView(_selectedClient);
                        RefreshList(TxtSearch.Text);

                        new ConfirmWindow("Canje realizado. El saldo ha sido actualizado.", "Operación Exitosa", ConfirmType.Success) { Owner = Window.GetWindow(this) }.ShowDialog();
                    }
                    else
                    {
                        new ConfirmWindow("El cliente no tiene saldo suficiente para esta operación.", "Saldo Insuficiente", ConfirmType.Danger) { Owner = Window.GetWindow(this) }.ShowDialog();
                    }
                }
                else
                {
                    new ConfirmWindow("Por favor, introduce una cantidad numérica válida.", "Error de Formato", ConfirmType.Danger) { Owner = Window.GetWindow(this) }.ShowDialog();
                }
            }
        }

        #endregion
    }
}