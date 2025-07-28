using CustomerManagement.Application.DTOs;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FluentAssertions;
using Xunit;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CustomerManagement.Tests.IntegrationTests;

/// <summary>
/// Testes de contrato de API para validar endpoints e contratos
/// Garantem que as APIs atendem aos contratos definidos
/// </summary>
public class CustomerApiContractTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonOptions;

    public CustomerApiContractTests(ApiTestFixture fixture)
    {
        _client = fixture.CreateClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    #region GET /api/customers Contract Tests

    [Fact]
    public async Task GetCustomers_ShouldReturnCorrectContractStructure()
    {
        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/json");

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<CustomerDto>>(json, _jsonOptions);

        // Validar estrutura do contrato
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.Page.Should().BeGreaterOrEqualTo(1);
        result.PageSize.Should().BeGreaterThan(0);
        result.TotalCount.Should().BeGreaterOrEqualTo(0);
        result.TotalPages.Should().BeGreaterOrEqualTo(0);

        // Validar estrutura dos itens se existirem
        if (result.Items.Any())
        {
            var firstCustomer = result.Items.First();
            ValidateCustomerDtoContract(firstCustomer);
        }
    }

    [Theory]
    [InlineData("?page=1&pageSize=10")]
    [InlineData("?page=2&pageSize=5")]
    [InlineData("?search=João&page=1&pageSize=20")]
    [InlineData("?page=1&pageSize=50")]
    public async Task GetCustomers_WithQueryParameters_ShouldReturnValidContract(string queryParams)
    {
        // Act
        var response = await _client.GetAsync($"/api/customers{queryParams}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<CustomerDto>>(json, _jsonOptions);

        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
    }

    [Theory]
    [InlineData("?page=0")] // Página inválida
    [InlineData("?pageSize=0")] // PageSize inválido
    [InlineData("?page=-1")] // Página negativa
    [InlineData("?pageSize=1001")] // PageSize muito grande
    public async Task GetCustomers_WithInvalidParameters_ShouldReturnBadRequest(string queryParams)
    {
        // Act
        var response = await _client.GetAsync($"/api/customers{queryParams}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var json = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ApiErrorResponse>(json, _jsonOptions);

        error.Should().NotBeNull();
        error.Title.Should().NotBeEmpty();
        error.Errors.Should().NotBeNull();
    }

    #endregion

    #region GET /api/customers/{id} Contract Tests

    [Fact]
    public async Task GetCustomerById_WithExistingId_ShouldReturnValidContract()
    {
        // Arrange - Primeiro obter um cliente existente
        var customersResponse = await _client.GetAsync("/api/customers");
        var customersJson = await customersResponse.Content.ReadAsStringAsync();
        var customers = JsonSerializer.Deserialize<PagedResult<CustomerDto>>(customersJson, _jsonOptions);

        if (!customers.Items.Any())
        {
            // Skip se não há customers
            return;
        }

        var existingCustomerId = customers.Items.First().Id;

        // Act
        var response = await _client.GetAsync($"/api/customers/{existingCustomerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var customer = JsonSerializer.Deserialize<CustomerDto>(json, _jsonOptions);

        customer.Should().NotBeNull();
        customer.Id.Should().Be(existingCustomerId);
        ValidateCustomerDtoContract(customer);
    }

    [Fact]
    public async Task GetCustomerById_WithNonExistingId_ShouldReturn404()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/customers/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData("invalid-guid")]
    [InlineData("123")]
    [InlineData("")]
    public async Task GetCustomerById_WithInvalidId_ShouldReturnBadRequest(string invalidId)
    {
        // Act
        var response = await _client.GetAsync($"/api/customers/{invalidId}");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    #endregion

    #region POST /api/customers Contract Tests

    [Fact]
    public async Task CreateCustomer_WithValidData_ShouldReturnCreatedWithValidContract()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            FirstName = "Contrato",
            LastName = "Teste",
            Email = $"contrato.teste.{DateTime.Now.Ticks}@email.com",
            Phone = "11987654321",
            Address = new AddressDto(
                "Rua dos Contratos", "123", "Centro",
                "São Paulo", "SP", "01234567", "Brasil"),
            Type = CustomerType.Standard
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        // Verificar Location header
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.ToString().Should().Contain("/api/customers/");

        var json = await response.Content.ReadAsStringAsync();
        var createdCustomer = JsonSerializer.Deserialize<CustomerDto>(json, _jsonOptions);

        createdCustomer.Should().NotBeNull();
        createdCustomer.Id.Should().NotBeEmpty();
        createdCustomer.FirstName.Should().Be(createDto.FirstName);
        createdCustomer.Email.Should().Be(createDto.Email);
        createdCustomer.Status.Should().Be(CustomerStatus.Active);
        createdCustomer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));

        ValidateCustomerDtoContract(createdCustomer);
    }

    [Theory]
    [InlineData("", "Silva", "valid@email.com", "11999999999")] // FirstName vazio
    [InlineData("João", "", "valid@email.com", "11999999999")] // LastName vazio
    [InlineData("João", "Silva", "", "11999999999")] // Email vazio
    [InlineData("João", "Silva", "invalid-email", "11999999999")] // Email inválido
    [InlineData("João", "Silva", "valid@email.com", "")] // Phone vazio
    public async Task CreateCustomer_WithInvalidData_ShouldReturnBadRequestWithValidationErrors(
        string firstName, string lastName, string email, string phone)
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Phone = phone,
            Address = new AddressDto(
                "Rua Teste", "123", "Centro",
                "São Paulo", "SP", "01234567", "Brasil"),
            Type = CustomerType.Standard
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/customers", createDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var json = await response.Content.ReadAsStringAsync();
        var error = JsonSerializer.Deserialize<ApiErrorResponse>(json, _jsonOptions);

        error.Should().NotBeNull();
        error.Title.Should().NotBeEmpty();
        error.Errors.Should().NotBeNull();
        error.Errors.Should().NotBeEmpty();
    }

    #endregion

    #region PUT /api/customers/{id} Contract Tests

    [Fact]
    public async Task UpdateCustomer_WithValidData_ShouldReturnOkWithValidContract()
    {
        // Arrange - Criar um cliente primeiro
        var createDto = new CreateCustomerDto
        {
            FirstName = "Update",
            LastName = "Test",
            Email = $"update.test.{DateTime.Now.Ticks}@email.com",
            Phone = "11987654321",
            Address = new AddressDto(
                "Rua Original", "123", "Centro",
                "São Paulo", "SP", "01234567", "Brasil"),
            Type = CustomerType.Standard
        };

        var createResponse = await _client.PostAsJsonAsync("/api/customers", createDto);
        var createJson = await createResponse.Content.ReadAsStringAsync();
        var createdCustomer = JsonSerializer.Deserialize<CustomerDto>(createJson, _jsonOptions);

        var updateDto = new UpdateCustomerDto
        {
            FirstName = "Updated",
            LastName = "Customer",
            Phone = "11888888888",
            Address = new AddressDto(
                "Rua Atualizada", "456", "Centro",
                "São Paulo", "SP", "01234567", "Brasil"),
            Type = CustomerType.Premium
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/customers/{createdCustomer.Id}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        var updatedCustomer = JsonSerializer.Deserialize<CustomerDto>(json, _jsonOptions);

        updatedCustomer.Should().NotBeNull();
        updatedCustomer.Id.Should().Be(createdCustomer.Id);
        updatedCustomer.FirstName.Should().Be(updateDto.FirstName);
        updatedCustomer.Phone.Should().Be(updateDto.Phone);
        updatedCustomer.Type.Should().Be(updateDto.Type);
        updatedCustomer.UpdatedAt.Should().BeAfter(createdCustomer.CreatedAt);

        ValidateCustomerDtoContract(updatedCustomer);
    }

    [Fact]
    public async Task UpdateCustomer_WithNonExistingId_ShouldReturn404()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();
        var updateDto = new UpdateCustomerDto
        {
            FirstName = "Updated",
            LastName = "Customer"
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/customers/{nonExistingId}", updateDto);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region DELETE /api/customers/{id} Contract Tests

    [Fact]
    public async Task DeleteCustomer_WithExistingId_ShouldReturnNoContent()
    {
        // Arrange - Criar um cliente primeiro
        var createDto = new CreateCustomerDto
        {
            FirstName = "Delete",
            LastName = "Test",
            Email = $"delete.test.{DateTime.Now.Ticks}@email.com",
            Phone = "11987654321",
            Address = new AddressDto(
                "Rua Delete", "123", "Centro",
                "São Paulo", "SP", "01234567", "Brasil"),
            Type = CustomerType.Standard
        };

        var createResponse = await _client.PostAsJsonAsync("/api/customers", createDto);
        var createJson = await createResponse.Content.ReadAsStringAsync();
        var createdCustomer = JsonSerializer.Deserialize<CustomerDto>(createJson, _jsonOptions);

        // Act
        var response = await _client.DeleteAsync($"/api/customers/{createdCustomer.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verificar se o cliente foi marcado como deletado
        var getResponse = await _client.GetAsync($"/api/customers/{createdCustomer.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCustomer_WithNonExistingId_ShouldReturn404()
    {
        // Arrange
        var nonExistingId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/customers/{nonExistingId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Contract Validation Helpers

    private void ValidateCustomerDtoContract(CustomerDto customer)
    {
        // Validar propriedades obrigatórias
        customer.Id.Should().NotBeEmpty();
        customer.FirstName.Should().NotBeEmpty();
        customer.LastName.Should().NotBeEmpty();
        customer.FullName.Should().NotBeEmpty();
        customer.Email.Should().NotBeEmpty();
        customer.CreatedAt.Should().NotBe(default);

        // Validar formato do email
        customer.Email.Should().MatchRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        // Validar enums
        customer.Type.Should().BeOneOf(Enum.GetValues<CustomerType>());
        customer.Status.Should().BeOneOf(Enum.GetValues<CustomerStatus>());

        // Validar address se presente
        if (customer.Address != null)
        {
            ValidateAddressDtoContract(customer.Address);
        }

        // Validar campos de auditoria
        customer.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddMinutes(1));
        
        if (customer.UpdatedAt.HasValue)
        {
            customer.UpdatedAt.Value.Should().BeAfter(customer.CreatedAt);
        }

        // Validar FullName é combinação de FirstName e LastName
        customer.FullName.Should().Be($"{customer.FirstName} {customer.LastName}");
    }

    private void ValidateAddressDtoContract(AddressDto address)
    {
        address.Street.Should().NotBeEmpty();
        address.Number.Should().NotBeEmpty();
        address.City.Should().NotBeEmpty();
        address.State.Should().NotBeEmpty();
        address.PostalCode.Should().NotBeEmpty();
        address.Country.Should().NotBeEmpty();

        // Validar formato do CEP brasileiro se for Brasil
        if (address.Country.Equals("Brasil", StringComparison.OrdinalIgnoreCase))
        {
            address.PostalCode.Should().MatchRegex(@"^\d{8}$");
        }
    }

    #endregion
}

/// <summary>
/// Modelo para respostas de erro da API
/// </summary>
public class ApiErrorResponse
{
    public string Title { get; set; } = string.Empty;
    public int Status { get; set; }
    public Dictionary<string, string[]> Errors { get; set; } = new();
    public string TraceId { get; set; } = string.Empty;
}

/// <summary>
/// Testes de performance e carga para APIs
/// </summary>
public class CustomerApiPerformanceTests : IClassFixture<ApiTestFixture>
{
    private readonly HttpClient _client;

    public CustomerApiPerformanceTests(ApiTestFixture fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task GetCustomers_ResponseTime_ShouldBeLessThan500ms()
    {
        // Arrange
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        stopwatch.Stop();
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(500);
    }

    [Fact]
    public async Task GetCustomers_ConcurrentRequests_ShouldHandleLoad()
    {
        // Arrange
        const int concurrentRequests = 10;
        var tasks = new List<Task<HttpResponseMessage>>();

        // Act
        for (int i = 0; i < concurrentRequests; i++)
        {
            tasks.Add(_client.GetAsync("/api/customers"));
        }

        var responses = await Task.WhenAll(tasks);

        // Assert
        responses.Should().HaveCount(concurrentRequests);
        responses.Should().OnlyContain(r => r.StatusCode == HttpStatusCode.OK);
    }
}
