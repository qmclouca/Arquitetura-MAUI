# 5. Design System

## Introdução ao Design System

Um Design System é um conjunto de padrões, componentes e diretrizes que garantem consistência visual e de experiência do usuário em toda a aplicação MAUI Desktop.

## Estrutura do Design System

### 5.1 Design Tokens

Valores fundamentais que definem a base visual da aplicação.

```xml
<!-- Resources/Styles/Tokens.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
    
    <!-- Colors -->
    <Color x:Key="Primary">#6366F1</Color>
    <Color x:Key="PrimaryDark">#4F46E5</Color>
    <Color x:Key="PrimaryLight">#A5B4FC</Color>
    
    <Color x:Key="Secondary">#06B6D4</Color>
    <Color x:Key="SecondaryDark">#0891B2</Color>
    <Color x:Key="SecondaryLight">#67E8F9</Color>
    
    <Color x:Key="Success">#10B981</Color>
    <Color x:Key="Warning">#F59E0B</Color>
    <Color x:Key="Error">#EF4444</Color>
    <Color x:Key="Info">#3B82F6</Color>
    
    <!-- Neutral Colors -->
    <Color x:Key="Gray50">#F9FAFB</Color>
    <Color x:Key="Gray100">#F3F4F6</Color>
    <Color x:Key="Gray200">#E5E7EB</Color>
    <Color x:Key="Gray300">#D1D5DB</Color>
    <Color x:Key="Gray400">#9CA3AF</Color>
    <Color x:Key="Gray500">#6B7280</Color>
    <Color x:Key="Gray600">#4B5563</Color>
    <Color x:Key="Gray700">#374151</Color>
    <Color x:Key="Gray800">#1F2937</Color>
    <Color x:Key="Gray900">#111827</Color>
    
    <!-- Surface Colors -->
    <Color x:Key="Surface">#FFFFFF</Color>
    <Color x:Key="SurfaceVariant">#F8FAFC</Color>
    <Color x:Key="Background">#FFFFFF</Color>
    <Color x:Key="BackgroundSecondary">#F1F5F9</Color>
    
    <!-- Text Colors -->
    <Color x:Key="TextPrimary">#0F172A</Color>
    <Color x:Key="TextSecondary">#475569</Color>
    <Color x:Key="TextDisabled">#94A3B8</Color>
    <Color x:Key="TextOnPrimary">#FFFFFF</Color>
    
    <!-- Spacing -->
    <x:Double x:Key="Spacing1">4</x:Double>
    <x:Double x:Key="Spacing2">8</x:Double>
    <x:Double x:Key="Spacing3">12</x:Double>
    <x:Double x:Key="Spacing4">16</x:Double>
    <x:Double x:Key="Spacing5">20</x:Double>
    <x:Double x:Key="Spacing6">24</x:Double>
    <x:Double x:Key="Spacing8">32</x:Double>
    <x:Double x:Key="Spacing10">40</x:Double>
    <x:Double x:Key="Spacing12">48</x:Double>
    <x:Double x:Key="Spacing16">64</x:Double>
    
    <!-- Border Radius -->
    <x:Double x:Key="RadiusSmall">4</x:Double>
    <x:Double x:Key="RadiusMedium">8</x:Double>
    <x:Double x:Key="RadiusLarge">12</x:Double>
    <x:Double x:Key="RadiusXLarge">16</x:Double>
    
    <!-- Font Sizes -->
    <x:Double x:Key="FontSizeXSmall">10</x:Double>
    <x:Double x:Key="FontSizeSmall">12</x:Double>
    <x:Double x:Key="FontSizeMedium">14</x:Double>
    <x:Double x:Key="FontSizeLarge">16</x:Double>
    <x:Double x:Key="FontSizeXLarge">18</x:Double>
    <x:Double x:Key="FontSize2XLarge">20</x:Double>
    <x:Double x:Key="FontSize3XLarge">24</x:Double>
    <x:Double x:Key="FontSize4XLarge">32</x:Double>
    
    <!-- Font Weights -->
    <x:String x:Key="FontWeightLight">Light</x:String>
    <x:String x:Key="FontWeightRegular">Regular</x:String>
    <x:String x:Key="FontWeightMedium">Medium</x:String>
    <x:String x:Key="FontWeightSemiBold">SemiBold</x:String>
    <x:String x:Key="FontWeightBold">Bold</x:String>
    
    <!-- Shadows -->
    <Shadow x:Key="ShadowSmall" Offset="0,1" Opacity="0.05" Radius="3" Brush="{StaticResource Gray900}" />
    <Shadow x:Key="ShadowMedium" Offset="0,4" Opacity="0.1" Radius="6" Brush="{StaticResource Gray900}" />
    <Shadow x:Key="ShadowLarge" Offset="0,10" Opacity="0.15" Radius="15" Brush="{StaticResource Gray900}" />
    
</ResourceDictionary>
```

### 5.2 Typography Styles

Sistema tipográfico consistente para toda a aplicação.

```xml
<!-- Resources/Styles/Typography.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
    
    <!-- Display Styles -->
    <Style x:Key="DisplayLarge" TargetType="Label">
        <Setter Property="FontSize" Value="{StaticResource FontSize4XLarge}" />
        <Setter Property="FontAttributes" Value="{StaticResource FontWeightBold}" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimary}" />
        <Setter Property="LineHeight" Value="1.2" />
    </Style>
    
    <Style x:Key="DisplayMedium" TargetType="Label">
        <Setter Property="FontSize" Value="{StaticResource FontSize3XLarge}" />
        <Setter Property="FontAttributes" Value="{StaticResource FontWeightSemiBold}" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimary}" />
        <Setter Property="LineHeight" Value="1.3" />
    </Style>
    
    <!-- Headline Styles -->
    <Style x:Key="HeadlineLarge" TargetType="Label">
        <Setter Property="FontSize" Value="{StaticResource FontSize2XLarge}" />
        <Setter Property="FontAttributes" Value="{StaticResource FontWeightSemiBold}" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimary}" />
        <Setter Property="LineHeight" Value="1.4" />
    </Style>
    
    <Style x:Key="HeadlineMedium" TargetType="Label">
        <Setter Property="FontSize" Value="{StaticResource FontSizeXLarge}" />
        <Setter Property="FontAttributes" Value="{StaticResource FontWeightMedium}" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimary}" />
        <Setter Property="LineHeight" Value="1.4" />
    </Style>
    
    <!-- Title Styles -->
    <Style x:Key="TitleLarge" TargetType="Label">
        <Setter Property="FontSize" Value="{StaticResource FontSizeLarge}" />
        <Setter Property="FontAttributes" Value="{StaticResource FontWeightMedium}" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimary}" />
        <Setter Property="LineHeight" Value="1.5" />
    </Style>
    
    <Style x:Key="TitleMedium" TargetType="Label">
        <Setter Property="FontSize" Value="{StaticResource FontSizeMedium}" />
        <Setter Property="FontAttributes" Value="{StaticResource FontWeightMedium}" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimary}" />
        <Setter Property="LineHeight" Value="1.5" />
    </Style>
    
    <!-- Body Styles -->
    <Style x:Key="BodyLarge" TargetType="Label">
        <Setter Property="FontSize" Value="{StaticResource FontSizeLarge}" />
        <Setter Property="FontAttributes" Value="{StaticResource FontWeightRegular}" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimary}" />
        <Setter Property="LineHeight" Value="1.6" />
    </Style>
    
    <Style x:Key="BodyMedium" TargetType="Label">
        <Setter Property="FontSize" Value="{StaticResource FontSizeMedium}" />
        <Setter Property="FontAttributes" Value="{StaticResource FontWeightRegular}" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimary}" />
        <Setter Property="LineHeight" Value="1.6" />
    </Style>
    
    <Style x:Key="BodySmall" TargetType="Label">
        <Setter Property="FontSize" Value="{StaticResource FontSizeSmall}" />
        <Setter Property="FontAttributes" Value="{StaticResource FontWeightRegular}" />
        <Setter Property="TextColor" Value="{StaticResource TextSecondary}" />
        <Setter Property="LineHeight" Value="1.6" />
    </Style>
    
    <!-- Label Styles -->
    <Style x:Key="LabelLarge" TargetType="Label">
        <Setter Property="FontSize" Value="{StaticResource FontSizeMedium}" />
        <Setter Property="FontAttributes" Value="{StaticResource FontWeightMedium}" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimary}" />
        <Setter Property="LineHeight" Value="1.4" />
    </Style>
    
    <Style x:Key="LabelMedium" TargetType="Label">
        <Setter Property="FontSize" Value="{StaticResource FontSizeSmall}" />
        <Setter Property="FontAttributes" Value="{StaticResource FontWeightMedium}" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimary}" />
        <Setter Property="LineHeight" Value="1.4" />
    </Style>
    
</ResourceDictionary>
```

### 5.3 Button Styles

Sistema de botões consistente com diferentes variações.

```xml
<!-- Resources/Styles/Buttons.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
    
    <!-- Base Button Style -->
    <Style x:Key="BaseButton" TargetType="Button">
        <Setter Property="FontSize" Value="{StaticResource FontSizeMedium}" />
        <Setter Property="FontAttributes" Value="{StaticResource FontWeightMedium}" />
        <Setter Property="CornerRadius" Value="{StaticResource RadiusMedium}" />
        <Setter Property="Padding" Value="{StaticResource Spacing4}, {StaticResource Spacing3}" />
        <Setter Property="MinimumHeightRequest" Value="44" />
        <Setter Property="Shadow" Value="{StaticResource ShadowSmall}" />
        <Setter Property="VisualStateManager.VisualStateGroups">
            <VisualStateGroupList>
                <VisualStateGroup x:Name="CommonStates">
                    <VisualState x:Name="Normal" />
                    <VisualState x:Name="Disabled">
                        <VisualState.Setters>
                            <Setter Property="Opacity" Value="0.6" />
                        </VisualState.Setters>
                    </VisualState>
                    <VisualState x:Name="Pressed">
                        <VisualState.Setters>
                            <Setter Property="Scale" Value="0.98" />
                        </VisualState.Setters>
                    </VisualState>
                </VisualStateGroup>
            </VisualStateGroupList>
        </Setter>
    </Style>
    
    <!-- Primary Button -->
    <Style x:Key="PrimaryButton" TargetType="Button" BasedOn="{StaticResource BaseButton}">
        <Setter Property="BackgroundColor" Value="{StaticResource Primary}" />
        <Setter Property="TextColor" Value="{StaticResource TextOnPrimary}" />
        <Setter Property="VisualStateManager.VisualStateGroups">
            <VisualStateGroupList>
                <VisualStateGroup x:Name="CommonStates">
                    <VisualState x:Name="Normal" />
                    <VisualState x:Name="Pressed">
                        <VisualState.Setters>
                            <Setter Property="BackgroundColor" Value="{StaticResource PrimaryDark}" />
                            <Setter Property="Scale" Value="0.98" />
                        </VisualState.Setters>
                    </VisualState>
                </VisualStateGroup>
            </VisualStateGroupList>
        </Setter>
    </Style>
    
    <!-- Secondary Button -->
    <Style x:Key="SecondaryButton" TargetType="Button" BasedOn="{StaticResource BaseButton}">
        <Setter Property="BackgroundColor" Value="{StaticResource Surface}" />
        <Setter Property="TextColor" Value="{StaticResource Primary}" />
        <Setter Property="BorderColor" Value="{StaticResource Primary}" />
        <Setter Property="BorderWidth" Value="1" />
    </Style>
    
    <!-- Ghost Button -->
    <Style x:Key="GhostButton" TargetType="Button" BasedOn="{StaticResource BaseButton}">
        <Setter Property="BackgroundColor" Value="Transparent" />
        <Setter Property="TextColor" Value="{StaticResource Primary}" />
        <Setter Property="Shadow" Value="{x:Null}" />
    </Style>
    
    <!-- Danger Button -->
    <Style x:Key="DangerButton" TargetType="Button" BasedOn="{StaticResource BaseButton}">
        <Setter Property="BackgroundColor" Value="{StaticResource Error}" />
        <Setter Property="TextColor" Value="{StaticResource TextOnPrimary}" />
    </Style>
    
    <!-- Icon Button -->
    <Style x:Key="IconButton" TargetType="Button">
        <Setter Property="BackgroundColor" Value="Transparent" />
        <Setter Property="TextColor" Value="{StaticResource TextSecondary}" />
        <Setter Property="CornerRadius" Value="{StaticResource RadiusMedium}" />
        <Setter Property="Padding" Value="{StaticResource Spacing2}" />
        <Setter Property="MinimumHeightRequest" Value="40" />
        <Setter Property="MinimumWidthRequest" Value="40" />
        <Setter Property="VisualStateManager.VisualStateGroups">
            <VisualStateGroupList>
                <VisualStateGroup x:Name="CommonStates">
                    <VisualState x:Name="Normal" />
                    <VisualState x:Name="Pressed">
                        <VisualState.Setters>
                            <Setter Property="BackgroundColor" Value="{StaticResource Gray100}" />
                            <Setter Property="Scale" Value="0.95" />
                        </VisualState.Setters>
                    </VisualState>
                </VisualStateGroup>
            </VisualStateGroupList>
        </Setter>
    </Style>
    
</ResourceDictionary>
```

### 5.4 Input Controls

Estilos para controles de entrada de dados.

```xml
<!-- Resources/Styles/Inputs.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
    
    <!-- Base Entry Style -->
    <Style x:Key="BaseEntry" TargetType="Entry">
        <Setter Property="FontSize" Value="{StaticResource FontSizeMedium}" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimary}" />
        <Setter Property="BackgroundColor" Value="{StaticResource Surface}" />
        <Setter Property="PlaceholderColor" Value="{StaticResource TextDisabled}" />
        <Setter Property="MinimumHeightRequest" Value="48" />
        <Setter Property="Padding" Value="{StaticResource Spacing4}" />
        <Setter Property="VisualStateManager.VisualStateGroups">
            <VisualStateGroupList>
                <VisualStateGroup x:Name="CommonStates">
                    <VisualState x:Name="Normal">
                        <VisualState.Setters>
                            <Setter Property="BackgroundColor" Value="{StaticResource Surface}" />
                        </VisualState.Setters>
                    </VisualState>
                    <VisualState x:Name="Focused">
                        <VisualState.Setters>
                            <Setter Property="BackgroundColor" Value="{StaticResource Surface}" />
                        </VisualState.Setters>
                    </VisualState>
                    <VisualState x:Name="Disabled">
                        <VisualState.Setters>
                            <Setter Property="Opacity" Value="0.6" />
                        </VisualState.Setters>
                    </VisualState>
                </VisualStateGroup>
            </VisualStateGroupList>
        </Setter>
    </Style>
    
    <!-- Outlined Entry -->
    <Style x:Key="OutlinedEntry" TargetType="Entry" BasedOn="{StaticResource BaseEntry}">
        <Setter Property="BackgroundColor" Value="Transparent" />
        <!-- Implementar usando Border ou Frame -->
    </Style>
    
    <!-- Editor Style -->
    <Style x:Key="BaseEditor" TargetType="Editor">
        <Setter Property="FontSize" Value="{StaticResource FontSizeMedium}" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimary}" />
        <Setter Property="BackgroundColor" Value="{StaticResource Surface}" />
        <Setter Property="PlaceholderColor" Value="{StaticResource TextDisabled}" />
        <Setter Property="MinimumHeightRequest" Value="100" />
        <Setter Property="Padding" Value="{StaticResource Spacing4}" />
    </Style>
    
    <!-- Picker Style -->
    <Style x:Key="BasePicker" TargetType="Picker">
        <Setter Property="FontSize" Value="{StaticResource FontSizeMedium}" />
        <Setter Property="TextColor" Value="{StaticResource TextPrimary}" />
        <Setter Property="BackgroundColor" Value="{StaticResource Surface}" />
        <Setter Property="TitleColor" Value="{StaticResource TextDisabled}" />
        <Setter Property="MinimumHeightRequest" Value="48" />
    </Style>
    
    <!-- Switch Style -->
    <Style x:Key="BaseSwitch" TargetType="Switch">
        <Setter Property="OnColor" Value="{StaticResource Primary}" />
        <Setter Property="ThumbColor" Value="{StaticResource Surface}" />
    </Style>
    
    <!-- CheckBox Style -->
    <Style x:Key="BaseCheckBox" TargetType="CheckBox">
        <Setter Property="Color" Value="{StaticResource Primary}" />
        <Setter Property="MinimumHeightRequest" Value="24" />
        <Setter Property="MinimumWidthRequest" Value="24" />
    </Style>
    
</ResourceDictionary>
```

### 5.5 Card Component

Componente reutilizável para exibição de conteúdo.

```xml
<!-- Controls/Card.xaml -->
<ContentView x:Class="App.Controls.Card"
             xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
    
    <Frame BackgroundColor="{StaticResource Surface}"
           CornerRadius="{StaticResource RadiusLarge}"
           Shadow="{StaticResource ShadowMedium}"
           Padding="{StaticResource Spacing6}"
           HasShadow="True"
           BorderColor="Transparent">
        
        <ContentPresenter Content="{Binding Source={x:Reference CardContent}, Path=Content}" />
        
    </Frame>
    
</ContentView>
```

```csharp
// Controls/Card.xaml.cs
using Microsoft.Maui.Controls;

namespace App.Controls;

public partial class Card : ContentView
{
    public static readonly BindableProperty ContentProperty =
        BindableProperty.Create(
            nameof(Content),
            typeof(View),
            typeof(Card),
            null);

    public View Content
    {
        get => (View)GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public Card()
    {
        InitializeComponent();
    }
}
```

### 5.6 Custom Controls

#### Loading Indicator

```xml
<!-- Controls/LoadingIndicator.xaml -->
<ContentView x:Class="App.Controls.LoadingIndicator"
             xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
    
    <Grid IsVisible="{Binding IsLoading}">
        <BoxView BackgroundColor="Black" Opacity="0.3" />
        
        <StackLayout VerticalOptions="Center" HorizontalOptions="Center">
            <ActivityIndicator IsRunning="{Binding IsLoading}"
                             Color="{StaticResource Primary}"
                             Scale="1.5" />
            
            <Label Text="{Binding LoadingMessage, FallbackValue='Loading...'}"
                   Style="{StaticResource BodyMedium}"
                   HorizontalOptions="Center"
                   Margin="0,{StaticResource Spacing4},0,0" />
        </StackLayout>
    </Grid>
    
</ContentView>
```

#### Alert Component

```xml
<!-- Controls/Alert.xaml -->
<ContentView x:Class="App.Controls.Alert"
             xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
    
    <Frame BackgroundColor="{Binding AlertColor}"
           CornerRadius="{StaticResource RadiusMedium}"
           Padding="{StaticResource Spacing4}"
           BorderColor="Transparent"
           IsVisible="{Binding IsVisible}">
        
        <Grid ColumnDefinitions="Auto,*,Auto">
            <!-- Icon -->
            <Label Grid.Column="0"
                   Text="{Binding AlertIcon}"
                   FontFamily="MaterialIcons"
                   FontSize="{StaticResource FontSizeLarge}"
                   TextColor="{Binding AlertTextColor}"
                   VerticalOptions="Start"
                   Margin="0,0,{StaticResource Spacing3},0" />
            
            <!-- Content -->
            <StackLayout Grid.Column="1">
                <Label Text="{Binding Title}"
                       Style="{StaticResource LabelMedium}"
                       TextColor="{Binding AlertTextColor}"
                       IsVisible="{Binding HasTitle}" />
                
                <Label Text="{Binding Message}"
                       Style="{StaticResource BodyMedium}"
                       TextColor="{Binding AlertTextColor}" />
            </StackLayout>
            
            <!-- Close Button -->
            <Button Grid.Column="2"
                    Text="✕"
                    Style="{StaticResource IconButton}"
                    Command="{Binding CloseCommand}"
                    IsVisible="{Binding CanClose}"
                    VerticalOptions="Start" />
        </Grid>
    </Frame>
    
</ContentView>
```

### 5.7 Layout Patterns

#### Master-Detail Layout

```xml
<!-- Views/MasterDetailView.xaml -->
<Grid ColumnDefinitions="300,*">
    
    <!-- Sidebar -->
    <Frame Grid.Column="0"
           BackgroundColor="{StaticResource SurfaceVariant}"
           CornerRadius="0"
           Padding="0">
        
        <ScrollView>
            <StackLayout Padding="{StaticResource Spacing6}">
                <!-- Navigation Items -->
                <CollectionView ItemsSource="{Binding NavigationItems}">
                    <CollectionView.ItemTemplate>
                        <DataTemplate>
                            <Grid Padding="{StaticResource Spacing3}">
                                <Frame BackgroundColor="Transparent"
                                       CornerRadius="{StaticResource RadiusMedium}">
                                    <Grid ColumnDefinitions="Auto,*">
                                        <Label Grid.Column="0"
                                               Text="{Binding Icon}"
                                               FontFamily="MaterialIcons" />
                                        <Label Grid.Column="1"
                                               Text="{Binding Title}"
                                               Style="{StaticResource BodyMedium}" />
                                    </Grid>
                                </Frame>
                            </Grid>
                        </DataTemplate>
                    </CollectionView.ItemTemplate>
                </CollectionView>
            </StackLayout>
        </ScrollView>
    </Frame>
    
    <!-- Main Content -->
    <ContentPresenter Grid.Column="1"
                     Content="{Binding CurrentView}"
                     Margin="{StaticResource Spacing6}" />
    
</Grid>
```

## Tema Escuro

```xml
<!-- Resources/Styles/DarkTheme.xaml -->
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml">
    
    <!-- Dark Theme Colors -->
    <Color x:Key="Surface">#1E1E1E</Color>
    <Color x:Key="SurfaceVariant">#2D2D30</Color>
    <Color x:Key="Background">#121212</Color>
    <Color x:Key="BackgroundSecondary">#1E1E1E</Color>
    
    <Color x:Key="TextPrimary">#FFFFFF</Color>
    <Color x:Key="TextSecondary">#B3B3B3</Color>
    <Color x:Key="TextDisabled">#666666</Color>
    
</ResourceDictionary>
```

## Responsividade

```csharp
// Services/ResponsiveService.cs
public class ResponsiveService
{
    public static double GetResponsiveValue(double screenWidth, 
                                          double mobileValue, 
                                          double tabletValue, 
                                          double desktopValue)
    {
        return screenWidth switch
        {
            < 768 => mobileValue,
            < 1024 => tabletValue,
            _ => desktopValue
        };
    }
    
    public static GridLength GetResponsiveGridLength(double screenWidth,
                                                   GridLength mobile,
                                                   GridLength tablet,
                                                   GridLength desktop)
    {
        return screenWidth switch
        {
            < 768 => mobile,
            < 1024 => tablet,
            _ => desktop
        };
    }
}
```

## Próximos Tópicos

- [Logging com Serilog](./06-logging-serilog.md)
- [Design Patterns](./07-design-patterns.md)
- [Diagramas UML](./08-diagramas-uml.md)
