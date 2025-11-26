using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using AppComida.Persistence;

namespace AppComida.Domain
{
    public class LoginController
    {
        private List<User> _usersCache;
        private DataAgent _agent;

        public LoginController()
        {
            _agent = new DataAgent();
            _usersCache = _agent.LoadUsers(); // Carga usuarios al iniciar el controlador
        }

        public bool ValidateLogin(string username, string password, out User userLogged)
        {
            userLogged = null;
            
            // 1. Busca usuario en memoria (case insensitive)
            User storedUser = _usersCache.Find(u => u.username.Equals(username, StringComparison.OrdinalIgnoreCase));
            
            if (storedUser == null) return false;

            // 2. Calcula Hash
            string calculatedDigest = CalculateMD5(password + storedUser.salt);

            // 3. Compara
            if (calculatedDigest.Trim().Equals(storedUser.digest.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                userLogged = storedUser;
                return true;
            }

            return false;
        }

        private string CalculateMD5(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                    sb.Append(hashBytes[i].ToString("x2")); // formato hex minúsculas
                return sb.ToString();
            }
        }
    }
}