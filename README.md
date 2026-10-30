# Fiap.Notifications

Serviço de notificações da plataforma. Ele **consome eventos publicados no RabbitMQ** e registra o envio das notificações correspondentes:

| Evento | Fila | Ação |
|---|---|---|
| Usuário criado | `user-created-event` | Envia e-mail de boas-vindas |
| Pagamento processado | `notificacao-payment-processed-event` | Envia e-mail de confirmação quando o pagamento é **aprovado** |
| Notificação padrão | `notifications-default` | Processamento padrão de notificações |

> O envio é registrado no console da aplicação (por exemplo, `Pagamento Aprovado: Email de confirmação foi enviado para o endereço: ...`).

## Estrutura do projeto

```
.
├── Fiap.Notifications.Api/        # API ASP.NET Core 8 (Swagger, consumidores, configuração)
│   └── Dockerfile
├── Fiap.Notifications.Domain/     # Eventos e regras de domínio
├── docker-compose.yml             # Ambiente local (RabbitMQ + API)
└── k8s/                           # Manifestos Kubernetes
    ├── 01-secret-rabbitmq.yaml
    ├── 02-configmap-rabbitmq.yaml
    ├── 03-deployment-rabbitmq.yaml
    ├── 04-deployment-notifications.yaml
    └── 05-svc-notifications-api.yaml
```

## Pré-requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download) (execução local sem container)
- [Docker](https://www.docker.com/) e Docker Compose
- [kubectl](https://kubernetes.io/docs/tasks/tools/) e um cluster local (kind, minikube ou Docker Desktop) para o deploy em Kubernetes

## Configuração

A aplicação lê as configurações do RabbitMQ pela seção `RabbitMQ`. Em variáveis de ambiente, o `:` vira `__`.

| Variável | Descrição | Exemplo |
|---|---|---|
| `RabbitMQ__HostName` | Host do RabbitMQ (**obrigatório**) | `rabbitmq` (Compose) / `rabbitmq-service` (Kubernetes) |
| `RabbitMQ__Port` | Porta AMQP | `5672` |
| `RabbitMQ__UserName` | Usuário | `admin` |
| `RabbitMQ__Password` | Senha | `admin1234` |
| `RabbitMQ__VirtualHost` | Virtual host | `/` |
| `ASPNETCORE_URLS` | Endereço de escuta da API | `http://+:8083` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente da aplicação | `Development` |

## Executando com Docker Compose

O `docker-compose.yml` sobe o RabbitMQ (com volume e healthcheck) e a API, que só inicia depois que o RabbitMQ estiver saudável.

```bash
docker compose up --build -d
docker compose logs -f api-notifications
docker compose down            # para os containers (mantém o volume)
docker compose down -v         # para e apaga o volume do RabbitMQ
```

| Serviço | URL |
|---|---|
| Swagger da API | http://localhost:8083/swagger/index.html |
| RabbitMQ (painel) | http://localhost:15672 (usuário `admin`, senha `admin1234`) |
| RabbitMQ (AMQP) | `localhost:5672` |

## Docker

O `Dockerfile` usa **multi-stage build** (`base` → `build` → `publish` → `final`) com as imagens oficiais da Microsoft: SDK 8.0 para compilar e ASP.NET 8.0 para executar. O contexto de build é a **raiz da solução**, porque o Dockerfile copia os projetos `Fiap.Notifications.Api` e `Fiap.Notifications.Domain`.

```bash
docker build -f Fiap.Notifications.Api/Dockerfile -t <usuario>/src-api-notifications:latest .
docker push <usuario>/src-api-notifications:latest
```

A imagem executa com o usuário não-root `app` e expõe a porta `8083`.

## Executando no Kubernetes

### Recursos

| Arquivo | Recurso | Descrição |
|---|---|---|
| `01-secret-rabbitmq.yaml` | Secret `rabbitmq-credentials` | Senha do RabbitMQ (`RABBITMQ_PASSWORD`) |
| `02-configmap-rabbitmq.yaml` | ConfigMap `configmap-rabbitmq` | Host, porta, usuário e virtual host |
| `03-deployment-rabbitmq.yaml` | Deployment + Service `rabbitmq-service` | RabbitMQ 3.13 com painel (portas 5672 e 15672) |
| `04-deployment-notifications.yaml` | Deployment `notifications-api-deployment` | API, com `initContainer` que aguarda o RabbitMQ |
| `05-svc-notifications-api.yaml` | Service `notifications-api` | `ClusterIP`, porta **9093** → container 8083 |

### Aplicar

Os arquivos têm prefixo numérico para indicar a ordem. O Secret e o ConfigMap precisam existir antes dos Deployments.

```bash
kubectl apply -f k8s/
kubectl get pod,svc
```

Para remover tudo: `kubectl delete -f k8s/`.

### Comunicação entre serviços

Dentro do cluster, os serviços se encontram pelo **nome do Service**:

| Serviço | Endereço no cluster | Porta do container |
|---|---|---|
| notifications-api | `http://notifications-api:9093` | 8083 |
| rabbitmq-service | `rabbitmq-service:5672` | 5672 |

Os demais serviços da plataforma seguem o mesmo padrão: `users-api:8091`, `catalog-api:8092` e `payments-api:8094`.

### Acessando da sua máquina

Nomes de Service só existem dentro do cluster. Para abrir no navegador:

1. Adicione o nome ao arquivo `hosts` (edite como administrador):

   ```
   # Windows: C:\Windows\System32\drivers\etc\hosts
   127.0.0.1 notifications-api
   ```

2. Mantenha um port-forward ativo, em um terminal separado:

   ```bash
   kubectl port-forward svc/notifications-api 9093:9093
   ```

3. Acesse http://notifications-api:9093/swagger/index.html.

Alternativa sem editar o `hosts`: http://127.0.0.1:9093/swagger/index.html.

Para o painel do RabbitMQ: `kubectl port-forward svc/rabbitmq-service 15672:15672` e abra http://localhost:15672.

### Verificação

```bash
kubectl get pod,svc,endpoints
kubectl logs deploy/notifications-api-deployment -c container-notifications-api -f
```

Quando o consumidor estiver ativo, os logs mostram `Aguardando mensagens da fila '...'`.

## Problemas comuns

| Sintoma | Causa provável | O que fazer |
|---|---|---|
| Pod em `CreateContainerConfigError` | Secret ou ConfigMap ainda não existem | Aplicar `01` e `02` antes dos Deployments |
| `Tentativa N/10 falhou: None of the specified endpoints were reachable` | RabbitMQ ainda subindo | Aguardar. A aplicação tenta reconectar |
| `DNS_PROBE_FINISHED_NXDOMAIN` no navegador | O nome não está no `hosts` | Adicionar a entrada (veja acima) |
| `ERR_CONNECTION_REFUSED` | Port-forward parado ou nome apontando para outro IP | Subir o `port-forward` e conferir o `hosts` com `ping notifications-api` |
| `Unable to listen on port ...: bind` | Porta já em uso (port-forward antigo) | Encerrar o processo `kubectl` anterior |
| `Endpoints` do Service como `<none>` | Selector não bate ou pod não está `Ready` | Conferir `app: svc-notifications-api` e `kubectl describe pod` |

## Pontos de atenção

- **Credenciais em texto puro:** o Secret e o `docker-compose.yml` trazem a senha `admin1234`. Serve para estudo. Em produção, crie o Secret por `kubectl create secret` (ou cofre de segredos) e não o versione.
- **Ambiente `Development`:** o Dockerfile e o Deployment fixam `ASPNETCORE_ENVIRONMENT=Development`. Em produção, use `Production`.
- **Imagem `:latest`:** com `imagePullPolicy: Always`, dificulta o rollback. Prefira uma tag de versão.
- **Versões do RabbitMQ:** o Compose usa `3.11-management` e o Kubernetes usa `3.13-management`.
- **Sem persistência no Kubernetes:** o RabbitMQ do cluster não tem volume, então filas e mensagens se perdem se o pod for recriado.
- **APIS:** para simular o processo completo é preciso baixar as apis: catalog, payments e user.
