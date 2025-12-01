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
        private ClientController _controller;
        private List<Client> _allClients; // Simula nuestra base de datos en memoria
        public ObservableCollection<Client> ClientsList { get; set; }

        private Client _selectedClient;
        private bool _isEditMode = false;
        private bool _isCreationMode = false;

        // Variables para manejo de errores visuales
        private readonly Brush _errorBorder = (Brush)new BrushConverter().ConvertFrom("#D32F2F");
        private readonly Brush _errorBackground = (Brush)new BrushConverter().ConvertFrom("#FFCDD2");
        private Brush _defaultBorder;
        private Brush _defaultBackground;

        public ClientesPage()
        {
            InitializeComponent();
            _controller = new ClientController();
            ClientsList = new ObservableCollection<Client>();

            // Guardamos los estilos originales del TextBox
            _defaultBorder = InputNombre.BorderBrush;
            _defaultBackground = InputNombre.Background;

            // Vincular la lista UI a la colección observable
            LstClients.ItemsSource = ClientsList;

            // Cargar datos iniciales
            LoadInitialData();
        }

        private void LoadInitialData()
        {
            _allClients = _controller.GetAllClients();
            if (_allClients == null) _allClients = new List<Client>();

            RefreshList();
        }

        private void RefreshList(string filter = "")
        {
            ClientsList.Clear();

            var filtered = string.IsNullOrWhiteSpace(filter)
                ? _allClients
                : _allClients.Where(c => c.NombreCompleto.ToLower().Contains(filter.ToLower()) ||
                                         (c.Contacto != null && c.Contacto.Telefono != null && c.Contacto.Telefono.Contains(filter)));

            foreach (var c in filtered)
            {
                ClientsList.Add(c);
            }
        }

        // --- EVENTOS UI ---

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            RefreshList(TxtSearch.Text);
        }

        // Limpia el error visual cuando el usuario empieza a escribir
        private void InputNombre_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (LblErrorNombre.Visibility == Visibility.Visible)
            {
                LblErrorNombre.Visibility = Visibility.Collapsed;
                InputNombre.BorderBrush = _defaultBorder;
                InputNombre.Background = _defaultBackground; // Restaurar fondo original
            }
        }

        private void LstClients_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isEditMode || _isCreationMode)
            {
                ToggleEditMode(false);
            }

            if (LstClients.SelectedItem is Client client)
            {
                _selectedClient = client;
                PopulateDetailView(client);
                PanelDetail.Visibility = Visibility.Visible;
            }
            else
            {
                if (!_isCreationMode)
                {
                    PanelDetail.Visibility = Visibility.Hidden;
                }
            }
        }

        // --- GESTIÓN DE VISTA DETALLE ---

        private void PopulateDetailView(Client client)
        {
            string telefonoStr = client.Contacto?.Telefono ?? "No registrado";
            string emailStr = client.Contacto?.Email ?? "No registrado";

            string direccionStr = (client.Direcciones != null && client.Direcciones.Any())
                                  ? client.Direcciones.First().Calle
                                  : "Sin dirección";

            string alergiasStr = (client.Alergias != null && client.Alergias.Any())
                                 ? string.Join(", ", client.Alergias)
                                 : "Ninguna";

            // --- MODO LECTURA ---
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

            // AGREGADO: Mostrar puntos canjeados
            int canjeados = client.Fidelizacion != null ? client.Fidelizacion.PuntosCanjeados : 0;
            TxtPuntosCanjeados.Text = $"Canjeados: {canjeados}";

            BadgeAlergias.Visibility = (alergiasStr == "Ninguna" || string.IsNullOrWhiteSpace(alergiasStr))
                                       ? Visibility.Collapsed : Visibility.Visible;

            // --- MODO EDICIÓN (Pre-cargar inputs) ---
            InputNombre.Text = client.Nombre;
            InputApellidos.Text = client.Apellidos;

            InputTelefono.Text = client.Contacto?.Telefono ?? "";
            InputEmail.Text = client.Contacto?.Email ?? "";

            InputDireccion.Text = (client.Direcciones != null && client.Direcciones.Any())
                                   ? client.Direcciones.First().Calle
                                   : "";

            InputAlergias.Text = (client.Alergias != null && client.Alergias.Any())
                                 ? string.Join(", ", client.Alergias)
                                 : "";

            // Cargar puntos en el Input para editar
            InputPuntos.Text = puntos.ToString();
        }

        private void ToggleEditMode(bool enable)
        {
            _isEditMode = enable;

            // Visibilidad Lectura
            Visibility readVis = enable ? Visibility.Collapsed : Visibility.Visible;
            TxtNombreHeader.Visibility = readVis;
            TxtTelefono.Visibility = readVis;
            TxtEmail.Visibility = readVis;
            TxtDireccion.Visibility = readVis;
            TxtAlergias.Visibility = readVis;
            TxtPuntos.Visibility = readVis; // Ocultar texto de puntos
            TxtPuntosCanjeados.Visibility = readVis; // Ocultar canjeados en edición si se desea
            BtnEdit.Visibility = readVis;
            BtnDelete.Visibility = readVis;

            // Visibilidad Escritura
            Visibility writeVis = enable ? Visibility.Visible : Visibility.Collapsed;
            PanelEditHeader.Visibility = writeVis;
            InputTelefono.Visibility = writeVis;
            InputEmail.Visibility = writeVis;
            InputDireccion.Visibility = writeVis;
            InputAlergias.Visibility = writeVis;
            InputPuntos.Visibility = writeVis; // Mostrar input de puntos
            PanelActions.Visibility = writeVis;
        }

        // --- BOTONES ACCIÓN ---

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            LstClients.SelectedItem = null;

            _selectedClient = new Client
            {
                Contacto = new ContactInfo(),
                Direcciones = new List<Address>(),
                Alergias = new List<string>(),
                Fidelizacion = new Loyalty(),
                Preferencias = new Preferences()
            };

            _isCreationMode = true;

            // Limpiar inputs visuales
            InputNombre.Text = "";
            InputApellidos.Text = "";
            InputTelefono.Text = "";
            InputEmail.Text = "";
            InputDireccion.Text = "";
            InputAlergias.Text = "";
            InputPuntos.Text = "0"; // Puntos por defecto al crear

            // Limpiar Errores previos y restaurar colores
            InputNombre.BorderBrush = _defaultBorder;
            InputNombre.Background = _defaultBackground;
            LblErrorNombre.Visibility = Visibility.Collapsed;

            PanelDetail.Visibility = Visibility.Visible;
            ToggleEditMode(true);

            BtnEdit.Visibility = Visibility.Collapsed;
            BtnDelete.Visibility = Visibility.Collapsed;

            // Foco al nombre para empezar a escribir
            InputNombre.Focus();
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient == null) return;
            _isCreationMode = false;

            // Limpiar Errores previos
            InputNombre.BorderBrush = _defaultBorder;
            InputNombre.Background = _defaultBackground;
            LblErrorNombre.Visibility = Visibility.Collapsed;

            ToggleEditMode(true);
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient == null) return;

            var confirm = new ConfirmWindow(
                   $"¿Seguro que quieres eliminar a este cliente?\nEsta acción no se puede deshacer.",
                   "Eliminar Cliente",
                   ConfirmType.Danger);
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
            // 1. Validación VISUAL
            if (string.IsNullOrWhiteSpace(InputNombre.Text))
            {
                LblErrorNombre.Visibility = Visibility.Visible;
                InputNombre.BorderBrush = _errorBorder;
                InputNombre.Background = _errorBackground; // Fondo rojo

                return;
            }
            var confirm = new ConfirmWindow(
                "¿Deseas guardar los cambios realizados?",
                "Guardar Cambios",
                ConfirmType.Question);
            confirm.Owner = Window.GetWindow(this);

            if (confirm.ShowDialog() == true)
            {

                // 2. Actualizar datos del objeto
                _selectedClient.Nombre = InputNombre.Text;
                _selectedClient.Apellidos = InputApellidos.Text;

                if (_selectedClient.Contacto == null) _selectedClient.Contacto = new ContactInfo();
                _selectedClient.Contacto.Telefono = InputTelefono.Text;
                _selectedClient.Contacto.Email = InputEmail.Text;

                if (_selectedClient.Direcciones == null) _selectedClient.Direcciones = new List<Address>();
                _selectedClient.Direcciones.Clear();
                if (!string.IsNullOrWhiteSpace(InputDireccion.Text))
                {
                    _selectedClient.Direcciones.Add(new Address { Calle = InputDireccion.Text, EsPrincipal = true });
                }

                if (_selectedClient.Alergias == null) _selectedClient.Alergias = new List<string>();
                _selectedClient.Alergias.Clear();
                if (!string.IsNullOrWhiteSpace(InputAlergias.Text))
                {
                    var listaAlergias = InputAlergias.Text.Split(',')
                                              .Select(x => x.Trim())
                                              .Where(x => !string.IsNullOrEmpty(x));
                    _selectedClient.Alergias.AddRange(listaAlergias);
                }

                // GUARDAR PUNTOS MODIFICADOS
                if (_selectedClient.Fidelizacion == null) _selectedClient.Fidelizacion = new Loyalty();
                int puntosFinales = 0;
                if (int.TryParse(InputPuntos.Text, out int newPoints))
                {
                    puntosFinales = newPoints;
                }
                _selectedClient.Fidelizacion.PuntosAcumulados = puntosFinales;

                // AGREGADO: Calcular Nivel automáticamente
                if (puntosFinales >= 2000)
                {
                    _selectedClient.Nivel = "Oro";
                }
                else if (puntosFinales >= 800)
                {
                    _selectedClient.Nivel = "Plata";
                }
                else
                {
                    _selectedClient.Nivel = "Bronce";
                }

                // 3. Persistencia Simulada
                if (_isCreationMode)
                {
                    int newId = 1;
                    if (_allClients.Any())
                    {
                        newId = _allClients.Max(c => c.Id) + 1;
                    }
                    _selectedClient.Id = newId;

                    // NOTA: Ya no asignamos "Bronce" por defecto aquí, 
                    // porque se ha calculado arriba según los puntos.

                    _allClients.Add(_selectedClient);

                    var cWindow = new ConfirmWindow(
                        "Cliente registrado correctamente.",
                        "Éxito",
                        ConfirmType.Success);
                    cWindow.Owner = Window.GetWindow(this);

                    cWindow.ShowDialog();
                }

                _isCreationMode = false;
                ToggleEditMode(false);


                // 4. Refrescar la lista
                RefreshList(TxtSearch.Text);

                // 5. Volver a pintar el detalle
                PopulateDetailView(_selectedClient);
                if (!ClientsList.Contains(_selectedClient))
                {
                    LstClients.SelectedItem = _selectedClient;
                }

            }
        }

        private void BtnCanjearPuntos_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedClient == null || _selectedClient.Fidelizacion == null) return;

            // --- CAMBIO: Usamos la nueva InputWindow ---
            var inputWindow = new InputWindow(
                $"El cliente tiene {_selectedClient.Fidelizacion.PuntosAcumulados} puntos.\n¿Cuántos deseas canjear?",
                "Canjear Puntos",
                "0");

            inputWindow.Owner = Window.GetWindow(this); // Para que aparezca centrada sobre la app

            if (inputWindow.ShowDialog() == true)
            {
                string input = inputWindow.ResponseText;

                // Validar que sea número y positivo
                if (int.TryParse(input, out int puntosACanjear) && puntosACanjear > 0)
                {
                    if (_selectedClient.Fidelizacion.PuntosAcumulados >= puntosACanjear)
                    {
                        // Operación
                        _selectedClient.Fidelizacion.PuntosAcumulados -= puntosACanjear;
                        _selectedClient.Fidelizacion.PuntosCanjeados += puntosACanjear;

                        // Refrescar UI
                        PopulateDetailView(_selectedClient);
                        RefreshList(TxtSearch.Text);

                        ConfirmWindow w = new ConfirmWindow(
                            $"Se han canjeado {puntosACanjear} puntos correctamente.",
                            "Canje Realizado",
                            ConfirmType.Success);
                        w.Owner = Window.GetWindow(this);
                        w.ShowDialog();
                    }
                    else
                    {
                        ConfirmWindow w = new ConfirmWindow(
                            $"Saldo insuficiente. El cliente solo tiene {_selectedClient.Fidelizacion.PuntosAcumulados} puntos.",
                            "Error de Saldo",
                            ConfirmType.Danger);
                        w.Owner = Window.GetWindow(this);
                        w.ShowDialog();
                    }
                }
                else
                {
                    ConfirmWindow w = new ConfirmWindow(
                       "Por favor, introduce una cantidad numérica válida mayor a 0.",
                       "Formato Incorrecto",
                       ConfirmType.Danger);
                    w.Owner = Window.GetWindow(this);
                    w.ShowDialog();
                }
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
                PopulateDetailView(_selectedClient);
            }
            ToggleEditMode(false);
        }
    }
}