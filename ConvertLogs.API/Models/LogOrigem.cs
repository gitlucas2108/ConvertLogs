using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvertLogs.API.Models
{
    public class LogOrigem
    {
        public int Id { get; set; } // Identificador único
        public int ResponseSize { get; set; }
        public int StatusCode { get; set; }
        public string CacheStatus { get; set; }
        public string HttpMethod { get; set; }
        public string UriPath { get; set; }
        public double TimeTaken { get; set; }
    }
}