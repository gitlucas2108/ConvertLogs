using ConvertLogs.API.Models;
using System;
using System.IO;

namespace ConvertLogs.API.Services
{
    public class ArmazenamentoArquivosService
    {
        public virtual string SalvarArquivoLog(LogConvertido logConvertido)
        {
            try
            {
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), "logs", $"{Guid.NewGuid()}.txt");
                var logText = $"#Version: 1.0\n#Date: {DateTime.UtcNow:dd/MM/yyyy HH:mm:ss}\n#Fields: provider http-method status-code uri-path time-taken response-size cache-status\n" +
                              $"\"{logConvertido.Provider}\" {logConvertido.HttpMethod} {logConvertido.StatusCode} {logConvertido.UriPath} {logConvertido.TimeTaken} {logConvertido.ResponseSize} {logConvertido.CacheStatus}";

                // Criação do diretório caso não exista
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Salvando o conteúdo no arquivo
                System.IO.File.WriteAllText(filePath, logText);

                return filePath; // Retorna o caminho do arquivo salvo
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro em [FileStorageService.SaveLogToFile]: {ex.Message}");
            }
        }
    }
}