using System;
using System.Text;

namespace BankerAlgorithm
{
    /// <summary>
    /// Classe principal que implementa o Algoritmo do Banqueiro.
    /// Gerencia a alocação de recursos para múltiplos clientes de forma segura,
    /// prevenindo deadlocks através da verificação de estados seguros.
    /// </summary>
    public class Banker
    {
        // Número fixo de clientes (threads)
        public const int NUMBER_OF_CUSTOMERS = 5;
        
        // Número de tipos de recursos (definido pelos argumentos da linha de comando)
        public int NumberOfResources { get; private set; }
        
        // Recursos disponíveis no sistema
        private int[] _available;
        
        // Demanda máxima de cada cliente
        private int[,] _maximum;
        
        // Recursos atualmente alocados a cada cliente
        private int[,] _allocation;
        
        // Recursos ainda necessários por cada cliente (need = maximum - allocation)
        private int[,] _need;
        
        // Objeto de sincronização para controle de concorrência
        private readonly object _lockObject = new object();
        
        // Gerador de números aleatórios
        private readonly Random _random = new Random();

        /// <summary>
        /// Construtor do Banqueiro.
        /// Inicializa todas as matrizes e gera demandas máximas aleatórias para os clientes.
        /// </summary>
        /// <param name="resources">Array com a quantidade de cada tipo de recurso disponível</param>
        public Banker(int[] resources)
        {
            NumberOfResources = resources.Length;
            
            // Inicializa o array de recursos disponíveis
            _available = new int[NumberOfResources];
            Array.Copy(resources, _available, NumberOfResources);
            
            // Inicializa as matrizes
            _maximum = new int[NUMBER_OF_CUSTOMERS, NumberOfResources];
            _allocation = new int[NUMBER_OF_CUSTOMERS, NumberOfResources];
            _need = new int[NUMBER_OF_CUSTOMERS, NumberOfResources];
            
            // Gera demandas máximas aleatórias válidas para cada cliente
            GenerateRandomMaximum();
            
            // Calcula a matriz need inicial (como allocation é zero, need = maximum)
            CalculateNeed();
            
            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║       ALGORITMO DO BANQUEIRO - SISTEMAS OPERACIONAIS           ║");
            Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
            Console.WriteLine("║  Simulação de gerenciamento de recursos com prevenção de       ║");
            Console.WriteLine("║  deadlock utilizando programação concorrente em C#             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.WriteLine();
            
            PrintState();
        }

        /// <summary>
        /// Gera valores aleatórios válidos para a matriz maximum.
        /// Cada valor é entre 1 e o valor máximo disponível daquele recurso.
        /// </summary>
        private void GenerateRandomMaximum()
        {
            for (int i = 0; i < NUMBER_OF_CUSTOMERS; i++)
            {
                for (int j = 0; j < NumberOfResources; j++)
                {
                    // Gera demanda máxima entre 1 e o valor disponível (inclusive)
                    _maximum[i, j] = _random.Next(1, _available[j] + 1);
                }
            }
        }

        /// <summary>
        /// Calcula a matriz need para todos os clientes.
        /// need[i,j] = maximum[i,j] - allocation[i,j]
        /// </summary>
        private void CalculateNeed()
        {
            for (int i = 0; i < NUMBER_OF_CUSTOMERS; i++)
            {
                for (int j = 0; j < NumberOfResources; j++)
                {
                    _need[i, j] = _maximum[i, j] - _allocation[i, j];
                }
            }
        }

        /// <summary>
        /// Solicita recursos para um cliente específico.
        /// Verifica se o pedido é válido e se resultará em um estado seguro.
        /// </summary>
        /// <param name="customerId">ID do cliente solicitante</param>
        /// <param name="request">Array com a quantidade de cada recurso solicitado</param>
        /// <returns>0 se sucesso, -1 se negado</returns>
        public int RequestResources(int customerId, int[] request)
        {
            lock (_lockObject)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine($"\n[CLIENTE {customerId}] Solicitando: [{string.Join(",", request)}]");
                Console.ResetColor();

                // Verifica se o pedido excede o need do cliente
                for (int i = 0; i < NumberOfResources; i++)
                {
                    if (request[i] > _need[customerId, i])
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[NEGADO] Pedido excede a necessidade máxima declarada.");
                        Console.ResetColor();
                        return -1;
                    }
                }

                // Verifica se há recursos disponíveis suficientes
                for (int i = 0; i < NumberOfResources; i++)
                {
                    if (request[i] > _available[i])
                    {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.WriteLine($"[AGUARDANDO] Recursos insuficientes no momento.");
                        Console.ResetColor();
                        return -1;
                    }
                }

                // Simula a alocação temporária
                for (int i = 0; i < NumberOfResources; i++)
                {
                    _available[i] -= request[i];
                    _allocation[customerId, i] += request[i];
                    _need[customerId, i] -= request[i];
                }

                // Verifica se o estado resultante é seguro
                int[] safeSequence;
                if (IsSafeState(out safeSequence))
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"[APROVADO] Estado seguro encontrado: <{string.Join(",", safeSequence)}>");
                    Console.ResetColor();
                    return 0;
                }
                else
                {
                    // Reverte a alocação temporária
                    for (int i = 0; i < NumberOfResources; i++)
                    {
                        _available[i] += request[i];
                        _allocation[customerId, i] -= request[i];
                        _need[customerId, i] += request[i];
                    }

                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"[NEGADO] Estado inseguro - possível deadlock detectado!");
                    Console.ResetColor();
                    return -1;
                }
            }
        }

        /// <summary>
        /// Libera recursos alocados de um cliente.
        /// </summary>
        /// <param name="customerId">ID do cliente</param>
        /// <param name="release">Array com a quantidade de cada recurso a liberar</param>
        /// <returns>0 se sucesso, -1 se erro</returns>
        public int ReleaseResources(int customerId, int[] release)
        {
            lock (_lockObject)
            {
                // Verifica se o cliente está tentando liberar mais do que possui
                for (int i = 0; i < NumberOfResources; i++)
                {
                    if (release[i] > _allocation[customerId, i])
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"[ERRO] Cliente {customerId} tentou liberar mais recursos do que possui.");
                        Console.ResetColor();
                        return -1;
                    }
                }

                // Libera os recursos
                for (int i = 0; i < NumberOfResources; i++)
                {
                    _available[i] += release[i];
                    _allocation[customerId, i] -= release[i];
                    _need[customerId, i] += release[i];
                }

                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"[CLIENTE {customerId}] Liberou: [{string.Join(",", release)}]");
                Console.ResetColor();

                return 0;
            }
        }

        /// <summary>
        /// Implementa o Algoritmo de Segurança (Safety Algorithm).
        /// Verifica se existe uma sequência segura de execução dos processos.
        /// </summary>
        /// <param name="safeSequence">Array de saída com a sequência segura encontrada</param>
        /// <returns>true se existe sequência segura, false caso contrário</returns>
        private bool IsSafeState(out int[] safeSequence)
        {
            // Cria cópias temporárias
            int[] work = new int[NumberOfResources];
            Array.Copy(_available, work, NumberOfResources);

            bool[] finish = new bool[NUMBER_OF_CUSTOMERS];
            safeSequence = new int[NUMBER_OF_CUSTOMERS];
            int sequenceIndex = 0;

            // Inicializa finish como false para todos
            for (int i = 0; i < NUMBER_OF_CUSTOMERS; i++)
            {
                finish[i] = false;
            }

            // Encontra uma sequência segura
            int count = 0;
            while (count < NUMBER_OF_CUSTOMERS)
            {
                bool found = false;

                for (int i = 0; i < NUMBER_OF_CUSTOMERS; i++)
                {
                    if (!finish[i])
                    {
                        // Verifica se need[i] <= work
                        bool canFinish = true;
                        for (int j = 0; j < NumberOfResources; j++)
                        {
                            if (_need[i, j] > work[j])
                            {
                                canFinish = false;
                                break;
                            }
                        }

                        if (canFinish)
                        {
                            // Processo i pode terminar
                            for (int j = 0; j < NumberOfResources; j++)
                            {
                                work[j] += _allocation[i, j];
                            }
                            finish[i] = true;
                            safeSequence[sequenceIndex++] = i;
                            found = true;
                            count++;
                        }
                    }
                }

                // Se não encontrou nenhum processo que pode terminar, estado inseguro
                if (!found)
                {
                    return false;
                }
            }

            // Todos os processos podem terminar
            return true;
        }

        /// <summary>
        /// Obtém o array de necessidades de um cliente específico.
        /// Usado pelos clientes para gerar pedidos válidos.
        /// </summary>
        /// <param name="customerId">ID do cliente</param>
        /// <returns>Array com as necessidades do cliente</returns>
        public int[] GetNeed(int customerId)
        {
            lock (_lockObject)
            {
                int[] need = new int[NumberOfResources];
                for (int i = 0; i < NumberOfResources; i++)
                {
                    need[i] = _need[customerId, i];
                }
                return need;
            }
        }

        /// <summary>
        /// Obtém o array de recursos alocados a um cliente específico.
        /// </summary>
        /// <param name="customerId">ID do cliente</param>
        /// <returns>Array com os recursos alocados</returns>
        public int[] GetAllocation(int customerId)
        {
            lock (_lockObject)
            {
                int[] allocation = new int[NumberOfResources];
                for (int i = 0; i < NumberOfResources; i++)
                {
                    allocation[i] = _allocation[customerId, i];
                }
                return allocation;
            }
        }

        /// <summary>
        /// Imprime o estado atual do sistema (matrizes e recursos disponíveis).
        /// </summary>
        public void PrintState()
        {
            lock (_lockObject)
            {
                Console.WriteLine("\n╔═══════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║                    ESTADO ATUAL DO SISTEMA                    ║");
                Console.WriteLine("╚═══════════════════════════════════════════════════════════════╝");

                // Cabeçalho dos recursos
                StringBuilder header = new StringBuilder("           ");
                for (int j = 0; j < NumberOfResources; j++)
                {
                    header.Append($"  R{j} ");
                }

                // Imprime Available
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"\n📦 AVAILABLE (Recursos Disponíveis):");
                Console.ResetColor();
                Console.Write("           ");
                for (int j = 0; j < NumberOfResources; j++)
                {
                    Console.Write($"  {_available[j],2} ");
                }
                Console.WriteLine();

                // Imprime Maximum
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.WriteLine($"\n📊 MAXIMUM (Demanda Máxima por Cliente):");
                Console.ResetColor();
                Console.WriteLine(header.ToString());
                for (int i = 0; i < NUMBER_OF_CUSTOMERS; i++)
                {
                    Console.Write($"Cliente {i}: ");
                    for (int j = 0; j < NumberOfResources; j++)
                    {
                        Console.Write($"  {_maximum[i, j],2} ");
                    }
                    Console.WriteLine();
                }

                // Imprime Allocation
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"\n📌 ALLOCATION (Recursos Alocados):");
                Console.ResetColor();
                Console.WriteLine(header.ToString());
                for (int i = 0; i < NUMBER_OF_CUSTOMERS; i++)
                {
                    Console.Write($"Cliente {i}: ");
                    for (int j = 0; j < NumberOfResources; j++)
                    {
                        Console.Write($"  {_allocation[i, j],2} ");
                    }
                    Console.WriteLine();
                }

                // Imprime Need
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"\n🎯 NEED (Recursos Necessários):");
                Console.ResetColor();
                Console.WriteLine(header.ToString());
                for (int i = 0; i < NUMBER_OF_CUSTOMERS; i++)
                {
                    Console.Write($"Cliente {i}: ");
                    for (int j = 0; j < NumberOfResources; j++)
                    {
                        Console.Write($"  {_need[i, j],2} ");
                    }
                    Console.WriteLine();
                }

                Console.WriteLine("\n═══════════════════════════════════════════════════════════════════");
            }
        }
    }
}
