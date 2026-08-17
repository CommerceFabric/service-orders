# Service Orders Microservice

> Note: when running locally, ensure you have a .env file in the root of the service-orders project, implementing the template provided at [.env.example](./.env.example). This is required for the service to run locally, as it contains the connection strings for the Azure Service Bus and Azure Entra ID app registration.

## Technical Info

### Technical Stack

* MongoDB
* Dependency Injection
* AutoMapper
* FluentValidation (manual validation for Minimal APIs)
* Exception Handling Middleware
* Swagger / OpenAPI
* MVC Controller based API endpoints
* Redis Cache (for caching data from other microservices)
* Ocelot (API Gateway)
* RabbitMQ (event messaging, to consume product update events from the Products microservice)
* Azure Service Bus (to post order created events to the Service Bus topic, which is then consumed by the Products microservice to update product stock levels)
* Polly (for retry and circuit breaker policies)
* GitHub actions for CI, pushing docker images to the Azure Container Registry, then triggering [Infra-Platform](https://github.com/CommerceFabric/infra-platform/blob/main/docs/DeploymentFlow.md#microservice-release-sequence) to do the CD of deploying the new image to the AKS cluster.

### Architecture

This service uses a **Layered Architecture** pattern:

#### API Layer

* Exposes endpoints
* Handles request/response mapping

#### Business Logic Layer

* Application workflows
* Validation and business rules

#### Data Access Layer

* Database interactions
* Repository implementations
* Persistence concerns

> **Architecture Note**
> A Clean Architecture approach is also used in other services (e.g. User Service), but this service intentionally uses a Layered Architecture to reduce complexity for a smaller bounded context.

---
