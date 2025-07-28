# 7. Design Patterns

## Introdução aos Design Patterns

Este documento apresenta os principais Design Patterns utilizados na arquitetura MAUI com MVVM e DDD, demonstrando como aplicá-los para criar um código mais limpo, reutilizável e de fácil manutenção.

## Padrões Criacionais

### 7.1 Factory Pattern

Cria objetos sem especificar suas classes concretas.

```csharp
// Domain/Factories/ICustomerFactory.cs
public interface ICustomerFactory
{
    Customer CreateStandardCustomer(string name, string email);
    Customer CreatePremiumCustomer(string name, string email, string loyaltyNumber);
    Customer CreateCorporateCustomer(string name, string email, string companyName);
}

// Domain/Factories/CustomerFactory.cs
public class CustomerFactory : ICustomerFactory
{
    public Customer CreateStandardCustomer(string name, string email)
    {
        var customer = new Customer(name, new Email(email));
        customer.SetCustomerType(CustomerType.Standard);
        return customer;
    }

    public Customer CreatePremiumCustomer(string name, string email, string loyaltyNumber)
    {
        var customer = new Customer(name, new Email(email));
        customer.SetCustomerType(CustomerType.Premium);
        customer.SetLoyaltyNumber(loyaltyNumber);
        return customer;
    }

    public Customer CreateCorporateCustomer(string name, string email, string companyName)
    {
        var customer = new Customer(name, new Email(email));
        customer.SetCustomerType(CustomerType.Corporate);
        customer.SetCompanyName(companyName);
        return customer;
    }
}

// Application/Services/CustomerService.cs
public class CustomerService : ICustomerService
{
    private readonly ICustomerFactory _customerFactory;
    private readonly ICustomerRepository _customerRepository;

    public CustomerService(ICustomerFactory customerFactory, ICustomerRepository customerRepository)
    {
        _customerFactory = customerFactory;
        _customerRepository = customerRepository;
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
    {
        Customer customer = request.CustomerType switch
        {
            CustomerType.Standard => _customerFactory.CreateStandardCustomer(request.Name, request.Email),
            CustomerType.Premium => _customerFactory.CreatePremiumCustomer(request.Name, request.Email, request.LoyaltyNumber),
            CustomerType.Corporate => _customerFactory.CreateCorporateCustomer(request.Name, request.Email, request.CompanyName),
            _ => throw new ArgumentException("Invalid customer type")
        };

        await _customerRepository.AddAsync(customer);
        return CustomerMapper.ToDto(customer);
    }
}
```

### 7.2 Builder Pattern

Constrói objetos complexos passo a passo.

```csharp
// Domain/Builders/IOrderBuilder.cs
public interface IOrderBuilder
{
    IOrderBuilder WithCustomer(int customerId);
    IOrderBuilder AddItem(int productId, int quantity, Money unitPrice);
    IOrderBuilder WithShippingAddress(Address address);
    IOrderBuilder WithDiscount(decimal discountPercentage);
    IOrderBuilder WithNotes(string notes);
    Order Build();
}

// Domain/Builders/OrderBuilder.cs
public class OrderBuilder : IOrderBuilder
{
    private int _customerId;
    private readonly List<OrderItem> _items = new();
    private Address? _shippingAddress;
    private decimal _discountPercentage;
    private string? _notes;

    public IOrderBuilder WithCustomer(int customerId)
    {
        _customerId = customerId;
        return this;
    }

    public IOrderBuilder AddItem(int productId, int quantity, Money unitPrice)
    {
        _items.Add(new OrderItem(productId, quantity, unitPrice));
        return this;
    }

    public IOrderBuilder WithShippingAddress(Address address)
    {
        _shippingAddress = address;
        return this;
    }

    public IOrderBuilder WithDiscount(decimal discountPercentage)
    {
        _discountPercentage = discountPercentage;
        return this;
    }

    public IOrderBuilder WithNotes(string notes)
    {
        _notes = notes;
        return this;
    }

    public Order Build()
    {
        if (_customerId == 0)
            throw new InvalidOperationException("Customer is required");

        if (!_items.Any())
            throw new InvalidOperationException("At least one item is required");

        var order = new Order(_customerId, _items);
        
        if (_shippingAddress != null)
            order.SetShippingAddress(_shippingAddress);
            
        if (_discountPercentage > 0)
            order.ApplyDiscount(_discountPercentage);
            
        if (!string.IsNullOrEmpty(_notes))
            order.AddNotes(_notes);

        return order;
    }
}

// Usage
public class OrderService
{
    private readonly IOrderBuilder _orderBuilder;

    public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
    {
        var order = _orderBuilder
            .WithCustomer(request.CustomerId)
            .WithShippingAddress(request.ShippingAddress);

        foreach (var item in request.Items)
        {
            order.AddItem(item.ProductId, item.Quantity, item.UnitPrice);
        }

        if (request.DiscountPercentage > 0)
            order.WithDiscount(request.DiscountPercentage);

        return order.Build();
    }
}
```

### 7.3 Singleton Pattern

Garante uma única instância de uma classe.

```csharp
// Infrastructure/Services/ConfigurationService.cs
public sealed class ConfigurationService
{
    private static readonly Lazy<ConfigurationService> _instance = 
        new(() => new ConfigurationService());
    
    private readonly Dictionary<string, object> _settings = new();

    private ConfigurationService()
    {
        LoadConfiguration();
    }

    public static ConfigurationService Instance => _instance.Value;

    public T GetSetting<T>(string key, T defaultValue = default)
    {
        if (_settings.TryGetValue(key, out var value))
        {
            return (T)value;
        }
        return defaultValue;
    }

    public void SetSetting<T>(string key, T value)
    {
        _settings[key] = value;
    }

    private void LoadConfiguration()
    {
        // Load from app settings, environment variables, etc.
        _settings["DatabaseConnectionString"] = "DefaultConnection";
        _settings["ApiBaseUrl"] = "https://api.example.com";
        _settings["CacheExpirationMinutes"] = 30;
    }
}

// Alternative: Dependency Injection approach (preferred)
public interface IConfigurationService
{
    T GetSetting<T>(string key, T defaultValue = default);
    void SetSetting<T>(string key, T value);
}

public class ConfigurationService : IConfigurationService
{
    private readonly Dictionary<string, object> _settings = new();

    public ConfigurationService()
    {
        LoadConfiguration();
    }

    // Implementation...
}

// Register as singleton in DI container
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
```

## Padrões Estruturais

### 7.4 Adapter Pattern

Permite que interfaces incompatíveis trabalhem juntas.

```csharp
// Infrastructure/Adapters/IPaymentGatewayAdapter.cs
public interface IPaymentGatewayAdapter
{
    Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
    Task<RefundResult> ProcessRefundAsync(RefundRequest request);
}

// Infrastructure/Adapters/StripePaymentAdapter.cs
public class StripePaymentAdapter : IPaymentGatewayAdapter
{
    private readonly StripePaymentService _stripeService;
    private readonly ILogger<StripePaymentAdapter> _logger;

    public StripePaymentAdapter(StripePaymentService stripeService, ILogger<StripePaymentAdapter> logger)
    {
        _stripeService = stripeService;
        _logger = logger;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            var stripeRequest = new StripeChargeRequest
            {
                Amount = (long)(request.Amount.Amount * 100), // Convert to cents
                Currency = request.Amount.Currency.Code.ToLower(),
                Source = request.PaymentMethod.Token,
                Description = request.Description
            };

            var stripeResult = await _stripeService.ChargeAsync(stripeRequest);

            return new PaymentResult
            {
                IsSuccess = stripeResult.Status == "succeeded",
                TransactionId = stripeResult.Id,
                Message = stripeResult.FailureMessage
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing Stripe payment");
            return new PaymentResult
            {
                IsSuccess = false,
                Message = "Payment processing failed"
            };
        }
    }

    public async Task<RefundResult> ProcessRefundAsync(RefundRequest request)
    {
        // Implementation for refund
        throw new NotImplementedException();
    }
}

// Infrastructure/Adapters/PayPalPaymentAdapter.cs
public class PayPalPaymentAdapter : IPaymentGatewayAdapter
{
    private readonly PayPalPaymentService _paypalService;
    private readonly ILogger<PayPalPaymentAdapter> _logger;

    public PayPalPaymentAdapter(PayPalPaymentService paypalService, ILogger<PayPalPaymentAdapter> logger)
    {
        _paypalService = paypalService;
        _logger = logger;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            var paypalRequest = new PayPalPaymentRequest
            {
                Amount = request.Amount.Amount,
                CurrencyCode = request.Amount.Currency.Code,
                PaymentMethod = request.PaymentMethod.Token
            };

            var paypalResult = await _paypalService.ExecutePaymentAsync(paypalRequest);

            return new PaymentResult
            {
                IsSuccess = paypalResult.State == "approved",
                TransactionId = paypalResult.Id,
                Message = paypalResult.FailureReason
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PayPal payment");
            return new PaymentResult
            {
                IsSuccess = false,
                Message = "Payment processing failed"
            };
        }
    }

    public async Task<RefundResult> ProcessRefundAsync(RefundRequest request)
    {
        // Implementation for refund
        throw new NotImplementedException();
    }
}
```

### 7.5 Decorator Pattern

Adiciona funcionalidades a objetos dinamicamente.

```csharp
// Application/Services/Decorators/ICachedCustomerService.cs
public interface ICachedCustomerService : ICustomerService
{
}

// Application/Services/Decorators/CachedCustomerService.cs
public class CachedCustomerService : ICachedCustomerService
{
    private readonly ICustomerService _customerService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CachedCustomerService> _logger;
    private readonly TimeSpan _cacheExpiration = TimeSpan.FromMinutes(10);

    public CachedCustomerService(
        ICustomerService customerService,
        IMemoryCache cache,
        ILogger<CachedCustomerService> logger)
    {
        _customerService = customerService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<CustomerDto> GetCustomerByIdAsync(int id)
    {
        var cacheKey = $"customer_{id}";
        
        if (_cache.TryGetValue(cacheKey, out CustomerDto cachedCustomer))
        {
            _logger.LogDebug("Customer {CustomerId} retrieved from cache", id);
            return cachedCustomer;
        }

        var customer = await _customerService.GetCustomerByIdAsync(id);
        
        if (customer != null)
        {
            _cache.Set(cacheKey, customer, _cacheExpiration);
            _logger.LogDebug("Customer {CustomerId} cached for {Minutes} minutes", id, _cacheExpiration.TotalMinutes);
        }

        return customer;
    }

    public async Task<IEnumerable<CustomerDto>> GetAllCustomersAsync()
    {
        const string cacheKey = "all_customers";
        
        if (_cache.TryGetValue(cacheKey, out IEnumerable<CustomerDto> cachedCustomers))
        {
            _logger.LogDebug("All customers retrieved from cache");
            return cachedCustomers;
        }

        var customers = await _customerService.GetAllCustomersAsync();
        
        _cache.Set(cacheKey, customers, _cacheExpiration);
        _logger.LogDebug("All customers cached for {Minutes} minutes", _cacheExpiration.TotalMinutes);

        return customers;
    }

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var customer = await _customerService.CreateCustomerAsync(request);
        
        // Invalidate relevant cache entries
        _cache.Remove("all_customers");
        _logger.LogDebug("Cache invalidated after creating customer");
        
        return customer;
    }

    // Other methods delegate to the wrapped service
    public Task<CustomerDto> UpdateCustomerAsync(int id, UpdateCustomerRequest request)
    {
        _cache.Remove($"customer_{id}");
        _cache.Remove("all_customers");
        return _customerService.UpdateCustomerAsync(id, request);
    }

    public Task DeleteCustomerAsync(int id)
    {
        _cache.Remove($"customer_{id}");
        _cache.Remove("all_customers");
        return _customerService.DeleteCustomerAsync(id);
    }
}

// Logging Decorator
public class LoggingCustomerService : ICustomerService
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<LoggingCustomerService> _logger;

    public LoggingCustomerService(ICustomerService customerService, ILogger<LoggingCustomerService> logger)
    {
        _customerService = customerService;
        _logger = logger;
    }

    public async Task<CustomerDto> GetCustomerByIdAsync(int id)
    {
        _logger.LogInformation("Getting customer {CustomerId}", id);
        
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var result = await _customerService.GetCustomerByIdAsync(id);
            stopwatch.Stop();
            
            _logger.LogInformation("Retrieved customer {CustomerId} in {ElapsedMs}ms", 
                                  id, stopwatch.ElapsedMilliseconds);
            return result;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error getting customer {CustomerId} after {ElapsedMs}ms", 
                           id, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    // Other methods follow the same pattern
}

// DI Registration with Decorators
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCustomerServices(this IServiceCollection services)
    {
        services.AddScoped<CustomerService>(); // Base service
        
        services.AddScoped<ICustomerService>(provider =>
        {
            var baseService = provider.GetRequiredService<CustomerService>();
            var logger = provider.GetRequiredService<ILogger<LoggingCustomerService>>();
            var loggingService = new LoggingCustomerService(baseService, logger);
            
            var cache = provider.GetRequiredService<IMemoryCache>();
            var cacheLogger = provider.GetRequiredService<ILogger<CachedCustomerService>>();
            return new CachedCustomerService(loggingService, cache, cacheLogger);
        });

        return services;
    }
}
```

### 7.6 Facade Pattern

Fornece uma interface simplificada para um subsistema complexo.

```csharp
// Application/Facades/IOrderProcessingFacade.cs
public interface IOrderProcessingFacade
{
    Task<OrderProcessingResult> ProcessOrderAsync(ProcessOrderRequest request);
}

// Application/Facades/OrderProcessingFacade.cs
public class OrderProcessingFacade : IOrderProcessingFacade
{
    private readonly IOrderService _orderService;
    private readonly IPaymentService _paymentService;
    private readonly IInventoryService _inventoryService;
    private readonly IShippingService _shippingService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<OrderProcessingFacade> _logger;

    public OrderProcessingFacade(
        IOrderService orderService,
        IPaymentService paymentService,
        IInventoryService inventoryService,
        IShippingService shippingService,
        INotificationService notificationService,
        ILogger<OrderProcessingFacade> logger)
    {
        _orderService = orderService;
        _paymentService = paymentService;
        _inventoryService = inventoryService;
        _shippingService = shippingService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task<OrderProcessingResult> ProcessOrderAsync(ProcessOrderRequest request)
    {
        using var scope = _logger.BeginScope("ProcessOrder {OrderId}", request.OrderId);
        
        try
        {
            _logger.LogInformation("Starting order processing for order {OrderId}", request.OrderId);

            // Step 1: Validate order
            var order = await _orderService.GetOrderAsync(request.OrderId);
            if (order == null)
            {
                return OrderProcessingResult.Failed("Order not found");
            }

            // Step 2: Check inventory
            var inventoryResult = await _inventoryService.CheckAvailabilityAsync(order.Items);
            if (!inventoryResult.IsAvailable)
            {
                return OrderProcessingResult.Failed("Insufficient inventory");
            }

            // Step 3: Reserve inventory
            await _inventoryService.ReserveItemsAsync(order.Items);

            try
            {
                // Step 4: Process payment
                var paymentResult = await _paymentService.ProcessPaymentAsync(new PaymentRequest
                {
                    Amount = order.TotalAmount,
                    PaymentMethod = request.PaymentMethod,
                    OrderId = order.Id
                });

                if (!paymentResult.IsSuccess)
                {
                    await _inventoryService.ReleaseReservedItemsAsync(order.Items);
                    return OrderProcessingResult.Failed($"Payment failed: {paymentResult.Message}");
                }

                // Step 5: Confirm order
                await _orderService.ConfirmOrderAsync(order.Id);

                // Step 6: Update inventory
                await _inventoryService.UpdateInventoryAsync(order.Items);

                // Step 7: Schedule shipping
                var shippingResult = await _shippingService.ScheduleShippingAsync(new ShippingRequest
                {
                    OrderId = order.Id,
                    ShippingAddress = order.ShippingAddress,
                    Items = order.Items
                });

                // Step 8: Send notifications
                await _notificationService.SendOrderConfirmationAsync(order.CustomerId, order.Id);

                _logger.LogInformation("Order {OrderId} processed successfully", request.OrderId);

                return OrderProcessingResult.Success(new OrderProcessingData
                {
                    OrderId = order.Id,
                    PaymentTransactionId = paymentResult.TransactionId,
                    ShippingTrackingNumber = shippingResult.TrackingNumber
                });
            }
            catch (Exception ex)
            {
                // Rollback inventory reservation on any failure after reservation
                await _inventoryService.ReleaseReservedItemsAsync(order.Items);
                throw;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing order {OrderId}", request.OrderId);
            return OrderProcessingResult.Failed("Order processing failed");
        }
    }
}

// Simple usage in ViewModel
public partial class OrderViewModel : ObservableObject
{
    private readonly IOrderProcessingFacade _orderProcessingFacade;

    [RelayCommand]
    private async Task ProcessOrderAsync()
    {
        try
        {
            IsProcessing = true;
            
            var request = new ProcessOrderRequest
            {
                OrderId = CurrentOrder.Id,
                PaymentMethod = SelectedPaymentMethod
            };

            var result = await _orderProcessingFacade.ProcessOrderAsync(request);
            
            if (result.IsSuccess)
            {
                // Show success message
                await ShowSuccessMessageAsync("Order processed successfully!");
            }
            else
            {
                // Show error message
                await ShowErrorMessageAsync(result.ErrorMessage);
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
```

## Padrões Comportamentais

### 7.7 Command Pattern

Encapsula solicitações como objetos.

```csharp
// Application/Commands/ICommand.cs
public interface ICommand<TResult>
{
    Task<TResult> ExecuteAsync();
}

public interface ICommand : ICommand<Unit>
{
}

// Application/Commands/CreateCustomerCommand.cs
public class CreateCustomerCommand : ICommand<CustomerDto>
{
    public string Name { get; set; }
    public string Email { get; set; }
    public CustomerType CustomerType { get; set; }
}

// Application/Commands/Handlers/CreateCustomerCommandHandler.cs
public class CreateCustomerCommandHandler : ICommandHandler<CreateCustomerCommand, CustomerDto>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerFactory _customerFactory;
    private readonly ILogger<CreateCustomerCommandHandler> _logger;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        ICustomerFactory customerFactory,
        ILogger<CreateCustomerCommandHandler> logger)
    {
        _customerRepository = customerRepository;
        _customerFactory = customerFactory;
        _logger = logger;
    }

    public async Task<CustomerDto> HandleAsync(CreateCustomerCommand command)
    {
        _logger.LogInformation("Creating customer with email {Email}", command.Email);

        // Check if customer already exists
        var existingCustomer = await _customerRepository.GetByEmailAsync(command.Email);
        if (existingCustomer != null)
        {
            throw new DomainException("Customer with this email already exists");
        }

        // Create customer using factory
        var customer = command.CustomerType switch
        {
            CustomerType.Standard => _customerFactory.CreateStandardCustomer(command.Name, command.Email),
            CustomerType.Premium => _customerFactory.CreatePremiumCustomer(command.Name, command.Email, string.Empty),
            CustomerType.Corporate => _customerFactory.CreateCorporateCustomer(command.Name, command.Email, string.Empty),
            _ => throw new ArgumentException("Invalid customer type")
        };

        await _customerRepository.AddAsync(customer);
        await _customerRepository.SaveChangesAsync();

        _logger.LogInformation("Customer {CustomerId} created successfully", customer.Id);

        return CustomerMapper.ToDto(customer);
    }
}

// Command Dispatcher
public interface ICommandDispatcher
{
    Task<TResult> DispatchAsync<TResult>(ICommand<TResult> command);
}

public class CommandDispatcher : ICommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public CommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> DispatchAsync<TResult>(ICommand<TResult> command)
    {
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));
        var handler = _serviceProvider.GetRequiredService(handlerType);
        
        var method = handlerType.GetMethod("HandleAsync");
        var result = await (Task<TResult>)method.Invoke(handler, new object[] { command });
        
        return result;
    }
}
```

### 7.8 Observer Pattern

Define dependência um-para-muitos entre objetos.

```csharp
// Domain/Events/IDomainEventDispatcher.cs
public interface IDomainEventDispatcher
{
    Task DispatchAsync<T>(T domainEvent) where T : IDomainEvent;
    void Subscribe<T>(IDomainEventHandler<T> handler) where T : IDomainEvent;
    void Unsubscribe<T>(IDomainEventHandler<T> handler) where T : IDomainEvent;
}

// Infrastructure/Events/DomainEventDispatcher.cs
public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly Dictionary<Type, List<object>> _handlers = new();
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(ILogger<DomainEventDispatcher> logger)
    {
        _logger = logger;
    }

    public async Task DispatchAsync<T>(T domainEvent) where T : IDomainEvent
    {
        var eventType = typeof(T);
        
        if (!_handlers.TryGetValue(eventType, out var handlers))
        {
            _logger.LogDebug("No handlers registered for event {EventType}", eventType.Name);
            return;
        }

        _logger.LogInformation("Dispatching event {EventType} to {HandlerCount} handlers", 
                              eventType.Name, handlers.Count);

        var tasks = handlers
            .Cast<IDomainEventHandler<T>>()
            .Select(handler => SafeHandleAsync(handler, domainEvent));

        await Task.WhenAll(tasks);
    }

    public void Subscribe<T>(IDomainEventHandler<T> handler) where T : IDomainEvent
    {
        var eventType = typeof(T);
        
        if (!_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers = new List<object>();
            _handlers[eventType] = handlers;
        }

        handlers.Add(handler);
        
        _logger.LogDebug("Handler {HandlerType} subscribed to event {EventType}", 
                        handler.GetType().Name, eventType.Name);
    }

    public void Unsubscribe<T>(IDomainEventHandler<T> handler) where T : IDomainEvent
    {
        var eventType = typeof(T);
        
        if (_handlers.TryGetValue(eventType, out var handlers))
        {
            handlers.Remove(handler);
            
            _logger.LogDebug("Handler {HandlerType} unsubscribed from event {EventType}", 
                            handler.GetType().Name, eventType.Name);
        }
    }

    private async Task SafeHandleAsync<T>(IDomainEventHandler<T> handler, T domainEvent) where T : IDomainEvent
    {
        try
        {
            await handler.HandleAsync(domainEvent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling event {EventType} with handler {HandlerType}", 
                           typeof(T).Name, handler.GetType().Name);
            // Don't rethrow - we don't want one handler failure to affect others
        }
    }
}

// Usage in Application Service
public class CustomerService : ICustomerService
{
    private readonly IDomainEventDispatcher _eventDispatcher;
    private readonly ICustomerRepository _customerRepository;

    public async Task<CustomerDto> CreateCustomerAsync(CreateCustomerRequest request)
    {
        var customer = new Customer(request.Name, request.Email);
        await _customerRepository.AddAsync(customer);
        
        // Dispatch domain event
        await _eventDispatcher.DispatchAsync(new CustomerCreatedEvent(
            customer.Id, 
            customer.Name, 
            customer.Email.Value
        ));

        return CustomerMapper.ToDto(customer);
    }
}
```

### 7.9 Strategy Pattern

Define família de algoritmos intercambiáveis.

```csharp
// Domain/Services/IDiscountStrategy.cs
public interface IDiscountStrategy
{
    string Name { get; }
    bool IsApplicable(Customer customer, Order order);
    Money CalculateDiscount(Customer customer, Order order);
}

// Domain/Services/Strategies/StandardDiscountStrategy.cs
public class StandardDiscountStrategy : IDiscountStrategy
{
    public string Name => "Standard Discount";

    public bool IsApplicable(Customer customer, Order order)
    {
        return customer.CustomerType == CustomerType.Standard && 
               order.TotalAmount.Amount > 100;
    }

    public Money CalculateDiscount(Customer customer, Order order)
    {
        var discountPercentage = 0.05m; // 5%
        return order.TotalAmount.Multiply(discountPercentage);
    }
}

// Domain/Services/Strategies/PremiumDiscountStrategy.cs
public class PremiumDiscountStrategy : IDiscountStrategy
{
    public string Name => "Premium Discount";

    public bool IsApplicable(Customer customer, Order order)
    {
        return customer.CustomerType == CustomerType.Premium;
    }

    public Money CalculateDiscount(Customer customer, Order order)
    {
        var discountPercentage = order.TotalAmount.Amount switch
        {
            > 500 => 0.15m, // 15%
            > 200 => 0.10m, // 10%
            _ => 0.05m      // 5%
        };

        return order.TotalAmount.Multiply(discountPercentage);
    }
}

// Domain/Services/Strategies/CorporateDiscountStrategy.cs
public class CorporateDiscountStrategy : IDiscountStrategy
{
    public string Name => "Corporate Discount";

    public bool IsApplicable(Customer customer, Order order)
    {
        return customer.CustomerType == CustomerType.Corporate;
    }

    public Money CalculateDiscount(Customer customer, Order order)
    {
        var discountPercentage = 0.20m; // 20%
        return order.TotalAmount.Multiply(discountPercentage);
    }
}

// Domain/Services/DiscountService.cs
public class DiscountService : IDiscountService
{
    private readonly IEnumerable<IDiscountStrategy> _discountStrategies;
    private readonly ILogger<DiscountService> _logger;

    public DiscountService(
        IEnumerable<IDiscountStrategy> discountStrategies,
        ILogger<DiscountService> logger)
    {
        _discountStrategies = discountStrategies;
        _logger = logger;
    }

    public Money CalculateBestDiscount(Customer customer, Order order)
    {
        var applicableStrategies = _discountStrategies
            .Where(strategy => strategy.IsApplicable(customer, order))
            .ToList();

        if (!applicableStrategies.Any())
        {
            _logger.LogDebug("No discount strategies applicable for customer {CustomerId}", customer.Id);
            return new Money(0, order.TotalAmount.Currency);
        }

        var bestDiscount = applicableStrategies
            .Select(strategy => new
            {
                Strategy = strategy,
                Discount = strategy.CalculateDiscount(customer, order)
            })
            .OrderByDescending(x => x.Discount.Amount)
            .First();

        _logger.LogInformation("Best discount for customer {CustomerId}: {DiscountAmount} using {StrategyName}",
                              customer.Id, bestDiscount.Discount.Amount, bestDiscount.Strategy.Name);

        return bestDiscount.Discount;
    }
}

// DI Registration
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDiscountStrategies(this IServiceCollection services)
    {
        services.AddScoped<IDiscountStrategy, StandardDiscountStrategy>();
        services.AddScoped<IDiscountStrategy, PremiumDiscountStrategy>();
        services.AddScoped<IDiscountStrategy, CorporateDiscountStrategy>();
        services.AddScoped<IDiscountService, DiscountService>();

        return services;
    }
}
```

### 7.10 Template Method Pattern

Define esqueleto de algoritmo, permitindo subclasses alterarem passos específicos.

```csharp
// Application/Services/Base/BaseImportService.cs
public abstract class BaseImportService<T>
{
    protected readonly ILogger _logger;

    protected BaseImportService(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<ImportResult<T>> ImportAsync(Stream dataStream)
    {
        var result = new ImportResult<T>();
        
        try
        {
            _logger.LogInformation("Starting import process for {EntityType}", typeof(T).Name);

            // Template method steps
            if (!await ValidateFileAsync(dataStream))
            {
                result.AddError("File validation failed");
                return result;
            }

            var rawData = await ParseFileAsync(dataStream);
            if (!rawData.Any())
            {
                result.AddError("No data found in file");
                return result;
            }

            var validationResults = await ValidateDataAsync(rawData);
            result.ValidationErrors.AddRange(validationResults);

            var validItems = rawData.Where(item => !validationResults.Any(v => v.LineNumber == GetLineNumber(item)));
            
            if (!validItems.Any())
            {
                result.AddError("No valid items to import");
                return result;
            }

            var entities = await TransformDataAsync(validItems);
            var savedEntities = await SaveEntitiesAsync(entities);
            
            await PostProcessAsync(savedEntities);

            result.SuccessCount = savedEntities.Count();
            result.ImportedItems.AddRange(savedEntities);

            _logger.LogInformation("Import completed successfully. {SuccessCount} items imported", 
                                  result.SuccessCount);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during import process");
            result.AddError($"Import failed: {ex.Message}");
            return result;
        }
    }

    // Abstract methods to be implemented by subclasses
    protected abstract Task<bool> ValidateFileAsync(Stream dataStream);
    protected abstract Task<IEnumerable<object>> ParseFileAsync(Stream dataStream);
    protected abstract Task<IEnumerable<ValidationError>> ValidateDataAsync(IEnumerable<object> rawData);
    protected abstract Task<IEnumerable<T>> TransformDataAsync(IEnumerable<object> validData);
    protected abstract Task<IEnumerable<T>> SaveEntitiesAsync(IEnumerable<T> entities);
    
    // Virtual methods with default implementations (can be overridden)
    protected virtual Task PostProcessAsync(IEnumerable<T> savedEntities)
    {
        return Task.CompletedTask;
    }

    protected virtual int GetLineNumber(object item)
    {
        // Default implementation
        return 0;
    }
}

// Concrete Implementation: Customer Import Service
public class CustomerImportService : BaseImportService<Customer>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ICustomerFactory _customerFactory;

    public CustomerImportService(
        ICustomerRepository customerRepository,
        ICustomerFactory customerFactory,
        ILogger<CustomerImportService> logger) : base(logger)
    {
        _customerRepository = customerRepository;
        _customerFactory = customerFactory;
    }

    protected override async Task<bool> ValidateFileAsync(Stream dataStream)
    {
        // Check file size, format, etc.
        if (dataStream.Length == 0)
            return false;

        if (dataStream.Length > 10 * 1024 * 1024) // 10MB limit
            return false;

        return true;
    }

    protected override async Task<IEnumerable<object>> ParseFileAsync(Stream dataStream)
    {
        // Parse CSV file
        using var reader = new StreamReader(dataStream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        
        var records = csv.GetRecords<CustomerCsvRecord>().ToList();
        return records.Cast<object>();
    }

    protected override async Task<IEnumerable<ValidationError>> ValidateDataAsync(IEnumerable<object> rawData)
    {
        var errors = new List<ValidationError>();
        var records = rawData.Cast<CustomerCsvRecord>();

        foreach (var (record, index) in records.Select((r, i) => (r, i)))
        {
            if (string.IsNullOrWhiteSpace(record.Name))
                errors.Add(new ValidationError(index + 1, "Name", "Name is required"));

            if (string.IsNullOrWhiteSpace(record.Email) || !IsValidEmail(record.Email))
                errors.Add(new ValidationError(index + 1, "Email", "Valid email is required"));

            // Check for duplicates
            var existingCustomer = await _customerRepository.GetByEmailAsync(record.Email);
            if (existingCustomer != null)
                errors.Add(new ValidationError(index + 1, "Email", "Customer with this email already exists"));
        }

        return errors;
    }

    protected override async Task<IEnumerable<Customer>> TransformDataAsync(IEnumerable<object> validData)
    {
        var records = validData.Cast<CustomerCsvRecord>();
        var customers = new List<Customer>();

        foreach (var record in records)
        {
            var customer = _customerFactory.CreateStandardCustomer(record.Name, record.Email);
            customers.Add(customer);
        }

        return customers;
    }

    protected override async Task<IEnumerable<Customer>> SaveEntitiesAsync(IEnumerable<Customer> entities)
    {
        var savedCustomers = new List<Customer>();

        foreach (var customer in entities)
        {
            await _customerRepository.AddAsync(customer);
            savedCustomers.Add(customer);
        }

        await _customerRepository.SaveChangesAsync();
        return savedCustomers;
    }

    protected override async Task PostProcessAsync(IEnumerable<Customer> savedEntities)
    {
        // Send welcome emails
        foreach (var customer in savedEntities)
        {
            // Dispatch domain event for welcome email
            await Task.Delay(100); // Simulate async operation
        }
    }

    private bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}
```

## Melhores Práticas

1. **Use Padrões Apropriados**: Não force padrões onde não são necessários
2. **Combine Padrões**: Muitos padrões trabalham bem juntos
3. **Mantenha Simplicidade**: Padrões devem simplificar, não complicar
4. **Documente Decisões**: Explique por que escolheu determinado padrão
5. **Teste Completamente**: Padrões devem facilitar testes
6. **Use Dependency Injection**: Facilita implementação de muitos padrões
7. **Considere Performance**: Alguns padrões podem impactar performance

## Próximos Tópicos

- [Diagramas UML](./08-diagramas-uml.md)
- [Estrutura do Projeto](./09-estrutura-projeto.md)
- [Configuração e Setup](./10-configuracao-setup.md)
