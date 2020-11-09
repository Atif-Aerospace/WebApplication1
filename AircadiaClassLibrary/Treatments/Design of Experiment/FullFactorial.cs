using System;
using System.Collections.Generic;
using System.Linq;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using System.Data;
using System.Xml.Linq;
using Aircadia.ObjectModel.Studies;

namespace Aircadia.ObjectModel.Treatments.DOE
{
	[Serializable()]
    public class FullFactorial : DesignOfExperiment
    {
		public int NFactors => Factors.Count;
		public long NSamples { get; }
		public List<BoundedDesignVariableNoInitial> Factors { get; }
		public List<Data> Responses { get; }

        public decimal[][] arr; // factors array

        public FullFactorial(string name, string description, WorkflowComponent component, List<BoundedDesignVariableNoInitial> factors, List<Data> responses)
            : base(name, description)
        {
			Component = component;
			Factors = factors;
			Responses = responses;
			NSamples = 1;
			arr = new decimal[NFactors][];
			for (int i = 0; i < NFactors; i++)
			{
				arr[i] = Factors[i].GetValues();
				NSamples *= Factors[i].Levels;
			}
		}


        public override bool ApplyOn()
        {
			double[,] inputsTable = GetFullFactorial();

			string[] factorNames = Factors.Select(f => f.Name).ToArray();
			string[] responseNames = Responses.Select(r => r.Name).ToArray();

			Execute_(Component, NFactors, NSamples, inputsTable, factorNames, responseNames);
            return true;
        }

		private double[,] GetFullFactorial()
		{
			var permutations = new List<List<decimal>>();
			foreach (decimal init in arr[0])
			{
				var temp = new List<decimal> { init };
				permutations.Add(temp);
			}
			for (int i = 1; i < NFactors; ++i)
			{
				permutations = AddFactorToPermutations(permutations, arr[i]);
			}

			double[,] inputsTable = new double[NSamples, NFactors];
			for (int i = 0; i < NSamples; i++)
			{
				List<decimal> permutation = permutations[i];
				for (int j = 0; j < NFactors; j++)
				{
					inputsTable[i, j] = Convert.ToDouble(permutation[j]);
				}
			}
			return inputsTable;
		}

		private List<List<decimal>> AddFactorToPermutations(List<List<decimal>> priorPermutations, decimal[] additions)
		{
			var newPermutationsResult = new List<List<decimal>>();
			foreach (List<decimal> priorPermutation in priorPermutations)
			{
				foreach (decimal addition in additions)
				{
					var priorWithAddition = new List<decimal>(priorPermutation);
					priorWithAddition.Add(addition);
					newPermutationsResult.Add(priorWithAddition);
				}
			}
			return newPermutationsResult;
		}

		public override void CreateFolder()
		{
			base.CreateFolder();


			metadata.AddAttribute("Type", "DesignsStudy");
			metadata.AddAttribute("WorkflowName", Component.Name);

			//var csvElement = new XElement("CSV", new XAttribute("Path", $"{studyName}.csv"));
			//resultElement.Add(csvElement);

			metadata.AddTag("Name", "FullFactorial Sampling");

			metadata.AddParameter(new IntegerData("ID"));

			foreach (BoundedDesignVariableNoInitial factor in Factors)
				metadata.AddParameter(factor.Data, "FullFactorialFactor",
					("StartingValue", factor.LowerBound),
					("StepSize", factor.Step),
					("FinalValue", factor.UpperBound),
					("NoOfLevels", factor.Levels));

			foreach (Data response in Responses)
				metadata.AddParameter(response, new XElement("FullFactorialResponse"));

			metadata.Save();
		}
	}
}
