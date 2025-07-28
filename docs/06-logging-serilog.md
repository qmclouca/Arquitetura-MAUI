# 6. Logging com Serilog

## Introdução ao Serilog

O Serilog é um framework de logging estruturado para .NET que oferece alta performance e flexibilidade para capturar, estruturar e analisar logs da aplicação.

## Configuração do Serilog

### 6.1 Instalação de Pacotes

```xml
<!-- Adicionar ao projeto MAUI -->
<PackageReference Include="Serilog" Version="3.1.1" />
<PackageReference Include="Serilog.Extensions.Hosting" Version="8.0.0" />
<PackageReference Include="Serilog.Extensions.Logging" Version="8.0.0" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.0.1" />
<PackageReference Include="Serilog.Sinks.File" Version="5.0.0" />
<PackageReference Include="Serilog.Sinks.Debug" Version="2.0.0" />
<PackageReference Include="Serilog.Enrichers.Thread" Version="3.1.0" />
<PackageReference Include="Serilog.Enrichers.Process" Version="2.0.2" />
<PackageReference Include="Serilog.Enrichers.Environment" Version="2.3.0" />
<PackageReference Include="Serilog.Settings.Configuration" Version="8.0.0" />
```

### 6.2 Configuração Inicial

```csharp
// MauiProgram.cs
using Serilog;
using Serilog.Events;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Configurar Serilog antes de criar o builder
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .Enrich.WithProcessName()
            .Enrich.WithEnvironmentName()
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
            .WriteTo.Debug()
#if !DEBUG
            .WriteTo.File(
                Path.Combine(FileSystem.AppDataDirectory, "logs", "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 10 * 1024 * 1024, // 10MB
                rollOnFileSizeLimit: true,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
#endif
            .CreateLogger();

        var builder = MauiApp.CreateBuilder();
        
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Adicionar Serilog ao container de DI
        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog();
        });

        // Registrar serviços
        builder.Services.RegisterServices();
        builder.Services.RegisterViewModels();
        builder.Services.RegisterViews();

        var app = builder.Build();

        // Log de inicialização
        Log.Information("Application starting up");

        return app;
    }
}
```

### 6.3 Configuração por Ambiente

```csharp
// Infrastructure/Logging/SerilogConfiguration.cs
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

public static class SerilogConfiguration
{
    public static LoggerConfiguration ConfigureLogger(bool isDevelopment)
    {
        var config = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithThreadId()
            .Enrich.WithProcessId()
            .Enrich.WithProcessName()
            .Enrich.WithMachineName()
            .Enrich.WithEnvironmentUserName();

        if (isDevelopment)
        {
            config.WriteTo.Console(
                theme: AnsiConsoleTheme.Code,
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext:l}: {Message:lj}{NewLine}{Exception}")
                .WriteTo.Debug();
        }
        else
        {
            config.WriteTo.File(
                path: Path.Combine(FileSystem.AppDataDirectory, "logs", "app-.log"),
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                fileSizeLimitBytes: 50 * 1024 * 1024, // 50MB
                rollOnFileSizeLimit: true,
                buffered: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1),
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}");
        }

        return config;
    }

    public static void ConfigureForPerformance(LoggerConfiguration config)
    {
        config.WriteTo.Async(a => a.File(
            path: Path.Combine(FileSystem.AppDataDirectory, "logs", "performance-.log"),
            rollingInterval: RollingInterval.Hour,
            retainedFileCountLimit: 48,
            restrictedToMinimumLevel: LogEventLevel.Information,
            outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff}] {Level:u3}: {Message:lj}{NewLine}"));
    }
}
```

## Enrichers Customizados

### 6.4 User Context Enricher

```csharp
// Infrastructure/Logging/Enrichers/UserContextEnricher.cs
using Serilog.Core;
using Serilog.Events;

public class UserContextEnricher : ILogEventEnricher
{
    private readonly ICurrentUserService _currentUserService;

    public UserContextEnricher(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var user = _currentUserService.GetCurrentUser();
        
        if (user != null)
        {
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserId", user.Id));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserName", user.Name));
            logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("UserRole", user.Role));
        }
    }
}

// Application Context Enricher
public class ApplicationContextEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Application", "MauiApp"));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Version", GetApplicationVersion()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("Platform", DeviceInfo.Platform.ToString()));
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("DeviceType", DeviceInfo.DeviceType.ToString()));
    }

    private static string GetApplicationVersion()
    {
        return AppInfo.VersionString;
    }
}
```

## Logging em Diferentes Camadas

### 6.5 Logging no Domain Layer

```csharp
// Domain/Events/DomainEventHandler.cs
public class CustomerCreatedEventHandler : IDomainEventHandler<CustomerCreatedEvent>
{
    private readonly ILogger<CustomerCreatedEventHandler> _logger;
    private readonly IEmailService _emailService;

    public CustomerCreatedEventHandler(
        ILogger<CustomerCreatedEventHandler> logger,
        IEmailService emailService)
    {
        _logger = logger;
        _emailService = emailService;
    }

    public async Task HandleAsync(CustomerCreatedEvent domainEvent)
    {
        using var scope = _logger.BeginScope("CustomerCreated {@DomainEvent}", domainEvent);
        
        _logger.LogInformation("Processing customer created event for customer {CustomerId}", 
                              domainEvent.CustomerId);

        try
        {
            await _emailService.SendWelcomeEmailAsync(
                domainEvent.Email, 
                domainEvent.CustomerName);

            _logger.LogInformation("Welcome email sent successfully to {Email}", 
                                  domainEvent.Email);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send welcome email to {Email}", 
                           domainEvent.Email);
            throw;
        }
    }
}
```

### 6.6 Logging no Application Layer

```csharp
// Application/Services/CustomerService.cs
public class CustomerService : ICustomerService
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<CustomerService> _logger;
    private readonly IMapper _mapper;

    public CustomerService(
        ICustomerRepository customerRepository,
        ILogger<CustomerService> logger,
        IMapper mapper)
    {
        _customerRepository = customerRepository;
        _logger = logger;
        _mapper = mapper;
    }

    public async Task<Result<CustomerDto>> CreateCustomerAsync(CreateCustomerRequest request)
    {
        using var scope = _logger.BeginScope("CreateCustomer {Email}", request.Email);
        
        _logger.LogInformation("Creating new customer with email {Email}", request.Email);

        try
        {
            // Verificar se cliente já existe
            var existingCustomer = await _customerRepository.GetByEmailAsync(request.Email);
            if (existingCustomer != null)
            {
                _logger.LogWarning("Customer with email {Email} already exists", request.Email);
                return Result<CustomerDto>.Failure("Customer already exists");
            }

            // Criar novo cliente
            var customer = new Customer(request.Name, request.Email);
            await _customerRepository.AddAsync(customer);
            
            var customerDto = _mapper.Map<CustomerDto>(customer);
            
            _logger.LogInformation("Customer {CustomerId} created successfully", customer.Id);
            
            return Result<CustomerDto>.Success(customerDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating customer with email {Email}", request.Email);
            return Result<CustomerDto>.Failure("Failed to create customer");
        }
    }

    public async Task<Result<IEnumerable<CustomerDto>>> GetCustomersAsync(CustomerFilter filter)
    {
        var stopwatch = Stopwatch.StartNew();
        
        using var scope = _logger.BeginScope("GetCustomers {@Filter}", filter);
        
        _logger.LogDebug("Retrieving customers with filter");

        try
        {
            var customers = await _customerRepository.GetFilteredAsync(filter);
            var customerDtos = _mapper.Map<IEnumerable<CustomerDto>>(customers);
            
            stopwatch.Stop();
            
            _logger.LogInformation("Retrieved {Count} customers in {ElapsedMs}ms", 
                                  customerDtos.Count(), stopwatch.ElapsedMilliseconds);
            
            return Result<IEnumerable<CustomerDto>>.Success(customerDtos);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _logger.LogError(ex, "Error retrieving customers in {ElapsedMs}ms", 
                           stopwatch.ElapsedMilliseconds);
            
            return Result<IEnumerable<CustomerDto>>.Failure("Failed to retrieve customers");
        }
    }
}
```

### 6.7 Logging no Presentation Layer

```csharp
// ViewModels/CustomerListViewModel.cs
public partial class CustomerListViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerListViewModel> _logger;

    public CustomerListViewModel(
        ICustomerService customerService,
        ILogger<CustomerListViewModel> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        using var scope = _logger.BeginScope("LoadCustomers");
        
        _logger.LogDebug("Starting to load customers");
        
        try
        {
            IsLoading = true;
            
            var result = await _customerService.GetCustomersAsync(new CustomerFilter());
            
            if (result.IsSuccess)
            {
                Customers.Clear();
                foreach (var customer in result.Value)
                {
                    Customers.Add(customer);
                }
                
                _logger.LogInformation("Loaded {Count} customers successfully", 
                                      Customers.Count);
            }
            else
            {
                _logger.LogWarning("Failed to load customers: {Error}", result.Error);
                // Show error message to user
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error loading customers");
            // Show generic error message to user
        }
        finally
        {
            IsLoading = false;
            _logger.LogDebug("Finished loading customers");
        }
    }
}
```

## Logging Estruturado

### 6.8 Structured Logging Examples

```csharp
// Structured Logging Service
public class StructuredLoggingService
{
    private readonly ILogger<StructuredLoggingService> _logger;

    public StructuredLoggingService(ILogger<StructuredLoggingService> logger)
    {
        _logger = logger;
    }

    public void LogUserAction(string userId, string action, object? parameters = null)
    {
        _logger.LogInformation("User action performed: {UserId} executed {Action} with {@Parameters}", 
                              userId, action, parameters);
    }

    public void LogPerformanceMetric(string operation, TimeSpan duration, bool success)
    {
        _logger.LogInformation("Performance metric: {Operation} completed in {Duration}ms - Success: {Success}",
                              operation, duration.TotalMilliseconds, success);
    }

    public void LogBusinessEvent(string eventName, object eventData)
    {
        _logger.LogInformation("Business event: {EventName} {@EventData}", eventName, eventData);
    }

    public void LogSecurityEvent(string userId, string action, string resource, bool allowed)
    {
        _logger.LogWarning("Security event: User {UserId} attempted {Action} on {Resource} - Allowed: {Allowed}",
                          userId, action, resource, allowed);
    }
}

// Usage Examples
public class OrderService
{
    private readonly ILogger<OrderService> _logger;
    private readonly StructuredLoggingService _structuredLogger;

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        using var scope = _logger.BeginScope("CreateOrder for customer {CustomerId}", request.CustomerId);
        
        try
        {
            _structuredLogger.LogUserAction(
                request.UserId, 
                "CreateOrder", 
                new { request.CustomerId, ItemCount = request.Items.Count });

            var order = new Order(request.CustomerId, request.Items);
            
            // Business logic...
            
            stopwatch.Stop();
            
            _structuredLogger.LogPerformanceMetric("CreateOrder", stopwatch.Elapsed, true);
            _structuredLogger.LogBusinessEvent("OrderCreated", new 
            { 
                OrderId = order.Id, 
                CustomerId = order.CustomerId,
                TotalAmount = order.TotalAmount,
                ItemCount = order.Items.Count
            });

            return order;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            _structuredLogger.LogPerformanceMetric("CreateOrder", stopwatch.Elapsed, false);
            
            _logger.LogError(ex, "Failed to create order for customer {CustomerId}", request.CustomerId);
            throw;
        }
    }
}
```

## Middleware de Logging

### 6.9 Request Logging Middleware

```csharp
// Infrastructure/Middleware/RequestLoggingMiddleware.cs
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var correlationId = Guid.NewGuid().ToString();
        
        using (_logger.BeginScope("Request {CorrelationId}", correlationId))
        {
            context.Items["CorrelationId"] = correlationId;
            
            _logger.LogInformation("Starting request {Method} {Path}", 
                                  context.Request.Method, context.Request.Path);

            try
            {
                await _next(context);
                
                stopwatch.Stop();
                
                _logger.LogInformation("Completed request {Method} {Path} with status {StatusCode} in {ElapsedMs}ms",
                                      context.Request.Method, 
                                      context.Request.Path, 
                                      context.Response.StatusCode, 
                                      stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                
                _logger.LogError(ex, "Request {Method} {Path} failed after {ElapsedMs}ms",
                               context.Request.Method, 
                               context.Request.Path, 
                               stopwatch.ElapsedMilliseconds);
                
                throw;
            }
        }
    }
}
```

## Configuração Avançada

### 6.10 Conditional Logging

```csharp
// Infrastructure/Logging/ConditionalLoggingConfiguration.cs
public static class ConditionalLoggingConfiguration
{
    public static LoggerConfiguration AddConditionalSinks(this LoggerConfiguration config)
    {
        // Log apenas erros para arquivo em produção
        config.WriteTo.Logger(l => l
            .Filter.ByIncludingOnly(Matching.WithProperty("Level", LogEventLevel.Error))
            .WriteTo.File(Path.Combine(FileSystem.AppDataDirectory, "logs", "errors-.log"),
                         rollingInterval: RollingInterval.Day));

        // Log de performance separado
        config.WriteTo.Logger(l => l
            .Filter.ByIncludingOnly(Matching.FromSource("Performance"))
            .WriteTo.File(Path.Combine(FileSystem.AppDataDirectory, "logs", "performance-.log"),
                         rollingInterval: RollingInterval.Hour));

        // Log de auditoria separado
        config.WriteTo.Logger(l => l
            .Filter.ByIncludingOnly(Matching.FromSource("Audit"))
            .WriteTo.File(Path.Combine(FileSystem.AppDataDirectory, "logs", "audit-.log"),
                         rollingInterval: RollingInterval.Day));

        return config;
    }
}
```

### 6.11 Log Filters

```csharp
// Infrastructure/Logging/LogFilters.cs
public static class LogFilters
{
    public static readonly Func<LogEvent, bool> ExcludeHealthChecks =
        Matching.FromSource("Microsoft.AspNetCore.Diagnostics.HealthChecks").Invoke;

    public static readonly Func<LogEvent, bool> ExcludeEntityFramework =
        Matching.FromSource("Microsoft.EntityFrameworkCore").Invoke;

    public static readonly Func<LogEvent, bool> IncludeOnlyBusinessEvents =
        le => le.Properties.ContainsKey("EventType") && 
              le.Properties["EventType"].ToString().Contains("Business");

    public static readonly Func<LogEvent, bool> IncludeOnlyErrors =
        le => le.Level >= LogEventLevel.Error;
}
```

## Análise e Monitoramento

### 6.12 Log Analysis

```csharp
// Infrastructure/Logging/LogAnalysisService.cs
public class LogAnalysisService
{
    private readonly ILogger<LogAnalysisService> _logger;

    public async Task<LogAnalysisReport> AnalyzeLogsAsync(DateTime from, DateTime to)
    {
        var logFiles = Directory.GetFiles(
            Path.Combine(FileSystem.AppDataDirectory, "logs"),
            "app-*.log");

        var report = new LogAnalysisReport();
        
        foreach (var logFile in logFiles)
        {
            var lines = await File.ReadAllLinesAsync(logFile);
            
            foreach (var line in lines)
            {
                if (line.Contains("ERROR"))
                    report.ErrorCount++;
                else if (line.Contains("WARN"))
                    report.WarningCount++;
                else if (line.Contains("INFO"))
                    report.InfoCount++;
            }
        }

        return report;
    }
}

public class LogAnalysisReport
{
    public int ErrorCount { get; set; }
    public int WarningCount { get; set; }
    public int InfoCount { get; set; }
    public Dictionary<string, int> ErrorsByType { get; set; } = new();
    public Dictionary<string, double> PerformanceMetrics { get; set; } = new();
}
```

## Melhores Práticas

1. **Use Structured Logging**: Facilita pesquisa e análise
2. **Implemente Log Scopes**: Agrupe logs relacionados
3. **Configure Níveis Apropriados**: Evite logs excessivos
4. **Use Enrichers**: Adicione contexto útil
5. **Monitore Performance**: Logs não devem impactar performance
6. **Rotacione Arquivos**: Evite arquivos muito grandes
7. **Proteja Dados Sensíveis**: Não logue informações confidenciais
8. **Use Correlation IDs**: Rastreie operações distribuídas

## Próximos Tópicos

- [Design Patterns](./07-design-patterns.md)
- [Diagramas UML](./08-diagramas-uml.md)
- [Estrutura do Projeto](./09-estrutura-projeto.md)
