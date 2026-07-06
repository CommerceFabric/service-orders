# Service Orders Microservice

## TODO

- Make a docker-compose file to run the service and MongoDB together

- Link with User microservice in future so users can only see their own orders

- Integrate this into the infra-platform repo, so in its centralised docker-compose, it will launch this along with the other services and MongoDB

## Running through docker

- Running MongoDB

```bash
docker run --rm -p 27017:27017 -v ./Resources/orders_mongoDB_init.js:/docker-entrypoint-initdb.d/orders_mongoDB_init.js:ro --name mongodb-server mongo:latest
```
    - this will destroy the container when it is stopped, so any data will be lost. This is fine for development purposes, but in production, you would want to use a volume to persist the data.

- if happy, can push this to docker-hub via:
```bash
docker build -t danielmusselwhite/commercefabric_order_microservice:1.0.0 -f .\OrdersMicroservice.API\Dockerfile .

docker push danielmusselwhite/commercefabric_order_microservice:1.0.0
```

- To access the MongoDB shell, launch a new terminal and run:
```bash
docker exec -it mongodb-server bash
mongosh
```

- Can then test it has successfully run by running:
```bash
show databases
use OrdersDatabase
show collections
db.orders.find()
```


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
