using AppComida.Domain;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace AppComida.Presentation
{
    public partial class ClientesPage : Page
    {
        private List<Client> _allClients;
        private List<Pedido> _externalPedidos; // Lista global de pedidos

        public ObservableCollection<Client> ClientsList { get; set; }
        private Client _selectedClient;
        private bool _isEditMode = false;
        private bool _isCreationMode = false;

        private readonly Brush _errorBorder = (Brush)new BrushConverter().ConvertFrom("#D32F2F");
        private readonly Brush _errorBackground = (Brush)new BrushConverter().ConvertFrom("#FFCDD2");
        private Brush _defaultBorder;
        private Brush _defaultBackground;

        public ClientesPage()
        {
            InitializeComponent();
            ClientsList = new ObservableCollection<Client>();
            _defaultBorder = InputNombre.BorderBrush;
            _defaultBackground = InputNombre.Background;
            LstClients.ItemsSource = ClientsList;
            _allClients = new List<Client>();
        }

        public void SetClientes(List<Client> clientes)
        {
            _allClients = clientes;
            RefreshList(TxtSearch.Text);
        }

        public List<Client> GetClientes()
        {
            return _allClients;
        }

        public void UpdateOrders(List<Pedido> pedidos)
        {
            _externalPedidos = pedidos;
            if (_selectedClient != null && PanelDetail.Visibility == Visibility.Visible)
            {
                PopulateDetailView(_selectedClient);
            }
        }

        private void RefreshList(string filter = "")
        {
            ClientsList.Clear();
            var filtered = string.IsNullOrWhiteSpace(filter)
                ? _allClients
                : _allClients.Where(c => c.NombreCompleto.ToLower().Contains(filter.ToLower()) ||
                                         (c.Contacto != null && c.Contacto.Telefono != null && c.Contacto.Telefono.Contains(filter)));
            foreach (var c in filtered) ClientsList.Add(c);
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e) => RefreshList(TxtSearch.Text);
        private void InputNombre_TextChanged(object sender, TextChangedEventArgs e) { if (LblErrorNombre.Visibility == Visibility.Visible) { LblErrorNombre.Visibility = Visibility.Collapsed; InputNombre.BorderBrush = _defaultBorder; InputNombre.Background = _defaultBackground; } }

        private void LstClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isEditMode || _isCreationMode) ToggleEditMode(false);
            if (LstClients.SelectedItem is Client client)
            {
                _selectedClient = client;
                PopulateDetailView(client);
                PanelDetail.Visibility = Visibility.Visible;
            }
            else
            {
                if (!_isCreationMode) PanelDetail.Visibility = Visibility.Hidden;
            }
        }

        private void PopulateDetailView(Client client)
        {
            string telefonoStr = client.Contacto?.Telefono ?? "No registrado";
            string emailStr = client.Contacto?.Email ?? "No registrado";
            string direccionStr = (client.Direcciones != null && client.Direcciones.Any()) ? client.Direcciones.First().Calle : "Sin dirección";
            string alergiasStr = (client.Alergias != null && client.Alergias.Any()) ? string.Join(", ", client.Alergias) : "Ninguna";

            TxtNombreHeader.Text = client.NombreCompleto;
            TxtNivel.Text = string.IsNullOrEmpty(client.Nivel) ? "ESTÁNDAR" : client.Nivel.ToUpper();
            if (client.Nivel == "Oro") TxtNivel.Foreground = Brushes.Goldenrod;
            else if (client.Nivel == "Plata") TxtNivel.Foreground = Brushes.Gray;
            else TxtNivel.Foreground = Brushes.Orange;

            TxtTelefono.Text = telefonoStr;
            TxtEmail.Text = emailStr;
            TxtDireccion.Text = direccionStr;
            TxtAlergias.Text = alergiasStr;
            int puntos = client.Fidelizacion != null ? client.Fidelizacion.PuntosAcumulados : 0;
            TxtPuntos.Text = puntos + " Pts";
            int canjeados = client.Fidelizacion != null ? client.Fidelizacion.PuntosCanjeados : 0;
            TxtPuntosCanjeados.Text = $"Canjeados: {canjeados}";

            BadgeAlergias.Visibility = (alergiasStr == "Ninguna" || string.IsNullOrWhiteSpace(alergiasStr)) ? Visibility.Collapsed : Visibility.Visible;

            // Historial dinámico
            System.Collections.IEnumerable fuenteDatos = null;
            if (_externalPedidos != null)
            {
                var pedidosDelCliente = _externalPedidos.Where(p => p.NombreCliente == client.NombreCompleto).ToList();
                if (pedidosDelCliente.Any()) fuenteDatos = pedidosDelCliente;
            }
            else if (client.Historial != null && client.Historial.Any()) fuenteDatos = client.Historial;

            if (fuenteDatos != null) { LstHistorial.ItemsSource = null; LstHistorial.ItemsSource = fuenteDatos; LstHistorial.Visibility = Visibility.Visible; TxtNoPedidos.Visibility = Visibility.Collapsed; }
            else { LstHistorial.ItemsSource = null; LstHistorial.Visibility = Visibility.Collapsed; TxtNoPedidos.Visibility = Visibility.Visible; }

            // Inputs Edit
            InputNombre.Text = client.Nombre; InputApellidos.Text = client.Apellidos; InputTelefono.Text = client.Contacto?.Telefono ?? "";
            InputEmail.Text = client.Contacto?.Email ?? ""; InputDireccion.Text = (client.Direcciones != null && client.Direcciones.Any()) ? client.Direcciones.First().Calle : "";
            InputAlergias.Text = (client.Alergias != null && client.Alergias.Any()) ? string.Join(", ", client.Alergias) : "";
            InputPuntos.Text = puntos.ToString();
        }

        private void ToggleEditMode(bool enable)
        {
            _isEditMode = enable;
            Visibility readVis = enable ? Visibility.Collapsed : Visibility.Visible;
            TxtNombreHeader.Visibility = readVis; TxtTelefono.Visibility = readVis; TxtEmail.Visibility = readVis; TxtDireccion.Visibility = readVis; TxtAlergias.Visibility = readVis; TxtPuntos.Visibility = readVis; TxtPuntosCanjeados.Visibility = readVis; BtnEdit.Visibility = readVis; BtnDelete.Visibility = readVis;
            Visibility writeVis = enable ? Visibility.Visible : Visibility.Collapsed;
            PanelEditHeader.Visibility = writeVis; InputTelefono.Visibility = writeVis; InputEmail.Visibility = writeVis; InputDireccion.Visibility = writeVis; InputAlergias.Visibility = writeVis; InputPuntos.Visibility = writeVis; PanelActions.Visibility = writeVis;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            LstClients.SelectedItem = null;
            _selectedClient = new Client { Contacto = new ContactInfo(), Direcciones = new List<Address>(), Alergias = new List<string>(), Fidelizacion = new Loyalty(), Preferencias = new Preferences(), Historial = new List<OrderRef>() };
            _isCreationMode = true;
            InputNombre.Text = ""; InputApellidos.Text = ""; InputTelefono.Text = ""; InputEmail.Text = ""; InputDireccion.Text = ""; InputAlergias.Text = ""; InputPuntos.Text = "0";
            InputNombre.BorderBrush = _defaultBorder; InputNombre.Background = _defaultBackground; LblErrorNombre.Visibility = Visibility.Collapsed;
            PanelDetail.Visibility = Visibility.Visible; ToggleEditMode(true); BtnEdit.Visibility = Visibility.Collapsed; BtnDelete.Visibility = Visibility.Collapsed; InputNombre.Focus();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e) { if (_selectedClient == null) return; _isCreationMode = false; InputNombre.BorderBrush = _defaultBorder; InputNombre.Background = _defaultBackground; LblErrorNombre.Visibility = Visibility.Collapsed; ToggleEditMode(true); }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient == null) return;
            var confirm = new ConfirmWindow("¿Eliminar cliente?", "Eliminar", ConfirmType.Danger); confirm.Owner = Window.GetWindow(this);
            if (confirm.ShowDialog() == true) { _allClients.Remove(_selectedClient); RefreshList(); PanelDetail.Visibility = Visibility.Hidden; _selectedClient = null; }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(InputNombre.Text)) { LblErrorNombre.Visibility = Visibility.Visible; InputNombre.BorderBrush = _errorBorder; InputNombre.Background = _errorBackground; return; }
            var confirm = new ConfirmWindow("¿Guardar cambios?", "Guardar", ConfirmType.Question); confirm.Owner = Window.GetWindow(this);
            if (confirm.ShowDialog() == true)
            {
                _selectedClient.Nombre = InputNombre.Text; _selectedClient.Apellidos = InputApellidos.Text;
                if (_selectedClient.Contacto == null) _selectedClient.Contacto = new ContactInfo();
                _selectedClient.Contacto.Telefono = InputTelefono.Text; _selectedClient.Contacto.Email = InputEmail.Text;
                if (_selectedClient.Direcciones == null) _selectedClient.Direcciones = new List<Address>();
                _selectedClient.Direcciones.Clear();
                if (!string.IsNullOrWhiteSpace(InputDireccion.Text)) _selectedClient.Direcciones.Add(new Address { Calle = InputDireccion.Text, EsPrincipal = true });
                if (_selectedClient.Alergias == null) _selectedClient.Alergias = new List<string>();
                _selectedClient.Alergias.Clear();
                if (!string.IsNullOrWhiteSpace(InputAlergias.Text)) _selectedClient.Alergias.AddRange(InputAlergias.Text.Split(',').Select(x => x.Trim()).Where(x => !string.IsNullOrEmpty(x)));
                if (_selectedClient.Fidelizacion == null) _selectedClient.Fidelizacion = new Loyalty();
                int puntosFinales = 0; if (int.TryParse(InputPuntos.Text, out int newPoints)) puntosFinales = newPoints;
                _selectedClient.Fidelizacion.PuntosAcumulados = puntosFinales;
                if (puntosFinales >= 2000) _selectedClient.Nivel = "Oro"; else if (puntosFinales >= 800) _selectedClient.Nivel = "Plata"; else _selectedClient.Nivel = "Bronce";

                if (_isCreationMode) { int newId = _allClients.Any() ? _allClients.Max(c => c.Id) + 1 : 1; _selectedClient.Id = newId; _allClients.Add(_selectedClient); new ConfirmWindow("Cliente registrado.", "Éxito", ConfirmType.Success) { Owner = Window.GetWindow(this) }.ShowDialog(); }
                _isCreationMode = false; ToggleEditMode(false); RefreshList(TxtSearch.Text); PopulateDetailView(_selectedClient); if (!ClientsList.Contains(_selectedClient)) LstClients.SelectedItem = _selectedClient;
            }
        }

        private void BtnCanjearPuntos_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient == null || _selectedClient.Fidelizacion == null) return;
            var inputWindow = new InputWindow($"Puntos: {_selectedClient.Fidelizacion.PuntosAcumulados}\n¿Canjear cuántos?", "Canjear", "0"); inputWindow.Owner = Window.GetWindow(this);
            if (inputWindow.ShowDialog() == true)
            {
                if (int.TryParse(inputWindow.ResponseText, out int puntosACanjear) && puntosACanjear > 0)
                {
                    if (_selectedClient.Fidelizacion.PuntosAcumulados >= puntosACanjear) { _selectedClient.Fidelizacion.PuntosAcumulados -= puntosACanjear; _selectedClient.Fidelizacion.PuntosCanjeados += puntosACanjear; PopulateDetailView(_selectedClient); RefreshList(TxtSearch.Text); new ConfirmWindow("Canjeado correctamente.", "Éxito", ConfirmType.Success) { Owner = Window.GetWindow(this) }.ShowDialog(); }
                    else new ConfirmWindow("Saldo insuficiente.", "Error", ConfirmType.Danger) { Owner = Window.GetWindow(this) }.ShowDialog();
                }
                else new ConfirmWindow("Cantidad inválida.", "Error", ConfirmType.Danger) { Owner = Window.GetWindow(this) }.ShowDialog();
            }
        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e) { if (_isCreationMode) { PanelDetail.Visibility = Visibility.Hidden; _isCreationMode = false; _selectedClient = null; LstClients.SelectedItem = null; } else PopulateDetailView(_selectedClient); ToggleEditMode(false); }
    }
}