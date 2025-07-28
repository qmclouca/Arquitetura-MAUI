# 3. Padrão MVVM

## Introdução ao MVVM

O padrão MVVM (Model-View-ViewModel) é fundamental para aplicações MAUI, proporcionando separação clara entre lógica de apresentação e interface do usuário, facilitando testes e manutenção.

## Componentes do MVVM

### 3.1 Model
Representa os dados e a lógica de negócio da aplicação.

```csharp
// Domain Entity (Model)
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
    
    public bool IsValid()
    {
        return !string.IsNullOrEmpty(Name) && 
               !string.IsNullOrEmpty(Email);
    }
}

// DTO (Model for View)
public class CustomerDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public string FormattedCreatedAt { get; set; }
}
```

### 3.2 View
Interface do usuário implementada em XAML.

```xml
<!-- CustomerListView.xaml -->
<ContentPage x:Class="App.Views.CustomerListView"
             xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:App.ViewModels">
    
    <ContentPage.BindingContext>
        <vm:CustomerListViewModel />
    </ContentPage.BindingContext>
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto" />
            <RowDefinition Height="*" />
            <RowDefinition Height="Auto" />
        </Grid.RowDefinitions>
        
        <!-- Header -->
        <Label Text="Customers" 
               Style="{StaticResource HeaderStyle}"
               Grid.Row="0" />
        
        <!-- List -->
        <CollectionView ItemsSource="{Binding Customers}"
                       SelectedItem="{Binding SelectedCustomer}"
                       Grid.Row="1">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <Grid Padding="15">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*" />
                            <ColumnDefinition Width="Auto" />
                        </Grid.ColumnDefinitions>
                        
                        <StackLayout Grid.Column="0">
                            <Label Text="{Binding Name}" 
                                   Style="{StaticResource TitleStyle}" />
                            <Label Text="{Binding Email}" 
                                   Style="{StaticResource SubtitleStyle}" />
                        </StackLayout>
                        
                        <Button Text="Edit"
                                Command="{Binding Source={RelativeSource AncestorType={x:Type vm:CustomerListViewModel}}, Path=EditCustomerCommand}"
                                CommandParameter="{Binding .}"
                                Grid.Column="1" />
                    </Grid>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
        
        <!-- Actions -->
        <Button Text="Add Customer"
                Command="{Binding AddCustomerCommand}"
                Style="{StaticResource PrimaryButtonStyle}"
                Grid.Row="2" />
    </Grid>
</ContentPage>
```

### 3.3 ViewModel
Conecta Model e View, contendo a lógica de apresentação.

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

public partial class CustomerListViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly INavigationService _navigationService;
    private readonly ILogger<CustomerListViewModel> _logger;
    
    public CustomerListViewModel(
        ICustomerService customerService,
        INavigationService navigationService,
        ILogger<CustomerListViewModel> logger)
    {
        _customerService = customerService;
        _navigationService = navigationService;
        _logger = logger;
        
        Customers = new ObservableCollection<CustomerDto>();
        LoadCustomersCommand = new AsyncRelayCommand(LoadCustomersAsync);
        AddCustomerCommand = new AsyncRelayCommand(AddCustomerAsync);
        EditCustomerCommand = new AsyncRelayCommand<CustomerDto>(EditCustomerAsync);
    }
    
    // Observable Properties
    public ObservableCollection<CustomerDto> Customers { get; }
    
    [ObservableProperty]
    private CustomerDto? selectedCustomer;
    
    [ObservableProperty]
    private bool isLoading;
    
    [ObservableProperty]
    private string searchText = string.Empty;
    
    // Commands
    public IAsyncRelayCommand LoadCustomersCommand { get; }
    public IAsyncRelayCommand AddCustomerCommand { get; }
    public IAsyncRelayCommand<CustomerDto> EditCustomerCommand { get; }
    
    // Command Implementations
    private async Task LoadCustomersAsync()
    {
        try
        {
            IsLoading = true;
            _logger.LogInformation("Loading customers");
            
            var customers = await _customerService.GetAllCustomersAsync();
            
            Customers.Clear();
            foreach (var customer in customers)
            {
                Customers.Add(customer);
            }
            
            _logger.LogInformation("Loaded {Count} customers", customers.Count());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading customers");
            // Handle error (show message to user)
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    private async Task AddCustomerAsync()
    {
        await _navigationService.NavigateToAsync("CustomerDetail");
    }
    
    private async Task EditCustomerAsync(CustomerDto? customer)
    {
        if (customer != null)
        {
            await _navigationService.NavigateToAsync("CustomerDetail", customer.Id);
        }
    }
    
    // Property Change Handlers
    partial void OnSearchTextChanged(string value)
    {
        FilterCustomers(value);
    }
    
    private void FilterCustomers(string searchText)
    {
        // Implement filtering logic
        if (string.IsNullOrWhiteSpace(searchText))
        {
            // Show all customers
            return;
        }
        
        // Filter customers based on search text
        var filteredCustomers = Customers
            .Where(c => c.Name.Contains(searchText, StringComparison.OrdinalIgnoreCase) ||
                       c.Email.Contains(searchText, StringComparison.OrdinalIgnoreCase))
            .ToList();
        
        // Update collection
    }
}
```

## CommunityToolkit.Mvvm Features

### Source Generators
Reduz código boilerplate usando source generators:

```csharp
public partial class CustomerDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private string name = string.Empty;
    
    [ObservableProperty]
    private string email = string.Empty;
    
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool isDirty;
    
    [RelayCommand(CanExecute = nameof(CanSave))]
    private async Task SaveAsync()
    {
        // Save logic
    }
    
    private bool CanSave() => IsDirty && IsValid();
}
```

### Validation
Integração com validação:

```csharp
public partial class CustomerDetailViewModel : ObservableValidator
{
    [ObservableProperty]
    [Required]
    [MinLength(2)]
    [NotifyDataErrorInfo]
    private string name = string.Empty;
    
    [ObservableProperty]
    [Required]
    [EmailAddress]
    [NotifyDataErrorInfo]
    private string email = string.Empty;
    
    partial void OnNameChanged(string value)
    {
        ValidateProperty(value, nameof(Name));
    }
    
    partial void OnEmailChanged(string value)
    {
        ValidateProperty(value, nameof(Email));
    }
}
```

## Data Binding

### Two-Way Binding
```xml
<Entry Text="{Binding Name, Mode=TwoWay}"
       Placeholder="Enter customer name" />
```

### Command Binding
```xml
<Button Text="Save"
        Command="{Binding SaveCommand}"
        IsEnabled="{Binding CanSave}" />
```

### Collection Binding
```xml
<CollectionView ItemsSource="{Binding Customers}"
               SelectedItem="{Binding SelectedCustomer}">
    <CollectionView.ItemTemplate>
        <DataTemplate>
            <ViewCell>
                <Grid>
                    <Label Text="{Binding Name}" />
                    <Label Text="{Binding Email}" />
                </Grid>
            </ViewCell>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

## Navegação

### Navigation Service
```csharp
public interface INavigationService
{
    Task NavigateToAsync(string route);
    Task NavigateToAsync(string route, object parameter);
    Task NavigateBackAsync();
    Task<T> ShowPopupAsync<T>(IPopup popup);
}

public class NavigationService : INavigationService
{
    public async Task NavigateToAsync(string route)
    {
        await Shell.Current.GoToAsync(route);
    }
    
    public async Task NavigateToAsync(string route, object parameter)
    {
        var parameters = new Dictionary<string, object>
        {
            ["Parameter"] = parameter
        };
        
        await Shell.Current.GoToAsync(route, parameters);
    }
    
    public async Task NavigateBackAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
```

## Testes de ViewModel

```csharp
[Test]
public async Task LoadCustomers_ShouldPopulateCollection()
{
    // Arrange
    var mockService = new Mock<ICustomerService>();
    var customers = new List<CustomerDto>
    {
        new() { Id = 1, Name = "John Doe", Email = "john@example.com" },
        new() { Id = 2, Name = "Jane Smith", Email = "jane@example.com" }
    };
    
    mockService.Setup(s => s.GetAllCustomersAsync())
           .ReturnsAsync(customers);
    
    var viewModel = new CustomerListViewModel(
        mockService.Object,
        Mock.Of<INavigationService>(),
        Mock.Of<ILogger<CustomerListViewModel>>()
    );
    
    // Act
    await viewModel.LoadCustomersCommand.ExecuteAsync(null);
    
    // Assert
    Assert.AreEqual(2, viewModel.Customers.Count);
    Assert.AreEqual("John Doe", viewModel.Customers[0].Name);
}
```

## Melhores Práticas

1. **Use Source Generators**: Reduza código boilerplate
2. **Valide Dados**: Implemente validação no ViewModel
3. **Gerencie Estado**: Use propriedades observáveis
4. **Teste ViewModels**: Facilite testes unitários
5. **Separe Responsabilidades**: Mantenha ViewModels focados
6. **Use Dependency Injection**: Facilite mocking em testes
7. **Implemente INotifyPropertyChanged**: Para binding automático
8. **Use Commands**: Para ações do usuário

## Próximos Tópicos

- [Domain-Driven Design](./04-domain-driven-design.md)
- [Design System](./05-design-system.md)
- [Logging com Serilog](./06-logging-serilog.md)
