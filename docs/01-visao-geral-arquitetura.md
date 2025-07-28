# 1. Visão Geral da Arquitetura

## Introdução

A arquitetura proposta combina os padrões MVVM e DDD para criar uma aplicação robusta, escalável e de fácil manutenção. O objetivo é separar claramente as responsabilidades entre as diferentes camadas da aplicação.

## Princípios Arquiteturais

### 1. Separação de Responsabilidades
- **UI Layer**: Responsável pela apresentação e interação com o usuário
- **Application Layer**: Orquestra os casos de uso da aplicação
- **Domain Layer**: Contém a lógica de negócio e regras do domínio
- **Infrastructure Layer**: Implementa detalhes técnicos e acesso a dados

### 2. Inversão de Dependência
- As camadas superiores não dependem de implementações das camadas inferiores
- Uso de interfaces para definir contratos
- Injeção de dependência para resolver dependências

### 3. Single Responsibility Principle
- Cada classe tem uma única responsabilidade
- Facilita testes e manutenção
- Melhora a legibilidade do código

## Diagrama de Alto Nível

```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   Presentation  │    │   Application   │    │     Domain      │
│     Layer       │───▶│     Layer       │───▶│     Layer      │
│   (MAUI Views)  │    │  (Use Cases)    │    │ (Business Logic)│
└─────────────────┘    └─────────────────┘    └─────────────────┘
         │                       │                       │
         │                       │                       │
         ▼                       ▼                       ▼
┌─────────────────────────────────────────────────────────────────┐
│                    Infrastructure Layer                        │
│        (Data Access, External Services, Logging)               │
└─────────────────────────────────────────────────────────────────┘
```

## Fluxo de Dados

1. **User Interaction**: Usuário interage com a View
2. **Command Binding**: Comando é executado no ViewModel
3. **Use Case Execution**: ViewModel chama Application Service
4. **Domain Logic**: Application Service executa lógica de domínio
5. **Data Persistence**: Dados são persistidos via Repository
6. **UI Update**: ViewModel atualiza propriedades observáveis
7. **View Refresh**: Interface é atualizada automaticamente

## Benefícios

- **Testabilidade**: Cada camada pode ser testada independentemente
- **Flexibilidade**: Fácil troca de implementações
- **Manutenibilidade**: Código organizado e bem estruturado
- **Escalabilidade**: Facilita o crescimento da aplicação
- **Reusabilidade**: Componentes podem ser reutilizados

## Próximos Passos

- [Arquitetura de Camadas](./02-arquitetura-camadas.md)
- [Padrão MVVM](./03-padrao-mvvm.md)
- [Domain-Driven Design](./04-domain-driven-design.md)
