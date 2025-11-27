using AppComida.Persistence;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Windows;

namespace AppComida.Domain
{
    public class LoginController
    {
        private List<User> _usersDataBase;
        private DataAgent _agent;

        public LoginController()
        {
            _agent = new DataAgent();
            _usersDataBase = _agent.LoadUsers();
        }

        public User ValidateLogin(string username, string password)
        {
            User userLogged = null;

            // 1. Busca usuario en memoria (case insensitive)
            User storedUser = _usersDataBase.Find(u => u.username.Equals(username, StringComparison.OrdinalIgnoreCase));

            if (storedUser == null) return null;

            // 2. Calcula Hash
            string calculatedDigest = CalculateMD5(password + storedUser.salt);

            // 3. Compara
            if (!calculatedDigest.Trim().Equals(storedUser.digest.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            userLogged = storedUser;
            return userLogged;
        }

        private string CalculateMD5(string input)
        {
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
    }
}