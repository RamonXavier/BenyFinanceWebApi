# BenyFinance API

API RESTful para controle financeiro pessoal, desenvolvida com .NET 8 e Clean Architecture.

## 📋 Sobre o Projeto

BenyFinance é uma API completa para gerenciamento de finanças pessoais que permite:

- ✅ Autenticação e autorização com JWT
- 💰 Gerenciamento de transações (receitas e despesas)
- 🏷️ Categorização de transações
- 💳 Controle de cartões de crédito
- 🔄 Templates de transações recorrentes
- 📊 Dashboard com dados agregados e gráficos

## 🏗️ Arquitetura

O projeto segue os princípios da **Clean Architecture**, organizado em 4 camadas:

```
BenyFinanceWebApi/
├── src/
│   ├── BenyFinance.Domain/          # Entidades e Interfaces de Repositório
│   ├── BenyFinance.Application/     # DTOs, Serviços e Lógica de Negócio
│   ├── BenyFinance.Infrastructure/  # EF Core, Repositórios e Persistência
│   └── BenyFinance.Api/             # Controllers, Configuração e Endpoints
```

### Camadas

- **Domain**: Entidades do domínio (`User`, `Transaction`, `Category`, `CreditCard`, `RecurringTemplate`) e interfaces de repositório
- **Application**: DTOs, interfaces de serviço e implementação da lógica de negócio
- **Infrastructure**: Implementação do Entity Framework Core, `AppDbContext` e repositórios concretos
- **API**: Controllers, configuração de autenticação JWT, Swagger e injeção de dependências

## 🚀 Tecnologias

- **.NET 8**
- **Entity Framework Core 8.0**
- **SQL Server**
- **JWT Bearer Authentication**
- **BCrypt.Net** (hash de senhas)
- **Swagger/OpenAPI**

## ⚙️ Configuração

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- SQL Server (local ou remoto)

### Configuração do Banco de Dados

1. Atualize a connection string em `src/BenyFinance.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=SEU_SERVIDOR;Database=SEU_BANCO;User Id=SEU_USUARIO;Password=SUA_SENHA;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True;"
  }
}
```

2. Execute as migrations para criar o banco de dados:

```bash
dotnet ef migrations add InitialCreate -p src/BenyFinance.Infrastructure -s src/BenyFinance.Api
dotnet ef database update -p src/BenyFinance.Infrastructure -s src/BenyFinance.Api
```

### Configuração do JWT

A chave JWT está configurada em `appsettings.json`. **Em produção**, use uma chave forte e armazene-a de forma segura (Azure Key Vault, variáveis de ambiente, etc.):

```json
{
  "Jwt": {
    "Key": "SuaChaveSecretaMuitoSeguraAqui",
    "Issuer": "BenyFinanceApi",
    "Audience": "BenyFinanceClient"
  }
}
```

## 🏃 Executando o Projeto

### Compilar

```bash
dotnet build
```

### Executar

```bash
dotnet run --project src/BenyFinance.Api/BenyFinance.Api.csproj
```

A API estará disponível em:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`
- **Swagger UI**: `https://localhost:5001/swagger`

## 📚 Endpoints da API

### Autenticação

| Método | Endpoint | Descrição |
|--------|----------|-----------|
| POST | `/auth/register` | Registrar novo usuário |
| POST | `/auth/login` | Login e obtenção do token JWT |

### Transações

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| GET | `/transactions` | Listar transações (com filtros opcionais) | ✅ |
| GET | `/transactions/{id}` | Obter transação por ID | ✅ |
| POST | `/transactions` | Criar nova transação | ✅ |
| PUT | `/transactions/{id}` | Atualizar transação | ✅ |
| DELETE | `/transactions/{id}` | Excluir transação | ✅ |
| POST | `/transactions/generate-monthly` | Gerar transações mensais a partir de templates | ✅ |

### Categorias

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| GET | `/categories` | Listar categorias do usuário | ✅ |
| POST | `/categories` | Criar nova categoria | ✅ |
| DELETE | `/categories/{id}` | Excluir categoria | ✅ |

### Cartões de Crédito

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| GET | `/cards` | Listar cartões do usuário | ✅ |
| POST | `/cards` | Adicionar novo cartão | ✅ |
| DELETE | `/cards/{id}` | Excluir cartão | ✅ |

### Templates Recorrentes

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| GET | `/recurring-templates` | Listar templates recorrentes | ✅ |
| POST | `/recurring-templates` | Criar novo template | ✅ |
| DELETE | `/recurring-templates/{id}` | Excluir template | ✅ |

### Dashboard

| Método | Endpoint | Descrição | Autenticação |
|--------|----------|-----------|--------------|
| GET | `/dashboard` | Obter dados agregados do dashboard | ✅ |

## 🔐 Autenticação

A API utiliza **JWT Bearer Token** para autenticação. Para acessar endpoints protegidos:

1. Registre um usuário via `/auth/register`
2. Faça login via `/auth/login` para obter o token
3. Inclua o token no header das requisições:

```
Authorization: Bearer SEU_TOKEN_AQUI
```

### Exemplo de Uso (cURL)

```bash
# Registrar usuário
curl -X POST https://localhost:5001/auth/register \
  -H "Content-Type: application/json" \
  -d '{"name":"João Silva","email":"joao@example.com","password":"Senha123!"}'

# Login
curl -X POST https://localhost:5001/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"joao@example.com","password":"Senha123!"}'

# Usar o token retornado
curl -X GET https://localhost:5001/transactions \
  -H "Authorization: Bearer SEU_TOKEN_AQUI"
```

## 📊 Modelos de Dados

### Transaction (Transação)

```json
{
  "id": "guid",
  "description": "string",
  "amount": "decimal",
  "date": "datetime",
  "type": "income | expense",
  "paymentMethod": "cash | debit | credit | pix | transfer",
  "status": "pending | completed | cancelled",
  "categoryId": "guid",
  "categoryName": "string",
  "cardId": "guid?",
  "cardName": "string?",
  "installments": "int?",
  "currentInstallment": "int?"
}
```

### Category (Categoria)

```json
{
  "id": "guid",
  "name": "string"
}
```

### CreditCard (Cartão de Crédito)

```json
{
  "id": "guid",
  "name": "string",
  "lastDigits": "string",
  "limit": "decimal",
  "closingDay": "int",
  "dueDay": "int"
}
```

### RecurringTemplate (Template Recorrente)

```json
{
  "id": "guid",
  "description": "string",
  "amount": "decimal",
  "type": "income | expense",
  "paymentMethod": "cash | debit | credit | pix | transfer",
  "categoryId": "guid",
  "categoryName": "string",
  "dayOfMonth": "int"
}
```

## 🧪 Testando com Swagger

1. Execute a aplicação
2. Acesse `https://localhost:5001/swagger`
3. Use o botão **Authorize** no topo da página
4. Faça login via `/auth/login` para obter o token
5. Cole o token no campo de autorização (sem o prefixo "Bearer")
6. Teste os endpoints protegidos

## 📝 Próximos Passos

- [ ] Implementar testes unitários e de integração
- [ ] Adicionar validações com FluentValidation
- [ ] Implementar paginação nos endpoints de listagem
- [ ] Adicionar logging estruturado (Serilog)
- [ ] Implementar cache (Redis)
- [ ] Criar documentação detalhada da API
- [ ] Adicionar rate limiting
- [ ] Implementar soft delete para entidades

## 📄 Licença

Este projeto é de uso pessoal/educacional.

## 👤 Autor

Ramon - BenyFinance Project

---

**Desenvolvido com .NET 8 e Clean Architecture** 🚀
