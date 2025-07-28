using CustomerManagement.Domain.Entities;
using CustomerManagement.Domain.ValueObjects;
using CustomerManagement.Application.DTOs;
using Bogus;

namespace CustomerManagement.Tests.TestUtilities;

/// <summary>
/// Factory para criação de dados de teste usando Bogus
/// Garante dados consistentes e realistas para os testes
/// </summary>
public static class TestDataFactory
{
    private static readonly Faker _faker = new("pt_BR");

    #region Customer Builders

    /// <summary>
    /// Cria um Customer com dados válidos padrão
    /// </summary>
    public static Customer CreateValidCustomer(
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        CustomerType? type = null)
    {
        firstName ??= _faker.Person.FirstName;
        lastName ??= _faker.Person.LastName;
        email ??= _faker.Internet.Email(firstName, lastName);
        type ??= _faker.PickRandom<CustomerType>();

        var address = CreateValidAddress();

        return Customer.Create(
            firstName,
            lastName,
            email,
            _faker.Phone.PhoneNumber("(##) #####-####"),
            address,
            type.Value);
    }

    /// <summary>
    /// Cria uma lista de Customers para testes de coleção
    /// </summary>
    public static List<Customer> CreateCustomerList(int count = 5)
    {
        return Enumerable.Range(1, count)
            .Select(_ => CreateValidCustomer())
            .ToList();
    }

    /// <summary>
    /// Cria um Customer com dados específicos para testes de edge cases
    /// </summary>
    public static Customer CreateCustomerWithSpecificData(
        string firstName = "João",
        string lastName = "Silva",
        string email = "joao.silva@email.com",
        string phone = "11999999999",
        CustomerType type = CustomerType.Standard,
        CustomerStatus status = CustomerStatus.Active)
    {
        var customer = Customer.Create(
            firstName,
            lastName,
            email,
            phone,
            CreateValidAddress(),
            type);

        // Usar reflection para alterar status se necessário
        if (status != CustomerStatus.Active)
        {
            var statusProperty = typeof(Customer).GetProperty(nameof(Customer.Status));
            statusProperty?.SetValue(customer, status);
        }

        return customer;
    }

    #endregion

    #region Address Builders

    /// <summary>
    /// Cria um Address válido com dados brasileiros
    /// </summary>
    public static Address CreateValidAddress(
        string? street = null,
        string? number = null,
        string? neighborhood = null,
        string? city = null,
        string? state = null,
        string? postalCode = null,
        string? country = null)
    {
        return new Address(
            street ?? _faker.Address.StreetName(),
            number ?? _faker.Address.BuildingNumber(),
            neighborhood ?? "Centro",
            city ?? _faker.Address.City(),
            state ?? _faker.Address.StateAbbr(),
            postalCode ?? _faker.Address.ZipCode("########"),
            country ?? "Brasil");
    }

    /// <summary>
    /// Cria Address com dados específicos para São Paulo
    /// </summary>
    public static Address CreateSaoPauloAddress()
    {
        return new Address(
            _faker.Address.StreetName(),
            _faker.Address.BuildingNumber(),
            _faker.PickRandom("Centro", "Vila Mariana", "Pinheiros", "Moema", "Itaim Bibi"),
            "São Paulo",
            "SP",
            _faker.Address.ZipCode("########"),
            "Brasil");
    }

    #endregion

    #region DTO Builders

    /// <summary>
    /// Cria um CustomerDto para testes de API
    /// </summary>
    public static CustomerDto CreateCustomerDto(
        Guid? id = null,
        string? firstName = null,
        string? lastName = null,
        string? email = null,
        CustomerType? type = null,
        CustomerStatus? status = null)
    {
        firstName ??= _faker.Person.FirstName;
        lastName ??= _faker.Person.LastName;
        email ??= _faker.Internet.Email(firstName, lastName);

        return new CustomerDto
        {
            Id = id ?? Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            FullName = $"{firstName} {lastName}",
            Email = email,
            Phone = _faker.Phone.PhoneNumber("(##) #####-####"),
            Address = CreateAddressDto(),
            Type = type ?? _faker.PickRandom<CustomerType>(),
            Status = status ?? _faker.PickRandom<CustomerStatus>(),
            CreatedAt = _faker.Date.Recent(30),
            UpdatedAt = _faker.Date.Recent(7),
            IsDeleted = false
        };
    }

    /// <summary>
    /// Cria CreateCustomerDto para testes de criação
    /// </summary>
    public static CreateCustomerDto CreateCreateCustomerDto(
        string? firstName = null,
        string? email = null)
    {
        firstName ??= _faker.Person.FirstName;
        var lastName = _faker.Person.LastName;
        email ??= _faker.Internet.Email(firstName, lastName);

        return new CreateCustomerDto
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = _faker.Phone.PhoneNumber("(##) #####-####"),
            Address = CreateAddressDto(),
            Type = _faker.PickRandom<CustomerType>()
        };
    }

    /// <summary>
    /// Cria UpdateCustomerDto para testes de atualização
    /// </summary>
    public static UpdateCustomerDto CreateUpdateCustomerDto()
    {
        return new UpdateCustomerDto
        {
            FirstName = _faker.Person.FirstName,
            LastName = _faker.Person.LastName,
            Phone = _faker.Phone.PhoneNumber("(##) #####-####"),
            Address = CreateAddressDto(),
            Type = _faker.PickRandom<CustomerType>()
        };
    }

    /// <summary>
    /// Cria AddressDto para testes
    /// </summary>
    public static AddressDto CreateAddressDto()
    {
        return new AddressDto(
            _faker.Address.StreetName(),
            _faker.Address.BuildingNumber(),
            "Centro",
            _faker.Address.City(),
            _faker.Address.StateAbbr(),
            _faker.Address.ZipCode("########"),
            "Brasil");
    }

    #endregion

    #region Paged Results

    /// <summary>
    /// Cria um PagedResult para testes de paginação
    /// </summary>
    public static PagedResult<T> CreatePagedResult<T>(
        IEnumerable<T> items,
        int page = 1,
        int pageSize = 20,
        int? totalCount = null)
    {
        var itemsList = items.ToList();
        totalCount ??= itemsList.Count;

        return new PagedResult<T>
        {
            Items = itemsList,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount.Value,
            TotalPages = (int)Math.Ceiling((double)totalCount.Value / pageSize)
        };
    }

    /// <summary>
    /// Cria PagedResult com CustomerDto para testes de API
    /// </summary>
    public static PagedResult<CustomerDto> CreateCustomerPagedResult(
        int itemCount = 5,
        int page = 1,
        int pageSize = 20)
    {
        var customers = Enumerable.Range(1, itemCount)
            .Select(_ => CreateCustomerDto())
            .ToList();

        return CreatePagedResult(customers, page, pageSize, itemCount);
    }

    #endregion

    #region Invalid Data Builders

    /// <summary>
    /// Cria dados inválidos para testes de validação
    /// </summary>
    public static class Invalid
    {
        public static CreateCustomerDto CreateCustomerDtoWithEmptyFirstName()
        {
            var dto = CreateCreateCustomerDto();
            dto.FirstName = "";
            return dto;
        }

        public static CreateCustomerDto CreateCustomerDtoWithInvalidEmail()
        {
            var dto = CreateCreateCustomerDto();
            dto.Email = "email-invalido";
            return dto;
        }

        public static CreateCustomerDto CreateCustomerDtoWithEmptyLastName()
        {
            var dto = CreateCreateCustomerDto();
            dto.LastName = "";
            return dto;
        }

        public static CreateCustomerDto CreateCustomerDtoWithEmptyPhone()
        {
            var dto = CreateCreateCustomerDto();
            dto.Phone = "";
            return dto;
        }

        public static Address CreateAddressWithEmptyStreet()
        {
            return new Address(
                "", // Street vazia
                "123",
                "Centro",
                "São Paulo",
                "SP",
                "01234567",
                "Brasil");
        }

        public static Address CreateAddressWithInvalidPostalCode()
        {
            return new Address(
                "Rua Teste",
                "123",
                "Centro",
                "São Paulo",
                "SP",
                "123", // CEP inválido
                "Brasil");
        }
    }

    #endregion

    #region Scenario Builders

    /// <summary>
    /// Cria cenários específicos para testes
    /// </summary>
    public static class Scenarios
    {
        /// <summary>
        /// Cliente Premium com histórico completo
        /// </summary>
        public static Customer PremiumCustomerWithHistory()
        {
            var customer = CreateValidCustomer(type: CustomerType.Premium);
            
            // Simular algumas atualizações
            customer.UpdateInfo("João Updated", "Silva Updated", "11888888888");
            customer.ChangeType(CustomerType.Premium);
            
            return customer;
        }

        /// <summary>
        /// Cliente inativo para testes de reativação
        /// </summary>
        public static Customer InactiveCustomer()
        {
            var customer = CreateValidCustomer();
            customer.Deactivate();
            return customer;
        }

        /// <summary>
        /// Cliente deletado para testes de soft delete
        /// </summary>
        public static Customer DeletedCustomer()
        {
            var customer = CreateValidCustomer();
            customer.Delete();
            return customer;
        }

        /// <summary>
        /// Lista de customers com diferentes tipos e status
        /// </summary>
        public static List<Customer> MixedCustomerList()
        {
            return new List<Customer>
            {
                CreateValidCustomer(type: CustomerType.Standard),
                CreateValidCustomer(type: CustomerType.Premium),
                PremiumCustomerWithHistory(),
                InactiveCustomer(),
                CreateValidCustomer(type: CustomerType.VIP)
            };
        }

        /// <summary>
        /// Dados para teste de busca e filtros
        /// </summary>
        public static List<Customer> SearchTestCustomers()
        {
            return new List<Customer>
            {
                CreateCustomerWithSpecificData("João", "Silva", "joao.silva@email.com"),
                CreateCustomerWithSpecificData("Maria", "Silva", "maria.silva@email.com"),
                CreateCustomerWithSpecificData("Pedro", "Santos", "pedro.santos@email.com"),
                CreateCustomerWithSpecificData("Ana", "Costa", "ana.costa@email.com"),
                CreateCustomerWithSpecificData("João", "Santos", "joao.santos@email.com")
            };
        }
    }

    #endregion

    #region Random Data Generators

    /// <summary>
    /// Gera email único para evitar conflitos em testes
    /// </summary>
    public static string GenerateUniqueEmail(string? baseName = null)
    {
        baseName ??= _faker.Person.FirstName.ToLower();
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        return $"{baseName}.{timestamp}@test.com";
    }

    /// <summary>
    /// Gera telefone no formato brasileiro
    /// </summary>
    public static string GenerateBrazilianPhone()
    {
        return _faker.Phone.PhoneNumber("(##) #####-####");
    }

    /// <summary>
    /// Gera CEP brasileiro válido
    /// </summary>
    public static string GenerateBrazilianZipCode()
    {
        return _faker.Address.ZipCode("########");
    }

    /// <summary>
    /// Gera dados de endereço brasileiro aleatório
    /// </summary>
    public static Address GenerateRandomBrazilianAddress()
    {
        var cities = new[] { "São Paulo", "Rio de Janeiro", "Belo Horizonte", "Salvador", "Brasília" };
        var states = new[] { "SP", "RJ", "MG", "BA", "DF" };
        var neighborhoods = new[] { "Centro", "Vila Nova", "Jardim das Flores", "Alto da Colina" };

        var cityIndex = _faker.Random.Int(0, cities.Length - 1);
        
        return new Address(
            _faker.Address.StreetName(),
            _faker.Address.BuildingNumber(),
            _faker.PickRandom(neighborhoods),
            cities[cityIndex],
            states[cityIndex],
            GenerateBrazilianZipCode(),
            "Brasil");
    }

    #endregion
}
