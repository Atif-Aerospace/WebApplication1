using System;
using System.Collections.Generic;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Distributions;
using Aircadia.ObjectModel.Treatments;
using Aircadia.Services.Serializers;

namespace Aircadia
{
	public class UnivariateReducedQuadrature : IUncertaintyPropagator
	{
		private readonly bool createFile;
		private readonly string path;
		private CSVFiler filer;

		[DeserializeConstructor]
		public UnivariateReducedQuadrature()
		{
			createFile = false;
			path = String.Empty;
		}

		public UnivariateReducedQuadrature(string path)
		{
			createFile = true;
			this.path = path;
		}

		public void Propagate(List<IProbabilityDistribution> inputDistributions, List<IProbabilityDistribution> outputDistributions, WorkflowComponent innerWorkflow)
		{
			filer = (createFile) ? new CSVFiler(path) : null;

			try
			{
				int NInputDistributions = inputDistributions.Count;
				int Nout = outputDistributions.Count;

				foreach (IProbabilityDistribution dist in inputDistributions)
					dist.Data.Value = dist.Mean;

				// Defining the "deltas" for the computation of the propagation stencils:
				double[] h_plus = new double[NInputDistributions];
				double[] h_minus = new double[NInputDistributions];
				for (int j = 0; j < NInputDistributions; j++)
				{
					IProbabilityDistribution dist = inputDistributions[j];
					h_plus[j] = dist.Skewness / 2 + Math.Sqrt(dist.Kurtosis - (3.0 / 4) * Math.Pow(dist.Skewness, 2));
					h_minus[j] = dist.Skewness / 2 - Math.Sqrt(dist.Kurtosis - (3.0 / 4) * Math.Pow(dist.Skewness, 2));
				}

				// Setup of the URQ weights:
				double W0 = 1;
				double[] Wp = new double[NInputDistributions];
				double[] Wp_plus = new double[NInputDistributions];
				double[] Wp_minus = new double[NInputDistributions];
				double[] Wp_plusminus = new double[NInputDistributions];
				for (int i = 0; i < NInputDistributions; i++)
				{
					W0 += 1.0 / (h_plus[i] * h_minus[i]);
					double delta = h_plus[i] - h_minus[i];
					Wp[i] = 1.0 / delta;
					Wp_plus[i] = (Math.Pow(h_plus[i], 2) - h_plus[i] * h_minus[i] - 1) / (Math.Pow(delta, 2));
					Wp_minus[i] = (Math.Pow(h_minus[i], 2) - h_plus[i] * h_minus[i] - 1) / (Math.Pow(delta, 2));
					Wp_plusminus[i] = 2 / (Math.Pow(delta, 2));
				}


				// Center point evaluation
				ExecutePoint(innerWorkflow, Nout, out double[] output0);

				double[] means = new double[Nout];
				double[] variances = new double[Nout];
				for (int i = 0; i < Nout; i++)
					means[i] = W0 * output0[i];

				// Stencil evaluation:
				for (int p = 0; p < NInputDistributions; p++)
				{
					IProbabilityDistribution dist = inputDistributions[p];

					// Dimension i, forward stencil point evaluation
					dist.Data.Value = dist.Mean + h_plus[p] * Math.Sqrt(dist.Variance);
					ExecutePoint(innerWorkflow, Nout, out double[] output_plus);

					// Dimension i, backeard stencil point evaluation
					dist.Data.Value = dist.Mean + h_minus[p] * Math.Sqrt(dist.Variance);
					ExecutePoint(innerWorkflow, Nout, out double[] output_minus);

					// Estimation of the mean and variance for all the model outputs:
					for (int j = 0; j < Nout; j++)
					{
						means[j] += Wp[p] * ((output_plus[j] / h_plus[p]) - (output_minus[j] / h_minus[p]));
						double deltap = (output_plus[j] - output0[j]) / h_plus[p];
						double deltam = (output_minus[j] - output0[j]) / h_minus[p];
						variances[j] += Wp_plus[p] * deltap * deltap
									 + Wp_minus[p] * deltam * deltam
									 + Wp_plusminus[p] * deltap * deltam;
					}

					// Recover original value
					dist.Data.Value = dist.Mean;
				}

				for (int i = 0; i < outputDistributions.Count; i++)
					outputDistributions[i].Update(new double[] { means[i], variances[i], 0, 3 });
			}
			finally
			{
				filer?.Dispose();
			}
		}

		private void ExecutePoint(WorkflowComponent innerWorkflow, int Nout, out double[] outputValues)
		{
			bool SubProcessExecutionStatus = innerWorkflow.Execute();
			if (SubProcessExecutionStatus == false)
			{ }

			outputValues = new double[Nout];
			int v = 0;
			foreach (Data output in innerWorkflow.ModelDataOutputs)
			{
				outputValues[v] = Convert.ToDouble(output.Value);
				v++;
			}

			if (createFile && SubProcessExecutionStatus)
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
	}
}
