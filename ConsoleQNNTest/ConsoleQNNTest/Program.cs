//   ConsoleQNNTest is a console application that drives the test ConsoleQNNTest
//
//   Coopyright © 2019 Wichita State University
//   The copyright holders grant the freedom to copy, modify, convey, adapt, and/or 
//   redistribute this work (except Appendices B–E) under the terms of the Creative 
//   Commons Attribution Share Alike 4.0 International License. A copy of that license 
//   is available at https://creativecommons.org/licenses/by-_sa/4.0/legalcode. 
//
//   Nathan Lee Thompson
//   William Ingle
//   Elizabeth Behrman
//   James Steck

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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

            [Option('f', "timef", Required = false, HelpText = "T_f for each time chunk.")]
            public double FinalTime{ get; set; }

            [Option('g', "gamma", Required = false, HelpText = "A comma-delimited list of complex values gamma in the unit circle")]
            public string GammaString { get; set; }

            [Option('o', "output", Required = false, HelpText = "Output file (csv).")]
            public string OutputFile { get; set; }

            [Option('s', "signed", Required = false, HelpText = "Emit signed values.")]
            public bool Signed { get; set; }

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
                    MeasureQuantumEntanglement quantumEntanglement = new MeasureQuantumEntanglement(o.Chunks, o.Count, o.Epochs, o.Verbose, o.Signed);

                    if ( !string.IsNullOrEmpty(o.OutputFile) )
                    {
                        quantumEntanglement.SetFileName(o.OutputFile);
                    }

                    if (o.FinalTime > 1e-12) { quantumEntanglement.Tf = o.FinalTime; }

                    if ( !string.IsNullOrEmpty(o.GammaString) )
                    {
                        quantumEntanglement.ClearStatesList();
                        quantumEntanglement.AddPStates(o.GammaString);
                    }

                    if (o.Witness)
                    {
                        quantumEntanglement.RunWitnessTest();
                    }
                    else
                    {
                        quantumEntanglement.RunQNNTest();
                    }
                });

            Console.WriteLine();
            Console.WriteLine("PRESS ANY KEY TO CONTINUE...");
            Console.ReadKey();
        }
    }
}
