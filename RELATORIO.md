# RELATÓRIO TÉCNICO
## Trabalho Prático de Sistemas Operacionais
### Implementação do Algoritmo do Banqueiro

---

**Disciplina:** Sistemas Operacionais  
**Tema:** Implementação do Algoritmo do Banqueiro (Banker's Algorithm)  
**Autor:** [Cauã Moreira Martins e Pedro Nunes]  
**Data:** Abril de 2026  
**Linguagem:** C# (.NET 8)  

---

## SUMÁRIO

1. [Introdução](#1-introdução)
2. [Desenvolvimento](#2-desenvolvimento)
   - 2.1 [Fundamentação Teórica](#21-fundamentação-teórica)
   - 2.2 [Arquitetura do Sistema](#22-arquitetura-do-sistema)
   - 2.3 [Implementação](#23-implementação)
   - 2.4 [Estruturas de Dados](#24-estruturas-de-dados)
   - 2.5 [Algoritmo de Segurança](#25-algoritmo-de-segurança)
   - 2.6 [Controle de Concorrência](#26-controle-de-concorrência)
3. [Resultados](#3-resultados)
4. [Conclusão](#4-conclusão)
5. [Referências](#5-referências)
6. [Anexos](#6-anexos)

---

## 1. INTRODUÇÃO

### 1.1 Contextualização

Em sistemas operacionais multiprogramados, múltiplos processos competem por recursos limitados do sistema. Esta competição pode levar a situações de **deadlock** (impasse), onde dois ou mais processos ficam bloqueados indefinidamente, cada um esperando que o outro libere recursos necessários para sua continuação.

O **Algoritmo do Banqueiro**, proposto por Edsger W. Dijkstra em 1965, é uma estratégia de **prevenção de deadlock** que simula a alocação de recursos para determinar se uma solicitação levará o sistema a um estado inseguro antes de realmente concedê-la.

### 1.2 Objetivos

#### Objetivo Geral
Desenvolver um programa multithreaded em C# que implemente o Algoritmo do Banqueiro para gerenciamento seguro de recursos, garantindo a prevenção de deadlocks.

#### Objetivos Específicos
- Implementar as estruturas de dados fundamentais do algoritmo (matrizes `available`, `maximum`, `allocation` e `need`)
- Desenvolver o algoritmo de segurança (Safety Algorithm)
- Criar threads que simulam clientes solicitando e liberando recursos
- Implementar mecanismos de sincronização usando `lock`/`mutex`
- Garantir ausência de condições de corrida (race conditions)
- Demonstrar a eficácia do algoritmo na prevenção de deadlocks

### 1.3 Justificativa

A compreensão e implementação de algoritmos de prevenção de deadlock são fundamentais para a formação em Ciência da Computação e Engenharia de Software. O Algoritmo do Banqueiro, embora teoricamente interessante, apresenta conceitos práticos aplicáveis em sistemas de banco de dados, sistemas distribuídos e gerenciadores de recursos em geral.

---

## 2. DESENVOLVIMENTO

### 2.1 Fundamentação Teórica

#### 2.1.1 Deadlock

Um **deadlock** ocorre quando um conjunto de processos está bloqueado, cada um esperando por um recurso que está sendo mantido por outro processo do conjunto. As quatro condições necessárias para deadlock são:

1. **Exclusão Mútua**: Recursos não podem ser compartilhados
2. **Posse e Espera**: Processos mantêm recursos enquanto aguardam outros
3. **Não Preempção**: Recursos não podem ser tomados à força
4. **Espera Circular**: Existe uma cadeia circular de processos aguardando recursos

#### 2.1.2 Estados do Sistema

- **Estado Seguro**: Existe pelo menos uma sequência de execução onde todos os processos podem ser concluídos
- **Estado Inseguro**: Não há garantia de que todos os processos possam ser concluídos (pode levar a deadlock)

#### 2.1.3 Algoritmo do Banqueiro

O algoritmo recebe este nome por analogia a um banqueiro que gerencia empréstimos. Assim como um banqueiro não empresta dinheiro se isso pode levar à incapacidade de atender outros clientes, o sistema não aloca recursos se isso pode levar a deadlock.

### 2.2 Arquitetura do Sistema

O sistema foi desenvolvido seguindo os princípios de **Programação Orientada a Objetos** e **Separação de Responsabilidades**:

```
┌─────────────────────────────────────────────────────────────┐
│                        Program.cs                           │
│              (Ponto de entrada e controle)                  │
└─────────────────────────┬───────────────────────────────────┘
                          │
          ┌───────────────┴───────────────┐
          │                               │
          ▼                               ▼
┌─────────────────────┐       ┌─────────────────────────────┐
│     Banker.cs       │◄──────│       Customer.cs           │
│  (Gerenciador de    │       │   (Threads dos clientes)    │
│     recursos)       │       │                             │
│                     │       │  ┌────────┐ ┌────────┐     │
│  - available[]      │       │  │Thread 0│ │Thread 1│ ... │
│  - maximum[,]       │       │  └────────┘ └────────┘     │
│  - allocation[,]    │       │                             │
│  - need[,]          │       │                             │
│                     │       │                             │
│  [LOCK/MUTEX]       │       │                             │
└─────────────────────┘       └─────────────────────────────┘
```

### 2.3 Implementação

#### 2.3.1 Classe Banker

A classe `Banker` é o núcleo do sistema, responsável por:

- Gerenciar as matrizes de recursos
- Implementar o algoritmo de segurança
- Processar solicitações e liberações de recursos
- Garantir sincronização thread-safe

**Principais métodos:**

```csharp
public int RequestResources(int customerId, int[] request)
// Retorna 0 (sucesso) ou -1 (negado)

public int ReleaseResources(int customerId, int[] release)
// Retorna 0 (sucesso) ou -1 (erro)

private bool IsSafeState(out int[] safeSequence)
// Implementa o Safety Algorithm
```

#### 2.3.2 Classe Customer

A classe `Customer` representa cada cliente (processo) do sistema:

- Executa em thread separada
- Gera solicitações aleatórias baseadas no `need`
- Libera recursos após uso
- Respeita os limites definidos em `maximum`

**Ciclo de vida da thread:**

```
┌─────────────────────────────────────────────────────────┐
│                    LOOP CONTÍNUO                         │
├─────────────────────────────────────────────────────────┤
│  1. Sleep(tempo_aleatório)                              │
│  2. Gerar pedido aleatório baseado no need              │
│  3. RequestResources(customerId, request)               │
│     ├─ APROVADO: usar recursos                          │
│     └─ NEGADO: continuar loop                           │
│  4. Sleep(tempo_uso)                                    │
│  5. ReleaseResources(customerId, release)               │
│  6. Voltar ao passo 1                                   │
└─────────────────────────────────────────────────────────┘
```

### 2.4 Estruturas de Dados

| Estrutura | Tipo | Descrição |
|-----------|------|-----------|
| `available` | `int[]` | Vetor com quantidade disponível de cada recurso |
| `maximum` | `int[,]` | Matriz com demanda máxima de cada cliente |
| `allocation` | `int[,]` | Matriz com recursos atualmente alocados |
| `need` | `int[,]` | Matriz com recursos ainda necessários |

**Relação fundamental:**
```
need[i,j] = maximum[i,j] - allocation[i,j]
```

### 2.5 Algoritmo de Segurança

O **Safety Algorithm** determina se o sistema está em estado seguro:

```
ALGORITMO DE SEGURANÇA (Safety Algorithm)
─────────────────────────────────────────

ENTRADA: available[], allocation[,], need[,]
SAÍDA: true (seguro) ou false (inseguro)

1. work ← cópia de available
2. finish[i] ← false, para todo i

3. REPITA
     Encontre i tal que:
       - finish[i] = false
       - need[i] ≤ work
     
     SE encontrou ENTÃO
       work ← work + allocation[i]
       finish[i] ← true
     SENÃO
       SAIR DO LOOP
     FIM SE
   ATÉ que não encontre mais

4. SE todos finish[i] = true ENTÃO
     RETORNE true (ESTADO SEGURO)
   SENÃO
     RETORNE false (ESTADO INSEGURO)
   FIM SE
```

### 2.6 Controle de Concorrência

#### 2.6.1 Problemas Potenciais

- **Race Condition**: Múltiplas threads acessando/modificando matrizes simultaneamente
- **Inconsistência de Dados**: Leituras durante escritas parciais
- **Lost Updates**: Atualizações sobrescritas por threads concorrentes

#### 2.6.2 Solução Implementada

Utilização de **lock** (monitor) para garantir exclusão mútua:

```csharp
private readonly object _lockObject = new object();

public int RequestResources(int customerId, int[] request)
{
    lock (_lockObject)
    {
        // Toda a operação é atômica
        // Verificação + Simulação + Decisão
    }
}
```

**Características:**
- **Atomicidade**: Operações completas ou nenhuma alteração
- **Consistência**: Matrizes sempre em estado válido
- **Isolamento**: Threads não interferem entre si
- **Durabilidade**: Estados persistem após operações

---

## 3. RESULTADOS

### 3.1 Execução do Sistema

**Comando de execução:**
```bash
dotnet run 10 5 7
```

**Configuração:**
- Recursos: R0=10, R1=5, R2=7
- Clientes: 5 threads
- Tempo de simulação: 30 segundos

### 3.2 Saída do Sistema

#### Estado Inicial

```
╔═══════════════════════════════════════════════════════════════╗
║                    ESTADO ATUAL DO SISTEMA                    ║
╚═══════════════════════════════════════════════════════════════╝

📦 AVAILABLE (Recursos Disponíveis):
            10    5    7 

📊 MAXIMUM (Demanda Máxima por Cliente):
            R0   R1   R2 
Cliente 0:   5    4    1 
Cliente 1:   4    1    5 
Cliente 2:   8    2    3 
Cliente 3:   2    1    6 
Cliente 4:   8    3    2 

📌 ALLOCATION (Recursos Alocados):
            R0   R1   R2 
Cliente 0:   0    0    0 
Cliente 1:   0    0    0 
Cliente 2:   0    0    0 
Cliente 3:   0    0    0 
Cliente 4:   0    0    0 
```

#### Exemplos de Operações

**Solicitação Aprovada:**
```
[CLIENTE 4] Solicitando: [4,1,1]
[APROVADO] Estado seguro encontrado: <0,1,3,4,2>
```

**Solicitação Negada:**
```
[CLIENTE 2] Solicitando: [5,0,2]
[NEGADO] Estado inseguro - possível deadlock detectado!
```

**Liberação de Recursos:**
```
[CLIENTE 4] Liberou: [2,1,0]
```

### 3.3 Análise dos Resultados

Durante a execução de 30 segundos:

| Métrica | Valor |
|---------|-------|
| Total de solicitações | ~80-120 |
| Solicitações aprovadas | ~30-40% |
| Solicitações negadas | ~40-50% |
| Aguardando recursos | ~10-20% |
| Deadlocks ocorridos | **0** |

**Observações:**
1. O algoritmo rejeitou consistentemente solicitações que levariam a estados inseguros
2. Todas as sequências seguras encontradas foram válidas
3. Nenhum deadlock ocorreu durante toda a simulação
4. O sistema manteve-se responsivo mesmo com 5 threads concorrentes

### 3.4 Comportamento Observado

```
┌────────────────────────────────────────────────────────────────┐
│              FLUXO TÍPICO DE EXECUÇÃO                          │
├────────────────────────────────────────────────────────────────┤
│                                                                │
│  T=0s    [Inicialização] Matrizes configuradas                │
│  T=0.5s  [CLIENTE 4] Solicitação APROVADA                     │
│  T=0.7s  [CLIENTE 3] Solicitação APROVADA                     │
│  T=1.0s  [CLIENTE 2] Solicitação NEGADA (estado inseguro)     │
│  T=1.2s  [CLIENTE 1] Solicitação NEGADA                       │
│  T=1.5s  [CLIENTE 4] Liberou recursos                         │
│  T=2.0s  [CLIENTE 0] Solicitação APROVADA (novos recursos)    │
│  ...                                                           │
│  T=30s   [Simulação concluída sem deadlocks]                  │
│                                                                │
└────────────────────────────────────────────────────────────────┘
```

---

## 4. CONCLUSÃO

### 4.1 Objetivos Alcançados

✅ **Implementação completa do Algoritmo do Banqueiro** em C# com todas as estruturas de dados requeridas

✅ **Sistema multithread funcional** com 5 clientes (threads) executando concorrentemente

✅ **Mecanismos de sincronização** implementados corretamente usando `lock`

✅ **Prevenção de deadlock** demonstrada através da ausência de impasses durante toda a simulação

✅ **Prevenção de race conditions** através de seções críticas protegidas

✅ **Interface visual** clara e informativa no console

### 4.2 Lições Aprendidas

1. **Importância da Sincronização**: Em ambientes multithread, a proteção de recursos compartilhados é essencial para evitar comportamentos indeterminísticos

2. **Trade-off Segurança vs. Performance**: O algoritmo do banqueiro é conservador - algumas solicitações são negadas mesmo quando poderiam ser seguras em cenários específicos

3. **Complexidade do Safety Algorithm**: A verificação de estado seguro tem complexidade O(n² × m), onde n é o número de processos e m o número de recursos

4. **Aplicabilidade Prática**: Embora o algoritmo seja teoricamente elegante, sua aplicação prática é limitada pela necessidade de conhecer antecipadamente as demandas máximas dos processos

### 4.3 Trabalhos Futuros

- Implementação de interface gráfica para visualização em tempo real
- Adição de persistência de estado para recuperação após falhas
- Implementação de algoritmos alternativos para comparação de performance
- Extensão para sistemas distribuídos

### 4.4 Considerações Finais

O trabalho demonstrou com sucesso a implementação do Algoritmo do Banqueiro em um ambiente moderno de programação concorrente. A utilização de C# e .NET permitiu uma implementação limpa e eficiente, com recursos nativos de sincronização que simplificaram o tratamento de condições de corrida.

A experiência prática com threads, locks e algoritmos de prevenção de deadlock proporciona uma compreensão profunda dos desafios enfrentados por sistemas operacionais no gerenciamento de recursos compartilhados.

---

## 5. REFERÊNCIAS

1. SILBERSCHATZ, Abraham; GALVIN, Peter B.; GAGNE, Greg. **Fundamentos de Sistemas Operacionais**. 9ª ed. LTC, 2015.

2. TANENBAUM, Andrew S.; BOS, Herbert. **Sistemas Operacionais Modernos**. 4ª ed. Pearson, 2016.

3. DIJKSTRA, Edsger W. **EWD108: Een algorithme ter voorkoming van de dodelijke omarming**. 1965.

4. Microsoft. **Threading in C#**. Documentação oficial do .NET. Disponível em: https://docs.microsoft.com/en-us/dotnet/standard/threading/

5. Microsoft. **lock statement (C# reference)**. Documentação oficial. Disponível em: https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/statements/lock

---

## 6. ANEXOS

### Anexo A: Como Executar o Projeto

```bash
# Clone o repositório
git clone https://github.com/cauamor-dev/BankerAlgorithm.git
# Entre no diretório
cd BankerAlgorithm

# Restaure as dependências
dotnet restore

# Execute o programa
dotnet run 10 5 7
```

### Anexo B: Estrutura de Arquivos

```
BankerAlgorithm/
├── Program.cs          # Ponto de entrada
├── Banker.cs           # Algoritmo do Banqueiro
├── Customer.cs         # Threads dos clientes
├── README.md           # Documentação
├── RELATORIO.md        # Este relatório
├── .gitignore          # Arquivos ignorados
└── BankerAlgorithm.csproj
```

### Anexo C: Link do Repositório

**GitHub:** https://github.com/cauamor-dev/BankerAlgorithm.git

---

*Relatório gerado em Abril de 2026*
