using System.Text.RegularExpressions;

namespace CustomerManagement.Domain.ValueObjects;

/// <summary>
/// Value Object para ID de Cliente
/// Implementa Strong Typing e validação
/// </summary>
public readonly record struct CustomerId
{
    public Guid Value { get; }

    private CustomerId(Guid value)
    {
        if (value == Guid.Empty)
            throw new ArgumentException("Customer ID não pode ser vazio", nameof(value));
        
        Value = value;
    }

    public static CustomerId New() => new(Guid.NewGuid());
    
    public static CustomerId Create(Guid value) => new(value);
    
    public static CustomerId Create(string value)
    {
        if (!Guid.TryParse(value, out var guid))
            throw new ArgumentException("Formato de Customer ID inválido", nameof(value));
        
        return new CustomerId(guid);
    }

    public override string ToString() => Value.ToString();

    public static implicit operator Guid(CustomerId customerId) => customerId.Value;
    public static implicit operator CustomerId(Guid guid) => new(guid);
}

/// <summary>
/// Value Object para Email
/// Implementa validação de formato
/// </summary>
public readonly record struct Email
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public string Value { get; }

    private Email(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Email é obrigatório", nameof(value));

        var normalizedEmail = value.Trim().ToLowerInvariant();
        
        if (normalizedEmail.Length > 254)
            throw new ArgumentException("Email não pode ter mais de 254 caracteres", nameof(value));

        if (!EmailRegex.IsMatch(normalizedEmail))
            throw new ArgumentException("Formato de email inválido", nameof(value));

        Value = normalizedEmail;
    }

    public static Email Create(string value) => new(value);

    public string Domain => Value.Split('@')[1];
    
    public string LocalPart => Value.Split('@')[0];

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}

/// <summary>
/// Value Object para Número de Telefone
/// Implementa validação e formatação
/// </summary>
public readonly record struct PhoneNumber
{
    private static readonly Regex PhoneRegex = new(
        @"^[\+]?[1-9][\d]{0,15}$",
        RegexOptions.Compiled);

    public string Value { get; }

    private PhoneNumber(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Número de telefone é obrigatório", nameof(value));

        // Remove caracteres especiais
        var cleanPhone = new string(value.Where(char.IsDigit).ToArray());
        
        if (string.IsNullOrEmpty(cleanPhone))
            throw new ArgumentException("Número de telefone deve conter dígitos", nameof(value));

        if (cleanPhone.Length < 8 || cleanPhone.Length > 15)
            throw new ArgumentException("Número de telefone deve ter entre 8 e 15 dígitos", nameof(value));

        Value = cleanPhone;
    }

    public static PhoneNumber Create(string value) => new(value);

    /// <summary>
    /// Formata o número para exibição (formato brasileiro)
    /// </summary>
    public string FormatBrazilian()
    {
        return Value.Length switch
        {
            11 => $"({Value[..2]}) {Value[2..7]}-{Value[7..]}",  // Celular: (11) 99999-9999
            10 => $"({Value[..2]}) {Value[2..6]}-{Value[6..]}",   // Fixo: (11) 9999-9999
            _ => Value // Outros formatos retornam sem formatação
        };
    }

    public override string ToString() => Value;

    public static implicit operator string(PhoneNumber phone) => phone.Value;
}

/// <summary>
/// Value Object para Endereço
/// Implementa validação de campos obrigatórios
/// </summary>
public readonly record struct Address
{
    public string Street { get; }
    public string Number { get; }
    public string? Complement { get; }
    public string Neighborhood { get; }
    public string City { get; }
    public string State { get; }
    public string ZipCode { get; }
    public string Country { get; }

    public Address(
        string street,
        string number,
        string neighborhood,
        string city,
        string state,
        string zipCode,
        string country = "Brasil",
        string? complement = null)
    {
        Street = ValidateAndTrim(street, nameof(street), 3, 100);
        Number = ValidateAndTrim(number, nameof(number), 1, 10);
        Neighborhood = ValidateAndTrim(neighborhood, nameof(neighborhood), 2, 50);
        City = ValidateAndTrim(city, nameof(city), 2, 50);
        State = ValidateAndTrim(state, nameof(state), 2, 50);
        Country = ValidateAndTrim(country, nameof(country), 2, 50);
        Complement = string.IsNullOrWhiteSpace(complement) ? null : complement.Trim();

        // Validação específica para CEP brasileiro
        ZipCode = ValidateZipCode(zipCode);
    }

    private static string ValidateAndTrim(string value, string fieldName, int minLength, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{fieldName} é obrigatório", fieldName);

        var trimmed = value.Trim();
        
        if (trimmed.Length < minLength)
            throw new ArgumentException($"{fieldName} deve ter pelo menos {minLength} caracteres", fieldName);

        if (trimmed.Length > maxLength)
            throw new ArgumentException($"{fieldName} não pode ter mais de {maxLength} caracteres", fieldName);

        return trimmed;
    }

    private static string ValidateZipCode(string zipCode)
    {
        if (string.IsNullOrWhiteSpace(zipCode))
            throw new ArgumentException("CEP é obrigatório", nameof(zipCode));

        // Remove caracteres especiais
        var cleanZipCode = new string(zipCode.Where(char.IsDigit).ToArray());

        if (cleanZipCode.Length != 8)
            throw new ArgumentException("CEP deve ter 8 dígitos", nameof(zipCode));

        return cleanZipCode;
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
    /// Formata o CEP para exibição (99999-999)
    /// </summary>
    public string FormatZipCode()
    {
        return ZipCode.Length == 8 
            ? $"{ZipCode[..5]}-{ZipCode[5..]}" 
            : ZipCode;
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

    public override string ToString() => GetSingleLineAddress();
}
