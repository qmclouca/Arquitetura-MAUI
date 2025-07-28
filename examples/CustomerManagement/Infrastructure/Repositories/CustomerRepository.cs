using Microsoft.EntityFrameworkCore;
using CustomerManagement.Application.DTOs;
using CustomerManagement.Domain.Entities;
using CustomerManagement.Domain.Repositories;
using CustomerManagement.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CustomerManagement.Infrastructure.Repositories;

/// <summary>
/// Implementação do repositório de clientes usando Entity Framework Core
/// </summary>
public class CustomerRepository : ICustomerRepository
{
    private readonly CustomerDbContext _context;
    private readonly ILogger<CustomerRepository> _logger;

    public CustomerRepository(CustomerDbContext context, ILogger<CustomerRepository> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtém cliente por ID
    /// </summary>
    public async Task<Customer?> GetByIdAsync(CustomerId id)
    {
        try
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cliente por ID: {CustomerId}", id.Value);
            throw;
        }
    }

    /// <summary>
    /// Obtém cliente por email
    /// </summary>
    public async Task<Customer?> GetByEmailAsync(Email email)
    {
        try
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Email.Value == email.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cliente por email: {Email}", email.Value);
            throw;
        }
    }

    /// <summary>
    /// Obtém todos os clientes
    /// </summary>
    public async Task<IEnumerable<Customer>> GetAllAsync()
    {
        try
        {
            return await _context.Customers
                .Where(c => !c.IsDeleted)
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar todos os clientes");
            throw;
        }
    }

    /// <summary>
    /// Obtém lista paginada de clientes
    /// </summary>
    public async Task<(IEnumerable<Customer> customers, int totalCount)> GetPagedAsync(
        string? searchText = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            var query = _context.Customers
                .Where(c => !c.IsDeleted);

            // Aplicar filtro de busca
            if (!string.IsNullOrWhiteSpace(searchText))
            {
                var search = searchText.ToLower();
                query = query.Where(c =>
                    c.FirstName.ToLower().Contains(search) ||
                    c.LastName.ToLower().Contains(search) ||
                    c.Email.Value.ToLower().Contains(search) ||
                    c.Phone.Value.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var customers = await query
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (customers, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar clientes paginados");
            throw;
        }
    }

    /// <summary>
    /// Adiciona novo cliente
    /// </summary>
    public async Task AddAsync(Customer customer)
    {
        try
        {
            await _context.Customers.AddAsync(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar cliente: {CustomerId}", customer.Id);
            throw;
        }
    }

    /// <summary>
    /// Atualiza cliente existente
    /// </summary>
    public void Update(Customer customer)
    {
        try
        {
            _context.Customers.Update(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar cliente: {CustomerId}", customer.Id);
            throw;
        }
    }

    /// <summary>
    /// Remove cliente
    /// </summary>
    public void Delete(Customer customer)
    {
        try
        {
            _context.Customers.Remove(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover cliente: {CustomerId}", customer.Id);
            throw;
        }
    }

    /// <summary>
    /// Verifica se cliente existe
    /// </summary>
    public async Task<bool> ExistsAsync(CustomerId id)
    {
        try
        {
            return await _context.Customers
                .AnyAsync(c => c.Id == id.Value);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do cliente: {CustomerId}", id.Value);
            throw;
        }
    }

    /// <summary>
    /// Verifica se email já existe
    /// </summary>
    public async Task<bool> EmailExistsAsync(Email email)
    {
        try
        {
            return await _context.Customers
                .AnyAsync(c => c.Email.Value == email.Value && !c.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao verificar existência do email: {Email}", email.Value);
            throw;
        }
    }

    /// <summary>
    /// Busca clientes com filtros avançados
    /// </summary>
    public async Task<(IEnumerable<Customer> customers, int totalCount)> SearchAsync(CustomerSearchFilter filter)
    {
        try
        {
            var query = _context.Customers.AsQueryable();

            // Aplicar filtro de exclusão
            if (filter.IsDeleted.HasValue)
            {
                query = query.Where(c => c.IsDeleted == filter.IsDeleted.Value);
            }
            else
            {
                query = query.Where(c => !c.IsDeleted);
            }

            // Aplicar filtro de texto
            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var search = filter.SearchText.ToLower();
                query = query.Where(c =>
                    c.FirstName.ToLower().Contains(search) ||
                    c.LastName.ToLower().Contains(search) ||
                    c.Email.Value.ToLower().Contains(search) ||
                    c.Phone.Value.Contains(search));
            }

            // Aplicar filtro de tipo
            if (filter.Type.HasValue)
            {
                query = query.Where(c => c.Type == filter.Type.Value);
            }

            // Aplicar filtro de status
            if (filter.Status.HasValue)
            {
                query = query.Where(c => c.Status == filter.Status.Value);
            }

            // Aplicar filtro de cidade
            if (!string.IsNullOrWhiteSpace(filter.City))
            {
                query = query.Where(c => c.Address.City.ToLower().Contains(filter.City.ToLower()));
            }

            // Aplicar filtro de estado
            if (!string.IsNullOrWhiteSpace(filter.State))
            {
                query = query.Where(c => c.Address.State.ToLower().Contains(filter.State.ToLower()));
            }

            // Aplicar filtro de data de criação
            if (filter.CreatedFrom.HasValue)
            {
                query = query.Where(c => c.CreatedAt >= filter.CreatedFrom.Value);
            }

            if (filter.CreatedTo.HasValue)
            {
                query = query.Where(c => c.CreatedAt <= filter.CreatedTo.Value);
            }

            var totalCount = await query.CountAsync();

            var customers = await query
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return (customers, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar clientes com filtros");
            throw;
        }
    }

    /// <summary>
    /// Obtém clientes por tipo
    /// </summary>
    public async Task<IEnumerable<Customer>> GetByTypeAsync(CustomerType type)
    {
        try
        {
            return await _context.Customers
                .Where(c => c.Type == type && !c.IsDeleted)
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar clientes por tipo: {Type}", type);
            throw;
        }
    }

    /// <summary>
    /// Obtém clientes por status
    /// </summary>
    public async Task<IEnumerable<Customer>> GetByStatusAsync(CustomerStatus status)
    {
        try
        {
            return await _context.Customers
                .Where(c => c.Status == status && !c.IsDeleted)
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar clientes por status: {Status}", status);
            throw;
        }
    }

    /// <summary>
    /// Obtém clientes por cidade
    /// </summary>
    public async Task<IEnumerable<Customer>> GetByCityAsync(string city)
    {
        try
        {
            return await _context.Customers
                .Where(c => c.Address.City.ToLower().Contains(city.ToLower()) && !c.IsDeleted)
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar clientes por cidade: {City}", city);
            throw;
        }
    }

    /// <summary>
    /// Obtém clientes por estado
    /// </summary>
    public async Task<IEnumerable<Customer>> GetByStateAsync(string state)
    {
        try
        {
            return await _context.Customers
                .Where(c => c.Address.State.ToLower().Contains(state.ToLower()) && !c.IsDeleted)
                .OrderBy(c => c.FirstName)
                .ThenBy(c => c.LastName)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar clientes por estado: {State}", state);
            throw;
        }
    }

    /// <summary>
    /// Obtém clientes criados entre datas
    /// </summary>
    public async Task<IEnumerable<Customer>> GetCreatedBetweenAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            return await _context.Customers
                .Where(c => c.CreatedAt >= startDate && c.CreatedAt <= endDate && !c.IsDeleted)
                .OrderBy(c => c.CreatedAt)
                .ToListAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar clientes por período");
            throw;
        }
    }

    /// <summary>
    /// Obtém estatísticas de clientes
    /// </summary>
    public async Task<CustomerStatistics> GetStatisticsAsync()
    {
        try
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var startOfYear = new DateTime(now.Year, 1, 1);

            var totalCustomers = await _context.Customers.CountAsync(c => !c.IsDeleted);
            var activeCustomers = await _context.Customers.CountAsync(c => c.Status == CustomerStatus.Active && !c.IsDeleted);
            var inactiveCustomers = await _context.Customers.CountAsync(c => c.Status == CustomerStatus.Inactive && !c.IsDeleted);
            
            var standardCustomers = await _context.Customers.CountAsync(c => c.Type == CustomerType.Standard && !c.IsDeleted);
            var premiumCustomers = await _context.Customers.CountAsync(c => c.Type == CustomerType.Premium && !c.IsDeleted);
            var corporateCustomers = await _context.Customers.CountAsync(c => c.Type == CustomerType.Corporate && !c.IsDeleted);
            
            var customersThisMonth = await _context.Customers.CountAsync(c => c.CreatedAt >= startOfMonth && !c.IsDeleted);
            var customersThisYear = await _context.Customers.CountAsync(c => c.CreatedAt >= startOfYear && !c.IsDeleted);

            var customersByCity = await _context.Customers
                .Where(c => !c.IsDeleted)
                .GroupBy(c => c.Address.City)
                .Select(g => new { City = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToDictionaryAsync(x => x.City, x => x.Count);

            var customersByState = await _context.Customers
                .Where(c => !c.IsDeleted)
                .GroupBy(c => c.Address.State)
                .Select(g => new { State = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .Take(10)
                .ToDictionaryAsync(x => x.State, x => x.Count);

            return new CustomerStatistics
            {
                TotalCustomers = totalCustomers,
                ActiveCustomers = activeCustomers,
                InactiveCustomers = inactiveCustomers,
                StandardCustomers = standardCustomers,
                PremiumCustomers = premiumCustomers,
                CorporateCustomers = corporateCustomers,
                CustomersCreatedThisMonth = customersThisMonth,
                CustomersCreatedThisYear = customersThisYear,
                CustomersByCity = customersByCity,
                CustomersByState = customersByState
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas de clientes");
            throw;
        }
    }

    /// <summary>
    /// Obtém total de clientes
    /// </summary>
    public async Task<int> GetTotalCountAsync()
    {
        try
        {
            return await _context.Customers.CountAsync(c => !c.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter total de clientes");
            throw;
        }
    }

    /// <summary>
    /// Obtém total de clientes ativos
    /// </summary>
    public async Task<int> GetActiveCountAsync()
    {
        try
        {
            return await _context.Customers.CountAsync(c => c.Status == CustomerStatus.Active && !c.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter total de clientes ativos");
            throw;
        }
    }

    /// <summary>
    /// Obtém total de clientes inativos
    /// </summary>
    public async Task<int> GetInactiveCountAsync()
    {
        try
        {
            return await _context.Customers.CountAsync(c => c.Status == CustomerStatus.Inactive && !c.IsDeleted);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter total de clientes inativos");
            throw;
        }
    }

    /// <summary>
    /// Salva alterações no contexto
    /// </summary>
    public async Task SaveChangesAsync()
    {
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar alterações no banco de dados");
            throw;
        }
    }

    /// <summary>
    /// Salva alterações e retorna número de registros afetados
    /// </summary>
    public async Task<int> SaveChangesWithResultAsync()
    {
        try
        {
            return await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar alterações no banco de dados");
            throw;
        }
    }
}
