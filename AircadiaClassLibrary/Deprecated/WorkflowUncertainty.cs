using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Workflows;
using Aircadia.ObjectModel.Distributions;
using Aircadia.ObjectModel.Treatments;

namespace Aircadia
{
	[Serializable()]
	public class WorkflowUncertainty : Workflow
	{
		public WorkflowComponent InnerWorkflow
		{
			get;
		}
		Dictionary<string, string[]> mappings;
		List<string> targetOutputs;


		public WorkflowUncertainty(string name, string description, WorkflowComponent innerWorkflow, List<Data> outerInputs, List<Data> outerOutputs, Dictionary<string, string[]> mappings, List<string> targetOutputs) : base(name, description, outerInputs, outerOutputs, new List<WorkflowComponent>() { innerWorkflow }, new List<WorkflowComponent>() { innerWorkflow })
		{
			Name = name;

			this.mappings = mappings;
			InnerWorkflow = innerWorkflow;
			this.targetOutputs = targetOutputs;
		}

		/// <summary>
		/// Executes the cSubprocessUNCER object
		/// </summary>
		/// <returns></returns>
		public override bool Execute()
		{
			var input_uncer_testpoints = new Dictionary<string, double[]>();

			#region setting inputs to URQ
			int NInput = ModelDataInputs.Count / 4;
			string[,] RobVars = new string[NInput, 5];


			int counter = 0;
			var innerInputs = InnerWorkflow.ModelDataInputs.ToDictionary(d => d.Name);
			var outerInputs = ModelDataInputs.ToDictionary(d => d.Name);
			foreach (string name in mappings.Keys)
			{
				string[] mapped = mappings[name];
				if (mapped.Length != 4)
					continue;

				innerInputs[name].ValueAsDouble = outerInputs[mapped[0]].ValueAsDouble;

				RobVars[counter, 0] = name;
				RobVars[counter, 1] = outerInputs[mapped[1]].ValueAsString;
				RobVars[counter, 2] = outerInputs[mapped[2]].ValueAsString;
				RobVars[counter, 3] = outerInputs[mapped[3]].ValueAsString;
				RobVars[counter, 4] = 0.ToString();

				counter++;
			}


			var input_options = new ArrayList { RobVars };


			//cTreatment_InOut input_options_treat = new cTreatment_InOut(input_options);
			var input_options_treat = new Treatment_InOut_OP_Input(input_options);
			#endregion

			string[,] temp_targs = new string[targetOutputs.Count, 7]; // get number of targets (i.e. from set the 'targetOutputs' in constructor)

			counter = 0;
			foreach (string outVar in targetOutputs)
			{
				temp_targs[counter, 0] = outVar;
				temp_targs[counter, 1] = "1";
				temp_targs[counter, 2] = "minimise";
				temp_targs[counter, 3] = "";
				temp_targs[counter, 4] = "";
				temp_targs[counter, 5] = "Quantile";
				temp_targs[counter, 6] = "+";

				counter++;
			}

			string[,] temp_constraints = new string[0, 0];
			string[,] temp_constants = new string[0, 0];
			input_options.Add(temp_targs);
			input_options.Add(temp_constraints);
			input_options.Add(temp_constants);

			input_options.Add("RobustAnalysis");

			throw new Exception("This Workflow is deprecated");
			//var output_to_set_treat = new Treatment_InOut_OP_Output("Study not executed yet.");

			////cTreatment_ROP URQ = new cTreatment_ROP("UncerPropInnerWF", input_options_treat, output_to_set_treat);
			//input_options_treat = new Treatment_InOut_OP_Input(input_options);
			//var URQ = new Treatment_ROP("UncerPropInnerWF", input_options_treat, output_to_set_treat) { databaseFileName = "URQ" };
			//URQ.ApplyOn(InnerWorkflow);

			//string[,] temp_store_outputs = URQ.RobustData;

			//var MeanAndSD = new List<string>();
			//counter = 0;
			//int NOut = temp_store_outputs.GetLength(1);
			//for (int i = 0; i < NOut; i++)
			//{
			//	switch (counter)
			//	{
			//		case 0:
			//			counter++;
			//			break;
			//		case 1:
			//			counter++;
			//			MeanAndSD.Add(temp_store_outputs[0, i]);
			//			break;
			//		case 2:
			//			MeanAndSD.Add(temp_store_outputs[0, i]);
			//			counter = 0;
			//			break;
			//	}
			//}

			//for (int i = 0; i < ModelDataOutputs.Count; i++)
			//{
			//	if (i % 2 == 0)
			//		ModelDataOutputs[i].ValueAsDouble = Convert.ToDouble(MeanAndSD.ElementAt(i));
			//	else
			//		ModelDataOutputs[i].ValueAsDouble = Math.Sqrt(Convert.ToDouble(MeanAndSD.ElementAt(i)));
			//}

			//return true;
		}
	}
}
