using System;
using System.Threading;

namespace BankerAlgorithm
{
    /// <summary>
    /// Classe que representa um cliente (thread) no sistema.
    /// Cada cliente solicita e libera recursos continuamente,
    /// simulando o comportamento de processos em um sistema operacional.
    /// </summary>
    public class Customer
    {
        // ID único do cliente
        public int CustomerId { get; private set; }
        
        // Referência ao banqueiro que gerencia os recursos
        private readonly Banker _banker;
        
        // Thread do cliente
        private Thread? _thread;
        
        // Flag para controlar a execução da thread
        private volatile bool _running;
        
        // Gerador de números aleatórios
        private readonly Random _random;
        
        // Tempos de espera em milissegundos
        private const int MIN_SLEEP_TIME = 500;
        private const int MAX_SLEEP_TIME = 2000;

        /// <summary>
        /// Construtor do cliente.
        /// </summary>
        /// <param name="customerId">ID único do cliente</param>
        /// <param name="banker">Referência ao banqueiro</param>
        public Customer(int customerId, Banker banker)
        {
            CustomerId = customerId;
            _banker = banker;
            _running = false;
            _random = new Random(customerId * DateTime.Now.Millisecond);
        }

        /// <summary>
        /// Inicia a execução da thread do cliente.
        /// </summary>
        public void Start()
        {
            _running = true;
            _thread = new Thread(Run)
            {
                Name = $"Customer-{CustomerId}",
                IsBackground = true
            };
            _thread.Start();
        }

        /// <summary>
        /// Para a execução da thread do cliente.
        /// </summary>
        public void Stop()
        {
            _running = false;
        }

        /// <summary>
        /// Aguarda a thread do cliente terminar.
        /// </summary>
        public void Join()
        {
            _thread?.Join(TimeSpan.FromSeconds(5));
        }

        /// <summary>
        /// Método principal executado pela thread.
        /// Implementa o ciclo de vida do cliente:
        /// solicitar recursos, usar, e liberar.
        /// </summary>
        private void Run()
        {
            Console.WriteLine($"[CLIENTE {CustomerId}] Thread iniciada.");

            while (_running)
            {
                try
                {
                    // 1. Espera tempo aleatório antes de solicitar
                    int sleepTime = _random.Next(MIN_SLEEP_TIME, MAX_SLEEP_TIME);
                    Thread.Sleep(sleepTime);

                    if (!_running) break;

                    // 2. Gera um pedido aleatório baseado no need atual
                    int[] request = GenerateRandomRequest();

                    // Verifica se há algo para solicitar
                    if (!IsZeroRequest(request))
                    {
                        // 3. Solicita os recursos ao banqueiro
                        int result = _banker.RequestResources(CustomerId, request);

                        // 4. Mostra resultado
                        if (result == 0)
                        {
                            // Sucesso! Usa os recursos por um tempo
                            Thread.Sleep(_random.Next(MIN_SLEEP_TIME, MAX_SLEEP_TIME));

                            if (!_running) break;

                            // 5. Libera parte ou todos os recursos alocados
                            int[] release = GenerateRandomRelease();
                            
                            if (!IsZeroRequest(release))
                            {
                                _banker.ReleaseResources(CustomerId, release);
                            }
                        }
                    }
                }
                catch (ThreadInterruptedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CLIENTE {CustomerId}] Erro: {ex.Message}");
                }
            }

            Console.WriteLine($"[CLIENTE {CustomerId}] Thread finalizada.");
        }

        /// <summary>
        /// Gera um pedido aleatório baseado no need atual do cliente.
        /// Cada recurso solicitado é entre 0 e need[recurso].
        /// </summary>
        /// <returns>Array com os recursos solicitados</returns>
        private int[] GenerateRandomRequest()
        {
            int[] need = _banker.GetNeed(CustomerId);
            int[] request = new int[_banker.NumberOfResources];

            for (int i = 0; i < _banker.NumberOfResources; i++)
            {
                if (need[i] > 0)
                {
                    // Solicita entre 0 e need[i] recursos
                    request[i] = _random.Next(0, need[i] + 1);
                }
                else
                {
                    request[i] = 0;
                }
            }

            // Garante que pelo menos algum recurso seja solicitado (se houver necessidade)
            bool hasNeed = false;
            for (int i = 0; i < _banker.NumberOfResources; i++)
            {
                if (need[i] > 0)
                {
                    hasNeed = true;
                    break;
                }
            }

            if (hasNeed && IsZeroRequest(request))
            {
                // Se o pedido gerado é zero mas há necessidade, força pelo menos 1 recurso
                for (int i = 0; i < _banker.NumberOfResources; i++)
                {
                    if (need[i] > 0)
                    {
                        request[i] = 1;
                        break;
                    }
                }
            }

            return request;
        }

        /// <summary>
        /// Gera uma liberação aleatória baseada nos recursos alocados.
        /// </summary>
        /// <returns>Array com os recursos a serem liberados</returns>
        private int[] GenerateRandomRelease()
        {
            int[] allocation = _banker.GetAllocation(CustomerId);
            int[] release = new int[_banker.NumberOfResources];

            for (int i = 0; i < _banker.NumberOfResources; i++)
            {
                if (allocation[i] > 0)
                {
                    // Libera entre 0 e allocation[i] recursos
                    // Com tendência a liberar pelo menos algo
                    release[i] = _random.Next(0, allocation[i] + 1);
                }
                else
                {
                    release[i] = 0;
                }
            }

            return release;
        }

        /// <summary>
        /// Verifica se um array de recursos é todo zero.
        /// </summary>
        /// <param name="resources">Array de recursos</param>
        /// <returns>true se todos os valores são zero</returns>
        private bool IsZeroRequest(int[] resources)
        {
            for (int i = 0; i < resources.Length; i++)
            {
                if (resources[i] > 0)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
