
namespace Quantum.QNeuralNetwork
{
    open Microsoft.Quantum.Primitive;
    open Microsoft.Quantum.Canon;

	open Microsoft.Quantum.Extensions.Math;
	open Microsoft.Quantum.Extensions.Convert;

	operation QNNChunkedMeasureEntanglement (weights: Double[][], parameters: Double[], count: Int) : (Int)
	{
		body
        {
			mutable entanglement = 0;

			let bases = [PauliZ; PauliZ];

			using (qubits = Qubit[2])
			{
				for(k in 1..count)
				{
					// Initialize qubits to desired state using parameters
					SetState(parameters, qubits);
					
					// Compute quantumn neural network operator in 4 time chunks, then apply entanglement witness
					for (j in 0..3) {
						QNNTimeChunk(weights[j], qubits); }
					
					// witness measures the qubit pair, same spin: Zero, different spin: One
					// computing this here because passing a mutable variable is irritating
					let witness = Measure( bases, qubits );
					
					if (witness == Zero) {
						set entanglement = entanglement + 1; }
					else {
						set entanglement = entanglement - 1; }

					ResetAll(qubits);
				}
			}

			// return engtanglement as a sum of measured witnesses
			return (entanglement);
		}
	}

	operation SetState(parameters: Double[], qubits: Qubit[]) : ()
	{
		body
		{
			if (Length(parameters) < 3)	
			{
				let n = Round(parameters[0]);

				if (n == 1)            // Bell state
				{
					H(qubits[0]);
					CNOT(qubits[0], qubits[1]);
				}
				elif (n == 2)          // Flat state
				{
					H(qubits[0]);
					H(qubits[1]);
				}
				elif (n == 3)          // C state 
				{
					X(qubits[0]);
					Ry(2.0*ArcCos(1.0/Sqrt(5.0)), qubits[1]);   // 2*arccos(1/sqrt(5)) ~ 2.21429734
				}
				else                   // Preset partially entangled state
				{
					let pcoeff = [ComplexPolar(1.0/Sqrt(3.0),0.0); ComplexPolar(1.0/Sqrt(3.0),0.0); ComplexPolar(1.0/Sqrt(3.0),0.0); ComplexPolar(0.0,0.0)];
					
					let qubitsBE = BigEndian(qubits);

					PrepareArbitraryState(pcoeff, qubitsBE);
				}
			}
			elif (Length(parameters) < 4)
			{
				let controls = [qubits[0]];

				(Controlled Ry)(controls, (parameters[0], qubits[1]));
				Ry(-parameters[0], qubits[1]);
		
				SWAP(qubits[0], qubits[1]);
				(Controlled Ry)(controls, (-parameters[1], qubits[1]));
				Ry(parameters[1], qubits[1]);

				SWAP(qubits[0], qubits[1]);
				(Controlled Ry)(controls, (-parameters[2], qubits[1]));
				Ry(parameters[2], qubits[1]);
			}
			else
			{
				let pcoeff = [ ComplexPolar(parameters[0],parameters[1]); 
							   ComplexPolar(parameters[2],parameters[3]); 
							   ComplexPolar(parameters[4],parameters[5]); 
							   ComplexPolar(parameters[6],parameters[7]) ];
				
				let qubitsBE = BigEndian(qubits);
				
				PrepareArbitraryState(pcoeff, qubitsBE);
			}
		}
	}

	operation BeginState (n: Int, qubits: Qubit[]) : ()
    {
        body
        {
            if (n == 1)            // Bell state
			{
				H(qubits[0]);
				CNOT(qubits[0], qubits[1]);
			}
            elif (n == 2)          // Flat state
            {
				H(qubits[0]);
				H(qubits[1]);
            }
			elif (n == 3)          // C state 
			{
				X(qubits[0]);
				Ry(2.0*ArcCos(1.0/Sqrt(5.0)), qubits[1]);   // 2*arccos(1/sqrt(5)) ~ 2.21429734
			}
			else                   // Partially entangled state
			{
				let pcoeff = [ComplexPolar(1.0/Sqrt(3.0),0.0); ComplexPolar(1.0/Sqrt(3.0),0.0); ComplexPolar(1.0/Sqrt(3.0),0.0); ComplexPolar(0.0,0.0)];
				let qubitsBE = BigEndian(qubits);
				PrepareArbitraryState(pcoeff, qubitsBE);
			}
        }
    }

	operation QNNTimeChunk (weights: Double[], qubits: Qubit[]) : ()
    {
        body
        {
			// two qubit e^(iH)
			CNOT(qubits[0], qubits[1]);
			Rz(weights[0], qubits[1]);		// R(PauliZ, beta[j], qubits[1]);
			CNOT(qubits[0], qubits[1]);

			// one qubit e^(iH)
			Ry(-weights[1], qubits[0]);		// R(PauliY, -theta_a[j], qubits[0]);
			Ry(-weights[2], qubits[1]);		// R(PauliY, -theta_b[j], qubits[1]);

			Rz(weights[3],  qubits[0]);		// R(PauliZ, alpha_a[j], qubits[1]);
			Rz(weights[4],  qubits[1]);		// R(PauliZ, alpha_b[j], qubits[0]);

			Ry(weights[1],  qubits[0]);		// R(PauliY, theta_a[j], qubits[0]);
			Ry(weights[2],  qubits[1]);		// R(PauliY, theta_b[j], qubits[1]);
		}
    }
}
