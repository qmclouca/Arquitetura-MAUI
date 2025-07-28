using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CustomerManagement.Application.DTOs;
using CustomerManagement.Application.Services;
using Microsoft.Extensions.Logging;

namespace CustomerManagement.Application.ViewModels;

/// <summary>
/// ViewModel principal para listagem de clientes
/// Implementa MVVM com CommunityToolkit.Mvvm
/// </summary>
public partial class CustomerListViewModel : ObservableObject
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerListViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<CustomerDto> customers = new();

    [ObservableProperty]
    private CustomerDto? selectedCustomer;

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private int totalRecords;

    [ObservableProperty]
    private int currentPage = 1;

    [ObservableProperty]
    private int pageSize = 20;

    public CustomerListViewModel(
        ICustomerService customerService,
        ILogger<CustomerListViewModel> logger)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Comando para carregar a lista de clientes
    /// </summary>
    [RelayCommand]
    private async Task LoadCustomersAsync()
    {
        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            _logger.LogInformation("Carregando lista de clientes. Página: {Page}, Tamanho: {PageSize}, Filtro: {SearchText}", 
                CurrentPage, PageSize, SearchText);

            var result = await _customerService.GetCustomersAsync(
                searchText: SearchText,
                page: CurrentPage,
                pageSize: PageSize);

            Customers.Clear();
            foreach (var customer in result.Items)
            {
                Customers.Add(customer);
            }

            TotalRecords = result.TotalCount;

            _logger.LogInformation("Lista de clientes carregada com sucesso. Total: {Total}", TotalRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar lista de clientes");
            HasError = true;
            ErrorMessage = "Erro ao carregar a lista de clientes. Tente novamente.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Comando para buscar clientes
    /// </summary>
    [RelayCommand]
    private async Task SearchAsync()
    {
        CurrentPage = 1; // Reset para primeira página
        await LoadCustomersAsync();
    }

    /// <summary>
    /// Comando para limpar a busca
    /// </summary>
    [RelayCommand]
    private async Task ClearSearchAsync()
    {
        SearchText = string.Empty;
        CurrentPage = 1;
        await LoadCustomersAsync();
    }

    /// <summary>
    /// Comando para ir para a próxima página
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoToNextPage))]
    private async Task NextPageAsync()
    {
        CurrentPage++;
        await LoadCustomersAsync();
    }

    /// <summary>
    /// Comando para ir para a página anterior
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGoToPreviousPage))]
    private async Task PreviousPageAsync()
    {
        CurrentPage--;
        await LoadCustomersAsync();
    }

    /// <summary>
    /// Comando para excluir cliente
    /// </summary>
    [RelayCommand]
    private async Task DeleteCustomerAsync(CustomerDto customer)
    {
        if (customer == null) return;

        try
        {
            IsLoading = true;
            _logger.LogInformation("Excluindo cliente {CustomerId}", customer.Id);

            await _customerService.DeleteCustomerAsync(customer.Id);
            
            Customers.Remove(customer);
            TotalRecords--;

            _logger.LogInformation("Cliente {CustomerId} excluído com sucesso", customer.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir cliente {CustomerId}", customer.Id);
            HasError = true;
            ErrorMessage = "Erro ao excluir o cliente. Tente novamente.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Comando para recarregar a lista
    /// </summary>
    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadCustomersAsync();
    }

    private bool CanGoToNextPage()
    {
        var totalPages = (int)Math.Ceiling((double)TotalRecords / PageSize);
        return CurrentPage < totalPages;
    }

    private bool CanGoToPreviousPage()
    {
        return CurrentPage > 1;
    }

    // Properties calculadas
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
    
    public bool HasCustomers => Customers.Count > 0;
    
    public bool ShowPagination => TotalRecords > PageSize;
}

/// <summary>
/// ViewModel para criação/edição de cliente
/// Implementa validação com Data Annotations
/// </summary>
public partial class CustomerFormViewModel : ObservableValidator
{
    private readonly ICustomerService _customerService;
    private readonly ILogger<CustomerFormViewModel> _logger;

    [ObservableProperty]
    [Required(ErrorMessage = "Nome é obrigatório")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Nome deve ter entre 2 e 50 caracteres")]
    private string firstName = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Sobrenome é obrigatório")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Sobrenome deve ter entre 2 e 50 caracteres")]
    private string lastName = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Formato de email inválido")]
    private string email = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Telefone é obrigatório")]
    [Phone(ErrorMessage = "Formato de telefone inválido")]
    private string phone = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Rua é obrigatória")]
    private string street = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Número é obrigatório")]
    private string number = string.Empty;

    [ObservableProperty]
    private string complement = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Bairro é obrigatório")]
    private string neighborhood = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Cidade é obrigatória")]
    private string city = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "Estado é obrigatório")]
    private string state = string.Empty;

    [ObservableProperty]
    [Required(ErrorMessage = "CEP é obrigatório")]
    [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "CEP deve estar no formato 99999-999")]
    private string zipCode = string.Empty;

    [ObservableProperty]
    private string country = "Brasil";

    [ObservableProperty]
    private CustomerType customerType = CustomerType.Standard;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private bool hasError;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isEditMode;

    private Guid? _customerId;

    public CustomerFormViewModel(
        ICustomerService customerService,
        ILogger<CustomerFormViewModel> logger)
    {
        _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Inicializa o form para edição de um cliente existente
    /// </summary>
    public async Task InitializeForEditAsync(Guid customerId)
    {
        try
        {
            IsLoading = true;
            IsEditMode = true;
            _customerId = customerId;

            _logger.LogInformation("Carregando dados do cliente {CustomerId} para edição", customerId);

            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (customer != null)
            {
                FirstName = customer.FirstName;
                LastName = customer.LastName;
                Email = customer.Email;
                Phone = customer.Phone;
                Street = customer.Address.Street;
                Number = customer.Address.Number;
                Complement = customer.Address.Complement ?? string.Empty;
                Neighborhood = customer.Address.Neighborhood;
                City = customer.Address.City;
                State = customer.Address.State;
                ZipCode = customer.Address.ZipCode;
                Country = customer.Address.Country;
                CustomerType = customer.Type;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar dados do cliente {CustomerId}", customerId);
            HasError = true;
            ErrorMessage = "Erro ao carregar os dados do cliente.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Inicializa o form para criação de novo cliente
    /// </summary>
    public void InitializeForCreate()
    {
        IsEditMode = false;
        _customerId = null;
        ClearForm();
    }

    /// <summary>
    /// Comando para salvar o cliente
    /// </summary>
    [RelayCommand]
    private async Task SaveAsync()
    {
        ValidateAllProperties();
        
        if (HasErrors)
        {
            ErrorMessage = "Corrija os campos obrigatórios antes de salvar.";
            HasError = true;
            return;
        }

        try
        {
            IsLoading = true;
            HasError = false;
            ErrorMessage = string.Empty;

            var addressDto = new AddressDto(
                Street, Number, Neighborhood, City, State, ZipCode, Country, 
                string.IsNullOrWhiteSpace(Complement) ? null : Complement);

            if (IsEditMode && _customerId.HasValue)
            {
                _logger.LogInformation("Atualizando cliente {CustomerId}", _customerId.Value);
                
                await _customerService.UpdateCustomerAsync(_customerId.Value, new UpdateCustomerDto
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Phone = Phone,
                    Address = addressDto,
                    Type = CustomerType
                });

                _logger.LogInformation("Cliente {CustomerId} atualizado com sucesso", _customerId.Value);
            }
            else
            {
                _logger.LogInformation("Criando novo cliente");
                
                await _customerService.CreateCustomerAsync(new CreateCustomerDto
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    Phone = Phone,
                    Address = addressDto,
                    Type = CustomerType
                });

                _logger.LogInformation("Novo cliente criado com sucesso");
            }

            // Limpar form após salvar
            if (!IsEditMode)
            {
                ClearForm();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao salvar cliente");
            HasError = true;
            ErrorMessage = "Erro ao salvar o cliente. Verifique os dados e tente novamente.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    /// <summary>
    /// Comando para cancelar a operação
    /// </summary>
    [RelayCommand]
    private void Cancel()
    {
        ClearForm();
    }

    /// <summary>
    /// Limpa todos os campos do formulário
    /// </summary>
    private void ClearForm()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
        Phone = string.Empty;
        Street = string.Empty;
        Number = string.Empty;
        Complement = string.Empty;
        Neighborhood = string.Empty;
        City = string.Empty;
        State = string.Empty;
        ZipCode = string.Empty;
        Country = "Brasil";
        CustomerType = CustomerType.Standard;
        HasError = false;
        ErrorMessage = string.Empty;
        ClearErrors();
    }

    // Properties calculadas
    public string FormTitle => IsEditMode ? "Editar Cliente" : "Novo Cliente";
    public string SaveButtonText => IsEditMode ? "Atualizar" : "Salvar";
    public bool CanSave => !IsLoading && !HasErrors;
}
