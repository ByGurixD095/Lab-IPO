using System;

namespace AppComida.Domain
{
    public class User
    {
        public string username { get; set; }
        public string firstname { get; set; }
        public string lastname { get; set; }
        public string email { get; set; }
        public DateTime last_access { get; set; }
        public string image { get; set; }
        public string salt { get; set; }
        public string digest { get; set; }
    }
}