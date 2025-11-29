using AppComida.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Serialization;

namespace AppComida.Persistence
{
    public class DataAgent
    {
        private string GetPath(string fileName)
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            // Busca hasta 6 niveles hacia arriba
            for (int i = 0; i < 6; i++)
            {
                string potentialPath = Path.Combine(currentDir, "data", fileName);
                if (File.Exists(potentialPath)) return potentialPath;

                DirectoryInfo parent = Directory.GetParent(currentDir);
                if (parent == null) break;
                currentDir = parent.FullName;
            }
            return null;
        }

        public List<User> LoadUsers()
        {
            string finalPath = GetPath("users.xml");

            if (finalPath == null) return new List<User>();

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<User>));
                using (StreamReader reader = new StreamReader(finalPath))
                {
                    return (List<User>)serializer.Deserialize(reader);
                }
            }
            catch (Exception ex)
            {
                return new List<User>();
            }
        }

        public List<Product> LoadProducts()
        {
            string finalPath = GetPath("products.xml");

            if (finalPath == null) return new List<Product>();

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Product>));
                using (StreamReader reader = new StreamReader(finalPath))
                {
                    return (List<Product>)serializer.Deserialize(reader);
                }
            }
            catch (Exception)
            {
                // En caso de error (archivo corrupto, etc), devolvemos lista vacía
                return new List<Product>();
            }
        }


        public void UpdateLastAccess(string username, DateTime newTime)
        {
            List<User> users = LoadUsers();
            string finalPath = GetPath("users.xml");

            if (finalPath == null) return;

            var userToUpdate = users.FirstOrDefault(u => u.username.Equals(username, StringComparison.OrdinalIgnoreCase));
            if (userToUpdate != null)
            {
                userToUpdate.last_access = newTime;

                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(List<User>));
                    using (StreamWriter writer = new StreamWriter(finalPath))
                    {
                        serializer.Serialize(writer, users);
                    }
                }
                catch (Exception)
                {
                    //  Podría capturar errores y hacerlo mejor, pero esto no es parte del requisito funcional y hay muchos trabajos que hacer como el de
                    //  distribuidos, que un dia de estos va a acabar conmigo
                }
            }
        }
    }
}