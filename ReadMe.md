## Technical Info

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

## Technical Stack

* MongoDB
* Dependency Injection
* AutoMapper
* FluentValidation (manual validation for Minimal APIs)
* Exception Handling Middleware
* Swagger / OpenAPI
* MVC Controller based API endpoints

## Design Notes

* FluentValidation is manually triggered in BLL services, but is triggered automatically within the API Controllers
* MongoDB is used as the primary data store, with repository abstractions handling persistence operations.
* The service is focused on write operations (Create, Update, Delete) as part of the overall microservices architecture.
