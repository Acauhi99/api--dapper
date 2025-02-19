# API with Dapper and SQLite

A simple RESTful API built with .NET 8, Dapper, and SQLite demonstrating CRUD operations.

## Technologies

- .NET 8
- Dapper
- SQLite
- Swagger/OpenAPI

## Prerequisites

- .NET 8 SDK
- Visual Studio Code or any preferred IDE

## Getting Started

1. Clone the repository

```bash
git clone https://github.com/Acauhi99/api--dapper
cd api--dapper
```

2. Build the project

```dotnetcli
dotnet build
```

3. Run the application

```dotnetcli
dotnet run
```

The API will be available at http://localhost:5090

## API Endpoints

| Method | Endpoint        | Description     |
| ------ | --------------- | --------------- |
| GET    | /api/users      | Get all users   |
| GET    | /api/users/{id} | Get user by ID  |
| POST   | /api/users      | Create new user |
| PUT    | /api/users/{id} | Update user     |
| DELETE | /api/users/{id} | Delete user     |
