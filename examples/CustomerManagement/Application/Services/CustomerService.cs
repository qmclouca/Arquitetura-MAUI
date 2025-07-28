using AutoMapper;
using CustomerManagement.Application.DTOs;
using CustomerManagement.Domain.Entities;
using CustomerManagement.Domain.Services;
using CustomerManagement.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace CustomerManagement.Application.Services;

/// <summary>
/// Interface para serviços de cliente
/// </summary>
public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> GetCustomersAsync(
        string? searchText = null,
        int page = 1,
        int pageSize = 20);
    
    Task<CustomerDto?> GetCustomerByIdAsync(Guid id);
    Task<CustomerDto?> GetCustomerByEmailAsync(string email);
    Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createDto);
    Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerDto updateDto);
    Task DeleteCustomerAsync(Guid id);
    Task ActivateCustomerAsync(Guid id);
    Task DeactivateCustomerAsync(Guid id);
    Task<CustomerStatistics> GetStatisticsAsync();
    Task<PagedResult<CustomerDto>> SearchCustomersAsync(CustomerSearchFilter filter);
}

/// <summary>
/// Implementação do serviço de cliente
/// Camada de aplicação que orquestra operações consumindo APIs de microserviços
/// </summary>
public class CustomerService : ICustomerService
{
    private readonly ICustomerApiService _customerApi;
    private readonly IMapper _mapper;
    private readonly ILogger<CustomerService> _logger;
    private readonly ICacheManager _cache;

    public CustomerService(
        ICustomerApiService customerApi,
        IMapper mapper,
        ILogger<CustomerService> logger,
        ICacheManager cache)
    {
        _customerApi = customerApi ?? throw new ArgumentNullException(nameof(customerApi));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <summary>
    /// Obtém lista paginada de clientes
    /// </summary>
    public async Task<PagedResult<CustomerDto>> GetCustomersAsync(
        string? searchText = null,
        int page = 1,
        int pageSize = 20)
    {
        try
        {
            _logger.LogInformation("Buscando clientes - Página: {Page}, Tamanho: {PageSize}, Filtro: {SearchText}",
                page, pageSize, searchText);

            var (customers, totalCount) = await _customerRepository.GetPagedAsync(
                searchText, page, pageSize);

            var customerDtos = _mapper.Map<IEnumerable<CustomerDto>>(customers);

            return new PagedResult<CustomerDto>
            {
                Items = customerDtos,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar clientes");
            throw;
        }
    }

    /// <summary>
    /// Obtém cliente por ID
    /// </summary>
    public async Task<CustomerDto?> GetCustomerByIdAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Buscando cliente por ID: {CustomerId}", id);

            var customer = await _customerRepository.GetByIdAsync(CustomerId.Create(id));
            
            return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cliente por ID: {CustomerId}", id);
            throw;
        }
    }

    /// <summary>
    /// Obtém cliente por email
    /// </summary>
    public async Task<CustomerDto?> GetCustomerByEmailAsync(string email)
    {
        try
        {
            _logger.LogInformation("Buscando cliente por email: {Email}", email);

            var customer = await _customerRepository.GetByEmailAsync(Email.Create(email));
            
            return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar cliente por email: {Email}", email);
            throw;
        }
    }

    /// <summary>
    /// Cria novo cliente
    /// </summary>
    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerDto createDto)
    {
        try
        {
            _logger.LogInformation("Criando novo cliente - Email: {Email}", createDto.Email);

            // Verificar se email já existe
            var existingCustomer = await _customerRepository.GetByEmailAsync(Email.Create(createDto.Email));
            if (existingCustomer != null)
            {
                throw new InvalidOperationException($"Já existe um cliente com o email {createDto.Email}");
            }

            // Criar endereço
            var address = new Address(
                createDto.Address.Street,
                createDto.Address.Number,
                createDto.Address.Neighborhood,
                createDto.Address.City,
                createDto.Address.State,
                createDto.Address.ZipCode,
                createDto.Address.Country,
                createDto.Address.Complement);

            // Criar cliente
            var customer = Customer.Create(
                createDto.FirstName,
                createDto.LastName,
                createDto.Email,
                createDto.Phone,
                address,
                createDto.Type);

            // Salvar no repositório
            await _customerRepository.AddAsync(customer);
            await _customerRepository.SaveChangesAsync();

            _logger.LogInformation("Cliente criado com sucesso - ID: {CustomerId}", customer.Id);

            return _mapper.Map<CustomerDto>(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar cliente");
            throw;
        }
    }

    /// <summary>
    /// Atualiza cliente existente
    /// </summary>
    public async Task<CustomerDto> UpdateCustomerAsync(Guid id, UpdateCustomerDto updateDto)
    {
        try
        {
            _logger.LogInformation("Atualizando cliente - ID: {CustomerId}", id);

            var customer = await _customerRepository.GetByIdAsync(CustomerId.Create(id));
            if (customer == null)
            {
                throw new InvalidOperationException($"Cliente com ID {id} não encontrado");
            }

            if (customer.IsDeleted)
            {
                throw new InvalidOperationException("Não é possível atualizar um cliente excluído");
            }

            // Atualizar informações básicas
            customer.UpdateBasicInfo(
                updateDto.FirstName,
                updateDto.LastName,
                updateDto.Phone);

            // Atualizar endereço
            var newAddress = new Address(
                updateDto.Address.Street,
                updateDto.Address.Number,
                updateDto.Address.Neighborhood,
                updateDto.Address.City,
                updateDto.Address.State,
                updateDto.Address.ZipCode,
                updateDto.Address.Country,
                updateDto.Address.Complement);

            customer.UpdateAddress(newAddress);

            // Atualizar tipo se necessário
            if (customer.Type != updateDto.Type)
            {
                customer.ChangeType(updateDto.Type);
            }

            // Salvar alterações
            _customerRepository.Update(customer);
            await _customerRepository.SaveChangesAsync();

            _logger.LogInformation("Cliente atualizado com sucesso - ID: {CustomerId}", id);

            return _mapper.Map<CustomerDto>(customer);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar cliente - ID: {CustomerId}", id);
            throw;
        }
    }

    /// <summary>
    /// Exclui cliente (soft delete)
    /// </summary>
    public async Task DeleteCustomerAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Excluindo cliente - ID: {CustomerId}", id);

            var customer = await _customerRepository.GetByIdAsync(CustomerId.Create(id));
            if (customer == null)
            {
                throw new InvalidOperationException($"Cliente com ID {id} não encontrado");
            }

            if (customer.IsDeleted)
            {
                _logger.LogWarning("Tentativa de excluir cliente já excluído - ID: {CustomerId}", id);
                return;
            }

            // Soft delete
            customer.MarkAsDeleted();

            _customerRepository.Update(customer);
            await _customerRepository.SaveChangesAsync();

            _logger.LogInformation("Cliente excluído com sucesso - ID: {CustomerId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir cliente - ID: {CustomerId}", id);
            throw;
        }
    }

    /// <summary>
    /// Ativa cliente
    /// </summary>
    public async Task ActivateCustomerAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Ativando cliente - ID: {CustomerId}", id);

            var customer = await _customerRepository.GetByIdAsync(CustomerId.Create(id));
            if (customer == null)
            {
                throw new InvalidOperationException($"Cliente com ID {id} não encontrado");
            }

            customer.Activate();

            _customerRepository.Update(customer);
            await _customerRepository.SaveChangesAsync();

            _logger.LogInformation("Cliente ativado com sucesso - ID: {CustomerId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao ativar cliente - ID: {CustomerId}", id);
            throw;
        }
    }

    /// <summary>
    /// Inativa cliente
    /// </summary>
    public async Task DeactivateCustomerAsync(Guid id)
    {
        try
        {
            _logger.LogInformation("Inativando cliente - ID: {CustomerId}", id);

            var customer = await _customerRepository.GetByIdAsync(CustomerId.Create(id));
            if (customer == null)
            {
                throw new InvalidOperationException($"Cliente com ID {id} não encontrado");
            }

            customer.Deactivate();

            _customerRepository.Update(customer);
            await _customerRepository.SaveChangesAsync();

            _logger.LogInformation("Cliente inativado com sucesso - ID: {CustomerId}", id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao inativar cliente - ID: {CustomerId}", id);
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
            _logger.LogInformation("Obtendo estatísticas de clientes");

            var stats = await _customerRepository.GetStatisticsAsync();

            _logger.LogInformation("Estatísticas obtidas com sucesso - Total de clientes: {Total}", 
                stats.TotalCustomers);

            return stats;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter estatísticas de clientes");
            throw;
        }
    }

    /// <summary>
    /// Busca clientes com filtros avançados
    /// </summary>
    public async Task<PagedResult<CustomerDto>> SearchCustomersAsync(CustomerSearchFilter filter)
    {
        try
        {
            _logger.LogInformation("Buscando clientes com filtros avançados - Página: {Page}, Tamanho: {PageSize}",
                filter.Page, filter.PageSize);

            var (customers, totalCount) = await _customerRepository.SearchAsync(filter);

            var customerDtos = _mapper.Map<IEnumerable<CustomerDto>>(customers);

            return new PagedResult<CustomerDto>
            {
                Items = customerDtos,
                Page = filter.Page,
                PageSize = filter.PageSize,
                TotalCount = totalCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar clientes com filtros");
            throw;
        }
    }
}
