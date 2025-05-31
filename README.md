# API com Dapper e SQLite

Uma API RESTful simples construída com .NET 8, Dapper e SQLite demonstrando operações CRUD com autenticação e autorização.

## Tecnologias

- .NET 8
- Dapper
- SQLite
- Autenticação JWT
- Swagger/OpenAPI

## Pré-requisitos

- .NET 8 SDK
- Visual Studio Code ou qualquer IDE de sua preferência

## Começando

1. Clone o repositório

```bash
git clone https://github.com/Acauhi99/api--dapper
cd api--dapper
```

2. Compile o projeto

```dotnetcli
dotnet build
```

3. Execute a aplicação

```dotnetcli
dotnet watch run
```

A API estará disponível em http://localhost:5090

## Endpoints da API

### Rotas de Autenticação

| Método | Endpoint           | Descrição                          | Autenticação Obrigatória |
| ------ | ------------------ | ---------------------------------- | ------------------------ |
| POST   | /api/auth/login    | Login do usuário e obter token JWT | Não                      |
| POST   | /api/auth/register | Registrar novo usuário             | Não                      |

### Rotas de Usuário

| Método | Endpoint        | Descrição               | Autenticação Obrigatória | Função Obrigatória |
| ------ | --------------- | ----------------------- | ------------------------ | ------------------ |
| GET    | /api/users      | Obter todos os usuários | Sim                      | Admin              |
| GET    | /api/users/{id} | Obter usuário por ID    | Sim                      | User/Admin         |
| POST   | /api/users      | Criar novo usuário      | Não                      | Nenhuma            |
| PUT    | /api/users/{id} | Atualizar usuário       | Sim                      | User/Admin         |
| DELETE | /api/users/{id} | Deletar usuário         | Sim                      | Admin              |

### Rotas de Administrador

| Método | Endpoint         | Descrição                   | Autenticação Obrigatória |
| ------ | ---------------- | --------------------------- | ------------------------ |
| POST   | /api/admin/setup | Criar usuário administrador | Não\*                    |

\*Requer segredo de administrador da configuração

## Equipe de Desenvolvimento

| Nome                    | GitHub                                             |
| ----------------------- | -------------------------------------------------- |
| Mateus Acauhi           | [@Acauhi99](https://github.com/Acauhi99)           |
| Fernanda Rooke da Silva | [@FernandaRooke](https://github.com/FernandaRooke) |
| Lucas Guilarducci Menon | [@Menon04](https://github.com/Menon04)             |
| Laura Patrício de Matos | [@lauramatos777](https://github.com/lauramatos777) |
| Mateus Sarlo            | [@msarlo](https://github.com/msarlo)               |
