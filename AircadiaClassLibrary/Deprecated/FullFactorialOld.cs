using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Workflows;



using System.IO;
using System.Data;
using System.Data.SqlServerCe;


using System.Xml.Linq;

namespace Aircadia.ObjectModel.Treatments.DOE
{
	public class FullFactorialOld : DesignOfExperiment
	{

		bool csvCreation = true;
		readonly bool sdfCreation = false;

		public List<Data> factors; // List of factors
		public int noOfFactors;
		public List<int> noOfLevels; // List of corresponding number of levels for factors
		public List<decimal> startingValues;
		public List<decimal> stepSizes;
		public List<Data> responses; // List of responses

		private List<double> responsesMinValues = new List<double>();
		private List<double> responsesMaxValues = new List<double>();

		public decimal[][] arr; // factors array



		// Takes starting values and step size with number of factors
		public FullFactorialOld(string name, string description, string databaseFileName, Workflow workflow, List<Data> factors, List<decimal> startingValues, List<decimal> stepSizes, List<int> noOfLevels, List<Data> responses)
			: base(name, description)
		{
			this.Component = workflow;
			this.factors = factors;
			noOfFactors = factors.Count;
			this.startingValues = startingValues;
			this.stepSizes = stepSizes;

			arr = new decimal[factors.Count][];
			this.noOfLevels = noOfLevels;
			for (int i = 0; i < factors.Count; i++)
				arr[i] = new decimal[noOfLevels[i]];
			for (int i = 0; i < factors.Count; i++)
				arr[i][0] = startingValues[i];
			for (int i = 0; i < factors.Count; i++)
			{
				for (int j = 1; j < noOfLevels[i]; j++)
					arr[i][j] = arr[i][j - 1] + this.stepSizes[i];
			}

			this.responses = responses;


			#region Version 1.0
			var tradestudy = new ArrayList();
			for (int i = 0; i < factors.Count; i++)
			{
				var options = new TS_Input_set("Data", factors[i].Name, "min", (double)startingValues[i], "max", (double)(startingValues[i] * noOfLevels[i]), "Increment", (double)noOfLevels[i]);
				tradestudy.Add(options);
			}
			var treatmentTSInput = new Treatment_InOut_TS_Input(tradestudy);
			//this.input_options = ;
			//this.output_struct = ;
			#endregion Version 1.0
		}

		// Takes discrete values
		public FullFactorialOld(string name, string description, string databaseFileName, Workflow workflow, List<Data> factors, List<List<decimal>> values, List<Data> responses)
			: base(name, description)
		{
			this.Component = workflow;
			this.factors = factors;
			noOfFactors = factors.Count;
			startingValues = new List<decimal>();
			for (int i = 0; i < factors.Count; i++)
				startingValues.Add(values[i][0]);

			arr = new decimal[factors.Count][];
			for (int i = 0; i < factors.Count; i++)
				arr[i] = new decimal[values[i].Count];
			for (int i = 0; i < factors.Count; i++)
			{
				for (int j = 0; j < values[i].Count; j++)
					arr[i][j] = values[i][j];
			}

			this.responses = responses;
		}




		private List<List<decimal>> Permutation(List<List<decimal>> priorPermutations, decimal[] additions)
		{
			var newPermutationsResult = new List<List<decimal>>();
			foreach (List<decimal> priorPermutation in priorPermutations)
			{
				foreach (decimal addition in additions)
				{
					var priorWithAddition = new List<decimal>(priorPermutation) { addition };
					newPermutationsResult.Add(priorWithAddition);
				}
			}
			return newPermutationsResult;
		}



		public override bool ApplyOn(ExecutableComponent ec)
		{
			if (ec is WorkflowComponent)
			{
				Component = ec as WorkflowComponent;
				ApplyOn();
			}
			return true;
		}

		public override bool ApplyOn()
		{
			csvCreation = true;


			#region Version 1.0
			var tradestudy = new ArrayList();
			for (int i = 0; i < factors.Count; i++)
			{
				var options = new TS_Input_set("Data", factors[i].Name, "min", (double)startingValues[i], "max", (double)(startingValues[i] * noOfLevels[i]), "Increment", (double)noOfLevels[i]);
				tradestudy.Add(options);
			}
			var treatmentTSInput = new Treatment_InOut_TS_Input(tradestudy);

			#endregion Version 1.0

			string directory = Path.GetDirectoryName(databaseFileName);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(databaseFileName);

			string connectionString = $"DataSource=\"{databaseFileName}\"";
			var connection = new SqlCeConnection(connectionString);


			SqlCeCommand insertCmd = null;
			string sql = "";


			var filer = new CSVFiler(CsvPath);


			try
			{
				#region Results Database Preparation

				for (int i = 0; i < factors.Count; i++)
				{
					Result.MinValues.Add((double)(startingValues[i]));
					if (stepSizes == null)
						Result.MaxValues.Add((double)(arr[i][arr[i].Count() - 1]));
					else
						Result.MaxValues.Add((double)(startingValues[i] + (noOfLevels[i] - 1) * stepSizes[i]));
				}
				foreach (Data data in responses)
				{
					// Minimum and maximum values for result will be added later after execution of the workflow
					responsesMinValues.Add(Double.PositiveInfinity);
					responsesMaxValues.Add(Double.NegativeInfinity);
				}

				#endregion Results Database Preparation

				#region Permutations Generation

				var permutations = new List<List<decimal>>();
				foreach (decimal init in arr[0])
				{
					var temp = new List<decimal> { init };
					permutations.Add(temp);
				}
				for (int i = 1; i < arr.Length; ++i)
				{
					permutations = Permutation(permutations, arr[i]);
				}

				#endregion Permutations Generation



				#region SDF File

				if (sdfCreation)
				{
					#region Create tables
					if (connection.State == ConnectionState.Closed)
					{
						connection.Open();
					}
					string createTableSQL = "create table " + fileNameWithoutExtension + " (ID int, ";
					for (int i = 0; i < factors.Count(); i++)
					{
						string columnHeader = factors[i].Name;
						createTableSQL += columnHeader + " ";
						if (factors[i] is IntegerData)
						{
							createTableSQL += "int, ";
						}
						else if (factors[i] is DoubleData)
						{
							createTableSQL += "float, ";
						}
					}
					for (int i = 0; i < responses.Count(); i++)
					{

						string columnHeader = responses[i].Name;

						createTableSQL += columnHeader + " ";
						if ((responses[i]) is IntegerData)
						{
							createTableSQL += "int, ";
						}
						else if (responses[i] is DoubleData)
						{
							createTableSQL += "float, ";
						}
						else if (responses[i] is DoubleVectorData)
						{
							createTableSQL += "nvarchar(2000), ";
						}
						else if (responses[i] is DoubleMatrixData)
						{
							createTableSQL += "nvarchar(4000), ";
						}

					}
					if (factors.Count() + responses.Count() > 0)
					{
						createTableSQL = createTableSQL.Remove(createTableSQL.Length - 2);
					}
					createTableSQL += ")";

					// Create SQL create table command for "SQL Server Compact Edition"
					var createTableSQLCmd = new SqlCeCommand(createTableSQL, connection);
					createTableSQLCmd.ExecuteNonQuery();


					#endregion Create tables


					#region Insert SQL Command

					sql = "insert into " + fileNameWithoutExtension + " (ID, ";
					string valuesString = "values (@ID, ";
					for (int i = 0; i < factors.Count; i++)
					{
						sql += factors[i].Name + ", ";
						valuesString += "@" + factors[i].Name + ", ";
					}
					for (int i = 0; i < responses.Count; i++)
					{
						sql += responses[i].Name + ", ";
						valuesString += "@" + responses[i].Name + ", ";
					}
					if (factors.Count + responses.Count > 0)
					{
						sql = sql.Remove(sql.Length - 2);
						valuesString = valuesString.Remove(valuesString.Length - 2);
					}
					sql += ")";
					valuesString += ")";
					sql += (" " + valuesString);

					#endregion Insert SQL Command
				}

				#endregion SDF File








				int tableID = 0;

				int sz = factors.Count;
				//int tot = (int)inf[1];
				long tot = permutations.Count;
				double[,] indices = new double[tot, sz];
				long updatePeriod = Math.Max(tot / 100, 1);
				foreach (List<decimal> list in permutations)
				{
					tableID++;

					#region Parameter Value Assignment
					for (int i = 0; i < list.Count; i++)
					{
						Data workflowInput = Component.ModelDataInputs.Find(delegate (Data d) { return d.Name == factors[i].Name; });
						if (workflowInput is IntegerData)
							workflowInput.Value = (int)list[i];
						if (workflowInput is DoubleData)
							workflowInput.Value = (double)list[i];
					}
					#endregion Parameter Value Assignment

					#region SDF Creation
					if (sdfCreation)
					{
						insertCmd = new SqlCeCommand(sql, connection);
						insertCmd.Parameters.AddWithValue("@ID", tableID);

						for (int i = 0; i < list.Count; i++)
						{
							insertCmd.Parameters.AddWithValue("@" + factors[i].Name, list[i]);
						}
					}
					#endregion SDF Creation

					// Execute workflow
					bool statusToCheck = Component.Execute();


					for (int i = 0; i < responses.Count; i++)
					{
						// Store workflow data outputs as responses
						Data workflowData = null;
						workflowData = Component.ModelDataInputs.Find(delegate (Data d) { return d.Name == responses[i].Name; });
						if (workflowData == null)
							workflowData = Component.ModelDataOutputs.Find(delegate (Data d) { return d.Name == responses[i].Name; });

						if (workflowData != null)
						{
							#region SDF Creation
							if (sdfCreation)
							{

								if (workflowData is DoubleData)
								{
									responses[i].Value = Convert.ToDouble(workflowData.Value);                      //atif and xin 29042016
									if (((double)(workflowData.Value)) < responsesMinValues[i])
										responsesMinValues[i] = Convert.ToDouble(workflowData.Value);
									if (((double)(workflowData.Value)) > responsesMaxValues[i])
										responsesMaxValues[i] = Convert.ToDouble(workflowData.Value);
									// Update database insert command
									insertCmd.Parameters.AddWithValue("@" + responses[i].Name, (double)(responses[i].Value));
								}
								else if (workflowData is DoubleVectorData)
								{
									responses[i].Value = workflowData.Value;

									// Update database insert command
									string val = "";
									foreach (double d in (double[])(responses[i].Value))
										val += (d + ",");
									val = val.TrimEnd(',');
									insertCmd.Parameters.AddWithValue("@" + responses[i].Name, val);
								}
								else if (workflowData is DoubleMatrixData)
								{
									responses[i].Value = workflowData.Value;

									// Update database insert command
									double[,] data = (double[,])(responses[i].Value);
									string val = "";
									for (int r = 0; r < data.GetLength(0); r++)
									{
										for (int c = 0; c < data.GetLength(1); c++)
										{
											val += (data[r, c] + ",");
										}
										val = val.TrimEnd(',');
										val += ";";
									}
									val = val.TrimEnd(';');
									insertCmd.Parameters.AddWithValue("@" + responses[i].Name, val);
								}
								else if (workflowData is IntegerData)
								{
									responses[i].Value = (int)(workflowData.Value);
									if (((int)(workflowData.Value)) < responsesMinValues[i])
										responsesMinValues[i] = (int)(workflowData.Value);
									if (((int)(workflowData.Value)) > responsesMaxValues[i])
										responsesMaxValues[i] = (int)(workflowData.Value);

									// Update database insert command
									insertCmd.Parameters.AddWithValue("@" + responses[i].Name, (int)(responses[i].Value));
								}
								else if (workflowData is IntegerVectorData)
								{
									responses[i].Value = workflowData.Value;

									// Update database insert command
									string val = "";
									foreach (int d in (int[])(responses[i].Value))
										val += (d + ",");
									val = val.TrimEnd(',');
									insertCmd.Parameters.AddWithValue("@" + responses[i].Name, val);
								}
								else
								{

								}
							}
							#endregion SDF Creation
						}
					}

					// Execute database insert command
					if (statusToCheck)
					{
						#region SDF Creation
						if (sdfCreation)
						{
							insertCmd.ExecuteNonQuery();
						}
						#endregion SDF Creation

						if (csvCreation)
						{
							filer.NewRow();

							filer.AddToRow(tableID);

							for (int i = 0; i < list.Count; i++)
								filer.AddToRow(list[i]);

							for (int i = 0; i < responses.Count; i++)
								filer.AddToRow(responses[i]);

							filer.WriteRow();
						}
					}

					if (tableID % updatePeriod == 0)
						ProgressReposter.ReportProgress(Convert.ToInt32(tableID * 100.0 / tot));
				}
			}
			catch (SqlCeException sqlexception)
			{
				Console.WriteLine(sqlexception.Message, "Oh Crap.");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message, "Oh Crap.");
			}
			finally
			{
				connection.Close();
				filer.Dispose();
				ProgressReposter.ReportProgress(100);
			}

			// Results Min and Max values
			for (int i = 0; i < responses.Count; i++)
			{
				Result.MinValues.Add(responsesMinValues[i]);
				Result.MaxValues.Add(responsesMaxValues[i]);
			}







			return true;
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

			for (int i = 0; i < factors.Count; i++)
				metadata.AddParameter(factors[i], new XElement("FullFactorialFactor",
					new XAttribute("StartingValue", startingValues[i]),
					new XAttribute("StepSize", stepSizes[i]),
					new XAttribute("NoOfLevels", noOfLevels[i])));

			foreach (Data response in responses)
				metadata.AddParameter(response, new XElement("FullFactorialResponse"));

			metadata.Save();
		}
	}
}
