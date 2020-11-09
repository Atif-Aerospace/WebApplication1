using System;
using System.Linq;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Treatments;
using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;
using Aircadia.ObjectModel.Treatments.Optimisers.GA;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.NSGAII;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.SGA;

namespace Aircadia.ObjectModel.Studies
{
	[Serializable]
    public class DetOptStudy : Study
    {
        public override string StudyType => "DeterministicOptimisation";

        public OptimisationTemplate OptimisationTemplate;
        public GAParameters gaParameters;



        string sqlStatement1;
        string sqlStatement2;


        public DetOptStudy(string name, string description, string parentName = "")
            : base(name, null, null, description, parentName)
        {



            // Create SQL statements to create tables, sqlStatement1 for OptimAllResults and sqlStatement2 for OptimGenResults
            sqlStatement1 = "create table OptimAllResults (";
            sqlStatement2 = "create table OptimGenResults (";
            sqlStatement1 += "ID int, ";
            sqlStatement2 += "ID int, Category int, ";

            #region Atif
            // Prepare results database
            foreach (var designVariable in OptimisationTemplate.DesignVariables)
            {
                string columnHeader = designVariable.Name;
				ColumnNames.Add(columnHeader);
                sqlStatement1 += columnHeader + " ";
                sqlStatement2 += columnHeader + " ";
                if (designVariable.Data is IntegerData)
                {
					ColumnTypes.Add(DataTypes.INTEGER);
					ColumnFormats.Add(0);
                    sqlStatement1 += "int, ";
                    sqlStatement2 += "int, ";
                }
                else if (designVariable.Data is DoubleData)
                {
					ColumnTypes.Add(DataTypes.DOUBLE);
					ColumnFormats.Add(((DoubleData)(designVariable.Data)).DecimalPlaces);
                    sqlStatement1 += "float, ";
                    sqlStatement2 += "float, ";
                }
				ColumnUnits.Add(designVariable.Data.Unit);
				MinValues.Add(designVariable.LowerBound);
				MaxValues.Add(designVariable.UpperBound);
            }
            foreach (Objective objective in OptimisationTemplate.Objectives)
            {
                string columnHeader = objective.Name;
				ColumnNames.Add(columnHeader);
                sqlStatement1 += columnHeader + " ";
                sqlStatement2 += columnHeader + " ";
                if (objective.Data is IntegerData)
                {
					ColumnTypes.Add(DataTypes.INTEGER);
					ColumnFormats.Add(0);
                    sqlStatement1 += "int, ";
                    sqlStatement2 += "int, ";
                }
                else if (objective.Data is DoubleData)
                {
					ColumnTypes.Add(DataTypes.DOUBLE);
					ColumnFormats.Add(((DoubleData)(objective.Data)).DecimalPlaces);
                    sqlStatement1 += "float, ";
                    sqlStatement2 += "float, ";
                }
				ColumnUnits.Add(objective.Data.Unit);
				MinValues.Add(Double.NaN);
				MaxValues.Add(Double.NaN);
            }
            foreach (Aircadia.ObjectModel.Treatments.Optimisers.Formulation.Constraint constraint in OptimisationTemplate.Constraints)
            {
                string columnHeader = constraint.Name;
				ColumnNames.Add(columnHeader);
                sqlStatement1 += columnHeader + " ";
                sqlStatement2 += columnHeader + " ";
                if (constraint.Data is IntegerData)
                {
					ColumnTypes.Add(DataTypes.INTEGER);
					ColumnFormats.Add(0);
                    sqlStatement1 += "int, ";
                    sqlStatement2 += "int, ";
                }
                else if (constraint.Data is DoubleData)
                {
					ColumnTypes.Add(DataTypes.DOUBLE);
					ColumnFormats.Add(((DoubleData)(constraint.Data)).DecimalPlaces);
                    sqlStatement1 += "float, ";
                    sqlStatement2 += "float, ";
                }
				ColumnUnits.Add(constraint.Data.Unit);
				MinValues.Add(Double.NaN);
				MaxValues.Add(Double.NaN);

            }

			TableNames.Add("OptimAllResults");
			TableNames.Add("OptimGenResults");

            if (OptimisationTemplate.DesignVariables.Count() + OptimisationTemplate.Objectives.Count() + OptimisationTemplate.Constraints.Count() > 0)
            {
                sqlStatement1 = sqlStatement1.Remove(sqlStatement1.Length - 2);
                sqlStatement2 = sqlStatement2.Remove(sqlStatement2.Length - 2);
            }
            sqlStatement1 += ")";
            sqlStatement2 += ")";

            
            #endregion Atif
            
        }

        public DetOptStudy(string name, string description, ExecutableComponent studiedComponent, OptimisationTemplate optimisationTemplate, Treatment treatment, string parentName = "")
            : base(name, null, null, description, parentName)
        {
            OptimisationTemplate = optimisationTemplate;
			base.Treatment = treatment;
			StudiedComponent = studiedComponent;

            // Create SQL statements to create tables, sqlStatement1 for OptimAllResults and sqlStatement2 for OptimGenResults
            sqlStatement1 = "create table OptimAllResults (";
            sqlStatement2 = "create table OptimGenResults (";
            sqlStatement1 += "ID int, ";
            sqlStatement2 += "ID int, Category int, ";

            #region Atif
            // Prepare results database
            foreach (BoundedDesignVariableNoInital designVariable in this.OptimisationTemplate.DesignVariables)
            {
                string columnHeader = designVariable.Name;
				ColumnNames.Add(columnHeader);
                sqlStatement1 += columnHeader + " ";
                sqlStatement2 += columnHeader + " ";
                if (designVariable.Data is IntegerData)
                {
					ColumnTypes.Add(DataTypes.INTEGER);
					ColumnFormats.Add(0);
                    sqlStatement1 += "int, ";
                    sqlStatement2 += "int, ";
                }
                else if (designVariable.Data is DoubleData)
                {
					ColumnTypes.Add(DataTypes.DOUBLE);
					ColumnFormats.Add(((DoubleData)(designVariable.Data)).DecimalPlaces);
                    sqlStatement1 += "float, ";
                    sqlStatement2 += "float, ";
                }
				ColumnUnits.Add(designVariable.Data.Unit);
				MinValues.Add(designVariable.LowerBound);
				MaxValues.Add(designVariable.UpperBound);
            }
            foreach (Objective objective in this.OptimisationTemplate.Objectives)
            {
                string columnHeader = objective.Name;
				ColumnNames.Add(columnHeader);
                sqlStatement1 += columnHeader + " ";
                sqlStatement2 += columnHeader + " ";
                if (objective.Data is IntegerData)
                {
					ColumnTypes.Add(DataTypes.INTEGER);
					ColumnFormats.Add(0);
                    sqlStatement1 += "int, ";
                    sqlStatement2 += "int, ";
                }
                else if (objective.Data is DoubleData)
                {
					ColumnTypes.Add(DataTypes.DOUBLE);
					ColumnFormats.Add(((DoubleData)(objective.Data)).DecimalPlaces);
                    sqlStatement1 += "float, ";
                    sqlStatement2 += "float, ";
                }
				ColumnUnits.Add(objective.Data.Unit);
				MinValues.Add(Double.NaN);
				MaxValues.Add(Double.NaN);
            }
            foreach (Aircadia.ObjectModel.Treatments.Optimisers.Formulation.Constraint constraint in this.OptimisationTemplate.Constraints)
            {
                string columnHeader = constraint.Name;
				ColumnNames.Add(columnHeader);
                sqlStatement1 += columnHeader + " ";
                sqlStatement2 += columnHeader + " ";
                if (constraint.Data is IntegerData)
                {
					ColumnTypes.Add(DataTypes.INTEGER);
					ColumnFormats.Add(0);
                    sqlStatement1 += "int, ";
                    sqlStatement2 += "int, ";
                }
                else if (constraint.Data is DoubleData)
                {
					ColumnTypes.Add(DataTypes.DOUBLE);
					ColumnFormats.Add(((DoubleData)(constraint.Data)).DecimalPlaces);
                    sqlStatement1 += "float, ";
                    sqlStatement2 += "float, ";
                }
				ColumnUnits.Add(constraint.Data.Unit);
				MinValues.Add(Double.NaN);
				MaxValues.Add(Double.NaN);

            }

			TableNames.Add("OptimAllResults");
			TableNames.Add("OptimGenResults");

            if (this.OptimisationTemplate.DesignVariables.Count() + this.OptimisationTemplate.Objectives.Count() + this.OptimisationTemplate.Constraints.Count() > 0)
            {
                sqlStatement1 = sqlStatement1.Remove(sqlStatement1.Length - 2);
                sqlStatement2 = sqlStatement2.Remove(sqlStatement2.Length - 2);
            }
            sqlStatement1 += ")";
            sqlStatement2 += ")";


            #endregion Atif

            Treatment.CreateFolder();

        }

		//public Treatment Treatment { get; }

		public override bool Execute()
        {
            bool status = true;








            /*
            #region Atif
            string databaseFileName = Path.Combine(AircadiaProject.ProjectPath, "Studies\\" + this.Name + "\\" + this.Name + ".sdf"); // Microsoft SQL server compact edition file

            // Create database for optimisation results
            string connectionString;

            connectionString = string.Format("Data Source = " + databaseFileName + ";Persist Security Info=False");
            SqlCeEngine engine = new SqlCeEngine(connectionString);

            // Create tables
            SqlCeConnection connection = new SqlCeConnection(connectionString);
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }



            

            // Create SQL statement command for "SQL Server Compact Edition"
            SqlCeCommand command1 = new SqlCeCommand(sqlStatement1, connection);
            SqlCeCommand command2 = new SqlCeCommand(sqlStatement2, connection);
            // Execute the SQL command
            try
            {
                command1.ExecuteNonQuery();
                command2.ExecuteNonQuery();
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
            */







            
            if (Treatment is NSGAIIOptimiser opt)
            {
                //throw new NotImplementedException("NSGAII or SGA  used a GUI to define some parameters, removing this dependencies needs to be implemented");

                //opt.GAParameters.SelectionOprMethod = Treatments.Optimisers.GA.Selection.SelectionOprMethods.TournamentSelectionWithoutReplacement;
                //opt.GAParameters.CrossoverOprMethod = Treatments.Optimisers.GA.Crossover.CrossoverOprMethods.UniformCrossover;
                //opt.GAParameters.MutationOprMethod = Treatments.Optimisers.GA.Mutation.MutationOprMethods.PolynomialMutation;
                //opt.GAParameters.EvaluatedSolutionsFile = Path.Combine(AircadiaProject.ProjectPath, "evaluatedSolutions.txt");

                // UI Code
                //    NoOfGenerations = (int)numericUpDownNoOfGenerations.Value;
                //GAParameters.NoOfGenerations = NoOfGenerations;

                //PopulationSize = (int)numericUpDownPopulationSize.Value;
                //GAParameters.PopulationSize = PopulationSize;

                //if ((comboBoxSelectionOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Roulette Wheel")
                //    GAParameters.SelectionOprMethod = SelectionOprMethods.RouletteWheelSelection;
                //else if ((comboBoxSelectionOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Tournament Without Replacement")
                //    GAParameters.SelectionOprMethod = SelectionOprMethods.TournamentSelectionWithoutReplacement;
                //else if ((comboBoxSelectionOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Tournament With Replacement")
                //    GAParameters.SelectionOprMethod = SelectionOprMethods.TournamentSelectionWithReplacement;

                //if ((comboBoxCrossoverOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "One-Point")
                //    GAParameters.CrossoverOprMethod = CrossoverOprMethods.OnePointCrossover;
                //else if ((comboBoxCrossoverOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Two-Point")
                //    GAParameters.CrossoverOprMethod = CrossoverOprMethods.TwoPointCrossover;
                //else if ((comboBoxCrossoverOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Uniform")
                //    GAParameters.CrossoverOprMethod = CrossoverOprMethods.UniformCrossover;
                //else if ((comboBoxCrossoverOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Simulated Binary")
                //    GAParameters.CrossoverOprMethod = CrossoverOprMethods.SimulatedBinaryCrossover;

                //if ((comboBoxMutationOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Genewise")
                //    GAParameters.MutationOprMethod = MutationOprMethods.GenewiseMutation;
                //else if ((comboBoxMutationOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Selective")
                //    GAParameters.MutationOprMethod = MutationOprMethods.SelectiveMutation;
                //else if ((comboBoxMutationOpr.SelectedItem as RadComboBoxItem).Content.ToString() == "Polynomial")
                //    GAParameters.MutationOprMethod = MutationOprMethods.PolynomialMutation;

                // GA setup
                GASetup gaSetup = new GASetup(this.OptimisationTemplate);
                if (gaSetup.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    GAParameters gaParameters = gaSetup.GAParameters;
                    //gaParameters.EvaluatedSolutionsFile = Path.Combine(AircadiaProject.ProjectPath, "evaluatedSolutions.txt");
                    ((NSGAIIOptimiser)(Treatment)).GAParameters = gaParameters;
                }
            }
            else if (Treatment is SGAOptimizer)
            {
                throw new NotImplementedException("NSGAII or SGA  used a GUI to define some parameters, removing this dependencies needs to be implemented");
            }

            Treatment.Result = ActiveResult;

            status = Treatment.ApplyOn();

            return status;
        }

		public override Study Copy(string id, string name = null, string parentName = null)
		{
			return new DetOptStudy(id, Description, StudiedComponent, OptimisationTemplate, Treatment, parentName ?? Name);
		}


	}
}
