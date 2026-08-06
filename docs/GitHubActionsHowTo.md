# GitHub Actions → Azure Container Registry & Docker Hub

This document describes how the `CommerceFabric` microservice repository automatically builds and publishes Docker images to:

* Azure Container Registry (ACR)
* Docker Hub

The setup uses **GitHub Actions OIDC authentication** for Azure, so no Azure client secret/password is stored in GitHub.

---

## 1. Repository structure

Each microservice has its own repository within the `CommerceFabric` GitHub organisation.

Example:

```text
CommerceFabric/
├── service-user
├── service-orders
├── service-products
└── infra
```

---

# 2. Create the GitHub Actions workflow

Create:

```text
.github/workflows/docker.yml
```

The workflow:

1. Runs when code is pushed to `main`
2. Builds the Docker image
3. Authenticates with Azure using OIDC
4. Authenticates with Docker Hub using a PAT
5. Pushes the image to ACR
6. Pushes the image to Docker Hub

The Docker build context must be the repository root because the Dockerfile references files outside its own directory:

```yaml
docker build \
  --file CommerceFabric.OrdersService/Dockerfile \
  ...
  .
```

---

# 3. Azure App Registration

Create one Azure App Registration for the CommerceFabric GitHub Actions identities.

Example name:

```text
github-commercefabric-actions
```

Using Azure CLI:

```powershell
az ad app create `
  --display-name "github-commercefabric-actions"
```

Get the Application/Client ID:

```powershell
$CLIENT_ID = az ad app list `
  --display-name "github-commercefabric-actions" `
  --query "[0].appId" `
  -o tsv
```

This becomes:

```text
AZURE_CLIENT_ID
```

---

# 4. Create the Service Principal

The App Registration needs a corresponding service principal so Azure RBAC permissions can be assigned to it.

```powershell
az ad sp create --id $CLIENT_ID
```

---

# 5. Configure GitHub OIDC

Create a federated credential on the Azure App Registration.

For `CommerceFabric/service-orders`:

```text
repo:CommerceFabric/service-orders:ref:refs/heads/main
```

Using Azure CLI/PowerShell:

```powershell
$parameters = @'
{
  "name": "github-service-orders-main",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:CommerceFabric/service-orders:ref:refs/heads/main",
  "description": "GitHub Actions for CommerceFabric/service-orders main branch",
  "audiences": [
    "api://AzureADTokenExchange"
  ]
}
'@

$parameters | Set-Content .\github-federated-credential.json

az ad app federated-credential create `
  --id $CLIENT_ID `
  --parameters .\github-federated-credential.json
```

The federated credential means Azure trusts OIDC tokens issued by GitHub Actions for the specified repository and branch.

For additional microservices, add another federated credential to the **same Azure App Registration**.

For example:

```text
repo:CommerceFabric/service-orders:ref:refs/heads/main
repo:CommerceFabric/service-products:ref:refs/heads/main
```

---

# 6. Give the identity permission to push to ACR

Get the ACR resource ID:

```powershell
$ACR_ID = az acr show `
  --name commercefabricregistry `
  --query id `
  -o tsv
```

Assign the `AcrPush` role:

```powershell
az role assignment create `
  --assignee $CLIENT_ID `
  --role AcrPush `
  --scope $ACR_ID
```

This allows the GitHub Actions identity to push images to:

```text
commercefabricregistry.azurecr.io
```

Verify the role:

```powershell
az role assignment list `
  --assignee $CLIENT_ID `
  --scope $ACR_ID `
  --query "[].{Role:roleDefinitionName,Scope:scope}" `
  -o table
```

Expected:

```text
Role
--------
AcrPush
```

---

# 7. Get the Azure IDs

Get the tenant ID:

```powershell
$TENANT_ID = az account show `
  --query tenantId `
  -o tsv
```

Get the subscription ID:

```powershell
$SUBSCRIPTION_ID = az account show `
  --query id `
  -o tsv
```

The three values required by GitHub are:

```text
AZURE_CLIENT_ID
AZURE_TENANT_ID
AZURE_SUBSCRIPTION_ID
```

---

# 8. Create Docker Hub Personal Access Token

Sign in to Docker Hub and create a **Personal Access Token (PAT)**.

Do not use your Docker Hub password.

The GitHub repository requires:

```text
DOCKERHUB_USERNAME
DOCKERHUB_TOKEN
```

For example:

```text
DOCKERHUB_USERNAME = danielmusselwhite
DOCKERHUB_TOKEN    = <Docker Hub PAT>
```

---

# 9. Add GitHub repository secrets

In the `service-orders` repository:

**Settings → Secrets and variables → Actions → New repository secret**

Add:

```text
AZURE_CLIENT_ID
AZURE_TENANT_ID
AZURE_SUBSCRIPTION_ID

DOCKERHUB_USERNAME
DOCKERHUB_TOKEN
```

These values are not exposed publicly, even though the GitHub repository itself is public.

---

# 10. Configure the GitHub Actions workflow

The workflow uses:

```yaml
permissions:
  id-token: write
  contents: read
```

Azure authentication:

```yaml
- name: Log in to Azure
  uses: azure/login@v2
  with:
    client-id: ${{ secrets.AZURE_CLIENT_ID }}
    tenant-id: ${{ secrets.AZURE_TENANT_ID }}
    subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
```

ACR authentication:

```yaml
- name: Log in to Azure Container Registry
  run: az acr login --name commercefabricregistry
```

Docker Hub authentication:

```yaml
- name: Log in to Docker Hub
  uses: docker/login-action@v3
  with:
    username: ${{ secrets.DOCKERHUB_USERNAME }}
    password: ${{ secrets.DOCKERHUB_TOKEN }}
```

---

# 11. Build and push the Docker image

The Dockerfile is located at:

```text
CommerceFabric.OrdersService/Dockerfile
```

The build context is the repository root:

```yaml
docker build \
  --file CommerceFabric.OrdersService/Dockerfile \
  --tag commercefabricregistry.azurecr.io/orders-microservice:$ORDER_VERSION \
  --tag danielmusselwhite/commercefabric_order_microservice:$ORDER_VERSION \
  .
```

On pushes to `main`, the image is pushed to both registries.

### Azure Container Registry

```text
commercefabricregistry.azurecr.io/orders-microservice:latest
commercefabricregistry.azurecr.io/orders-microservice:<version>
```

### Docker Hub

```text
danielmusselwhite/commercefabric_order_microservice:latest
danielmusselwhite/commercefabric_order_microservice:<version>
```

---

# 12. Overall architecture

```text
GitHub
CommerceFabric/service-orders
          │
          │ Push to main
          ▼
    GitHub Actions
          │
          ├──────── OIDC ────────► Azure Entra ID
          │                              │
          │                              ▼
          │                     github-commercefabric-actions
          │                              │
          │                         AcrPush
          │                              │
          │                              ▼
          │                    Azure Container Registry
          │
          │
          └──────── PAT ─────────► Docker Hub
                                      │
                                      ▼
                    danielmusselwhite/commercefabric_order_microservice
```

## Security model

* No Azure client secret is stored in GitHub.
* Azure authentication uses short-lived GitHub OIDC tokens.
* The Azure identity only has `AcrPush` permission on the ACR.
* Federated credentials are restricted to specific GitHub repositories/branches.
* Docker Hub uses a Personal Access Token rather than the account password.
* Pull requests can build/test the Docker image but do not push it.
