using CustomerManagement.Application.DTOs;

namespace CustomerManagement.Domain.Services;

/// <summary>
/// Interface para comunicação com a API de clientes
/// Define os contratos para comunicação com microserviços
/// </summary>
public interface ICustomerApiService
{
    // Operações básicas de CRUD
    Task<PagedResult<CustomerDto>> GetCustomersAsync(
        string? searchText = null, 
        int page = 1, 
        int pageSize = 20);
    
    Task<CustomerDto?> GetCustomerByIdAsync(Guid id);
    Task<CustomerDto?> GetCustomerByEmailAsync(string email);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto customer);
    Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerDto customer);
    Task DeleteCustomerAsync(Guid id);

    // Operações de status
    Task ActivateCustomerAsync(Guid id);
    Task DeactivateCustomerAsync(Guid id);

    // Operações de busca avançada
    Task<PagedResult<CustomerDto>> SearchCustomersAsync(CustomerSearchFilter filter);
    Task<IEnumerable<CustomerDto>> GetCustomersByCityAsync(string city);
    Task<IEnumerable<CustomerDto>> GetCustomersByStateAsync(string state);
    Task<IEnumerable<CustomerDto>> GetCustomersByTypeAsync(CustomerType type);

    // Estatísticas e relatórios
    Task<CustomerStatistics> GetStatisticsAsync();
    Task<int> GetTotalCustomersCountAsync();
    Task<bool> EmailExistsAsync(string email);

    // Operações em lote
    Task<IEnumerable<CustomerDto>> CreateCustomersAsync(IEnumerable<CreateCustomerDto> customers);
    Task<IEnumerable<CustomerDto>> UpdateCustomersAsync(IDictionary<Guid, UpdateCustomerDto> updates);
    Task DeleteCustomersAsync(IEnumerable<Guid> ids);
}

/// <summary>
/// Interface para gerenciamento de cache
/// </summary>
public interface ICacheManager
{
    Task<T?> GetAsync<T>(string key);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null);
    Task RemoveAsync(string key);
    Task RemovePatternAsync(string pattern);
    Task<bool> ExistsAsync(string key);
}

/// <summary>
/// Interface para gerenciamento de autenticação
/// </summary>
public interface IAuthenticationManager
{
    Task<string> GetAccessTokenAsync();
    Task<bool> RefreshTokenAsync();
    Task LoginAsync(string username, string password);
    Task LogoutAsync();
    bool IsAuthenticated { get; }
    bool IsTokenExpired { get; }
    DateTime? TokenExpirationTime { get; }
}
