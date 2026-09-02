# Demonstração de CI/CD com Azure DevOps

![Azure DevOps](https://img.shields.io/badge/Azure%20DevOps-CI%2FCD-0078D7) ![.NET](https://img.shields.io/badge/.NET-8.0-512BD4) ![Docker](https://img.shields.io/badge/Docker-Container-2496ED) ![Terraform](https://img.shields.io/badge/Terraform-IaC-844FBA) ![GitVersion](https://img.shields.io/badge/GitVersion-SemVer-blue)

Este repositório apresenta uma implementação prática de uma pipeline de **CI/CD utilizando Azure DevOps**, criada como laboratório e portfólio técnico de DevOps.

A solução utiliza uma aplicação **.NET 8**, testes automatizados, Docker, GitVersion, Terraform e pipelines YAML para demonstrar o ciclo de integração e entrega contínua, desde a criação de uma feature até sua promoção para produção.

O projeto implementa conceitos como:

- Integração contínua (CI)
- Pull Requests e proteção da branch `main`
- Build e testes automatizados
- Cobertura de testes
- Validação de qualidade durante o build
- Cache de pacotes NuGet
- Gerenciamento de secrets com Variable Groups
- Criação e versionamento de imagens Docker
- Pipeline Artifacts
- Estratégia **Build Once, Deploy Many**
- Deploy em HML e PRD
- Health Checks automatizados
- Aprovação manual antes de PRD
- Templates YAML reutilizáveis
- Rollback utilizando versões anteriores da aplicação
- Infrastructure as Code (IaC) com Terraform
- Remote State do Terraform no Azure Storage
- Terraform Plan em pipeline
- Terraform Apply com aprovação manual
- Autenticação Azure via Workload Identity Federation
- Versionamento semântico com GitVersion

> Este é um ambiente de laboratório. Os deployments utilizam agentes hospedados pelo Azure Pipelines e containers temporários, portanto HML e PRD representam ambientes lógicos para demonstração do processo de CI/CD.

## Tecnologias

- **Azure DevOps / Azure Pipelines** — gerenciamento e execução da pipeline
- **YAML** — definição da pipeline e dos templates reutilizáveis
- **.NET 8 / ASP.NET Core** — aplicação utilizada no laboratório
- **xUnit** — testes automatizados
- **Cobertura** — geração e publicação de cobertura de testes
- **Docker** — criação e execução das imagens da aplicação
- **Git e GitHub** — versionamento, branches, Pull Requests e Releases
- **GitVersion** — cálculo de versões semânticas
- **Terraform** — Infrastructure as Code
- **Azure Resource Manager** — provisionamento de recursos Azure
- **Azure Storage** — armazenamento remoto do state do Terraform
- **Pipeline Artifacts** — transporte de artefatos entre stages
- **Variable Groups** — gerenciamento de variáveis e secrets
- **Shell/Bash** — automação de comandos Docker, Terraform e Health Checks

## Fluxo do Pipeline

```text
Feature Branch
      │
      ▼
Pull Request
      │
      ├──────────────► CI
      │                 │
      │                 ├─ Restore / Cache NuGet
      │                 ├─ Build
      │                 ├─ Testes
      │                 ├─ Cobertura
      │                 ├─ Quality Check
      │                 ├─ GitVersion
      │                 └─ Build da imagem Docker
      │
      └──────────────► Terraform Validate + Plan
                        │
                        ▼
                   Artifact tfplan

            Merge aprovado
                  │
                  ▼
                 main
                  │
          ┌───────┴────────┐
          │                │
          ▼                ▼
 Terraform Plan          HML
          │                │
          ▼                ▼
 Aprovação manual      Health Check
          │                │
          ▼                ▼
 Terraform Apply     Aprovação manual
                           │
                           ▼
                          PRD
                           │
                           ▼
                      Health Check
```

A branch `main` é protegida e as alterações são realizadas através de **Pull Requests**. O CI é executado antes do merge para validar o código.

Em Pull Requests, o Terraform executa `init`, `validate` e `plan`, permitindo revisar previamente o impacto de infraestrutura. O `apply` é permitido somente após o merge na `main` e depende de aprovação manual.

Após o merge, a mesma imagem Docker criada pelo CI é promovida entre HML e PRD, seguindo o princípio **Build Once, Deploy Many**.

## Estrutura do Projeto

```text
Azure-DevOps-CICD/
├── SampleApp/
│   ├── Program.cs
│   ├── SampleApp.csproj
│   └── Dockerfile
│
├── SampleApp.Tests/
│   └── HealthCheckTests.cs
│
├── templates/
│   ├── deploy-docker.yml
│   └── rollback-docker.yml
│
├── terraform/
│   └── main.tf
│
├── GitVersion.yml
├── azure-pipelines.yml
├── .gitignore
└── README.md
```

## CI, Testes e Qualidade

O stage de CI prepara e valida a aplicação antes de qualquer promoção.

Entre as validações executadas estão:

- restauração de dependências;
- cache de pacotes NuGet;
- compilação em modo `Release`;
- compilação com warnings tratados como erro;
- execução de testes automatizados com xUnit;
- geração de cobertura de testes;
- publicação do resultado de cobertura no Azure DevOps;
- validação de secrets da pipeline sem exposição de seu conteúdo.

O projeto utiliza o endpoint `/health` para validar automaticamente a disponibilidade da aplicação nos fluxos de deploy e rollback.

## Build Once, Deploy Many

A pipeline utiliza a estratégia **Build Once, Deploy Many**, garantindo que a mesma imagem Docker validada em HML seja posteriormente promovida para PRD.

Durante o stage de CI, a aplicação é compilada, testada e publicada. Em seguida, uma única imagem Docker é criada.

```text
CI
 │
 ▼
Build + Testes
 │
 ▼
dotnet publish
 │
 ▼
docker build
 │
 ▼
Imagem Docker
 │
 ▼
Pipeline Artifact
 │
 ├──────────────► HML
 │                  │
 │                  ▼
 │             mesma imagem
 │
 └──────────────► PRD
                    │
                    ▼
               mesma imagem
```

Os stages de HML e PRD **não realizam um novo build da aplicação ou da imagem Docker**. Eles recuperam a imagem produzida anteriormente pelo CI.

Dessa forma, a versão promovida para PRD é exatamente a mesma que foi validada em HML.

## Versionamento

A pipeline mantém rastreabilidade por meio do `BuildId` do Azure DevOps e também utiliza **GitVersion** para evolução do versionamento semântico.

O histórico Git é obtido com profundidade completa (`fetchDepth: 0`) para permitir que o GitVersion analise commits, branches e tags.

A versão inicial do projeto foi marcada pela tag:

```text
v1.0.0
```

A configuração utiliza mensagens explícitas de SemVer:

```text
+semver: fix       → PATCH
+semver: feature   → MINOR
+semver: breaking  → MAJOR
```

Em Pull Requests, o GitVersion identifica a versão como pré-release, permitindo distinguir builds de validação das versões promovidas pela `main`.

> A integração final da versão calculada pelo GitVersion com o `appVersion` e as tags da imagem Docker ainda está sendo validada neste laboratório.

## Infrastructure as Code com Terraform

A infraestrutura Azure utilizada pelo laboratório também é gerenciada como código.

O Terraform atualmente gerencia:

- Resource Group `rg-cicd-lab`;
- Storage Account utilizado pelo backend remoto;
- container privado para armazenamento do state;
- tags de identificação dos recursos.

O state do Terraform é armazenado remotamente no Azure Storage:

```text
Azure Storage Account
        │
        ▼
Container tfstate
        │
        ▼
terraform.tfstate
```

### Terraform Plan

A pipeline executa:

```text
terraform init
terraform validate
terraform plan -out=tfplan
```

O arquivo `tfplan` é publicado como **Pipeline Artifact**.

### Terraform Apply

O `Terraform Apply`:

- executa somente na branch `main`;
- utiliza o mesmo `tfplan` previamente gerado;
- depende de aprovação no environment `Terraform-Apply`;
- utiliza uma Service Connection com **Workload Identity Federation**.

```text
Terraform Code
      │
      ▼
Validate
      │
      ▼
Plan
      │
      ▼
tfplan Artifact
      │
      ▼
Manual Approval
      │
      ▼
Terraform Apply
      │
      ▼
Azure
```

## Secrets e Variable Groups

A pipeline utiliza o Variable Group:

```text
cicd-variables
```

Secrets são armazenados como variáveis protegidas e consumidos pela pipeline sem exposição do conteúdo nos logs.

O acesso ao Variable Group foi limitado à pipeline necessária, seguindo o princípio de menor privilégio.

## Estratégia de Rollback

A pipeline possui um fluxo específico para rollback, permitindo recuperar e validar uma versão anterior da aplicação utilizando o `BuildId` de uma execução já realizada.

O rollback é iniciado manualmente através do parâmetro:

```text
rollbackBuildId
```

Exemplo:

```text
rollbackBuildId = 24
```

A pipeline recupera o artifact `docker-image` produzido pelo BuildId informado e executa a imagem correspondente.

```text
BuildId anterior
       │
       ▼
Download do Pipeline Artifact
       │
       ▼
Carregar imagem Docker
       │
       ▼
Validar versão
       │
       ▼
Health Check
       │
       ▼
Aprovação manual
       │
       ▼
Rollback PRD
```

> O rollback depende da disponibilidade do Pipeline Artifact da execução anterior. Em um ambiente produtivo, normalmente seria utilizado um Container Registry com políticas adequadas de retenção e versionamento de imagens.

## Templates Reutilizáveis

### Deploy

```text
templates/deploy-docker.yml
```

Centraliza download do artifact, carregamento da imagem, inicialização do container, Health Check e remoção do container.

### Rollback

```text
templates/rollback-docker.yml
```

Centraliza a recuperação, execução e validação de uma versão anterior.

Essa abordagem segue o princípio **DRY (Don't Repeat Yourself)**.

## Proteção da Branch e Pull Requests

A branch `main` possui regras de proteção para evitar alterações diretas no código principal.

As principais regras configuradas são:

- Pull Request obrigatório para alterações na `main`;
- execução do CI antes do merge;
- Status Check do Azure Pipelines obrigatório;
- merge bloqueado quando o CI apresenta falha;
- Force Push bloqueado;
- utilização de **Squash and Merge**.

## Decisões de Arquitetura

Algumas decisões foram adotadas de forma intencional para o escopo do laboratório:

- HML e PRD são environments lógicos do Azure DevOps;
- os containers são executados em agentes hospedados e são temporários;
- a imagem Docker é transportada por Pipeline Artifact;
- a mesma imagem é reutilizada entre ambientes;
- o Terraform utiliza backend remoto;
- `Terraform Plan` e `Terraform Apply` são separados;
- o Apply exige aprovação manual;
- a autenticação Terraform/Azure utiliza identidade federada;
- o rollback reutiliza artifacts de builds anteriores.

## Limitações e Próximas Evoluções

Algumas evoluções possíveis:

- utilização de um **Container Registry**, como ACR ou GHCR;
- deploy em infraestrutura persistente, como Azure Container Apps, App Service, VMs ou Kubernetes;
- gerenciamento de secrets com Azure Key Vault;
- análise estática mais avançada com SonarQube/SonarCloud;
- observabilidade com métricas, logs e alertas;
- deployment Blue-Green ou Canary;
- scanners de segurança de dependências, containers e IaC.

## Objetivo

Este projeto foi desenvolvido com finalidade educacional e de portfólio técnico, com o objetivo de aplicar na prática conceitos relacionados a **DevOps, CI/CD, Infrastructure as Code, automação e estratégias de entrega de software**.

Todo o projeto foi desenvolvido em ambiente próprio e não utiliza código, configurações, credenciais ou informações confidenciais de empresas.

## Conclusão

O projeto demonstra um fluxo de CI/CD contemplando:

- feature branches e Pull Requests;
- proteção da branch `main`;
- build e testes automatizados;
- cobertura de testes;
- quality check;
- cache de dependências;
- gerenciamento de secrets;
- criação e versionamento de imagens Docker;
- Build Once, Deploy Many;
- HML e PRD com Health Checks;
- aprovação manual;
- rollback;
- templates YAML reutilizáveis;
- Terraform e Remote State;
- Terraform Plan e Apply;
- autenticação federada com Azure;
- versionamento semântico com GitVersion.

A implementação foi construída de forma incremental, permitindo validar individualmente cada etapa e evoluir a pipeline mantendo rastreabilidade e controle sobre código, infraestrutura e versões implantadas.
