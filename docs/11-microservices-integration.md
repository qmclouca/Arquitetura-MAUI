# Infraestrutura para Microserviços - Arquitetura MAUI Desktop

## 🌐 **Comunicação com Microserviços .NET Core**

A aplicação MAUI Desktop foi projetada para consumir microserviços .NET Core ao invés de acessar diretamente bancos de dados, seguindo uma arquitetura distribuída moderna.

## 🏗️ **Camada de Infraestrutura Atualizada**

### **HTTP Clients**
```csharp
// Configuração de HttpClient com HttpClientFactory
services.AddHttpClient<ICustomerApiService, CustomerApiService>(client =>
{
    client.BaseAddress = new Uri("https://api.empresa.com/customers/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    client.Timeout = TimeSpan.FromSeconds(30);
});
```

### **API Service Implementations**
```csharp
public interface ICustomerApiService
{
    Task<PagedResult<CustomerDto>> GetCustomersAsync(int page, int size);
    Task<CustomerDto> GetCustomerByIdAsync(Guid id);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto customer);
    Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerDto customer);
    Task DeleteCustomerAsync(Guid id);
}

public class CustomerApiService : ICustomerApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerApiService> _logger;

    public CustomerApiService(HttpClient httpClient, ILogger<CustomerApiService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<PagedResult<CustomerDto>> GetCustomersAsync(int page, int size)
    {
        var response = await _httpClient.GetAsync($"?page={page}&size={size}");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PagedResult<CustomerDto>>(json);
    }
}
```

### **Authentication Manager**
```csharp
public interface IAuthenticationManager
{
    Task<string> GetAccessTokenAsync();
    Task<bool> RefreshTokenAsync();
    Task LogoutAsync();
    bool IsAuthenticated { get; }
}

public class JwtAuthenticationManager : IAuthenticationManager
{
    private readonly HttpClient _httpClient;
    private readonly ISecureStorage _secureStorage;
    
    public async Task<string> GetAccessTokenAsync()
    {
        var token = await _secureStorage.GetAsync("access_token");
        
        if (IsTokenExpired(token))
        {
            await RefreshTokenAsync();
            token = await _secureStorage.GetAsync("access_token");
        }
        
        return token;
    }
}
```

### **Retry Policies com Polly**
```csharp
services.AddHttpClient<ICustomerApiService, CustomerApiService>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());

private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => 
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
            onRetry: (outcome, timespan, retryCount, context) =>
            {
                var logger = context.GetLogger();
                logger?.LogWarning("Retry {RetryCount} after {TimeSpan}ms", 
                    retryCount, timespan.TotalMilliseconds);
            });
}
```

### **Cache Layer**
```csharp
public interface ICacheManager
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemovePatternAsync(string pattern);
}

public class MemoryCacheManager : ICacheManager
{
    private readonly IMemoryCache _cache;
    
    public async Task<T?> GetAsync<T>(string key)
    {
        return _cache.TryGetValue(key, out T? value) ? value : default;
    }
    
    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        var options = new MemoryCacheEntryOptions();
        if (expiration.HasValue)
            options.SetAbsoluteExpiration(expiration.Value);
            
        _cache.Set(key, value, options);
    }
}
```

## 🔄 **Padrão de Comunicação com APIs**

### **1. Application Service Coordenação**
```csharp
public class CustomerService : ICustomerService
{
    private readonly ICustomerApiService _customerApi;
    private readonly ICacheManager _cache;
    private readonly ILogger<CustomerService> _logger;

    public async Task<PagedResult<CustomerDto>> GetCustomersAsync(int page, int size)
    {
        var cacheKey = $"customers_page_{page}_size_{size}";
        
        // Verificar cache primeiro
        var cachedResult = await _cache.GetAsync<PagedResult<CustomerDto>>(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        // Buscar da API
        var result = await _customerApi.GetCustomersAsync(page, size);
        
        // Armazenar em cache
        await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));
        
        return result;
    }
}
```

### **2. Tratamento de Erros**
```csharp
public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto customer)
{
    try
    {
        return await _customerApi.CreateCustomerAsync(customer);
    }
    catch (HttpRequestException ex) when (ex.Message.Contains("400"))
    {
        _logger.LogWarning("Dados inválidos para criação de cliente: {Error}", ex.Message);
        throw new ValidationException("Dados do cliente são inválidos");
    }
    catch (HttpRequestException ex) when (ex.Message.Contains("401"))
    {
        _logger.LogWarning("Token de autenticação expirado");
        await _authManager.RefreshTokenAsync();
        
        // Retry após refresh do token
        return await _customerApi.CreateCustomerAsync(customer);
    }
    catch (HttpRequestException ex)
    {
        _logger.LogError(ex, "Erro na comunicação com API de clientes");
        throw new ServiceUnavailableException("Serviço temporariamente indisponível");
    }
}
```

## 🌍 **Arquitetura de Microserviços**

### **API Gateway**
- **Ponto único de entrada** para todas as chamadas
- **Roteamento inteligente** para microserviços específicos
- **Rate limiting** e throttling
- **Autenticação centralizada**
- **Logging e monitoramento**

### **Microserviços .NET Core**
```
📦 Customer Service      → Gerenciamento de clientes
📦 Order Service         → Processamento de pedidos  
📦 Product Service       → Catálogo de produtos
📦 Notification Service  → Envio de notificações
📦 Identity Service      → Autenticação e autorização
```

### **Configuração de Endpoints**
```json
{
  "ApiSettings": {
    "BaseUrl": "https://api.empresa.com",
    "Services": {
      "Customer": "/api/v1/customers",
      "Order": "/api/v1/orders",
      "Product": "/api/v1/products",
      "Identity": "/api/v1/auth"
    },
    "Timeout": "00:00:30",
    "RetryCount": 3
  }
}
```

## 🔒 **Segurança e Autenticação**

### **JWT Token Flow**
1. **Login** → Credenciais enviadas para Identity Service
2. **Token Response** → JWT access token + refresh token
3. **API Calls** → Access token no header Authorization
4. **Token Refresh** → Automático quando token expira
5. **Logout** → Invalidação dos tokens

### **Configuração de Autenticação**
```csharp
services.AddHttpClient<ICustomerApiService, CustomerApiService>()
    .AddHttpMessageHandler<AuthenticationHandler>();

public class AuthenticationHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _authManager.GetAccessTokenAsync();
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        return await base.SendAsync(request, cancellationToken);
    }
}
```

## 📊 **Benefícios da Arquitetura**

### **✅ Escalabilidade**
- Microserviços podem ser escalados independentemente
- Load balancing automático
- Distribuição de carga

### **✅ Resiliência**
- Circuit breaker para falhas
- Retry policies inteligentes
- Fallback mechanisms

### **✅ Manutenibilidade**
- Separação clara de responsabilidades
- Versionamento independente de serviços
- Deploy independente

### **✅ Performance**
- Cache inteligente
- Conexões HTTP reutilizáveis
- Compressão de dados

### **✅ Segurança**
- Autenticação centralizada
- Tokens JWT seguros
- Comunicação HTTPS

Esta arquitetura garante que a aplicação MAUI Desktop seja robusta, escalável e mantenha alta performance mesmo consumindo dados de múltiplos microserviços distribuídos.
