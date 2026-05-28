# Purchase Transaction API

A comprehensive .NET 10 REST API for managing purchase transactions with currency conversion capabilities. This application provides endpoints for creating, searching, and retrieving purchase transactions with real-time exchange rate support.

## Table of Contents

- [Project Overview](#project-overview)
- [Technology Stack](#technology-stack)
- [Project Structure](#project-structure)
- [Installation & Setup](#installation--setup)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [Using Swagger UI](#using-swagger-ui)
- [API Endpoints](#api-endpoints)
- [Testing](#testing)
  - [Unit Tests](#unit-tests)
  - [Integration Tests](#integration-tests)
- [Database](#database)

## Project Overview

The Purchase Transaction API is built using a layered architecture pattern with the following responsibilities:

- **Create Purchase Transactions**: Store new purchase transactions with source and target currency information
- **Search Transactions**: Query transactions with flexible filtering options
- **Retrieve Transaction Details**: Get specific transaction details with currency conversion

## Technology Stack

- **.NET Runtime**: .NET 10
- **Language**: C#
- **Web Framework**: ASP.NET Core
- **Database**: SQLite (default, configurable)
- **ORM**: Entity Framework Core
- **Validation**: FluentValidation
- **Logging**: NLog
- **API Documentation**: Swagger/OpenAPI
- **Testing**: xUnit, Moq

## Project Structure

```
PurchaseTransaction/
├── PurchaseTransactions.API/           # Main API project (ASP.NET Core)
│   ├── Controllers/                    # API controllers
│   ├── Middleware/                     # Custom middleware (exception handling)
│   ├── Program.cs                      # Application entry point & configuration
│   ├── appsettings.json               # Configuration file
│   └── PurchaseTransactions.API.http  # REST client file for testing
├── PurchaseTransactions.Application/   # Business logic layer
│   ├── Services/                       # Business services
│   ├── Models/                         # Request/response models
│   ├── Interfaces/                     # Service contracts
│   └── Exceptions/                     # Custom exceptions
├── PurchaseTransactions.Infrastructure/ # Data access layer
│   ├── Persistence/                    # Database context & repositories
│   └── ExternalServices/               # External API integrations
├── PurchaseTransactions.Domain/        # Domain models & entities
├── tests/
│   ├── PurchaseTransactions.API.IntegrationTest/  # Integration tests
│   └── PurchaseTransctions.UnitTest/              # Unit tests
└── README.md                           # This file
```

## Installation & Setup

### 1. Clone the Repository

```bash
git clone https://github.com/manishbsr/CorporatePayments.git
cd PurchaseTransaction
```

### 2. Restore NuGet Packages

```bash
dotnet restore
```

### 3. Build the Solution

```bash
dotnet build
```

## Running the Application

### Option 1: Using Visual Studio

1. Open the solution in Visual Studio 2022
2. Set `PurchaseTransactions.API` as the startup project
3. Press `F5` or click the Run button
4. The application will launch and open Swagger UI in your browser

### Option 2: Using .NET CLI

```bash
cd PurchaseTransactions.API
dotnet run
```

The API will be available at:
- **HTTP**: `http://localhost:5182`

## API Documentation

### Using Swagger UI

The API includes interactive Swagger UI documentation for easy exploration and testing.

#### Accessing Swagger UI

1. **Start the application** (see [Running the Application](#running-the-application))
2. **Open your browser** and navigate to:
   ```
   http://localhost:5182
   ```

3. **Swagger UI** will automatically load at the root URL, displaying all available endpoints

#### Testing Endpoints with Swagger

1. **Click on an endpoint** to expand it
2. **Click "Try it out"** button
3. **Fill in the required parameters** (if any)
4. **Click "Execute"** to send the request
5. **View the response** in the Response section

#### Available Operations in Swagger

- Create a new purchase transaction
- Search for transactions with filters
- Get transaction details by ID

## API Endpoints

All endpoints are prefixed with `/api/purchasetransactions`

### 1. Create Purchase Transaction

**Endpoint:** `POST /api/purchasetransactions`

**Description:** Create a new purchase transaction

**Request Body:**
```json
{
  "fromCurrency": "USD",
  "toCurrency": "EUR",
  "amountInSourceCurrency": 100.50
}
```

**Response:** `201 Created`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fromCurrency": "USD",
  "toCurrency": "EUR",
  "amountInSourceCurrency": 100.50,
  "amountInTargetCurrency": 92.30,
  "exchangeRate": 0.923,
  "transactionDate": "2024-01-15T10:30:00Z"
}
```

**Status Codes:**
- `201 Created` - Transaction successfully created
- `400 Bad Request` - Invalid input data
- `500 Internal Server Error` - Server error

---

### 2. Search Purchase Transactions

**Endpoint:** `GET /api/purchasetransactions`

**Description:** Search for purchase transactions with optional filters

**Query Parameters:**
- `fromCurrency` (optional) - Filter by source currency
- `toCurrency` (optional) - Filter by target currency
- `amountInSourceCurrency` (optional) - Filter by amount

**Example Request:**
```
GET /api/purchasetransactions?fromCurrency=USD&toCurrency=EUR&amountInSourceCurrency=100
```

**Response:** `200 OK`
```json
[
  {
    "id": "550e8400-e29b-41d4-a716-446655440000",
    "fromCurrency": "USD",
    "toCurrency": "EUR",
    "amountInSourceCurrency": 100.50,
    "amountInTargetCurrency": 92.30,
    "exchangeRate": 0.923,
    "transactionDate": "2024-01-15T10:30:00Z"
  }
]
```

**Status Codes:**
- `200 OK` - Transactions retrieved successfully
- `400 Bad Request` - Invalid query parameters
- `500 Internal Server Error` - Server error

---

### 3. Get Purchase Transaction by ID

**Endpoint:** `GET /api/purchasetransactions/{transactionId}`

**Description:** Get a specific purchase transaction by ID with currency conversion

**Path Parameters:**
- `transactionId` (required) - The transaction ID (GUID format)

**Query Parameters:**
- `targetCurrency` (required) - Currency to convert the amount to

**Example Request:**
```
GET /api/purchasetransactions/550e8400-e29b-41d4-a716-446655440000?targetCurrency=EURO
```

**Response:** `200 OK`
```json
{
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "fromCurrency": "USD",
  "originalCurrency": "USD",
  "originalAmount": 100.50,
  "targetCurrency": "EURO",
  "amountInTargetCurrency": 92.30,
  "exchangeRate": 0.923,
  "transactionDate": "2024-01-15T10:30:00Z"
}
```

**Status Codes:**
- `200 OK` - Transaction retrieved successfully
- `400 Bad Request` - Invalid transaction ID or currency
- `404 Not Found` - Transaction not found
- `500 Internal Server Error` - Server error

---

## Testing

The project includes comprehensive unit and integration tests.

### Unit Tests

**Location:** `tests\PurchaseTransctions.UnitTest\`

**Run all unit tests:**
```bash
dotnet test tests/PurchaseTransctions.UnitTest/
```

**Run a specific test class:**
```bash
dotnet test tests/PurchaseTransctions.UnitTest/ --filter ClassName=PurchaseTransactionsServiceTest
```

**Test Coverage Includes:**
- Business service logic
- Request validation
- Exception handling
- Currency conversion calculations

### Integration Tests

**Location:** `tests\PurchaseTransactions.API.IntegrationTest\`

**Run all integration tests:**
```bash
dotnet test tests/PurchaseTransactions.API.IntegrationTest/
```

**Run a specific test:**
```bash
dotnet test tests/PurchaseTransactions.API.IntegrationTest/ --filter ClassName=PurchaseTransactionsControllerTest
```

**Test Coverage Includes:**
- End-to-end API requests
- HTTP status codes validation
- Response schema validation
- Database state verification

### Run All Tests (Unit + Integration)

```bash
dotnet test
```



## Database

### SQLite (Default)

The application uses SQLite for local development:

**Database File:** `PurchaseTransactions.db`

**Auto-Creation:** The database is automatically created on first run using Entity Framework Core migrations.

### Database Initialization

The database schema is initialized automatically when the application starts:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}
```

### Key Database Tables

1. **PurchaseTransactions**
   - Id (GUID)
   - FromCurrency (string)
   - ToCurrency (string)
   - AmountInSourceCurrency (decimal)
   - AmountInTargetCurrency (decimal)
   - ExchangeRate (decimal)
   - TransactionDate (DateTime)

---

## Author

Manish Bsr

---

**Last Updated:** January 2024
