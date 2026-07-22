# Service Orders Microservice

## TODO

- make this and my other microservices use https redirection, but for now, I will leave it off to make it easier to test in development.

## Running through docker

If you have made code changes, you should rebuild and push the Docker image before running the docker-compose file.

1. Build microservice image
```bash
docker build -t danielmusselwhite/commercefabric_order_microservice:1.0.0 -f .\OrdersMicroservice.API\Dockerfile .
```

2. Push to Docker Hub
```bash
docker push danielmusselwhite/commercefabric_order_microservice:1.0.0
```

### Running docker-compose

- Build and run the docker-compose file
```bash
docker-compose -f docker-compose.yaml up --build
```
- Can access the service's Swagger UI at http://localhost:9090/swagger/index.html

- stop the containers
```bash
docker-compose -f docker-compose.yaml down
```

### Manually running MongoDB
- If you wish to run the app in visual studio, launch MongoDB through docker via the below command, which will run the MongoDB container and initialise it with the orders_mongoDB_init.js script. This script will create the OrdersDatabase and the orders collection, and insert some initial data - then you can debug in visual studio.

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
use OrdersDb
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
* Redis Cache (for caching data from other microservices)
* Ocelot (API Gateway)

## Design Notes

* FluentValidation is manually triggered in BLL services, but is triggered automatically within the API Controllers
* MongoDB is used as the primary data store, with repository abstractions handling persistence operations.
* The service is focused on write operations (Create, Update, Delete) as part of the overall microservices architecture.
