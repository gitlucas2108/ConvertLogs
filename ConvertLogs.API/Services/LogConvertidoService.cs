using ConvertLogs.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ConvertLogs.API.Services
{
    public class LogConvertidoService
    {
        public virtual LogConvertido Converter(LogOrigem origem)
        {
            try
            {
                return new LogConvertido
                {
                    HttpMethod = origem.HttpMethod,
                    StatusCode = origem.StatusCode,
                    UriPath = origem.UriPath,
                    TimeTaken = (int)Math.Round(origem.TimeTaken), // Converte para inteiro
                    ResponseSize = origem.ResponseSize,
                    CacheStatus = ConverterCacheStatus(origem.CacheStatus),
                    Provider  = "MINHA CDN"
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [LogConvertidoService.Converter] Erro: {ex.Message}");
            }            
        }
        private string ConverterCacheStatus(string cacheStatus)
        {
            try
            {
                switch (cacheStatus)
                {
                    case "HIT":
                        return "HIT";
                    case "MISS":
                        return "MISS";
                    case "INVALIDATE":
                        return "REFRESH_HIT";
                    default:
                        return cacheStatus;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [LogConvertidoService.ConverterCacheStatus] Erro: {ex.Message}");
            }            
        }
    }
}