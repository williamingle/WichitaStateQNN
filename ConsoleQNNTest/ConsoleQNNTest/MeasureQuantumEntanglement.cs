using Quantum.QNeuralNetwork;
using System;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace ConsoleQNNTest
{
    public class MeasureQuantumEntanglement
    {
        private const string BaseFileName = "ConsoleQNNTest";
        private const string FileNameExt = ".csv";
        private const string FMT = " 0.0000000;-0.0000000";
        private const int DEFAULTCOUNTSTEP = 50;

        private readonly int Count;
        private readonly int CountStep;
        private readonly int TimeChunks;
        private readonly int Epochs;
        private readonly bool Verbose;
        private readonly bool Signed;

        private List<string> EntanglementLabel = new List<string>();
        private List<double> EntanglementTarget = new List<double>();
        private List<double[]> States = new List<double[]>();
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
            double[][] defaultStates = new double[4][];
            double[] defaultTargets = new double[] { -1.0, 0.0, 0.0, 0.663325 };
            string[] defaultLabels = new string[] { "   Bell   ", "   Flat   ", "    C     ", "    P     " };

            for (int i = 0; i < 4; i++) { defaultStates[i] = new double[] { i+1 };  }

            AddState(defaultStates, defaultLabels, defaultTargets);

            EntanglementType = "Entanglement";
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

        public List<double[]> AddPStates(string gammaList)
        {
            double[] targets = gammaList.Split(',').Select(Double.Parse).ToArray();
            double[][] states = new double[targets.Length][];
            string[] labels = new string[targets.Length];

            for (int i=0; i<targets.Length; i++)
            {
                double gamma = targets[i];
                double magnitude = Math.Sqrt(2 + gamma * gamma);

                states[i] = new double[] { 1/magnitude, 0, 1/magnitude, 0, gamma/magnitude, 0, 0, 0 };
                labels[i] = targets[i].ToString(FMT);
            }

            EntanglementType = "       Gamma";

            return AddState(states, labels, targets);
        }

        public List<double[]> AddState(double[][] stateArray, string[] labelArray, double[] targetArray)
        {
            for (int i= 0; i < labelArray.Length; i++ )
            {
                States.Add(stateArray[i]);
                EntanglementLabel.Add(labelArray[i]);
                EntanglementTarget.Add(targetArray[i]);
            }

            return States;

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
                        Stream.WriteLine("Targets," + string.Join(delimiter, EntanglementTarget));
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
                Console.WriteLine(EntanglementType + ":  " + String.Join("  ", EntanglementLabel.ToArray()));

                StringBuilder sb = new StringBuilder("      Target:");

                foreach ( double target in EntanglementTarget ) { sb.Append("  " + target.ToString(FMT)); }
                Console.WriteLine(sb.ToString());
            }

            InitializeStream();

            CoupledTwoQubitQNN network = new CoupledTwoQubitQNN(TimeChunks, Tf);

            for (int epoch = 1; epoch <= Epochs; epoch++)
            {
                StringBuilder fileString = new StringBuilder(epoch.ToString());
                StringBuilder consoleString = new StringBuilder("     " + epoch.ToString("0000000") + ":");

                foreach (double[] state in States)
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

                foreach (double target in EntanglementTarget) { sb.Append("  " + target.ToString(FMT)); }
                Console.WriteLine(sb.ToString());
            }

            InitializeStream();

            CoupledTwoQubitQNN network = new CoupledTwoQubitQNN(TimeChunks, Tf);

            for (int currentCount = CountStep; currentCount <= Count; currentCount += CountStep)
            {
                StringBuilder fileString = new StringBuilder(currentCount.ToString());
                StringBuilder consoleString = new StringBuilder("     " + currentCount.ToString("0000000") + ":");

                foreach (double[] state in States)
                {
                    double entanglement = 0;

                    for (int epoch = 1; epoch <= Epochs; epoch++ )
                    {
                        double witness = network.MeasureEntanglementWitness(state, currentCount);

                        if (!Signed)
                        {
                            witness = Math.Abs(entanglement);
                        }

                        entanglement += witness;
                    }

                    entanglement /= Epochs;

                    fileString.Append(",").Append(entanglement);
                    consoleString.Append("  " + entanglement.ToString(FMT));
                }

                if (Verbose) { 
                    Console.WriteLine(consoleString.ToString());   }

                Stream.WriteLine(fileString.ToString());
                Console.Out.Flush();
            }

            Stream.Close();
        }
    }
}