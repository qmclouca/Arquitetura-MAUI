# 10. Configuração e Setup

## Introdução

Este documento fornece um guia completo para configurar e executar a aplicação MAUI Desktop com arquitetura MVVM e DDD, incluindo setup do ambiente de desenvolvimento, configuração de dependências e instruções de deploy.

## Pré-requisitos

### 10.1 Ambiente de Desenvolvimento

**Software Necessário:**
- Visual Studio 2022 (17.8 ou superior) ou VS Code
- .NET 8.0 SDK
- Git
- SQL Server LocalDB ou SQLite
- Node.js (para ferramentas de build)

**Workloads do Visual Studio:**
- .NET Multi-platform App UI development
- ASP.NET and web development
- Data storage and processing

**Extensões VS Code (se aplicável):**
- C# Dev Kit
- .NET MAUI
- XAML
- PlantUML
- Draw.io Integration

### 10.2 Verificação do Ambiente

```powershell
# Verificar versão do .NET
dotnet --version

# Verificar workloads instalados
dotnet workload list

# Verificar templates MAUI
dotnet new maui --help
```

## Configuração Inicial

### 10.3 Criação do Projeto

```bash
# Criar diretório do projeto
mkdir MauiDesktopApp
cd MauiDesktopApp

# Criar solution
dotnet new sln -n MauiDesktopApp

# Criar projetos das camadas
dotnet new classlib -n MauiDesktopApp.Domain -o src/MauiDesktopApp.Domain
dotnet new classlib -n MauiDesktopApp.Application -o src/MauiDesktopApp.Application
dotnet new classlib -n MauiDesktopApp.Infrastructure -o src/MauiDesktopApp.Infrastructure
dotnet new maui -n MauiDesktopApp.Presentation -o src/MauiDesktopApp.Presentation

# Criar projetos de teste
dotnet new nunit -n MauiDesktopApp.UnitTests -o tests/MauiDesktopApp.UnitTests
dotnet new nunit -n MauiDesktopApp.IntegrationTests -o tests/MauiDesktopApp.IntegrationTests
dotnet new nunit -n MauiDesktopApp.ArchitectureTests -o tests/MauiDesktopApp.ArchitectureTests

# Adicionar projetos à solution
dotnet sln add src/MauiDesktopApp.Domain/MauiDesktopApp.Domain.csproj
dotnet sln add src/MauiDesktopApp.Application/MauiDesktopApp.Application.csproj
dotnet sln add src/MauiDesktopApp.Infrastructure/MauiDesktopApp.Infrastructure.csproj
dotnet sln add src/MauiDesktopApp.Presentation/MauiDesktopApp.Presentation.csproj
dotnet sln add tests/MauiDesktopApp.UnitTests/MauiDesktopApp.UnitTests.csproj
dotnet sln add tests/MauiDesktopApp.IntegrationTests/MauiDesktopApp.IntegrationTests.csproj
dotnet sln add tests/MauiDesktopApp.ArchitectureTests/MauiDesktopApp.ArchitectureTests.csproj
```

### 10.4 Configuração de Referências

```bash
# Presentation -> Application
dotnet add src/MauiDesktopApp.Presentation reference src/MauiDesktopApp.Application

# Presentation -> Infrastructure
dotnet add src/MauiDesktopApp.Presentation reference src/MauiDesktopApp.Infrastructure

# Application -> Domain
dotnet add src/MauiDesktopApp.Application reference src/MauiDesktopApp.Domain

# Infrastructure -> Application
dotnet add src/MauiDesktopApp.Infrastructure reference src/MauiDesktopApp.Application

# Infrastructure -> Domain
dotnet add src/MauiDesktopApp.Infrastructure reference src/MauiDesktopApp.Domain

# Testes
dotnet add tests/MauiDesktopApp.UnitTests reference src/MauiDesktopApp.Domain
dotnet add tests/MauiDesktopApp.UnitTests reference src/MauiDesktopApp.Application
dotnet add tests/MauiDesktopApp.IntegrationTests reference src/MauiDesktopApp.Infrastructure
dotnet add tests/MauiDesktopApp.ArchitectureTests reference src/MauiDesktopApp.Presentation
```

## Configuração de Dependências

### 10.5 Packages.props

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
    <PackageVersion Include="Microsoft.Extensions.Logging.Debug" Version="8.0.0" />
    
    <!-- MVVM -->
    <PackageVersion Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    <PackageVersion Include="CommunityToolkit.Maui" Version="7.0.1" />
    
    <!-- Entity Framework -->
    <PackageVersion Include="Microsoft.EntityFrameworkCore" Version="8.0.4" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Design" Version="8.0.4" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.4" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.4" />
    <PackageVersion Include="Microsoft.EntityFrameworkCore.InMemory" Version="8.0.4" />
    
    <!-- Dependency Injection -->
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection" Version="8.0.0" />
    <PackageVersion Include="Microsoft.Extensions.DependencyInjection.Abstractions" Version="8.0.1" />
    <PackageVersion Include="Microsoft.Extensions.Hosting" Version="8.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Configuration" Version="8.0.0" />
    <PackageVersion Include="Microsoft.Extensions.Configuration.Json" Version="8.0.0" />
    
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
    <PackageVersion Include="Serilog.Enrichers.Thread" Version="3.1.0" />
    <PackageVersion Include="Serilog.Enrichers.Process" Version="2.0.2" />
    <PackageVersion Include="Serilog.Enrichers.Environment" Version="2.3.0" />
    <PackageVersion Include="Serilog.Settings.Configuration" Version="8.0.0" />
    
    <!-- Testing -->
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="17.9.0" />
    <PackageVersion Include="NUnit" Version="4.1.0" />
    <PackageVersion Include="NUnit3TestAdapter" Version="4.5.0" />
    <PackageVersion Include="Moq" Version="4.20.70" />
    <PackageVersion Include="FluentAssertions" Version="6.12.0" />
    <PackageVersion Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.4" />
    
    <!-- Architecture Testing -->
    <PackageVersion Include="NetArchTest.Rules" Version="1.3.2" />
    
    <!-- Utilities -->
    <PackageVersion Include="Newtonsoft.Json" Version="13.0.3" />
    <PackageVersion Include="System.Text.Json" Version="8.0.3" />
  </ItemGroup>
</Project>
```

### 10.6 Project Files

```xml
<!-- src/MauiDesktopApp.Domain/MauiDesktopApp.Domain.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

</Project>
```

```xml
<!-- src/MauiDesktopApp.Application/MauiDesktopApp.Application.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="AutoMapper" />
    <PackageReference Include="AutoMapper.Extensions.Microsoft.DependencyInjection" />
    <PackageReference Include="FluentValidation" />
    <PackageReference Include="FluentValidation.DependencyInjectionExtensions" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection.Abstractions" />
    <PackageReference Include="Microsoft.Extensions.Logging.Abstractions" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MauiDesktopApp.Domain\MauiDesktopApp.Domain.csproj" />
  </ItemGroup>

</Project>
```

```xml
<!-- src/MauiDesktopApp.Infrastructure/MauiDesktopApp.Infrastructure.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.EntityFrameworkCore" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Design" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" />
    <PackageReference Include="Microsoft.Extensions.Configuration" />
    <PackageReference Include="Microsoft.Extensions.DependencyInjection" />
    <PackageReference Include="Serilog" />
    <PackageReference Include="Serilog.Extensions.Hosting" />
    <PackageReference Include="Serilog.Sinks.Console" />
    <PackageReference Include="Serilog.Sinks.File" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MauiDesktopApp.Application\MauiDesktopApp.Application.csproj" />
    <ProjectReference Include="..\MauiDesktopApp.Domain\MauiDesktopApp.Domain.csproj" />
  </ItemGroup>

</Project>
```

```xml
<!-- src/MauiDesktopApp.Presentation/MauiDesktopApp.Presentation.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net8.0-windows10.0.19041.0</TargetFrameworks>
    <OutputType>Exe</OutputType>
    <RootNamespace>MauiDesktopApp.Presentation</RootNamespace>
    <UseMaui>true</UseMaui>
    <SingleProject>true</SingleProject>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>

    <!-- Display Version -->
    <ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>

    <!-- Application Version -->
    <ApplicationVersion>1</ApplicationVersion>

    <SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">10.0.17763.0</SupportedOSPlatformVersion>
    <TargetPlatformMinVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'">10.0.17763.0</TargetPlatformMinVersion>
  </PropertyGroup>

  <ItemGroup>
    <!-- App Icon -->
    <MauiIcon Include="Resources\AppIcon\appicon.svg" ForegroundFile="Resources\AppIcon\appiconfg.svg" Color="#512BD4" />

    <!-- Splash Screen -->
    <MauiSplashScreen Include="Resources\Splash\splash.svg" Color="#512BD4" BaseSize="128,128" />

    <!-- Images -->
    <MauiImage Include="Resources\Images\*" />
    <MauiImage Update="Resources\Images\dotnet_bot.svg" BaseSize="168,208" />

    <!-- Custom Fonts -->
    <MauiFont Include="Resources\Fonts\*" />

    <!-- Raw Assets (also remove the "Resources\Raw" prefix) -->
    <MauiAsset Include="Resources\Raw\**" LogicalName="%(RecursiveDir)%(Filename)%(Extension)" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="Microsoft.Maui.Controls" />
    <PackageReference Include="Microsoft.Maui.Controls.Compatibility" />
    <PackageReference Include="Microsoft.Extensions.Logging.Debug" />
    <PackageReference Include="CommunityToolkit.Mvvm" />
    <PackageReference Include="CommunityToolkit.Maui" />
    <PackageReference Include="Serilog.Extensions.Hosting" />
    <PackageReference Include="Serilog.Extensions.Logging" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\MauiDesktopApp.Application\MauiDesktopApp.Application.csproj" />
    <ProjectReference Include="..\MauiDesktopApp.Infrastructure\MauiDesktopApp.Infrastructure.csproj" />
  </ItemGroup>

</Project>
```

## Configuração da Aplicação

### 10.7 MauiProgram.cs

```csharp
// src/MauiDesktopApp.Presentation/MauiProgram.cs
using MauiDesktopApp.Application.Extensions;
using MauiDesktopApp.Infrastructure.Extensions;
using MauiDesktopApp.Presentation.Extensions;
using Microsoft.Extensions.Logging;
using Serilog;

namespace MauiDesktopApp.Presentation;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.Debug()
            .WriteTo.File(Path.Combine(FileSystem.AppDataDirectory, "logs", "app-.log"), 
                         rollingInterval: RollingInterval.Day)
            .CreateLogger();

        var builder = MauiApp.CreateBuilder();
        
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // Add logging
        builder.Logging.ClearProviders();
        builder.Logging.AddSerilog();

        // Register services from each layer
        builder.Services.AddApplicationServices();
        builder.Services.AddInfrastructureServices(builder.Configuration);
        builder.Services.AddPresentationServices();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        // Log application startup
        Log.Information("Application starting up");

        return app;
    }
}
```

### 10.8 App.xaml.cs

```csharp
// src/MauiDesktopApp.Presentation/App.xaml.cs
using MauiDesktopApp.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace MauiDesktopApp.Presentation;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
        
        // Initialize database
        InitializeDatabaseAsync();
        
        MainPage = new AppShell();
    }

    private async void InitializeDatabaseAsync()
    {
        try
        {
            var dbContext = ServiceHelper.GetService<ApplicationDbContext>();
            if (dbContext != null)
            {
                await dbContext.Database.EnsureCreatedAsync();
                Log.Information("Database initialized successfully");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error initializing database");
        }
    }
}
```

### 10.9 Service Helper

```csharp
// src/MauiDesktopApp.Presentation/Helpers/ServiceHelper.cs
namespace MauiDesktopApp.Presentation.Helpers;

public static class ServiceHelper
{
    public static TService GetService<TService>()
        => Current.GetService<TService>();

    public static TService GetRequiredService<TService>()
        => Current.GetRequiredService<TService>();

    public static IServiceProvider Current =>
#if WINDOWS10_0_17763_0_OR_GREATER
        MauiWinUIApplication.Current.Services;
#elif ANDROID
        MauiApplication.Current.Services;
#elif IOS || MACCATALYST
        MauiUIApplicationDelegate.Current.Services;
#else
        null;
#endif
}
```

## Configuração de Dependências

### 10.10 Application Layer DI

```csharp
// src/MauiDesktopApp.Application/Extensions/ServiceCollectionExtensions.cs
using FluentValidation;
using MauiDesktopApp.Application.Mappers;
using MauiDesktopApp.Application.Services.Customer;
using MauiDesktopApp.Application.Services.Order;
using Microsoft.Extensions.DependencyInjection;

namespace MauiDesktopApp.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        // AutoMapper
        services.AddAutoMapper(typeof(MappingProfile));
        
        // Application Services
        services.AddScoped<ICustomerService, CustomerService>();
        services.AddScoped<IOrderService, OrderService>();
        
        // FluentValidation
        services.AddValidatorsFromAssembly(typeof(ServiceCollectionExtensions).Assembly);
        
        return services;
    }
}
```

### 10.11 Infrastructure Layer DI

```csharp
// src/MauiDesktopApp.Infrastructure/Extensions/ServiceCollectionExtensions.cs
using MauiDesktopApp.Domain.Repositories;
using MauiDesktopApp.Infrastructure.Data.Context;
using MauiDesktopApp.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MauiDesktopApp.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
                             ?? Path.Combine(FileSystem.AppDataDirectory, "app.db");
        
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlite($"Data Source={connectionString}"));
        
        // Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        
        // Domain Services
        services.AddScoped<IPricingService, PricingService>();
        services.AddScoped<IDiscountService, DiscountService>();
        
        // Infrastructure Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IFileService, FileService>();
        
        return services;
    }
}
```

### 10.12 Presentation Layer DI

```csharp
// src/MauiDesktopApp.Presentation/Extensions/ServiceCollectionExtensions.cs
using MauiDesktopApp.Presentation.Services;
using MauiDesktopApp.Presentation.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace MauiDesktopApp.Presentation.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        // Navigation Service
        services.AddSingleton<INavigationService, NavigationService>();
        services.AddSingleton<IDialogService, DialogService>();
        
        // ViewModels
        services.AddTransient<MainViewModel>();
        services.AddTransient<CustomerListViewModel>();
        services.AddTransient<CustomerDetailViewModel>();
        services.AddTransient<OrderListViewModel>();
        services.AddTransient<OrderDetailViewModel>();
        
        // Views
        services.AddTransient<MainPage>();
        services.AddTransient<CustomerListPage>();
        services.AddTransient<CustomerDetailPage>();
        services.AddTransient<OrderListPage>();
        services.AddTransient<OrderDetailPage>();
        
        return services;
    }
}
```

## Configuração do Banco de Dados

### 10.13 DbContext

```csharp
// src/MauiDesktopApp.Infrastructure/Data/Context/ApplicationDbContext.cs
using MauiDesktopApp.Domain.Entities;
using MauiDesktopApp.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;

namespace MauiDesktopApp.Infrastructure.Data.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Order> Orders { get; set; } = null!;
    public DbSet<OrderItem> OrderItems { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfiguration(new CustomerConfiguration());
        modelBuilder.ApplyConfiguration(new OrderConfiguration());
        modelBuilder.ApplyConfiguration(new OrderItemConfiguration());
        modelBuilder.ApplyConfiguration(new ProductConfiguration());
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "app.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }
    }
}
```

### 10.14 Migrations

```bash
# Instalar ferramenta EF Core
dotnet tool install --global dotnet-ef

# Criar primeira migration
dotnet ef migrations add InitialCreate --project src/MauiDesktopApp.Infrastructure --startup-project src/MauiDesktopApp.Presentation

# Atualizar banco de dados
dotnet ef database update --project src/MauiDesktopApp.Infrastructure --startup-project src/MauiDesktopApp.Presentation
```

## Configuração de Logging

### 10.15 appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=app.db"
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "Microsoft.EntityFrameworkCore": "Information"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "outputTemplate": "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      },
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 30,
          "fileSizeLimitBytes": 10485760,
          "rollOnFileSizeLimit": true,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}"
        }
      }
    ],
    "Enrich": ["FromLogContext", "WithThreadId", "WithProcessId"]
  }
}
```

## Build e Deploy

### 10.16 Build Script

```powershell
# scripts/Build.ps1
param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Debug", "Release")]
    [string]$Configuration = "Release",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("win-x64", "win-x86", "win-arm64")]
    [string]$Runtime = "win-x64",
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipTests,
    
    [Parameter(Mandatory=$false)]
    [switch]$SkipPublish
)

Write-Host "Building MauiDesktopApp..." -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Yellow
Write-Host "Runtime: $Runtime" -ForegroundColor Yellow

try {
    # Restore packages
    Write-Host "`nRestoring packages..." -ForegroundColor Yellow
    dotnet restore
    if ($LASTEXITCODE -ne 0) { throw "Package restore failed" }

    # Build solution
    Write-Host "`nBuilding solution..." -ForegroundColor Yellow
    dotnet build --configuration $Configuration --no-restore
    if ($LASTEXITCODE -ne 0) { throw "Build failed" }

    if (-not $SkipTests) {
        # Run tests
        Write-Host "`nRunning tests..." -ForegroundColor Yellow
        dotnet test --configuration $Configuration --no-build --verbosity normal --logger "trx;LogFileName=test-results.trx"
        if ($LASTEXITCODE -ne 0) { throw "Tests failed" }
    }

    if (-not $SkipPublish) {
        # Clean publish directory
        $publishDir = "publish/$Runtime"
        if (Test-Path $publishDir) {
            Remove-Item $publishDir -Recurse -Force
        }

        # Publish application
        Write-Host "`nPublishing application..." -ForegroundColor Yellow
        dotnet publish src/MauiDesktopApp.Presentation/MauiDesktopApp.Presentation.csproj `
            --configuration $Configuration `
            --runtime $Runtime `
            --self-contained true `
            --output $publishDir `
            --verbosity normal
        
        if ($LASTEXITCODE -ne 0) { throw "Publish failed" }
        
        Write-Host "`nApplication published to: $publishDir" -ForegroundColor Green
    }

    Write-Host "`nBuild completed successfully!" -ForegroundColor Green
}
catch {
    Write-Host "`nBuild failed: $_" -ForegroundColor Red
    exit 1
}
```

### 10.17 Development Setup

```powershell
# scripts/Setup-Dev.ps1
Write-Host "Setting up development environment..." -ForegroundColor Green

try {
    # Check .NET installation
    Write-Host "`nChecking .NET installation..." -ForegroundColor Yellow
    $dotnetVersion = dotnet --version
    Write-Host ".NET Version: $dotnetVersion" -ForegroundColor Green

    # Install required workloads
    Write-Host "`nInstalling .NET MAUI workload..." -ForegroundColor Yellow
    dotnet workload install maui
    if ($LASTEXITCODE -ne 0) { throw "Workload installation failed" }

    # Install global tools
    Write-Host "`nInstalling global tools..." -ForegroundColor Yellow
    dotnet tool install --global dotnet-ef
    dotnet tool install --global Microsoft.Web.LibraryManager.Cli
    
    # Create app data directory
    $appDataDir = [System.IO.Path]::Combine([System.Environment]::GetFolderPath("LocalApplicationData"), "MauiDesktopApp")
    if (-not (Test-Path $appDataDir)) {
        New-Item -Path $appDataDir -ItemType Directory -Force
        Write-Host "Created app data directory: $appDataDir" -ForegroundColor Green
    }

    # Setup database
    Write-Host "`nSetting up database..." -ForegroundColor Yellow
    Push-Location (Get-Location)
    try {
        dotnet ef database update --project src/MauiDesktopApp.Infrastructure --startup-project src/MauiDesktopApp.Presentation
        if ($LASTEXITCODE -ne 0) { throw "Database setup failed" }
        Write-Host "Database setup completed!" -ForegroundColor Green
    }
    finally {
        Pop-Location
    }

    # Create sample data (optional)
    Write-Host "`nCreating sample data..." -ForegroundColor Yellow
    # Add your sample data creation logic here

    Write-Host "`nDevelopment environment setup completed successfully!" -ForegroundColor Green
    Write-Host "`nYou can now run the application with:" -ForegroundColor Yellow
    Write-Host "dotnet run --project src/MauiDesktopApp.Presentation" -ForegroundColor Cyan
}
catch {
    Write-Host "`nSetup failed: $_" -ForegroundColor Red
    exit 1
}
```

## Execução da Aplicação

### 10.18 Comandos de Execução

```bash
# Executar em modo de desenvolvimento
dotnet run --project src/MauiDesktopApp.Presentation

# Executar com configuração específica
dotnet run --project src/MauiDesktopApp.Presentation --configuration Release

# Executar testes
dotnet test

# Executar testes com cobertura
dotnet test --collect:"XPlat Code Coverage"

# Publicar para Windows x64
dotnet publish src/MauiDesktopApp.Presentation -c Release -r win-x64 --self-contained

# Executar aplicação publicada
./publish/win-x64/MauiDesktopApp.Presentation.exe
```

### 10.19 Troubleshooting

**Problemas Comuns:**

1. **Erro de Workload MAUI não encontrado:**
   ```bash
   dotnet workload install maui
   ```

2. **Erro de banco de dados:**
   ```bash
   dotnet ef database drop --project src/MauiDesktopApp.Infrastructure --startup-project src/MauiDesktopApp.Presentation
   dotnet ef database update --project src/MauiDesktopApp.Infrastructure --startup-project src/MauiDesktopApp.Presentation
   ```

3. **Erro de permissões no diretório:**
   - Execute o PowerShell como Administrador
   - Verifique permissões de escrita no diretório do projeto

4. **Erro de dependências não encontradas:**
   ```bash
   dotnet restore --force
   dotnet clean
   dotnet build
   ```

### 10.20 Verificação da Instalação

```powershell
# scripts/Verify-Setup.ps1
Write-Host "Verifying installation..." -ForegroundColor Green

# Check .NET
$dotnetVersion = dotnet --version
Write-Host ".NET Version: $dotnetVersion" -ForegroundColor $(if($dotnetVersion) {"Green"} else {"Red"})

# Check MAUI workload
$workloads = dotnet workload list
$mauiInstalled = $workloads -match "maui"
Write-Host "MAUI Workload: $(if($mauiInstalled) {"Installed"} else {"Not Installed"})" -ForegroundColor $(if($mauiInstalled) {"Green"} else {"Red"})

# Check project build
Write-Host "`nTesting project build..." -ForegroundColor Yellow
dotnet build --verbosity quiet
$buildSuccess = $LASTEXITCODE -eq 0
Write-Host "Project Build: $(if($buildSuccess) {"Success"} else {"Failed"})" -ForegroundColor $(if($buildSuccess) {"Green"} else {"Red"})

# Check database
Write-Host "`nChecking database..." -ForegroundColor Yellow
try {
    dotnet ef database update --project src/MauiDesktopApp.Infrastructure --startup-project src/MauiDesktopApp.Presentation --verbosity quiet
    Write-Host "Database: Ready" -ForegroundColor Green
} catch {
    Write-Host "Database: Error" -ForegroundColor Red
}

if ($buildSuccess -and $mauiInstalled) {
    Write-Host "`nSetup verification completed successfully!" -ForegroundColor Green
    Write-Host "You can run the application with: dotnet run --project src/MauiDesktopApp.Presentation" -ForegroundColor Cyan
} else {
    Write-Host "`nSetup verification failed. Please check the errors above." -ForegroundColor Red
}
```

## Próximos Passos

1. **Executar a aplicação**: Use os comandos fornecidos para executar a aplicação
2. **Explorar a documentação**: Consulte os outros documentos para entender a arquitetura
3. **Personalizar**: Adapte o código para suas necessidades específicas
4. **Contribuir**: Siga as diretrizes de contribuição se estiver trabalhando em equipe
5. **Deploy**: Configure CI/CD para automação de build e deploy

Este guia fornece uma base sólida para começar a trabalhar com a arquitetura MAUI Desktop proposta. A estrutura é flexível e pode ser adaptada conforme necessário para diferentes tipos de projetos.
