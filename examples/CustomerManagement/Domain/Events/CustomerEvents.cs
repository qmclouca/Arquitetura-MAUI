using CustomerManagement.Domain.ValueObjects;
using CustomerManagement.Domain.Entities;

namespace CustomerManagement.Domain.Events;

/// <summary>
/// Interface base para eventos de domínio
/// </summary>
public interface IDomainEvent
{
    Guid Id { get; }
    DateTime OccurredOn { get; }
}

/// <summary>
/// Classe base para eventos de domínio
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid Id { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

/// <summary>
/// Evento disparado quando um cliente é criado
/// </summary>
public record CustomerCreatedEvent(CustomerId CustomerId, string Email) : DomainEvent;

/// <summary>
/// Evento disparado quando um cliente é atualizado
/// </summary>
public record CustomerUpdatedEvent(CustomerId CustomerId, string Email) : DomainEvent;

/// <summary>
/// Evento disparado quando o tipo do cliente é alterado
/// </summary>
public record CustomerTypeChangedEvent(
    CustomerId CustomerId, 
    CustomerType OldType, 
    CustomerType NewType) : DomainEvent;

/// <summary>
/// Evento disparado quando um cliente é ativado
/// </summary>
public record CustomerActivatedEvent(CustomerId CustomerId, string Email) : DomainEvent;

/// <summary>
/// Evento disparado quando um cliente é inativado
/// </summary>
public record CustomerDeactivatedEvent(CustomerId CustomerId, string Email) : DomainEvent;

/// <summary>
/// Evento disparado quando um cliente é excluído
/// </summary>
public record CustomerDeletedEvent(CustomerId CustomerId, string Email) : DomainEvent;

/// <summary>
/// Evento disparado quando um cliente é restaurado
/// </summary>
public record CustomerRestoredEvent(CustomerId CustomerId, string Email) : DomainEvent;
