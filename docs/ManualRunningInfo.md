# Additional running information

- This is the old document on how to run the service locally.
- It is now deprecated as the service is now run through docker-compose, and deployed through the automation github actions CI/CD pipeline.
- But have left it here in case you want to run the service locally without docker-compose, and just run it in visual studio.

## Running through docker

If you have made code changes, you should rebuild and push the Docker images before running the docker-compose file.

1. Build microservice images

Build the Orders microservice:

```bash
docker build -t danielmusselwhite/commercefabric_order_microservice:1.0.0 -f .\OrdersMicroservice.API\Dockerfile .
```

Build the ApiGateway:

```bash
docker build -t danielmusselwhite/commercefabric_api_gateway:1.0.0 -f .\ApiGateway\Dockerfile .
```

2. Push to Docker Hub

Push the Orders microservice:

```bash
docker push danielmusselwhite/commercefabric_order_microservice:1.0.0
```

Push the ApiGateway:

```bash
docker push danielmusselwhite/commercefabric_api_gateway:1.0.0
```

The following Dockerfile is therefore expected to be pushed to the corresponding Docker Hub image:

```text
ApiGateway\Dockerfile
    -> danielmusselwhite/commercefabric_api_gateway:1.0.0
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
