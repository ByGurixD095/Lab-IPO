using AppComida.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace AppComida.Persistence
{
    public class DataAgent
    {
        public List<User> LoadUsers()
        {
            // Busca el archivo users.xml
            string finalPath = GetPath("users.xml");
            
            // Si no existe, devuelve lista vacía para evitar crash
            if (finalPath == null) return new List<User>();

            try
            {
                // Deserialización nativa XML
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

        public String ImagePath(string user)
        {
            string[] imageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

            foreach (string extension in imageExtensions)
            {
                string path = GetPath($"userPic_{user}.{extension}");
                if (path != null)
                    return path;
            }

            return null;
        }

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
    }
}