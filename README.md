# Demonstração de CI/CD com Azure DevOps

Este repositório demonstra uma implementação de CI/CD utilizando Azure DevOps, pipelines YAML e automação com PowerShell.

O projeto foi criado como portfólio técnico para demonstrar, na prática, conceitos de DevOps, incluindo automação de build, gerenciamento de artefatos, deploy, validação de ambientes e estratégias de rollback.

## Tecnologias

- Azure DevOps
- Azure Pipelines
- YAML
- PowerShell
- Git
- .NET
- CI/CD

## Fluxo do Pipeline

O pipeline implementará o seguinte fluxo:

Commit
→ Build
→ Testes
→ Geração do Artefato
→ Deploy em Homologação
→ Health Check
→ Aprovação
→ Deploy em Produção
→ Health Check
→ Estratégia de Rollback

## Estrutura do Projeto

```text
azure-devops-cicd-demo/
├── src/
├── scripts/
├── docs/
├── azure-pipelines.yml
└── README.md

```

## Objetivo

Este projeto possui finalidade educacional e de portfólio, demonstrando boas práticas de DevOps e automação de processos de entrega de software em um ambiente controlado.

Nenhum código, configuração ou informação confidencial de empresas é utilizado neste repositório.
