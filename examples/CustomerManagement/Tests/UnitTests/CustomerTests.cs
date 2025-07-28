using CustomerManagement.Domain.Entities;
using CustomerManagement.Domain.ValueObjects;
using CustomerManagement.Domain.Events;
using FluentAssertions;
using Xunit;

namespace CustomerManagement.Tests.UnitTests.Domain;

/// <summary>
/// Testes unitários para a entidade Customer
/// Validam regras de negócio e comportamentos do domínio
/// </summary>
public class CustomerTests
{
    private readonly Address _validAddress;

    public CustomerTests()
    {
        _validAddress = new Address(
            "Rua das Flores", "123", "Centro", 
            "São Paulo", "SP", "01234567", "Brasil");
    }

    #region Factory Method Tests

    [Fact]
    public void Create_WithValidData_ShouldCreateCustomer()
    {
        // Arrange
        var firstName = "João";
        var lastName = "Silva";
        var email = "joao.silva@email.com";
        var phone = "11999999999";

        // Act
        var customer = Customer.Create(firstName, lastName, email, phone, _validAddress);

        // Assert
        customer.Should().NotBeNull();
        customer.Id.Should().NotBe(CustomerId.Create(Guid.Empty));
        customer.FirstName.Should().Be(firstName);
        customer.LastName.Should().Be(lastName);
        customer.FullName.Should().Be($"{firstName} {lastName}");
        customer.Email.Value.Should().Be(email.ToLowerInvariant());
        customer.Phone.Value.Should().Be("11999999999");
        customer.Address.Should().Be(_validAddress);
        customer.Type.Should().Be(CustomerType.Standard);
        customer.Status.Should().Be(CustomerStatus.Active);
        customer.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        customer.IsDeleted.Should().BeFalse();
        customer.DomainEvents.Should().ContainSingle(e => e is CustomerCreatedEvent);
    }

    [Fact]
    public void Create_WithPremiumType_ShouldCreatePremiumCustomer()
    {
        // Act
        var customer = Customer.Create(
            "Maria", "Santos", "maria@email.com", 
            "11888888888", _validAddress, CustomerType.Premium);

        // Assert
        customer.Type.Should().Be(CustomerType.Premium);
        customer.IsPremiumCustomer().Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithInvalidFirstName_ShouldThrowException(string invalidFirstName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Customer.Create(invalidFirstName, "Silva", "joao@email.com", "11999999999", _validAddress));
    }

    [Theory]
    [InlineData("A")] // Muito curto
    [InlineData("João Silva Santos Oliveira Pereira Costa Lima")] // Muito longo (>50 chars)
    public void Create_WithInvalidFirstNameLength_ShouldThrowException(string invalidFirstName)
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            Customer.Create(invalidFirstName, "Silva", "joao@email.com", "11999999999", _validAddress));
    }

    [Fact]
    public void Create_WithNullAddress_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            Customer.Create("João", "Silva", "joao@email.com", "11999999999", null!));
    }

    #endregion

    #region Update Methods Tests

    [Fact]
    public void UpdateBasicInfo_WithValidData_ShouldUpdateCustomer()
    {
        // Arrange
        var customer = Customer.Create("João", "Silva", "joao@email.com", "11999999999", _validAddress);
        var originalCreatedAt = customer.CreatedAt;
        var newFirstName = "Pedro";
        var newLastName = "Santos";
        var newPhone = "11888888888";

        // Act
        customer.UpdateBasicInfo(newFirstName, newLastName, newPhone);

        // Assert
        customer.FirstName.Should().Be(newFirstName);
        customer.LastName.Should().Be(newLastName);
        customer.FullName.Should().Be($"{newFirstName} {newLastName}");
        customer.Phone.Value.Should().Be(newPhone);
        customer.UpdatedAt.Should().NotBeNull();
        customer.UpdatedAt.Should().BeAfter(originalCreatedAt);
        customer.DomainEvents.Should().Contain(e => e is CustomerUpdatedEvent);
    }

    [Fact]
    public void UpdateBasicInfo_WithSameData_ShouldNotUpdateOrRaiseEvent()
    {
        // Arrange
        var customer = Customer.Create("João", "Silva", "joao@email.com", "11999999999", _validAddress);
        var originalUpdatedAt = customer.UpdatedAt;
        customer.ClearDomainEvents(); // Limpar evento de criação

        // Act
        customer.UpdateBasicInfo("João", "Silva", "11999999999");

        // Assert
        customer.UpdatedAt.Should().Be(originalUpdatedAt);
        customer.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void UpdateBasicInfo_WhenDeleted_ShouldThrowException()
    {
        // Arrange
        var customer = Customer.Create("João", "Silva", "joao@email.com", "11999999999", _validAddress);
        customer.MarkAsDeleted();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            customer.UpdateBasicInfo("Pedro", "Santos", "11888888888"));
    }

    [Fact]
    public void UpdateAddress_WithNewAddress_ShouldUpdateCustomer()
    {
        // Arrange
        var customer = Customer.Create("João", "Silva", "joao@email.com", "11999999999", _validAddress);
        var newAddress = new Address("Nova Rua", "456", "Novo Bairro", "Rio de Janeiro", "RJ", "87654321");

        // Act
        customer.UpdateAddress(newAddress);

        // Assert
        customer.Address.Should().Be(newAddress);
        customer.UpdatedAt.Should().NotBeNull();
        customer.DomainEvents.Should().Contain(e => e is CustomerUpdatedEvent);
    }

    [Fact]
    public void ChangeType_ToDifferentType_ShouldUpdateTypeAndRaiseEvent()
    {
        // Arrange
        var customer = Customer.Create("João", "Silva", "joao@email.com", "11999999999", _validAddress);
        var originalType = customer.Type;

        // Act
        customer.ChangeType(CustomerType.Premium);

        // Assert
        customer.Type.Should().Be(CustomerType.Premium);
        customer.UpdatedAt.Should().NotBeNull();
        customer.DomainEvents.Should().Contain(e => 
            e is CustomerTypeChangedEvent evt && 
            evt.OldType == originalType && 
            evt.NewType == CustomerType.Premium);
    }

    #endregion

    #region Status Management Tests

    [Fact]
    public void Activate_WhenInactive_ShouldActivateCustomer()
    {
        // Arrange
        var customer = Customer.Create("João", "Silva", "joao@email.com", "11999999999", _validAddress);
        customer.Deactivate();
        customer.ClearDomainEvents();

        // Act
        customer.Activate();

        // Assert
        customer.Status.Should().Be(CustomerStatus.Active);
        customer.UpdatedAt.Should().NotBeNull();
        customer.DomainEvents.Should().ContainSingle(e => e is CustomerActivatedEvent);
    }

    [Fact]
    public void Deactivate_WhenActive_ShouldDeactivateCustomer()
    {
        // Arrange
        var customer = Customer.Create("João", "Silva", "joao@email.com", "11999999999", _validAddress);
        customer.ClearDomainEvents();

        // Act
        customer.Deactivate();

        // Assert
        customer.Status.Should().Be(CustomerStatus.Inactive);
        customer.UpdatedAt.Should().NotBeNull();
        customer.DomainEvents.Should().ContainSingle(e => e is CustomerDeactivatedEvent);
    }

    [Fact]
    public void MarkAsDeleted_WhenActive_ShouldMarkAsDeletedAndInactive()
    {
        // Arrange
        var customer = Customer.Create("João", "Silva", "joao@email.com", "11999999999", _validAddress);
        customer.ClearDomainEvents();

        // Act
        customer.MarkAsDeleted();

        // Assert
        customer.IsDeleted.Should().BeTrue();
        customer.Status.Should().Be(CustomerStatus.Inactive);
        customer.UpdatedAt.Should().NotBeNull();
        customer.DomainEvents.Should().ContainSingle(e => e is CustomerDeletedEvent);
    }

    [Fact]
    public void Restore_WhenDeleted_ShouldRestoreAndActivateCustomer()
    {
        // Arrange
        var customer = Customer.Create("João", "Silva", "joao@email.com", "11999999999", _validAddress);
        customer.MarkAsDeleted();
        customer.ClearDomainEvents();

        // Act
        customer.Restore();

        // Assert
        customer.IsDeleted.Should().BeFalse();
        customer.Status.Should().Be(CustomerStatus.Active);
        customer.UpdatedAt.Should().NotBeNull();
        customer.DomainEvents.Should().ContainSingle(e => e is CustomerRestoredEvent);
    }

    #endregion

    #region Business Logic Tests

    [Theory]
    [InlineData(CustomerType.Premium, true)]
    [InlineData(CustomerType.Corporate, true)]
    [InlineData(CustomerType.Standard, false)]
    public void IsPremiumCustomer_ShouldReturnCorrectValue(CustomerType type, bool expectedResult)
    {
        // Arrange
        var customer = Customer.Create("João", "Silva", "joao@email.com", "11999999999", _validAddress, type);

        // Act
        var result = customer.IsPremiumCustomer();

        // Assert
        result.Should().Be(expectedResult);
    }

    [Theory]
    [InlineData("1990-01-01", 34)] // Assumindo ano atual 2024
    [InlineData("2000-05-15", 24)]
    [InlineData("1985-12-31", 38)]
    public void CalculateAge_WithValidBirthDate_ShouldReturnCorrectAge(string birthDateStr, int expectedAge)
    {
        // Arrange
        var customer = Customer.Create("João", "Silva", "joao@email.com", "11999999999", _validAddress);
        var birthDate = DateTime.Parse(birthDateStr);

        // Act
        var age = customer.CalculateAge(birthDate);

        // Assert
        age.Should().Be(expectedAge);
    }

    [Fact]
    public void CalculateAge_WithNullBirthDate_ShouldReturnNull()
    {
        // Arrange
        var customer = Customer.Create("João", "Silva", "joao@email.com", "11999999999", _validAddress);

        // Act
        var age = customer.CalculateAge(null);

        // Assert
        age.Should().BeNull();
    }

    #endregion

    #region Domain Events Tests

    [Fact]
    public void DomainEvents_AfterCreation_ShouldContainCreatedEvent()
    {
        // Act
        var customer = Customer.Create("João", "Silva", "joao@email.com", "11999999999", _validAddress);

        // Assert
        customer.DomainEvents.Should().ContainSingle();
        var domainEvent = customer.DomainEvents.Single();
        domainEvent.Should().BeOfType<CustomerCreatedEvent>();
        
        var createdEvent = (CustomerCreatedEvent)domainEvent;
        createdEvent.CustomerId.Should().Be(customer.Id);
        createdEvent.Email.Should().Be(customer.Email.Value);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var customer = Customer.Create("João", "Silva", "joao@email.com", "11999999999", _validAddress);
        customer.UpdateBasicInfo("Pedro", "Santos", "11888888888");

        // Act
        customer.ClearDomainEvents();

        // Assert
        customer.DomainEvents.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void Create_WithEmailUpperCase_ShouldConvertToLowerCase()
    {
        // Act
        var customer = Customer.Create("João", "Silva", "JOAO.SILVA@EMAIL.COM", "11999999999", _validAddress);

        // Assert
        customer.Email.Value.Should().Be("joao.silva@email.com");
    }

    [Fact]
    public void Create_WithNameSpaces_ShouldTrimSpaces()
    {
        // Act
        var customer = Customer.Create("  João  ", "  Silva  ", "joao@email.com", "11999999999", _validAddress);

        // Assert
        customer.FirstName.Should().Be("João");
        customer.LastName.Should().Be("Silva");
    }

    [Fact]
    public void UpdateBasicInfo_MultipleTimes_ShouldAccumulateEvents()
    {
        // Arrange
        var customer = Customer.Create("João", "Silva", "joao@email.com", "11999999999", _validAddress);
        customer.ClearDomainEvents();

        // Act
        customer.UpdateBasicInfo("Pedro", "Silva", "11999999999");
        customer.UpdateBasicInfo("Pedro", "Santos", "11999999999");

        // Assert
        customer.DomainEvents.Should().HaveCount(2);
        customer.DomainEvents.Should().AllBeOfType<CustomerUpdatedEvent>();
    }

    #endregion
}
