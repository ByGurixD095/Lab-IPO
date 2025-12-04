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
    /// <summary>
    /// Clase encargada de la gestión de ficheros XML.
    /// Centraliza todas las operaciones de lectura y escritura para desacoplar el dominio del almacenamiento.
    /// </summary>
    public class DataAgent
    {
        /// <summary>
        /// Busca el archivo de datos subiendo niveles de directorio.
        /// Necesario porque al compilar, el ejecutable corre en /bin/Debug y los datos suelen estar en la raíz del proyecto.
        /// </summary>
        private string GetPath(string fileName)
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;

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
                Debug.WriteLine($"Error cargando usuarios: {ex.Message}");
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
                Debug.WriteLine($"Error cargando productos: {ex.Message}");
                return new List<Product>();
            }
        }

        public List<Client> LoadClients()
        {
            string finalPath = GetPath("clients.xml");

            if (finalPath == null)
            {
                MessageBox.Show("No se encontró el archivo 'data/clients.xml'.\nVerifica que la carpeta 'data' existe en la raíz del proyecto.",
                                "Archivo de Datos Ausente", MessageBoxButton.OK, MessageBoxImage.Warning);
                return new List<Client>();
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ClientListWrapper));
                using (StreamReader reader = new StreamReader(finalPath))
                {
                    var wrapper = (ClientListWrapper)serializer.Deserialize(reader);
                    return wrapper.Clients;
                }
            }
            catch (Exception ex)
            {
                string errorDetalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;

                MessageBox.Show($"Error de formato en 'clients.xml'.\nDetalle: {errorDetalle}",
                                "Error de Parseo XML", MessageBoxButton.OK, MessageBoxImage.Error);

                Debug.WriteLine("Stack trace clientes: " + ex.ToString());
                return new List<Client>();
            }
        }

        public List<Pedido> LoadPedidos()
        {
            string finalPath = GetPath("pedidos.xml");

            if (finalPath == null)
            {
                // Si no hay pedidos, simplemente empezamos de cero sin alertar
                return new List<Pedido>();
            }

            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PedidoListWrapper));
                using (StreamReader reader = new StreamReader(finalPath))
                {
                    var wrapper = (PedidoListWrapper)serializer.Deserialize(reader);
                    return wrapper.Pedidos;
                }
            }
            catch (Exception ex)
            {
                string errorDetalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                MessageBox.Show($"Error leyendo 'pedidos.xml':\n{errorDetalle}", "Error XML", MessageBoxButton.OK, MessageBoxImage.Error);
                return new List<Pedido>();
            }
        }

        /// <summary>
        /// Actualiza la fecha de último acceso.
        /// </summary>
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
                    Debug.WriteLine($"Fallo al guardar timestamp: {ex.Message}");
                }
            }
        }
    }
}