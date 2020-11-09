using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.IO;
using System.Data;
using System.Data.SqlServerCe;

using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Treatments;

using Aircadia.ObjectModel.Treatments.DOE;

using Aircadia.ObjectModel.Treatments.Optimisers;
using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;
using Aircadia.ObjectModel.Treatments.Optimisers.GA;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.NSGAII;

namespace Aircadia.ObjectModel.Studies
{
    [Serializable()]
    public class DOEplusURQStudy : Study
    {
        List<string> names = new List<string>();

        public Treatment TreatmentDOE
        {
            get;
            set;
        }
        public Treatment TreatmentURQ
        {
            get;
            set;
        }
        public FullFactorial FullFactorialDOE
        {
            get;
            set;
        }


        public RobOptTemplate robOptTemplate;



        //public DOEplusURQStudy(string name, string description, Model worflow, List<Data> factors, List<Data> responses)
        public DOEplusURQStudy(string name, string description, List<string> names, Treatment treatment, FullFactorial fullFactorialDOE, WorkflowComponent worflow)
            : base(name, description)
        {
			TreatmentURQ = treatment;
            this.names = names; //DOE factors
			Treatment = treatment;
			StudiedComponent = worflow;

			FullFactorialDOE = fullFactorialDOE;



            


            var DesignVariables = new List<RobOptDesignVariable>();
            for (int i = 0; i < ((string[,])(Treatment.input_options.setuplist[0])).GetLength(0); i++)
            {
				var designVariable = new RobOptDesignVariable
				{
					Name = ((string[,])(Treatment.input_options.setuplist[0]))[i, 0]
				};
				//designVariable.LowerBound = ;
				//designVariable.UpperBound = ;
				DesignVariables.Add(designVariable);
            }
            var Objectives = new List<RobOptObjective>();
            for (int i = 0; i < ((string[,])(Treatment.input_options.setuplist[1])).GetLength(0); i++)
            {
				var objective = new RobOptObjective
				{
					Name = ((string[,])(Treatment.input_options.setuplist[1]))[i, 0]
				};
				if (((string[,])(Treatment.input_options.setuplist[1]))[i, 2] == "minimise")
                    objective.Type = ObjectiveType.Minimise;
                else if (((string[,])(Treatment.input_options.setuplist[1]))[i, 2] == "maximise")
                    objective.Type = ObjectiveType.Maximise;
                Objectives.Add(objective);
            }
            var Constraints = new List<RobOptConstraint>();
            for (int i = 0; i < ((string[,])(Treatment.input_options.setuplist[2])).GetLength(0); i++)
            {
				var constraint = new RobOptConstraint
				{
					Name = ((string[,])(Treatment.input_options.setuplist[2]))[i, 0]
				};
				if (((string[,])(Treatment.input_options.setuplist[2]))[i, 2] == "<=")
                    constraint.Type = ConstraintType.LessThanOrEqual;
                else if (((string[,])(Treatment.input_options.setuplist[2]))[i, 2] == ">=")
                    constraint.Type = ConstraintType.GreatorThanOrEqual;
                constraint.Value = Convert.ToDouble(((string[,])(Treatment.input_options.setuplist[2]))[i, 3]);
                Constraints.Add(constraint);
            }
			robOptTemplate = new RobOptTemplate() { DesignVariables = DesignVariables, Objectives = Objectives, Constraints = Constraints };







			ColumnNames.Add("ID");
			ColumnTypes.Add(DataTypes.INTEGER);
			ColumnFormats.Add(0);
			ColumnUnits.Add("");

            for (int i = 0; i < this.names.Count; i++)
            {
                string columnHeader = this.names[i];
				ColumnNames.Add(columnHeader);
				ColumnTypes.Add(DataTypes.DOUBLE);
				ColumnFormats.Add(4);
				ColumnUnits.Add("");
            }


            for (int i = 0; i < robOptTemplate.DesignVariables.Count(); i++)
            {
                if (!this.names.Contains(robOptTemplate.DesignVariables[i].Name))
                {
                    string columnHeader = robOptTemplate.DesignVariables[i].Name;
					ColumnNames.Add(columnHeader);
					ColumnTypes.Add(DataTypes.DOUBLE);
					ColumnFormats.Add(4);
					ColumnUnits.Add("");
                }
            }
            for (int i = 0; i < robOptTemplate.Objectives.Count(); i++)
            {
                string columnHeader = robOptTemplate.Objectives[i].Name;

				// Loss Function
				ColumnNames.Add(columnHeader + "LF");
				ColumnTypes.Add(DataTypes.DOUBLE);
				ColumnFormats.Add(4);
				ColumnUnits.Add("");
				// Mean
				ColumnNames.Add(columnHeader + "mean");
				ColumnTypes.Add(DataTypes.DOUBLE);
				ColumnFormats.Add(4);
				ColumnUnits.Add("");
				// Variance
				ColumnNames.Add(columnHeader + "var");
				ColumnTypes.Add(DataTypes.DOUBLE);
				ColumnFormats.Add(4);
				ColumnUnits.Add("");
            }
            for (int i = 0; i < robOptTemplate.Constraints.Count(); i++)
            {
                string columnHeader = robOptTemplate.Constraints[i].Name;

				// Loss Function
				ColumnNames.Add(columnHeader + "LF");
				ColumnTypes.Add(DataTypes.DOUBLE);
				ColumnFormats.Add(4);
				ColumnUnits.Add("");
				// Mean
				ColumnNames.Add(columnHeader + "mean");
				ColumnTypes.Add(DataTypes.DOUBLE);
				ColumnFormats.Add(4);
				ColumnUnits.Add("");
				// Variance
				ColumnNames.Add(columnHeader + "var");
				ColumnTypes.Add(DataTypes.DOUBLE);
				ColumnFormats.Add(4);
				ColumnUnits.Add("");
            }






			TableNames.Add("DOE");
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







            string databaseFileName = Path.Combine(Project.ProjectPath, "Studies\\" + Name, Name + ".sdf"); // Microsoft SQL server compact edition file

            // Create database for optimisation results
            string connectionString;
            connectionString = String.Format("Data Source = " + databaseFileName + ";Persist Security Info=False");


            // Create tables
            var connection = new SqlCeConnection(connectionString);
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }









            #region Atif
            // Create SQL statements to create tables, sqlStatement1 for OptimAllResults and sqlStatement2 for OptimGenResults
            //            this.Result.TableNames.Add("RobOptimAllResults");
            //            this.Result.TableNames.Add("RobOptimGenResults");
            string sqlStatement1 = "create table " + Name + " (";
            sqlStatement1 += "ID int, ";



            for (int i = 0; i < names.Count; i++)
            {
                sqlStatement1 += names[i] + " ";

                sqlStatement1 += "float, ";
            }


            for (int i = 0; i < robOptTemplate.DesignVariables.Count; i++)
            {
                if (!names.Contains(robOptTemplate.DesignVariables[i].Name))
                {
                    string columnHeader = robOptTemplate.DesignVariables[i].Name;
                    sqlStatement1 += columnHeader + " ";

                    sqlStatement1 += "float, ";
                }
            }


            //for (int i = 0; i < this.robOptTemplate.DesignVariables.Count(); i++)
            //{
            //    string columnHeader = this.robOptTemplate.DesignVariables[i].Name;
            //    sqlStatement1 += columnHeader + " ";
            //    sqlStatement2 += columnHeader + " ";
            //    /*
            //    if (this.robOptTemplate.DesignVariables[i].WrappedData is IntegerData)
            //    {
            //        sqlStatement1 += "int, ";
            //        sqlStatement2 += "int, ";
            //    }
            //    else if (this.robOptTemplate.DesignVariables[i].WrappedData is DoubleData)
            //    {
            //    */
            //    sqlStatement1 += "float, ";
            //    sqlStatement2 += "float, ";
            //    /*
            //    }
            //    */
            //}
            for (int i = 0; i < robOptTemplate.Objectives.Count(); i++)
            {
                string columnHeader = robOptTemplate.Objectives[i].Name;

                // Loss Function
                sqlStatement1 += columnHeader + "LF" + " ";
                /*
                if ((this.robOptTemplate.Objectives[i].WrappedData) is IntegerData)
                {
                    sqlStatement1 += "int, ";
                    sqlStatement2 += "int, ";
                }
                else if (this.robOptTemplate.Objectives[i].WrappedData is DoubleData)
                {
                */
                sqlStatement1 += "float, ";
                /*
                }
                */

                // Mean
                sqlStatement1 += columnHeader + "mean" + " ";
                /*
                if ((this.robOptTemplate.Objectives[i].WrappedData) is IntegerData)
                {
                    sqlStatement1 += "int, ";
                    sqlStatement2 += "int, ";
                }
                else if (this.robOptTemplate.Objectives[i].WrappedData is DoubleData)
                {
                */
                sqlStatement1 += "float, ";
                /*
                }
                */

                // Variance
                sqlStatement1 += columnHeader + "var" + " ";
                /*
                if ((this.robOptTemplate.Objectives[i].WrappedData) is IntegerData)
                {
                    sqlStatement1 += "int, ";
                    sqlStatement2 += "int, ";
                }
                else if (this.robOptTemplate.Objectives[i].WrappedData is DoubleData)
                {
                */
                sqlStatement1 += "float, ";
                /*
                }
                */
            }
            for (int i = 0; i < robOptTemplate.Constraints.Count(); i++)
            {
                string columnHeader = robOptTemplate.Constraints[i].Name;

                // Loss Function
                sqlStatement1 += columnHeader + "LF" + " ";
                /*
                if ((this.robOptTemplate.Constraints[i].WrappedData) is IntegerData)
                {
                    sqlStatement1 += "int, ";
                    sqlStatement2 += "int, ";
                }
                else if (this.robOptTemplate.Constraints[i].WrappedData is DoubleData)
                {
                */
                sqlStatement1 += "float, ";
                /*
                }
                */

                // Mean
                sqlStatement1 += columnHeader + "mean" + " ";
                /*
                if ((this.robOptTemplate.Constraints[i].WrappedData) is IntegerData)
                {
                    sqlStatement1 += "int, ";
                    sqlStatement2 += "int, ";
                }
                else if (this.robOptTemplate.Constraints[i].WrappedData is DoubleData)
                {
                */
                sqlStatement1 += "float, ";
                /*
                }
                */

                // Variance
                sqlStatement1 += columnHeader + "var" + " ";
                /*
                if ((this.robOptTemplate.Constraints[i].WrappedData) is IntegerData)
                {
                    sqlStatement1 += "int, ";
                    sqlStatement2 += "int, ";
                }
                else if (this.robOptTemplate.Constraints[i].WrappedData is DoubleData)
                {
                */
                sqlStatement1 += "float, ";
                /*
                }
                */
            }
            if (robOptTemplate.DesignVariables.Count() + robOptTemplate.Objectives.Count() + robOptTemplate.Constraints.Count() > 0)
            {
                sqlStatement1 = sqlStatement1.Remove(sqlStatement1.Length - 2);
            }
            sqlStatement1 += ")";

            // Create SQL statement command for "SQL Server Compact Edition"
            var command1 = new SqlCeCommand(sqlStatement1, connection);
            // Execute the SQL command
            try
            {
                command1.ExecuteNonQuery();
            }
            catch (SqlCeException sqlexception)
            {
                Console.WriteLine(sqlexception.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
			#endregion Atif


















			Treatment.Result = ActiveResult;

            status = Treatment.ApplyOn(StudiedComponent);



            if (Treatment.Name == "ROP" && status == true) //This is to store the results of Robust Analysis studies(RA) so that they can be visualised on the grid of the fExecuter form
            {
                //Storing variables and targets labels:
                string[,] ROPvars = Treatment.input_options.setuplist[0] as string[,];
                string[,] ROPtargets = Treatment.input_options.setuplist[1] as string[,];
                int NoROPvars = ROPvars.GetLength(0);
                int NoROPtargets = ROPtargets.GetLength(0);
                string[] ROPvarsNames = new string[NoROPvars];
                string[] ROPtargetsNames = new string[NoROPtargets];
                string OutputString = "";
                for (int i = 0; i < NoROPvars; i++)
                {
                    OutputString = OutputString + ROPvars[i, 0] + "(var)\t";
                }
                for (int i = 0; i < NoROPtargets - 1; i++)
                {
                    OutputString = OutputString + ROPtargets[i, 0] + "(LossFunc)\t";
                    OutputString = OutputString + ROPtargets[i, 0] + "(Mean)\t";
                    OutputString = OutputString + ROPtargets[i, 0] + "(Variance)\t";
                }
                OutputString = OutputString + ROPtargets[NoROPtargets - 1, 0] + "(LossFunc)\t";
                OutputString = OutputString + ROPtargets[NoROPtargets - 1, 0] + "(Mean)\t";
                OutputString = OutputString + ROPtargets[NoROPtargets - 1, 0] + "(Variance)\r\n";

                //Storing variables and targets values:
                //for (int i = 0; i < NoROPvars + (3 * NoROPtargets) - 1; i++)
                //{
                //    OutputString = OutputString + (Treatment as URQTreatmentOld).RobustData[0, i] + "\t";
                //}
                //OutputString = OutputString + (Treatment as URQTreatmentOld).RobustData[0, NoROPvars + 3 * NoROPtargets - 1] + "\t";


                Treatment.output_struct.output = OutputString;
            }




            if (status == true)
            {

                string CurrentDir = System.IO.Directory.GetCurrentDirectory();
                string FileName = Name;
                if (File.Exists(CurrentDir + "\\" + FileName + ".acdstd"))
                {
                    FileName = Name + "_" + DateTime.Today.Day + "-" + DateTime.Today.Month + "-" + DateTime.Today.Year + "_" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + "-" + DateTime.Now.Second;
                }
                if (File.Exists(Project.ProjectPath + "\\FuncEvalsH.txt"))
                {
                    File.Copy(Project.ProjectPath + "\\FuncEvalsH.txt", CurrentDir + "\\" + FileName + ".acdstd", false);
                }

                string output = File.ReadAllText(Project.ProjectPath + "\\FuncEvalsRob.txt");
                var filer = new FileStream(CurrentDir + "\\" + FileName + ".acdstd", FileMode.Append, FileAccess.Write);
                var evals = new StreamWriter(filer);
                evals.Write(output);
                evals.Close();
                filer.Close();


                string VisualisationCase = ((this as Study) as Study).Treatment.Name;
                if (VisualisationCase == "TradeStudy" || VisualisationCase == "rDOE" || Treatment is Aircadia.ObjectModel.Treatments.DOE.DesignOfExperiment)
                {
                    string output1 = File.ReadAllText("evaluatedSolutions.txt");
                    var filer1 = new FileStream(CurrentDir + "\\" + FileName + ".acdstd", FileMode.Append, FileAccess.Write);
                    var evals1 = new StreamWriter(filer1);
                    evals1.Write(output1);
                    evals1.Close();
                    filer1.Close();
                }

            }



            return status;
        }
    }
}
