Documentação da API Backend - BenyFinance
Esta documentação descreve os requisitos da API Backend para integrar com o frontend do BenyFinance.

Visão Geral
A API deve ser RESTful, utilizar JSON para troca de dados e autenticação via Token (Bearer Token).

Base URL Sugerida: https://api.benyfinance.com/v1

Autenticação
Login
Endpoint: POST /auth/login
Body:
{
  "email": "user@example.com",
  "password": "password123"
}
Response (200 OK):
{
  "token": "jwt_token_aqui",
  "user": {
    "id": "uuid",
    "name": "Nome do Usuário",
    "email": "user@example.com"
  }
}
Registro (Criar Conta)
Endpoint: POST /auth/register
Body:
{
  "name": "Nome do Usuário",
  "email": "user@example.com",
  "password": "password123"
}
Response (201 Created):
{
  "token": "jwt_token_aqui",
  "user": {
    "id": "uuid",
    "name": "Nome do Usuário",
    "email": "user@example.com"
  }
}
Transações (Transactions)
Listar Transações
Endpoint: GET /transactions
Query Params (Opcionais):
month: (int) Mês (1-12)
year: (int) Ano (ex: 2025)
type: (string) 'income' | 'expense'
Response (200 OK):
[
  {
    "id": "uuid",
    "date": "2025-11-19T10:00:00.000Z",
    "description": "Supermercado",
    "amount": 150.50,
    "type": "expense", // 'income' ou 'expense'
    "category": "Alimentação",
    "categoryId": "uuid_categoria", // Opcional se enviar o nome
    "paymentMethod": "credit_card", // 'cash' ou 'credit_card'
    "cardId": "uuid_cartao", // Null se paymentMethod for 'cash'
    "status": "paid" // 'paid', 'pending', 'canceled'
  }
]
Criar Transação
Endpoint: POST /transactions
Body:
{
  "date": "2025-11-19T10:00:00.000Z",
  "description": "Salário",
  "amount": 5000.00,
  "type": "income",
  "category": "Salário",
  "paymentMethod": "cash",
  "status": "paid"
}
Response (201 Created): Objeto da transação criada.
Atualizar Transação
Endpoint: PUT /transactions/:id
Body: Campos a serem atualizados (parcial ou total).
Response (200 OK): Objeto da transação atualizada.
Deletar Transação
Endpoint: DELETE /transactions/:id
Response (204 No Content)
Dashboard
Obter Dados do Dashboard
Endpoint: GET /dashboard
Query Params: month (int), year (int)
Response (200 OK):
{
  "balance": 1500.00, // Saldo (Receitas - Despesas em Dinheiro)
  "income": 5000.00, // Total Receitas
  "expense": 3500.00, // Total Despesas
  "cardExpenses": 1200.00, // Total Despesas no Cartão
  "transactions": [], // Lista das 5 últimas transações
  "barChartData": [ // Dados para o gráfico de barras (últimos 6 meses)
    { "name": "Jun", "Receitas": 4000, "Despesas": 3000 }
  ],
  "pieChartData": [ // Dados para o gráfico de pizza (despesas por categoria)
    { "name": "Casa", "value": 1500 }
  ]
}
Nota: Se o backend não puder fornecer os dados agregados, o frontend pode calcular com base no endpoint GET /transactions, mas a performance será melhor se vier pronto.
Categorias (Categories)
Listar Categorias
Endpoint: GET /categories
Response (200 OK):
[
  { "id": "1", "name": "Casa", "color": "#3b82f6" },
  { "id": "2", "name": "Saúde", "color": "#ef4444" }
]
Criar Categoria
Endpoint: POST /categories
Body: { "name": "Nova Categoria", "color": "#000000" }
Deletar Categoria
Endpoint: DELETE /categories/:id
Cartões de Crédito (Credit Cards)
Listar Cartões
Endpoint: GET /cards
Response (200 OK):
[
  {
    "id": "1",
    "name": "Nubank",
    "limit": 5000,
    "bank": "Nubank",
    "closingDay": 10,
    "dueDay": 17
  }
]
Criar Cartão
Endpoint: POST /cards
Body: Objeto do cartão.
Deletar Cartão
Endpoint: DELETE /cards/:id
Contas Recorrentes (Recurring Templates)
Listar Modelos
Endpoint: GET /recurring-templates
Response (200 OK):
[
  { "id": "1", "description": "Aluguel", "amount": 1500, "category": "Casa" }
]
Criar Modelo
Endpoint: POST /recurring-templates
Body: { "description": "Netflix", "amount": 55.90, "category": "Lazer" }
Deletar Modelo
Endpoint: DELETE /recurring-templates/:id
Gerar Transações Mensais
Endpoint: POST /transactions/generate-monthly
Body:
{
  "month": 11,
  "year": 2025
}
Comportamento: O backend deve pegar todos os modelos recorrentes do usuário e criar novas transações para o mês/ano especificado.
Data: Dia 10 do mês.
Valor: 0.00 (conforme regra de negócio atual) ou o valor do template (configurável).
Status: 'pending'.
Response (201 Created): Lista das transações criadas.