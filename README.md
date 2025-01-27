ConvertLogs

ConvertLogs é uma API REST desenvolvida em **.NET Core 2.1** para conversão de logs do formato **"MINHA CDN"** para o formato **"Agora"**. Além disso, a API armazena logs no banco de dados e permite salvá-los em arquivos `.txt`.

🚀 Tecnologias Utilizadas

- **.NET Core 2.1**
- **Entity Framework Core**
- **SQL Server**
- **xUnit** (Testes automatizados)
- **Moq** (Mocking para testes)
- **InMemoryDatabase** (Banco de dados em memória para testes)

---

📥 Instalação e Execução

**Pré-requisitos**
- Visual Studio 2019
- SQL Server
- .NET Core 2.1 SDK

**1️⃣ Clonando o Repositório**
```sh
git clone https://github.com/seu-usuario/ConvertLogs.git
cd ConvertLogs

2️⃣ Configurando a String de Conexão
No arquivo appsettings.json, configure a string de conexão para o SQL Server:
"ConnectionStrings": {
  "DefaultConnection": "Server=192.168.70.91;Database=ConvertLogsDB;User Id=convert;Password=convert@2025;"
}

3️⃣ Executando a Aplicação
dotnet run --project ConvertLogs.API

A API estará rodando em:
🔗 http://localhost:5000/api/logs

🗄️ Estrutura do Banco de Dados
Tabela LogsOrigem (Formato "MINHA CDN")
Campo	Tipo	Descrição
Id	int	Identificador único
HttpMethod	string	Método HTTP (GET, POST, etc.)
StatusCode	int	Código de status da resposta
UriPath	string	Caminho do recurso acessado
TimeTaken	double	Tempo de resposta da requisição
ResponseSize	int	Tamanho da resposta em bytes
CacheStatus	string	Status do cache (HIT/MISS/etc.)

📌 Endpoints da API
1️⃣ Buscar todos os logs salvos ("MINHA CDN"): GET /api/logs
Resposta Exemplo
[
  {
    "id": 1,
    "httpMethod": "GET",
    "statusCode": 200,
    "uriPath": "/robots.txt",
    "timeTaken": 0.1,
    "responseSize": 100,
    "cacheStatus": "HIT"
  }
]

2️⃣ Buscar um log pelo ID ("MINHA CDN"):GET /api/logs/{id}
Resposta Exemplo
{
  "id": 1,
  "httpMethod": "GET",
  "statusCode": 200,
  "uriPath": "/robots.txt",
  "timeTaken": 0.1,
  "responseSize": 100,
  "cacheStatus": "HIT"
}

3️⃣ Salvar um log no formato "MINHA CDN": POST /api/logs
Body Exemplo
{
  "httpMethod": "GET",
  "statusCode": 200,
  "uriPath": "/robots.txt",
  "timeTaken": 0.1,
  "responseSize": 100,
  "cacheStatus": "HIT"
}
Resposta Exemplo
{
  "id": 2,
  "httpMethod": "GET",
  "statusCode": 200,
  "uriPath": "/robots.txt",
  "timeTaken": 0.1,
  "responseSize": 100,
  "cacheStatus": "HIT"
}

4️⃣ Buscar um log transformado pelo ID ("Agora"): GET /api/logs/transform/{id}
Resposta Exemplo
{
  "httpMethod": "GET",
  "statusCode": 200,
  "uriPath": "/robots.txt",
  "timeTaken": 0,
  "responseSize": 100,
  "cacheStatus": "MISS",
  "provider": "MINHA CDN"
}

5️⃣ Buscar todos os logs transformados: GET /api/logs/transformed
Resposta Exemplo
{
  "LogsOrigem": [
    {
      "id": 1,
      "httpMethod": "GET",
      "statusCode": 200,
      "uriPath": "/robots.txt",
      "timeTaken": 0.1,
      "responseSize": 100,
      "cacheStatus": "HIT"
    }
  ],
  "LogsTransformados": [
    {
      "httpMethod": "GET",
      "statusCode": 200,
      "uriPath": "/robots.txt",
      "timeTaken": 0,
      "responseSize": 100,
      "cacheStatus": "MISS",
      "provider": "MINHA CDN"
    }
  ]
}

6️⃣ Converter um log e salvar em um arquivo: POST /api/logs/transform
Body Exemplo
{
  "httpMethod": "GET",
  "statusCode": 200,
  "uriPath": "/robots.txt",
  "timeTaken": 0.1,
  "responseSize": 100,
  "cacheStatus": "HIT"
}
Resposta Exemplo
{
  "logConvertido": {
    "httpMethod": "GET",
    "statusCode": 200,
    "uriPath": "/robots.txt",
    "timeTaken": 0,
    "responseSize": 100,
    "cacheStatus": "MISS",
    "provider": "MINHA CDN"
  },
  "filePath": "C:\\Logs\\logConvertido.txt"
}


✅ Executando os Testes Automatizados
1️⃣ Rodar todos os testes
  dotnet test ConvertLogs.Tests

2️⃣ Testes Implementados
✅ SaveLog_ReturnsCreated_WhenLogIsValid
✅ SaveLog_ReturnsBadRequest_WhenLogIsNull
✅ SaveLog_Returns500_WhenExceptionOccurs
✅ GetTransformedLogById_ReturnsOk_WhenLogExists
✅ GetTransformedLogById_Returns404_WhenLogDoesNotExist
✅ GetTransformedLogById_Returns500_WhenExceptionOccurs
✅ TransformLog_ReturnsOk_WhenLogIsValid
✅ TransformLog_ReturnsBadRequest_WhenLogIsNull

🏗️ Estrutura do Projeto
ConvertLogs/
│── ConvertLogs.API/            # API principal
│── ConvertLogs.API/Data/       # Contexto do banco de dados
│── ConvertLogs.API/Models/     # Modelos de dados
│── ConvertLogs.API/Services/   # Serviços de conversão e armazenamento
│── ConvertLogs.API/Controllers/ # Controladores da API
│── ConvertLogs.Tests/          # Testes unitários e de integração
│── README.md                   # Documentação do projeto

📜 Licença
Este projeto é open-source e está licenciado sob a MIT License.
MIT License © 2025 ConvertLogs



