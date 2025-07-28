using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Concurrent;

namespace CustomerManagement.Tests.TestUtilities;

/// <summary>
/// Mock helpers para facilitar a criação de mocks comuns nos testes
/// </summary>
public static class MockHelpers
{
    /// <summary>
    /// Cria um mock do ILogger configurado para capturar logs
    /// </summary>
    public static Mock<ILogger<T>> CreateLoggerMock<T>()
    {
        return new Mock<ILogger<T>>();
    }

    /// <summary>
    /// Cria um mock do ILogger que permite verificar se mensagens específicas foram logadas
    /// </summary>
    public static MockLogger<T> CreateMockLogger<T>()
    {
        return new MockLogger<T>();
    }

    /// <summary>
    /// Verifica se uma mensagem específica foi logada
    /// </summary>
    public static void VerifyLogMessage<T>(
        Mock<ILogger<T>> mockLogger,
        LogLevel level,
        string message,
        Times times)
    {
        mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times);
    }

    /// <summary>
    /// Verifica se um erro foi logado com a exceção específica
    /// </summary>
    public static void VerifyErrorLogged<T>(
        Mock<ILogger<T>> mockLogger,
        string message,
        Exception exception)
    {
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}

/// <summary>
/// Logger mock que captura todas as mensagens para verificação
/// </summary>
public class MockLogger<T> : ILogger<T>
{
    private readonly ConcurrentBag<LogEntry> _logEntries = new();

    public IReadOnlyList<LogEntry> LogEntries => _logEntries.ToList();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return true;
    }

    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        var message = formatter(state, exception);
        _logEntries.Add(new LogEntry(logLevel, eventId, message, exception));
    }

    /// <summary>
    /// Verifica se uma mensagem específica foi logada
    /// </summary>
    public bool HasLogEntry(LogLevel level, string messageContains)
    {
        return _logEntries.Any(entry => 
            entry.Level == level && 
            entry.Message.Contains(messageContains, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Verifica se uma exceção específica foi logada
    /// </summary>
    public bool HasException<TException>() where TException : Exception
    {
        return _logEntries.Any(entry => entry.Exception is TException);
    }

    /// <summary>
    /// Limpa todas as entradas de log
    /// </summary>
    public void Clear()
    {
        _logEntries.Clear();
    }

    /// <summary>
    /// Obtém todas as mensagens de um nível específico
    /// </summary>
    public IEnumerable<string> GetMessages(LogLevel level)
    {
        return _logEntries
            .Where(entry => entry.Level == level)
            .Select(entry => entry.Message);
    }
}

/// <summary>
/// Representa uma entrada de log capturada
/// </summary>
public record LogEntry(
    LogLevel Level,
    EventId EventId,
    string Message,
    Exception? Exception);

/// <summary>
/// Helper para criar HttpClient mock com respostas pré-configuradas
/// </summary>
public static class HttpClientMockHelper
{
    /// <summary>
    /// Cria um HttpClient mock que retorna a resposta especificada
    /// </summary>
    public static HttpClient CreateMockHttpClient(
        HttpStatusCode statusCode,
        string content,
        string contentType = "application/json")
    {
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("*")
                  .Respond(statusCode, contentType, content);

        return new HttpClient(mockHandler);
    }

    /// <summary>
    /// Cria um HttpClient mock que simula timeout
    /// </summary>
    public static HttpClient CreateTimeoutHttpClient()
    {
        var mockHandler = new MockHttpMessageHandler();
        mockHandler.When("*")
                  .Respond(async () =>
                  {
                      await Task.Delay(TimeSpan.FromSeconds(10));
                      return new HttpResponseMessage(HttpStatusCode.OK);
                  });

        return new HttpClient(mockHandler)
        {
            Timeout = TimeSpan.FromSeconds(1)
        };
    }
}

/// <summary>
/// Handler mock para HttpClient
/// </summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, Func<HttpRequestMessage, Task<HttpResponseMessage>>> _responses = new();
    private readonly List<HttpRequestMessage> _requests = new();

    public IReadOnlyList<HttpRequestMessage> Requests => _requests.AsReadOnly();

    public MockHttpMessageHandler When(string url)
    {
        _currentUrl = url;
        return this;
    }

    public void Respond(HttpStatusCode statusCode, string contentType, string content)
    {
        if (_currentUrl == null) throw new InvalidOperationException("Call When() first");

        _responses[_currentUrl] = _ => Task.FromResult(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, System.Text.Encoding.UTF8, contentType)
        });
    }

    public void Respond(Func<HttpRequestMessage, Task<HttpResponseMessage>> responseFunc)
    {
        if (_currentUrl == null) throw new InvalidOperationException("Call When() first");

        _responses[_currentUrl] = responseFunc;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _requests.Add(request);

        var url = request.RequestUri?.ToString() ?? "";
        
        // Procurar por match exato primeiro
        if (_responses.TryGetValue(url, out var response))
        {
            return await response(request);
        }

        // Procurar por wildcard
        if (_responses.TryGetValue("*", out var wildcardResponse))
        {
            return await wildcardResponse(request);
        }

        // Retornar 404 se não encontrar match
        return new HttpResponseMessage(HttpStatusCode.NotFound);
    }

    private string? _currentUrl;
}

/// <summary>
/// Helpers para configuração de ambiente de teste
/// </summary>
public static class TestEnvironmentHelper
{
    /// <summary>
    /// Configura variáveis de ambiente para testes
    /// </summary>
    public static void SetTestEnvironmentVariables()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Test");
        Environment.SetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULT", "InMemory");
        Environment.SetEnvironmentVariable("LOGGING__LOGLEVEL__DEFAULT", "Warning");
    }

    /// <summary>
    /// Limpa variáveis de ambiente após os testes
    /// </summary>
    public static void CleanupTestEnvironmentVariables()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", null);
        Environment.SetEnvironmentVariable("CONNECTIONSTRINGS__DEFAULT", null);
        Environment.SetEnvironmentVariable("LOGGING__LOGLEVEL__DEFAULT", null);
    }

    /// <summary>
    /// Cria configuração de teste
    /// </summary>
    public static Dictionary<string, string?> CreateTestConfiguration()
    {
        return new Dictionary<string, string?>
        {
            ["ConnectionStrings:Default"] = "Data Source=:memory:",
            ["ApiConfiguration:BaseUrl"] = "http://localhost:5000",
            ["ApiConfiguration:Timeout"] = "00:00:30",
            ["ApiConfiguration:ApiKey"] = "test-api-key",
            ["Logging:LogLevel:Default"] = "Warning",
            ["Caching:DefaultExpiration"] = "00:05:00",
            ["Features:EnableCaching"] = "true",
            ["Features:EnableRetry"] = "true"
        };
    }
}

/// <summary>
/// Helper para criação de claims de usuário para testes
/// </summary>
public static class TestUserHelper
{
    /// <summary>
    /// Cria claims principal para usuário administrador
    /// </summary>
    public static System.Security.Claims.ClaimsPrincipal CreateAdminUser()
    {
        var claims = new[]
        {
            new System.Security.Claims.Claim("sub", Guid.NewGuid().ToString()),
            new System.Security.Claims.Claim("name", "Test Admin"),
            new System.Security.Claims.Claim("email", "admin@test.com"),
            new System.Security.Claims.Claim("role", "Admin")
        };

        return new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(claims, "test"));
    }

    /// <summary>
    /// Cria claims principal para usuário comum
    /// </summary>
    public static System.Security.Claims.ClaimsPrincipal CreateRegularUser()
    {
        var claims = new[]
        {
            new System.Security.Claims.Claim("sub", Guid.NewGuid().ToString()),
            new System.Security.Claims.Claim("name", "Test User"),
            new System.Security.Claims.Claim("email", "user@test.com"),
            new System.Security.Claims.Claim("role", "User")
        };

        return new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(claims, "test"));
    }
}

/// <summary>
/// Helper para criação de dados de teste de performance
/// </summary>
public static class PerformanceTestHelper
{
    /// <summary>
    /// Executa uma ação e mede o tempo de execução
    /// </summary>
    public static async Task<(T Result, TimeSpan Duration)> MeasureAsync<T>(Func<Task<T>> action)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var result = await action();
        stopwatch.Stop();
        return (result, stopwatch.Elapsed);
    }

    /// <summary>
    /// Executa múltiplas ações em paralelo e mede o tempo total
    /// </summary>
    public static async Task<(T[] Results, TimeSpan Duration)> MeasureConcurrentAsync<T>(
        Func<Task<T>>[] actions)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var results = await Task.WhenAll(actions.Select(action => action()));
        stopwatch.Stop();
        return (results, stopwatch.Elapsed);
    }

    /// <summary>
    /// Verifica se uma operação atende aos critérios de performance
    /// </summary>
    public static void AssertPerformance(
        TimeSpan actualDuration,
        TimeSpan maxExpectedDuration,
        string operationName)
    {
        if (actualDuration > maxExpectedDuration)
        {
            throw new AssertionException(
                $"Performance test failed for {operationName}. " +
                $"Expected: <= {maxExpectedDuration.TotalMilliseconds}ms, " +
                $"Actual: {actualDuration.TotalMilliseconds}ms");
        }
    }
}

/// <summary>
/// Exceção customizada para falhas de assertion
/// </summary>
public class AssertionException : Exception
{
    public AssertionException(string message) : base(message) { }
    public AssertionException(string message, Exception innerException) : base(message, innerException) { }
}
