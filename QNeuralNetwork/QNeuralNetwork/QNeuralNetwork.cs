using System;
using Microsoft.Quantum.Simulation.Core;
using Microsoft.Quantum.Simulation.Simulators;

namespace Quantum.QNeuralNetwork
{
    public class CoupledTwoQubitQNN
    {
        /* {2008arXiv0808.1558B} Behrman, E.~C., Steck, J.~E., Kumar, P., \& Walsh, K.~A.\ 2008, arXiv:0808.1558
         * Two interacting qubits, labeled A and B, can be used to build a quantum gate where each qubit interacts 
         * with a coupling (connectivity) that can be externally adjusted. This is a dynamical system that can be 
         * prepared in an initial (input) state, which then evolves in time to the point where it can be measured 
         * at some final time to yield an output. Adjustable physical parameters of the qubits allow “programming” 
         * to “compute” a specified output in response to a given input. Consider a two-qubit quantum system that 
         * evolves in time according to the Hamiltonian: 
         * 
         *              H = K_A*σ_{xA} + K_B*σ_{xB} + ε_A*σ_{zA} + ε_B*σ_{zB} + ζ*σ_{zA} σ_{zB} (1)
         *                 
         * where {σ} are the Pauli operators corresponding to each of the two qubits, A and B, K_A and K_B are the 
         * tunneling amplitudes, ε_A and, ε_B are the biases, and ζ the qubit-qubit coupling.
         */

        private readonly double[] TunnelingA = { 2.4886, 2.4730, 2.4852, 2.4949 };      // { 2.489,  2.473,  2.485,  2.495 };
        private readonly double[] TunnelingB = { 2.4886, 2.4730, 2.4852, 2.4949 };      // { 2.489,  2.473,  2.485,  2.495 };
        private readonly double[] BiasA = { 0.092889, 0.11577, 0.095443, 0.083292 };    // { 0.0929, 0.116, 0.0954, 0.0833 };
        private readonly double[] BiasB = { 0.092889, 0.11577, 0.095443, 0.083292 };    // { 0.0929, 0.116, 0.0954, 0.0833 };
        private readonly double[] Coupling = { 0.03820, 0.12759, 0.11692, 0.038180 };   // { 0.0382, 0.128,  0.117, 0.0382 };

        //		let parameters = [4.511031; 2.300524; 5.355890]; // values come from Cosine-Sine Decomp, computed in MATLAB


        private readonly int TimeChunks;
        private readonly double TimeInterval;

        public CoupledTwoQubitQNN(int numberOfTimeChunks, double timeInterval)
        {
            TimeChunks = numberOfTimeChunks;
            TimeInterval = timeInterval;
        }

        public QArray<QArray<double>> GetAngles(double timeInterval)
        // GetAngles computes the weights using the amplitudes, biases, and qubit couping parameters
        {
            QArray<QArray<double>> weights = new QArray<QArray<double>>(TimeChunks);

            for (int i = 0; i < TimeChunks; i++)
            {
                weights[i] = new QArray<double>(0, 0, 0, 0, 0);

                double timeScale = TimeChunks * timeInterval * Math.PI;
                double normalFactorA = Math.Sqrt(Math.Pow(TunnelingA[i], 2) + Math.Pow(BiasB[i], 2));
                double normalFactorB = Math.Sqrt(Math.Pow(TunnelingB[i], 2) + Math.Pow(BiasB[i], 2));

                weights[i][0] = timeScale * Coupling[i];
                weights[i][1] = Math.Asin(TunnelingA[i] / normalFactorA);
                weights[i][2] = Math.Asin(TunnelingB[i] / normalFactorB);
                weights[i][3] = timeScale * normalFactorA;
                weights[i][4] = timeScale * normalFactorB;
            }

            return weights;
        }

        public double MeasureEntanglementWitness(double[] state, int count)
        {
            double witness = 0;

            QArray<QArray<double>> weights = GetAngles(TimeInterval);
            QArray<double> parameters = new QArray<double>(state);

            using (var sim = new QuantumSimulator())
            {
                long res = QNNChunkedMeasureEntanglement.Run(sim, weights, parameters, count).Result;

                // data processing for returned values from quantum neural network
                witness = (double)res / (double)count;
            }

            return witness;
        }
    }
}

