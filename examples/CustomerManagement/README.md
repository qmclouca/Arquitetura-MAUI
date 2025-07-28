# CustomerManagement - Exemplo Completo

## 📋 Visão Geral

Este exemplo demonstra uma implementação completa de um sistema de gerenciamento de clientes usando:

- ✅ **MAUI Desktop** - Interface nativa para Windows
- ✅ **MVVM Pattern** - Separação clara de responsabilidades
- ✅ **Domain-Driven Design** - Modelagem rica de domínio
- ✅ **Clean Architecture** - Camadas bem definidas
- ✅ **Entity Framework Core** - Persistência de dados
- ✅ **AutoMapper** - Mapeamento entre objetos
- ✅ **Serilog** - Logging estruturado
- ✅ **Dependency Injection** - Inversão de controle
- ✅ **CommunityToolkit.Mvvm** - Recursos avançados de MVVM

## 🏗️ Arquitetura Implementada

```
📁 CustomerManagement/
├── 📁 Domain/                     # Camada de Domínio
│   ├── 📁 Entities/              # Entidades de negócio
│   │   └── Customer.cs           # Agregado raiz Customer
│   ├── 📁 ValueObjects/          # Objetos de valor
│   │   └── ValueObjects.cs       # Email, Phone, Address, etc.
│   ├── 📁 Events/                # Eventos de domínio
│   │   └── CustomerEvents.cs     # Eventos do Customer
│   └── 📁 Repositories/          # Interfaces de repositório
│       └── ICustomerRepository.cs
├── 📁 Application/               # Camada de Aplicação
│   ├── 📁 DTOs/                  # Data Transfer Objects
│   │   └── CustomerDTOs.cs       # DTOs para Customer
│   ├── 📁 Services/              # Serviços de aplicação
│   │   └── CustomerService.cs    # Orquestração de operações
│   └── 📁 ViewModels/            # ViewModels MVVM
│       └── CustomerViewModels.cs # ViewModels para UI
├── 📁 Infrastructure/            # Camada de Infraestrutura
│   └── 📁 Repositories/          # Implementações de repositório
│       └── CustomerRepository.cs # Acesso a dados EF Core
└── 📁 Presentation/              # Camada de Apresentação
    └── 📁 Views/                 # Views MAUI
        └── CustomerViews.xaml    # Interfaces de usuário
```

## 🎯 Funcionalidades Implementadas

### 1. **Gestão de Clientes**
- ✅ Listagem paginada com busca
- ✅ Criação de novos clientes
- ✅ Edição de dados existentes
- ✅ Exclusão lógica (soft delete)
- ✅ Ativação/Inativação

### 2. **Validações Robustas**
- ✅ Validação de email
- ✅ Validação de telefone
- ✅ Validação de CEP brasileiro
- ✅ Validação de campos obrigatórios
- ✅ Validação de tamanho de campos

### 3. **Tipos de Cliente**
- ✅ Standard (Padrão)
- ✅ Premium
- ✅ Corporate (Corporativo)

### 4. **Endereçamento Completo**
- ✅ Rua, número, complemento
- ✅ Bairro, cidade, estado
- ✅ CEP com formatação brasileira
- ✅ País (padrão: Brasil)

## 🔧 Padrões de Design Implementados

### **Domain-Driven Design (DDD)**
```csharp
// Entidade rica com comportamentos
public class Customer : IAggregateRoot
{
    // Value Objects tipados
    public CustomerId Id { get; private set; }
    public Email Email { get; private set; }
    public PhoneNumber Phone { get; private set; }
    public Address Address { get; private set; }
    
    // Factory method
    public static Customer Create(...)
    
    // Comportamentos de domínio
    public void UpdateBasicInfo(...)
    public void ChangeType(...)
    public void Activate()
    public void MarkAsDeleted()
}
```

### **MVVM com CommunityToolkit**
```csharp
public partial class CustomerListViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<CustomerDto> customers = new();
    
    [ObservableProperty]
    private bool isLoading;
    
    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        // Implementação...
    }
}
```

### **Repository Pattern**
```csharp
public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(CustomerId id);
    Task<(IEnumerable<Customer>, int)> GetPagedAsync(...);
    Task AddAsync(Customer customer);
    void Update(Customer customer);
    Task SaveChangesAsync();
}
```

### **Value Objects**
```csharp
public readonly record struct Email
{
    public string Value { get; }
    
    private Email(string value)
    {
        // Validação no construtor
        if (!EmailRegex.IsMatch(value))
            throw new ArgumentException("Email inválido");
        Value = value.ToLowerInvariant();
    }
    
    public static Email Create(string value) => new(value);
}
```

## 📊 Eventos de Domínio

```csharp
// Eventos disparados automaticamente
public record CustomerCreatedEvent(CustomerId CustomerId, string Email) : DomainEvent;
public record CustomerUpdatedEvent(CustomerId CustomerId, string Email) : DomainEvent;
public record CustomerTypeChangedEvent(CustomerId CustomerId, CustomerType OldType, CustomerType NewType) : DomainEvent;
```

## 🎨 Recursos de UX

### **Validação em Tempo Real**
- Feedback imediato ao usuário
- Mensagens de erro contextuais
- Campos obrigatórios destacados

### **Estados da Interface**
- Loading states
- Error states
- Empty states
- Success feedback

### **Paginação Inteligente**
- Navegação por páginas
- Informações de contagem
- Busca com filtros

## 🧪 Testabilidade

### **Separação de Responsabilidades**
- Lógica de negócio no domínio
- Orquestração na aplicação
- Acesso a dados na infraestrutura
- Interface na apresentação

### **Injeção de Dependências**
- Interfaces bem definidas
- Fácil criação de mocks
- Testes unitários isolados

### **Logging Estruturado**
```csharp
_logger.LogInformation("Criando novo cliente - Email: {Email}", createDto.Email);
_logger.LogError(ex, "Erro ao salvar cliente - ID: {CustomerId}", id);
```

## 🚀 Como Usar Este Exemplo

### 1. **Estude a Estrutura**
- Analise como as camadas se comunicam
- Observe a separação de responsabilidades
- Veja como os padrões são aplicados

### 2. **Execute o Código**
- Configure o banco de dados
- Execute as migrations
- Teste as funcionalidades

### 3. **Experimente Modificações**
- Adicione novos campos
- Implemente novas validações
- Crie novos comportamentos

### 4. **Aplique em Seus Projetos**
- Use como base para outros agregados
- Adapte os padrões às suas necessidades
- Mantenha a consistência arquitetural

## 📈 Próximos Passos

1. **Implementar testes unitários**
2. **Adicionar cache Redis**
3. **Implementar auditoria**
4. **Adicionar notificações**
5. **Criar relatórios**
6. **Implementar importação/exportação**

---

Este exemplo serve como **template de referência** para implementação de funcionalidades seguindo as melhores práticas de arquitetura de software em aplicações .NET MAUI Desktop.
