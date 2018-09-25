using Quantum.QNeuralNetwork;
using System;
using System.IO;
using System.Text;

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
        private readonly double Tf;
        private readonly string FileName;
        private StreamWriter Stream;

        private readonly int[] States = { 1, 2, 3, 4 };
        private readonly double[] Entanglement = { 1.0, 0, 0, 0.663325 };          // There must be one target for every initial state

        public MeasureQuantumEntanglement(int timeChunks, double tf, int count, int epochs, bool verbose, bool signed, string filename)
        {
            TimeChunks = timeChunks;
            Tf         = tf;
            Count      = count;
            Epochs     = epochs;
            Verbose    = verbose;
            Signed     = signed;
            FileName   = filename;
            CountStep  = DEFAULTCOUNTSTEP;

            if (String.IsNullOrEmpty(FileName))
            {
                string currentDate = Convert.ToString(DateTime.Now).Replace("/", "").Replace(":", "_").Replace(" ", "_");
                FileName = BaseFileName + currentDate + FileNameExt;
            }

            if (Verbose)
            {
                Console.WriteLine();
                Console.WriteLine($"File Name: {FileName}");
                Console.WriteLine();
                Console.WriteLine("Entanglement:      Bell        Flat         C           P");
                Console.WriteLine("      Target:  " + Entanglement[0].ToString(FMT) + "  " +
                                                      Entanglement[1].ToString(FMT) + "  " +
                                                      Entanglement[2].ToString(FMT) + "  " +
                                                      Entanglement[3].ToString(FMT));
            }
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
                        Stream.WriteLine(" ,Bell,Flat,C,P");
                        Stream.WriteLine("Targets," + string.Join(delimiter, Entanglement));
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
            string delimiter = @",";

            InitializeStream();

            CoupledTwoQubitQNN network = new CoupledTwoQubitQNN(TimeChunks, Tf);

            for (int epoch = 1; epoch <= Epochs; epoch++)
            {
                double[] entanglement = network.MeasureEntanglementWitness(States, Count);

                if (!Signed)
                {
                    for (int i = 0; i < TimeChunks; i++)
                    {
                        entanglement[i] = Math.Abs(entanglement[i]);
                    }
                }

                if (Verbose)
                {
                    Console.WriteLine("     " + epoch.ToString("0000000") + ":  " +
                                                entanglement[0].ToString(FMT) + "  " +
                                                entanglement[1].ToString(FMT) + "  " +
                                                entanglement[2].ToString(FMT) + "  " +
                                                entanglement[3].ToString(FMT));
                }

                Stream.WriteLine(epoch.ToString() + delimiter + string.Join(delimiter, entanglement));
            }

            Stream.Close();
        }

        public void RunWitnessTest()
        {
            string delimiter = @",";

            InitializeStream();

            CoupledTwoQubitQNN network = new CoupledTwoQubitQNN(TimeChunks, Tf);

            for (int currentCount = CountStep; currentCount <= Count; currentCount += CountStep )
            {
                double[] entanglement = network.MeasureEntanglementWitness(States, currentCount);

                if (!Signed)
                {
                    for (int i = 0; i < TimeChunks; i++)
                    {
                        entanglement[i] = Math.Abs(entanglement[i]);
                    }
                }

                if (Verbose)
                {
                    Console.WriteLine("     " + currentCount.ToString("0000000") + ":  " + 
                                                entanglement[0].ToString(FMT) + "  " +
                                                entanglement[1].ToString(FMT) + "  " +
                                                entanglement[2].ToString(FMT) + "  " +
                                                entanglement[3].ToString(FMT));
                }

                Stream.WriteLine(currentCount.ToString() + delimiter + string.Join(delimiter, entanglement));
            }

            Stream.Close();
        }
    }
}