using CustomerManagement.Application.DTOs;
using CustomerManagement.Infrastructure.Api;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using FluentAssertions;
using Xunit;
using System.Net;
using System.Net.Http.Json;
using WireMock.Server;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace CustomerManagement.Tests.IntegrationTests;

/// <summary>
/// Testes de integração para CustomerApiService
/// Testa comunicação real com APIs usando WireMock para simular serviços externos
/// </summary>
public class CustomerApiServiceIntegrationTests : IClassFixture<ApiTestFixture>, IDisposable
{
    private readonly ApiTestFixture _fixture;
    private readonly HttpClient _httpClient;
    private readonly WireMockServer _mockServer;
    private readonly CustomerApiService _apiService;

    public CustomerApiServiceIntegrationTests(ApiTestFixture fixture)
    {
        _fixture = fixture;
        _httpClient = _fixture.CreateClient();
        
        // Setup WireMock server para simular API externa
        _mockServer = WireMockServer.Start();
        
        // Configurar o ApiService para usar o mock server
        var serviceProvider = _fixture.Services;
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        
        _apiService = new CustomerApiService(
            _httpClient,
            Microsoft.Extensions.Options.Options.Create(new ApiConfiguration 
            { 
                BaseUrl = _mockServer.Urls[0],
                Timeout = TimeSpan.FromSeconds(30),
                ApiKey = "test-api-key"
            }));
    }

    #region GetCustomersAsync Integration Tests

    [Fact]
    public async Task GetCustomersAsync_WithValidResponse_ShouldDeserializeCorrectly()
    {
        // Arrange
        var expectedCustomers = new[]
        {
            CreateSampleCustomerDto(),
            CreateSampleCustomerDto(Guid.NewGuid(), "Maria", "Santos", "maria@email.com")
        };

        var response = new PagedResult<CustomerDto>
        {
            Items = expectedCustomers,
            Page = 1,
            PageSize = 20,
            TotalCount = 2,
            TotalPages = 1
        };

        _mockServer
            .Given(Request.Create()
                .WithPath("/api/customers")
                .WithParam("page", "1")
                .WithParam("pageSize", "20")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBodyAsJson(response));

        // Act
        var result = await _apiService.GetCustomersAsync(page: 1, pageSize: 20);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
        result.Page.Should().Be(1);
        
        var firstCustomer = result.Items.First();
        firstCustomer.FirstName.Should().Be("João");
        firstCustomer.Email.Should().Be("joao@email.com");
    }

    [Fact]
    public async Task GetCustomersAsync_WithSearchFilter_ShouldPassParametersCorrectly()
    {
        // Arrange
        var searchText = "João";
        
        _mockServer
            .Given(Request.Create()
                .WithPath("/api/customers")
                .WithParam("search", searchText)
                .WithParam("page", "1")
                .WithParam("pageSize", "10")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new PagedResult<CustomerDto>
                {
                    Items = new[] { CreateSampleCustomerDto() },
                    Page = 1,
                    PageSize = 10,
                    TotalCount = 1
                }));

        // Act
        var result = await _apiService.GetCustomersAsync(searchText, 1, 10);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        
        // Verifica se os parâmetros foram enviados corretamente
        var requests = _mockServer.LogEntries.ToList();
        requests.Should().HaveCount(1);
        requests[0].RequestMessage.Query.Should().Contain("search=João");
    }

    [Theory]
    [InlineData(HttpStatusCode.NotFound)]
    [InlineData(HttpStatusCode.InternalServerError)]
    [InlineData(HttpStatusCode.ServiceUnavailable)]
    public async Task GetCustomersAsync_WithErrorResponse_ShouldThrowHttpRequestException(HttpStatusCode statusCode)
    {
        // Arrange
        _mockServer
            .Given(Request.Create()
                .WithPath("/api/customers")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode((int)statusCode)
                .WithBody("Server Error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            _apiService.GetCustomersAsync());

        exception.Message.Should().Contain(((int)statusCode).ToString());
    }

    #endregion

    #region GetCustomerByIdAsync Integration Tests

    [Fact]
    public async Task GetCustomerByIdAsync_WithExistingCustomer_ShouldReturnCorrectData()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var expectedCustomer = CreateSampleCustomerDto(customerId);

        _mockServer
            .Given(Request.Create()
                .WithPath($"/api/customers/{customerId}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(expectedCustomer));

        // Act
        var result = await _apiService.GetCustomerByIdAsync(customerId);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(customerId);
        result.Email.Should().Be(expectedCustomer.Email);
        result.FullName.Should().Be(expectedCustomer.FullName);
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithNonExistingCustomer_ShouldReturnNull()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _mockServer
            .Given(Request.Create()
                .WithPath($"/api/customers/{customerId}")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(404));

        // Act
        var result = await _apiService.GetCustomerByIdAsync(customerId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateCustomerAsync Integration Tests

    [Fact]
    public async Task CreateCustomerAsync_WithValidData_ShouldCreateAndReturnCustomer()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            FirstName = "João",
            LastName = "Silva",
            Email = "joao@email.com",
            Phone = "11999999999",
            Address = CreateSampleAddressDto(),
            Type = CustomerType.Standard
        };

        var createdCustomer = CreateSampleCustomerDto();
        
        _mockServer
            .Given(Request.Create()
                .WithPath("/api/customers")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(201)
                .WithHeader("Location", $"/api/customers/{createdCustomer.Id}")
                .WithBodyAsJson(createdCustomer));

        // Act
        var result = await _apiService.CreateCustomerAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be(createDto.FirstName);
        result.Email.Should().Be(createDto.Email);
        result.Status.Should().Be(CustomerStatus.Active);

        // Verifica se os dados foram enviados corretamente
        var requests = _mockServer.LogEntries.ToList();
        var requestBody = requests.Last().RequestMessage.Body;
        requestBody.Should().Contain("joao@email.com");
        requestBody.Should().Contain("João");
    }

    [Fact]
    public async Task CreateCustomerAsync_WithInvalidData_ShouldThrowValidationException()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            // Email inválido
            Email = "invalid-email"
        };

        _mockServer
            .Given(Request.Create()
                .WithPath("/api/customers")
                .UsingPost())
            .RespondWith(Response.Create()
                .WithStatusCode(400)
                .WithBodyAsJson(new 
                {
                    title = "Validation Error",
                    errors = new Dictionary<string, string[]>
                    {
                        ["Email"] = new[] { "Email format is invalid" }
                    }
                }));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            _apiService.CreateCustomerAsync(createDto));

        exception.Message.Should().Contain("400");
    }

    #endregion

    #region Authentication and Authorization Tests

    [Fact]
    public async Task ApiCalls_WithoutAuthToken_ShouldReturn401()
    {
        // Arrange
        var unauthorizedApiService = new CustomerApiService(
            _httpClient,
            Microsoft.Extensions.Options.Options.Create(new ApiConfiguration 
            { 
                BaseUrl = _mockServer.Urls[0],
                ApiKey = "" // Sem API key
            }));

        _mockServer
            .Given(Request.Create()
                .WithPath("/api/customers")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(401)
                .WithBody("Unauthorized"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<HttpRequestException>(() =>
            unauthorizedApiService.GetCustomersAsync());

        exception.Message.Should().Contain("401");
    }

    [Fact]
    public async Task ApiCalls_WithValidApiKey_ShouldIncludeAuthHeader()
    {
        // Arrange
        _mockServer
            .Given(Request.Create()
                .WithPath("/api/customers")
                .WithHeader("Authorization", "Bearer test-api-key")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new PagedResult<CustomerDto>
                {
                    Items = Array.Empty<CustomerDto>(),
                    Page = 1,
                    PageSize = 20,
                    TotalCount = 0
                }));

        // Act
        await _apiService.GetCustomersAsync();

        // Assert
        var requests = _mockServer.LogEntries.ToList();
        requests.Should().HaveCount(1);
        
        var authHeader = requests[0].RequestMessage.Headers
            .FirstOrDefault(h => h.Key.Equals("Authorization", StringComparison.OrdinalIgnoreCase));
        authHeader.Should().NotBeNull();
        authHeader.Value.Should().Contain("Bearer test-api-key");
    }

    #endregion

    #region Timeout and Retry Tests

    [Fact]
    public async Task ApiCalls_WithTimeout_ShouldThrowTimeoutException()
    {
        // Arrange
        _mockServer
            .Given(Request.Create()
                .WithPath("/api/customers")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithDelay(TimeSpan.FromSeconds(35)) // Maior que o timeout configurado
                .WithStatusCode(200));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<TaskCanceledException>(() =>
            _apiService.GetCustomersAsync());

        exception.Message.Should().Contain("timeout");
    }

    [Fact]
    public async Task ApiCalls_WithRetryPolicy_ShouldRetryOnTransientFailures()
    {
        // Arrange - Primeira chamada falha, segunda sucede
        _mockServer
            .Given(Request.Create()
                .WithPath("/api/customers")
                .UsingGet())
            .InScenario("retry-test")
            .WhenStateIs(WireMock.Admin.Mappings.Scenario.Started)
            .RespondWith(Response.Create()
                .WithStatusCode(503) // Service Unavailable
                .WithBody("Service Temporarily Unavailable"))
            .WillSetStateTo("second-call");

        _mockServer
            .Given(Request.Create()
                .WithPath("/api/customers")
                .UsingGet())
            .InScenario("retry-test")
            .WhenStateIs("second-call")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBodyAsJson(new PagedResult<CustomerDto>
                {
                    Items = Array.Empty<CustomerDto>(),
                    Page = 1,
                    PageSize = 20,
                    TotalCount = 0
                }));

        // Act
        var result = await _apiService.GetCustomersAsync();

        // Assert
        result.Should().NotBeNull();
        var requests = _mockServer.LogEntries.ToList();
        requests.Should().HaveCount(2); // Primeira chamada + retry
    }

    #endregion

    #region Helper Methods

    private CustomerDto CreateSampleCustomerDto(
        Guid? id = null, 
        string firstName = "João", 
        string lastName = "Silva", 
        string email = "joao@email.com")
    {
        return new CustomerDto
        {
            Id = id ?? Guid.NewGuid(),
            FirstName = firstName,
            LastName = lastName,
            FullName = $"{firstName} {lastName}",
            Email = email,
            Phone = "11999999999",
            Address = CreateSampleAddressDto(),
            Type = CustomerType.Standard,
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    private AddressDto CreateSampleAddressDto()
    {
        return new AddressDto(
            "Rua das Flores", "123", "Centro",
            "São Paulo", "SP", "01234567", "Brasil");
    }

    #endregion

    public void Dispose()
    {
        _mockServer?.Stop();
        _mockServer?.Dispose();
    }
}

/// <summary>
/// Fixture para configurar o ambiente de testes de integração
/// </summary>
public class ApiTestFixture : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        // Configurar o host para testes
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ApiConfiguration:BaseUrl"] = "http://localhost:5000",
                ["ApiConfiguration:Timeout"] = "00:00:30",
                ["ApiConfiguration:ApiKey"] = "test-api-key",
                ["Logging:LogLevel:Default"] = "Warning"
            });
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Configurar serviços específicos para testes
            services.Configure<ApiConfiguration>(options =>
            {
                options.BaseUrl = "http://localhost:5000";
                options.Timeout = TimeSpan.FromSeconds(30);
                options.ApiKey = "test-api-key";
            });
        });
    }
}

/// <summary>
/// Configuração da API para testes
/// </summary>
public class ApiConfiguration
{
    public string BaseUrl { get; set; } = string.Empty;
    public TimeSpan Timeout { get; set; }
    public string ApiKey { get; set; } = string.Empty;
}
