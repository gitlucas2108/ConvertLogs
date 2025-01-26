using ConvertLogs.API.Controllers;
using ConvertLogs.API.Data;
using ConvertLogs.API.Models;
using ConvertLogs.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ConvertLogs.Tests
{
    public class LogsControllerTests
    {
        //xUnit por padrão não exibe o datalhamento incorporado no teste, usando ITestOutputHelper para colaborar nessa tarefa
        private readonly ITestOutputHelper _output;

        public LogsControllerTests(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task GetLogs_ReturnsOkResult_WithListOfLogs()
        {
            _output.WriteLine("Iniciando teste GetLogs_ReturnsOkResult_WithListOfLogs...");

            //Usando mock do log de MINHA CDN para simular uma requisição http de terceiros: https://s3.amazonaws.com/uux-itaas-static/minha-cdn-logs/input-01.txt
            var mockResponse = @"312|200|HIT|""GET /robots.txt HTTP/1.1""|100.2
                             101|200|MISS|""POST /myImages HTTP/1.1""|319.4
                             199|404|MISS|""GET /not-found HTTP/1.1""|142.9
                             312|200|INVALIDATE|""GET /robots.txt HTTP/1.1""|245.1";

            _output.WriteLine("MockResponse:\n" + mockResponse);

            var logs = new List<LogOrigem>();
            var logLines = mockResponse.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in logLines)
            {
                var parts = line.Split('|');
                var log = new LogOrigem
                {
                    StatusCode = int.Parse(parts[1]),
                    CacheStatus = parts[2],
                    HttpMethod = parts[3].Split(' ')[0].Replace("\"", ""),
                    UriPath = parts[3].Split(' ')[1],
                    ResponseSize = int.TryParse(parts[0], out var responseSize) ? responseSize : 0,
                    TimeTaken = double.TryParse(parts[4], out var timeTaken) ? timeTaken : 0.0
                };
                logs.Add(log);
                _output.WriteLine($"Parsed Log: {log.StatusCode}, {log.CacheStatus}, {log.HttpMethod}, {log.UriPath}, {log.ResponseSize}, {log.TimeTaken}");
            }

            var options = new DbContextOptionsBuilder<ConvertLogsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var context = new ConvertLogsDbContext(options))
            {
                context.LogsOrigem.AddRange(logs);
                context.SaveChanges();
                _output.WriteLine("Logs saved to in-memory database.");
            }

            using (var context = new ConvertLogsDbContext(options))
            {
                var mockLogConvertidoService = new Mock<LogConvertidoService>();
                var mockArmazenamentoArquivosService = new Mock<ArmazenamentoArquivosService>();

                var controller = new LogsController(context, mockLogConvertidoService.Object, mockArmazenamentoArquivosService.Object);

                // Chamada da action GetLogs da API
                var result = await controller.GetLogs();
                _output.WriteLine("GetLogs API called.");

                // Verifica resultado
                var okResult = Assert.IsType<OkObjectResult>(result);
                _output.WriteLine("Result is OkObjectResult.");

                var returnLogs = Assert.IsType<List<LogOrigem>>(okResult.Value);
                _output.WriteLine($"Returned Logs Count: {returnLogs.Count}");
                foreach (var log in returnLogs)
                {
                    _output.WriteLine($"Returned Log: {log.StatusCode}, {log.CacheStatus}, {log.HttpMethod}, {log.UriPath}, {log.ResponseSize}, {log.TimeTaken}");
                }

                // Valida a quantidade
                Assert.Equal(4, returnLogs.Count);
            }
        }

        [Fact]
        public async Task GetLogById_ReturnsOkResult_WhenLogExists()
        {
            var options = new DbContextOptionsBuilder<ConvertLogsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var context = new ConvertLogsDbContext(options))
            {
                //Ações para evitar erro no teste na tentativa de incluir um ID já existente
                context.Database.EnsureDeleted(); // Remove todos os dados anteriores
                context.Database.EnsureCreated(); // Cria um novo banco vazio

                var log = new LogOrigem { Id = 1, StatusCode = 200, CacheStatus = "HIT", HttpMethod = "GET", UriPath = "/test", ResponseSize = 312, TimeTaken = 100.2 };
                context.LogsOrigem.Add(log);
                context.SaveChanges();
            }

            using (var context = new ConvertLogsDbContext(options))
            {
                var mockLogConvertidoService = new Mock<LogConvertidoService>();
                var mockArmazenamentoArquivosService = new Mock<ArmazenamentoArquivosService>();

                var controller = new LogsController(context, mockLogConvertidoService.Object, mockArmazenamentoArquivosService.Object);

                var result = await controller.GetLogById(1);

                var okResult = Assert.IsType<OkObjectResult>(result);
                var returnLog = Assert.IsType<LogOrigem>(okResult.Value);

                Assert.Equal(1, returnLog.Id);
                Assert.Equal(200, returnLog.StatusCode);

                _output.WriteLine($"Test Passed (GetLogById_200) - Found Log with ID: {returnLog.Id}");
            }
        }

        [Fact]
        public async Task GetLogById_ReturnsNotFound_WhenLogDoesNotExist()
        {
            var options = new DbContextOptionsBuilder<ConvertLogsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;

            using (var context = new ConvertLogsDbContext(options))
            {
                var mockLogConvertidoService = new Mock<LogConvertidoService>();
                var mockArmazenamentoArquivosService = new Mock<ArmazenamentoArquivosService>();

                var controller = new LogsController(context, mockLogConvertidoService.Object, mockArmazenamentoArquivosService.Object);

                var result = await controller.GetLogById(999); // ID inexistente

                Assert.IsType<NotFoundResult>(result);

                _output.WriteLine("Test Passed (GetLogById_404) - NotFound returned for non-existent ID.");
            }
        }

        [Fact]
        public async Task GetLogById_Returns500_WhenExceptionOccurs()
        {
            // Configura um banco de dados em memória
            var options = new DbContextOptionsBuilder<ConvertLogsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_Exception")
                .Options;

            // Usa uma classe que força erro ao acessar LogsOrigem
            using (var context = new FailingConvertLogsDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var mockLogConvertidoService = new Mock<LogConvertidoService>();
                var mockArmazenamentoArquivosService = new Mock<ArmazenamentoArquivosService>();

                var failingController = new LogsController(context, mockLogConvertidoService.Object, mockArmazenamentoArquivosService.Object);

                var result = await failingController.GetLogById(1);

                var objectResult = Assert.IsType<ObjectResult>(result);
                Assert.Equal(500, objectResult.StatusCode);
                Assert.Contains("Erro em [LogsController.GetLogById]", objectResult.Value.ToString());

                _output.WriteLine("Test Passed (GetLogById_500) - Internal Server Error simulated.");
            }
        }

        [Fact]
        public async Task SaveLog_ReturnsCreatedAtAction_WhenLogIsValid()
        {
            // Configura o banco de dados em memória
            var options = new DbContextOptionsBuilder<ConvertLogsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_SaveLog_Valid")
                .Options;

            using (var context = new ConvertLogsDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var log = new LogOrigem
                {
                    StatusCode = 200,
                    CacheStatus = "HIT",
                    HttpMethod = "GET",
                    UriPath = "/robots.txt",
                    ResponseSize = 100,
                    TimeTaken = 0.1
                };

                var mockLogConvertidoService = new Mock<LogConvertidoService>();
                var mockArmazenamentoArquivosService = new Mock<ArmazenamentoArquivosService>();
                var controller = new LogsController(context, mockLogConvertidoService.Object, mockArmazenamentoArquivosService.Object);

                var result = await controller.SaveLog(log);

                var createdResult = Assert.IsType<CreatedAtActionResult>(result);
                Assert.Equal(201, createdResult.StatusCode);
                var returnedLog = Assert.IsType<LogOrigem>(createdResult.Value);
                Assert.Equal(log.StatusCode, returnedLog.StatusCode);
                Assert.Equal(log.CacheStatus, returnedLog.CacheStatus);

                _output.WriteLine("Test Passed (SaveLog_Valid) - Log successfully saved.");
            }
        }

        [Fact]
        public async Task SaveLog_ReturnsBadRequest_WhenLogIsNull()
        {
            // Configura o banco de dados em memória
            var options = new DbContextOptionsBuilder<ConvertLogsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_SaveLog_Invalid")
                .Options;

            using (var context = new ConvertLogsDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var mockLogConvertidoService = new Mock<LogConvertidoService>();
                var mockArmazenamentoArquivosService = new Mock<ArmazenamentoArquivosService>();
                var controller = new LogsController(context, mockLogConvertidoService.Object, mockArmazenamentoArquivosService.Object);

                var result = await controller.SaveLog(null);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal(400, badRequestResult.StatusCode);
                Assert.Contains("Log inválido", badRequestResult.Value.ToString());

                _output.WriteLine("Test Passed (SaveLog_Invalid) - Null log returned BadRequest.");
            }
        }

        [Fact]
        public async Task SaveLog_Returns500_WhenExceptionOccurs()
        {
            // Configura o banco de dados em memória
            var options = new DbContextOptionsBuilder<ConvertLogsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_SaveLog_Exception")
                .Options;

            using (var context = new ConvertLogsDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var log = new LogOrigem
                {
                    StatusCode = 500,
                    CacheStatus = "MISS",
                    HttpMethod = "POST",
                    UriPath = "/upload",
                    ResponseSize = 1024,
                    TimeTaken = 1.5
                };

                var mockLogConvertidoService = new Mock<LogConvertidoService>();
                var mockArmazenamentoArquivosService = new Mock<ArmazenamentoArquivosService>();

                // Simula uma falha ao adicionar o log
                var controller = new LogsController(new FailingConvertLogsDbContext(options), mockLogConvertidoService.Object, mockArmazenamentoArquivosService.Object);

                var result = await controller.SaveLog(log);

                var objectResult = Assert.IsType<ObjectResult>(result);
                Assert.Equal(500, objectResult.StatusCode);
                Assert.Contains("Erro em [LogsController.SaveLog]", objectResult.Value.ToString());

                _output.WriteLine("Test Passed (SaveLog_Exception) - Internal Server Error simulated.");
            }
        }

        [Fact]
        public async Task GetTransformedLogById_ReturnsOkResult_WhenLogExists()
        {
            // Configura o banco de dados em memória
            var options = new DbContextOptionsBuilder<ConvertLogsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_TransformedLog_Valid")
                .Options;

            using (var context = new ConvertLogsDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                // Adiciona um log de teste
                var logOrigem = new LogOrigem
                {
                    StatusCode = 200,
                    CacheStatus = "HIT",
                    HttpMethod = "GET",
                    UriPath = "/robots.txt",
                    ResponseSize = 100,
                    TimeTaken = 0.1
                };
                context.LogsOrigem.Add(logOrigem);
                await context.SaveChangesAsync();

                var mockLogConvertidoService = new Mock<LogConvertidoService>();
                var mockArmazenamentoArquivosService = new Mock<ArmazenamentoArquivosService>();
                var controller = new LogsController(context, mockLogConvertidoService.Object, mockArmazenamentoArquivosService.Object);

                var result = await controller.GetTransformedLogById(logOrigem.Id);

                var okResult = Assert.IsType<OkObjectResult>(result);
                var transformedLog = Assert.IsType<LogConvertido>(okResult.Value);
                Assert.Equal(logOrigem.StatusCode, transformedLog.StatusCode);
                Assert.Equal(logOrigem.CacheStatus, transformedLog.CacheStatus);

                _output.WriteLine("Test Passed (GetTransformedLogById_Valid) - Log successfully transformed.");
            }
        }

        [Fact]
        public async Task GetTransformedLogById_ReturnsNotFound_WhenLogDoesNotExist()
        {
            // Configura o banco de dados em memória
            var options = new DbContextOptionsBuilder<ConvertLogsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_TransformedLog_NotFound")
                .Options;

            using (var context = new ConvertLogsDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var mockLogConvertidoService = new Mock<LogConvertidoService>();
                var mockArmazenamentoArquivosService = new Mock<ArmazenamentoArquivosService>();
                var controller = new LogsController(context, mockLogConvertidoService.Object, mockArmazenamentoArquivosService.Object);

                // ID que não existe
                var result = await controller.GetTransformedLogById(999);

                var notFoundResult = Assert.IsType<NotFoundResult>(result);

                _output.WriteLine("Test Passed (GetTransformedLogById_NotFound) - Log not found.");
            }
        }

        [Fact]
        public async Task GetTransformedLogById_Returns500_WhenExceptionOccurs()
        {
            // Configura o banco de dados em memória
            var options = new DbContextOptionsBuilder<ConvertLogsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_TransformedLog_Exception")
                .Options;

            using (var context = new ConvertLogsDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var logOrigem = new LogOrigem
                {
                    StatusCode = 200,
                    CacheStatus = "HIT",
                    HttpMethod = "GET",
                    UriPath = "/robots.txt",
                    ResponseSize = 100,
                    TimeTaken = 0.1
                };
                context.LogsOrigem.Add(logOrigem);
                await context.SaveChangesAsync();

                // Subclasse do LogConvertidoService para simular exceção
                var failingLogConvertidoService = new FailingLogConvertidoService();

                var mockArmazenamentoArquivosService = new Mock<ArmazenamentoArquivosService>();

                // Instanciando o controlador com o serviço customizado
                var controller = new LogsController(context, failingLogConvertidoService, mockArmazenamentoArquivosService.Object);

                var result = await controller.GetTransformedLogById(logOrigem.Id);

                var objectResult = Assert.IsType<ObjectResult>(result);
                Assert.Equal(500, objectResult.StatusCode);
                Assert.Contains("Erro em [LogsController.GetTransformedLogById]", objectResult.Value.ToString());

                _output.WriteLine("Test Passed (GetTransformedLogById_Exception) - Internal Server Error simulated.");
            }
        }


        [Fact]
        public async Task TransformLog_ReturnsOk_WhenLogIsValid()
        {
            // Configura o banco de dados em memória
            var options = new DbContextOptionsBuilder<ConvertLogsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_TransformLog_Success")
                .Options;

            using (var context = new ConvertLogsDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var logOrigem = new LogOrigem
                {
                    StatusCode = 200,
                    CacheStatus = "HIT",
                    HttpMethod = "GET",
                    UriPath = "/robots.txt",
                    ResponseSize = 100,
                    TimeTaken = 0.1
                };
                context.LogsOrigem.Add(logOrigem);
                await context.SaveChangesAsync();

                // Mock do serviço LogConvertidoService
                var mockLogConvertidoService = new Mock<LogConvertidoService>();
                mockLogConvertidoService.Setup(s => s.Converter(It.IsAny<LogOrigem>())).Returns(new LogConvertido
                {
                    HttpMethod = logOrigem.HttpMethod,
                    StatusCode = logOrigem.StatusCode,
                    UriPath = logOrigem.UriPath,
                    TimeTaken = (int)Math.Round(logOrigem.TimeTaken), // Converte para inteiro
                    ResponseSize = logOrigem.ResponseSize,
                    CacheStatus = "MISS",  // Valor fixo para teste apenas
                    Provider = "MINHA CDN"
                });

                // Mock do serviço ArmazenamentoArquivosService
                var mockArmazenamentoArquivosService = new Mock<ArmazenamentoArquivosService>();
                mockArmazenamentoArquivosService.Setup(s => s.SalvarArquivoLog(It.IsAny<LogConvertido>())).Returns("C:\\Logs\\logConvertido.txt");

                // Controller com os serviços mockados
                var controller = new LogsController(context, mockLogConvertidoService.Object, mockArmazenamentoArquivosService.Object);

                // Executando a ação
                var result = await controller.TransformLog(logOrigem);

                var okResult = Assert.IsType<OkObjectResult>(result);
                Assert.Equal(200, okResult.StatusCode);

                var response = okResult.Value as dynamic;
                Assert.NotNull(response.logConvertido);
                Assert.Equal("C:\\Logs\\logConvertido.txt", response.filePath);

                _output.WriteLine("Test Passed (TransformLog_Success) - Log convertido e arquivo salvo com sucesso.");
            }
        }


        [Fact]
        public async Task TransformLog_ReturnsBadRequest_WhenLogIsNull()
        {
            // Configura o banco de dados em memória
            var options = new DbContextOptionsBuilder<ConvertLogsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_TransformLog_BadRequest")
                .Options;

            using (var context = new ConvertLogsDbContext(options))
            {

                // Mock dos serviços
                var mockLogConvertidoService = new Mock<LogConvertidoService>();
                var mockArmazenamentoArquivosService = new Mock<ArmazenamentoArquivosService>();

                var controller = new LogsController(context, mockLogConvertidoService.Object, mockArmazenamentoArquivosService.Object);

                // Enviar um logOrigem nulo
                var result = await controller.TransformLog(null);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result); 
                Assert.Equal(400, badRequestResult.StatusCode);
                Assert.Equal("Log inválido", badRequestResult.Value);
            }
        }

        [Fact]
        public async Task TransformLog_Returns500_WhenExceptionOccurs()
        {
            // Configura o banco de dados em memória
            var options = new DbContextOptionsBuilder<ConvertLogsDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb_TransformLog_Exception")
                .Options;

            using (var context = new ConvertLogsDbContext(options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var logOrigem = new LogOrigem
                {
                    StatusCode = 200,
                    CacheStatus = "HIT",
                    HttpMethod = "GET",
                    UriPath = "/robots.txt",
                    ResponseSize = 100,
                    TimeTaken = 0.1
                };
                context.LogsOrigem.Add(logOrigem);
                await context.SaveChangesAsync();

                // Mock do serviço LogConvertidoService que lança exceção
                var mockLogConvertidoService = new Mock<LogConvertidoService>();
                mockLogConvertidoService.Setup(s => s.Converter(It.IsAny<LogOrigem>())).Throws(new Exception("Simulated Exception"));

                // Mock do serviço ArmazenamentoArquivosService
                var mockArmazenamentoArquivosService = new Mock<ArmazenamentoArquivosService>();

                // Controller com os serviços mockados
                var controller = new LogsController(context, mockLogConvertidoService.Object, mockArmazenamentoArquivosService.Object);

                // Executando a ação
                var result = await controller.TransformLog(logOrigem);

                var objectResult = Assert.IsType<ObjectResult>(result);
                Assert.Equal(500, objectResult.StatusCode);
                Assert.Contains("Erro em [LogsController.TransformLog]", objectResult.Value.ToString());

                _output.WriteLine("Test Passed (TransformLog_Exception) - Internal Server Error simulated.");
            }
        }


        // Classe que herda de ConvertLogsDbContext e força erro ao chamar FindAsync
        public class FailingConvertLogsDbContext : ConvertLogsDbContext
        {
            public FailingConvertLogsDbContext(DbContextOptions<ConvertLogsDbContext> options)
         : base(options) { }

            public override async Task<LogOrigem> FindLogByIdAsync(int id)
            {
                throw new Exception("Simulated Exception");
            }

            public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
            {
                throw new Exception("Simulated Exception");
            }
        }

        // Implementação personalizada do LogConvertidoService para simular a exceção
        public class FailingLogConvertidoService : LogConvertidoService
        {
            public override LogConvertido Converter(LogOrigem logOrigem)
            {
                throw new Exception("Simulated Exception");
            }
        }
    }
}