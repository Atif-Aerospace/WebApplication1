/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Studies;
using System.Xml.Linq;

namespace Aircadia.ObjectModel.Treatments.DOE
{
	[Serializable()]
	//// Subclass of the treatment class. This is the treatment class for random DOE
	public class RandomDoE : DesignOfExperiment
	{
		public long NFactors => Factors.Count;
		public long NSamples { get; }
		public List<Factor> Factors { get; }
		public List<Data> Responses { get; }

		Random rd = new Random();

		public RandomDoE(string name, string description, WorkflowComponent component, List<Factor> factors, List<Data> responses, long samples) :
			base(name, description)
		{
			Component = component;
            // Copy factors to avoid aliasing problems when editing studies
            Factors = factors.Select(f => f.Copy()).ToList();
            Responses = responses;
			NSamples = samples;
		}

		public override string ToString()
		{
			string output = "";
			foreach (Factor factor in Factors)
				output = output + factor.LowerBound + "->" + factor.Name + "->" + factor.UpperBound + "\r\n";
			output = output + "Total Number of Samples: " + NSamples;
			return output;
		}

		public override bool ApplyOn()
		{
			double[,] inputsTable = Randomarray();

			string[] factorNames = Factors.Select(f => f.Name).ToArray();
			string[] responseNames = Responses.Select(r => r.Name).ToArray();

			Execute_(Component, NFactors, NSamples, inputsTable, factorNames, responseNames);

			return true;
		}

		/// <summary>
		/// Method to create random sample of data
		/// </summary>
		/// <returns></returns>
		public double[,] Randomarray()
		{
			int j = 0; decimal rv; double cv;
			double[,] inputsTable = new double[NSamples, NFactors];
			for (int i = 0; i < NSamples; i++)
			{
				j = 0;
				foreach (Factor f in Factors)
				{
					rv = Convert.ToDecimal(rd.NextDouble());
					cv = Convert.ToDouble(f.LowerBound + (f.UpperBound - f.LowerBound) * rv);
					inputsTable[i, j] = cv;
					j = j + 1;
				}
			}
			return inputsTable;
		}

		public override void CreateFolder()
		{
			base.CreateFolder();

			// Add custom metadata for Random DoE
			metadata.AddAttribute("Type", "DesignsStudy");
			metadata.AddAttribute("WorkflowName", Component.Name);

			metadata.AddTag("Name", "Random Sampling");

			var parametersElement = new XElement("Parameters");

			metadata.AddParameter(new IntegerData("ID"));

			foreach (Factor factor in Factors)
				metadata.AddParameter(factor.Data, new XElement("RandomFactor",
					new XAttribute("StartingValue", factor.LowerBound),
					new XAttribute("EndingValue", factor.UpperBound)));

			foreach (Data response in Responses)
				metadata.AddParameter(response, new XElement("HyperCubeResponse"));

			metadata.Save();
		}
	}
}
