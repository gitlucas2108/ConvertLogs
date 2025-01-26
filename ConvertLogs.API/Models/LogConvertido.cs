using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvertLogs.API.Models
{
    public class LogConvertido
    {
        public int Id { get; set; }
        public string Provider { get; set; } = "MINHA CDN"; // considerar  que só existe este formato de entrada
        public string HttpMethod { get; set; }
        public int StatusCode { get; set; }
        public string UriPath { get; set; }
        public int TimeTaken { get; set; } // Armazenado como int (arredondado)
        public int ResponseSize { get; set; }
        public string CacheStatus { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // Data de criação
    }
}
