using CustomerManagement.Domain.Entities;

namespace CustomerManagement.Application.DTOs;

/// <summary>
/// DTO para exibição de dados do cliente
/// </summary>
public record CustomerDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public AddressDto Address { get; init; } = new();
    public CustomerType Type { get; init; }
    public CustomerStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public bool IsDeleted { get; init; }

    // Properties calculadas
    public string TypeDescription => Type switch
    {
        CustomerType.Standard => "Padrão",
        CustomerType.Premium => "Premium",
        CustomerType.Corporate => "Corporativo",
        _ => "Desconhecido"
    };

    public string StatusDescription => Status switch
    {
        CustomerStatus.Active => "Ativo",
        CustomerStatus.Inactive => "Inativo",
        CustomerStatus.Suspended => "Suspenso",
        _ => "Desconhecido"
    };

    public string FormattedPhone => FormatPhone(Phone);
    public string FormattedAddress => Address.GetSingleLineAddress();

    private static string FormatPhone(string phone)
    {
        if (string.IsNullOrEmpty(phone)) return string.Empty;

        return phone.Length switch
        {
            11 => $"({phone[..2]}) {phone[2..7]}-{phone[7..]}",
            10 => $"({phone[..2]}) {phone[2..6]}-{phone[6..]}",
            _ => phone
        };
    }
}

/// <summary>
/// DTO para criação de cliente
/// </summary>
public record CreateCustomerDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public AddressDto Address { get; init; } = new();
    public CustomerType Type { get; init; } = CustomerType.Standard;
}

/// <summary>
/// DTO para atualização de cliente
/// </summary>
public record UpdateCustomerDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
    public AddressDto Address { get; init; } = new();
    public CustomerType Type { get; init; }
}

/// <summary>
/// DTO para endereço
/// </summary>
public record AddressDto
{
    public string Street { get; init; } = string.Empty;
    public string Number { get; init; } = string.Empty;
    public string? Complement { get; init; }
    public string Neighborhood { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string ZipCode { get; init; } = string.Empty;
    public string Country { get; init; } = "Brasil";

    public AddressDto() { }

    public AddressDto(
        string street,
        string number,
        string neighborhood,
        string city,
        string state,
        string zipCode,
        string country = "Brasil",
        string? complement = null)
    {
        Street = street;
        Number = number;
        Neighborhood = neighborhood;
        City = city;
        State = state;
        ZipCode = zipCode;
        Country = country;
        Complement = complement;
    }

    /// <summary>
    /// Retorna o endereço formatado para exibição
    /// </summary>
    public string GetFormattedAddress()
    {
        var address = $"{Street}, {Number}";
        
        if (!string.IsNullOrWhiteSpace(Complement))
            address += $" - {Complement}";

        address += $"\n{Neighborhood}";
        address += $"\n{City}/{State}";
        address += $"\nCEP: {FormatZipCode()}";

        if (!string.Equals(Country, "Brasil", StringComparison.OrdinalIgnoreCase))
            address += $"\n{Country}";

        return address;
    }

    /// <summary>
    /// Retorna o endereço em uma única linha
    /// </summary>
    public string GetSingleLineAddress()
    {
        var address = $"{Street}, {Number}";
        
        if (!string.IsNullOrWhiteSpace(Complement))
            address += $" - {Complement}";

        address += $", {Neighborhood}, {City}/{State}, CEP: {FormatZipCode()}";

        if (!string.Equals(Country, "Brasil", StringComparison.OrdinalIgnoreCase))
            address += $", {Country}";

        return address;
    }

    /// <summary>
    /// Formata o CEP para exibição (99999-999)
    /// </summary>
    public string FormatZipCode()
    {
        if (string.IsNullOrEmpty(ZipCode)) return string.Empty;
        
        var digits = new string(ZipCode.Where(char.IsDigit).ToArray());
        
        return digits.Length == 8 
            ? $"{digits[..5]}-{digits[5..]}" 
            : ZipCode;
    }
}

/// <summary>
/// DTO para resultado paginado
/// </summary>
public record PagedResult<T>
{
    public IEnumerable<T> Items { get; init; } = Enumerable.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// DTO para filtros de busca
/// </summary>
public record CustomerSearchFilter
{
    public string? SearchText { get; init; }
    public CustomerType? Type { get; init; }
    public CustomerStatus? Status { get; init; }
    public bool? IsDeleted { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
    public string? City { get; init; }
    public string? State { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// DTO para estatísticas de clientes
/// </summary>
public record CustomerStatistics
{
    public int TotalCustomers { get; init; }
    public int ActiveCustomers { get; init; }
    public int InactiveCustomers { get; init; }
    public int StandardCustomers { get; init; }
    public int PremiumCustomers { get; init; }
    public int CorporateCustomers { get; init; }
    public int CustomersCreatedThisMonth { get; init; }
    public int CustomersCreatedThisYear { get; init; }
    public Dictionary<string, int> CustomersByCity { get; init; } = new();
    public Dictionary<string, int> CustomersByState { get; init; } = new();
}
