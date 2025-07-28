# 4. Domain-Driven Design (DDD)

## Introdução ao DDD

Domain-Driven Design é uma abordagem para desenvolvimento de software que foca na modelagem do domínio de negócio e na colaboração entre especialistas técnicos e de domínio.

## Conceitos Fundamentais

### 4.1 Entidades (Entities)

Objetos que possuem identidade única e persistem ao longo do tempo.

```csharp
public class Customer : Entity<int>
{
    private readonly List<Order> _orders = new();
    
    public Customer(string name, Email email)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Email = email ?? throw new ArgumentNullException(nameof(email));
        CreatedAt = DateTime.UtcNow;
        Status = CustomerStatus.Active;
    }
    
    public string Name { get; private set; }
    public Email Email { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public CustomerStatus Status { get; private set; }
    public IReadOnlyList<Order> Orders => _orders.AsReadOnly();
    
    public void UpdateEmail(Email newEmail)
    {
        if (newEmail == null)
            throw new ArgumentNullException(nameof(newEmail));
            
        Email = newEmail;
        AddDomainEvent(new CustomerEmailUpdatedEvent(Id, newEmail.Value));
    }
    
    public void Deactivate()
    {
        if (Status == CustomerStatus.Inactive)
            throw new DomainException("Customer is already inactive");
            
        Status = CustomerStatus.Inactive;
        AddDomainEvent(new CustomerDeactivatedEvent(Id));
    }
    
    public Order CreateOrder(IEnumerable<OrderItem> items)
    {
        if (Status == CustomerStatus.Inactive)
            throw new DomainException("Cannot create order for inactive customer");
            
        var order = new Order(Id, items);
        _orders.Add(order);
        
        AddDomainEvent(new OrderCreatedEvent(order.Id, Id));
        return order;
    }
}

// Base Entity Class
public abstract class Entity<TId> : IEquatable<Entity<TId>>
{
    private readonly List<IDomainEvent> _domainEvents = new();
    
    public TId Id { get; protected set; }
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
    
    public bool Equals(Entity<TId>? other)
    {
        return other != null && Id.Equals(other.Id);
    }
    
    public override bool Equals(object? obj)
    {
        return Equals(obj as Entity<TId>);
    }
    
    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
```

### 4.2 Value Objects

Objetos imutáveis que são definidos por seus atributos, não por identidade.

```csharp
public class Email : ValueObject
{
    public string Value { get; }
    
    public Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email cannot be empty", nameof(value));
            
        if (!IsValidEmail(value))
            throw new ArgumentException("Invalid email format", nameof(value));
            
        Value = value.ToLowerInvariant();
    }
    
    private static bool IsValidEmail(string email)
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
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    
    public static implicit operator string(Email email) => email.Value;
    public static implicit operator Email(string email) => new(email);
}

public class Money : ValueObject
{
    public decimal Amount { get; }
    public Currency Currency { get; }
    
    public Money(decimal amount, Currency currency)
    {
        if (amount < 0)
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
            
        Amount = amount;
        Currency = currency ?? throw new ArgumentNullException(nameof(currency));
    }
    
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
            
        return new Money(Amount + other.Amount, Currency);
    }
    
    public Money Multiply(decimal factor)
    {
        return new Money(Amount * factor, Currency);
    }
    
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
    
    public override string ToString() => $"{Amount:C} {Currency.Code}";
}

// Base Value Object Class
public abstract class ValueObject : IEquatable<ValueObject>
{
    protected abstract IEnumerable<object> GetEqualityComponents();
    
    public bool Equals(ValueObject? other)
    {
        return other != null && GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
    }
    
    public override bool Equals(object? obj)
    {
        return Equals(obj as ValueObject);
    }
    
    public override int GetHashCode()
    {
        return GetEqualityComponents()
            .Select(x => x?.GetHashCode() ?? 0)
            .Aggregate((x, y) => x ^ y);
    }
}
```

### 4.3 Aggregates e Aggregate Roots

Aggregates definem limites de consistência e transação.

```csharp
public class Order : AggregateRoot<int>
{
    private readonly List<OrderItem> _items = new();
    
    public Order(int customerId, IEnumerable<OrderItem> items)
    {
        CustomerId = customerId;
        OrderDate = DateTime.UtcNow;
        Status = OrderStatus.Pending;
        
        foreach (var item in items)
        {
            AddItem(item.ProductId, item.Quantity, item.UnitPrice);
        }
        
        if (!_items.Any())
            throw new DomainException("Order must have at least one item");
    }
    
    public int CustomerId { get; private set; }
    public DateTime OrderDate { get; private set; }
    public OrderStatus Status { get; private set; }
    public Money TotalAmount => _items.Aggregate(
        new Money(0, Currency.USD),
        (total, item) => total.Add(item.TotalPrice)
    );
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    
    public void AddItem(int productId, int quantity, Money unitPrice)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Cannot add items to a confirmed order");
            
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);
        
        if (existingItem != null)
        {
            existingItem.UpdateQuantity(existingItem.Quantity + quantity);
        }
        else
        {
            _items.Add(new OrderItem(productId, quantity, unitPrice));
        }
        
        AddDomainEvent(new OrderItemAddedEvent(Id, productId, quantity));
    }
    
    public void RemoveItem(int productId)
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Cannot remove items from a confirmed order");
            
        var item = _items.FirstOrDefault(i => i.ProductId == productId);
        if (item != null)
        {
            _items.Remove(item);
            AddDomainEvent(new OrderItemRemovedEvent(Id, productId));
        }
    }
    
    public void Confirm()
    {
        if (Status != OrderStatus.Pending)
            throw new DomainException("Only pending orders can be confirmed");
            
        if (!_items.Any())
            throw new DomainException("Cannot confirm order without items");
            
        Status = OrderStatus.Confirmed;
        AddDomainEvent(new OrderConfirmedEvent(Id, CustomerId, TotalAmount));
    }
    
    public void Ship()
    {
        if (Status != OrderStatus.Confirmed)
            throw new DomainException("Only confirmed orders can be shipped");
            
        Status = OrderStatus.Shipped;
        AddDomainEvent(new OrderShippedEvent(Id));
    }
}

public class OrderItem : Entity<int>
{
    public OrderItem(int productId, int quantity, Money unitPrice)
    {
        ProductId = productId;
        Quantity = quantity > 0 ? quantity : throw new ArgumentException("Quantity must be positive");
        UnitPrice = unitPrice ?? throw new ArgumentNullException(nameof(unitPrice));
    }
    
    public int ProductId { get; private set; }
    public int Quantity { get; private set; }
    public Money UnitPrice { get; private set; }
    public Money TotalPrice => UnitPrice.Multiply(Quantity);
    
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be positive");
            
        Quantity = newQuantity;
    }
}
```

### 4.4 Domain Services

Contêm lógica de domínio que não pertence a uma entidade específica.

```csharp
public interface IPricingService
{
    Money CalculateOrderTotal(IEnumerable<OrderItem> items, Customer customer);
    Money CalculateDiscountedPrice(Money originalPrice, Customer customer);
}

public class PricingService : IPricingService
{
    private readonly IDiscountRepository _discountRepository;
    
    public PricingService(IDiscountRepository discountRepository)
    {
        _discountRepository = discountRepository;
    }
    
    public Money CalculateOrderTotal(IEnumerable<OrderItem> items, Customer customer)
    {
        var subtotal = items.Aggregate(
            new Money(0, Currency.USD),
            (total, item) => total.Add(item.TotalPrice)
        );
        
        var discountedTotal = CalculateDiscountedPrice(subtotal, customer);
        var tax = CalculateTax(discountedTotal);
        
        return discountedTotal.Add(tax);
    }
    
    public Money CalculateDiscountedPrice(Money originalPrice, Customer customer)
    {
        var applicableDiscounts = _discountRepository.GetApplicableDiscounts(customer);
        
        var totalDiscountPercentage = applicableDiscounts
            .Sum(d => d.Percentage);
        
        var discountAmount = originalPrice.Multiply(totalDiscountPercentage / 100m);
        
        return originalPrice.Add(discountAmount.Multiply(-1));
    }
    
    private Money CalculateTax(Money amount)
    {
        // Tax calculation logic
        const decimal taxRate = 0.10m; // 10%
        return amount.Multiply(taxRate);
    }
}
```

### 4.5 Domain Events

Comunicam mudanças importantes no domínio.

```csharp
public interface IDomainEvent
{
    DateTime OccurredOn { get; }
    Guid Id { get; }
}

public abstract class DomainEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid Id { get; } = Guid.NewGuid();
}

public class CustomerCreatedEvent : DomainEvent
{
    public CustomerCreatedEvent(int customerId, string customerName, string email)
    {
        CustomerId = customerId;
        CustomerName = customerName;
        Email = email;
    }
    
    public int CustomerId { get; }
    public string CustomerName { get; }
    public string Email { get; }
}

public class OrderConfirmedEvent : DomainEvent
{
    public OrderConfirmedEvent(int orderId, int customerId, Money totalAmount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
    }
    
    public int OrderId { get; }
    public int CustomerId { get; }
    public Money TotalAmount { get; }
}

// Domain Event Handler
public interface IDomainEventHandler<T> where T : IDomainEvent
{
    Task HandleAsync(T domainEvent);
}

public class OrderConfirmedEventHandler : IDomainEventHandler<OrderConfirmedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ICustomerRepository _customerRepository;
    private readonly ILogger<OrderConfirmedEventHandler> _logger;
    
    public OrderConfirmedEventHandler(
        IEmailService emailService,
        ICustomerRepository customerRepository,
        ILogger<OrderConfirmedEventHandler> logger)
    {
        _emailService = emailService;
        _customerRepository = customerRepository;
        _logger = logger;
    }
    
    public async Task HandleAsync(OrderConfirmedEvent domainEvent)
    {
        _logger.LogInformation("Handling order confirmed event for order {OrderId}", 
                              domainEvent.OrderId);
        
        var customer = await _customerRepository.GetByIdAsync(domainEvent.CustomerId);
        if (customer != null)
        {
            await _emailService.SendOrderConfirmationAsync(
                customer.Email, 
                domainEvent.OrderId, 
                domainEvent.TotalAmount
            );
        }
    }
}
```

### 4.6 Repositories

Abstração para acesso a dados de aggregates.

```csharp
public interface IRepository<T> where T : AggregateRoot<int>
{
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> GetAllAsync();
    Task AddAsync(T entity);
    Task UpdateAsync(T entity);
    Task DeleteAsync(T entity);
    Task<int> SaveChangesAsync();
}

public interface ICustomerRepository : IRepository<Customer>
{
    Task<Customer?> GetByEmailAsync(Email email);
    Task<IEnumerable<Customer>> GetActiveCustomersAsync();
    Task<IEnumerable<Customer>> SearchByNameAsync(string name);
}

public interface IOrderRepository : IRepository<Order>
{
    Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId);
    Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
    Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
}
```

### 4.7 Specifications

Encapsulam lógica de consulta complexa.

```csharp
public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();
    
    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }
    
    public Specification<T> And(Specification<T> specification)
    {
        return new AndSpecification<T>(this, specification);
    }
    
    public Specification<T> Or(Specification<T> specification)
    {
        return new OrSpecification<T>(this, specification);
    }
}

public class ActiveCustomerSpecification : Specification<Customer>
{
    public override Expression<Func<Customer, bool>> ToExpression()
    {
        return customer => customer.Status == CustomerStatus.Active;
    }
}

public class CustomerWithRecentOrdersSpecification : Specification<Customer>
{
    private readonly DateTime _cutoffDate;
    
    public CustomerWithRecentOrdersSpecification(DateTime cutoffDate)
    {
        _cutoffDate = cutoffDate;
    }
    
    public override Expression<Func<Customer, bool>> ToExpression()
    {
        return customer => customer.Orders.Any(o => o.OrderDate >= _cutoffDate);
    }
}

// Usage
var activeCustomersWithRecentOrders = new ActiveCustomerSpecification()
    .And(new CustomerWithRecentOrdersSpecification(DateTime.UtcNow.AddMonths(-3)));

var customers = await _customerRepository.FindAsync(activeCustomersWithRecentOrders);
```

## Melhores Práticas DDD

1. **Modele o Domínio Primeiro**: Foque na lógica de negócio antes da tecnologia
2. **Use Linguagem Ubíqua**: Mesma linguagem entre desenvolvedores e especialistas
3. **Mantenha Aggregates Pequenos**: Facilita performance e manutenção
4. **Proteja Invariantes**: Garanta consistência dentro dos agregados
5. **Use Domain Events**: Para comunicação entre bounded contexts
6. **Separe Leitura de Escrita**: Consider CQRS para operações complexas
7. **Teste o Domínio**: Foque em testes unitários das regras de negócio

## Próximos Tópicos

- [Design System](./05-design-system.md)
- [Logging com Serilog](./06-logging-serilog.md)
- [Design Patterns](./07-design-patterns.md)
