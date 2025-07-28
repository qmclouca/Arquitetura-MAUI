using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Windows;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using FluentAssertions;
using System.Drawing;

namespace CustomerManagement.Tests.UITests;

/// <summary>
/// Testes de UI para a aplicação CustomerManagement MAUI Desktop
/// Utiliza WinAppDriver e Appium para automação de testes
/// </summary>
[TestClass]
public class CustomerManagementUITests
{
    private WindowsDriver<WindowsElement>? _driver;
    private readonly TimeSpan _defaultTimeout = TimeSpan.FromSeconds(10);

    [TestInitialize]
    public void Setup()
    {
        // Configuração do WinAppDriver
        var appiumOptions = new AppiumOptions();
        
        // Path para o executável da aplicação MAUI
        appiumOptions.AddAdditionalCapability("app", @"f:\Rodolfo Bortoluzzi\OneDrive\SENAI\Arquitetura MAUI\examples\CustomerManagement\bin\Debug\net8.0-windows10.0.19041.0\CustomerManagement.exe");
        appiumOptions.AddAdditionalCapability("deviceName", "WindowsPC");
        appiumOptions.AddAdditionalCapability("platformName", "Windows");
        
        // Aguardar a aplicação inicializar
        appiumOptions.AddAdditionalCapability("ms:waitForAppLaunch", "25");
        
        // Criar driver
        _driver = new WindowsDriver<WindowsElement>(
            new Uri("http://127.0.0.1:4723"), appiumOptions);
        
        _driver.Manage().Timeouts().ImplicitWait = _defaultTimeout;
    }

    [TestCleanup]
    public void Cleanup()
    {
        _driver?.Quit();
        _driver?.Dispose();
    }

    #region Customer List Tests

    [TestMethod]
    public void CustomerList_WhenAppLaunches_ShouldDisplayMainWindow()
    {
        // Assert
        _driver.Should().NotBeNull();
        
        var mainWindow = _driver!.FindElementByAccessibilityId("MainWindow");
        mainWindow.Should().NotBeNull();
        mainWindow.Displayed.Should().BeTrue();
        
        var title = _driver.Title;
        title.Should().Contain("Customer Management");
    }

    [TestMethod]
    public void CustomerList_WhenLoaded_ShouldDisplayCustomerGrid()
    {
        // Arrange & Act
        var customerGrid = WaitForElement("CustomerDataGrid");
        
        // Assert
        customerGrid.Should().NotBeNull();
        customerGrid.Displayed.Should().BeTrue();
        customerGrid.Enabled.Should().BeTrue();
    }

    [TestMethod]
    public void CustomerList_WhenSearchTextEntered_ShouldFilterResults()
    {
        // Arrange
        var searchBox = WaitForElement("SearchTextBox");
        var customerGrid = WaitForElement("CustomerDataGrid");
        
        // Act
        searchBox.Clear();
        searchBox.SendKeys("João");
        
        // Aguardar filtro ser aplicado
        Thread.Sleep(1000);
        
        // Assert
        var rows = customerGrid.FindElementsByClassName("DataGridRow");
        rows.Should().NotBeEmpty();
        
        // Verificar se pelo menos uma linha contém "João"
        var hasJoaoRow = rows.Any(row => 
            row.Text.Contains("João", StringComparison.OrdinalIgnoreCase));
        hasJoaoRow.Should().BeTrue();
    }

    [TestMethod]
    public void CustomerList_WhenRefreshButtonClicked_ShouldReloadData()
    {
        // Arrange
        var refreshButton = WaitForElement("RefreshButton");
        var customerGrid = WaitForElement("CustomerDataGrid");
        
        // Act
        refreshButton.Click();
        
        // Aguardar reload
        Thread.Sleep(2000);
        
        // Assert
        customerGrid.Displayed.Should().BeTrue();
        
        // Verificar se existe indicador de loading ou dados foram atualizados
        var loadingIndicator = TryFindElement("LoadingIndicator");
        if (loadingIndicator != null)
        {
            // Aguardar loading terminar
            WaitForElementToDisappear("LoadingIndicator");
        }
    }

    #endregion

    #region Customer Creation Tests

    [TestMethod]
    public void CreateCustomer_WhenAddButtonClicked_ShouldOpenCreateDialog()
    {
        // Arrange
        var addButton = WaitForElement("AddCustomerButton");
        
        // Act
        addButton.Click();
        
        // Assert
        var createDialog = WaitForElement("CreateCustomerDialog");
        createDialog.Should().NotBeNull();
        createDialog.Displayed.Should().BeTrue();
        
        // Verificar campos obrigatórios
        var firstNameField = WaitForElement("FirstNameEntry");
        var lastNameField = WaitForElement("LastNameEntry");
        var emailField = WaitForElement("EmailEntry");
        
        firstNameField.Should().NotBeNull();
        lastNameField.Should().NotBeNull();
        emailField.Should().NotBeNull();
    }

    [TestMethod]
    public void CreateCustomer_WithValidData_ShouldCreateSuccessfully()
    {
        // Arrange
        OpenCreateCustomerDialog();
        
        var firstName = "Teste";
        var lastName = "Automação";
        var email = $"teste.automacao.{DateTime.Now.Ticks}@email.com";
        var phone = "11987654321";
        
        // Act
        FillCustomerForm(firstName, lastName, email, phone);
        
        var saveButton = WaitForElement("SaveButton");
        saveButton.Click();
        
        // Assert
        // Aguardar dialog fechar
        WaitForElementToDisappear("CreateCustomerDialog");
        
        // Verificar se cliente aparece na lista
        var customerGrid = WaitForElement("CustomerDataGrid");
        Thread.Sleep(2000); // Aguardar refresh da lista
        
        var rows = customerGrid.FindElementsByClassName("DataGridRow");
        var hasNewCustomer = rows.Any(row => 
            row.Text.Contains(firstName) && row.Text.Contains(lastName));
        hasNewCustomer.Should().BeTrue();
    }

    [TestMethod]
    public void CreateCustomer_WithEmptyFields_ShouldShowValidationErrors()
    {
        // Arrange
        OpenCreateCustomerDialog();
        
        // Act - Tentar salvar sem preencher campos
        var saveButton = WaitForElement("SaveButton");
        saveButton.Click();
        
        // Assert
        // Dialog deve permanecer aberto
        var createDialog = WaitForElement("CreateCustomerDialog");
        createDialog.Displayed.Should().BeTrue();
        
        // Verificar mensagens de erro
        var errorMessages = _driver!.FindElementsByClassName("ValidationError");
        errorMessages.Should().NotBeEmpty();
        
        var hasFirstNameError = errorMessages.Any(error => 
            error.Text.Contains("Nome", StringComparison.OrdinalIgnoreCase));
        hasFirstNameError.Should().BeTrue();
    }

    [TestMethod]
    public void CreateCustomer_WithInvalidEmail_ShouldShowEmailValidationError()
    {
        // Arrange
        OpenCreateCustomerDialog();
        
        // Act
        FillCustomerForm("João", "Silva", "email-invalido", "11999999999");
        
        var saveButton = WaitForElement("SaveButton");
        saveButton.Click();
        
        // Assert
        var emailError = WaitForElement("EmailValidationError");
        emailError.Should().NotBeNull();
        emailError.Text.Should().Contain("email", StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Customer Editing Tests

    [TestMethod]
    public void EditCustomer_WhenRowDoubleClicked_ShouldOpenEditDialog()
    {
        // Arrange
        var customerGrid = WaitForElement("CustomerDataGrid");
        var firstRow = customerGrid.FindElementByClassName("DataGridRow");
        
        // Act
        DoubleClick(firstRow);
        
        // Assert
        var editDialog = WaitForElement("EditCustomerDialog");
        editDialog.Should().NotBeNull();
        editDialog.Displayed.Should().BeTrue();
        
        // Verificar se campos estão preenchidos
        var firstNameField = WaitForElement("FirstNameEntry");
        firstNameField.Text.Should().NotBeEmpty();
    }

    [TestMethod]
    public void EditCustomer_WithValidChanges_ShouldUpdateSuccessfully()
    {
        // Arrange
        OpenEditCustomerDialog();
        
        var firstNameField = WaitForElement("FirstNameEntry");
        var originalName = firstNameField.Text;
        var newName = $"{originalName} Editado";
        
        // Act
        firstNameField.Clear();
        firstNameField.SendKeys(newName);
        
        var updateButton = WaitForElement("UpdateButton");
        updateButton.Click();
        
        // Assert
        WaitForElementToDisappear("EditCustomerDialog");
        
        // Verificar se mudança foi aplicada na lista
        Thread.Sleep(2000);
        var customerGrid = WaitForElement("CustomerDataGrid");
        var rows = customerGrid.FindElementsByClassName("DataGridRow");
        
        var hasUpdatedName = rows.Any(row => row.Text.Contains(newName));
        hasUpdatedName.Should().BeTrue();
    }

    #endregion

    #region Customer Deletion Tests

    [TestMethod]
    public void DeleteCustomer_WhenConfirmed_ShouldRemoveFromList()
    {
        // Arrange
        var customerGrid = WaitForElement("CustomerDataGrid");
        var firstRow = customerGrid.FindElementByClassName("DataGridRow");
        var customerName = ExtractCustomerNameFromRow(firstRow);
        
        // Selecionar linha
        firstRow.Click();
        
        var deleteButton = WaitForElement("DeleteCustomerButton");
        
        // Act
        deleteButton.Click();
        
        // Confirmar exclusão
        var confirmDialog = WaitForElement("ConfirmDeleteDialog");
        var confirmButton = WaitForElement("ConfirmButton");
        confirmButton.Click();
        
        // Assert
        WaitForElementToDisappear("ConfirmDeleteDialog");
        
        // Verificar se cliente foi removido
        Thread.Sleep(2000);
        customerGrid = WaitForElement("CustomerDataGrid");
        var rows = customerGrid.FindElementsByClassName("DataGridRow");
        
        var stillExists = rows.Any(row => row.Text.Contains(customerName));
        stillExists.Should().BeFalse();
    }

    [TestMethod]
    public void DeleteCustomer_WhenCancelled_ShouldKeepInList()
    {
        // Arrange
        var customerGrid = WaitForElement("CustomerDataGrid");
        var firstRow = customerGrid.FindElementByClassName("DataGridRow");
        var customerName = ExtractCustomerNameFromRow(firstRow);
        
        firstRow.Click();
        var deleteButton = WaitForElement("DeleteCustomerButton");
        
        // Act
        deleteButton.Click();
        
        var confirmDialog = WaitForElement("ConfirmDeleteDialog");
        var cancelButton = WaitForElement("CancelButton");
        cancelButton.Click();
        
        // Assert
        WaitForElementToDisappear("ConfirmDeleteDialog");
        
        // Verificar se cliente ainda existe
        customerGrid = WaitForElement("CustomerDataGrid");
        var rows = customerGrid.FindElementsByClassName("DataGridRow");
        
        var stillExists = rows.Any(row => row.Text.Contains(customerName));
        stillExists.Should().BeTrue();
    }

    #endregion

    #region Navigation Tests

    [TestMethod]
    public void Navigation_BetweenPages_ShouldWorkCorrectly()
    {
        // Arrange
        var customersTab = WaitForElement("CustomersTab");
        var settingsTab = WaitForElement("SettingsTab");
        
        // Act - Navegar para Settings
        settingsTab.Click();
        
        // Assert
        var settingsPage = WaitForElement("SettingsPage");
        settingsPage.Displayed.Should().BeTrue();
        
        // Act - Retornar para Customers
        customersTab.Click();
        
        // Assert
        var customersPage = WaitForElement("CustomersPage");
        customersPage.Displayed.Should().BeTrue();
    }

    #endregion

    #region Responsive Layout Tests

    [TestMethod]
    public void Layout_WhenWindowResized_ShouldAdaptCorrectly()
    {
        // Arrange
        var originalSize = _driver!.Manage().Window.Size;
        
        // Act - Redimensionar para menor
        _driver.Manage().Window.Size = new Size(800, 600);
        Thread.Sleep(1000);
        
        // Assert
        var customerGrid = WaitForElement("CustomerDataGrid");
        customerGrid.Displayed.Should().BeTrue();
        
        // Verificar se layout se adaptou (pode ter scrollbars)
        var horizontalScrollbar = TryFindElement("HorizontalScrollBar");
        
        // Act - Restaurar tamanho original
        _driver.Manage().Window.Size = originalSize;
        Thread.Sleep(1000);
        
        // Assert
        customerGrid.Displayed.Should().BeTrue();
    }

    #endregion

    #region Helper Methods

    private WindowsElement WaitForElement(string accessibilityId, int timeoutSeconds = 10)
    {
        var timeout = DateTime.Now.AddSeconds(timeoutSeconds);
        
        while (DateTime.Now < timeout)
        {
            try
            {
                var element = _driver!.FindElementByAccessibilityId(accessibilityId);
                if (element != null && element.Displayed)
                    return element;
            }
            catch (WebDriverException)
            {
                // Elemento não encontrado, continuar tentando
            }
            
            Thread.Sleep(500);
        }
        
        throw new TimeoutException($"Elemento '{accessibilityId}' não encontrado dentro do timeout de {timeoutSeconds} segundos");
    }

    private WindowsElement? TryFindElement(string accessibilityId)
    {
        try
        {
            return _driver!.FindElementByAccessibilityId(accessibilityId);
        }
        catch (WebDriverException)
        {
            return null;
        }
    }

    private void WaitForElementToDisappear(string accessibilityId, int timeoutSeconds = 10)
    {
        var timeout = DateTime.Now.AddSeconds(timeoutSeconds);
        
        while (DateTime.Now < timeout)
        {
            try
            {
                var element = _driver!.FindElementByAccessibilityId(accessibilityId);
                if (element == null || !element.Displayed)
                    return;
            }
            catch (WebDriverException)
            {
                // Elemento não encontrado - é o que queremos
                return;
            }
            
            Thread.Sleep(500);
        }
        
        throw new TimeoutException($"Elemento '{accessibilityId}' ainda está visível após {timeoutSeconds} segundos");
    }

    private void OpenCreateCustomerDialog()
    {
        var addButton = WaitForElement("AddCustomerButton");
        addButton.Click();
        WaitForElement("CreateCustomerDialog");
    }

    private void OpenEditCustomerDialog()
    {
        var customerGrid = WaitForElement("CustomerDataGrid");
        var firstRow = customerGrid.FindElementByClassName("DataGridRow");
        DoubleClick(firstRow);
        WaitForElement("EditCustomerDialog");
    }

    private void FillCustomerForm(string firstName, string lastName, string email, string phone)
    {
        var firstNameField = WaitForElement("FirstNameEntry");
        var lastNameField = WaitForElement("LastNameEntry");
        var emailField = WaitForElement("EmailEntry");
        var phoneField = WaitForElement("PhoneEntry");
        
        firstNameField.Clear();
        firstNameField.SendKeys(firstName);
        
        lastNameField.Clear();
        lastNameField.SendKeys(lastName);
        
        emailField.Clear();
        emailField.SendKeys(email);
        
        phoneField.Clear();
        phoneField.SendKeys(phone);
    }

    private void DoubleClick(WindowsElement element)
    {
        var actions = new OpenQA.Selenium.Interactions.Actions(_driver);
        actions.DoubleClick(element).Perform();
    }

    private string ExtractCustomerNameFromRow(WindowsElement row)
    {
        // Implementação simplificada - em caso real, seria mais específica
        var cells = row.FindElementsByClassName("DataGridCell");
        return cells.FirstOrDefault()?.Text ?? "Unknown";
    }

    #endregion
}

/// <summary>
/// Configuração para testes de UI do MAUI Desktop
/// </summary>
[TestClass]
public class UITestConfiguration
{
    [AssemblyInitialize]
    public static void AssemblyInitialize(TestContext context)
    {
        // Verificar se WinAppDriver está rodando
        try
        {
            using var client = new HttpClient();
            var response = client.GetAsync("http://127.0.0.1:4723/status").Result;
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("WinAppDriver não está rodando. Execute: WinAppDriver.exe");
            }
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Erro ao conectar com WinAppDriver: {ex.Message}");
        }
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        // Cleanup geral se necessário
    }
}
