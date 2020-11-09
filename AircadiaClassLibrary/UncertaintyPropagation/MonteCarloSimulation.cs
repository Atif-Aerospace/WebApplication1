using System;
using System.Collections.Generic;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Distributions;
using MathNet.Numerics.LinearAlgebra;
using Aircadia.ObjectModel.Treatments;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;
using System.Linq;

namespace Aircadia
{
	public class MonteCarloSimulation : IUncertaintyPropagatorBySamples
	{
		[Serialize]
		public int NSamples { get; }

		public Matrix<double> Samples { get; private set; }

		private readonly bool createFile;
		private readonly string path;
		private CSVFiler filer;

		[DeserializeConstructor]
		public MonteCarloSimulation(int nSamples = 1000)
		{
			createFile = false;
			path = String.Empty;
			NSamples = nSamples;
		}

		public MonteCarloSimulation(string path, int nSamples = 1000)
		{
			createFile = true;
			this.path = path;
			NSamples = nSamples;
		}

		public void Propagate(List<IProbabilityDistribution> inputDistributions, List<IProbabilityDistribution> outputDistributions, WorkflowComponent innerWorkflow)
		{
			List<IMCSDistribution> inputs = null;
			List<ProbabilityDistributionFromSamples> outputs = null;
			try
			{
				inputs = inputDistributions.Cast<IMCSDistribution>().ToList();
			}
			catch (InvalidCastException)
			{
				Console.WriteLine("At least one of the input distributions used for this Monte Carlo Simulation are not of the right kind. Samples cannot be generated");
				throw;
			}

			try
			{
				outputs = inputDistributions.Cast<ProbabilityDistributionFromSamples>().ToList();
			}
			catch (InvalidCastException)
			{
				Console.WriteLine("At least one of the output distributions used for this Monte Carlo Simulation are not of the right kind. Distribution annot be infered from samples");
				throw;
			}

			Propagate(inputs, outputs, innerWorkflow); 
		}

		public void Propagate(List<IMCSDistribution> inputDistributions, List<ProbabilityDistributionFromSamples> outputDistributions, WorkflowComponent innerWorkflow)
		{
			filer = (createFile) ? new CSVFiler(path) : null;

			try
			{
				int Ninputs = inputDistributions.Count;
				int Noutputs = outputDistributions.Count;

				Matrix<double> samples = Matrix<double>.Build.Dense(NSamples, Ninputs + Noutputs);

				for (int i = 0; i < inputDistributions.Count; i++)
					samples.SetColumn(i, inputDistributions[i].GetSamples(NSamples));

				for (int s = 0; s < NSamples; s++)
				{
					int v = 0;
					foreach (IProbabilityDistribution input in inputDistributions)
					{
						input.Data.Value = samples[s, v];
						v++;
					}

					// Execute workflow
					bool statusToCheck = innerWorkflow.Execute();

					foreach (IProbabilityDistribution output in outputDistributions)
					{
						samples[s, v] = Convert.ToDouble(output.Data.Value);
						v++;
					}

					if (createFile && statusToCheck)
					{
						// Execute database insert command
						filer.NewRow();

						//filer.AddToRow(i);

						foreach (Data input in innerWorkflow.ModelDataInputs)
							filer.AddToRow(input);

						foreach (Data output in innerWorkflow.ModelDataInputs)
							filer.AddToRow(output);

						filer.WriteRow();
					}
				}

				int o = Ninputs;
				foreach (IProbabilityDistribution output in outputDistributions)
				{
					output.Update(samples.Column(o).AsArray());
					o++;
				}

				Samples = samples;

			}
			finally
			{
				filer?.Dispose();
			}
		}
	}
}
