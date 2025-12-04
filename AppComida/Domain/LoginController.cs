using AppComida.Persistence;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace AppComida.Domain
{
    public class LoginController
    {
        private List<User> _usersDataBase;
        private DataAgent _agent;

        public LoginController()
        {
            // Inicializamos el agente de persistencia y cargamos usuarios en memoria al inicio
            _agent = new DataAgent();
            _usersDataBase = _agent.LoadUsers();
        }

        /// <summary>
        /// Valida las credenciales comparando el hash de la contraseña introducida
        /// con el hash almacenado en base de datos (con Salt).
        /// </summary>
        public User ValidateLogin(string username, string password)
        {
            User storedUser = _usersDataBase.Find(u => u.username.Equals(username, StringComparison.OrdinalIgnoreCase));

            // Si el usuario no existe, rechazamos inmediatamente (Fail fast)
            if (storedUser == null) return null;

            // Recalculamos el hash con el Salt del usuario encontrado
            string calculatedDigest = CalculateMD5(password + storedUser.salt);

            // Comparación segura ignorando mayúsculas/minúsculas en el hash
            if (!calculatedDigest.Trim().Equals(storedUser.digest.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return storedUser;
        }

        /// <summary>
        /// Genera un hash MD5 a partir del input.
        /// Método sacado de distribuidos
        /// </summary>
        private string CalculateMD5(string input)
        {
            // Usamos 'using' para asegurar que el objeto criptográfico libera recursos nativos
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                    sb.Append(hashBytes[i].ToString("x2"));

                return sb.ToString();
            }
        }

        public void RegisterExit(string username)
        {
            _agent.UpdateLastAccess(username, DateTime.Now);
        }
    }
}