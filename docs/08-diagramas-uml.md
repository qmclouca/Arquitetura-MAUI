# 8. Diagramas UML

## Introdução aos Diagramas UML

Este documento apresenta os diagramas UML que ilustram a arquitetura e design da aplicação MAUI Desktop com MVVM e DDD. Os diagramas são criados usando PlantUML, Mermaid e Draw.io.

## Diagrama de Arquitetura Geral

### 8.1 Visão Geral da Arquitetura

```mermaid
graph TB
    subgraph "Presentation Layer"
        V[Views/XAML]
        VM[ViewModels]
        C[Converters]
        B[Behaviors]
    end
    
    subgraph "Application Layer"
        AS[Application Services]
        DTO[DTOs]
        VAL[Validators]
        MAP[Mappers]
    end
    
    subgraph "Domain Layer"
        E[Entities]
        VO[Value Objects]
        DS[Domain Services]
        R[Repository Interfaces]
        DE[Domain Events]
    end
    
    subgraph "Infrastructure Layer"
        RI[Repository Implementations]
        DB[Database Context]
        ES[External Services]
        LOG[Logging]
        CONF[Configuration]
    end
    
    V --> VM
    VM --> AS
    AS --> DS
    AS --> R
    R --> RI
    RI --> DB
    AS --> ES
    
    style V fill:#e1f5fe
    style VM fill:#e1f5fe
    style AS fill:#e8f5e8
    style E fill:#fff3e0
    style VO fill:#fff3e0
    style RI fill:#fce4ec
    style DB fill:#fce4ec
```

### 8.2 Diagrama de Camadas (PlantUML)

```plantuml
@startuml Architecture_Layers
!define RECTANGLE class

package "Presentation Layer" {
    RECTANGLE Views {
        + MainWindow.xaml
        + CustomerListView.xaml
        + CustomerDetailView.xaml
    }
    
    RECTANGLE ViewModels {
        + MainViewModel
        + CustomerListViewModel
        + CustomerDetailViewModel
    }
    
    RECTANGLE Controls {
        + CustomButton
        + LoadingIndicator
        + Alert
    }
}

package "Application Layer" {
    RECTANGLE Services {
        + CustomerService
        + OrderService
        + PaymentService
    }
    
    RECTANGLE DTOs {
        + CustomerDto
        + OrderDto
        + CreateCustomerDto
    }
    
    RECTANGLE Validators {
        + CreateCustomerValidator
        + UpdateCustomerValidator
    }
}

package "Domain Layer" {
    RECTANGLE Entities {
        + Customer
        + Order
        + OrderItem
    }
    
    RECTANGLE ValueObjects {
        + Email
        + Money
        + Address
    }
    
    RECTANGLE DomainServices {
        + PricingService
        + DiscountService
    }
    
    RECTANGLE Repositories {
        + ICustomerRepository
        + IOrderRepository
    }
}

package "Infrastructure Layer" {
    RECTANGLE DataAccess {
        + ApplicationDbContext
        + CustomerRepository
        + OrderRepository
    }
    
    RECTANGLE ExternalServices {
        + EmailService
        + PaymentGateway
        + FileService
    }
    
    RECTANGLE Logging {
        + SerilogConfiguration
        + StructuredLogging
    }
}

Views --> ViewModels
ViewModels --> Services
Services --> Entities
Services --> Repositories
Repositories --> DataAccess
Services --> ExternalServices

@enduml
```

## Diagramas de Classes

### 8.3 Domain Model - Customer Aggregate

```plantuml
@startuml Customer_Domain_Model
!define ENTITY class
!define VALUEOBJECT class
!define SERVICE class

ENTITY Customer {
    - id: int
    - name: string
    - email: Email
    - createdAt: DateTime
    - status: CustomerStatus
    - orders: List<Order>
    + UpdateEmail(newEmail: Email): void
    + Deactivate(): void
    + CreateOrder(items: IEnumerable<OrderItem>): Order
}

VALUEOBJECT Email {
    - value: string
    + Email(value: string)
    + IsValid(): bool
    + ToString(): string
}

VALUEOBJECT Money {
    - amount: decimal
    - currency: Currency
    + Add(other: Money): Money
    + Multiply(factor: decimal): Money
    + ToString(): string
}

VALUEOBJECT Address {
    - street: string
    - city: string
    - zipCode: string
    - country: string
    + IsValid(): bool
}

ENTITY Order {
    - id: int
    - customerId: int
    - orderDate: DateTime
    - status: OrderStatus
    - items: List<OrderItem>
    - shippingAddress: Address
    + AddItem(productId: int, quantity: int, price: Money): void
    + RemoveItem(productId: int): void
    + Confirm(): void
    + Ship(): void
    + CalculateTotal(): Money
}

ENTITY OrderItem {
    - id: int
    - productId: int
    - quantity: int
    - unitPrice: Money
    + UpdateQuantity(newQuantity: int): void
    + GetTotalPrice(): Money
}

enum CustomerStatus {
    Active
    Inactive
    Suspended
}

enum OrderStatus {
    Pending
    Confirmed
    Shipped
    Delivered
    Cancelled
}

Customer ||--|| Email : contains
Customer ||--o{ Order : has
Order ||--|| Address : ships to
Order ||--o{ OrderItem : contains
OrderItem ||--|| Money : priced in
Customer ||--|| CustomerStatus : has
Order ||--|| OrderStatus : has

@enduml
```

### 8.4 MVVM Pattern

```plantuml
@startuml MVVM_Pattern
!define VIEW class
!define VIEWMODEL class
!define MODEL class
!define SERVICE class

VIEW CustomerListView {
    + CustomerListView.xaml
    + CustomerListView.xaml.cs
    - DataContext: CustomerListViewModel
}

VIEWMODEL CustomerListViewModel {
    - customerService: ICustomerService
    - navigationService: INavigationService
    + Customers: ObservableCollection<CustomerDto>
    + SelectedCustomer: CustomerDto
    + IsLoading: bool
    + SearchText: string
    + LoadCustomersCommand: IAsyncRelayCommand
    + AddCustomerCommand: IAsyncRelayCommand
    + EditCustomerCommand: IAsyncRelayCommand<CustomerDto>
    - LoadCustomersAsync(): Task
    - AddCustomerAsync(): Task
    - EditCustomerAsync(customer: CustomerDto): Task
}

MODEL CustomerDto {
    + Id: int
    + Name: string
    + Email: string
    + Status: string
    + CreatedAt: DateTime
}

SERVICE ICustomerService {
    + GetAllCustomersAsync(): Task<IEnumerable<CustomerDto>>
    + GetCustomerByIdAsync(id: int): Task<CustomerDto>
    + CreateCustomerAsync(request: CreateCustomerRequest): Task<CustomerDto>
    + UpdateCustomerAsync(id: int, request: UpdateCustomerRequest): Task<CustomerDto>
    + DeleteCustomerAsync(id: int): Task
}

CustomerListView --> CustomerListViewModel : DataBinding
CustomerListViewModel --> CustomerDto : Uses
CustomerListViewModel --> ICustomerService : Calls
CustomerListView ..> CustomerDto : Displays

note right of CustomerListView
    - Uses data binding for UI updates
    - Commands bound to user actions
    - No business logic in code-behind
end note

note right of CustomerListViewModel
    - Implements INotifyPropertyChanged
    - Uses CommunityToolkit.Mvvm
    - Handles UI state management
end note

@enduml
```

### 8.5 Repository Pattern

```plantuml
@startuml Repository_Pattern
!define INTERFACE interface
!define CLASS class

INTERFACE IRepository<T> {
    + GetByIdAsync(id: int): Task<T>
    + GetAllAsync(): Task<IEnumerable<T>>
    + AddAsync(entity: T): Task
    + UpdateAsync(entity: T): Task
    + DeleteAsync(entity: T): Task
    + SaveChangesAsync(): Task<int>
}

INTERFACE ICustomerRepository {
    + GetByEmailAsync(email: Email): Task<Customer>
    + GetActiveCustomersAsync(): Task<IEnumerable<Customer>>
    + SearchByNameAsync(name: string): Task<IEnumerable<Customer>>
}

CLASS CustomerRepository {
    - context: ApplicationDbContext
    - logger: ILogger<CustomerRepository>
    + CustomerRepository(context: ApplicationDbContext)
    + GetByIdAsync(id: int): Task<Customer>
    + GetByEmailAsync(email: Email): Task<Customer>
    + GetActiveCustomersAsync(): Task<IEnumerable<Customer>>
    + SearchByNameAsync(name: string): Task<IEnumerable<Customer>>
    + AddAsync(customer: Customer): Task
    + UpdateAsync(customer: Customer): Task
    + DeleteAsync(customer: Customer): Task
    + SaveChangesAsync(): Task<int>
}

CLASS ApplicationDbContext {
    + Customers: DbSet<Customer>
    + Orders: DbSet<Order>
    + OnModelCreating(builder: ModelBuilder): void
    + OnConfiguring(options: DbContextOptionsBuilder): void
}

ENTITY Customer {
    + Id: int
    + Name: string
    + Email: Email
    + Status: CustomerStatus
    + CreatedAt: DateTime
}

ICustomerRepository --|> IRepository : extends
CustomerRepository ..|> ICustomerRepository : implements
CustomerRepository --> ApplicationDbContext : uses
ApplicationDbContext --> Customer : maps

note right of IRepository
    Generic repository interface
    with common CRUD operations
end note

note right of CustomerRepository
    EF Core implementation
    with specific customer queries
end note

@enduml
```

## Diagramas de Sequência

### 8.6 Create Customer Use Case

```plantuml
@startuml Create_Customer_Sequence
actor User
participant "CustomerListView" as View
participant "CustomerListViewModel" as ViewModel
participant "ICustomerService" as Service
participant "ICustomerRepository" as Repository
participant "CustomerFactory" as Factory
participant "ApplicationDbContext" as DbContext
database "Database" as DB

User -> View : Click "Add Customer"
View -> ViewModel : AddCustomerCommand.Execute()
ViewModel -> View : Navigate to CustomerDetailView

User -> View : Enter customer data
User -> View : Click "Save"
View -> ViewModel : SaveCustomerCommand.Execute()

ViewModel -> Service : CreateCustomerAsync(request)
activate Service

Service -> Repository : GetByEmailAsync(email)
Repository -> DbContext : Customers.FirstOrDefault(...)
DbContext -> DB : SELECT * FROM Customers WHERE Email = ?
DB --> DbContext : Customer data or null
DbContext --> Repository : Customer or null
Repository --> Service : Customer or null

alt Email already exists
    Service --> ViewModel : Failure("Email already exists")
    ViewModel -> View : Show error message
else Email is unique
    Service -> Factory : CreateStandardCustomer(name, email)
    Factory --> Service : Customer instance
    
    Service -> Repository : AddAsync(customer)
    Repository -> DbContext : Customers.Add(customer)
    
    Service -> Repository : SaveChangesAsync()
    Repository -> DbContext : SaveChangesAsync()
    DbContext -> DB : INSERT INTO Customers (...)
    DB --> DbContext : Success
    DbContext --> Repository : Customer with ID
    Repository --> Service : Customer with ID
    
    Service --> ViewModel : Success(CustomerDto)
    deactivate Service
    
    ViewModel -> View : Navigate back to list
    ViewModel -> ViewModel : Refresh customer list
end

@enduml
```

### 8.7 Order Processing Sequence

```plantuml
@startuml Order_Processing_Sequence
participant "OrderViewModel" as VM
participant "OrderProcessingFacade" as Facade
participant "OrderService" as OrderSvc
participant "InventoryService" as InventorySvc
participant "PaymentService" as PaymentSvc
participant "ShippingService" as ShippingSvc
participant "NotificationService" as NotificationSvc

VM -> Facade : ProcessOrderAsync(request)
activate Facade

Facade -> OrderSvc : GetOrderAsync(orderId)
OrderSvc --> Facade : Order

Facade -> InventorySvc : CheckAvailabilityAsync(items)
InventorySvc --> Facade : InventoryResult

alt Inventory not available
    Facade --> VM : Failed("Insufficient inventory")
else Inventory available
    Facade -> InventorySvc : ReserveItemsAsync(items)
    InventorySvc --> Facade : Success
    
    Facade -> PaymentSvc : ProcessPaymentAsync(paymentRequest)
    PaymentSvc --> Facade : PaymentResult
    
    alt Payment failed
        Facade -> InventorySvc : ReleaseReservedItemsAsync(items)
        Facade --> VM : Failed("Payment failed")
    else Payment successful
        Facade -> OrderSvc : ConfirmOrderAsync(orderId)
        OrderSvc --> Facade : Success
        
        Facade -> InventorySvc : UpdateInventoryAsync(items)
        InventorySvc --> Facade : Success
        
        Facade -> ShippingSvc : ScheduleShippingAsync(shippingRequest)
        ShippingSvc --> Facade : ShippingResult
        
        Facade -> NotificationSvc : SendOrderConfirmationAsync(customerId, orderId)
        NotificationSvc --> Facade : Success
        
        Facade --> VM : Success(ProcessingResult)
    end
end

deactivate Facade

@enduml
```

## Diagramas de Atividade

### 8.8 Customer Registration Flow

```mermaid
flowchart TD
    A[Start Registration] --> B[Enter Customer Data]
    B --> C{Validate Data}
    C -->|Invalid| D[Show Validation Errors]
    D --> B
    C -->|Valid| E[Check Email Uniqueness]
    E --> F{Email Exists?}
    F -->|Yes| G[Show Error: Email Already Exists]
    G --> B
    F -->|No| H[Create Customer Entity]
    H --> I[Save to Database]
    I --> J{Save Successful?}
    J -->|No| K[Show Error Message]
    K --> B
    J -->|Yes| L[Send Welcome Email]
    L --> M[Show Success Message]
    M --> N[Navigate to Customer List]
    N --> O[End]
    
    style A fill:#e1f5fe
    style O fill:#e8f5e8
    style G fill:#ffebee
    style K fill:#ffebee
```

### 8.9 Order Processing Workflow

```mermaid
flowchart TD
    A[Order Submitted] --> B[Validate Order]
    B --> C{Order Valid?}
    C -->|No| D[Return Validation Errors]
    C -->|Yes| E[Check Inventory]
    E --> F{Items Available?}
    F -->|No| G[Return Inventory Error]
    F -->|Yes| H[Reserve Inventory]
    H --> I[Process Payment]
    I --> J{Payment Successful?}
    J -->|No| K[Release Reserved Items]
    K --> L[Return Payment Error]
    J -->|Yes| M[Confirm Order]
    M --> N[Update Inventory]
    N --> O[Schedule Shipping]
    O --> P[Send Confirmation Email]
    P --> Q[Return Success]
    
    style A fill:#e1f5fe
    style Q fill:#e8f5e8
    style D fill:#ffebee
    style G fill:#ffebee
    style L fill:#ffebee
```

## Diagramas de Componentes

### 8.10 Application Components

```plantuml
@startuml Application_Components
!define COMPONENT component

package "MAUI Application" {
    COMPONENT [Presentation Layer] as PL {
        [Views]
        [ViewModels] 
        [Converters]
        [Behaviors]
    }
    
    COMPONENT [Application Layer] as AL {
        [Application Services]
        [DTOs]
        [Validators]
        [Mappers]
    }
    
    COMPONENT [Domain Layer] as DL {
        [Entities]
        [Value Objects]
        [Domain Services]
        [Repository Interfaces]
        [Domain Events]
    }
    
    COMPONENT [Infrastructure Layer] as IL {
        [Data Access]
        [External Services]
        [Logging]
        [Configuration]
    }
}

COMPONENT [Database] as DB
COMPONENT [External APIs] as API
COMPONENT [File System] as FS
COMPONENT [Email Service] as EMAIL

PL --> AL : Uses
AL --> DL : Uses
AL --> IL : Uses
IL --> DL : Implements
IL --> DB : Connects
IL --> API : Calls
IL --> FS : Reads/Writes
IL --> EMAIL : Sends

note right of PL
    - XAML Views
    - ViewModels with MVVM
    - Data Binding
    - Commands
end note

note right of AL
    - Use Cases
    - Application Services
    - Data Transfer Objects
    - Validation
end note

note right of DL
    - Business Logic
    - Domain Rules
    - Aggregates
    - Domain Events
end note

note right of IL
    - Data Persistence
    - External Integrations
    - Cross-cutting Concerns
end note

@enduml
```

### 8.11 MVVM Component Interaction

```mermaid
graph LR
    subgraph "View Layer"
        V[XAML Views]
        UC[User Controls]
        C[Converters]
        B[Behaviors]
    end
    
    subgraph "ViewModel Layer"
        VM[ViewModels]
        CMD[Commands]
        OC[Observable Collections]
        P[Properties]
    end
    
    subgraph "Model Layer"
        DTO[Data Transfer Objects]
        S[Services]
        R[Repositories]
    end
    
    subgraph "Infrastructure"
        DB[(Database)]
        API[External APIs]
        FS[File System]
    end
    
    V --> VM
    UC --> VM
    C --> VM
    B --> VM
    
    VM --> DTO
    VM --> S
    S --> R
    
    R --> DB
    S --> API
    S --> FS
    
    VM --> CMD
    VM --> OC
    VM --> P
    
    style V fill:#e1f5fe
    style VM fill:#e8f5e8
    style S fill:#fff3e0
    style DB fill:#fce4ec
```

## Diagramas de Estado

### 8.12 Order State Machine

```mermaid
stateDiagram-v2
    [*] --> Draft
    Draft --> Pending : Submit Order
    Pending --> Confirmed : Confirm Payment
    Pending --> Cancelled : Cancel Order
    Confirmed --> Shipped : Ship Order
    Confirmed --> Cancelled : Cancel Order
    Shipped --> Delivered : Deliver Order
    Shipped --> ReturnRequested : Request Return
    Delivered --> ReturnRequested : Request Return
    ReturnRequested --> Returned : Process Return
    Returned --> [*]
    Cancelled --> [*]
    Delivered --> [*]
    
    note right of Draft
        Order is being created
        Can add/remove items
    end note
    
    note right of Pending
        Waiting for payment
        confirmation
    end note
    
    note right of Confirmed
        Payment successful
        Ready to ship
    end note
```

### 8.13 Customer State Machine

```mermaid
stateDiagram-v2
    [*] --> Active
    Active --> Suspended : Suspend Account
    Active --> Inactive : Deactivate Account
    Suspended --> Active : Reactivate Account
    Suspended --> Inactive : Deactivate Account
    Inactive --> Active : Reactivate Account
    Inactive --> [*] : Delete Account
    
    note right of Active
        Customer can place orders
        Full access to system
    end note
    
    note right of Suspended
        Temporary restriction
        Cannot place new orders
    end note
    
    note right of Inactive
        Account disabled
        No system access
    end note
```

## Como Usar os Diagramas

### 8.14 Ferramentas Recomendadas

1. **PlantUML**: Para diagramas de classes, sequência e componentes
2. **Mermaid**: Para fluxogramas e diagramas de estado
3. **Draw.io**: Para diagramas personalizados e wireframes
4. **VS Code Extensions**: PlantUML, Mermaid Preview, Draw.io Integration

### 8.15 Geração Automática

```csharp
// Infrastructure/Documentation/DiagramGenerator.cs
public class DiagramGenerator
{
    public async Task GenerateClassDiagramAsync(Assembly assembly, string outputPath)
    {
        var types = assembly.GetTypes()
            .Where(t => t.Namespace?.Contains("Domain") == true)
            .ToList();

        var plantUml = new StringBuilder();
        plantUml.AppendLine("@startuml");
        
        foreach (var type in types)
        {
            if (type.IsClass)
            {
                plantUml.AppendLine($"class {type.Name} {{");
                
                // Add properties
                foreach (var prop in type.GetProperties())
                {
                    plantUml.AppendLine($"  + {prop.Name}: {prop.PropertyType.Name}");
                }
                
                // Add methods
                foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    var parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.Name}: {p.ParameterType.Name}"));
                    plantUml.AppendLine($"  + {method.Name}({parameters}): {method.ReturnType.Name}");
                }
                
                plantUml.AppendLine("}");
            }
        }
        
        plantUml.AppendLine("@enduml");
        
        await File.WriteAllTextAsync(outputPath, plantUml.ToString());
    }
}
```

## Próximos Tópicos

- [Estrutura do Projeto](./09-estrutura-projeto.md)
- [Configuração e Setup](./10-configuracao-setup.md)
