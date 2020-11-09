using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using System;
using System.Collections.Generic;

namespace Aircadia.ObjectModel.Treatments.DOE
{
	[Serializable()]
    public abstract class DesignOfExperiment : Treatment
    {
		public WorkflowComponent Component { get; set; }

		public DesignOfExperiment(string name, string description)
            : base(name, description)
        {
        }

		public override bool ApplyOn(ExecutableComponent ec)
		{
			if (ec is WorkflowComponent wc)
			{
				Component = wc;
				return ApplyOn();
			}
			return false;
		}

		protected void Execute_(ExecutableComponent oModSub, long NFactors, long NSamples, double[,] inputsTable, string[] factorNames, string[] responseNames)
		{
			int[] indices = new int[NFactors];
			for (int i = 0; i < NFactors; i++)
				indices[i] = oModSub.ModelDataInputs.IndexOf(oModSub.ModelDataInputs.Find(d => d.Name == factorNames[i]));

			List<Data> allData = (oModSub as WorkflowComponent).GetAllData();
			long NResponses = allData.Count - NFactors;
			int[] indices2 = new int[NResponses];
			for (int i = 0; i < NResponses; i++)
				indices2[i] = allData.IndexOf(allData.Find(d => d.Name == responseNames[i]));

			IterationSize = NSamples;
			using (var csvFile = new CSVFiler(CsvPath))
			{
				for (int i = 0; i < NSamples; i++)
				{
					// If user request to cancel the iterations the method will throw
					EndIteratoinIfCancelled();

					for (int j = 0; j < NFactors; j++)
						oModSub.ModelDataInputs[indices[j]].Value = inputsTable[i, j];

                    bool statusToCheck = false;
                    try
                    {
                        statusToCheck = oModSub.Execute();
                    }
                    catch(Exception e)
                    {

                    }

					// Report that i iterations have been completed
					ReportProgress(i);

					// Execute database insert command
					if (statusToCheck)
					{
						csvFile.NewRow();

						csvFile.AddToRow(i);

						for (int d = 0; d < NFactors; d++)
							csvFile.AddToRow(oModSub.ModelDataInputs[indices[d]]);

						for (int d = 0; d < NResponses; d++)
							csvFile.AddToRow(allData[indices2[d]]);

                        var con = allData[indices2[0]];


                        csvFile.WriteRow();
					}
                    else
                    {
                        csvFile.NewRow();

                        csvFile.AddToRow(i);

                        for (int d = 0; d < NFactors; d++)
                            csvFile.AddToRow(oModSub.ModelDataInputs[indices[d]]);

                        for (int d = 0; d < NResponses; d++)
                            csvFile.AddToRow(0.0);

                        csvFile.WriteRow();
                    }
				}
			}
		}
	}
}
