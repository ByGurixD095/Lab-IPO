using AppComida.Domain;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
                Debug.WriteLine(ex.ToString());
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
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
                return new List<Product>();
            }
        }

        public List<Client> LoadClients()
        {
            string finalPath = GetPath("clients.xml");

            // 1. DETECCIÓN DE ERROR DE RUTA
            if (finalPath == null)
            {
                MessageBox.Show("No se encontró el archivo 'data/clients.xml'.\nVerifica que la carpeta 'data' existe y el archivo está dentro.",
                                "Archivo No Encontrado", MessageBoxButton.OK, MessageBoxImage.Warning);
                return new List<Client>();
            }

            try
            {
                // El wrapper debe coincidir con la estructura del XML
                XmlSerializer serializer = new XmlSerializer(typeof(ClientListWrapper));
                using (StreamReader reader = new StreamReader(finalPath))
                {
                    var wrapper = (ClientListWrapper)serializer.Deserialize(reader);
                    return wrapper.Clients;
                }
            }
            catch (Exception ex)
            {
                // 2. DETECCIÓN DE ERROR DE FORMATO XML
                // Mostramos el InnerException que es el que suele decir la línea exacta del fallo
                string errorDetalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                MessageBox.Show($"Error leyendo 'clients.xml':\n{errorDetalle}\n\nRevisa que el XML tenga la raíz <Clientes> y dentro <Cliente>.",
                                "Error de Formato XML", MessageBoxButton.OK, MessageBoxImage.Error);

                Debug.WriteLine("Error cargando clientes: " + ex.ToString());
                return new List<Client>();
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
                catch (Exception ex)
                {
                    //  Podría capturar errores y hacerlo mejor, pero esto no es parte del requisito funcional y hay muchos trabajos que hacer como el de
                    //  distribuidos, que un dia de estos va a acabar conmigo
                    Debug.WriteLine(ex.ToString());
                }
            }
        }
    }
}