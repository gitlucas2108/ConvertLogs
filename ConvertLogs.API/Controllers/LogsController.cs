using ConvertLogs.API.Data;
using ConvertLogs.API.Models;
using ConvertLogs.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ConvertLogs.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogsController : ControllerBase
    {
        private readonly ConvertLogsDbContext _context;
        private readonly LogConvertidoService _logConvertidoService;
        private readonly ArmazenamentoArquivosService _armazenamentoArquivosService;

        public LogsController(ConvertLogsDbContext context, LogConvertidoService logConvertidoService, ArmazenamentoArquivosService armazenamentoArquivosService)
        {
            _context = context;
            _logConvertidoService = logConvertidoService;
            _armazenamentoArquivosService = armazenamentoArquivosService;
        }

        // Endpoint para buscar logs MINHA CDN salvos;
        [HttpGet]
        public async Task<IActionResult> GetLogs()
        {
            try
            {
                var logs = await _context.LogsOrigem.ToListAsync();
                return Ok(logs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro em [LogsController.GetLogs] Erro: {ex.Message}");
            }
        }

        // Endpoint para buscar um log MINHA CDN por ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetLogById(int id)
        {
            try
            {
                var log = await _context.FindLogByIdAsync(id); // Usando o novo método
                if (log == null) return NotFound();
                return Ok(log);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro em [LogsController.GetLogById] Erro: {ex.Message}");
            }
        }

        // Endpoint para salvar logs MINHA CDN
        [HttpPost]
        public async Task<IActionResult> SaveLog([FromBody] LogOrigem log)
        {
            try
            {
                if (log == null) return BadRequest("Log inválido");

                // Adiciona o log no banco de dados
                await _context.LogsOrigem.AddAsync(log);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetLogById), new { id = log.Id }, log);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro em [LogsController.SaveLog] Erro: {ex.Message}");
            }
        }       

        // Endpoint para buscar um log transformado (do formato "MINHA CDN" para "Agora")
        //Na pratica busca o log MINHA CDN, converte e retorna na versão "AGORA"
        [HttpGet("transform/{id}")]
        public async Task<IActionResult> GetTransformedLogById(int id)
        {
            try
            {
                var logOrigem = await _context.LogsOrigem.FindAsync(id);
                if (logOrigem == null) return NotFound();
                                
                var logConvertido = _logConvertidoService.Converter(logOrigem);                

                return Ok(logConvertido);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro em [LogsController.GetTransformedLogById] Erro: {ex.Message}");
            }
        }

        // Endpoint para transformar um log no formato "Agora" e salvar em um arquivo
        [HttpPost("transform")]
        public async Task<IActionResult> TransformLog([FromBody] LogOrigem logOrigem)
        {
            try
            {
                if (logOrigem == null)
                {
                    return BadRequest("Log inválido");
                }               
                
                var logConvertido = _logConvertidoService.Converter(logOrigem);                
                var filePath = _armazenamentoArquivosService.SalvarArquivoLog(logConvertido);

                return Ok(new { logConvertido, filePath });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro em [LogsController.TransformLog] Erro: {ex.Message}");
            }
        }


        [HttpGet("transformed")]
        public async Task<IActionResult> GetTransformedLogs()
        {
            try
            {
                var logsOrigem = await _context.LogsOrigem.ToListAsync();
                if (!logsOrigem.Any())
                {
                    return NotFound("Nenhum log encontrado.");
                }

                var logsTransformados = logsOrigem.Select(log => _logConvertidoService.Converter(log)).ToList();

                return Ok(new
                {
                    LogsOrigem = logsOrigem,
                    LogsTransformados = logsTransformados
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro em [LogsController.GetTransformedLogs] Erro: {ex.Message}");
            }
        }

    }
}
