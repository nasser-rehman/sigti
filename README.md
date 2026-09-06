# SIGTI

Sistema de Gerenciamento de Chamados de Tecnologia da Informação.

O SIGTI é uma API para gerenciamento de chamados de suporte de TI, desenvolvida como projeto de estudo e portfólio com foco em arquitetura de software, Domain-Driven Design (DDD), Clean Architecture, CQRS e testes automatizados.

> 🚧 **Projeto em desenvolvimento.**

---

## Sobre o projeto

O SIGTI tem como objetivo centralizar o gerenciamento de chamados de suporte técnico, permitindo organizar o atendimento através de:

- Tickets de suporte;
- Filas de atendimento;
- Departamentos;
- Técnicos;
- Atribuição automática de chamados;
- Controle de prioridade e categoria;
- Histórico de atribuições;
- Comentários;
- Paginação, filtros e ordenação dinâmica.

A aplicação está sendo construída de forma incremental, buscando manter as regras de negócio concentradas no domínio e separar claramente as responsabilidades de cada camada.

---

## Arquitetura

O projeto utiliza uma arquitetura em camadas baseada em Clean Architecture e princípios de DDD.

```text
SIGTI
├── src
│   ├── SIGTI.API
│   ├── SIGTI.Application
│   ├── SIGTI.Domain
│   └── SIGTI.Infrastructure
│
└── tests
    ├── SIGTI.Application.Tests
    ├── SIGTI.Domain.Tests
    └── SIGTI.Infrastructure.Tests
```

### SIGTI.Domain

Contém as regras e conceitos centrais do sistema. Sem dependências externas.

Entre os principais componentes estão:

- Entities e Aggregates;
- Value Objects;
- Enums com peso semântico (ex: Prioridades ordenadas);
- Domain Exceptions;
- Regras e invariantes de negócio.

Principais entidades:

- `Ticket`
- `TicketAssignment`
- `SupportQueue`
- `SupportQueueMember`
- `User`
- `Department`
- `Comment`

---

### SIGTI.Application

Contém os casos de uso da aplicação. Orquestrada utilizando Commands e Queries através do MediatR.

Exemplo de fluxo:

```text
Commands (Escrita)
├── CreateTicketCommand
├── DispatchTicketCommand
├── StartTicketServiceCommand
├── ResolveTicketCommand
└── CloseTicketCommand

Queries (Leitura)
├── GetTicketByIdQuery
└── ListTicketsQuery
```

A camada também contém:

- Validators (FluentValidation);
- Pipeline Behaviors;
- DTOs/Responses;
- Interfaces de persistência e serviços;
- Modelos compartilhados (Paginação).

---

### SIGTI.Infrastructure

Responsável pelas implementações externas da aplicação.

Inclui:

- Entity Framework Core;
- PostgreSQL;
- Repositories e Unit of Work;
- Configurações de mapeamento (Fluent API);
- Migrations seguras;
- Database Seeder;
- Geração atômica dos números dos tickets via PostgreSQL Sequence.

---

### SIGTI.API

É a camada de entrada da aplicação HTTP/REST.

Responsável por:

- Controllers;
- Injeção de Dependência (DI);
- Exception Handling centralizado (Problem Details);
- Swagger/OpenAPI.

---

## Principais conceitos utilizados

### Domain-Driven Design
As principais regras de negócio permanecem no domínio, encapsuladas. O estado só é alterado através de métodos de negócio que garantem consistência.

Exemplo:
```csharp
queue.AddMember(technician, maxConcurrentTickets);
ticket.AssignTechnician(technician, assignedBy, "Motivo da atribuição");
ticket.StartService();
ticket.Resolve();
ticket.Close();
```

### Atribuição Inteligente (Strategy Pattern)
O despacho de tickets suporta dois comportamentos:
- **Atribuição manual:** quando um técnico específico é informado na requisição;
- **Atribuição automática:** via `LowestUtilizationStrategy`, avaliando a taxa percentual de ocupação dos técnicos ativos na fila em relação aos seus limites de chamados simultâneos (`MaxConcurrentTickets`).

### CQRS e MediatR
Separação clara entre operações de mutação de estado (Commands) e consultas otimizadas (Queries), mediadas pelo pipeline do MediatR que também orquestra os comportamentos transversais (como validação).

### Repository + Unit of Work
A persistência é abstraída, permitindo testabilidade e garantindo que as transações de banco de dados (Unit of Work) ocorram apenas quando o caso de uso for concluído com sucesso.

---

## Ciclo de Vida do Ticket

O ciclo de vida do chamado segue uma máquina de estados finita e estrita, centralizada na entidade `Ticket`:

`New` -> `Dispatched` -> `InProgress` -> `Resolved` -> `Closed`

- **Dispatch (`/dispatch`):** Permite despacho direto ou automático via fila (`LowestUtilizationStrategy`).
- **Start (`/start`):** Transiciona para `InProgress`. Requer atribuição ativa.
- **Resolve (`/resolve`):** Transiciona para `Resolved`. Válido a partir de `InProgress` ou `WaitingCustomer`.
- **Close (`/close`):** Transiciona para `Closed`. Válido apenas a partir de `Resolved`.
- **Estados Terminais:** Chamados no estado `Closed` são definitivos e não podem ser reabertos para preservar a integridade histórica de SLA e MTTR.

### Endpoints de Ciclo de Vida

- `POST /api/tickets` - Criação de chamado (`New`)
- `GET /api/tickets/{id}` - Busca detalhada
- `GET /api/tickets` - Listagem paginada
- `PATCH /api/tickets/{id}/dispatch` - Despacho manual ou automático
- `PATCH /api/tickets/{id}/start` - Início do atendimento (`InProgress`)
- `PATCH /api/tickets/{id}/resolve` - Resolução técnica (`Resolved`)
- `PATCH /api/tickets/{id}/close` - Fechamento terminal (`Closed`)

---

## Funcionalidades implementadas

### Tickets
- [x] Criar ticket;
- [x] Geração automática e sequencial de número;
- [x] Atribuição manual direta de técnico;
- [x] Atribuição automática baseada em fila e taxa de utilização (`LowestUtilizationStrategy`);
- [x] Iniciar atendimento (`InProgress`);
- [x] Resolver ticket (`Resolved`);
- [x] Fechar ticket (`Closed` como estado terminal);
- [x] Buscar ticket por ID (carregamento de grafo de relacionamentos);
- [x] Listar tickets;
- [x] Paginação dinâmica (`PagedResult<T>`);
- [x] Filtros multifatoriais;
- [x] Ordenação customizada;
- [x] Controle de prioridade e categoria;
- [x] Histórico completo de atribuições;
- [x] Comentários no domínio.

### Infraestrutura
- [x] Entity Framework Core & PostgreSQL (Npgsql);
- [x] Migrations estruturadas;
- [x] Database Seeder para ambiente de desenvolvimento;
- [x] Global Exception Handler;
- [x] Swagger/OpenAPI.

### Testes
- [x] Testes de Domínio (Invariantes e Regras);
- [x] Testes de Commands e Queries;
- [x] Testes de Validators;
- [x] Testes com Moq para isolamento na camada Application;
- [x] Testes de Integração de Repositórios com PostgreSQL e Respawn.

---

## Paginação, filtros e ordenação

A listagem de tickets suporta consultas dinâmicas de alta performance:

```http
GET /api/tickets?page=1&pageSize=20&status=InProgress&priority=High&sortBy=Priority&sortDirection=Ascending
```

Os resultados podem ser filtrados por status, prioridade, categoria, departamento, fila e técnico atual.

---

## Estratégia de Testes

Os testes estão separados por responsabilidade e nível de isolamento:

```text
tests
├── SIGTI.Domain.Tests           # Regras puras de negócio e invariantes, sem I/O.
├── SIGTI.Application.Tests      # Casos de uso com mocks de dependências (Moq).
└── SIGTI.Infrastructure.Tests   # Testes de integração reais contra PostgreSQL.
```

Os testes de integração utilizam a biblioteca **Respawn** para isolar cada cenário, truncando os dados de forma determinística e garantindo validações precisas de Migrations, ordenações (ex: por severidade de prioridade) e relacionamentos complexos (`Include`).

Para executar todos os testes da solução:
```bash
dotnet test
```

---

## Tecnologias

- C# / .NET
- ASP.NET Core
- Entity Framework Core & PostgreSQL (Npgsql)
- MediatR & FluentValidation
- xUnit & FluentAssertions
- Moq & Respawn (Database Isolation)
- Swagger / OpenAPI

---

## Executando o projeto

### Pré-requisitos
- .NET SDK;
- PostgreSQL;
- Banco de dados configurado.

### Configuração
Configure a connection string em `appsettings.Development.json` no projeto `SIGTI.API`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=SIGTI_Dev;Username=postgres;Password=sua-senha"
  }
}
```

### Aplicando Migrations
```bash
dotnet ef database update -p src/SIGTI.Infrastructure -s src/SIGTI.API
```

### Executando a API
```bash
dotnet run --project src/SIGTI.API
```
Acesse a documentação interativa pelo Swagger gerado localmente.

---

## Banco de dados e Seed

Na inicialização da aplicação, o `DatabaseSeeder` injeta dados essenciais para testes locais:
- Departamento de Tecnologia da Informação;
- Fila de Suporte Técnico;
- Usuários e Técnicos associados.

---

## Próximos passos

- [x] Iniciar atendimento;
- [x] Despacho manual e automático de tickets;
- [x] Resolver ticket;
- [x] Fechar ticket;
- [x] Testes de Integração (Repositórios e Infraestrutura);
- [ ] Adicionar comentários ao chamado (`AddCommentCommand`);
- [ ] Transferir ticket entre filas/departamentos (`TransferTicketCommand`);
- [ ] Histórico/Auditoria completa do ticket;
- [ ] Autenticação e Autorização (JWT);
- [ ] Gestão completa de Usuários, Departamentos e Filas;
- [ ] Testes End-to-End (E2E) na API.

---

## Objetivo

O SIGTI é desenvolvido como um projeto de aprendizado prático e contínuo. O foco central não está apenas em entregar endpoints, mas em aplicar metodologias profissionais da indústria, entender as contrapartidas de cada decisão arquitetural e garantir a confiabilidade do software através da pirâmide de testes.

---

## Licença

Este projeto não possui uma licença definida no momento.
