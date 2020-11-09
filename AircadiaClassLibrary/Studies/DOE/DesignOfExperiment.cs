using System;
using System.Collections.Generic;
using System.IO;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using System.Xml.Linq;

namespace Aircadia.ObjectModel.Studies
{
	[Serializable()]
    public abstract class DesignOfExperiment : Study
    {

        public List<string> FactorNames
        {
            get;
            set;
        }

        public List<string> LowerValues
        {
            get;
            set;
        }

        public List<string> UpperValues
        {
            get;
            set;
        }

        public List<string> ResponseNames
        {
            get;
            set;
        }

		public override ExecutableComponent StudiedComponent
		{
			get => base.StudiedComponent;
			set
			{
				if (Treatment != null)
					(Treatment as Treatments.DOE.DesignOfExperiment).Component = value as WorkflowComponent;
				base.StudiedComponent = value;
			}
		}

		public DesignOfExperiment(string name, string description, WorkflowComponent worflow, List<Data> factors, List<Data> responses, string parentName = "")
			: base(name, null, null, description, parentName)
        {
			StudiedComponent = worflow;

			var metadata = new XDocument();

            var parametersElement = new XElement("Parameters");


			ColumnNames.Add("ID");
			ColumnTypes.Add(DataTypes.INTEGER);
			ColumnFormats.Add(0);
			ColumnUnits.Add("");

            for (int i = 0; i < factors.Count; i++)
            {
                var parameterElement = new XElement("Parameter");
                string columnHeader = factors[i].Name;
                parameterElement.Add(new XAttribute("Name", columnHeader));
				ColumnNames.Add(columnHeader);
                if (factors[i] is IntegerData)
                {
					ColumnTypes.Add(DataTypes.INTEGER);
                    parameterElement.Add(new XAttribute("Type", "Integer"));
					ColumnFormats.Add(0);
					ColumnUnits.Add(((IntegerData)factors[i]).Unit);
                    parameterElement.Add(new XAttribute("Unit", ((IntegerData)factors[i]).Unit));
                }
                else if (factors[i] is DoubleData)
                {
					ColumnTypes.Add(DataTypes.DOUBLE);
                    parameterElement.Add(new XAttribute("Type", "Double"));
					ColumnFormats.Add(((DoubleData)factors[i]).DecimalPlaces);
                    parameterElement.Add(new XAttribute("DecimalPlaces", ((DoubleData)factors[i]).DecimalPlaces));
					ColumnUnits.Add(((DoubleData)factors[i]).Unit);
                    parameterElement.Add(new XAttribute("Unit", ((DoubleData)factors[i]).Unit));
                }
                var fullFactorialFactorElement = new XElement("FullFactorialFactor");
                fullFactorialFactorElement.Add(new XAttribute("StartingValue", 0));
                fullFactorialFactorElement.Add(new XAttribute("StepSize", 0));
                fullFactorialFactorElement.Add(new XAttribute("NoOfLevels", 0));
                parameterElement.Add(fullFactorialFactorElement);
                parametersElement.Add(parameterElement);
            }
            for (int i = 0; i < responses.Count; i++)
            {
                var parameterElement = new XElement("Parameter");
                string columnHeader = responses[i].Name;
                parameterElement.Add(new XAttribute("Name", columnHeader));
				ColumnNames.Add(columnHeader);
                if (responses[i] is IntegerData)
                {
					ColumnTypes.Add(DataTypes.INTEGER);
                    parameterElement.Add(new XAttribute("Type", "Integer"));
					ColumnFormats.Add(0);
					ColumnUnits.Add(((IntegerData)responses[i]).Unit);
                    parameterElement.Add(new XAttribute("Unit", ((IntegerData)responses[i]).Unit));
                }
                else if (responses[i] is DoubleData)
                {
					ColumnTypes.Add(DataTypes.DOUBLE);
                    parameterElement.Add(new XAttribute("Type", "Double"));
					ColumnFormats.Add(((DoubleData)responses[i]).DecimalPlaces);
                    parameterElement.Add(new XAttribute("DecimalPlaces", ((DoubleData)responses[i]).DecimalPlaces));
					ColumnUnits.Add(((DoubleData)responses[i]).Unit);
                    parameterElement.Add(new XAttribute("Unit", ((DoubleData)responses[i]).Unit));
                }
                var fullFactorialFactorElement = new XElement("FullFactorialResponse");
                parameterElement.Add(fullFactorialFactorElement);
                parametersElement.Add(parameterElement);
            }

            metadata.Add(parametersElement);

			//metadata.Save(p);















			TableNames.Add(Name);
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
