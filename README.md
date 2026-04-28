# 🏦 Algoritmo do Banqueiro em C#

## 📋 Descrição

Este projeto implementa o **Algoritmo do Banqueiro** (Banker's Algorithm), desenvolvido originalmente por Edsger Dijkstra, para prevenção de deadlocks em sistemas operacionais.

O sistema simula um ambiente multithread onde múltiplos clientes (processos) competem por recursos limitados. O "banqueiro" atua como gerenciador de recursos, garantindo que o sistema permaneça sempre em um **estado seguro**, prevenindo deadlocks.

### Conceitos Aplicados

- **Programação Concorrente**: Múltiplas threads executando simultaneamente
- **Mutex/Lock**: Sincronização para prevenir condições de corrida
- **Prevenção de Deadlock**: Algoritmo de segurança para validação de estados
- **Alocação de Recursos**: Gerenciamento dinâmico de recursos

## 🎯 Funcionalidades

- ✅ Simulação de 5 clientes (threads) concorrentes
- ✅ Alocação dinâmica de recursos com verificação de estado seguro
- ✅ Algoritmo de segurança (Safety Algorithm) completo
- ✅ Liberação parcial ou total de recursos
- ✅ Interface visual colorida no console
- ✅ Logs detalhados de todas as operações
- ✅ Geração automática de demandas máximas

## 🛠️ Tecnologias

- **Linguagem**: C# 12
- **Framework**: .NET 8
- **Conceitos**: Threads, Mutex, Lock, Concorrência

## 📁 Estrutura do Projeto

```
BankerAlgorithm/
├── Program.cs      # Ponto de entrada e controle da simulação
├── Banker.cs       # Implementação do Algoritmo do Banqueiro
├── Customer.cs     # Classe que representa os clientes (threads)
├── README.md       # Documentação do projeto
└── .gitignore      # Arquivos ignorados pelo Git
```

## 🚀 Como Executar

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) ou superior

### Passos

1. **Clone o repositório** (ou navegue até a pasta do projeto):
   ```bash
   cd BankerAlgorithm
   ```

2. **Restaure as dependências**:
   ```bash
   dotnet restore
   ```

3. **Execute o programa** com os recursos desejados:
   ```bash
   dotnet run 10 5 7
   ```

### Parâmetros

Os argumentos da linha de comando definem a quantidade de cada tipo de recurso:

| Comando | Significado |
|---------|-------------|
| `dotnet run 10 5 7` | 3 tipos de recursos: R0=10, R1=5, R2=7 |
| `dotnet run 8 4` | 2 tipos de recursos: R0=8, R1=4 |
| `dotnet run 15 10 8 5` | 4 tipos de recursos |

## 📊 Matrizes Utilizadas

| Matriz | Descrição |
|--------|-----------|
| `available[]` | Recursos disponíveis no sistema |
| `maximum[,]` | Demanda máxima de cada cliente |
| `allocation[,]` | Recursos atualmente alocados |
| `need[,]` | Recursos ainda necessários (`maximum - allocation`) |

## 🔒 Sincronização

Todas as operações críticas utilizam `lock` para garantir:
- **Exclusão mútua**: Apenas uma thread acessa os recursos por vez
- **Consistência**: As matrizes sempre refletem um estado válido
- **Atomicidade**: Operações de alocação e liberação são atômicas

```csharp
lock (_lockObject)
{
    // Operação atômica nas matrizes
}
```

## 📸 Exemplo de Saída

```
╔════════════════════════════════════════════════════════════════╗
║       ALGORITMO DO BANQUEIRO - SISTEMAS OPERACIONAIS           ║
╚════════════════════════════════════════════════════════════════╝

[CLIENTE 2] Solicitando: [1,0,2]
[APROVADO] Estado seguro encontrado: <0,2,1,3,4>

[CLIENTE 0] Solicitando: [3,2,1]
[NEGADO] Estado inseguro - possível deadlock detectado!

[CLIENTE 1] Liberou: [1,0,1]
```

## 📖 Algoritmo de Segurança

O algoritmo verifica se existe uma sequência de execução que permite que todos os processos terminem:

1. Inicializa `work = available` e `finish[i] = false`
2. Encontra um processo `i` onde `finish[i] == false` e `need[i] <= work`
3. Se encontrado: `work = work + allocation[i]`, `finish[i] = true`
4. Repete até que todos os processos terminem ou não seja possível continuar
5. Se todos terminaram: **ESTADO SEGURO** ✅
6. Caso contrário: **ESTADO INSEGURO** ❌

## 👨‍💻 Autor

**[Cauã Moreira Martins e Pedro Nunes]**

Trabalho Prático - Disciplina de Sistemas Operacionais

## 📄 Licença

Este projeto é de uso acadêmico.

---

*Desenvolvido como parte do curso de Sistemas Operacionais*
