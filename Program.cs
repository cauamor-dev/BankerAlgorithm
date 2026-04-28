using System;
using System.Threading;

namespace BankerAlgorithm
{
    /// <summary>
    /// Programa principal que simula o Algoritmo do Banqueiro.
    /// 
    /// O Algoritmo do Banqueiro é um algoritmo de alocação de recursos e
    /// prevenção de deadlock desenvolvido por Edsger Dijkstra.
    /// 
    /// Uso: dotnet run [recurso1] [recurso2] [recurso3] ...
    /// Exemplo: dotnet run 10 5 7
    /// 
    /// Isso cria um sistema com 3 tipos de recursos:
    /// - Recurso A: 10 unidades
    /// - Recurso B: 5 unidades
    /// - Recurso C: 7 unidades
    /// </summary>
    class Program
    {
        // Tempo de execução da simulação em segundos
        private const int SIMULATION_TIME_SECONDS = 30;
        
        // Intervalo para impressão do estado do sistema
        private const int STATE_PRINT_INTERVAL_SECONDS = 10;

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            
            // Exibe banner inicial
            PrintBanner();

            // Valida os argumentos
            if (args.Length == 0)
            {
                PrintUsage();
                return;
            }

            // Converte os argumentos para array de inteiros (recursos)
            int[] resources;
            try
            {
                resources = ParseArguments(args);
            }
            catch (FormatException)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("ERRO: Todos os argumentos devem ser números inteiros positivos.");
                Console.ResetColor();
                PrintUsage();
                return;
            }

            // Valida os recursos
            foreach (int resource in resources)
            {
                if (resource <= 0)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("ERRO: Todos os recursos devem ser maiores que zero.");
                    Console.ResetColor();
                    return;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Recursos inicializados: [{string.Join(", ", resources)}]");
            Console.WriteLine($"✓ Número de clientes: {Banker.NUMBER_OF_CUSTOMERS}");
            Console.WriteLine($"✓ Tempo de simulação: {SIMULATION_TIME_SECONDS} segundos");
            Console.ResetColor();
            Console.WriteLine();

            // Cria o banqueiro
            Banker banker = new Banker(resources);

            // Cria os clientes (threads)
            Customer[] customers = new Customer[Banker.NUMBER_OF_CUSTOMERS];
            for (int i = 0; i < Banker.NUMBER_OF_CUSTOMERS; i++)
            {
                customers[i] = new Customer(i, banker);
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("\n🚀 Iniciando simulação do Algoritmo do Banqueiro...");
            Console.WriteLine("   Pressione Ctrl+C para encerrar a qualquer momento.\n");
            Console.ResetColor();

            // Configura handler para Ctrl+C
            bool isRunning = true;
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                isRunning = false;
                Console.WriteLine("\n[SISTEMA] Encerrando simulação...");
            };

            // Inicia todas as threads
            foreach (var customer in customers)
            {
                customer.Start();
            }

            // Thread para imprimir o estado periodicamente
            Thread stateThread = new Thread(() =>
            {
                int elapsed = 0;
                while (isRunning && elapsed < SIMULATION_TIME_SECONDS)
                {
                    Thread.Sleep(STATE_PRINT_INTERVAL_SECONDS * 1000);
                    elapsed += STATE_PRINT_INTERVAL_SECONDS;
                    
                    if (isRunning && elapsed < SIMULATION_TIME_SECONDS)
                    {
                        banker.PrintState();
                    }
                }
            });
            stateThread.IsBackground = true;
            stateThread.Start();

            // Aguarda o tempo de simulação
            int timeRemaining = SIMULATION_TIME_SECONDS;
            while (timeRemaining > 0 && isRunning)
            {
                Thread.Sleep(1000);
                timeRemaining--;
            }

            // Para todos os clientes
            Console.WriteLine("\n[SISTEMA] Parando todas as threads...");
            foreach (var customer in customers)
            {
                customer.Stop();
            }

            // Aguarda todas as threads terminarem
            foreach (var customer in customers)
            {
                customer.Join();
            }

            // Imprime estado final
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("\n════════════════════════════════════════════════════════════════");
            Console.WriteLine("                     ESTADO FINAL DO SISTEMA                    ");
            Console.WriteLine("════════════════════════════════════════════════════════════════");
            Console.ResetColor();
            
            banker.PrintState();

            // Mensagem de conclusão
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║           SIMULAÇÃO CONCLUÍDA COM SUCESSO!                     ║");
            Console.WriteLine("║                                                                ║");
            Console.WriteLine("║   O Algoritmo do Banqueiro garantiu que não ocorreram          ║");
            Console.WriteLine("║   deadlocks durante toda a execução da simulação.              ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
            Console.ResetColor();
        }

        /// <summary>
        /// Converte os argumentos da linha de comando para array de inteiros.
        /// </summary>
        private static int[] ParseArguments(string[] args)
        {
            int[] resources = new int[args.Length];
            for (int i = 0; i < args.Length; i++)
            {
                resources[i] = int.Parse(args[i]);
            }
            return resources;
        }

        /// <summary>
        /// Exibe o banner inicial do programa.
        /// </summary>
        private static void PrintBanner()
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(@"
╔═══════════════════════════════════════════════════════════════════════════╗
║                                                                           ║
║     ██████╗  █████╗ ███╗   ██╗██╗  ██╗███████╗██████╗                     ║
║     ██╔══██╗██╔══██╗████╗  ██║██║ ██╔╝██╔════╝██╔══██╗                    ║
║     ██████╔╝███████║██╔██╗ ██║█████╔╝ █████╗  ██████╔╝                    ║
║     ██╔══██╗██╔══██║██║╚██╗██║██╔═██╗ ██╔══╝  ██╔══██╗                    ║
║     ██████╔╝██║  ██║██║ ╚████║██║  ██╗███████╗██║  ██║                    ║
║     ╚═════╝ ╚═╝  ╚═╝╚═╝  ╚═══╝╚═╝  ╚═╝╚══════╝╚═╝  ╚═╝                    ║
║                                                                           ║
║              A L G O R I T H M   -   B A N K E R ' S                      ║
║                                                                           ║
║     Trabalho Prático - Sistemas Operacionais                              ║
║     Implementação do Algoritmo do Banqueiro em C#                         ║
║                                                                           ║
╚═══════════════════════════════════════════════════════════════════════════╝
");
            Console.ResetColor();
        }

        /// <summary>
        /// Exibe instruções de uso do programa.
        /// </summary>
        private static void PrintUsage()
        {
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("USO:");
            Console.ResetColor();
            Console.WriteLine("  dotnet run [recurso1] [recurso2] [recurso3] ...");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("EXEMPLO:");
            Console.ResetColor();
            Console.WriteLine("  dotnet run 10 5 7");
            Console.WriteLine();
            Console.WriteLine("  Isso cria um sistema com 3 tipos de recursos:");
            Console.WriteLine("    - Recurso 0: 10 unidades");
            Console.WriteLine("    - Recurso 1: 5 unidades");
            Console.WriteLine("    - Recurso 2: 7 unidades");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("DESCRIÇÃO:");
            Console.ResetColor();
            Console.WriteLine("  O Algoritmo do Banqueiro é um algoritmo de alocação de recursos");
            Console.WriteLine("  que garante a prevenção de deadlock. O sistema simula múltiplos");
            Console.WriteLine("  clientes (threads) que solicitam e liberam recursos continuamente,");
            Console.WriteLine("  com o banqueiro garantindo que o sistema permaneça sempre em um");
            Console.WriteLine("  estado seguro.");
            Console.WriteLine();
        }
    }
}
