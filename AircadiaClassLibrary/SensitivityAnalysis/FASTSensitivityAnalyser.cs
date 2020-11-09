using Aircadia.ObjectModel.Distributions;
using Aircadia.ObjectModel.Models;
using Aircadia.Treatments.Uncertainty;
using MathNet.Numerics.LinearAlgebra;
using System.Collections.Generic;
using System.IO;
using static System.Math;

namespace Aircadia.ObjectModel.Treatments
{
	public class FASTSensitivityAnalyser : ISensitivityAnalyser
	{
		readonly IUncertaintyPropagatorBySamples propagator;
		private CSVFiler filer;
		private  readonly string path;
		private readonly bool createFile;

		public FASTSensitivityAnalyser(bool createFile = false, string path = "")
		{
			this.createFile = createFile;
			this.path = path;

			string directory = Path.GetDirectoryName(path);
			string fileName = $"{Path.GetFileNameWithoutExtension(path)}.samples{Path.GetExtension(path)}";
			propagator = new FASTPropagator(Path.Combine(directory, fileName));
		}

		public void Analyse(List<IProbabilityDistribution> inputDistributions, List<IProbabilityDistribution> outputDistributions, WorkflowComponent innerWorkflow)
		{
			using (filer = new CSVFiler(path))
			{
				int NVariables = inputDistributions.Count;
				int NTargets = outputDistributions.Count;

				var sampler = new FASTSampler(NVariables);
				int NSamples = sampler.Ns;
				int Ns2 = NSamples / 2;

				propagator.Propagate(inputDistributions, outputDistributions, innerWorkflow);
				Matrix<double> samples = propagator.Samples;

				double[] ResultMeans = new double[NTargets];
				double[] ResultVariances = new double[NTargets];
				double[] ResultStandardDeviations = new double[NTargets];
				for (int r = 0; r < NTargets; r++) //for rth result (output)
				{
					//to store intermedient sum
					double sum = 0;
					double sumSquared = 0;

					int r2 = NVariables + r;
					for (int s = 0; s < Ns2; s++)
					{
						double sample = samples[s, r2];
						sum += sample;
						sumSquared += sample * sample;
					}

					ResultMeans[r] = sum / Ns2;
					ResultVariances[r] = sumSquared / Ns2 - (ResultMeans[r]) * (ResultMeans[r]);                                //sobol's original approach (satelli's approach to be added)
					ResultStandardDeviations[r] = Sqrt(ResultVariances[r]);
				}


				var A = new List<double>();
				var B = new List<double>();
				var Lambda = new List<double>();
				double[] samplesFAST = sampler.Samples;

				Matrix<double> Sensitivities = Matrix<double>.Build.Dense(NTargets, NVariables);
				for (int t = 0; t < NTargets; t++)
				{
					ResultVariances[t] = 0;
					Lambda.Clear();

					//for (int j = -(NSamples - 1) / 2; j <= (NSamples - 1) / 2; j++)   in "A Quantitative Model-Independent Method for Global Sensitivity Analysis of Model Output"
					for (int s = 0; s < Ns2; s++)     //seems -(NSamples - 1) / 2 ~ 0 are not used
					{
						double tempFA = 0;
						double tempFB = 0;


						for (int k = 0; k < NSamples; k++)
						{
							//tempFA += ResultMatrix[k][t] * Cos(s * samplesFAST[k]);
							//tempFB += ResultMatrix[k][t] * Sin(s * samplesFAST[k]);
							tempFA += samples[k, NVariables + t] * Cos(s * samplesFAST[k]);
							tempFB += samples[k, NVariables + t] * Sin(s * samplesFAST[k]);
						}
						tempFA = tempFA / NSamples;
						tempFB = tempFB / NSamples;

						A.Add(tempFA);
						B.Add(tempFB);

						double lambda = Pow(tempFA, 2) + Pow(tempFB, 2);
						Lambda.Add(lambda);
						ResultVariances[t] += lambda;
					}

					ResultVariances[t] *= 2;
					ResultStandardDeviations[t] = Sqrt(ResultVariances[t]);
					ResultMeans[t] = samples.SubMatrix(0, Ns2, NVariables + t, 1).Column(0).Sum() / Ns2;

					// Get Sensitivities
					for (int v = 0; v < NVariables; v++)
					{
						Sensitivities[t, v] = 0;
						for (int p = 1; p <= sampler.M; p++)
						{
							int temp_counter = p * sampler.Omega[v] - 1;
							Sensitivities[t, v] += Lambda[temp_counter];
						}
						Sensitivities[t, v] *= 2 / ResultVariances[t];
					}

					//Write .csv file
					filer.NewRow();

					filer.AddToRow(t);
					filer.AddToRow(outputDistributions[t].Name);
					foreach (var s in Sensitivities.Row(t))
						filer.AddToRow(s);

					filer.WriteRow();
				}

				for (int t = 0; t < NTargets; t++)
				{
					filer.NewRow();
					filer.AddToRow(t);
					filer.AddToRow(outputDistributions[t].Name);
					for (int v = 0; v < NVariables; v++)
					{
						filer.AddToRow(Sensitivities[t, v]);
					}
					filer.AddToRow(1 - Sensitivities.Row(t).Sum());
				}
			} 
		}

	}
}
