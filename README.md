# Microservice Order System

A production-ready microservices architecture built with .NET 8, demonstrating modern backend development practices including event-driven architecture, API Gateway pattern, and containerized deployment.

## Architecture Overview

This system demonstrates a complete microservices architecture with three core services:

- **ProductService**: Manages product catalog with EF Core and PostgreSQL
- **OrderService**: Handles order creation with Dapper and publishes events to Kafka
- **InventoryService**: Consumes Kafka events and manages inventory stock
- **API Gateway**: YARP-based gateway for routing external requests
- **PostgreSQL**: Database system (database-per-service pattern)
- **Kafka**: Message broker for asynchronous communication

```
                +-----------------+
                |  API Gateway    |
                |    (YARP)       |
                +--------+--------+
                         |
          -----------------------------------
          |                |                |
     +----v----+     +-----v-----+    +-----v------+
     | Product |     |  Order    |    | Inventory  |
     | Service |     | Service   |    | Service    |
     +----+----+     +-----+-----+    +------+-----+
          |                |                |
          |       Publish OrderCreated     |
          |--------------->●-------------->|
                          Kafka
```

## Technology Stack

- **Framework**: .NET 8.0
- **API Style**: Minimal APIs
- **ORM**: Entity Framework Core, Dapper
- **Database**: PostgreSQL
- **Message Broker**: Apache Kafka
- **API Gateway**: YARP (Yet Another Reverse Proxy)
- **Containerization**: Docker & Docker Compose
- **API Documentation**: Swagger/OpenAPI

## Project Structure

```
/microservices
   /product-service     - Product catalog management
   /order-service       - Order processing and event publishing
   /inventory-service   - Inventory tracking and event consumption
/gateway               - API Gateway with YARP
/docker                - Docker-related files
docker-compose.yml     - Container orchestration
```

## Prerequisites

- Docker and Docker Compose
- .NET 8.0 SDK (for local development)
- Git

## Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/loilukojk/microservice-order-system
cd microservice-order-system
```

### 2. Start All Services with Docker Compose

```bash
docker-compose up --build
```

This command will:
- Build all microservices
- Start PostgreSQL database
- Start Kafka and Zookeeper
- Launch all three microservices
- Start the API Gateway
- Start Adminer (database UI)

### 3. Access the Services

Once all services are running:

- **API Gateway**: http://localhost:5000
- **Product Service**: http://localhost:5001
- **Order Service**: http://localhost:5002
- **Inventory Service**: http://localhost:5003
- **Adminer (DB UI)**: http://localhost:8080
  - System: PostgreSQL
  - Server: postgres
  - Username: postgres
  - Password: postgres

### 4. API Documentation

Each service provides Swagger UI for API exploration:

- Product Service Swagger: http://localhost:5001/swagger
- Order Service Swagger: http://localhost:5002/swagger
- Inventory Service Swagger: http://localhost:5003/swagger

## API Endpoints

### Via API Gateway (Recommended)

#### Products
- `GET /api/products` - Get all products
- `GET /api/products/{id}` - Get product by ID
- `POST /api/products` - Create new product
- `PUT /api/products/{id}` - Update product
- `DELETE /api/products/{id}` - Delete product

#### Orders
- `POST /api/orders` - Create new order
- `GET /api/orders/{id}` - Get order by ID
- `GET /api/orders` - Get all orders

#### Inventory
- `GET /api/inventory/{productId}` - Get inventory for product
- `GET /api/inventory` - Get all inventory records

### Example: Creating an Order

```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "productId": 1,
    "quantity": 2
  }'
```

This will:
1. Validate product stock availability
2. Create the order in the database
3. Publish an `OrderCreated` event to Kafka
4. Trigger the InventoryService to decrease stock
5. Publish a `StockUpdated` event

## Service Details

### ProductService

**Technology**: ASP.NET Core, Entity Framework Core, PostgreSQL

**Responsibilities**:
- CRUD operations for products
- Stock availability checking
- Serves internal API for other services

**Database**: `productdb`

### OrderService

**Technology**: ASP.NET Core, Dapper, Kafka Producer

**Responsibilities**:
- Order creation and validation
- Stock validation via ProductService
- Publishing order events to Kafka

**Database**: `orderdb`

### InventoryService

**Technology**: ASP.NET Core, Dapper, Kafka Consumer

**Responsibilities**:
- Consuming `OrderCreated` events
- Updating inventory stock
- Publishing `StockUpdated` events

**Database**: `inventorydb`

### API Gateway

**Technology**: YARP Reverse Proxy

**Responsibilities**:
- Route external requests to appropriate services
- Single entry point for all API calls
- CORS handling

## Development

### Running Locally (Without Docker)

1. **Start PostgreSQL and Kafka** (using Docker):
```bash
docker-compose up postgres kafka zookeeper
```

2. **Run each service** (in separate terminals):
```bash
# Product Service
cd microservices/product-service
dotnet run

# Order Service
cd microservices/order-service
dotnet run

# Inventory Service
cd microservices/inventory-service
dotnet run

# API Gateway
cd gateway
dotnet run
```

### Database Management

Access Adminer at http://localhost:8080 to:
- View database schemas
- Execute SQL queries
- Monitor data changes
- Debug database issues

**Connection Details**:
- Server: `postgres`
- Username: `postgres`
- Password: `postgres`
- Databases: `productdb`, `orderdb`, `inventorydb`

## Key Architectural Patterns

### 1. Database-per-Service
Each microservice has its own database, ensuring loose coupling and independent scalability.

### 2. Event-Driven Architecture
Services communicate asynchronously through Kafka events, enabling better decoupling and resilience.

### 3. API Gateway Pattern
YARP provides a single entry point, simplifying client interactions and enabling centralized concerns like authentication.

### 4. Clean Architecture
Each service follows clean architecture principles with clear separation of concerns:
- Models/Entities
- Repositories (Data Access)
- Services (Business Logic)
- API Endpoints

### 5. Dependency Injection
Built-in .NET DI container manages service lifetimes and dependencies.

## Message Flow

```
1. Client creates order via API Gateway
2. OrderService validates stock with ProductService
3. OrderService saves order to database
4. OrderService publishes OrderCreated event to Kafka
5. InventoryService consumes OrderCreated event
6. InventoryService decreases stock in database
7. InventoryService publishes StockUpdated event
```

## Stopping the System

```bash
docker-compose down
```

To remove volumes (databases):
```bash
docker-compose down -v
```

## Troubleshooting

### Services not starting
Check logs for specific service:
```bash
docker-compose logs product-service
docker-compose logs order-service
docker-compose logs inventory-service
```

### Kafka connection issues
Ensure Kafka is fully started before services:
```bash
docker-compose logs kafka
```

### Database connection issues
Verify PostgreSQL is healthy:
```bash
docker-compose ps postgres
```

## Learning Objectives

This project demonstrates:

- Microservices architecture design
- RESTful API development with .NET
- Entity Framework Core and Dapper usage
- Event-driven communication with Kafka
- API Gateway implementation with YARP
- Docker containerization
- Database-per-service pattern
- Clean code organization
- Asynchronous messaging patterns
- Service-to-service communication
- Configuration management
- Logging and error handling

## Future Enhancements

- Add authentication and authorization (JWT)
- Implement API versioning
- Add distributed tracing (OpenTelemetry)
- Implement circuit breaker pattern
- Add health checks and monitoring
- Implement CQRS pattern
- Add unit and integration tests
- Implement rate limiting
- Add API documentation with examples

## License

This is an educational project for learning .NET microservices architecture.
