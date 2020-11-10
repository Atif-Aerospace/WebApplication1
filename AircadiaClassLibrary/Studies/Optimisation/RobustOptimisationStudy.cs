using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Treatments;
using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;
using Aircadia.ObjectModel.Treatments.Optimisers.GA;



namespace Aircadia.ObjectModel.Studies
{
	[Serializable]
    public class RobustOptimisationStudy : Study
    {
        public RobOptTemplate robOptTemplate;
        public GAParameters gaParameters;

		public OptimisationTemplate Template { get; }

		public RobustOptimisationStudy(string name, Treatment treatment, ExecutableComponent ec, OptimisationTemplate template, string parentName = "")
			: base(name, null, null, name, parentName)
		{
			Treatment = treatment;
			StudiedComponent = ec;


			var DesignVariables = template.DesignVariables.Select(v => new RobOptDesignVariable { Name = v.Name }).ToList();
			var Objectives = template.Objectives.Select(o => new RobOptObjective { Name = o.Name, Type = o.Type }).ToList();
			var Constraints = template.Constraints.Select(c => new RobOptConstraint { Name = c.Name, Type = c.Type, Value = c.Value }).ToList();
			
			robOptTemplate = new RobOptTemplate() { DesignVariables = DesignVariables, Objectives = Objectives, Constraints = Constraints };


			ColumnNames.Add("ID");
			ColumnTypes.Add(DataTypes.INTEGER);
			ColumnFormats.Add(0);
			ColumnUnits.Add("");

			for (int i = 0; i < robOptTemplate.DesignVariables.Count(); i++)
			{
				string columnHeader = robOptTemplate.DesignVariables[i].Name;
				ColumnNames.Add(columnHeader);
				ColumnTypes.Add(DataTypes.DOUBLE);
				ColumnFormats.Add(4);
				ColumnUnits.Add("");
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


			TableNames.Add("RobOptimAllResults");
			TableNames.Add("RobOptimGenResults");
			Template = template;
		}

		public override bool Execute()
		{
			bool status = true;

			////((StudiedComponent as Study).Treatment as Treatment_ROP).Counter = 1;


			//string databaseFileName = Path.Combine(Project.ProjectPath, "Studies\\" + Name, Name + ".sdf"); // Microsoft SQL server compact edition file

			//// Create database for optimisation results
			//string connectionString;
			//connectionString = String.Format("Data Source = " + databaseFileName + ";Persist Security Info=False");


			//// Create tables
			//var connection = new SqlCeConnection(connectionString);
			//if (connection.State == ConnectionState.Closed)
			//{
			//	connection.Open();
			//}

			//#region Atif
			//// Create SQL statements to create tables, sqlStatement1 for OptimAllResults and sqlStatement2 for OptimGenResults
			////            this.Result.TableNames.Add("RobOptimAllResults");
			////            this.Result.TableNames.Add("RobOptimGenResults");
			//string sqlStatement1 = "create table RobOptimAllResults (";
			//string sqlStatement2 = "create table RobOptimGenResults (";
			//sqlStatement1 += "ID int, ";
            
			//sqlStatement2 += "ID int, Category int, ";
			//for (int i = 0; i < robOptTemplate.DesignVariables.Count(); i++)
			//{
			//	string columnHeader = robOptTemplate.DesignVariables[i].Name;
			//	sqlStatement1 += columnHeader + " ";
			//	sqlStatement2 += columnHeader + " ";
			//	/*
			//	if (this.robOptTemplate.DesignVariables[i].WrappedData is IntegerData)
			//	{
			//		sqlStatement1 += "int, ";
			//		sqlStatement2 += "int, ";
			//	}
			//	else if (this.robOptTemplate.DesignVariables[i].WrappedData is DoubleData)
			//	{
			//	*/
			//	sqlStatement1 += "float, ";
			//	sqlStatement2 += "float, ";
			//	/*
			//	}
			//	*/
			//}
			//for (int i = 0; i < robOptTemplate.Objectives.Count(); i++)
			//{
			//	string columnHeader = robOptTemplate.Objectives[i].Name;
                
			//	// Loss Function
			//	sqlStatement1 += columnHeader + "LF" + " ";
			//	sqlStatement2 += columnHeader + "LF" + " ";
			//	/*
			//	if ((this.robOptTemplate.Objectives[i].WrappedData) is IntegerData)
			//	{
			//		sqlStatement1 += "int, ";
			//		sqlStatement2 += "int, ";
			//	}
			//	else if (this.robOptTemplate.Objectives[i].WrappedData is DoubleData)
			//	{
			//	*/
			//	sqlStatement1 += "float, ";
			//	sqlStatement2 += "float, ";
			//	/*
			//	}
			//	*/

			//	// Mean
			//	sqlStatement1 += columnHeader + "mean" + " ";
			//	sqlStatement2 += columnHeader + "mean" + " ";
			//	/*
			//	if ((this.robOptTemplate.Objectives[i].WrappedData) is IntegerData)
			//	{
			//		sqlStatement1 += "int, ";
			//		sqlStatement2 += "int, ";
			//	}
			//	else if (this.robOptTemplate.Objectives[i].WrappedData is DoubleData)
			//	{
			//	*/
			//	sqlStatement1 += "float, ";
			//	sqlStatement2 += "float, ";
			//	/*
			//	}
			//	*/

			//	// Variance
			//	sqlStatement1 += columnHeader + "var" + " ";
			//	sqlStatement2 += columnHeader + "var" + " ";
			//	/*
			//	if ((this.robOptTemplate.Objectives[i].WrappedData) is IntegerData)
			//	{
			//		sqlStatement1 += "int, ";
			//		sqlStatement2 += "int, ";
			//	}
			//	else if (this.robOptTemplate.Objectives[i].WrappedData is DoubleData)
			//	{
			//	*/
			//	sqlStatement1 += "float, ";
			//	sqlStatement2 += "float, ";
			//	/*
			//	}
			//	*/
			//}
			//for (int i = 0; i < robOptTemplate.Constraints.Count(); i++)
			//{
			//	string columnHeader = robOptTemplate.Constraints[i].Name;

			//	// Loss Function
			//	sqlStatement1 += columnHeader + "LF" + " ";
			//	sqlStatement2 += columnHeader + "LF" + " ";
			//	/*
			//	if ((this.robOptTemplate.Constraints[i].WrappedData) is IntegerData)
			//	{
			//		sqlStatement1 += "int, ";
			//		sqlStatement2 += "int, ";
			//	}
			//	else if (this.robOptTemplate.Constraints[i].WrappedData is DoubleData)
			//	{
			//	*/
			//	sqlStatement1 += "float, ";
			//	sqlStatement2 += "float, ";
			//	/*
			//	}
			//	*/

			//	// Mean
			//	sqlStatement1 += columnHeader + "mean" + " ";
			//	sqlStatement2 += columnHeader + "mean" + " ";
			//	/*
			//	if ((this.robOptTemplate.Constraints[i].WrappedData) is IntegerData)
			//	{
			//		sqlStatement1 += "int, ";
			//		sqlStatement2 += "int, ";
			//	}
			//	else if (this.robOptTemplate.Constraints[i].WrappedData is DoubleData)
			//	{
			//	*/
			//	sqlStatement1 += "float, ";
			//	sqlStatement2 += "float, ";
			//	/*
			//	}
			//	*/

			//	// Variance
			//	sqlStatement1 += columnHeader + "var" + " ";
			//	sqlStatement2 += columnHeader + "var" + " ";
			//	/*
			//	if ((this.robOptTemplate.Constraints[i].WrappedData) is IntegerData)
			//	{
			//		sqlStatement1 += "int, ";
			//		sqlStatement2 += "int, ";
			//	}
			//	else if (this.robOptTemplate.Constraints[i].WrappedData is DoubleData)
			//	{
			//	*/
			//	sqlStatement1 += "float, ";
			//	sqlStatement2 += "float, ";
			//	/*
			//	}
			//	*/
			//}
			//if (robOptTemplate.DesignVariables.Count() + robOptTemplate.Objectives.Count() + robOptTemplate.Constraints.Count() > 0)
			//{
			//	sqlStatement1 = sqlStatement1.Remove(sqlStatement1.Length - 2);
			//	sqlStatement2 = sqlStatement2.Remove(sqlStatement2.Length - 2);
			//}
			//sqlStatement1 += ")";
			//sqlStatement2 += ")";

			//// Create SQL statement command for "SQL Server Compact Edition"
			//var command1 = new SqlCeCommand(sqlStatement1, connection);
			//var command2 = new SqlCeCommand(sqlStatement2, connection);
			//// Execute the SQL command
			//try
			//{
			//	command1.ExecuteNonQuery();
			//	command2.ExecuteNonQuery();
			//}
			//catch (SqlCeException sqlexception)
			//{
			//	Console.WriteLine(sqlexception.Message);
			//}
			//catch (Exception ex)
			//{
			//	Console.WriteLine(ex.Message);
			//}
			//#endregion Atif


			//Treatment.Result = ActiveResult;

			////status = this.Treatment.ApplyOn();
			//status = Treatment.ApplyOn(StudiedComponent);


			//#region Post Execution

			///*
			//((cTreatment_ROP)((Study)(this.modsub)).Treatment).Counter = 1;




			//// Drop tables generated by optimiser
			//string sqlText = "DROP TABLE " + "RobOptimAllResults";
			//SqlCeCommand cmd = new SqlCeCommand(sqlText, connection);
			//cmd.ExecuteNonQuery();
			//sqlText = "DROP TABLE " + "RobOptimGenResults";
			//cmd = new SqlCeCommand(sqlText, connection);
			//cmd.ExecuteNonQuery();
			//connection.Close();
			//*/



			//#endregion Post Execution

			return status;
		}

		public override Study Copy(string id, string name = null, string parentName = null)
		{
			return new RobustOptimisationStudy(id, Treatment, StudiedComponent, Template, parentName ?? Name);
		}
	}



    [Serializable()]
    public class RobOptTemplate
    {
        public List<RobOptDesignVariable> DesignVariables = new List<RobOptDesignVariable>();
        public List<RobOptObjective> Objectives = new List<RobOptObjective>();
        public List<RobOptConstraint> Constraints = new List<RobOptConstraint>();
    }
    [Serializable()]
    public class RobOptDesignVariable
    {
        public string Name;
        public Data WrappedData;
    }
    [Serializable()]
    public class RobOptObjective
    {
        public string Name;
        public Data WrappedData;
        public ObjectiveType Type;
    }
    [Serializable()]
    public class RobOptConstraint
    {
        public string Name;
        public Data WrappedData;
        public ConstraintType Type;
        public double Value;
    }
}
