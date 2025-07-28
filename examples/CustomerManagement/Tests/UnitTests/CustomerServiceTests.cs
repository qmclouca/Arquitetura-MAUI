using CustomerManagement.Application.Services;
using CustomerManagement.Application.DTOs;
using CustomerManagement.Domain.Services;
using AutoMapper;
using Microsoft.Extensions.Logging;
using Moq;
using FluentAssertions;
using Xunit;

namespace CustomerManagement.Tests.UnitTests.Application;

/// <summary>
/// Testes unitários para CustomerService
/// Validam a orquestração de operações e coordenação com APIs
/// </summary>
public class CustomerServiceTests
{
    private readonly Mock<ICustomerApiService> _mockApiService;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ILogger<CustomerService>> _mockLogger;
    private readonly Mock<ICacheManager> _mockCache;
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _mockApiService = new Mock<ICustomerApiService>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<CustomerService>>();
        _mockCache = new Mock<ICacheManager>();

        _service = new CustomerService(
            _mockApiService.Object,
            _mockMapper.Object,
            _mockLogger.Object,
            _mockCache.Object);
    }

    #region GetCustomersAsync Tests

    [Fact]
    public async Task GetCustomersAsync_WithCacheHit_ShouldReturnCachedResult()
    {
        // Arrange
        var cachedResult = new PagedResult<CustomerDto>
        {
            Items = new[] { CreateSampleCustomerDto() },
            Page = 1,
            PageSize = 20,
            TotalCount = 1
        };

        _mockCache.Setup(x => x.GetAsync<PagedResult<CustomerDto>>(It.IsAny<string>()))
                  .ReturnsAsync(cachedResult);

        // Act
        var result = await _service.GetCustomersAsync(searchText: "João", page: 1, pageSize: 20);

        // Assert
        result.Should().Be(cachedResult);
        _mockApiService.Verify(x => x.GetCustomersAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), 
                              Times.Never);
        VerifyLogCalled("Retornando clientes do cache");
    }

    [Fact]
    public async Task GetCustomersAsync_WithCacheMiss_ShouldCallApiAndCache()
    {
        // Arrange
        var apiResult = new PagedResult<CustomerDto>
        {
            Items = new[] { CreateSampleCustomerDto() },
            Page = 1,
            PageSize = 20,
            TotalCount = 1
        };

        _mockCache.Setup(x => x.GetAsync<PagedResult<CustomerDto>>(It.IsAny<string>()))
                  .ReturnsAsync((PagedResult<CustomerDto>?)null);

        _mockApiService.Setup(x => x.GetCustomersAsync("João", 1, 20))
                      .ReturnsAsync(apiResult);

        // Act
        var result = await _service.GetCustomersAsync(searchText: "João", page: 1, pageSize: 20);

        // Assert
        result.Should().Be(apiResult);
        _mockCache.Verify(x => x.SetAsync(It.IsAny<string>(), apiResult, It.IsAny<TimeSpan>()), Times.Once);
        VerifyLogCalled("Buscando clientes da API");
    }

    [Fact]
    public async Task GetCustomersAsync_WhenApiThrows_ShouldRethrowException()
    {
        // Arrange
        var exception = new HttpRequestException("API Error");
        
        _mockCache.Setup(x => x.GetAsync<PagedResult<CustomerDto>>(It.IsAny<string>()))
                  .ReturnsAsync((PagedResult<CustomerDto>?)null);

        _mockApiService.Setup(x => x.GetCustomersAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                      .ThrowsAsync(exception);

        // Act & Assert
        var thrownException = await Assert.ThrowsAsync<HttpRequestException>(() =>
            _service.GetCustomersAsync());

        thrownException.Should().Be(exception);
        VerifyLogCalled("Erro ao buscar clientes", LogLevel.Error);
    }

    #endregion

    #region GetCustomerByIdAsync Tests

    [Fact]
    public async Task GetCustomerByIdAsync_WithExistingCustomer_ShouldReturnCustomer()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var expectedCustomer = CreateSampleCustomerDto(customerId);

        _mockApiService.Setup(x => x.GetCustomerByIdAsync(customerId))
                      .ReturnsAsync(expectedCustomer);

        // Act
        var result = await _service.GetCustomerByIdAsync(customerId);

        // Assert
        result.Should().Be(expectedCustomer);
        VerifyLogCalled($"Buscando cliente por ID: {customerId}");
    }

    [Fact]
    public async Task GetCustomerByIdAsync_WithNonExistingCustomer_ShouldReturnNull()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _mockApiService.Setup(x => x.GetCustomerByIdAsync(customerId))
                      .ReturnsAsync((CustomerDto?)null);

        // Act
        var result = await _service.GetCustomerByIdAsync(customerId);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateCustomerAsync Tests

    [Fact]
    public async Task CreateCustomerAsync_WithValidData_ShouldCreateAndInvalidateCache()
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

        _mockApiService.Setup(x => x.EmailExistsAsync(createDto.Email))
                      .ReturnsAsync(false);

        _mockApiService.Setup(x => x.CreateCustomerAsync(createDto))
                      .ReturnsAsync(createdCustomer);

        // Act
        var result = await _service.CreateCustomerAsync(createDto);

        // Assert
        result.Should().Be(createdCustomer);
        _mockCache.Verify(x => x.RemovePatternAsync("customers_page_*"), Times.Once);
        VerifyLogCalled($"Criando novo cliente - Email: {createDto.Email}");
        VerifyLogCalled("Cliente criado com sucesso");
    }

    [Fact]
    public async Task CreateCustomerAsync_WithExistingEmail_ShouldThrowException()
    {
        // Arrange
        var createDto = new CreateCustomerDto
        {
            Email = "existing@email.com"
        };

        _mockApiService.Setup(x => x.EmailExistsAsync(createDto.Email))
                      .ReturnsAsync(true);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.CreateCustomerAsync(createDto));

        exception.Message.Should().Contain("Já existe um cliente com o email");
        _mockApiService.Verify(x => x.CreateCustomerAsync(It.IsAny<CreateCustomerDto>()), Times.Never);
    }

    #endregion

    #region UpdateCustomerAsync Tests

    [Fact]
    public async Task UpdateCustomerAsync_WithValidData_ShouldUpdateAndInvalidateCache()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var updateDto = new UpdateCustomerDto
        {
            FirstName = "João Updated",
            LastName = "Silva Updated",
            Phone = "11888888888",
            Address = CreateSampleAddressDto(),
            Type = CustomerType.Premium
        };

        var existingCustomer = CreateSampleCustomerDto(customerId);
        var updatedCustomer = CreateSampleCustomerDto(customerId);
        updatedCustomer.FirstName = updateDto.FirstName;

        _mockApiService.Setup(x => x.GetCustomerByIdAsync(customerId))
                      .ReturnsAsync(existingCustomer);

        _mockApiService.Setup(x => x.UpdateCustomerAsync(customerId, updateDto))
                      .ReturnsAsync(updatedCustomer);

        // Act
        var result = await _service.UpdateCustomerAsync(customerId, updateDto);

        // Assert
        result.Should().Be(updatedCustomer);
        _mockCache.Verify(x => x.RemoveAsync($"customer_{customerId}"), Times.Once);
        _mockCache.Verify(x => x.RemovePatternAsync("customers_page_*"), Times.Once);
        VerifyLogCalled($"Atualizando cliente - ID: {customerId}");
    }

    [Fact]
    public async Task UpdateCustomerAsync_WithNonExistingCustomer_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var updateDto = new UpdateCustomerDto();

        _mockApiService.Setup(x => x.GetCustomerByIdAsync(customerId))
                      .ReturnsAsync((CustomerDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateCustomerAsync(customerId, updateDto));

        exception.Message.Should().Contain("não encontrado");
        _mockApiService.Verify(x => x.UpdateCustomerAsync(It.IsAny<Guid>(), It.IsAny<UpdateCustomerDto>()), Times.Never);
    }

    [Fact]
    public async Task UpdateCustomerAsync_WithDeletedCustomer_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var updateDto = new UpdateCustomerDto();
        var deletedCustomer = CreateSampleCustomerDto(customerId);
        deletedCustomer.IsDeleted = true;

        _mockApiService.Setup(x => x.GetCustomerByIdAsync(customerId))
                      .ReturnsAsync(deletedCustomer);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.UpdateCustomerAsync(customerId, updateDto));

        exception.Message.Should().Contain("cliente excluído");
    }

    #endregion

    #region DeleteCustomerAsync Tests

    [Fact]
    public async Task DeleteCustomerAsync_WithExistingCustomer_ShouldDeleteAndInvalidateCache()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var existingCustomer = CreateSampleCustomerDto(customerId);

        _mockApiService.Setup(x => x.GetCustomerByIdAsync(customerId))
                      .ReturnsAsync(existingCustomer);

        _mockApiService.Setup(x => x.DeleteCustomerAsync(customerId))
                      .Returns(Task.CompletedTask);

        // Act
        await _service.DeleteCustomerAsync(customerId);

        // Assert
        _mockApiService.Verify(x => x.DeleteCustomerAsync(customerId), Times.Once);
        _mockCache.Verify(x => x.RemoveAsync($"customer_{customerId}"), Times.Once);
        _mockCache.Verify(x => x.RemovePatternAsync("customers_page_*"), Times.Once);
        VerifyLogCalled($"Excluindo cliente - ID: {customerId}");
    }

    [Fact]
    public async Task DeleteCustomerAsync_WithNonExistingCustomer_ShouldThrowException()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        _mockApiService.Setup(x => x.GetCustomerByIdAsync(customerId))
                      .ReturnsAsync((CustomerDto?)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.DeleteCustomerAsync(customerId));

        exception.Message.Should().Contain("não encontrado");
        _mockApiService.Verify(x => x.DeleteCustomerAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task DeleteCustomerAsync_WithAlreadyDeletedCustomer_ShouldNotCallApiAgain()
    {
        // Arrange
        var customerId = Guid.NewGuid();
        var deletedCustomer = CreateSampleCustomerDto(customerId);
        deletedCustomer.IsDeleted = true;

        _mockApiService.Setup(x => x.GetCustomerByIdAsync(customerId))
                      .ReturnsAsync(deletedCustomer);

        // Act
        await _service.DeleteCustomerAsync(customerId);

        // Assert
        _mockApiService.Verify(x => x.DeleteCustomerAsync(It.IsAny<Guid>()), Times.Never);
        VerifyLogCalled("Tentativa de excluir cliente já excluído", LogLevel.Warning);
    }

    #endregion

    #region Helper Methods

    private CustomerDto CreateSampleCustomerDto(Guid? id = null)
    {
        return new CustomerDto
        {
            Id = id ?? Guid.NewGuid(),
            FirstName = "João",
            LastName = "Silva",
            FullName = "João Silva",
            Email = "joao@email.com",
            Phone = "11999999999",
            Address = CreateSampleAddressDto(),
            Type = CustomerType.Standard,
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };
    }

    private AddressDto CreateSampleAddressDto()
    {
        return new AddressDto(
            "Rua das Flores", "123", "Centro",
            "São Paulo", "SP", "01234567", "Brasil");
    }

    private void VerifyLogCalled(string message, LogLevel level = LogLevel.Information)
    {
        _mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.AtLeastOnce);
    }

    #endregion
}
