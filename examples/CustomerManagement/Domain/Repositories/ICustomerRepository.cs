using CustomerManagement.Application.DTOs;
using CustomerManagement.Domain.Entities;
using CustomerManagement.Domain.ValueObjects;

namespace CustomerManagement.Domain.Repositories;

/// <summary>
/// Interface do repositório de clientes
/// Define as operações de acesso a dados para a entidade Customer
/// </summary>
public interface ICustomerRepository
{
    // Operações básicas
    Task<Customer?> GetByIdAsync(CustomerId id);
    Task<Customer?> GetByEmailAsync(Email email);
    Task<IEnumerable<Customer>> GetAllAsync();
    Task<(IEnumerable<Customer> customers, int totalCount)> GetPagedAsync(
        string? searchText = null, 
        int page = 1, 
        int pageSize = 20);
    
    Task AddAsync(Customer customer);
    void Update(Customer customer);
    void Delete(Customer customer);
    Task<bool> ExistsAsync(CustomerId id);
    Task<bool> EmailExistsAsync(Email email);

    // Operações de busca avançada
    Task<(IEnumerable<Customer> customers, int totalCount)> SearchAsync(CustomerSearchFilter filter);
    Task<IEnumerable<Customer>> GetByTypeAsync(CustomerType type);
    Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status);
    Task<IEnumerable<Customer>> GetByCityAsync(string city);
    Task<IEnumerable<Customer>> GetByStateAsync(string state);
    Task<IEnumerable<Customer>> GetCreatedBetweenAsync(DateTime startDate, DateTime endDate);

    // Estatísticas
    Task<CustomerStatistics> GetStatisticsAsync();
    Task<int> GetTotalCountAsync();
    Task<int> GetActiveCountAsync();
    Task<int> GetInactiveCountAsync();

    // Operações de persistência
    Task SaveChangesAsync();
    Task<int> SaveChangesWithResultAsync();
}
