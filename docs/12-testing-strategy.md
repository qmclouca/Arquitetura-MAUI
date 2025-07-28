# Estratégia de Testes - Arquitetura MAUI Desktop

## 🧪 **Visão Geral da Estratégia de Testes**

A estratégia de testes para aplicações MAUI Desktop com microserviços segue a **Pirâmide de Testes**, priorizando testes rápidos e confiáveis com diferentes níveis de granularidade.

## 🏗️ **Pirâmide de Testes**

```
           🔺 UI Tests (E2E)
          📊 Integration Tests  
         🏃‍♂️ Unit Tests (Base da pirâmide)
```

### **Distribuição Recomendada:**
- **70%** - Unit Tests (Rápidos, isolados)
- **20%** - Integration Tests (Médios, componentes)
- **10%** - UI Tests (Lentos, end-to-end)

## 🏃‍♂️ **Unit Tests (Testes Unitários)**

### **Objetivo:**
Testar componentes individuais em isolamento, sem dependências externas.

### **Tecnologias:**
- **xUnit** ou **NUnit** - Framework de testes
- **Moq** ou **NSubstitute** - Mocking framework
- **FluentAssertions** - Assertions mais legíveis
- **AutoFixture** - Geração automática de dados de teste

### **Cobertura:**

#### **1. Domain Layer Tests**
```csharp
[Test]
public class CustomerTests
{
    [Test]
    public void Create_WithValidData_ShouldCreateCustomer()
    {
        // Arrange
        var firstName = "João";
        var lastName = "Silva";
        var email = "joao@email.com";
        var phone = "11999999999";
        var address = new Address("Rua A", "123", "Centro", "São Paulo", "SP", "01234567");

        // Act
        var customer = Customer.Create(firstName, lastName, email, phone, address);

        // Assert
        customer.Should().NotBeNull();
        customer.FirstName.Should().Be(firstName);
        customer.Email.Value.Should().Be(email.ToLowerInvariant());
        customer.Status.Should().Be(CustomerStatus.Active);
        customer.DomainEvents.Should().ContainSingle(e => e is CustomerCreatedEvent);
    }

    [Test]
    public void Create_WithInvalidEmail_ShouldThrowException()
    {
        // Arrange & Act & Assert
        Assert.Throws<ArgumentException>(() => 
            Customer.Create("João", "Silva", "email-inválido", "11999999999", validAddress));
    }

    [Test]
    public void UpdateBasicInfo_WhenDeleted_ShouldThrowException()
    {
        // Arrange
        var customer = Customer.Create("João", "Silva", "joao@email.com", "11999999999", validAddress);
        customer.MarkAsDeleted();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => 
            customer.UpdateBasicInfo("Pedro", "Santos", "11888888888"));
    }
}
```

#### **2. Value Object Tests**
```csharp
[Test]
public class EmailTests
{
    [Test]
    [TestCase("user@domain.com")]
    [TestCase("test.email@subdomain.domain.co.uk")]
    [TestCase("user+tag@domain.com")]
    public void Create_WithValidEmail_ShouldCreateEmail(string validEmail)
    {
        // Act
        var email = Email.Create(validEmail);

        // Assert
        email.Value.Should().Be(validEmail.ToLowerInvariant());
    }

    [Test]
    [TestCase("")]
    [TestCase("invalid-email")]
    [TestCase("@domain.com")]
    [TestCase("user@")]
    public void Create_WithInvalidEmail_ShouldThrowException(string invalidEmail)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() => Email.Create(invalidEmail));
    }
}
```

#### **3. Application Service Tests**
```csharp
[Test]
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

    [Test]
    public async Task GetCustomersAsync_WithCache_ShouldReturnCachedResult()
    {
        // Arrange
        var cachedResult = new PagedResult<CustomerDto> 
        { 
            Items = new[] { new CustomerDto { Id = Guid.NewGuid() } },
            TotalCount = 1 
        };
        
        _mockCache.Setup(x => x.GetAsync<PagedResult<CustomerDto>>(It.IsAny<string>()))
                  .ReturnsAsync(cachedResult);

        // Act
        var result = await _service.GetCustomersAsync(page: 1, pageSize: 20);

        // Assert
        result.Should().Be(cachedResult);
        _mockApiService.Verify(x => x.GetCustomersAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()), 
                              Times.Never);
    }

    [Test]
    public async Task CreateCustomerAsync_WithValidData_ShouldCallApiAndInvalidateCache()
    {
        // Arrange
        var createDto = new CreateCustomerDto 
        { 
            FirstName = "João", 
            Email = "joao@email.com" 
        };
        var createdCustomer = new CustomerDto { Id = Guid.NewGuid() };
        
        _mockApiService.Setup(x => x.CreateCustomerAsync(createDto))
                      .ReturnsAsync(createdCustomer);

        // Act
        var result = await _service.CreateCustomerAsync(createDto);

        // Assert
        result.Should().Be(createdCustomer);
        _mockCache.Verify(x => x.RemovePatternAsync("customers_page_*"), Times.Once);
    }
}
```

#### **4. ViewModel Tests**
```csharp
[Test]
public class CustomerListViewModelTests
{
    private readonly Mock<ICustomerService> _mockCustomerService;
    private readonly Mock<ILogger<CustomerListViewModel>> _mockLogger;
    private readonly CustomerListViewModel _viewModel;

    public CustomerListViewModelTests()
    {
        _mockCustomerService = new Mock<ICustomerService>();
        _mockLogger = new Mock<ILogger<CustomerListViewModel>>();
        _viewModel = new CustomerListViewModel(_mockCustomerService.Object, _mockLogger.Object);
    }

    [Test]
    public async Task LoadCustomersCommand_ShouldLoadCustomersAndUpdateProperties()
    {
        // Arrange
        var customers = new PagedResult<CustomerDto>
        {
            Items = new[] 
            { 
                new CustomerDto { Id = Guid.NewGuid(), FirstName = "João" },
                new CustomerDto { Id = Guid.NewGuid(), FirstName = "Maria" }
            },
            TotalCount = 2
        };

        _mockCustomerService.Setup(x => x.GetCustomersAsync(null, 1, 20))
                          .ReturnsAsync(customers);

        // Act
        await _viewModel.LoadCustomersCommand.ExecuteAsync(null);

        // Assert
        _viewModel.Customers.Should().HaveCount(2);
        _viewModel.TotalRecords.Should().Be(2);
        _viewModel.IsLoading.Should().BeFalse();
        _viewModel.HasError.Should().BeFalse();
    }

    [Test]
    public async Task LoadCustomersCommand_WhenServiceThrows_ShouldSetErrorState()
    {
        // Arrange
        _mockCustomerService.Setup(x => x.GetCustomersAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>()))
                          .ThrowsAsync(new HttpRequestException("API Error"));

        // Act
        await _viewModel.LoadCustomersCommand.ExecuteAsync(null);

        // Assert
        _viewModel.HasError.Should().BeTrue();
        _viewModel.ErrorMessage.Should().NotBeEmpty();
        _viewModel.IsLoading.Should().BeFalse();
    }
}
```

## 📊 **Integration Tests (Testes de Integração)**

### **Objetivo:**
Testar a integração entre componentes, incluindo comunicação com APIs externas.

### **Cobertura:**

#### **1. API Service Integration Tests**
```csharp
[Test]
public class CustomerApiServiceIntegrationTests
{
    private readonly HttpClient _httpClient;
    private readonly Mock<IAuthenticationManager> _mockAuthManager;
    private readonly Mock<ICacheManager> _mockCache;
    private readonly CustomerApiService _service;

    public CustomerApiServiceIntegrationTests()
    {
        // Configurar HttpClient com handler de teste
        var handler = new MockHttpMessageHandler();
        _httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.test.com/") };
        
        _mockAuthManager = new Mock<IAuthenticationManager>();
        _mockCache = new Mock<ICacheManager>();
        
        _service = new CustomerApiService(_httpClient, Mock.Of<ILogger<CustomerApiService>>(), 
                                        _mockAuthManager.Object, _mockCache.Object);
    }

    [Test]
    public async Task GetCustomersAsync_WithValidResponse_ShouldReturnCustomers()
    {
        // Arrange
        var expectedResponse = new PagedResult<CustomerDto>
        {
            Items = new[] { new CustomerDto { Id = Guid.NewGuid(), FirstName = "João" } },
            TotalCount = 1
        };

        var handler = new MockHttpMessageHandler();
        handler.SetupRequest(HttpMethod.Get, "customers?page=1&pageSize=20")
               .ReturnsResponse(JsonSerializer.Serialize(expectedResponse), "application/json");

        _mockAuthManager.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("valid-token");
        _mockAuthManager.Setup(x => x.IsAuthenticated).Returns(true);

        // Act
        var result = await _service.GetCustomersAsync(page: 1, pageSize: 20);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }

    [Test]
    public async Task CreateCustomerAsync_WithUnauthorized_ShouldRefreshTokenAndRetry()
    {
        // Arrange
        var createDto = new CreateCustomerDto { FirstName = "João", Email = "joao@email.com" };
        
        var handler = new MockHttpMessageHandler();
        handler.SetupRequest(HttpMethod.Post, "customers")
               .ReturnsResponse(HttpStatusCode.Unauthorized) // Primeira tentativa
               .ReturnsResponse(new CustomerDto { Id = Guid.NewGuid() }, "application/json"); // Segunda tentativa

        _mockAuthManager.Setup(x => x.IsAuthenticated).Returns(true);
        _mockAuthManager.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync("valid-token");
        _mockAuthManager.Setup(x => x.RefreshTokenAsync()).ReturnsAsync(true);

        // Act
        var result = await _service.CreateCustomerAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        _mockAuthManager.Verify(x => x.RefreshTokenAsync(), Times.Once);
    }
}
```

#### **2. Cache Integration Tests**
```csharp
[Test]
public class CacheIntegrationTests
{
    [Test]
    public async Task MemoryCache_SetAndGet_ShouldWorkCorrectly()
    {
        // Arrange
        var cache = new MemoryCacheManager(new MemoryCache(new MemoryCacheOptions()));
        var key = "test-key";
        var value = new CustomerDto { Id = Guid.NewGuid(), FirstName = "João" };

        // Act
        await cache.SetAsync(key, value, TimeSpan.FromMinutes(5));
        var retrieved = await cache.GetAsync<CustomerDto>(key);

        // Assert
        retrieved.Should().NotBeNull();
        retrieved.Id.Should().Be(value.Id);
        retrieved.FirstName.Should().Be(value.FirstName);
    }

    [Test]
    public async Task Cache_WithExpiration_ShouldExpireCorrectly()
    {
        // Arrange
        var cache = new MemoryCacheManager(new MemoryCache(new MemoryCacheOptions()));
        var key = "expiring-key";
        var value = "test-value";

        // Act
        await cache.SetAsync(key, value, TimeSpan.FromMilliseconds(100));
        await Task.Delay(150); // Aguardar expiração
        var retrieved = await cache.GetAsync<string>(key);

        // Assert
        retrieved.Should().BeNull();
    }
}
```

## 🖥️ **UI Tests (Testes de Interface)**

### **Objetivo:**
Testar o comportamento da interface do usuário e fluxos end-to-end.

### **Tecnologias:**
- **Appium** - Automação de testes para aplicações móveis/desktop
- **WinAppDriver** - Para aplicações Windows Desktop
- **Selenium** (para componentes web dentro do MAUI)

### **Exemplo de UI Test:**
```csharp
[Test]
public class CustomerListUITests
{
    private AppiumDriver _driver;

    [SetUp]
    public void SetUp()
    {
        var options = new AppiumOptions();
        options.AddAdditionalCapability("app", "path/to/your/maui/app.exe");
        options.AddAdditionalCapability("deviceName", "WindowsPC");
        options.AddAdditionalCapability("platformName", "Windows");
        
        _driver = new WindowsDriver(new Uri("http://127.0.0.1:4723"), options);
    }

    [Test]
    public void CustomerList_LoadPage_ShouldDisplayCustomers()
    {
        // Arrange & Act
        var customerListPage = new CustomerListPage(_driver);
        customerListPage.NavigateTo();

        // Assert
        customerListPage.CustomersGrid.Should().BeDisplayed();
        customerListPage.LoadMoreButton.Should().BeDisplayed();
        customerListPage.SearchBox.Should().BeDisplayed();
    }

    [Test]
    public void CustomerList_SearchCustomer_ShouldFilterResults()
    {
        // Arrange
        var customerListPage = new CustomerListPage(_driver);
        customerListPage.NavigateTo();

        // Act
        customerListPage.SearchBox.SendKeys("João");
        customerListPage.SearchButton.Click();

        // Assert
        customerListPage.WaitForSearchResults();
        customerListPage.CustomerRows.Should().Contain(row => row.Text.Contains("João"));
    }
}
```

## 🌐 **API Contract Tests**

### **Objetivo:**
Validar que as APIs dos microserviços atendem aos contratos esperados.

```csharp
[Test]
public class CustomerApiContractTests
{
    private readonly HttpClient _httpClient;

    public CustomerApiContractTests()
    {
        _httpClient = new HttpClient { BaseAddress = new Uri("https://api.staging.com/") };
    }

    [Test]
    public async Task GetCustomers_ShouldReturnValidContract()
    {
        // Act
        var response = await _httpClient.GetAsync("api/v1/customers?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<PagedResult<CustomerDto>>(content);
        
        result.Should().NotBeNull();
        result.Items.Should().NotBeNull();
        result.TotalCount.Should().BeGreaterOrEqualTo(0);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Test]
    public async Task CreateCustomer_WithInvalidData_ShouldReturn400()
    {
        // Arrange
        var invalidCustomer = new { FirstName = "", Email = "invalid-email" };
        var content = new StringContent(JsonSerializer.Serialize(invalidCustomer), Encoding.UTF8, "application/json");

        // Act
        var response = await _httpClient.PostAsync("api/v1/customers", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        
        var errorContent = await response.Content.ReadAsStringAsync();
        errorContent.Should().Contain("FirstName");
        errorContent.Should().Contain("Email");
    }
}
```

## 📋 **Test Data Builders**

### **Padrão Builder para Dados de Teste:**
```csharp
public class CustomerDtoBuilder
{
    private Guid _id = Guid.NewGuid();
    private string _firstName = "João";
    private string _lastName = "Silva";
    private string _email = "joao@email.com";
    private CustomerType _type = CustomerType.Standard;
    private CustomerStatus _status = CustomerStatus.Active;

    public CustomerDtoBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public CustomerDtoBuilder WithName(string firstName, string lastName)
    {
        _firstName = firstName;
        _lastName = lastName;
        return this;
    }

    public CustomerDtoBuilder WithEmail(string email)
    {
        _email = email;
        return this;
    }

    public CustomerDtoBuilder AsPremium()
    {
        _type = CustomerType.Premium;
        return this;
    }

    public CustomerDtoBuilder AsInactive()
    {
        _status = CustomerStatus.Inactive;
        return this;
    }

    public CustomerDto Build()
    {
        return new CustomerDto
        {
            Id = _id,
            FirstName = _firstName,
            LastName = _lastName,
            FullName = $"{_firstName} {_lastName}",
            Email = _email,
            Type = _type,
            Status = _status,
            CreatedAt = DateTime.UtcNow
        };
    }
}

// Uso nos testes:
var customer = new CustomerDtoBuilder()
    .WithName("Maria", "Santos")
    .WithEmail("maria@email.com")
    .AsPremium()
    .Build();
```

## 🔧 **Configuração de Teste**

### **Test Project Structure:**
```
📁 Tests/
├── 📁 UnitTests/
│   ├── 📁 Domain/
│   ├── 📁 Application/
│   ├── 📁 ViewModels/
│   └── 📁 ValueObjects/
├── 📁 IntegrationTests/
│   ├── 📁 ApiServices/
│   ├── 📁 Authentication/
│   └── 📁 Cache/
├── 📁 UITests/
│   ├── 📁 Pages/
│   └── 📁 Flows/
├── 📁 ApiTests/
│   └── 📁 Contracts/
└── 📁 TestInfrastructure/
    ├── 📁 Builders/
    ├── 📁 Fixtures/
    └── 📁 Mocks/
```

### **Dependencies (PackageReference):**
```xml
<PackageReference Include="xunit" Version="2.4.2" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.4.5" />
<PackageReference Include="Moq" Version="4.20.69" />
<PackageReference Include="FluentAssertions" Version="6.12.0" />
<PackageReference Include="AutoFixture" Version="4.18.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
<PackageReference Include="Appium.WebDriver" Version="4.4.0" />
<PackageReference Include="coverlet.collector" Version="6.0.0" />
```

## 📊 **Métricas de Qualidade**

### **Cobertura de Código:**
- **Line Coverage**: > 80%
- **Branch Coverage**: > 70%
- **Method Coverage**: > 90%
- **Class Coverage**: > 85%

### **Performance de Testes:**
- **Unit Tests**: < 1 segundo por teste
- **Integration Tests**: < 5 segundos por teste
- **UI Tests**: < 30 segundos por cenário

### **CI/CD Integration:**
```yaml
# Azure DevOps Pipeline exemplo
- task: DotNetCoreCLI@2
  displayName: 'Run Unit Tests'
  inputs:
    command: 'test'
    projects: '**/*UnitTests*.csproj'
    arguments: '--configuration Release --collect:"XPlat Code Coverage" --logger trx --results-directory $(Agent.TempDirectory)'

- task: DotNetCoreCLI@2
  displayName: 'Run Integration Tests'
  inputs:
    command: 'test'
    projects: '**/*IntegrationTests*.csproj'
    arguments: '--configuration Release --logger trx --results-directory $(Agent.TempDirectory)'

- task: PublishCodeCoverageResults@1
  displayName: 'Publish Code Coverage'
  inputs:
    codeCoverageTool: 'Cobertura'
    summaryFileLocation: '$(Agent.TempDirectory)/**/coverage.cobertura.xml'
```

## 🎯 **Benefícios da Estratégia**

### **✅ Qualidade:**
- Detecção precoce de bugs
- Refatoração segura
- Documentação viva do código

### **✅ Confiança:**
- Deploy com segurança
- Mudanças sem medo
- Validação automática

### **✅ Manutenibilidade:**
- Código autodocumentado
- Casos de uso claros
- Regressões evitadas

### **✅ Performance:**
- Testes rápidos no desenvolvimento
- Feedback imediato
- Pipeline otimizado

Esta estratégia de testes garante alta qualidade, confiabilidade e manutenibilidade para aplicações MAUI Desktop que consomem microserviços .NET Core! 🧪✨
