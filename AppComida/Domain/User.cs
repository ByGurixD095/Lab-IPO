using System;

namespace AppComida.Domain
{
    /// <summary>
    /// Clase que representa al usuario del sistema para el login y gestión de sesión.
    /// </summary>
    public class User
    {
        // Propiedades básicas mapeadas directamente de la BDD
        public string username { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public DateTime last_access { get; set; }

        // Gestión de imagen de perfil (ruta relativa o base64)
        public string image { get; set; }

        // Seguridad: Almacenamos el salt y el hash, nunca la contraseña en plano
        public string salt { get; set; }
        public string digest { get; set; }
    }
}