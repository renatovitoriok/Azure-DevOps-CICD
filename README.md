# Demonstração de CI/CD com Azure DevOps

Este repositório apresenta uma implementação prática de uma pipeline de **CI/CD utilizando Azure DevOps**, criada como laboratório e portfólio técnico de DevOps.

A solução utiliza uma aplicação **.NET 8**, testes automatizados, Docker e pipelines YAML para demonstrar o ciclo completo de integração e entrega contínua, desde a criação de uma feature até sua promoção para produção.

O projeto implementa conceitos como:

- Integração contínua (CI)
- Pull Requests e proteção da branch `main`
- Build e testes automatizados
- Criação e versionamento de imagens Docker
- Pipeline Artifacts
- Estratégia **Build Once, Deploy Many**
- Deploy em HML e PRD
- Health Checks automatizados
- Aprovação manual antes de PRD
- Templates YAML reutilizáveis
- Rollback utilizando versões anteriores da aplicação

> Este é um ambiente de laboratório. Os deployments utilizam agentes hospedados pelo Azure Pipelines e containers temporários, portanto HML e PRD representam ambientes lógicos para demonstração do processo de CI/CD.

## Tecnologias

- **Azure DevOps** — gerenciamento e execução da pipeline de CI/CD
- **Azure Pipelines** — automação dos processos de build, testes, deploy e rollback
- **YAML** — definição da pipeline e dos templates reutilizáveis
- **.NET 8 / ASP.NET Core** — desenvolvimento da aplicação utilizada no laboratório
- **xUnit** — testes automatizados da aplicação
- **Docker** — criação e execução das imagens da aplicação
- **Git e GitHub** — versionamento de código, branches e Pull Requests
- **Pipeline Artifacts** — armazenamento e transporte da imagem Docker entre os stages
- **Shell/Bash** — automação de comandos Docker e Health Checks

## Fluxo do Pipeline

O projeto implementa um fluxo completo de CI/CD, desde o desenvolvimento de uma nova funcionalidade até sua promoção para produção.

### Fluxo principal

```text
Feature Branch
      │
      ▼
Pull Request
      │
      ▼
CI - Build e Testes
      │
      ▼
Merge na main
      │
      ▼
Build da aplicação
      │
      ▼
Testes automatizados
      │
      ▼
dotnet publish
      │
      ▼
Build da imagem Docker
      │
      ▼
Pipeline Artifact
      │
      ▼
Deploy HML
      │
      ▼
Health Check
      │
      ▼
Aprovação manual
      │
      ▼
Deploy PRD
      │
      ▼
Health Check


```

A branch `main` é protegida e as alterações são realizadas através de **Pull Requests**. O CI é executado antes do merge para validar o código.

Após o merge, uma nova execução da pipeline realiza o build da aplicação e da imagem Docker. A mesma imagem é promovida entre HML e PRD, seguindo o princípio **Build Once, Deploy Many**.

Antes do deployment em PRD, o Azure DevOps exige uma **aprovação manual**, permitindo validar o resultado obtido em HML antes da promoção.

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
├── azure-pipelines.yml
├── .gitignore
└── README.md

```

## Build Once, Deploy Many

A pipeline utiliza a estratégia **Build Once, Deploy Many**, garantindo que a mesma imagem Docker validada em HML seja posteriormente promovida para PRD.

Durante o stage de CI, a aplicação é compilada, testada e publicada. Em seguida, uma única imagem Docker é criada e identificada utilizando o `BuildId` da execução do Azure DevOps.

Exemplo:

```text
CI
 │
 ▼
Build + Testes
 │
 ▼
docker build
 │
 ▼
sampleapp:123
 │
 ▼
Pipeline Artifact
 │
 ├──────────────► HML
 │                 │
 │                 ▼
 │            sampleapp:123
 │
 └──────────────► PRD
                   │
                   ▼
              sampleapp:123
```

Os stages de HML e PRD **não realizam um novo build da aplicação ou da imagem Docker**. Eles recuperam e executam a imagem produzida anteriormente pelo CI.

Dessa forma, a versão promovida para PRD é exatamente a mesma que foi validada em HML, reduzindo o risco de diferenças entre os artefatos utilizados nos ambientes.

## Estratégia de Rollback

A pipeline possui um fluxo específico para rollback, permitindo recuperar e validar uma versão anterior da aplicação utilizando o `BuildId` de uma execução já realizada.

O rollback é iniciado manualmente através do parâmetro:

```text
rollbackBuildId
```

Por exemplo, ao informar:

```text
rollbackBuildId = 24
```

a pipeline recupera o artefato `docker-image` produzido pelo BuildId `24`, carrega a imagem Docker correspondente e executa a versão:

```text
sampleapp:24
```

### Fluxo de Rollback

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
Rollback HML
       │
       ▼
Health Check
       │
       ▼
Aprovação manual
       │
       ▼
Rollback PRD
       │
       ▼
Health Check
```

Durante uma execução de rollback, os stages normais de CI, HML e PRD são ignorados por condições definidas na pipeline.

A versão anterior é primeiro validada através de um Health Check. Após a validação, a promoção do rollback para PRD passa pela mesma aprovação manual utilizada no fluxo normal de deployment.

A lógica comum de rollback foi centralizada no template:

```text
templates/rollback-docker.yml
```

Esse template é reutilizado tanto na validação inicial quanto no rollback de PRD, reduzindo duplicação de código no arquivo principal da pipeline.

> O rollback depende da disponibilidade do Pipeline Artifact da execução anterior. Em um ambiente produtivo, normalmente seria utilizado um Container Registry com políticas adequadas de retenção e versionamento de imagens.

## Templates Reutilizáveis

Para reduzir duplicação de código e facilitar a manutenção da pipeline, os processos comuns de deploy e rollback foram separados em templates YAML reutilizáveis.

### Template de Deploy

O arquivo:

```text
templates/deploy-docker.yml
```

centraliza os steps utilizados pelos ambientes HML e PRD, incluindo:

- Download do Pipeline Artifact
- Carregamento da imagem Docker
- Inicialização do container
- Execução do Health Check
- Remoção do container após a validação

Os stages passam parâmetros como nome do ambiente e nome do container, permitindo que a mesma implementação seja utilizada em HML e PRD.

```text
                    deploy-docker.yml
                    /               \
                   ▼                 ▼
                 HML                PRD
```

### Template de Rollback

O arquivo:

```text
templates/rollback-docker.yml
```

centraliza a lógica necessária para recuperar e executar uma versão anterior da aplicação.

O template recebe o `rollbackBuildId` e utiliza esse identificador para localizar o Pipeline Artifact correspondente à versão selecionada.

```text
                  rollback-docker.yml
                    /               \
                   ▼                 ▼
            Rollback HML       Rollback PRD
```

Essa abordagem segue o princípio **DRY (Don't Repeat Yourself)**, reduzindo código duplicado e permitindo que alterações na lógica comum sejam realizadas em um único local.

## Proteção da Branch e Pull Requests

A branch `main` possui regras de proteção para evitar alterações diretas no código principal e garantir que as mudanças sejam validadas antes do merge.

O fluxo de desenvolvimento utilizado no projeto é:

```text
main
 │
 └──► feature/*
          │
          ▼
     Alteração do código
          │
          ▼
     Pull Request
          │
          ▼
     Azure Pipelines CI
          │
          ▼
     Build + Testes
          │
       Sucesso?
       /     \
     Não     Sim
      │       │
      ▼       ▼
 Bloqueia   Merge
  merge      main
```

As principais regras configuradas são:

- Pull Request obrigatório para alterações na `main`
- Execução do CI antes do merge
- Status Check do Azure Pipelines obrigatório
- Merge bloqueado quando o CI apresenta falha
- Force Push bloqueado
- Utilização de **Squash and Merge** para manter o histórico da `main` mais organizado

Essa configuração garante que alterações que não compilam ou que provoquem falhas nos testes automatizados não sejam promovidas para a branch principal.

## Limitações e Próximas Evoluções

Este projeto foi desenvolvido como laboratório técnico de CI/CD e, por isso, utiliza uma infraestrutura simplificada para demonstrar os conceitos de automação, promoção entre ambientes e rollback.

Atualmente, os deployments de HML e PRD são executados em agentes hospedados pelo Azure Pipelines (`ubuntu-latest`). Esses agentes são temporários, portanto os containers utilizados durante os deployments não permanecem em execução após o término dos jobs.

Além disso, a imagem Docker é armazenada como **Pipeline Artifact**, em vez de ser publicada em um Container Registry.

Em um cenário produtivo, algumas evoluções possíveis seriam:

- Utilização de um **Container Registry**, como Azure Container Registry (ACR), GitHub Container Registry (GHCR) ou equivalente
- Deploy em infraestrutura persistente, como Azure Container Apps, App Service, máquinas virtuais ou Kubernetes
- Versionamento e retenção de imagens em um Container Registry
- Gerenciamento de secrets utilizando Azure Key Vault ou solução equivalente
- Implementação de Infrastructure as Code (IaC) utilizando Terraform ou Bicep
- Inclusão de análise estática de código e controles adicionais de segurança
- Implementação de observabilidade com métricas, logs e alertas
- Evolução da estratégia de deployment para modelos como Blue-Green ou Canary

A arquitetura atual permite que essas melhorias sejam incorporadas gradualmente sem alterar os principais conceitos de CI/CD demonstrados pelo projeto.

## Objetivo

Este projeto foi desenvolvido com finalidade educacional e de portfólio técnico, com o objetivo de aplicar na prática conceitos relacionados a **DevOps, CI/CD, automação e estratégias de entrega de software**.

O laboratório busca demonstrar não apenas a criação de uma pipeline, mas também decisões relacionadas a qualidade, rastreabilidade, reutilização, promoção entre ambientes e recuperação de versões anteriores.

Todo o projeto foi desenvolvido em ambiente próprio e não utiliza código, configurações, credenciais ou informações confidenciais de empresas.

## Conclusão

O projeto demonstra um fluxo de CI/CD automatizado contemplando:

- Desenvolvimento através de feature branches e Pull Requests
- Proteção da branch `main`
- Build e testes automatizados
- Criação e versionamento de imagens Docker
- Estratégia Build Once, Deploy Many
- Deploy automatizado em HML
- Health Checks
- Aprovação manual para PRD
- Promoção da mesma imagem para PRD
- Rollback baseado em versões anteriores
- Templates YAML reutilizáveis

A implementação foi construída de forma incremental, permitindo validar individualmente cada etapa do processo e evoluir a pipeline mantendo rastreabilidade e controle sobre as versões implantadas.
