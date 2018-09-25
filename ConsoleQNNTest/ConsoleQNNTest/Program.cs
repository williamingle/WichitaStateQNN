using System;
using CommandLine;
using Quantum.QNeuralNetwork;

namespace ConsoleQNNTest
{
    class Program
    {
        class Options
        {
            [Option('c', "count", Required = false, Default = 1000, HelpText = "Number of measurements for each witness.")]
            public int Count { get; set; }

            [Option('e', "epochs", Required = false, Default = 4, HelpText = "Number of Epochs to measure targets.")]
            public int Epochs { get; set; }

            [Option('f', "timef", Required = false, HelpText = "t_f for each time chunk.")]
            public double FinalTime{ get; set; }

            [Option('o', "output", Required = false, HelpText = "output file (csv).")]
            public string OutputFile { get; set; }

            [Option('t', "timechunks", Required = false, Default = 4, HelpText = "Number of time chunks per epoch.")]
            public int Chunks { get; set; }

            [Option('v', "verbose", Required = false, Default = true, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }

            [Option('w', "witness", Required = false, Default = false, HelpText = "Run Entanglement Witness Test.")]
            public bool Witness { get; set; }
        }

        static void Main(string[] args) 
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed<Options>(o =>
                {
                    if (o.FinalTime < 1e-12) { o.FinalTime = 1.580 / (8 * Math.PI); }

                    if (o.Witness)
                    {
                        MeasureQuantumEntanglement quantumEntanglement = new MeasureQuantumEntanglement(o.Chunks, o.FinalTime, o.Count, o.Epochs, o.Verbose, o.OutputFile);

                        quantumEntanglement.RunWitnessTest();
                    }
                    else
                    {
                        MeasureQuantumEntanglement quantumEntanglement = new MeasureQuantumEntanglement(o.Chunks, o.FinalTime, o.Count, o.Epochs, o.Verbose, o.OutputFile);

                        quantumEntanglement.RunQNNTest();
                    }
                });

            Console.WriteLine();
            Console.WriteLine("PRESS ANY KEY TO CONTINUE...");
            Console.ReadKey();
        }
    }
}
