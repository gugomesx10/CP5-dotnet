# 🎮 ArenaSync API — Checkpoint 5

API REST para gerenciamento de **times de e-sports, jogadores e perfis competitivos**, desenvolvida em **C# e ASP.NET Core (.NET 8)**. O CP5 amplia o CRUD do projeto com padronização de respostas, HATEOAS, autenticação JWT, integração com a OpenDota e testes automatizados.

## Informações acadêmicas

**Disciplina:** Advanced Business Development with .NET — FIAP (2026)  
**Entrega:** Checkpoint 5  
**Turma:** 2TDSPO

| Integrante | RM |
|---|---|
| Gustavo Gomes Martins | 555999 |
| Matheus de Mattos Vecchi | 561716 |
| Nicholas Albuquerque Buzo | 561082 |
| Nicholas Camillo Canadas de Paula | 561262 |

## Tecnologias

- **Back-end:** .NET 8, ASP.NET Core Web API e C#
- **Persistência:** Entity Framework Core 8 e Oracle Database
- **Arquitetura:** Domain, Application, Infrastructure e API; Repository Pattern; Services; DTOs; Dependency Injection
- **Segurança:** autenticação JWT Bearer e autorização por endpoint
- **Integração externa:** OpenDota API, `HttpClient` e tratamento de falhas HTTP
- **Documentação:** Swagger/OpenAPI, anotações Swagger e comentários XML
- **Qualidade:** xUnit, Moq e testes de integração com `WebApplicationFactory`
- **Observabilidade e proteção:** `ILogger<T>`, Application Insights opcional, Health Checks, Rate Limiting e Response Compression

## Organização da solução

```text
CP5.sln
├── CP5/                         # API, controllers, filtros e configuração
│   ├── Controllers/
│   ├── Exceptions/
│   ├── Filters/
│   └── Program.cs
├── CP5.Domain/                  # Entidades e exceções do domínio
├── CP5.Application/             # DTOs, contratos, regras e resultados
│   ├── Common/
│   ├── DTOs/
│   ├── Exceptions/
│   ├── Interfaces/
│   ├── Mappings/
│   ├── Results/
│   └── Services/
├── CP5.Infrastructure/          # Oracle/EF Core, repositories, serviços externos
│   ├── Data/
│   ├── ExternalServices/
│   └── Repositories/
├── CP5.UnitTests/               # Testes unitários
└── CP5.IntegrationTests/        # Testes via pipeline HTTP
```

**Fluxo principal:** `Requisição HTTP → Controller → Service → Repository → EF Core → Oracle`.

O controller traduz resultados de aplicação em respostas HTTP. O service concentra as regras de negócio; o repository isola a persistência. A integração com a OpenDota é implementada na Infrastructure e exposta por um controller próprio.

## Modelo de domínio

- **Time:** `Id`, `Nome`, `Jogo`, `Pais`, `Ranking`.
- **Jogador:** `Id`, `Nickname`, `Funcao`, `Idade`, `TimeId`.
- **PerfilCompetitivo:** `Id`, `KDA`, `WinRate`, `HorasJogadas`, `JogadorId`.

**Relacionamentos:** `Time 1:N Jogador` e `Jogador 1:1 PerfilCompetitivo`. A aplicação valida a associação dos recursos e impede que um jogador receba mais de um perfil competitivo.

## Funcionalidades do CP5

### Respostas padronizadas — `OperationResult<T>`

Os serviços retornam resultados tipados com indicação de sucesso, status, dados, mensagem e código de erro. O mapeamento de resposta HTTP fica centralizado em `OperationResultHttpExtensions`.

Exemplo de resposta de consulta bem-sucedida:

```json
{
  "isSuccess": true,
  "status": 0,
  "data": {
    "id": 1,
    "nome": "FURIA",
    "jogo": "CS2",
    "pais": "Brasil",
    "ranking": 3,
    "links": [
      { "rel": "self", "href": "/api/Time/1", "method": "GET" },
      { "rel": "update", "href": "/api/Time/1", "method": "PUT" },
      { "rel": "delete", "href": "/api/Time/1", "method": "DELETE" },
      { "rel": "jogadores", "href": "/api/Jogador/time/1", "method": "GET" }
    ]
  },
  "message": "Time encontrado com sucesso.",
  "errorCode": null
}
```

O serviço trata cenários conhecidos como entidade inexistente, falhas de banco e conflitos de negócio. Um handler global trata exceções inesperadas sem devolver detalhes internos da aplicação ao consumidor.

### HATEOAS

As respostas dos recursos incluem links de navegação para as operações disponíveis, incluindo `self`, `update`, `delete` e vínculos entre times, jogadores e perfis. Listagens paginadas incluem links como `self`, `previous` e `next` quando aplicáveis.

Exemplo: ao consultar um jogador, a resposta inclui links para o próprio jogador, seu time e a consulta de seu perfil competitivo.

### Autenticação e autorização JWT

O endpoint `POST /api/Auth/login` valida credenciais de demonstração **apenas no ambiente `Development`** e emite um JWT com prazo de validade. A API valida assinatura, emissor, audiência e expiração do token.

- **GET:** consultas públicas.
- **POST, PUT e DELETE:** exigem um Bearer token válido (`401 Unauthorized` sem autenticação).

Na interface Swagger, execute o login, copie o campo `token` e utilize **Authorize** antes de chamar os endpoints protegidos.

> **Escopo de demonstração:** o login por credenciais configuradas localmente não substitui um provedor de identidade ou gerenciamento seguro de usuários em produção.

### Integração externa — OpenDota

O endpoint `GET /api/OpenDota/teams/{id}` consulta informações públicas de equipes de Dota 2 usando `HttpClient`.

Exemplo testado pelo Swagger:

```http
GET /api/OpenDota/teams/2163
```

A resposta de sucesso contém dados como `team_id`, `name`, `tag`, `rating`, `wins` e `losses`, dentro de `OperationResult`. Erros de entrada, ausência da equipe e indisponibilidade do serviço externo são tratados com respostas apropriadas (400, 404 e 503, respectivamente).

### Paginação e filtros

Os endpoints de listagem recebem `pageNumber` e `pageSize`, com valores padrão **1** e **10**, e limite de **100** registros por página. A paginação utiliza consultas do Entity Framework Core (`CountAsync`, `OrderBy`, `Skip` e `Take`).

Há filtros por jogo, time e função do jogador. Uma página vazia pode resultar em **204 No Content**.

### Observabilidade e proteção da API

- **Rate Limiting:** máximo de 20 requisições por IP, por minuto; excesso retorna **429**.
- **Health Check:** `GET /health`, com verificações da API e conexão Oracle.
- **Response Compression:** compressão de respostas HTTP.
- **Logs:** `ILogger<T>` com mensagens estruturadas.
- **Application Insights:** habilitado quando há uma connection string de telemetria configurada; desativado no ambiente `Testing`.

## Endpoints

| Método | Rota | Objetivo | JWT |
|---|---|---|---|
| GET | `/api/Time` | Listar times | Não |
| GET | `/api/Time/{id}` | Buscar time | Não |
| GET | `/api/Time/jogo/{jogo}` | Filtrar times por jogo | Não |
| POST | `/api/Time` | Cadastrar time | Sim |
| PUT | `/api/Time/{id}` | Atualizar time | Sim |
| DELETE | `/api/Time/{id}` | Excluir time | Sim |
| GET | `/api/Jogador` | Listar jogadores | Não |
| GET | `/api/Jogador/{id}` | Buscar jogador | Não |
| GET | `/api/Jogador/time/{timeId}` | Filtrar por time | Não |
| GET | `/api/Jogador/funcao/{funcao}` | Filtrar por função | Não |
| POST | `/api/Jogador` | Cadastrar jogador | Sim |
| PUT | `/api/Jogador/{id}` | Atualizar jogador | Sim |
| DELETE | `/api/Jogador/{id}` | Excluir jogador | Sim |
| GET | `/api/PerfilCompetitivo` | Listar perfis | Não |
| GET | `/api/PerfilCompetitivo/{id}` | Buscar perfil | Não |
| GET | `/api/PerfilCompetitivo/jogador/{jogadorId}` | Buscar perfil por jogador | Não |
| POST | `/api/PerfilCompetitivo` | Cadastrar perfil | Sim |
| PUT | `/api/PerfilCompetitivo/{id}` | Atualizar perfil | Sim |
| DELETE | `/api/PerfilCompetitivo/{id}` | Excluir perfil | Sim |
| POST | `/api/Auth/login` | Gerar JWT de desenvolvimento | Não |
| GET | `/api/OpenDota/teams/{id}` | Consultar equipe externa | Não |
| GET | `/health` | Verificar saúde da aplicação | Não |

Listagens aceitam `?pageNumber=1&pageSize=10`.

## Exemplos de requisição

**Cadastrar time** — `POST /api/Time` (JWT necessário):

```json
{
  "nome": "FURIA",
  "jogo": "CS2",
  "pais": "Brasil",
  "ranking": 3
}
```

**Cadastrar jogador** — `POST /api/Jogador` (JWT necessário):

```json
{
  "nickname": "KSCERATO",
  "funcao": "Rifler",
  "idade": 25,
  "timeId": 1
}
```

**Cadastrar perfil competitivo** — `POST /api/PerfilCompetitivo` (JWT necessário):

```json
{
  "kda": 1.32,
  "winRate": 68,
  "horasJogadas": 12000,
  "jogadorId": 1
}
```

**Login de desenvolvimento** — `POST /api/Auth/login`:

```json
{
  "usuario": "USUARIO_DE_DEMONSTRACAO",
  "senha": "SENHA_CONFIGURADA_LOCALMENTE"
}
```

## Executando no Rider com Oracle da FIAP

1. Abra `CP5.sln` no JetBrains Rider e restaure os pacotes NuGet.
2. Configure **localmente** `CP5/appsettings.json` com a conexão Oracle da FIAP e os parâmetros JWT.
3. Selecione a configuração de execução `CP5: http` (ambiente `Development`).
4. Clique em **▶ Run** e abra o Swagger no endereço indicado pelo Rider (na configuração HTTP do projeto, `http://localhost:5105/swagger`).

**Não é necessário Docker para executar a API.** O `Program.cs` utiliza `ConnectionStrings:OracleConnection` para acessar o Oracle já existente.

### Exemplo de configuração local (sem valores reais)

> O arquivo `CP5/appsettings.json` contém dados sensíveis no ambiente de desenvolvimento e **não deve ser enviado ao repositório**. O exemplo abaixo usa exclusivamente marcadores substituíveis.

```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=USUARIO;Password=SENHA;Data Source=HOST:PORTA/SERVICO"
  },
  "Jwt": {
    "Key": "SUBSTITUA_POR_CHAVE_FORTE_COM_32_BYTES_OU_MAIS",
    "Issuer": "ArenaSync",
    "Audience": "ArenaSyncAPI"
  },
  "DemoLogin": {
    "Usuario": "USUARIO_DE_DESENVOLVIMENTO",
    "Senha": "SENHA_DE_DESENVOLVIMENTO"
  }
}
```

Preserve quaisquer outras seções do arquivo local, como logging. As credenciais de demonstração são destinadas apenas ao ambiente `Development`; em produção, devem ser substituídas por autenticação apropriada.

## Testes

O projeto contém testes unitários com **xUnit e Moq** para services, controllers, respostas padronizadas e integração externa simulada. Também há testes de integração com **`WebApplicationFactory`** que exercitam as rotas HTTP, HATEOAS e operações no Oracle.

Para rodar testes pelo Rider, utilize a janela **Unit Tests** e execute os testes do projeto desejado.

Alternativamente, em um ambiente com o SDK .NET 8:

```bash
dotnet build CP5.sln
dotnet test CP5.UnitTests/CP5.UnitTests.csproj
dotnet test CP5.IntegrationTests/CP5.IntegrationTests.csproj
```

**Isolamento de testes:** a suíte de integração configurada com Oracle exige uma conexão própria em `ARENASYNC_TEST_ORACLE_CONNECTION` e um schema exclusivo de testes (`CP5_TEST_...`). **Não utilize a conexão de produção ou o usuário normal da FIAP para executar operações de criação e exclusão dos testes.** Esse ambiente de testes é separado da execução habitual da API.

## Documentação e códigos HTTP

O Swagger está disponível no ambiente `Development`, com comentários XML e atributos de documentação dos controllers.

| HTTP | Significado |
|---|---|
| 200 | Consulta ou alteração bem-sucedida |
| 201 | Recurso criado |
| 204 | Sem conteúdo ou exclusão bem-sucedida |
| 400 | Dados/parâmetros inválidos |
| 401 | Token ausente ou inválido |
| 404 | Recurso não encontrado |
| 409 | Conflito de regra de negócio |
| 429 | Limite de requisições excedido |
| 500 | Erro interno |
| 503 | Serviço externo indisponível |

## Segurança das configurações

- O `appsettings.json` local da aplicação fica fora do versionamento do Git por conter configurações confidenciais.
- Não envie senhas do Oracle, credenciais de login, chaves JWT ou tokens nos commits, no README ou nas capturas do vídeo.
- O código e as rotas são demonstráveis pelo Swagger sem disponibilizar as credenciais reais no repositório.
- Caso alguma credencial já tenha sido publicada em commits anteriores, excluí-la dos commits futuros não elimina o histórico: a credencial deverá ser trocada.

## Evolução do CP4 para o CP5

O CP5 mantém o gerenciamento de times, jogadores e perfis competitivos e acrescenta **contratos de resultado padronizados, erros tipados, links HATEOAS, integração OpenDota, autenticação JWT, tratamento global de exceções e expansão da cobertura de testes**, sem alterar a estrutura básica em camadas da aplicação.
