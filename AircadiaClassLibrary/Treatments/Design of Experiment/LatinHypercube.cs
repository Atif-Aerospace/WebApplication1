using System;
using System.Collections.Generic;
using System.Linq;
using Aircadia.ObjectModel.DataObjects;
using System.Data;
using System.Xml.Linq;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Studies;

namespace Aircadia.ObjectModel.Treatments.DOE
{
	[Serializable()]
    public class LatinHypercube : DesignOfExperiment
    {
		public int NFactors => Factors.Count;
		public long NSamples { get; }
		public List<Factor> Factors { get; }
		public List<Data> Responses { get; }

        Random random = new Random();

		public LatinHypercube(string name, string description, WorkflowComponent component, List<Factor> factors, List<Data> responses, long samples) :
            base(name, description)
        {
			Component = component;
            // Copy factors to avoid aliasing problems when editing studies
            Factors = factors.Select(f => f.Copy()).ToList();
            Responses = responses;
			NSamples = samples;
		}


		public override bool ApplyOn()
		{

			double[,] inputsTable = GetLatinHypercube();

			string[] factorNames = Factors.Select(f => f.Name).ToArray();
			string[] responseNames = Responses.Select(r => r.Name).ToArray();

			Execute_(Component, NFactors, NSamples, inputsTable, factorNames, responseNames);
			return true;
		}

		private double[,] GetLatinHypercube()
		{
			int NSamples = Convert.ToInt32(this.NSamples);


			int[] u = new int[NSamples];
			for (int i = 0; i < NSamples; i++)
				u[i] = i;


			int[,] H = new int[NSamples, NFactors];
			for (int i = 0; i < NFactors; i++)
			{
				List<int> order = RandomIndicesDenseVector(NSamples);
				for (int j = 0; j < NSamples; j++)
					H[j, i] = u[order[j]];
			}

			double[,] inputsTable = new double[NSamples, NFactors];
			for (int j = 0; j < NFactors; j++)
			{
				Factor factor = Factors[j];
				decimal start = factor.LowerBound;
				decimal step = (factor.UpperBound - factor.LowerBound) / (NSamples - 1);
				for (int i = 0; i < NSamples; i++)
				{
					inputsTable[i, j] = Convert.ToDouble(start + H[i, j] * step);
					if (factor.Data is IntegerData)
						inputsTable[i, j] = (int)Math.Round(inputsTable[i, j], MidpointRounding.AwayFromZero);
				}
			}

			return inputsTable;
		}

        public List<int> RandomIndicesDenseVector(int num)
        {
            //
            var shuffledIndices = new List<int>(num);

            // Create a list to store the indices
            var indices = new List<int>(num);
            for (int i = 0; i < num; i++)
                indices.Add(i);

            // Shuffle the list
            while (indices.Count > 0)
            {
                int position = random.Next(indices.Count);
                shuffledIndices.Add(indices[position]);
                indices.RemoveAt(position);
            }

            return shuffledIndices;
        }

		public override void CreateFolder()
		{
			base.CreateFolder();

			// Add custom metadata for Latin Hypercube
			metadata.AddAttribute("Type", "DesignsStudy");
			metadata.AddAttribute("WorkflowName", Component.Name);

			metadata.AddTag("Name", "Latin Hypercube Sampling");

			metadata.AddParameter(new IntegerData("ID"));

			foreach (Factor factor in Factors)
				metadata.AddParameter(factor.Data, new XElement("HyperCubeFactor",
					new XAttribute("StartingValue", factor.LowerBound),
					new XAttribute("EndingValue", factor.UpperBound)));

			foreach (Data response in Responses)
				metadata.AddParameter(response, new XElement("HyperCubeResponse"));

			metadata.Save();
		}

	}
}
