# 9. Estrutura do Projeto

## Visão Geral da Estrutura

Este documento apresenta a estrutura completa do projeto MAUI Desktop seguindo os princípios de Clean Architecture, DDD e MVVM.

## Estrutura de Pastas

```
📁 MauiDesktopApp/
├── 📁 src/
│   ├── 📁 MauiDesktopApp.Presentation/          # Camada de Apresentação
│   │   ├── 📁 Views/                           # Views XAML
│   │   ├── 📁 ViewModels/                      # ViewModels
│   │   ├── 📁 Controls/                        # Controles Customizados
│   │   ├── 📁 Converters/                      # Value Converters
│   │   ├── 📁 Behaviors/                       # Behaviors
│   │   ├── 📁 Resources/                       # Recursos (Styles, Templates)
│   │   ├── 📁 Services/                        # Serviços UI
│   │   └── 📁 Extensions/                      # Extensões
│   │
│   ├── 📁 MauiDesktopApp.Application/          # Camada de Aplicação
│   │   ├── 📁 Services/                        # Application Services
│   │   ├── 📁 DTOs/                           # Data Transfer Objects
│   │   ├── 📁 Commands/                        # Commands (CQRS)
│   │   ├── 📁 Queries/                         # Queries (CQRS)
│   │   ├── 📁 Validators/                      # Validadores
│   │   ├── 📁 Mappers/                         # Mappers/AutoMapper
│   │   ├── 📁 Interfaces/                      # Interfaces de Serviços
│   │   ├── 📁 Exceptions/                      # Exceções de Aplicação
│   │   └── 📁 Extensions/                      # Extensões
│   │
│   ├── 📁 MauiDesktopApp.Domain/               # Camada de Domínio
│   │   ├── 📁 Entities/                        # Entidades do Domínio
│   │   ├── 📁 ValueObjects/                    # Value Objects
│   │   ├── 📁 Aggregates/                      # Aggregate Roots
│   │   ├── 📁 Services/                        # Domain Services
│   │   ├── 📁 Repositories/                    # Interfaces de Repositórios
│   │   ├── 📁 Events/                          # Domain Events
│   │   ├── 📁 Specifications/                  # Specifications
│   │   ├── 📁 Factories/                       # Domain Factories
│   │   ├── 📁 Exceptions/                      # Domain Exceptions
│   │   └── 📁 Enums/                          # Enumerações
│   │
│   └── 📁 MauiDesktopApp.Infrastructure/       # Camada de Infraestrutura
│       ├── 📁 Data/                           # Data Access
│       │   ├── 📁 Context/                    # DbContext
│       │   ├── 📁 Configurations/             # Entity Configurations
│       │   ├── 📁 Repositories/               # Repository Implementations
│       │   ├── 📁 Migrations/                 # Database Migrations
│       │   └── 📁 Seed/                      # Data Seeding
│       ├── 📁 Services/                       # Infrastructure Services
│       ├── 📁 ExternalServices/               # External API Services
│       ├── 📁 Logging/                        # Logging Configuration
│       ├── 📁 Configuration/                  # App Configuration
│       ├── 📁 Security/                       # Security Services
│       ├── 📁 Caching/                        # Caching Services
│       ├── 📁 Messaging/                      # Event Messaging
│       └── 📁 Extensions/                     # DI Extensions
│
├── 📁 tests/
│   ├── 📁 MauiDesktopApp.UnitTests/           # Testes Unitários
│   ├── 📁 MauiDesktopApp.IntegrationTests/    # Testes de Integração
│   ├── 📁 MauiDesktopApp.ArchitectureTests/   # Testes de Arquitetura
│   └── 📁 MauiDesktopApp.UITests/             # Testes de UI
│
├── 📁 docs/                                   # Documentação
├── 📁 scripts/                                # Scripts de Build/Deploy
├── 📁 tools/                                  # Ferramentas de Desenvolvimento
├── 📀 .editorconfig                           # Configuração do Editor
├── 📀 .gitignore                              # Git Ignore
├── 📀 Directory.Build.props                   # Propriedades MSBuild
├── 📀 global.json                             # Versão do .NET
└── 📀 MauiDesktopApp.sln                     # Solution File
```

## Estrutura Detalhada das Camadas

### 9.1 Presentation Layer

```
📁 MauiDesktopApp.Presentation/
├── 📁 Views/
│   ├── 📁 Customer/
│   │   ├── CustomerListView.xaml
│   │   ├── CustomerListView.xaml.cs
│   │   ├── CustomerDetailView.xaml
│   │   └── CustomerDetailView.xaml.cs
│   ├── 📁 Order/
│   │   ├── OrderListView.xaml
│   │   ├── OrderListView.xaml.cs
│   │   ├── OrderDetailView.xaml
│   │   └── OrderDetailView.xaml.cs
│   ├── 📁 Shell/
│   │   ├── AppShell.xaml
│   │   └── AppShell.xaml.cs
│   └── 📁 Common/
│       ├── MainWindow.xaml
│       └── MainWindow.xaml.cs
│
├── 📁 ViewModels/
│   ├── 📁 Customer/
│   │   ├── CustomerListViewModel.cs
│   │   └── CustomerDetailViewModel.cs
│   ├── 📁 Order/
│   │   ├── OrderListViewModel.cs
│   │   └── OrderDetailViewModel.cs
│   ├── 📁 Base/
│   │   ├── BaseViewModel.cs
│   │   └── ViewModelLocator.cs
│   └── MainViewModel.cs
│
├── 📁 Controls/
│   ├── 📁 Common/
│   │   ├── Card.xaml
│   │   ├── LoadingIndicator.xaml
│   │   ├── Alert.xaml
│   │   └── EmptyState.xaml
│   ├── 📁 Input/
│   │   ├── SearchBox.xaml
│   │   ├── DatePickerField.xaml
│   │   └── NumericEntry.xaml
│   └── 📁 Layout/
│       ├── ResponsiveGrid.xaml
│       └── SidebarLayout.xaml
│
├── 📁 Converters/
│   ├── BoolToVisibilityConverter.cs
│   ├── DateTimeToStringConverter.cs
│   ├── MoneyToStringConverter.cs
│   └── StatusToColorConverter.cs
│
├── 📁 Behaviors/
│   ├── ValidationBehavior.cs
│   ├── EventToCommandBehavior.cs
│   └── NumericValidationBehavior.cs
│
├── 📁 Resources/
│   ├── 📁 Styles/
│   │   ├── Tokens.xaml
│   │   ├── Typography.xaml
│   │   ├── Buttons.xaml
│   │   ├── Inputs.xaml
│   │   └── App.xaml
│   ├── 📁 Templates/
│   │   ├── DataTemplates.xaml
│   │   └── ControlTemplates.xaml
│   ├── 📁 Images/
│   └── 📁 Fonts/
│
├── 📁 Services/
│   ├── INavigationService.cs
│   ├── NavigationService.cs
│   ├── IDialogService.cs
│   ├── DialogService.cs
│   ├── IThemeService.cs
│   └── ThemeService.cs
│
├── 📁 Extensions/
│   ├── ViewModelExtensions.cs
│   └── ServiceCollectionExtensions.cs
│
├── App.xaml
├── App.xaml.cs
├── MauiProgram.cs
└── Platforms/
    ├── Windows/
    ├── MacCatalyst/
    └── Tizen/
```

### 9.2 Application Layer

```
📁 MauiDesktopApp.Application/
├── 📁 Services/
│   ├── 📁 Customer/
│   │   ├── ICustomerService.cs
│   │   └── CustomerService.cs
│   ├── 📁 Order/
│   │   ├── IOrderService.cs
│   │   └── OrderService.cs
│   ├── 📁 Payment/
│   │   ├── IPaymentService.cs
│   │   └── PaymentService.cs
│   └── 📁 Base/
│       └── BaseService.cs
│
├── 📁 DTOs/
│   ├── 📁 Customer/
│   │   ├── CustomerDto.cs
│   │   ├── CreateCustomerDto.cs
│   │   ├── UpdateCustomerDto.cs
│   │   └── CustomerFilterDto.cs
│   ├── 📁 Order/
│   │   ├── OrderDto.cs
│   │   ├── OrderItemDto.cs
│   │   ├── CreateOrderDto.cs
│   │   └── OrderSummaryDto.cs
│   └── 📁 Common/
│       ├── PagedResultDto.cs
│       ├── ResultDto.cs
│       └── ErrorDto.cs
│
├── 📁 Commands/
│   ├── 📁 Customer/
│   │   ├── CreateCustomerCommand.cs
│   │   ├── UpdateCustomerCommand.cs
│   │   └── DeleteCustomerCommand.cs
│   ├── 📁 Order/
│   │   ├── CreateOrderCommand.cs
│   │   ├── ConfirmOrderCommand.cs
│   │   └── CancelOrderCommand.cs
│   └── 📁 Base/
│       ├── ICommand.cs
│       ├── ICommandHandler.cs
│       └── CommandDispatcher.cs
│
├── 📁 Queries/
│   ├── 📁 Customer/
│   │   ├── GetCustomerByIdQuery.cs
│   │   ├── GetAllCustomersQuery.cs
│   │   └── SearchCustomersQuery.cs
│   ├── 📁 Order/
│   │   ├── GetOrderByIdQuery.cs
│   │   ├── GetOrdersByCustomerQuery.cs
│   │   └── GetOrderSummaryQuery.cs
│   └── 📁 Base/
│       ├── IQuery.cs
│       ├── IQueryHandler.cs
│       └── QueryDispatcher.cs
│
├── 📁 Validators/
│   ├── 📁 Customer/
│   │   ├── CreateCustomerValidator.cs
│   │   ├── UpdateCustomerValidator.cs
│   │   └── CustomerFilterValidator.cs
│   ├── 📁 Order/
│   │   ├── CreateOrderValidator.cs
│   │   └── OrderItemValidator.cs
│   └── 📁 Base/
│       └── BaseValidator.cs
│
├── 📁 Mappers/
│   ├── CustomerMapper.cs
│   ├── OrderMapper.cs
│   ├── MappingProfile.cs
│   └── AutoMapperConfig.cs
│
├── 📁 Interfaces/
│   ├── IApplicationService.cs
│   ├── IEmailService.cs
│   ├── IFileService.cs
│   └── INotificationService.cs
│
├── 📁 Exceptions/
│   ├── ApplicationException.cs
│   ├── ValidationException.cs
│   ├── NotFoundException.cs
│   └── BusinessRuleException.cs
│
└── 📁 Extensions/
    ├── ServiceCollectionExtensions.cs
    ├── QueryableExtensions.cs
    └── ValidationExtensions.cs
```

### 9.3 Domain Layer

```
📁 MauiDesktopApp.Domain/
├── 📁 Entities/
│   ├── 📁 Customer/
│   │   └── Customer.cs
│   ├── 📁 Order/
│   │   ├── Order.cs
│   │   └── OrderItem.cs
│   ├── 📁 Product/
│   │   └── Product.cs
│   └── 📁 Base/
│       ├── Entity.cs
│       ├── AggregateRoot.cs
│       └── IAuditable.cs
│
├── 📁 ValueObjects/
│   ├── Email.cs
│   ├── Money.cs
│   ├── Address.cs
│   ├── PhoneNumber.cs
│   └── 📁 Base/
│       └── ValueObject.cs
│
├── 📁 Aggregates/
│   ├── CustomerAggregate.cs
│   ├── OrderAggregate.cs
│   └── ProductAggregate.cs
│
├── 📁 Services/
│   ├── IPricingService.cs
│   ├── PricingService.cs
│   ├── IDiscountService.cs
│   ├── DiscountService.cs
│   ├── IOrderProcessingService.cs
│   └── OrderProcessingService.cs
│
├── 📁 Repositories/
│   ├── IRepository.cs
│   ├── ICustomerRepository.cs
│   ├── IOrderRepository.cs
│   ├── IProductRepository.cs
│   └── IUnitOfWork.cs
│
├── 📁 Events/
│   ├── 📁 Customer/
│   │   ├── CustomerCreatedEvent.cs
│   │   ├── CustomerUpdatedEvent.cs
│   │   └── CustomerDeactivatedEvent.cs
│   ├── 📁 Order/
│   │   ├── OrderCreatedEvent.cs
│   │   ├── OrderConfirmedEvent.cs
│   │   ├── OrderShippedEvent.cs
│   │   └── OrderCancelledEvent.cs
│   └── 📁 Base/
│       ├── IDomainEvent.cs
│       ├── DomainEvent.cs
│       └── IDomainEventHandler.cs
│
├── 📁 Specifications/
│   ├── 📁 Customer/
│   │   ├── ActiveCustomerSpec.cs
│   │   ├── CustomerByEmailSpec.cs
│   │   └── CustomerWithRecentOrdersSpec.cs
│   ├── 📁 Order/
│   │   ├── OrderByStatusSpec.cs
│   │   ├── OrderByDateRangeSpec.cs
│   │   └── OrderByCustomerSpec.cs
│   └── 📁 Base/
│       ├── Specification.cs
│       ├── AndSpecification.cs
│       └── OrSpecification.cs
│
├── 📁 Factories/
│   ├── ICustomerFactory.cs
│   ├── CustomerFactory.cs
│   ├── IOrderFactory.cs
│   └── OrderFactory.cs
│
├── 📁 Exceptions/
│   ├── DomainException.cs
│   ├── BusinessRuleException.cs
│   ├── InvalidEntityException.cs
│   └── ConcurrencyException.cs
│
└── 📁 Enums/
    ├── CustomerStatus.cs
    ├── CustomerType.cs
    ├── OrderStatus.cs
    ├── PaymentStatus.cs
    └── Currency.cs
```

### 9.4 Infrastructure Layer

```
📁 MauiDesktopApp.Infrastructure/
├── 📁 Data/
│   ├── 📁 Context/
│   │   ├── ApplicationDbContext.cs
│   │   ├── DbContextFactory.cs
│   │   └── DesignTimeDbContextFactory.cs
│   ├── 📁 Configurations/
│   │   ├── CustomerConfiguration.cs
│   │   ├── OrderConfiguration.cs
│   │   ├── ProductConfiguration.cs
│   │   └── ValueObjectConfigurations.cs
│   ├── 📁 Repositories/
│   │   ├── Repository.cs
│   │   ├── CustomerRepository.cs
│   │   ├── OrderRepository.cs
│   │   ├── ProductRepository.cs
│   │   └── UnitOfWork.cs
│   ├── 📁 Migrations/
│   └── 📁 Seed/
│       ├── CustomerSeed.cs
│       ├── ProductSeed.cs
│       └── DataSeeder.cs
│
├── 📁 Services/
│   ├── 📁 Email/
│   │   ├── IEmailService.cs
│   │   ├── EmailService.cs
│   │   └── EmailTemplate.cs
│   ├── 📁 File/
│   │   ├── IFileService.cs
│   │   └── FileService.cs
│   ├── 📁 Notification/
│   │   ├── INotificationService.cs
│   │   └── NotificationService.cs
│   └── 📁 Report/
│       ├── IReportService.cs
│       └── ReportService.cs
│
├── 📁 ExternalServices/
│   ├── 📁 Payment/
│   │   ├── IPaymentGateway.cs
│   │   ├── StripePaymentGateway.cs
│   │   └── PayPalPaymentGateway.cs
│   ├── 📁 Shipping/
│   │   ├── IShippingService.cs
│   │   └── ShippingService.cs
│   └── 📁 API/
│       ├── HttpClientService.cs
│       └── ApiConfiguration.cs
│
├── 📁 Logging/
│   ├── SerilogConfiguration.cs
│   ├── LoggingMiddleware.cs
│   ├── 📁 Enrichers/
│   │   ├── UserContextEnricher.cs
│   │   └── ApplicationContextEnricher.cs
│   └── 📁 Sinks/
│       └── DatabaseSink.cs
│
├── 📁 Configuration/
│   ├── AppSettings.cs
│   ├── DatabaseSettings.cs
│   ├── EmailSettings.cs
│   ├── PaymentSettings.cs
│   └── ConfigurationExtensions.cs
│
├── 📁 Security/
│   ├── 📁 Authentication/
│   │   ├── IAuthenticationService.cs
│   │   └── AuthenticationService.cs
│   ├── 📁 Authorization/
│   │   ├── IAuthorizationService.cs
│   │   └── AuthorizationService.cs
│   └── 📁 Encryption/
│       ├── IEncryptionService.cs
│       └── EncryptionService.cs
│
├── 📁 Caching/
│   ├── ICacheService.cs
│   ├── MemoryCacheService.cs
│   ├── DistributedCacheService.cs
│   └── CacheConfiguration.cs
│
├── 📁 Messaging/
│   ├── 📁 Events/
│   │   ├── IEventBus.cs
│   │   ├── InMemoryEventBus.cs
│   │   └── EventBusConfiguration.cs
│   └── 📁 Handlers/
│       ├── CustomerEventHandlers.cs
│       └── OrderEventHandlers.cs
│
└── 📁 Extensions/
    ├── ServiceCollectionExtensions.cs
    ├── ConfigurationExtensions.cs
    ├── DatabaseExtensions.cs
    └── LoggingExtensions.cs
```

## Arquivos de Configuração

### 9.5 Solution Structure

```xml
<!-- Directory.Build.props -->
<Project>
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <LangVersion>latest</LangVersion>
    <Nullable>enable</Nullable>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <WarningsAsErrors />
    <WarningsNotAsErrors>CS8618</WarningsNotAsErrors>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
  </PropertyGroup>

  <PropertyGroup>
    <Company>MauiDesktopApp</Company>
    <Product>MauiDesktopApp</Product>
    <Copyright>Copyright © 2024</Copyright>
    <Version>1.0.0</Version>
    <AssemblyVersion>1.0.0.0</AssemblyVersion>
    <FileVersion>1.0.0.0</FileVersion>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.CodeAnalysis.NetAnalyzers" Version="8.0.0">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers</IncludeAssets>
    </PackageReference>
  </ItemGroup>
</Project>
```

### 9.6 EditorConfig

```ini
# .editorconfig
root = true

[*]
charset = utf-8
end_of_line = crlf
insert_final_newline = true
trim_trailing_whitespace = true

[*.{cs,vb}]
indent_style = space
indent_size = 4

[*.{xaml,xml}]
indent_style = space
indent_size = 2

[*.{json,yml,yaml}]
indent_style = space
indent_size = 2

# C# coding conventions
[*.cs]
# New line preferences
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true
csharp_new_line_before_members_in_object_initializers = true
csharp_new_line_before_members_in_anonymous_types = true

# Indentation preferences
csharp_indent_case_contents = true
csharp_indent_switch_labels = true

# Space preferences
csharp_space_after_cast = false
csharp_space_after_keywords_in_control_flow_statements = true
csharp_space_between_method_declaration_parameter_list_parentheses = false
csharp_space_between_method_call_parameter_list_parentheses = false

# Organize usings
dotnet_sort_system_directives_first = true

# Code quality rules
dotnet_analyzer_diagnostic.category-security.severity = error
dotnet_analyzer_diagnostic.category-performance.severity = warning
```

### 9.7 Global.json

```json
{
  "sdk": {
    "version": "8.0.100",
    "rollForward": "latestMinor"
  },
  "msbuild-sdks": {
    "Microsoft.Build.CentralPackageVersions": "2.1.3"
  }
}
```

## Package References

### 9.8 Central Package Management

```xml
<!-- Directory.Packages.props -->
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
  </PropertyGroup>

  <ItemGroup>
    <!-- MAUI -->
    <PackageVersion Include="Microsoft.Maui.Controls" Version="8.0.40" />
    <PackageVersion Include="Microsoft.Maui.Controls.Compatibility" Version="8.0.40" />
    
    <!-- MVVM -->
    <PackageVersion Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    <PackageVersion Include="CommunityToolkit.Maui" Version="7.0.1" />
    
    <!-- Entity Framework -->
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="8.0.4" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.4" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.4" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.4" />
    
    <!-- AutoMapper -->
    <PackageVersion Include="AutoMapper" Version="13.0.1" />
    <PackageVersion Include="AutoMapper.Extensions.Microsoft.DependencyInjection" Version="13.0.1" />
    
    <!-- FluentValidation -->
    <PackageVersion Include="FluentValidation" Version="11.9.1" />
    <PackageVersion Include="FluentValidation.DependencyInjectionExtensions" Version="11.9.1" />
    
    <!-- Serilog -->
    <PackageVersion Include="Serilog" Version="3.1.1" />
    <PackageVersion Include="Serilog.Extensions.Hosting" Version="8.0.0" />
    <PackageVersion Include="Serilog.Extensions.Logging" Version="8.0.0" />
    <PackageVersion Include="Serilog.Sinks.Console" Version="5.0.1" />
    <PackageVersion Include="Serilog.Sinks.File" Version="5.0.0" />
    <PackageVersion Include="Serilog.Sinks.Debug" Version="2.0.0" />
    
    <!-- Testing -->
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
    <PackageVersion Include="NUnit" Version="4.1.0" />
    <PackageVersion Include="NUnit3TestAdapter" Version="4.5.0" />
    <PackageVersion Include="Moq" Version="4.20.70" />
    <PackageVersion Include="FluentAssertions" Version="6.12.0" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.4" />
    
    <!-- Architecture Testing -->
    <PackageVersion Include="NetArchTest.Rules" Version="1.3.2" />
  </ItemGroup>
</Project>
```

## Scripts de Build

### 9.9 Build Scripts

```powershell
# scripts/Build.ps1
param(
    [string]$Configuration = "Release",
    [string]$Platform = "win-x64",
    [switch]$SkipTests,
    [switch]$SkipPublish
)

Write-Host "Building MauiDesktopApp..." -ForegroundColor Green

# Restore packages
Write-Host "Restoring packages..." -ForegroundColor Yellow
dotnet restore

# Build solution
Write-Host "Building solution..." -ForegroundColor Yellow
dotnet build --configuration $Configuration --no-restore

if (-not $SkipTests) {
    # Run tests
    Write-Host "Running tests..." -ForegroundColor Yellow
    dotnet test --configuration $Configuration --no-build --verbosity normal
}

if (-not $SkipPublish) {
    # Publish application
    Write-Host "Publishing application..." -ForegroundColor Yellow
    dotnet publish src/MauiDesktopApp.Presentation/MauiDesktopApp.Presentation.csproj `
        --configuration $Configuration `
        --runtime $Platform `
        --self-contained true `
        --output "publish/$Platform"
}

Write-Host "Build completed successfully!" -ForegroundColor Green
```

### 9.10 Development Setup

```powershell
# scripts/Setup-Dev.ps1
Write-Host "Setting up development environment..." -ForegroundColor Green

# Install .NET workloads
Write-Host "Installing .NET MAUI workload..." -ForegroundColor Yellow
dotnet workload install maui

# Install tools
Write-Host "Installing global tools..." -ForegroundColor Yellow
dotnet tool install --global dotnet-ef
dotnet tool install --global Microsoft.Web.LibraryManager.Cli

# Setup database
Write-Host "Setting up database..." -ForegroundColor Yellow
$connectionString = "Data Source=LocalDatabase.db"
dotnet ef database update --project src/MauiDesktopApp.Infrastructure --startup-project src/MauiDesktopApp.Presentation

Write-Host "Development environment setup completed!" -ForegroundColor Green
```

## Melhores Práticas

### 9.11 Organização de Código

1. **Naming Conventions**:
   - Use PascalCase para classes, métodos e propriedades
   - Use camelCase para campos privados e parâmetros
   - Use UPPER_CASE para constantes

2. **File Organization**:
   - Um arquivo por classe
   - Organize arquivos relacionados em pastas
   - Use namespaces consistentes com estrutura de pastas

3. **Dependency Management**:
   - Use Central Package Management
   - Mantenha dependências atualizadas
   - Evite dependências circulares

4. **Testing Structure**:
   - Espelhe estrutura do projeto principal
   - Use naming convention: `MethodName_Scenario_ExpectedBehavior`
   - Organize testes por categoria

### 9.12 CI/CD Structure

```yaml
# .github/workflows/ci.yml
name: CI/CD Pipeline

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-test:
    runs-on: windows-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
        
    - name: Install MAUI workload
      run: dotnet workload install maui
    
    - name: Restore dependencies
      run: dotnet restore
    
    - name: Build
      run: dotnet build --no-restore --configuration Release
    
    - name: Test
      run: dotnet test --no-build --configuration Release --logger trx --results-directory TestResults
    
    - name: Publish Test Results
      uses: dorny/test-reporter@v1
      if: success() || failure()
      with:
        name: Test Results
        path: TestResults/*.trx
        reporter: dotnet-trx
```

## Próximos Tópicos

- [Configuração e Setup](./10-configuracao-setup.md)
