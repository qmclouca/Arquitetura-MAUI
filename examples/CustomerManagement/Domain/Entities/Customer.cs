using CustomerManagement.Domain.ValueObjects;
using CustomerManagement.Domain.Events;

namespace CustomerManagement.Domain.Entities;

/// <summary>
/// Entidade raiz do agregado Customer
/// Implementa DDD Entity Pattern com eventos de domínio
/// </summary>
public class Customer : IAggregateRoot
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public CustomerId Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber Phone { get; private set; }
    public Address Address { get; private set; }
    public CustomerType Type { get; private set; }
    public CustomerStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public bool IsDeleted { get; private set; }

    // Nome completo calculado
    public string FullName => $"{FirstName} {LastName}";

    // Construtor privado para EF Core
    private Customer() { }

    // Factory method para criação
    public static Customer Create(
        string firstName,
        string lastName,
        string email,
        string phone,
        Address address,
        CustomerType type = CustomerType.Standard)
    {
        var customer = new Customer
        {
            Id = CustomerId.New(),
            FirstName = firstName?.Trim() ?? throw new ArgumentNullException(nameof(firstName)),
            LastName = lastName?.Trim() ?? throw new ArgumentNullException(nameof(lastName)),
            Email = Email.Create(email),
            Phone = PhoneNumber.Create(phone),
            Address = address ?? throw new ArgumentNullException(nameof(address)),
            Type = type,
            Status = CustomerStatus.Active,
            CreatedAt = DateTime.UtcNow,
            IsDeleted = false
        };

        // Aplicar regras de negócio
        customer.ValidateCustomer();

        // Adicionar evento de domínio
        customer.AddDomainEvent(new CustomerCreatedEvent(customer.Id, customer.Email.Value));

        return customer;
    }

    /// <summary>
    /// Atualiza as informações básicas do cliente
    /// </summary>
    public void UpdateBasicInfo(string firstName, string lastName, string phone)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Não é possível atualizar um cliente excluído");

        var hasChanges = false;

        if (!string.Equals(FirstName, firstName?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            FirstName = firstName?.Trim() ?? throw new ArgumentNullException(nameof(firstName));
            hasChanges = true;
        }

        if (!string.Equals(LastName, lastName?.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            LastName = lastName?.Trim() ?? throw new ArgumentNullException(nameof(lastName));
            hasChanges = true;
        }

        var newPhone = PhoneNumber.Create(phone);
        if (!Phone.Equals(newPhone))
        {
            Phone = newPhone;
            hasChanges = true;
        }

        if (hasChanges)
        {
            UpdatedAt = DateTime.UtcNow;
            ValidateCustomer();
            AddDomainEvent(new CustomerUpdatedEvent(Id, Email.Value));
        }
    }

    /// <summary>
    /// Atualiza o endereço do cliente
    /// </summary>
    public void UpdateAddress(Address newAddress)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Não é possível atualizar um cliente excluído");

        if (newAddress == null)
            throw new ArgumentNullException(nameof(newAddress));

        if (!Address.Equals(newAddress))
        {
            Address = newAddress;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new CustomerUpdatedEvent(Id, Email.Value));
        }
    }

    /// <summary>
    /// Atualiza o tipo do cliente
    /// </summary>
    public void ChangeType(CustomerType newType)
    {
        if (IsDeleted)
            throw new InvalidOperationException("Não é possível atualizar um cliente excluído");

        if (Type != newType)
        {
            var oldType = Type;
            Type = newType;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new CustomerTypeChangedEvent(Id, oldType, newType));
        }
    }

    /// <summary>
    /// Ativa o cliente
    /// </summary>
    public void Activate()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Não é possível ativar um cliente excluído");

        if (Status != CustomerStatus.Active)
        {
            Status = CustomerStatus.Active;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new CustomerActivatedEvent(Id, Email.Value));
        }
    }

    /// <summary>
    /// Inativa o cliente
    /// </summary>
    public void Deactivate()
    {
        if (IsDeleted)
            throw new InvalidOperationException("Não é possível inativar um cliente excluído");

        if (Status != CustomerStatus.Inactive)
        {
            Status = CustomerStatus.Inactive;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new CustomerDeactivatedEvent(Id, Email.Value));
        }
    }

    /// <summary>
    /// Marca o cliente como excluído (soft delete)
    /// </summary>
    public void MarkAsDeleted()
    {
        if (!IsDeleted)
        {
            IsDeleted = true;
            Status = CustomerStatus.Inactive;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new CustomerDeletedEvent(Id, Email.Value));
        }
    }

    /// <summary>
    /// Restaura um cliente excluído
    /// </summary>
    public void Restore()
    {
        if (IsDeleted)
        {
            IsDeleted = false;
            Status = CustomerStatus.Active;
            UpdatedAt = DateTime.UtcNow;
            AddDomainEvent(new CustomerRestoredEvent(Id, Email.Value));
        }
    }

    /// <summary>
    /// Verifica se o cliente é Premium ou Corporate
    /// </summary>
    public bool IsPremiumCustomer() => Type is CustomerType.Premium or CustomerType.Corporate;

    /// <summary>
    /// Calcula a idade baseada na data de nascimento (se disponível)
    /// </summary>
    public int? CalculateAge(DateTime? birthDate)
    {
        if (!birthDate.HasValue) return null;

        var today = DateTime.Today;
        var age = today.Year - birthDate.Value.Year;
        
        if (birthDate.Value.Date > today.AddYears(-age))
            age--;

        return age;
    }

    /// <summary>
    /// Valida as regras de negócio do cliente
    /// </summary>
    private void ValidateCustomer()
    {
        if (string.IsNullOrWhiteSpace(FirstName))
            throw new ArgumentException("Nome é obrigatório", nameof(FirstName));

        if (string.IsNullOrWhiteSpace(LastName))
            throw new ArgumentException("Sobrenome é obrigatório", nameof(LastName));

        if (FirstName.Length < 2)
            throw new ArgumentException("Nome deve ter pelo menos 2 caracteres", nameof(FirstName));

        if (LastName.Length < 2)
            throw new ArgumentException("Sobrenome deve ter pelo menos 2 caracteres", nameof(LastName));

        if (FirstName.Length > 50)
            throw new ArgumentException("Nome não pode ter mais de 50 caracteres", nameof(FirstName));

        if (LastName.Length > 50)
            throw new ArgumentException("Sobrenome não pode ter mais de 50 caracteres", nameof(LastName));
    }

    // Implementação de eventos de domínio
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    public void AddDomainEvent(IDomainEvent eventItem)
    {
        _domainEvents.Add(eventItem);
    }

    public void RemoveDomainEvent(IDomainEvent eventItem)
    {
        _domainEvents.Remove(eventItem);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

/// <summary>
/// Interface para agregados que suportam eventos de domínio
/// </summary>
public interface IAggregateRoot
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }
    void AddDomainEvent(IDomainEvent eventItem);
    void RemoveDomainEvent(IDomainEvent eventItem);
    void ClearDomainEvents();
}

/// <summary>
/// Tipos de cliente
/// </summary>
public enum CustomerType
{
    Standard = 1,
    Premium = 2,
    Corporate = 3
}

/// <summary>
/// Status do cliente
/// </summary>
public enum CustomerStatus
{
    Active = 1,
    Inactive = 2,
    Suspended = 3
}
