# Plano de Implementação — Ballast Lane .NET Exercise

Oct 23, 2026 · @Marcos

## Resumo executivo

A recomendação é construir **um único domínio — gerenciamento de tarefas (Task Manager)** — que atende ao mesmo tempo o projeto principal e a seção de GenAI do PDF. Assim você apresenta uma história só, coerente, em vez de dois projetos desconectados.

Decisões principais:

- **Backend:** .NET 10 (LTS), ASP.NET Core com **Controllers** (atende ao "ASP.NET MVC, Web API" do PDF), Clean Architecture em 4 projetos (Domain, Application, Infrastructure, Api).
- **Banco:** PostgreSQL em container, acessado via EF Core com migrations e seed.
- **Autenticação:** JWT Bearer, senha com hash (PBKDF2 via `PasswordHasher<T>`).
- **Duas APIs:** Auth API (`/api/auth`) e Tasks API (`/api/tasks`) no mesmo host, com dois documentos OpenAPI separados — a escolha fica registrada em ADR.
- **Testes:** TDD com xUnit; unitários com NSubstitute; integração com **Testcontainers** (PostgreSQL real) e `WebApplicationFactory`; testes de arquitetura com NetArchTest.
- **Frontend:** Angular (versão estável atual, v22) com standalone components, signals, reactive forms tipados, interceptors e guards funcionais.
- **Execução:** `docker compose up --build` sobe banco, API e frontend com dados e credenciais de demo.
- **Processo:** Spec-Driven Development (spec → design docs e ADRs → tarefas → implementação), com Test-Driven Development em cada tarefa, commits no padrão Conventional Commits mostrando o ciclo de TDD (teste primeiro).
- **IA:** `CLAUDE.md` + subagentes em `.claude/agents/` trabalhando em git worktrees isolados, orquestrados pela sessão principal.

Princípio que guia tudo: **simplicidade defensável**. Cada peça deve ter um motivo que você consiga explicar em uma frase durante a apresentação. Evite o que não consegue justificar (CQRS completo, event sourcing, microsserviços).

## Metodologia: SDD, Technical Design Document e TDD

As três metodologias não competem, elas se encaixam em níveis diferentes. **Spec-Driven Development (SDD)** é o fluxo do projeto inteiro; o **Technical Design Document** é o artefato da etapa de design; o **Test-Driven Development (TDD)** é a técnica usada dentro de cada tarefa, e os testes unitários são o produto direto dela.

&#91;embedded content: metodologia · SDD em 4 etapas, TDD dentro da implementação\]

| Metodologia | Pergunta que responde | Artefato | Onde fica | Quando |
| --- | --- | --- | --- | --- |
| SDD — etapa Spec | O que vamos construir e por quê? | User story, critérios de aceite, fora de escopo | `docs/spec/user-story.md` | Fase 0, antes de tudo |
| Technical Design Document | Como cada funcionalidade será construída? | Um design doc por funcionalidade | `docs/design/DD-0N-*.md` | Fase 0, depois da spec |
| ADR | Qual decisão tomamos e o que descartamos? | Um registro por decisão | `docs/adr/` | Junto com os design docs |
| SDD — etapa Tasks | Em que partes independentes dividimos o design? | Um arquivo por tarefa | `docs/tasks/T*.md` | Fim da Fase 0 |
| Test-Driven Development | Como garantir que cada parte funciona? | Testes unitários escritos antes do código | `backend/tests/`, `*.spec.ts` | Fases 1 a 3, em toda tarefa |
| Testes de integração | O design funciona com banco e HTTP reais? | Testes com Testcontainers | `backend/tests/*IntegrationTests` | Fases 2 e 3 |

Design docs previstos (um por funcionalidade, curtos, 1 a 2 páginas):

- `DD-01-authentication.md` — cadastro, login, JWT, hash de senha, Auth API.
- `DD-02-task-management.md` — modelo de domínio, regras, Tasks API, persistência, propriedade das tarefas.
- `DD-03-frontend.md` — rotas, telas, store com signals, integração com a API.
- `DD-04-runtime-and-quality.md` — Docker, seed, estratégia de testes, CI.

Modelo de Technical Design Document (`docs/design/DD-00-template.md`):

```markdown
# DD-0N — Feature name

- Status: Approved
- Spec: docs/spec/user-story.md (acceptance criteria 1, 2, 9)
- Related ADRs: ADR-0004, ADR-0005

## 1. Goal and scope
What this feature delivers. What is explicitly out of scope.

## 2. Proposed solution
How it works end to end (a short flow or sequence).

## 3. Contracts
Endpoints, DTOs, interfaces (ports) and error cases.

## 4. Data model
Tables, fields, constraints, indexes.

## 5. Business rules and validation
Where each rule lives (Domain or Application).

## 6. Testing approach
Which rules get unit tests, which flows get integration tests.

## 7. Risks and open questions

## 8. Task breakdown
T20 — ..., T22 — ... (links to docs/tasks/)
```

A seção 8 é a ponte para a etapa Tasks do SDD, e a seção 6 é a ponte para o Test-Driven Development: cada tarefa herda dela a lista de testes a escrever primeiro.

Diferença entre ADR e design doc: o ADR registra **uma decisão** e as alternativas descartadas; o design doc descreve **uma funcionalidade inteira** e cita os ADRs que segue. Um design doc costuma gerar dois ou três ADRs.

Cuidado com a sigla: no repositório e na apresentação, use **TDD** só para Test-Driven Development e chame o outro de **design doc** (pasta `docs/design/`, prefixo `DD-`). O painel vai ouvir "TDD" como teste; falar "Technical Design Document" por extenso na primeira menção evita confusão.

## Requisitos do PDF mapeados para a solução

Cada requisito do PDF tem uma resposta concreta e um lugar no repositório. Use esta tabela como checklist final antes da entrega e mostre-a no README — avaliadores gostam de ver que nada foi esquecido.

| Requisito do PDF | Como será atendido | Onde fica |
| --- | --- | --- |
| User story informal | User story + critérios de aceite do Task Manager | `docs/spec/user-story.md` |
| Banco com tabela de dados + tabela de usuários | PostgreSQL com tabelas `tasks` e `users` | `Infrastructure/Persistence` |
| PK + pelo menos 2 campos | `tasks`: Id, Title, Description, Status, DueDate, UserId, CreatedAt, UpdatedAt | Migrations EF Core |
| Web API com CRUD | `TasksController`: GET lista, GET por id, POST, PUT, DELETE | `Api/Controllers` |
| Verbos, parâmetros e retornos adequados | 200/201/204/400/401/404/409, `Location` header, ProblemDetails | Contrato da API (seção abaixo) |
| Segunda API: criar usuário, login, endpoints autorizados e não autorizados | `AuthController`: register, login (públicos), `me` (autorizado); documento OpenAPI separado | `Api/Controllers` |
| Camada de acesso a dados | Repositórios EF Core atrás de interfaces da Application | `Infrastructure` |
| Camada de negócio independente de dados e API | Projeto Application + Domain sem referência a EF Core nem ASP.NET | `Domain`, `Application` |
| Testes unitários de todos os componentes | Domain, Application, Controllers (unit); repositórios e API (integração) | `tests/` |
| Frontend com framework | Angular, consumindo a API | `frontend/` |
| Responsivo e amigável | Angular Material + layout flexível, testado em largura de celular | `frontend/` |
| CRUD no frontend | Lista, criar, editar, excluir tarefas, com filtros por status | `features/tasks` |
| Código estruturado | Pastas `core`, `shared`, `features`; estado em services com signals | `frontend/src/app` |
| README com setup | Pré-requisitos, `docker compose up`, credenciais, como rodar testes | `README.md` |
| Dados e credenciais seed | Usuário demo + \~8 tarefas em status variados | Seed na Infrastructure |
| GenAI: prompt, código gerado, validação | Pasta dedicada com prompt, amostra e log de correções | `docs/genai/` |
| Sem warnings no console (desejável) | `track` em todo `@for`, sem erros 404/CORS, build em modo produção | Checklist de entrega |

Duas interpretações ficam registradas em ADR para você defender na apresentação: (1) a "segunda API" é um conjunto separado de endpoints e documento OpenAPI no mesmo host, e (2) "unit tests da camada de dados" são testes de integração contra PostgreSQL real, porque mockar o EF Core testa o mock, não o acesso a dados.

## Domínio e user story

No Spec-Driven Development, esta seção é a **spec**: vive em `docs/spec/user-story.md` e descreve o quê e o porquê, nunca o como. O como fica nos design docs (`docs/design/`).

O domínio recomendado é **gerenciamento de tarefas pessoais**, porque a seção de GenAI do PDF pede exatamente esse sistema (title, description, status, due\_date, associado a um usuário). Você desenvolve uma vez e usa como exemplo nas duas partes da avaliação.

User story principal (escreva em inglês no repositório, já que a vaga é internacional):

> **As a** busy professional, **I want to** register, log in and manage my own tasks with a title, description, status and due date, **so that** I can track what I need to do and what is overdue, without seeing anyone else's tasks.

Critérios de aceite (cada um vira pelo menos um teste):

1. Posso me cadastrar com nome, e-mail e senha; e-mail duplicado é rejeitado (409).
2. Posso fazer login e recebo um token; credenciais erradas retornam 401 com mensagem genérica.
3. Só vejo, edito e excluo minhas próprias tarefas; a tarefa de outro usuário responde 404 (não revela que existe).
4. Título é obrigatório (3 a 100 caracteres); descrição tem no máximo 1000 caracteres.
5. Ao criar, a data de vencimento não pode estar no passado.
6. Status possíveis: `Todo`, `InProgress`, `Done`. Uma tarefa `Done` pode ser reaberta para `Todo`.
7. A lista pode ser filtrada por status e é paginada, ordenada por data de vencimento.
8. Uma tarefa não concluída com vencimento passado aparece como atrasada (`isOverdue`).
9. Sem token, qualquer endpoint de tarefas responde 401.

Regras 4, 5, 6 e 8 ficam no **Domain** (invariantes da entidade). Regras 1, 2 e 3 ficam na **Application** (casos de uso). Isso dá um exemplo claro de "onde mora cada regra" para a apresentação.

Mantenha o escopo exatamente aqui. Categorias, compartilhamento de tarefas, anexos e notificações ficam numa seção "Future improvements" do README — mostrar que você sabe o que deixou de fora vale mais do que implementar pela metade.

## Arquitetura e estrutura da solução

A solução segue Clean Architecture com quatro projetos e uma regra única: **as dependências apontam sempre para dentro**. Domain não conhece ninguém; Application conhece só Domain; Infrastructure e Api conhecem Application. A Api é a única que conhece todos, porque é o *composition root* (onde a injeção de dependência é montada).

&#91;embedded content: camadas da solução · 4 projetos, 2 sistemas externos\]

Api e Infrastructure ficam nas bordas e dependem da Application; o Domain, no centro, não depende de nada — e os testes de arquitetura garantem que continue assim.

Responsabilidade de cada camada:

- **Domain** — entidades (`User`, `TaskItem`), enum `TaskItemStatus`, invariantes e exceções de domínio. Zero pacotes NuGet.
- **Application** — casos de uso (`TaskService`, `AuthService`), DTOs de entrada e saída, validadores, interfaces de portas (`ITaskRepository`, `IUserRepository`, `IPasswordHasher`, `ITokenService`, `ICurrentUser`). Usa `TimeProvider` do .NET para datas testáveis. Não há `IUnitOfWork`: o `DbContext` do EF Core já é a unidade de trabalho.
- **Infrastructure** — `AppDbContext`, configurações EF Core (Fluent API), repositórios, migrations, seed, implementação de JWT e hash de senha.
- **Api** — controllers, configuração de autenticação, tratamento global de erros, OpenAPI, CORS, health checks, `Program.cs`.

Use o nome `TaskItem` em vez de `Task` para não colidir com `System.Threading.Tasks.Task` — é um detalhe pequeno que vale citar na apresentação.

Estrutura de diretórios do repositório:

```text
task-manager/
├── CLAUDE.md                       # instruções globais para a IA
├── README.md
├── docker-compose.yml
├── .env.example
├── .editorconfig
├── Directory.Build.props           # nullable, warnings as errors, analyzers
├── Directory.Packages.props        # versões centralizadas dos pacotes NuGet
├── TaskManager.sln
├── .claude/                        # configuração do Claude Code (ver seção própria)
├── docs/
│   ├── spec/                       # SDD etapa 1: user story e critérios de aceite
│   ├── design/                     # Technical Design Documents (DD-01 a DD-04)
│   ├── adr/                        # decisões de arquitetura
│   ├── tasks/                      # SDD etapa 3: uma especificação por tarefa
│   ├── genai/                      # entregável da seção GenAI
│   ├── architecture.md             # visão geral com diagrama
│   └── api.md
├── backend/
│   ├── src/
│   │   ├── TaskManager.Domain/
│   │   ├── TaskManager.Application/
│   │   ├── TaskManager.Infrastructure/
│   │   └── TaskManager.Api/
│   ├── tests/                      # Test-Driven Development: unitários + integração
│   │   ├── TaskManager.Domain.UnitTests/
│   │   ├── TaskManager.Application.UnitTests/
│   │   ├── TaskManager.Api.UnitTests/
│   │   ├── TaskManager.Infrastructure.IntegrationTests/
│   │   ├── TaskManager.Api.IntegrationTests/
│   │   └── TaskManager.ArchitectureTests/
│   └── Dockerfile
└── frontend/
    ├── src/app/
    ├── nginx.conf
    └── Dockerfile
```

Por que não CQRS com MediatR, vertical slices ou microsserviços: o domínio tem duas entidades e nove regras. Serviços de aplicação explícitos deixam o fluxo legível do controller até o banco em três saltos, o que facilita muito a code review ao vivo. Diga isso com essas palavras na apresentação — mostra critério, não falta de conhecimento.

## Modelo de dados e banco de dados

A escolha é **PostgreSQL 18 em container com EF Core** (provider Npgsql). Relacional é natural aqui (tarefa pertence a usuário, FK com índice), migrations dão histórico versionado do schema, e o par EF Core + PostgreSQL é o mais esperado por um painel .NET.

Tabela `users`:

| Coluna | Tipo | Regras |
| --- | --- | --- |
| `id` | uuid | PK, gerado na aplicação (`Guid.CreateVersion7()`, ordenável) |
| `name` | varchar(100) | obrigatório |
| `email` | varchar(254) | obrigatório, único, armazenado em minúsculas |
| `password_hash` | varchar(500) | obrigatório, nunca exposto pela API |
| `created_at` | timestamptz | obrigatório |

Tabela `tasks`:

| Coluna | Tipo | Regras |
| --- | --- | --- |
| `id` | uuid | PK, Guid v7 |
| `user_id` | uuid | FK para `users.id`, `ON DELETE CASCADE`, indexado |
| `title` | varchar(100) | obrigatório |
| `description` | varchar(1000) | opcional |
| `status` | varchar(20) | `Todo`, `InProgress`, `Done` (enum salvo como texto, legível no banco) |
| `due_date` | date | obrigatório |
| `created_at` | timestamptz | obrigatório |
| `updated_at` | timestamptz | obrigatório |

Índice composto `(user_id, status, due_date)` cobre a consulta principal: listar minhas tarefas filtradas por status e ordenadas por vencimento.

Boas práticas no acesso a dados:

- Configuração por `IEntityTypeConfiguration<T>` (uma classe por entidade), nunca atributos de EF no Domain.
- Nomes em `snake_case` via convenção (pacote `EFCore.NamingConventions`), padrão do PostgreSQL.
- `AsNoTracking()` em consultas de leitura; paginação com `Skip/Take` e contagem total.
- Toda consulta de tarefa filtra por `user_id` dentro do repositório — a regra de propriedade não depende do controller lembrar.
- Controle de concorrência otimista com a coluna de sistema `xmin` do PostgreSQL (opcional, ótimo argumento técnico se der tempo).

Seed de demonstração (executado no startup em ambiente Development, idempotente):

- Usuário `demo@taskmanager.local` / senha `Demo@12345`.
- Segundo usuário `other@taskmanager.local` / `Other@12345`, para demonstrar ao vivo que um não vê as tarefas do outro.
- Cerca de 8 tarefas para o usuário demo, cobrindo os três status e pelo menos uma atrasada.

Migrations são aplicadas automaticamente no startup só em Development (`Database.MigrateAsync()`); em produção, o README explica o uso de `dotnet ef migrations bundle`. Isso mostra que você conhece a diferença entre conveniência de demo e prática de produção.

## Contrato da API

São duas APIs no mesmo host, cada uma com seu controller e seu documento OpenAPI (`auth-v1` e `tasks-v1`), visíveis separadamente no Scalar ou Swagger UI. Todas as respostas de erro seguem o padrão **ProblemDetails (RFC 9457)**, com o campo `errors` para falhas de validação.

Auth API — `/api/auth`:

| Verbo | Rota | Auth | Corpo / parâmetros | Sucesso | Erros |
| --- | --- | --- | --- | --- | --- |
| POST | `/api/auth/register` | público | `{ name, email, password }` | 201 + `{ id, name, email }` | 400 validação, 409 e-mail já existe |
| POST | `/api/auth/login` | público | `{ email, password }` | 200 + `{ accessToken, expiresAt, user }` | 400, 401 credenciais inválidas |
| GET | `/api/auth/me` | **JWT** | — | 200 + `{ id, name, email }` | 401 |
| GET | `/api/auth/status` | público | — | 200 + `{ status, version }` | — |

`/me` e `/status` são o par "endpoint autorizado e não autorizado" pedido pelo PDF, fáceis de demonstrar lado a lado.

Tasks API — `/api/tasks` (todas exigem JWT):

| Verbo | Rota | Corpo / parâmetros | Sucesso | Erros |
| --- | --- | --- | --- | --- |
| GET | `/api/tasks` | query `status`, `page`, `pageSize` (máx. 50) | 200 + `{ items, page, pageSize, totalCount }` | 400, 401 |
| GET | `/api/tasks/{id}` | id `guid` na rota | 200 + `TaskResponse` | 401, 404 |
| POST | `/api/tasks` | `{ title, description, dueDate, status? }` | 201 + `Location` + `TaskResponse` | 400, 401 |
| PUT | `/api/tasks/{id}` | `{ title, description, dueDate, status }` (substituição completa) | 200 + `TaskResponse` | 400, 401, 404 |
| DELETE | `/api/tasks/{id}` | — | 204 | 401, 404 |

`TaskResponse` = `{ id, title, description, status, dueDate, isOverdue, createdAt, updatedAt }`. Nunca retorne a entidade nem o `userId` — DTOs isolam o contrato do modelo interno.

Autenticação e segurança:

- JWT Bearer assinado com HMAC-SHA256, chave lida de variável de ambiente (`Jwt__Key`), nunca do código. Expiração de 60 minutos; claims `sub` (id do usuário), `email`, `name`.
- `ICurrentUser` lê o `sub` do `HttpContext` e é injetado nos serviços — o `userId` nunca vem do corpo da requisição.
- Senha com `PasswordHasher<T>` do ASP.NET Core Identity (PBKDF2 com salt), atrás da interface `IPasswordHasher` da Application.
- Política de senha: mínimo 8 caracteres, com letra e número.
- Login com mensagem genérica ("Invalid email or password") para não revelar quais e-mails existem.
- Tarefa de outro usuário responde 404, não 403, pelo mesmo motivo.
- Rate limiting nativo (`AddRateLimiter`) no endpoint de login — melhoria de baixo custo e boa para comentar.
- CORS restrito à origem do frontend.

Refresh token fica explicitamente fora do escopo, registrado como "Future improvement". É uma decisão consciente de simplicidade que você deve mencionar antes que perguntem.

## Design patterns e boas práticas

Use poucos padrões, todos com motivo claro. A tabela abaixo é praticamente o seu roteiro de respostas na code review.

| Padrão | Onde | Por que |
| --- | --- | --- |
| Dependency Inversion / Ports & Adapters | Interfaces na Application, implementações na Infrastructure | Regras de negócio testáveis sem banco; Application independente da camada de dados, como o PDF exige |
| Repository | `ITaskRepository`, `IUserRepository` — específicos, sem repositório genérico | É a camada de acesso a dados pedida no PDF; mantém EF Core fora da Application, permite testar casos de uso com mocks e centraliza o filtro por usuário |
| Unit of Work | O próprio `AppDbContext`, sem interface `IUnitOfWork` | O EF Core já implementa o padrão; cada escrita do repositório chama `SaveChangesAsync`, e cada caso de uso altera um único agregado |
| Rich Domain Model + Factory Method | `TaskItem.Create(...)`, `task.Update(...)`, `task.ChangeStatus(...)` | Setters privados; objeto nunca existe em estado inválido |
| Result pattern | Retorno dos serviços: `Result<T>` com `Error` tipado (Validation, NotFound, Conflict, Unauthorized) | Falhas esperadas sem exceções; o controller traduz para HTTP num único lugar |
| DTO / Mapping manual | `TaskResponse.FromEntity(task)` | Contrato explícito, sem mágica de mapeamento |
| Options pattern | `JwtOptions` com validação no startup (`ValidateOnStart`) | Configuração tipada; app não sobe com chave JWT ausente |
| Global exception handler | `IExceptionHandler` + `AddProblemDetails()` | Erro inesperado vira 500 em ProblemDetails, sem stack trace vazando |

Como os erros fluem: entrada inválida é barrada pelo validador (FluentValidation) na Application; regra de negócio violada retorna `Result` com erro; invariante quebrada dentro do Domain lança `DomainException` (última linha de defesa), convertida em 400 pelo handler global.

Boas práticas de código que o avaliador vai notar:

- `Directory.Build.props` com `<Nullable>enable</Nullable>`, `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` e analyzers do .NET ligados.
- `Directory.Packages.props` (Central Package Management) — uma versão por pacote em todo o repositório.
- `.editorconfig` + `dotnet format` verificado no CI.
- `async` de ponta a ponta com `CancellationToken` repassado até o EF Core.
- `TimeProvider` injetado em vez de `DateTime.UtcNow` — torna a regra "data não pode estar no passado" testável.
- `sealed` em classes que não foram pensadas para herança; `record` para DTOs.
- Logs estruturados com `ILogger` (nunca logar senha nem token).
- Health check `/health` que verifica o banco, usado pelo `docker compose`.

Bibliotecas — escolha e evite com consciência:

| Usar | Para | Observação |
| --- | --- | --- |
| FluentValidation | Validação de DTOs | Licença Apache 2.0 |
| Npgsql.EntityFrameworkCore.PostgreSQL | Provider EF Core | — |
| Microsoft.AspNetCore.Authentication.JwtBearer | Autenticação | Nativo |
| Scalar.AspNetCore | UI do OpenAPI | Com `Microsoft.AspNetCore.OpenApi` nativo |
| xUnit, NSubstitute, Shouldly | Testes | Ver seção de testes |

Evite MediatR, AutoMapper e FluentAssertions (v8+): os três passaram a exigir licença comercial para uso empresarial em 2025, e nenhum é necessário num projeto deste tamanho. Confirme o estado atual das licenças antes de escrever o ADR, e cite isso na apresentação — avaliar dependências por licença e custo é sinal de senioridade.

## Estratégia de testes

O objetivo é que cada critério de aceite da user story tenha um teste com o mesmo nome em linguagem de negócio, e que o histórico do Git prove o TDD. São seis projetos de teste, cada um com um papel claro.

Neste plano, **TDD sempre significa Test-Driven Development**. Os testes unitários são o produto direto do ciclo red-green-refactor de cada tarefa; os testes de integração verificam, com banco e HTTP reais, o que a seção 6 (Testing approach) de cada design doc prometeu.

| Projeto | Tipo | O que cobre | Ferramentas |
| --- | --- | --- | --- |
| `Domain.UnitTests` | Unitário | Invariantes de `TaskItem` e `User`, transições de status, `IsOverdue` | xUnit, Shouldly |
| `Application.UnitTests` | Unitário | Casos de uso e validadores: e-mail duplicado, propriedade da tarefa, login inválido | xUnit, NSubstitute, `FakeTimeProvider` |
| `Api.UnitTests` | Unitário | Controllers: mapeamento `Result` → status HTTP, `Location` no 201 | xUnit, NSubstitute |
| `Infrastructure.IntegrationTests` | Integração | Repositórios, mapeamentos EF, índice único de e-mail, filtro por usuário, migrations aplicam | Testcontainers.PostgreSql, Respawn |
| `Api.IntegrationTests` | Integração ponta a ponta | HTTP real: register → login → CRUD; 401 sem token; 404 na tarefa de outro usuário | `WebApplicationFactory`, Testcontainers |
| `ArchitectureTests` | Arquitetura | Domain não referencia nada; Application não referencia Infrastructure nem ASP.NET | NetArchTest.Rules |

O projeto de arquitetura é barato (meia hora) e muito eficaz na apresentação: ele **prova** a Clean Architecture em vez de só afirmá-la.

Como montar os testes de integração com Testcontainers:

1. Uma `PostgresFixture` implementa `IAsyncLifetime`, sobe um container `postgres:18-alpine` e aplica as migrations uma vez.
2. Uma *collection fixture* do xUnit compartilha o container entre as classes de teste — um container por execução, não um por teste.
3. `Respawn` limpa as tabelas entre testes (rápido, sem recriar o banco).
4. Na API, uma `CustomWebApplicationFactory` sobrescreve a connection string com a do container e uma chave JWT de teste.
5. Um helper `AuthenticateAsync()` registra e faz login de um usuário e devolve um `HttpClient` com o header `Authorization` pronto.

Pré-requisito: Docker rodando na máquina e no CI. O GitHub Actions com `ubuntu-latest` já tem Docker, então os testes de integração rodam no pipeline sem configuração extra.

Convenções:

- Nome: `Metodo_Cenario_ResultadoEsperado`, por exemplo `Create_WhenDueDateIsInThePast_ReturnsValidationError`.
- Estrutura Arrange / Act / Assert visível, um comportamento por teste.
- *Test data builders* (`TaskItemBuilder`) em vez de construtores repetidos.
- Cobertura com `coverlet.collector` + ReportGenerator; meta de 80% ou mais em Domain e Application. Coloque o resumo no README.

Como o TDD aparece no histórico:

```text
test(domain): TaskItem rejects due date in the past        <- vermelho
feat(domain): validate due date on TaskItem.Create         <- verde
refactor(domain): extract Guard helpers                    <- refatoração
```

Commits pequenos nesse ritmo são a evidência mais forte de TDD que existe — os avaliadores podem abrir o `git log` durante a review.

Frontend: testes de unidade com Vitest (runner padrão das versões recentes do Angular CLI) para services, guard, interceptor e o formulário de tarefa. Um teste E2E com Playwright cobrindo login → criar → editar → excluir é opcional, e só vale se sobrar tempo.

## Frontend Angular

Use a versão estável atual (Angular 22, lançada em junho de 2026) com Angular Material, e as práticas modernas que o CLI já gera por padrão: standalone components, signals, novo control flow (`@if`, `@for`) e aplicação sem Zone.js. Isso atende aos três critérios do PDF: responsivo, CRUD completo e código organizado.

Estrutura de pastas:

```text
frontend/src/app/
├── app.config.ts            # provideRouter, provideHttpClient(withInterceptors(...))
├── app.routes.ts            # rotas com loadComponent (lazy loading)
├── core/
│   ├── auth/
│   │   ├── auth.service.ts      # login, register, logout, currentUser (signal)
│   │   ├── auth.guard.ts        # CanActivateFn funcional
│   │   ├── auth.interceptor.ts  # adiciona Bearer token
│   │   └── token.storage.ts
│   ├── http/
│   │   └── error.interceptor.ts # 401 -> logout; ProblemDetails -> mensagem
│   └── models/                  # tipos espelhando os DTOs da API
├── shared/
│   └── ui/                      # status-badge, confirm-dialog, empty-state, loading
├── layout/
│   └── shell.component.ts       # toolbar + router-outlet
└── features/
    ├── auth/
    │   ├── login.page.ts
    │   └── register.page.ts
    └── tasks/
        ├── data-access/
        │   ├── tasks.api.ts     # só HTTP
        │   └── tasks.store.ts   # estado com signals
        ├── task-list.page.ts
        ├── task-form.page.ts    # criar e editar
        └── components/
            ├── task-card.component.ts
            └── task-filters.component.ts
```

Gerenciamento de estado: um *store* por feature, implementado como service com `signal()` e `computed()` — sem NgRx. `TasksStore` guarda `tasks`, `filter`, `loading` e `error`, expõe métodos `load()`, `create()`, `update()`, `remove()`, e os componentes só leem signals. `TasksApi` faz apenas HTTP. Essa separação (componente → store → api) é a resposta para "organize seus componentes e estado de forma limpa".

Telas:

1. **Login** e **Cadastro** — reactive forms tipados, mensagens de validação por campo, erro do servidor exibido.
2. **Lista de tarefas** — filtro por status, paginação, badge de status, destaque para atrasadas; tabela no desktop e cards no celular (`BreakpointObserver`).
3. **Formulário de tarefa** — mesmo componente para criar e editar, com datepicker e validação espelhando as regras do backend.
4. **Excluir** — diálogo de confirmação, depois snackbar de sucesso.

Boas práticas a seguir:

- `ChangeDetectionStrategy.OnPush` e `inject()` em todos os componentes.
- Interceptors e guards funcionais (`HttpInterceptorFn`, `CanActivateFn`).
- URL da API relativa (`/api`): em desenvolvimento via `proxy.conf.json` do `ng serve`, no Docker via proxy do nginx. Assim não há problema de CORS no navegador.
- Estados de carregamento, vazio e erro em todas as listas.
- Acessibilidade básica: `label` em todos os campos, contraste do tema Material, navegação por teclado.
- Lint com `angular-eslint` e formatação com Prettier.

Token: guarde em `sessionStorage` e registre a decisão em ADR, citando o trade-off (cookie `httpOnly` é mais seguro contra XSS, mas exige CSRF e mais configuração). Mostrar que você conhece o trade-off pesa mais do que a escolha em si.

Para zerar warnings no console (critério desejável do PDF): `track` em todo `@for`, build de produção no Docker, favicon presente, nenhuma requisição 404 ou CORS, nenhum erro `NG0` — confira com o DevTools aberto antes de gravar ou apresentar.

## Docker e execução local

Um único comando — `docker compose up --build` — deve subir tudo com dados de demonstração. O avaliador não deve precisar instalar .NET nem Node para ver o projeto funcionando.

Serviços do `docker-compose.yml`:

| Serviço | Imagem | Porta local | Detalhes |
| --- | --- | --- | --- |
| `db` | `postgres:18-alpine` | 5432 | Volume nomeado, healthcheck com `pg_isready` |
| `api` | Build de `backend/Dockerfile` | 5080 | `depends_on: db: condition: service_healthy`; healthcheck em `/health` |
| `web` | Build de `frontend/Dockerfile` | 4200 | nginx servindo o build e fazendo proxy de `/api` para `api:8080` |

Esboço do compose (referência para a implementação, não código final):

```yaml
services:
  db:
    image: postgres:18-alpine
    environment:
      POSTGRES_DB: taskmanager
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
    volumes: [pgdata:/var/lib/postgresql]
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U ${POSTGRES_USER} -d taskmanager"]
      interval: 5s
      retries: 10
  api:
    build: ./backend
    environment:
      ASPNETCORE_ENVIRONMENT: Development
      ConnectionStrings__Default: Host=db;Database=taskmanager;Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}
      Jwt__Key: ${JWT_KEY}
    ports: ["5080:8080"]
    depends_on:
      db: { condition: service_healthy }
  web:
    build: ./frontend
    ports: ["4200:80"]
    depends_on: [api]
volumes:
  pgdata:
```

Dockerfiles:

- **Backend** — multi-stage: `mcr.microsoft.com/dotnet/sdk:10.0` para `restore` e `publish`, `mcr.microsoft.com/dotnet/aspnet:10.0` para rodar. As imagens oficiais já trazem o usuário não-root `app`; use `USER app`. Copie primeiro os `.csproj` e restaure, depois o código — o cache de camadas acelera rebuilds.
- **Frontend** — multi-stage: `node:22-alpine` executa `npm ci` e `ng build`; `nginx:alpine` serve `dist/` com `try_files $uri /index.html` (rotas do SPA) e `location /api/ { proxy_pass http://api:8080; }`.
- `.dockerignore` nos dois (exclua `bin/`, `obj/`, `node_modules/`, `.angular/`).

Variáveis e segredos:

- `.env.example` versionado com valores de demo; `.env` no `.gitignore`. O README manda copiar um para o outro.
- A chave JWT de demo deve ter pelo menos 32 caracteres e nunca aparecer em `appsettings.json`.

Desenvolvimento sem Docker completo: `docker compose up db` para só o banco, `dotnet run` na Api e `ng serve` com proxy no frontend. Documente os dois modos no README.

Confirme a tag de imagem do Node (versão LTS ativa) no momento de implementar; as tags do .NET 10 e do PostgreSQL 18 são estáveis.

## ADRs (Architecture Decision Records)

Escreva cerca de dez ADRs curtos (meia página cada) em `docs/adr/`. Eles servem a três propósitos: guiam os agentes de IA (que leem as decisões antes de codar), deixam as tarefas independentes coerentes entre si, e viram o roteiro da parte "design choices" da apresentação.

| Nº | Título | Decisão |
| --- | --- | --- |
| 0001 | Record architecture decisions | Usar ADRs no formato MADR simplificado |
| 0002 | Clean Architecture with four projects | Domain, Application, Infrastructure, Api; dependências para dentro; repositórios específicos como camada de acesso a dados |
| 0003 | PostgreSQL with EF Core | PostgreSQL em container; modelo relacional e migrations versionadas |
| 0004 | Two APIs in a single host | Auth API e Tasks API como controllers e documentos OpenAPI separados; dois hosts seria custo sem benefício neste escopo |
| 0005 | JWT bearer authentication | JWT de curta duração, PBKDF2, sem refresh token (escopo) |
| 0006 | Result pattern for expected failures | `Result<T>` na Application; exceções só para o inesperado |
| 0007 | No MediatR, AutoMapper, FluentAssertions or IUnitOfWork | Serviços explícitos, mapeamento manual e DbContext como unidade de trabalho; licenças comerciais e complexidade desnecessária |
| 0008 | Testing strategy | Pirâmide: unitários + integração com Testcontainers; sem EF InMemory |
| 0009 | Angular with signal-based stores | Signals em vez de NgRx; Angular Material para responsividade |
| 0010 | Token storage in the browser | `sessionStorage`, com trade-offs documentados |
| 0011 | Spec-driven, AI-assisted workflow | SDD com design docs, CLAUDE.md, subagentes, worktrees, revisão humana obrigatória |

Modelo de cada ADR (`docs/adr/0000-template.md`):

```markdown
# NNNN — Título curto no imperativo

- Status: Accepted
- Date: 2026-10-24

## Context
Qual problema ou força motivou a decisão (3 a 5 linhas).

## Decision
O que foi decidido, em uma ou duas frases.

## Alternatives considered
- Alternativa A — por que não.
- Alternativa B — por que não.

## Consequences
- Positivas: ...
- Negativas / trade-offs aceitos: ...
```

Regra prática: se você não consegue preencher "Alternatives considered", a decisão provavelmente não precisa de ADR. E escreva os ADRs **antes** de implementar a parte correspondente — é isso que os torna úteis para os agentes, e não documentação retroativa.

## Backlog em fases com tarefas independentes

O segredo para paralelizar é a **Fase 0 de contratos**: depois que as interfaces, DTOs e o formato das entidades estão no `main`, cada trilha (domínio, infraestrutura, API, frontend) trabalha contra esses contratos sem esperar as outras. São 5 fases e 22 tarefas; cada tarefa cabe em uma sessão de trabalho e termina com testes verdes e um PR.

| ID | Fase | Tarefa | Depende de | Agente |
| --- | --- | --- | --- | --- |
| T00 | 0 Fundação | Solution, projetos, referências, `Directory.Build.props`, CPM, `.gitignore`, CI (build + test) | — | orquestrador |
| T01 | 0 Fundação | `spec/user-story.md`, design docs DD-01 a DD-04, ADRs 0001–0011, `CLAUDE.md`, pasta `.claude/` | T00 | você + orquestrador |
| T02 | 0 Fundação | Contratos: interfaces da Application, DTOs, `Result`/`Error`, formato das entidades, `docs/api.md` | T01 | orquestrador |
| T03 | 0 Fundação | Testes de arquitetura (NetArchTest) | T02 | test-engineer |
| T10 | 1 Núcleo | Domain: comportamento de `TaskItem` e `User` com TDD | T02 | backend-domain |
| T11 | 1 Núcleo | Infra base: `AppDbContext`, configurações EF, migration inicial, `PostgresFixture` | T02 | backend-infra |
| T12 | 1 Núcleo | Frontend: app Angular, Material, rotas, shell, proxy, modelos | T02 | frontend |
| T20 | 2 Casos de uso | `AuthService` + validadores (TDD com mocks) | T10 | backend-app |
| T21 | 2 Casos de uso | `TaskService` + validadores, regra de propriedade | T10 | backend-app |
| T22 | 2 Casos de uso | Repositórios (DbContext como unidade de trabalho) + testes com Testcontainers | T11 | backend-infra |
| T23 | 2 Casos de uso | JWT, hash de senha, seed de demo | T11 | backend-infra |
| T24 | 2 Casos de uso | Frontend auth: login, cadastro, interceptor, guard | T12 | frontend |
| T30 | 3 API | `Program.cs`: DI, auth, ProblemDetails, dois documentos OpenAPI, CORS, health, rate limit | T20–T23 | backend-api |
| T31 | 3 API | `AuthController` + testes unitários | T30 | backend-api |
| T32 | 3 API | `TasksController` + testes unitários | T30 | backend-api |
| T33 | 3 API | Testes de integração ponta a ponta da API | T31, T32 | test-engineer |
| T34 | 3 API | Frontend tasks: lista, filtros, formulário, exclusão, responsivo | T24, T32 | frontend |
| T40 | 4 Entrega | Dockerfiles da api e web, compose completo, smoke test | T33, T34 | orquestrador |
| T41 | 4 Entrega | README, `architecture.md` com diagrama, relatório de cobertura | T40 | docs-writer |
| T42 | 4 Entrega | Entregável de GenAI em `docs/genai/` | T33 | você + docs-writer |
| T43 | 4 Entrega | Revisão final: code-reviewer, checklist de requisitos, console limpo | T41, T42 | code-reviewer + você |
| T44 | 4 Entrega | Ensaio da apresentação e gravação de backup da demo | T43 | você |

Na Fase 1 rodam 3 trilhas em paralelo; na Fase 2, até 4. Fases 0 e 4 são sequenciais por natureza: definem e integram.

Pela ótica do SDD, este backlog é a etapa Tasks: cada tarefa nasce da seção 8 de um design doc e é implementada com Test-Driven Development. DD-01 gera T20, T23 e T31; DD-02 gera T10, T11, T21, T22 e T32; DD-03 gera T12, T24 e T34; DD-04 gera T00, T03, T33 e T40.

Cada tarefa tem um arquivo próprio em `docs/tasks/` — é a especificação que você entrega a um agente. Modelo (`docs/tasks/T21-task-service.md`):

```markdown
# T21 — TaskService (Application)

## Goal
Implement CRUD use cases for tasks, enforcing ownership.

## Context
- Spec: docs/spec/user-story.md — acceptance criteria 3, 4, 5, 7
- Design: docs/design/DD-02-task-management.md — sections 3, 5 and 6
- ADRs: ADR-0002, ADR-0006
- Contracts: ITaskRepository, ICurrentUser (already on main)

## Scope — files you may create or change
- backend/src/TaskManager.Application/Tasks/**
- backend/tests/TaskManager.Application.UnitTests/Tasks/**

## Out of scope
- Controllers, EF Core, anything in Infrastructure or Api

## Tests to write first (red) — from DD-02, section 6
- Create_WhenTitleIsEmpty_ReturnsValidationError
- GetById_WhenTaskBelongsToAnotherUser_ReturnsNotFound
- List_FiltersByStatus_AndPaginates
- ...

## Definition of done
- [ ] All tests above written first, then passing (Test-Driven Development)
- [ ] dotnet build with zero warnings; dotnet format clean
- [ ] Architecture tests still green
- [ ] Conventional commits: test → feat → refactor
- [ ] PR description lists what was done and any decision taken
```

A seção **Scope** é o que evita conflitos de merge entre agentes paralelos: duas tarefas da mesma fase nunca podem listar os mesmos arquivos.

## Orquestração de múltiplos agentes

O modelo recomendado é **uma sessão orquestradora + subagentes especializados, cada um em seu próprio git worktree**, com você aprovando todo merge. A sessão principal do Claude Code é a orquestradora: lê o backlog da fase, delega cada tarefa a um subagente, e os subagentes trabalham isolados em branches separadas sem pisar no código uns dos outros.

Três níveis possíveis, do mais simples ao mais automatizado:

1. **Sessões paralelas manuais** — você abre 2 ou 3 sessões do Claude Code, cada uma num worktree (`claude --worktree`), e entrega um arquivo de `docs/tasks/` a cada uma. Você é o orquestrador. Mais controle, ideal para começar.
2. **Orquestrador + subagentes** (recomendado a partir da Fase 1) — a sessão principal dispara os subagentes definidos em `.claude/agents/`, com `isolation: worktree`, em background. Cada um devolve um resumo; o `code-reviewer` revisa; você faz o merge.
3. **Agent teams** — recurso do Claude Code em que várias sessões se coordenam por uma lista de tarefas compartilhada. Poderoso, mas mais difícil de controlar; não dependa dele neste projeto. Cite como algo que você avaliou.

Comece pelo nível 1 na Fase 0, passe para o nível 2 nas Fases 1 a 3, e volte ao controle manual na Fase 4.

Agentes sugeridos:

| Agente | Responsabilidade | Ferramentas | Isolamento |
| --- | --- | --- | --- |
| orquestrador (sessão principal) | Lê a fase, delega, integra, resolve arquivos compartilhados | Todas | checkout principal |
| `backend-domain` | Entidades e invariantes com TDD | Read, Edit, Write, Bash, Grep, Glob | worktree |
| `backend-app` | Casos de uso, validadores, testes com mocks | idem | worktree |
| `backend-infra` | EF Core, repositórios, JWT, seed, Testcontainers | idem | worktree |
| `backend-api` | Controllers, configuração do host, testes de controller | idem | worktree |
| `frontend` | Angular: features, store, componentes, testes | idem | worktree |
| `test-engineer` | Testes de arquitetura e de integração ponta a ponta | idem | worktree |
| `code-reviewer` | Revisão adversarial do diff contra ADRs e Definition of Done | Read, Grep, Glob, Bash (só leitura) | não precisa |
| `docs-writer` | README, `architecture.md`, documentação da API | Read, Edit, Write, Grep, Glob | worktree |

Ciclo de cada fase:

1. Você revisa e ajusta os arquivos `docs/tasks/` da fase (gerados a partir da seção 8 dos design docs — eles são o contrato com os agentes).
2. O orquestrador dispara um subagente por tarefa, em paralelo, cada um em seu worktree e branch (`feat/T21-task-service`).
3. Cada subagente segue o ciclo TDD: escreve o teste que falha → commit `test:` → implementa → `dotnet test` verde → commit `feat:` → refatora → commit `refactor:`.
4. O `code-reviewer` analisa o diff de cada branch: arquitetura, segurança, nomes, testes faltando. Achados críticos voltam para o agente original corrigir.
5. **Você** lê o diff e faz o merge no `main`. Nenhum agente faz merge ou push.
6. CI verde no `main` libera a próxima fase.

Exemplo de prompt para o orquestrador iniciar a Fase 2:

```text
We are starting Phase 2. Read docs/tasks/T20, T21, T22, T23 and the ADRs they reference.
For each task, spawn the agent named in the backlog table, in the background, with
isolation in its own worktree and branch feat/<task-id>-<slug>. Give each agent only
its task file path and tell it to follow the TDD cycle and the Definition of Done.
Do not edit Program.cs, DependencyInjection.cs or the .sln yourself until all four finish.
When they finish, run the code-reviewer on each branch and give me one summary table:
task, branch, tests added, review findings, ready to merge (yes/no).
```

Cuidados que evitam os problemas mais comuns:

- **Arquivos compartilhados** (`Program.cs`, `DependencyInjection.cs`, `.sln`, `app.routes.ts`) ficam reservados ao orquestrador, que os ajusta depois dos merges.
- **Subagentes não veem a conversa principal.** Tudo o que precisam tem que estar no arquivo da tarefa, nos ADRs e no `CLAUDE.md`.
- **Não paralelize demais.** Três ou quatro agentes simultâneos já é o limite do que você consegue revisar com qualidade.
- **`.worktreeinclude`** na raiz copia o `.env` para cada worktree novo (worktrees não levam arquivos ignorados pelo Git).
- **Testcontainers em paralelo** funciona: cada execução sobe seu próprio container em porta aleatória.
- **Custo:** cada subagente consome uso do seu plano; agentes de implementação podem usar um modelo mais leve, e o revisor um mais capaz.

Na apresentação, mostre a pasta `.claude/agents/`, um arquivo de `docs/tasks/`, e um PR com a sequência de commits `test → feat → refactor`. É a demonstração mais concreta possível de "fluência com ferramentas de GenAI".

## Organização dos arquivos da LLM no repositório

A regra que organiza tudo: **`CLAUDE.md` diz como trabalhar, `docs/` diz o que construir, `.claude/` configura a ferramenta.** Quase tudo é versionado no Git, porque faz parte do projeto e será mostrado na apresentação; só o que é pessoal fica no `.gitignore`.

```text
task-manager/
├── CLAUDE.md                  # versionado — instruções do projeto, carregado em toda sessão
├── CLAUDE.local.md            # .gitignore — suas preferências pessoais (opcional)
├── .worktreeinclude           # versionado — arquivos ignorados a copiar para worktrees (.env)
├── .mcp.json                  # versionado — servidores MCP do projeto (opcional)
├── docs/
│   ├── spec/                  # SDD: o quê e por quê — fonte da verdade do escopo
│   ├── design/                # Technical Design Documents — como cada funcionalidade é feita
│   ├── adr/                   # decisões — lidas pelos agentes antes de codar
│   ├── tasks/                 # SDD: tarefas — o "contrato" de cada agente
│   └── genai/                 # entregável da seção GenAI do PDF
└── .claude/
    ├── settings.json          # versionado — permissões e hooks do time
    ├── settings.local.json    # .gitignore — suas permissões extras
    ├── rules/                 # instruções que carregam só para certos caminhos
    │   ├── dotnet.md          # paths: backend/**
    │   ├── dotnet-tests.md    # paths: backend/tests/** (convenções de Test-Driven Development)
    │   └── angular.md         # paths: frontend/**
    ├── agents/                # um arquivo .md por subagente
    │   ├── backend-domain.md
    │   ├── backend-app.md
    │   ├── backend-infra.md
    │   ├── backend-api.md
    │   ├── frontend.md
    │   ├── test-engineer.md
    │   ├── code-reviewer.md
    │   └── docs-writer.md
    └── skills/                # fluxos reutilizáveis, chamados com /nome
        ├── new-design-doc/SKILL.md     # SDD etapa 2: cria um DD a partir da spec
        ├── break-into-tasks/SKILL.md   # SDD etapa 3: gera docs/tasks a partir de um DD
        ├── start-task/SKILL.md         # SDD etapa 4: inicia uma tarefa
        ├── tdd-cycle/SKILL.md          # Test-Driven Development: red, green, refactor
        ├── new-adr/SKILL.md
        └── pre-submit-check/SKILL.md
```

O que vai em cada lugar:

| Arquivo | Quando carrega | O que colocar |
| --- | --- | --- |
| `CLAUDE.md` | Início de toda sessão e de todo subagente | Visão geral, comandos de build e teste, regras de arquitetura, fluxo TDD, Git. Menos de 200 linhas |
| `.claude/rules/*.md` | Quando a IA lê um arquivo que casa com o `paths:` | Convenções específicas de .NET, testes ou Angular, sem poluir o contexto do outro lado |
| `.claude/agents/*.md` | Quando o subagente é acionado | Papel, ferramentas permitidas, isolamento, passo a passo do trabalho |
| `.claude/skills/*/SKILL.md` | Quando você digita `/nome` ou a IA identifica a tarefa | Procedimentos repetidos: iniciar tarefa, criar ADR, checklist de entrega |
| `.claude/settings.json` | Sempre, e é **aplicado** (não é sugestão) | Comandos permitidos e proibidos, hooks |

Diferença importante: `CLAUDE.md` e `rules/` são orientações que o modelo lê e normalmente segue; `settings.json` (permissões e hooks) é imposto pela ferramenta. O que não pode acontecer de jeito nenhum (push, apagar pastas, ler `.env`) vai em `settings.json`.

Exemplo de `CLAUDE.md` (escreva em inglês):

```markdown
# Task Manager — project instructions

Full-stack task manager for a technical interview. Clarity beats cleverness:
every change must be easy to explain in a code review.

## Read before coding (spec-driven)
- docs/spec/user-story.md — what and why: scope and acceptance criteria
- docs/design/ — the design doc of your feature: how it is built
- docs/adr/ — accepted decisions; do not contradict them without a new ADR
- The task file in docs/tasks/ you were given — your exact scope

## Commands
- Build: `dotnet build backend/TaskManager.sln`
- Unit tests: `dotnet test backend --filter "Category!=Integration"`
- All tests (Docker required): `dotnet test backend`
- Format: `dotnet format backend/TaskManager.sln`
- Frontend: `cd frontend && npm test` / `npm run lint` / `npm run build`
- Full stack: `docker compose up --build`

## Architecture rules
- Domain depends on nothing. Application depends only on Domain.
- Infrastructure and Api depend on Application. Never reference EF Core
  or ASP.NET from Domain or Application.
- Expected failures return Result<T>; exceptions only for the unexpected.
- Never return entities from controllers; use DTOs.
- Every task query filters by the current user inside the repository.

## Workflow
- Spec-driven: no code without a task file; no task without a design doc.
- In this repo "TDD" always means Test-Driven Development. Technical design
  documents are called design docs (DD-xx).
- Test-Driven Development: failing test → commit `test:` → make it pass →
  commit `feat:` → refactor.
- Conventional Commits. Small commits. Zero build warnings.
- Touch only files listed in your task's Scope section.
- If the design doc is wrong or incomplete, stop and report; do not improvise.
- Never push, never merge, never edit .env.

## Done means
- Tests written first and green, architecture tests green,
  `dotnet format` clean, and a short summary of what changed and why.
```

Exemplo de subagente (`.claude/agents/backend-app.md`):

```markdown
---
name: backend-app
description: Implements Application-layer use cases and validators with Test-Driven Development. Use for docs/tasks files that target TaskManager.Application.
tools: Read, Edit, Write, Bash, Grep, Glob
isolation: worktree
skills:
  - tdd-cycle
---

You implement exactly one task file from docs/tasks/, given in your prompt.

1. Read the task file, the design doc sections and ADRs it lists, and the
   acceptance criteria it cites in docs/spec/user-story.md.
2. For each test listed under "Tests to write first": write it, run it, see it fail,
   commit with `test(app): ...`.
3. Write the minimum code to pass. Run
   `dotnet test backend/tests/TaskManager.Application.UnitTests`. Commit `feat(app): ...`.
4. Refactor with tests green. Commit `refactor(app): ...` if anything changed.
5. Never edit files outside the task's Scope. If you need a contract change or the
   design doc does not cover a case, stop and report it instead of deciding alone.
6. Finish with: files changed, tests added, decisions taken, open questions.
```

Exemplo de revisor (`.claude/agents/code-reviewer.md`):

```markdown
---
name: code-reviewer
description: Reviews a branch diff against the ADRs, CLAUDE.md and the task's Definition of Done. Use after any implementation task finishes.
tools: Read, Grep, Glob, Bash
---

You are a strict senior .NET and Angular reviewer. You never edit files.
Run `git diff main...HEAD` and review for: layer violations, missing ownership
checks, missing or weak tests, security (secrets, auth, input validation),
naming and readability, warnings. Report findings as Critical / Should fix /
Suggestion, each with file, line and a concrete fix.
```

Exemplo de skill (`.claude/skills/start-task/SKILL.md`), usado como `/start-task T21`:

```markdown
---
description: Starts a backlog task from docs/tasks by its ID, following TDD and the task's Definition of Done
disable-model-invocation: true
argument-hint: <task-id>
---

Open the file in docs/tasks/ whose name starts with $ARGUMENTS.
Summarize the goal, scope and tests to write first, then ask me to confirm
before writing any code. After confirmation, follow the tdd-cycle skill.
```

Exemplo de `.claude/settings.json`:

```json
{
  "permissions": {
    "allow": [
      "Bash(dotnet build *)", "Bash(dotnet test *)", "Bash(dotnet format *)",
      "Bash(dotnet ef *)", "Bash(npm run *)", "Bash(npm test *)",
      "Bash(docker compose *)", "Bash(git status)", "Bash(git diff *)",
      "Bash(git log *)", "Bash(git add *)", "Bash(git commit *)"
    ],
    "deny": [
      "Bash(git push *)", "Bash(rm -rf *)",
      "Read(./.env)", "Read(./**/.env)"
    ]
  }
}
```

Comece só com `CLAUDE.md` e `settings.json` na Fase 0. Adicione os agentes antes da Fase 1 e as skills quando perceber que está repetindo a mesma instrução — não crie tudo de uma vez. A documentação oficial tem um explorador interativo dessa estrutura: [Explore the .claude directory](https://code.claude.com/docs/en/claude-directory).

## Seção GenAI do PDF

Como o projeto principal já é um task manager, a seção GenAI vira um **registro real** do que aconteceu durante o desenvolvimento, não um exercício separado. Isso é muito mais convincente para o painel. Organize em `docs/genai/`:

| Arquivo | Conteúdo | Pedido do PDF atendido |
| --- | --- | --- |
| `01-prompt.md` | O prompt usado para gerar o scaffold da Tasks API, com explicação de cada bloco | "write the prompt you would use" |
| `02-output-sample.md` | Trecho representativo da **primeira saída da IA, sem edição** (controller + service) | "show the output code" |
| `03-review-log.md` | Tabela: problema encontrado → como detectou → correção → commit | "validated / corrected" |
| `04-edge-cases.md` | Como tratou autenticação, autorização, validação e casos-limite | "edge cases, authentication, validations" |
| `05-workflow.md` | SDD (spec → design docs → tarefas), CLAUDE.md, subagentes, worktrees, revisão adversarial — o processo | Fluência com ferramentas |

Prompt sugerido para `01-prompt.md` (estrutura: papel, contexto, restrições, critérios, formato de saída):

```text
You are a senior .NET engineer. Generate a RESTful API for a task management system.

Context
- .NET 10, ASP.NET Core Web API with controllers, EF Core with PostgreSQL.
- Clean Architecture: Domain, Application, Infrastructure, Api projects.
- A User entity and JWT authentication already exist; the current user id
  comes from the `sub` claim via an ICurrentUser service.

Requirements
- TaskItem: Id (Guid), Title, Description, Status (Todo | InProgress | Done),
  DueDate, UserId, CreatedAt, UpdatedAt.
- Endpoints: GET /api/tasks (filter by status, paginated), GET /api/tasks/{id},
  POST /api/tasks, PUT /api/tasks/{id}, DELETE /api/tasks/{id}. All require auth.
- A user can only read or change their own tasks; another user's task returns 404.

Constraints
- Title required, 3-100 chars; description max 1000; due date not in the past on create.
- Return DTOs, never entities. Errors as ProblemDetails (RFC 9457).
- Correct status codes: 201 with Location on create, 204 on delete.
- Use TimeProvider, not DateTime.Now. Pass CancellationToken end to end.
- Do not use MediatR or AutoMapper.

Output
1. The files, grouped by project, with full paths.
2. xUnit tests for the service, written so they would fail without the implementation.
3. A list of assumptions you made and anything you were unsure about.
```

Cada bloco tem uma razão que você explica na apresentação: o **contexto** evita que a IA invente uma arquitetura; as **restrições** atacam exatamente os erros que IAs costumam cometer; o item 3 da **saída** força a IA a expor incertezas em vez de escondê-las.

Como validar a saída da IA (o processo, não só o resultado):

1. **Compila com zero warnings** (`TreatWarningsAsErrors`) — pega APIs inventadas ou obsoletas.
2. **Os testes escritos antes passam** — no TDD, o teste é a especificação, e a IA tem que satisfazê-la.
3. **Testes de arquitetura passam** — pega dependências na direção errada.
4. **Revisão adversarial** pelo subagente `code-reviewer` e depois por você, com checklist de segurança.
5. **Teste manual** no Scalar e no frontend, incluindo os caminhos de erro.

Checklist de falhas típicas de código gerado por IA — verifique cada uma e registre no `03-review-log.md` **apenas as que realmente aconteceram**:

- Consulta sem filtro por usuário (falha de autorização do tipo IDOR) ou `userId` vindo do corpo da requisição.
- Entidade retornada direto pelo controller, expondo campos internos.
- `DateTime.Now` em vez de `TimeProvider`/UTC; datas comparadas com fuso errado.
- 200 no lugar de 201/204; ausência do header `Location`.
- Listagem sem paginação.
- Hash de senha fraco ou chave JWT escrita no `appsettings.json`.
- Pacotes ou APIs desatualizados (por exemplo, pacote de Swagger antigo em vez do OpenAPI nativo).
- `catch (Exception)` engolindo erros; ausência de `CancellationToken`.
- Testes que testam o mock, não o comportamento.

Honestidade aqui conta muito: um review log com 4 problemas reais e bem explicados vale mais que uma lista de 15 itens genéricos. Se o painel perguntar "o que a IA errou?", você responde com exemplos concretos do seu próprio repositório.

## README e checklist de entrega

O README é a primeira coisa que o avaliador abre. Ele precisa permitir rodar o projeto em menos de 5 minutos e entender a arquitetura em menos de 2. Estrutura sugerida:

1. **Título + uma frase** do que é, e uma captura de tela da lista de tarefas.
2. **Quick start** — pré-requisitos (Docker), `cp .env.example .env`, `docker compose up --build`, URLs (app em `localhost:4200`, API e documentação em `localhost:5080/scalar`).
3. **Demo credentials** — os dois usuários seed, em destaque.
4. **Spec e design** — resumo com links para `docs/spec/` e `docs/design/`.
5. **Architecture** — diagrama das camadas, uma linha por projeto, link para os ADRs.
6. **API** — tabela de endpoints (a da seção de contrato) e como autenticar no Scalar.
7. **Running tests** — comandos de unitários e integração (com nota sobre Docker), resumo de cobertura.
8. **Local development without Docker** — banco no container, API e Angular rodando localmente.
9. **AI-assisted development** — link para `docs/genai/` e explicação curta da pasta `.claude/`.
10. **Requirements traceability** — a tabela de requisitos deste plano.
11. **Future improvements** — refresh token, categorias, compartilhamento, observabilidade, deploy.

Checklist antes de enviar:

- [ ] Clone limpo em outra pasta + `docker compose up --build` funciona sem nenhum passo extra
- [ ] Login com o usuário demo funciona; o segundo usuário não vê as tarefas do primeiro
- [ ] CRUD completo no frontend, no desktop e na largura de celular (DevTools)
- [ ] Console do navegador sem erros nem warnings
- [ ] `dotnet build` sem warnings; todos os testes verdes, inclusive integração
- [ ] Testes de arquitetura verdes
- [ ] Nenhum segredo real no repositório (`.env` fora do Git; procure por "password" e "key" no histórico)
- [ ] Scalar mostra as duas APIs separadas, com autenticação funcionando
- [ ] Todos os ADRs com status Accepted e coerentes com o código
- [ ] `docs/genai/` completo, com exemplos reais
- [ ] Histórico de commits legível, mostrando `test → feat → refactor`
- [ ] CI verde no último commit, com badge no README
- [ ] Branches de agentes já mescladas ou removidas; nada de `WIP` no `main`

## Roteiro da apresentação

Planeje 20 minutos de apresentação e deixe o resto para a code review. A ordem segue exatamente o que o PDF diz que será avaliado.

| Bloco | Tempo | O que mostrar |
| --- | --- | --- |
| 1. Spec e design | 2 min | `docs/spec/user-story.md`: a história e os critérios, depois o DD-02 mostrando como um critério virou tarefa e teste |
| 2. Arquitetura | 4 min | Diagrama de camadas, estrutura da solução, um ADR (o das "duas APIs" ou o do banco) |
| 3. Demo ao vivo | 5 min | Login → lista → criar → editar → excluir → celular (DevTools) → login com o outro usuário → Scalar com 401 e 200 |
| 4. Código | 4 min | Um fluxo completo: `TasksController.Create` → `TaskService` → `TaskItem.Create` → repositório |
| 5. Testes | 3 min | Rodar `dotnet test`, mostrar Testcontainers subindo, teste de arquitetura, `git log` com TDD |
| 6. GenAI | 2 min | `CLAUDE.md`, um subagente, um arquivo de tarefa, o review log com um erro real que a IA cometeu |

Prepare-se para estas perguntas prováveis:

- **Por que não MediatR/CQRS?** Escopo pequeno; serviços explícitos são mais legíveis; licença comercial (ADR-0007).
- **Por que Testcontainers e não EF InMemory?** InMemory não é um banco relacional: não testa índice único, FK, SQL gerado nem migrations.
- **Como você garante que um usuário não acessa a tarefa de outro?** Filtro no repositório + `userId` só do token + teste de integração que prova o 404.
- **Por que 404 e não 403?** Não revelar a existência do recurso.
- **O que faria diferente em produção?** Refresh token, cookie `httpOnly`, migrations via bundle no pipeline, observabilidade (OpenTelemetry), secrets em cofre.
- **Onde a IA errou e como você percebeu?** Responda com um caso real do `03-review-log.md`.
- **Como os agentes não entraram em conflito?** Fase de contratos, escopo de arquivos por tarefa, worktrees isolados, merge humano.

Cuidados práticos:

- Suba o `docker compose` **antes** da chamada e deixe as abas abertas (app, Scalar, IDE, GitHub).
- Grave um vídeo de backup da demo de 3 minutos — se algo falhar ao vivo, você mostra o vídeo e segue com calma.
- Aumente a fonte da IDE e do terminal para o compartilhamento de tela.
- Ensaie em voz alta pelo menos duas vezes, cronometrando. A apresentação em inglês fica muito mais fluida depois do segundo ensaio.

## Cronograma sugerido

Com dedicação integral, o projeto cabe em **7 dias de trabalho**, incluindo ensaio. Se o prazo da vaga for menor, mantenha a ordem das fases e corte pela lista de prioridades abaixo.

&#91;embedded content: cronograma · 5 fases em 7 dias, 4 gates\]

O dia 1 é o mais importante: sem contratos e ADRs bem definidos, os agentes paralelos produzem código que não encaixa.

Se faltar tempo, corte nesta ordem (do primeiro a sair para o último):

1. Teste E2E com Playwright no frontend.
2. Concorrência otimista com `xmin` e rate limiting no login.
3. Orquestração automatizada (nível 2) — volte para sessões paralelas manuais.
4. Diagrama elaborado em `architecture.md` (use o do README).

Nunca corte: testes de domínio e aplicação, testes de integração com Testcontainers, regra de propriedade das tarefas, `docker compose` funcionando, seed com credenciais, README e o entregável de GenAI. São exatamente os itens que o PDF lista como critério de avaliação.

Uma última recomendação, considerando o momento que você está passando: trabalhe em blocos com pausas reais e reserve o dia 7 de verdade. Um projeto um pouco menor, entregue com calma e explicado com segurança, avalia muito melhor do que um projeto maior apresentado no limite do cansaço.

## Fontes

- [Explore the .claude directory — Claude Code docs](https://code.claude.com/docs/en/claude-directory)
- [Create custom subagents — Claude Code docs](https://code.claude.com/docs/en/sub-agents)
- [Angular version support timeline (v22 lançado em junho de 2026)](https://versionlog.com/angular/)

Versões de .NET 10, PostgreSQL 18 e o status das licenças de MediatR, AutoMapper e FluentAssertions foram citados sem consulta nesta sessão; confirme-os ao escrever os ADRs.
