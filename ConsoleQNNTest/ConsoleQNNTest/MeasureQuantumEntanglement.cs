// ConsoleQNNTest embodies the public class implementation of our console application
//   for measuring pairwise entanglement created via unitary operators using parameters
//   "learned" during offline simulation of a two qubit system trained to "identify" 
//   specified input parameters by returning a target entanglement value for each of four
//   inputs.
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

using Quantum.QNeuralNetwork;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Globalization;

namespace ConsoleQNNTest
{
    public class MeasureQuantumEntanglement
    {
        private const string BaseFileName = "ConsoleQNNTest";
        private const string FileNameExt = ".csv";
        private const string FMT = "  0.0000000; -0.0000000";
        private const string FMT_SHORT = "+0.00;-0.00";
        private const int DEFAULTCOUNTSTEP = 50;

        private readonly int Count;
        private readonly int CountStep;
        private readonly int TimeChunks;
        private readonly int Epochs;
        private readonly bool Verbose;
        private readonly bool Signed;

        private List<string> EntanglementLabel = new List<string>();
        private List<Double> EntanglementTarget = new List<Double>();
        private List<Complex[]> States = new List<Complex[]>();
        private StreamWriter Stream;
        private string FileName;

        public  double Tf { get; set; }
        public string EntanglementType { get; set; }

        public MeasureQuantumEntanglement(int timeChunks, int count, int epochs, bool verbose, bool signed)
        {
            TimeChunks = timeChunks;
            Tf         = 1.580 / (8 * Math.PI);
            Count      = count;
            Epochs     = epochs;
            Verbose    = verbose;
            Signed     = signed;
            CountStep  = DEFAULTCOUNTSTEP;

            SetFileName("");

            // Default pre-set enumerated states  1=Bell, 2=Flat, 3=C, 4=Preset Partially Entangled State
            Complex[][] defaultStates = new Complex[4][];
            Double[] defaultTargets = new Double[] { -1.0, 0.0, 0.0, 0.663325 };
            string[] defaultLabels = new string[] { "   Bell   ", "   Flat   ", "    C     ", "    P     " };

            for (int i = 0; i < 4; i++) { defaultStates[i] = new Complex[] { i+1 };  }

            AddState(defaultStates);
            AddTarget(defaultTargets);
            AddStateLabel(defaultLabels);

            EntanglementType = "       State";
        }

        public string SetFileName(string filename)
        {
            if (String.IsNullOrEmpty(FileName))
            {
                string currentDate = Convert.ToString(DateTime.Now).Replace("/", "").Replace(":", "_").Replace(" ", "_");
                FileName = BaseFileName + currentDate + FileNameExt;
            }

            return FileName;
        }

        public string GetFileName()
        {
            return FileName;
        }

        private Complex[] ParseStringForComplexArguments(string s)
        {
            string[] sArray = s.Split(',');
            Complex[] args = new Complex[sArray.Length];
            int k = 0;

            foreach (string sArg in sArray)
            {
                Double real, imag, magnitude;
                Complex z;

                if (sArg.IndexOf('i') == -1)
                {
                    z = new Complex(Convert.ToDouble(sArg), 0);
                }
                else
                {
                    // stack overflow - steiner
                    // The pattern has been broken down for educational purposes
                    string regexPattern =
                        // Match any float, negative or positive, group it
                        @"([-+]?\d+\.?\d*|[-+]?\d*\.?\d+)" +
                        // ... possibly following that with whitespace
                        // @"\s*" +
                        // ... followed by a plus
                        // @"\+" +
                        // and possibly more whitespace:
                        // @"\s*" +
                        // Match any other float, and save it
                        @"([-+]?\d+\.?\d*|[-+]?\d*\.?\d+)" +
                        // ... followed by 'i'
                        @"i";

                    Regex regex = new Regex(regexPattern);
                    Match match = regex.Match(sArg);

                    real = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
                    imag = double.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture);

                    z = new Complex(real, imag);
                    magnitude = Complex.Abs(z);

                    if (magnitude > 1) { z = z / magnitude; }
                }

                args[k] = z;
                Console.WriteLine("Gamma : {0} = {1}", sArg, z);
                k++;
            }

            return args;
        }

        public void AddPStates(string gammaList)
        {
            // Need to use regular expressions to parse for embedded i=sqrt(-1) in string for complex values

            Complex[] gammaArray = ParseStringForComplexArguments(gammaList);

            foreach (Complex gamma in gammaArray )
            {
                Complex Czero = new Complex(0, 0);
                Complex Cone = new Complex(1, 0);

                Double length = Complex.Abs(gamma);
                Double magnitude = 2 + length * length; 
                Double concurrence = 2 * length / magnitude;

                magnitude = Math.Sqrt(magnitude);
                Complex[] state = new Complex[] { Cone / magnitude, Cone / magnitude, gamma / magnitude, Czero };

                AddState(new Complex[1][] { state });
                AddTarget(new double[] { concurrence });

                
                AddStateLabel(new string[] { gamma.Real.ToString(FMT_SHORT) + gamma.Imaginary.ToString(FMT_SHORT) + "i" });
            }

            EntanglementType = "       Gamma";
        }

        public void AddState(Complex[][] stateArray)
        {
            foreach (Complex[] state in stateArray)
            {
                States.Add(state);
            }
        }

        public void AddTarget(Double[] targetArray)
        {
            foreach(Double target in targetArray)
            {
                EntanglementTarget.Add(target);
            }
        }

        public void AddStateLabel(string[] labelArray)
        {
            foreach  (string label in labelArray)
            {
                EntanglementLabel.Add(label);
            }
        }

        public void ClearStatesList()
        {
            States.Clear();
            EntanglementLabel.Clear();
            EntanglementTarget.Clear();
        }

        private void InitializeStream()
        {
            try
            {
                if (File.Exists(FileName))
                {
                    Stream = File.AppendText(FileName);
                }
                else
                {
                    Stream = new StreamWriter(FileName);

                    if (new FileInfo(FileName).Length == 0)
                    {
                        string delimiter = @",";

                        Stream.WriteLine("Chunked QNN Test");
                        Stream.WriteLine("Time Chunks,T_f,Count,Epochs");
                        Stream.WriteLine(string.Join(delimiter, TimeChunks, Tf, Count, Epochs));
                        Stream.WriteLine("," + string.Join(delimiter, EntanglementLabel.ToArray()));
                        Stream.WriteLine("Targets," + string.Join(delimiter, EntanglementTarget.ToArray()));
                        Console.Out.Flush();
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("RunQNNTest Exception Opening File: " + e.Message);
                throw (e);
            }
        }

        public void RunQNNTest()
        {
            if (Verbose)
            {
                Console.WriteLine();
                Console.WriteLine($"File Name: {FileName}");
                Console.WriteLine();
                Console.WriteLine(EntanglementType + ":   " + String.Join("  ", EntanglementLabel.ToArray()));

                StringBuilder sb = new StringBuilder("      Target:");
                foreach ( Double target in EntanglementTarget ) { sb.Append("  " + target.ToString(FMT)); }
                Console.WriteLine(sb.ToString());
            }

            InitializeStream();

            CoupledTwoQubitQNN network = new CoupledTwoQubitQNN(TimeChunks, Tf);

            for (int epoch = 1; epoch <= Epochs; epoch++)
            {
                StringBuilder fileString = new StringBuilder(epoch.ToString());
                StringBuilder consoleString = new StringBuilder("     " + epoch.ToString("0000000") + ":");

                foreach (Complex[] state in States)
                {
                    double entanglement = network.MeasureEntanglementWitness(state, Count);

                    if (!Signed)
                    {
                        entanglement = Math.Abs(entanglement);
                    }

                    fileString.Append(",").Append(entanglement);
                    consoleString.Append("  " + entanglement.ToString(FMT));
                }

                if (Verbose) {
                    Console.WriteLine(consoleString.ToString()); }

                Stream.WriteLine(fileString.ToString());
                Console.Out.Flush();
            }

            Stream.Close();
        }

        public void RunWitnessTest()
        {
            if (Verbose)
            {
                Console.WriteLine();
                Console.WriteLine($"File Name: {FileName}");
                Console.WriteLine();
                Console.WriteLine(EntanglementType + ":  " + String.Join("  ", EntanglementLabel.ToArray()));

                StringBuilder sb = new StringBuilder("      Target:");
                foreach (Double target in EntanglementTarget) { sb.Append(target.ToString(FMT)); }
                Console.WriteLine(sb.ToString());
            }

            InitializeStream();

            CoupledTwoQubitQNN network = new CoupledTwoQubitQNN(TimeChunks, Tf);

            const int V = 14200;
            for (int currentCount = V; currentCount <= Count; currentCount += CountStep)
            {
                for (int epoch = 1; epoch <= Epochs; epoch++)
                {
                    StringBuilder fileString = new StringBuilder(currentCount.ToString());
                    StringBuilder consoleString = new StringBuilder("     " + currentCount.ToString("0000000") + ":");

                    foreach (Complex[] state in States)
                    {
                        double entanglement = network.MeasureEntanglementWitness(state, currentCount);

                        if (!Signed)
                        {
                            entanglement = Math.Abs(entanglement);
                        }

                        if (Verbose)
                        {
                            consoleString.Append("  " + entanglement.ToString(FMT));
                        }

                        fileString.Append(",").Append(entanglement);
                    }

                    if (Verbose)
                    {
                        Console.WriteLine(consoleString.ToString());
                        Console.Out.Flush();
                    }


                    Stream.WriteLine(fileString.ToString());
                }
            }

            Stream.Close();
        }
    }
}