using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



using System.IO;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;

using Aircadia.ObjectModel.Treatments.DOE;
using Aircadia.ObjectModel.Workflows;

using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;



namespace Aircadia.ObjectModel.Studies
{
    [Serializable()]
    public class FullFactorialDOEStudy : DOEStudy
    {
        public List<int> noOfLevels; // List of corresponding number of levels for factors
        public List<decimal> startingValues;
        public List<decimal> stepSizes;

        public FullFactorialDOEStudy(string name, string description, WorkflowComponent workflow, List<Data> factors, List<decimal> startingValues, List<decimal> stepSizes, List<int> noOfLevels, List<Data> responses)
            : base(name, description, workflow, factors, responses)
        {
            base.StudiedComponent = workflow;

            this.startingValues = startingValues;
            this.stepSizes = stepSizes;
            this.noOfLevels = noOfLevels;








			ColumnNames.Add("ID");
			ColumnTypes.Add(DataTypes.INTEGER);
			ColumnFormats.Add(0);
			ColumnUnits.Add("");

            for (int i = 0; i < factors.Count; i++)
            {
                string columnHeader = factors[i].Name;
				ColumnNames.Add(columnHeader);
                if (factors[i] is IntegerData)
                {
					ColumnTypes.Add(DataTypes.INTEGER);
					ColumnFormats.Add(0);
					ColumnUnits.Add(((IntegerData)factors[i]).Unit);
                }
                else if (factors[i] is DoubleData)
                {
					ColumnTypes.Add(DataTypes.DOUBLE);
					ColumnFormats.Add(((DoubleData)factors[i]).DecimalPlaces);
					ColumnUnits.Add(((DoubleData)factors[i]).Unit);
                }
            }
            for (int i = 0; i < responses.Count; i++)
            {
                string columnHeader = responses[i].Name;
				ColumnNames.Add(columnHeader);
                if (responses[i] is IntegerData)
                {
					ColumnTypes.Add(DataTypes.INTEGER);
					ColumnFormats.Add(0);
					ColumnUnits.Add(((IntegerData)responses[i]).Unit);
                }
                else if (responses[i] is DoubleData)
                {
					ColumnTypes.Add(DataTypes.DOUBLE);
					ColumnFormats.Add(((DoubleData)responses[i]).DecimalPlaces);
					ColumnUnits.Add(((DoubleData)responses[i]).Unit);
                }
            }















			TableNames.Add(Name);



			Treatment = new FullFactorial("testFF", "", Project.ProjectPath + "\\DOE.sdf", workflow as Workflow, factors, startingValues, stepSizes, noOfLevels, responses);
			Treatment.CreateFolder();
		}

        public override bool Execute()
        {
            bool status = true;





			/*

            // Name of the results file
            StudyExecutionDialog studyExecutionDialog = new StudyExecutionDialog();
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(studyExecutionDialog);
//            studyExecutionDialog.Owner = this;
            bool? result = studyExecutionDialog.ShowDialog(); // Show model dialog

            if (result == true)
            {
                Result currentResult = new Result(studyExecutionDialog.ResultName);
                string fullPath = System.IO.Path.Combine("Studies", this.Name, studyExecutionDialog.ResultName + ".sdf");
                currentResult.DatabasePath = fullPath;
                this.Results.Add(currentResult);
                this.ActiveResult = currentResult;
            }
            else
            {
                studyExecutionDialog.Close();
            }

            */




			Treatment.Result = ActiveResult;

            status = Treatment.ApplyOn();





            string CurrentDir = System.IO.Directory.GetCurrentDirectory();
            string FileName = Name;
            if (File.Exists(CurrentDir + "\\" + FileName + ".acdstd"))
            {
                FileName = Name + "_" + DateTime.Today.Day + "-" + DateTime.Today.Month + "-" + DateTime.Today.Year + "_" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second;
            }


            /*
            string output1 = File.ReadAllText("evaluatedSolutions.txt");
            FileStream filer1 = new FileStream(CurrentDir + "\\" + FileName + ".acdstd", FileMode.Append, FileAccess.Write);
            StreamWriter evals1 = new StreamWriter(filer1);
            evals1.Write(output1);
            evals1.Close();
            filer1.Close();
            */


            return status;
        }
    }
}
