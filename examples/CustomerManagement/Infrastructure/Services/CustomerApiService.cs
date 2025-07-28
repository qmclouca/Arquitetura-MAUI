using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using CustomerManagement.Application.DTOs;
using CustomerManagement.Domain.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CustomerManagement.Infrastructure.Services;

/// <summary>
/// Implementação do serviço de API de clientes
/// Responsável pela comunicação HTTP com o microserviço de clientes
/// </summary>
public class CustomerApiService : ICustomerApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CustomerApiService> _logger;
    private readonly IAuthenticationManager _authManager;
    private readonly ICacheManager _cache;
    private readonly JsonSerializerOptions _jsonOptions;

    public CustomerApiService(
        HttpClient httpClient,
        ILogger<CustomerApiService> logger,
        IAuthenticationManager authManager,
        ICacheManager cache)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _authManager = authManager ?? throw new ArgumentNullException(nameof(authManager));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    /// <summary>
    /// Obtém lista paginada de clientes da API
    /// </summary>
    public async Task<PagedResult<CustomerDto>> GetCustomersAsync(
        string? searchText = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            await SetAuthenticationHeaderAsync();

            // Verificar cache primeiro
            var cacheKey = $"customers_page_{page}_size_{pageSize}_search_{searchText ?? "all"}";
            var cachedResult = await _cache.GetAsync<PagedResult<CustomerDto>>(cacheKey);
            if (cachedResult != null)
            {
                _logger.LogInformation("Retornando clientes do cache - Página: {Page}", page);
                return cachedResult;
            }

            _logger.LogInformation("Buscando clientes da API - Página: {Page}, Tamanho: {PageSize}, Filtro: {SearchText}",
                page, pageSize, searchText);

            var queryParams = new List<string>
            {
                $"page={page}",
                $"pageSize={pageSize}"
            };

            if (!string.IsNullOrWhiteSpace(searchText))
            {
                queryParams.Add($"searchText={Uri.EscapeDataString(searchText)}");
            }

            var query = string.Join("&", queryParams);
            var response = await _httpClient.GetAsync($"customers?{query}");

            await EnsureSuccessStatusCodeAsync(response);

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<PagedResult<CustomerDto>>(json, _jsonOptions);

            // Armazenar resultado no cache por 5 minutos
            await _cache.SetAsync(cacheKey, result, TimeSpan.FromMinutes(5));

            _logger.LogInformation("Clientes obtidos com sucesso da API - Total: {Total}", result?.TotalCount);
            return result ?? new PagedResult<CustomerDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar clientes da API");
            throw;
        }
    }

    /// <summary>
    /// Obtém cliente por ID da API
    /// </summary>
    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id)
    {
        try
        {
            await SetAuthenticationHeaderAsync();

            // Verificar cache primeiro
            var cacheKey = $"customer_{id}";
            var cachedCustomer = await _cache.GetAsync<CustomerDto>(cacheKey);
            if (cachedCustomer != null)
            {
                _logger.LogInformation("Retornando cliente do cache - ID: {CustomerId}", id);
                return cachedCustomer;
            }

            _logger.LogInformation("Buscando cliente por ID da API: {CustomerId}", id);

            var response = await _httpClient.GetAsync($"customers/{id}");

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            await EnsureSuccessStatusCodeAsync(response);

            var json = await response.Content.ReadAsStringAsync();
            var customer = JsonSerializer.Deserialize<CustomerDto>(json, _jsonOptions);

            // Cache por 10 minutos
            if (customer != null)
            {
                await _cache.SetAsync(cacheKey, customer, TimeSpan.FromMinutes(10));
            }

            return customer;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cliente por ID: {CustomerId}", id);
            throw;
        }
    }

    /// <summary>
    /// Cria novo cliente via API
    /// </summary>
    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto customer)
    {
        try
        {
            await SetAuthenticationHeaderAsync();

            _logger.LogInformation("Criando cliente via API - Email: {Email}", customer.Email);

            var json = JsonSerializer.Serialize(customer, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("customers", content);
            await EnsureSuccessStatusCodeAsync(response);

            var responseJson = await response.Content.ReadAsStringAsync();
            var createdCustomer = JsonSerializer.Deserialize<CustomerDto>(responseJson, _jsonOptions);

            // Invalidar cache relacionado
            await _cache.RemovePatternAsync("customers_page_*");

            _logger.LogInformation("Cliente criado com sucesso via API - ID: {CustomerId}", createdCustomer?.Id);
            return createdCustomer ?? throw new InvalidOperationException("Falha ao criar cliente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar cliente via API");
            throw;
        }
    }

    /// <summary>
    /// Atualiza cliente via API
    /// </summary>
    public async Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerDto customer)
    {
        try
        {
            await SetAuthenticationHeaderAsync();

            _logger.LogInformation("Atualizando cliente via API - ID: {CustomerId}", id);

            var json = JsonSerializer.Serialize(customer, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"customers/{id}", content);
            await EnsureSuccessStatusCodeAsync(response);

            var responseJson = await response.Content.ReadAsStringAsync();
            var updatedCustomer = JsonSerializer.Deserialize<CustomerDto>(responseJson, _jsonOptions);

            // Invalidar cache
            await _cache.RemoveAsync($"customer_{id}");
            await _cache.RemovePatternAsync("customers_page_*");

            _logger.LogInformation("Cliente atualizado com sucesso via API - ID: {CustomerId}", id);
            return updatedCustomer ?? throw new InvalidOperationException("Falha ao atualizar cliente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar cliente via API - ID: {CustomerId}", id);
            throw;
        }
    }

    /// <summary>
    /// Exclui cliente via API
    /// </summary>
    public async Task DeleteCustomerAsync(Guid id)
    {
        try
        {
            await SetAuthenticationHeaderAsync();

            _logger.LogInformation("Excluindo cliente via API - ID: {CustomerId}", id);

            var response = await _httpClient.DeleteAsync($"customers/{id}");
            await EnsureSuccessStatusCodeAsync(response);

            // Invalidar cache
            await _cache.RemoveAsync($"customer_{id}");
            await _cache.RemovePatternAsync("customers_page_*");

            _logger.LogInformation("Cliente excluído com sucesso via API - ID: {CustomerId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir cliente via API - ID: {CustomerId}", id);
            throw;
        }
    }

    // Implementações dos outros métodos da interface...
    public async Task<CustomerDto?> GetCustomerByEmailAsync(string email)
    {
        await SetAuthenticationHeaderAsync();
        var response = await _httpClient.GetAsync($"customers/by-email/{Uri.EscapeDataString(email)}");
        
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;
            
        await EnsureSuccessStatusCodeAsync(response);
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CustomerDto>(json, _jsonOptions);
    }

    public async Task ActivateCustomerAsync(Guid id)
    {
        await SetAuthenticationHeaderAsync();
        var response = await _httpClient.PatchAsync($"customers/{id}/activate", null);
        await EnsureSuccessStatusCodeAsync(response);
        await _cache.RemoveAsync($"customer_{id}");
    }

    public async Task DeactivateCustomerAsync(Guid id)
    {
        await SetAuthenticationHeaderAsync();
        var response = await _httpClient.PatchAsync($"customers/{id}/deactivate", null);
        await EnsureSuccessStatusCodeAsync(response);
        await _cache.RemoveAsync($"customer_{id}");
    }

    public async Task<PagedResult<CustomerDto>> SearchCustomersAsync(CustomerSearchFilter filter)
    {
        await SetAuthenticationHeaderAsync();
        var json = JsonSerializer.Serialize(filter, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("customers/search", content);
        await EnsureSuccessStatusCodeAsync(response);
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PagedResult<CustomerDto>>(responseJson, _jsonOptions) 
               ?? new PagedResult<CustomerDto>();
    }

    public async Task<CustomerStatistics> GetStatisticsAsync()
    {
        await SetAuthenticationHeaderAsync();
        var response = await _httpClient.GetAsync("customers/statistics");
        await EnsureSuccessStatusCodeAsync(response);
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<CustomerStatistics>(json, _jsonOptions) 
               ?? new CustomerStatistics();
    }

    public async Task<IEnumerable<CustomerDto>> GetCustomersByCityAsync(string city)
    {
        await SetAuthenticationHeaderAsync();
        var response = await _httpClient.GetAsync($"customers/by-city/{Uri.EscapeDataString(city)}");
        await EnsureSuccessStatusCodeAsync(response);
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<CustomerDto>>(json, _jsonOptions) 
               ?? Enumerable.Empty<CustomerDto>();
    }

    public async Task<IEnumerable<CustomerDto>> GetCustomersByStateAsync(string state)
    {
        await SetAuthenticationHeaderAsync();
        var response = await _httpClient.GetAsync($"customers/by-state/{Uri.EscapeDataString(state)}");
        await EnsureSuccessStatusCodeAsync(response);
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<CustomerDto>>(json, _jsonOptions) 
               ?? Enumerable.Empty<CustomerDto>();
    }

    public async Task<IEnumerable<CustomerDto>> GetCustomersByTypeAsync(CustomerType type)
    {
        await SetAuthenticationHeaderAsync();
        var response = await _httpClient.GetAsync($"customers/by-type/{type}");
        await EnsureSuccessStatusCodeAsync(response);
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<CustomerDto>>(json, _jsonOptions) 
               ?? Enumerable.Empty<CustomerDto>();
    }

    public async Task<int> GetTotalCustomersCountAsync()
    {
        await SetAuthenticationHeaderAsync();
        var response = await _httpClient.GetAsync("customers/count");
        await EnsureSuccessStatusCodeAsync(response);
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<int>(json, _jsonOptions);
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        await SetAuthenticationHeaderAsync();
        var response = await _httpClient.GetAsync($"customers/email-exists/{Uri.EscapeDataString(email)}");
        await EnsureSuccessStatusCodeAsync(response);
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<bool>(json, _jsonOptions);
    }

    public async Task<IEnumerable<CustomerDto>> CreateCustomersAsync(IEnumerable<CreateCustomerDto> customers)
    {
        await SetAuthenticationHeaderAsync();
        var json = JsonSerializer.Serialize(customers, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("customers/batch", content);
        await EnsureSuccessStatusCodeAsync(response);
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<CustomerDto>>(responseJson, _jsonOptions) 
               ?? Enumerable.Empty<CustomerDto>();
    }

    public async Task<IEnumerable<CustomerDto>> UpdateCustomersAsync(IDictionary<Guid, UpdateCustomerDto> updates)
    {
        await SetAuthenticationHeaderAsync();
        var json = JsonSerializer.Serialize(updates, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PutAsync("customers/batch", content);
        await EnsureSuccessStatusCodeAsync(response);
        
        var responseJson = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<IEnumerable<CustomerDto>>(responseJson, _jsonOptions) 
               ?? Enumerable.Empty<CustomerDto>();
    }

    public async Task DeleteCustomersAsync(IEnumerable<Guid> ids)
    {
        await SetAuthenticationHeaderAsync();
        var json = JsonSerializer.Serialize(ids, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("customers/batch-delete", content);
        await EnsureSuccessStatusCodeAsync(response);
    }

    /// <summary>
    /// Configura o header de autenticação
    /// </summary>
    private async Task SetAuthenticationHeaderAsync()
    {
        if (!_authManager.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Usuário não autenticado");
        }

        var token = await _authManager.GetAccessTokenAsync();
        _httpClient.DefaultRequestHeaders.Authorization = 
            new AuthenticationHeaderValue("Bearer", token);
    }

    /// <summary>
    /// Verifica o status da resposta e trata erros específicos
    /// </summary>
    private async Task EnsureSuccessStatusCodeAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var content = await response.Content.ReadAsStringAsync();
        
        switch (response.StatusCode)
        {
            case System.Net.HttpStatusCode.Unauthorized:
                _logger.LogWarning("Token de autenticação inválido ou expirado");
                await _authManager.RefreshTokenAsync();
                throw new UnauthorizedAccessException("Token de autenticação inválido");
                
            case System.Net.HttpStatusCode.BadRequest:
                _logger.LogWarning("Requisição inválida: {Content}", content);
                throw new ArgumentException($"Dados inválidos: {content}");
                
            case System.Net.HttpStatusCode.NotFound:
                throw new KeyNotFoundException("Recurso não encontrado");
                
            case System.Net.HttpStatusCode.Conflict:
                throw new InvalidOperationException($"Conflito de dados: {content}");
                
            case System.Net.HttpStatusCode.InternalServerError:
                _logger.LogError("Erro interno do servidor: {Content}", content);
                throw new InvalidOperationException("Erro interno do servidor");
                
            default:
                _logger.LogError("Erro na API: {StatusCode} - {Content}", response.StatusCode, content);
                throw new HttpRequestException($"Erro na API: {response.StatusCode}");
        }
    }
}
