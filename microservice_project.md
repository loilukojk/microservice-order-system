# Microservice Order System -- Project Guide

## Overview

This project is a practical end-to-end backend system built with
**.NET**, designed to help you learn core backend concepts while
demonstrating production-ready skills.\
It mimics a real microservice architecture used in modern companies.

The system contains **three microservices**, an **API Gateway**, a
**database**, and a **message broker**.

------------------------------------------------------------------------

## Architecture Diagram (Conceptual)

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

------------------------------------------------------------------------

## Services Breakdown

### 1. ProductService

Responsible for managing product catalog.

**Tech stack:** - ASP.NET Core Web API (or Minimal API) - EF Core -
PostgreSQL

**Main features:** - Create product - Update product - Delete product -
Get product list - Check stock availability - Expose REST APIs: -
`GET /products` - `GET /products/{id}` - `POST /products` -
`PUT /products/{id}` - `DELETE /products/{id}` - Internal API:
`GET /internal/products/{id}/stock`

------------------------------------------------------------------------

### 2. OrderService

Handles order creation and publishes events to Kafka.

**Tech stack:** - ASP.NET Core Web API - Dapper (for lightweight DB
access) - Kafka (Publisher)

**Main features:** - Create new order - Validate stock by calling
ProductService - Save order to database - Publish `OrderCreated` event
to message broker

**Endpoints:** - `POST /orders` - `GET /orders/{id}`

**Events published:** - `OrderCreated`

``` json
{
  "orderId": 123,
  "productId": 10,
  "quantity": 2
}
```

------------------------------------------------------------------------

### 3. InventoryService

Listens for order events and updates stock accordingly.

**Tech stack:** - ASP.NET Core Worker or Web API - Kafka (Consumer) -
EF Core / Dapper

**Responsibilities:** - Consume `OrderCreated` event - Decrease stock of
product - Publish `StockUpdated` event (optional)

Event example:

``` json
{
  "productId": 10,
  "newStock": 97
}
```

------------------------------------------------------------------------

## API Gateway (YARP)

Used to route all external requests into the appropriate service.

### Example routes:

``` json
"Routes": [
  {
    "RouteId": "product_api",
    "ClusterId": "product_cluster",
    "Match": { "Path": "/api/products/{*path}" }
  },
  {
    "RouteId": "order_api",
    "ClusterId": "order_cluster",
    "Match": { "Path": "/api/orders/{*path}" }
  }
]
```

------------------------------------------------------------------------

## Database

Use **PostgreSQL** for storing:

-   Product catalog\
-   Order records\
-   Inventory data

Each microservice should have **its own database** (database-per-service
pattern).

------------------------------------------------------------------------

## Message Broker

Use **Kafka** for asynchronous communication.

### Queues:

-   `order.created.queue`
-   `stock.updated.queue` (optional)

### Exchange:

-   `order.exchange` (fanout or direct)

------------------------------------------------------------------------

## Docker Compose Setup

Your `docker-compose.yml` should include: - ProductService -
OrderService - InventoryService - API Gateway - PostgreSQL - Kafka -
Adminer (optional UI for DB)

------------------------------------------------------------------------

## Tech Keys

-   ASP.NET Core Web API
-   Minimal APIs
-   Entity Framework Core
-   Dapper
-   Kafka
-   Message-driven communication
-   Docker & containerized microservices
-   API Gateway (YARP)
-   Clean architecture
-   Dependency injection
-   Configuration management
-   Logging & error handling

------------------------------------------------------------------------

## Folder Structure

    /microservices
       /product-service
       /order-service
       /inventory-service
    /gateway
    /docker
    docker-compose.yml
    README.md
