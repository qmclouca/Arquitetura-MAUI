# Exemplo Prático: Customer Management

Este diretório contém exemplos práticos de implementação dos conceitos apresentados na documentação de arquitetura.

## Estrutura dos Exemplos

```
📁 examples/
├── 📁 CustomerManagement/          # Exemplo completo de gerenciamento de clientes
│   ├── 📁 Domain/                  # Camada de domínio
│   ├── 📁 Application/             # Camada de aplicação
│   ├── 📁 Infrastructure/          # Camada de infraestrutura
│   ├── 📁 Presentation/            # Camada de apresentação
│   └── 📁 Tests/                   # Testes
├── 📁 OrderProcessing/             # Exemplo de processamento de pedidos
├── 📁 DesignPatterns/              # Exemplos de Design Patterns
└── 📁 ValidationExamples/          # Exemplos de validação
```

## CustomerManagement - Caso de Uso Completo

### Funcionalidades Implementadas

1. **Listagem de Clientes**
   - Exibição em grid
   - Busca e filtros
   - Paginação

2. **Criação de Cliente**
   - Formulário com validação
   - Tipos de cliente (Standard, Premium, Corporate)
   - Verificação de duplicatas

3. **Edição de Cliente**
   - Formulário pré-preenchido
   - Validação de alterações
   - Histórico de modificações

4. **Exclusão de Cliente**
   - Confirmação de exclusão
   - Verificação de dependências
   - Exclusão lógica vs física

### Tecnologias Demonstradas

- **MVVM**: Separação clara entre View, ViewModel e Model
- **DDD**: Entidades, Value Objects, Domain Services (locais)
- **API Integration**: Comunicação com microserviços via HTTP
- **HTTP Client Pattern**: Abstração de comunicação com APIs
- **Cache Pattern**: Cache local para melhor performance
- **Dependency Injection**: Injeção de dependências
- **Validation**: FluentValidation
- **Logging**: Serilog estruturado
- **Mapping**: AutoMapper
- **Testing**: Testes unitários, integração e UI
- **Retry Policies**: Polly para resiliência

### Como Executar

1. Abra o projeto no Visual Studio 2022
2. Restaure os pacotes NuGet
3. Configure as URLs das APIs no appsettings.json
4. Inicie os microserviços necessários (CustomerService, etc.)
5. Execute a aplicação MAUI Desktop
4. Execute as migrations: `dotnet ef database update`
5. Execute a aplicação: `F5` ou `Ctrl+F5`

### Pontos de Aprendizado

- Como estruturar um projeto MAUI seguindo Clean Architecture
- Implementação prática do padrão MVVM
- Uso de Domain-Driven Design em aplicações desktop
- Configuração de injeção de dependências
- Implementação de validações robustas
- Uso de logging estruturado
- Testes automatizados

### Próximos Passos

1. Explore o código-fonte nos diretórios correspondentes
2. Execute os testes para entender o comportamento
3. Modifique o código para experimentar
4. Implemente novas funcionalidades usando os mesmos padrões
