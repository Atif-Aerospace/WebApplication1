using System;
using System.Collections.Generic;
using System.Linq;


using Aircadia.ObjectModel.DataObjects;

using Aircadia.ObjectModel.Treatments.Optimisers.Formulation;

using Aircadia.ObjectModel.Treatments.Optimisers.GA.Selection;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Crossover;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Mutation;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Elitism;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Population;
using Aircadia.ObjectModel.Treatments.Optimisers.GA.Chromosome;


using System.IO;


namespace Aircadia.ObjectModel.Treatments.Optimisers.GA.NSGAII
{
	[Serializable()]
    public class NSGAIIOptimiser : MultiObjectiveGeneticAlgorithmOptimiser
    {
        //*****constructor*****
        public NSGAIIOptimiser(string name, string description)
            : base(name, description)
        {
        }
        public NSGAIIOptimiser(string name, string description, OptimisationTemplate template,  GAParameters gaParameters)
            : base(name, description, gaParameters)
        {
			OptimisationTemplate = template;
        }


		public override bool ApplyOn(ExecutableComponent ec) => ApplyOn();
		//*****implementation of the NSGA-II*****
		//genetic algorithm of NSGA-II
		public override bool ApplyOn()
        {
   //         // Prepare results database
   //         foreach (BoundedDesignVariableNoInital designVariable in OptimisationTemplate.DesignVariables)
   //         {
			//	Result.MinValues.Add(designVariable.LowerBound);
			//	Result.MaxValues.Add(designVariable.UpperBound);
                
   //         }
   //         foreach (Objective objective in OptimisationTemplate.Objectives)
   //         {
			//	Result.MinValues.Add(Double.NaN);
			//	Result.MaxValues.Add(Double.NaN);
   //         }
   //         foreach (Aircadia.ObjectModel.Treatments.Optimisers.Formulation.Constraint constraint in OptimisationTemplate.Constraints)
   //         {
			//	Result.MinValues.Add(Double.NaN);
			//	Result.MaxValues.Add(Double.NaN);
   //         }
			//Result.BatchSize = GAParameters.PopulationSize;
			//Result.NoOfBatches = GAParameters.NoOfGenerations;

   //         string studyName = Path.GetFileNameWithoutExtension(Result.DatabasePath);
   //         string sd = Path.GetDirectoryName(Result.DatabasePath);

   //         string databaseFileName1 = Path.Combine(Project.ProjectPath, sd, studyName + "All.sdf"); // Microsoft SQL server compact edition file
   //         string databaseFileName2 = Path.Combine(Project.ProjectPath, sd, studyName + "Gen.sdf"); // Microsoft SQL server compact edition file

   //         // Create database for optimisation results
   //         string connectionString1;
   //         string connectionString2;
   //         if (File.Exists(databaseFileName1))
   //             File.Delete(databaseFileName1);
   //         if (File.Exists(databaseFileName2))
   //             File.Delete(databaseFileName2);
   //         connectionString1 = String.Format("Data Source = " + databaseFileName1 + ";Persist Security Info=False");
   //         var engine1 = new SqlCeEngine(connectionString1);
   //         engine1.CreateDatabase();
   //         connectionString2 = String.Format("Data Source = " + databaseFileName2 + ";Persist Security Info=False");
   //         var engine2 = new SqlCeEngine(connectionString2);
   //         engine2.CreateDatabase();

   //         // Create tables
   //         var connection1 = new SqlCeConnection(connectionString1);
   //         if (connection1.State == System.Data.ConnectionState.Closed)
   //         {
   //             connection1.Open();
   //         }
   //         var connection2 = new SqlCeConnection(connectionString2);
   //         if (connection2.State == System.Data.ConnectionState.Closed)
   //         {
   //             connection2.Open();
   //         }


   //         // Create SQL statements to create tables, sqlStatement1 for OptimAllResults and sqlStatement2 for OptimGenResults
   //         var names = new List<string>();
   //         string sqlStatement1 = "create table " + studyName + "All (";
   //         string sqlStatement2 = "create table " + studyName + "Gen (";
   //         sqlStatement1 += "ID int, ";
   //         sqlStatement2 += "ID int, Category int, ";
   //         for (int i = 0; i < OptimisationTemplate.DesignVariables.Count(); i++)
   //         {
   //             string columnHeader = OptimisationTemplate.DesignVariables[i].Name;
   //             sqlStatement1 += columnHeader + " ";
   //             sqlStatement2 += columnHeader + " ";
   //             if (OptimisationTemplate.DesignVariables[i].Data is IntegerData)
   //             {
   //                 sqlStatement1 += "int, ";
   //                 sqlStatement2 += "int, ";
   //             }
   //             else if (OptimisationTemplate.DesignVariables[i].Data is DoubleData)
   //             {
   //                 sqlStatement1 += "float, ";
   //                 sqlStatement2 += "float, ";
   //             }
   //             names.Add(OptimisationTemplate.DesignVariables[i].Name);
   //         }
   //         for (int i = 0; i < OptimisationTemplate.Objectives.Count(); i++)
   //         {
   //             string columnHeader = OptimisationTemplate.Objectives[i].Name;
   //             sqlStatement1 += columnHeader + " ";
   //             sqlStatement2 += columnHeader + " ";
   //             if ((OptimisationTemplate.Objectives[i].Data) is IntegerData)
   //             {
   //                 sqlStatement1 += "int, ";
   //                 sqlStatement2 += "int, ";
   //             }
   //             else if (OptimisationTemplate.Objectives[i].Data is DoubleData)
   //             {
   //                 sqlStatement1 += "float, ";
   //                 sqlStatement2 += "float, ";
   //             }
   //             names.Add(OptimisationTemplate.Objectives[i].Name);
   //         }
   //         for (int i = 0; i < OptimisationTemplate.Constraints.Count(); i++)
   //         {
   //             if (names.Contains(OptimisationTemplate.Constraints[i].Name))
   //                 continue;
   //             string columnHeader = OptimisationTemplate.Constraints[i].Name;
   //             sqlStatement1 += columnHeader + " ";
   //             sqlStatement2 += columnHeader + " ";
   //             if ((OptimisationTemplate.Constraints[i].Data) is IntegerData)
   //             {
   //                 sqlStatement1 += "int, ";
   //                 sqlStatement2 += "int, ";
   //             }
   //             else if (OptimisationTemplate.Constraints[i].Data is DoubleData)
   //             {
   //                 sqlStatement1 += "float, ";
   //                 sqlStatement2 += "float, ";
   //             }
   //             names.Add(OptimisationTemplate.Constraints[i].Name);
   //         }
   //         if (OptimisationTemplate.DesignVariables.Count() + OptimisationTemplate.Objectives.Count() + OptimisationTemplate.Constraints.Count() > 0)
   //         {
   //             sqlStatement1 = sqlStatement1.Remove(sqlStatement1.Length - 2);
   //             sqlStatement2 = sqlStatement2.Remove(sqlStatement2.Length - 2);
   //         }
   //         sqlStatement1 += ")";
   //         sqlStatement2 += ")";

   //         // Create SQL statement command for "SQL Server Compact Edition"
   //         var command1 = new SqlCeCommand(sqlStatement1, connection1);
   //         var command2 = new SqlCeCommand(sqlStatement2, connection2);
   //         // Execute the SQL command
   //         try
   //         {
   //             command1.ExecuteNonQuery();
   //             command2.ExecuteNonQuery();
   //         }
   //         catch (SqlCeException sqlexception)
   //         {
			//	Console.WriteLine(sqlexception.Message);
   //         }
   //         catch (Exception ex)
   //         {
			//	Console.WriteLine(ex.Message);
   //         }













   //         //initialising selection operator
   //         if (GAParameters.SelectionOprMethod == SelectionOprMethods.TournamentSelectionWithoutReplacement)
   //         {
			//	selectionOpr = new TournamentSelectionOprWithoutReplacement(OptimisationTemplate, GAParameters);
   //         }
   //         else if (GAParameters.SelectionOprMethod == SelectionOprMethods.TournamentSelectionWithReplacement)
   //         {
   //         }

   //         //initialising crossover operator
   //         if (GAParameters.CrossoverOprMethod == CrossoverOprMethods.OnePointCrossover)
   //         {
			//	crossoverOpr = new OnePointCrossoverOpr(OptimisationTemplate, GAParameters);
   //         }
   //         else if (GAParameters.CrossoverOprMethod == CrossoverOprMethods.TwoPointCrossover)
   //         {
   //         }
   //         else if (GAParameters.CrossoverOprMethod == CrossoverOprMethods.UniformCrossover)
   //         {
   //         }
   //         else if (GAParameters.CrossoverOprMethod == CrossoverOprMethods.SimulatedBinaryCrossover)
   //         {
			//	crossoverOpr = new SimulatedBinaryCrossoverOpr(OptimisationTemplate, GAParameters);
   //         }

   //         //initialising mutation operator
   //         if (GAParameters.MutationOprMethod == MutationOprMethods.SelectiveMutation)
   //         {
			//	mutationOpr = new SelectiveMutationOpr(OptimisationTemplate, GAParameters);
   //         }
   //         else if (GAParameters.MutationOprMethod == MutationOprMethods.GenewiseMutation)
   //         {
   //         }
   //         else if (GAParameters.MutationOprMethod == MutationOprMethods.PolynomialMutation)
   //         {
			//	mutationOpr = new PolynomialMutationOpr(OptimisationTemplate, GAParameters);
   //         }

   //         //initialising elitism operator
   //         if (GAParameters.ElitismOprMethod == ElitismOprMethods.ProportionalElitism)
   //         {
   //             //this.elitismOpr = new ProportionalElitismOpr(Template, gaParameters);
   //         }
   //         else if (GAParameters.ElitismOprMethod == ElitismOprMethods.UnproportionalElitism)
   //         {
   //             //this.elitismOpr = new UnproportionalElitismOpr(Template, gaParameters);
   //         }

   //         var t = new MultiObjectiveChromosome(OptimisationTemplate, GAParameters, false);
			//parentPopulation = new MultiObjectivePopulation(OptimisationTemplate, GAParameters, false);
			//childPopulation = new MultiObjectivePopulation(OptimisationTemplate, GAParameters, false);




















   //         int table1ID = 0;
   //         int table2ID = 0;


   //         //if (connection1.State == ConnectionState.Closed)
   //         //{
   //         //    connection1.Open();
   //         //}

   //         SqlCeCommand cmd1 = null;
   //         SqlCeCommand cmd2 = null;
   //         names.Clear();
   //         string sql1 = "insert into " + studyName + "All (ID, ";
   //         string sql2 = "insert into " + studyName + "Gen (ID, Category, ";
   //         string valuesString1 = "values (@ID, ";
   //         string valuesString2 = "values (@ID, @Category, ";
   //         string valuesString = "";
   //         for (int i = 0; i < OptimisationTemplate.DesignVariables.Count; i++)
   //         {
   //             sql1 += OptimisationTemplate.DesignVariables[i].Name + ", ";
   //             sql2 += OptimisationTemplate.DesignVariables[i].Name + ", ";
   //             valuesString += "@" + OptimisationTemplate.DesignVariables[i].Name + ", ";
   //             names.Add(OptimisationTemplate.DesignVariables[i].Name);
   //         }
   //         for (int i = 0; i < OptimisationTemplate.Objectives.Count; i++)
   //         {
   //             sql1 += OptimisationTemplate.Objectives[i].Name + ", ";
   //             sql2 += OptimisationTemplate.Objectives[i].Name + ", ";
   //             valuesString += "@" + OptimisationTemplate.Objectives[i].Name + ", ";
   //             names.Add(OptimisationTemplate.Objectives[i].Name);
   //         }
   //         for (int i = 0; i < OptimisationTemplate.Constraints.Count; i++)
   //         {
   //             if (names.Contains(OptimisationTemplate.Constraints[i].Name))
   //                 continue;
   //             sql1 += OptimisationTemplate.Constraints[i].Name + ", ";
   //             sql2 += OptimisationTemplate.Constraints[i].Name + ", ";
   //             valuesString += "@" + OptimisationTemplate.Constraints[i].Name + ", ";
   //             names.Add(OptimisationTemplate.Constraints[i].Name);
   //         }
   //         if (OptimisationTemplate.DesignVariables.Count() + OptimisationTemplate.Objectives.Count() + OptimisationTemplate.Constraints.Count() > 0)
   //         {
   //             sql1 = sql1.Remove(sql1.Length - 2);
   //             sql2 = sql2.Remove(sql2.Length - 2);
   //             valuesString = valuesString.Remove(valuesString.Length - 2);
   //         }
   //         sql1 += ")";
   //         sql2 += ")";
   //         valuesString += ")";
   //         valuesString1 += valuesString;
   //         valuesString2 += valuesString;
   //         sql1 += (" " + valuesString1);
   //         sql2 += (" " + valuesString2);



   //         string sssss1 = Path.Combine(Project.ProjectPath, sd, studyName + "All.csv");
   //         string sssss2 = Path.Combine(Project.ProjectPath, sd, studyName + "Gen.csv");
   //         TextWriter solutionsFileWriter = new StreamWriter(GAParameters.EvaluatedSolutionsFile);
   //         TextWriter allSolutionsFileWriter = new StreamWriter(sssss1);
   //         TextWriter generationsFileWriter = new StreamWriter(sssss2);

   //         //*****need to remove (only for getting same results with Cpp), there is already one at the correct place*****
   //         //combined population
   //         var combinedPopulation = new MultiObjectivePopulation(OptimisationTemplate, GAParameters, parentPopulation.Size * 2, false);

   //         int generationID = 0;


   //         parentPopulation.Evaluate();
   //         parentPopulation.ComputeMaximumFitness();
   //         parentPopulation.ComputeRanks();
   //         parentPopulation.ComputeCrowdingDistances();




   //         //writing to console and text file
   //         System.Console.WriteLine("Generation: " + generationID);
   //         double[] x;
   //         double[] y;
   //         double[] c;
			//string result = "";
   //         for (int i = 0; i < OptimisationTemplate.Objectives.Count; i++)
   //             result += "obj# " + i + "\t\t\t";
   //         System.Console.WriteLine(result);
   //         //writing to optimResult object

   //         //// Added by Marco: ============================================================================================================================================
   //         //fOptimisationStatus OptimisationProgress = new fOptimisationStatus(this.parentPopulation.Size, this.gaParameters.NoOfGenerations);
   //         //OptimisationProgress.Show();
   //         //OptimisationProgress.Tag = "Executing";
   //         //// ============================================================================================================================================================


   //         // Storing to optimResult object and writing to both files (all solutions file and generations file)
   //         for (int i = 0; i < parentPopulation.Size; i++)
   //         {
   //             x = ((MultiObjectiveChromosome)(parentPopulation[i])).Genes;
   //             y = ((MultiObjectiveChromosome)(parentPopulation[i])).ObjectiveValues;
   //             c = ((MultiObjectiveChromosome)(parentPopulation[i])).ConstraintValues;
   //             try
   //             {
   //                 cmd1 = new SqlCeCommand(sql1, connection1);
   //                 cmd2 = new SqlCeCommand(sql2, connection2);
   //             }
   //             catch (SqlCeException sqlexception)
   //             {
			//		Console.WriteLine(sqlexception.Message);
			//	}
			//	catch (Exception ex)
   //             {
			//		Console.WriteLine(ex.Message);
			//	}
			//	names.Clear();
   //             cmd1.Parameters.AddWithValue("@ID", ++table1ID);
   //             cmd2.Parameters.AddWithValue("@ID", ++table2ID);
   //             cmd2.Parameters.AddWithValue("@Category", generationID);
   //             result = "\"" + generationID + "\",";
   //             for (int j = 0; j < x.Length; j++)
   //             {
   //                 cmd1.Parameters.AddWithValue("@" + OptimisationTemplate.DesignVariables[j].Name, x[j]);
   //                 cmd2.Parameters.AddWithValue("@" + OptimisationTemplate.DesignVariables[j].Name, x[j]);
   //                 result += "\"" + x[j] + "\",";
   //                 names.Add(OptimisationTemplate.DesignVariables[j].Name);
   //             }
   //             for (int j = 0; j < y.Length; j++)
   //             {
   //                 cmd1.Parameters.AddWithValue("@" + OptimisationTemplate.Objectives[j].Name, y[j]);
   //                 cmd2.Parameters.AddWithValue("@" + OptimisationTemplate.Objectives[j].Name, y[j]);
   //                 result += "\"" + y[j] + "\",";
   //                 names.Add(OptimisationTemplate.Objectives[j].Name);
   //             }
   //             for (int j = 0; j < c.Length; j++)
   //             {
   //                 if (names.Contains(OptimisationTemplate.Constraints[j].Name))
   //                     continue;
   //                 cmd1.Parameters.AddWithValue("@" + OptimisationTemplate.Constraints[j].Name, c[j]);
   //                 cmd2.Parameters.AddWithValue("@" + OptimisationTemplate.Constraints[j].Name, c[j]);
   //                 result += "\"" + c[j] + "\",";
   //                 names.Add(OptimisationTemplate.Constraints[j].Name);
   //             }

   //             try
   //             {
   //                 cmd1.ExecuteNonQuery();
   //                 cmd2.ExecuteNonQuery();
   //             }
   //             catch (SqlCeException sqlexception)
   //             {
			//		Console.WriteLine(sqlexception.Message);
			//	}
			//	catch (Exception ex)
   //             {
			//		Console.WriteLine(ex.Message);
			//	}
			//	allSolutionsFileWriter.WriteLine(result);
   //             generationsFileWriter.WriteLine(result);
   //             solutionsFileWriter.WriteLine(result);
   //             System.Console.WriteLine(result);
   //         }

   //         // Main loop for NSGAII
   //         while (generationID < GAParameters.NoOfGenerations)
   //         {
   //             #region Writing to console for debugging
   //             for (int i = 0; i < parentPopulation.Size; i++)
   //             {
   //                 System.Console.WriteLine("^^^" + ((MultiObjectiveChromosome)(parentPopulation[i])).Rank + "^^^" + ((MultiObjectiveChromosome)(parentPopulation[i])).CrowdingDistance);
   //             }
   //             #endregion

   //             // Apply selection operator
   //             int[] matingPoolIndex = selectionOpr.ApplySelectionOpr(parentPopulation);
   //             for (int i = 0; i < parentPopulation.Size; i++)
   //             {
			//		childPopulation[i] = parentPopulation[matingPoolIndex[i]];
   //             }
   //             #region Writing to console for debugging
   //             System.Console.WriteLine("++++++++++++++++++++++++++++++++++++++");
   //             System.Console.WriteLine("++++++++++++After Selection+++++++++++");
   //             System.Console.WriteLine("++++++++++++++++++++++++++++++++++++++");
   //             for (int i = 0; i < childPopulation.Size; i++)
   //             {
   //                 x = ((MultiObjectiveChromosome)(childPopulation[i])).Genes;
   //                 y = ((MultiObjectiveChromosome)(childPopulation[i])).ObjectiveValues;
   //                 c = ((MultiObjectiveChromosome)(childPopulation[i])).ConstraintViolationValues;
   //                 result = "";
   //                 for (int j = 0; j < x.Length; j++)
   //                     result += x[j] + "\t";
   //                 for (int j = 0; j < y.Length; j++)
   //                     result += y[j] + "\t";
   //                 for (int j = 0; j < c.Length; j++)
   //                     result += c[j] + "\t";
   //                 System.Console.WriteLine(result);
   //             }
   //             System.Console.WriteLine("++++++++++++++++++++++++++++++++++++++");
   //             #endregion

   //             // Apply crossover operator
   //             for (int i = 0; i < parentPopulation.Size; i += 2)
   //             {
   //                 if (GARandom.Flip(GAParameters.CrossoverProbability))
   //                 {
   //                     crossoverOpr.ApplyCrossoverOpr(childPopulation[i], childPopulation[i + 1]);
   //                 }
   //             }
   //             #region Writing to console for debugging
   //             System.Console.WriteLine("++++++++++++++++++++++++++++++++++++++");
   //             System.Console.WriteLine("++++++++++++After Crossover+++++++++++");
   //             System.Console.WriteLine("++++++++++++++++++++++++++++++++++++++");
   //             for (int i = 0; i < childPopulation.Size; i++)
   //             {
   //                 x = ((MultiObjectiveChromosome)(childPopulation[i])).Genes;
   //                 y = ((MultiObjectiveChromosome)(childPopulation[i])).ObjectiveValues;
   //                 c = ((MultiObjectiveChromosome)(childPopulation[i])).ConstraintViolationValues;
   //                 result = "";
   //                 for (int j = 0; j < x.Length; j++)
   //                     result += x[j] + "\t";
   //                 for (int j = 0; j < y.Length; j++)
   //                     result += y[j] + "\t";
   //                 for (int j = 0; j < c.Length; j++)
   //                     result += c[j] + "\t";
   //                 System.Console.WriteLine(result);
   //             }
   //             System.Console.WriteLine("++++++++++++++++++++++++++++++++++++++");
   //             #endregion

   //             // Apply mutation operator
   //             for (int i = 0; i < parentPopulation.Size; i++)
   //             {
   //                 mutationOpr.ApplyMutationOpr(childPopulation[i]);
   //             }
   //             #region Writing to console for debugging
   //             System.Console.WriteLine("++++++++++++++++++++++++++++++++++++++");
   //             System.Console.WriteLine("++++++++++++After Mutation++++++++++++");
   //             System.Console.WriteLine("++++++++++++++++++++++++++++++++++++++");
   //             for (int i = 0; i < childPopulation.Size; i++)
   //             {
   //                 x = ((MultiObjectiveChromosome)(childPopulation[i])).Genes;
   //                 y = ((MultiObjectiveChromosome)(childPopulation[i])).ObjectiveValues;
   //                 c = ((MultiObjectiveChromosome)(childPopulation[i])).ConstraintViolationValues;
   //                 result = "";
   //                 for (int j = 0; j < x.Length; j++)
   //                     result += x[j] + "\t";
   //                 for (int j = 0; j < y.Length; j++)
   //                     result += y[j] + "\t";
   //                 for (int j = 0; j < c.Length; j++)
   //                     result += c[j] + "\t";
   //                 System.Console.WriteLine(result);
   //             }
   //             System.Console.WriteLine("++++++++++++++++++++++++++++++++++++++");
			//	#endregion

			//	childPopulation.Evaluate();

   //             // Storing child population to optimResult object
   //             for (int i = 0; i < childPopulation.Size; i++)
   //             {
   //                 x = ((MultiObjectiveChromosome)(childPopulation[i])).Genes;
   //                 y = ((MultiObjectiveChromosome)(childPopulation[i])).ObjectiveValues;
   //                 c = ((MultiObjectiveChromosome)(childPopulation[i])).ConstraintValues;
   //                 try
   //                 {
   //                     cmd1 = new SqlCeCommand(sql1, connection1);
   //                 }
   //                 catch (SqlCeException sqlexception)
   //                 {
			//			Console.WriteLine(sqlexception.Message);
			//		}
			//		catch (Exception ex)
   //                 {
			//			Console.WriteLine(ex.Message);
			//		}
			//		names.Clear();
   //                 cmd1.Parameters.AddWithValue("@ID", ++table1ID);
   //                 for (int j = 0; j < x.Length; j++)
   //                 {
   //                     cmd1.Parameters.AddWithValue("@" + OptimisationTemplate.DesignVariables[j].Name, x[j]);
   //                     names.Add(OptimisationTemplate.DesignVariables[j].Name);
   //                 }
   //                 for (int j = 0; j < y.Length; j++)
   //                 {
   //                     cmd1.Parameters.AddWithValue("@" + OptimisationTemplate.Objectives[j].Name, y[j]);
   //                     names.Add(OptimisationTemplate.Objectives[j].Name);
   //                 }
   //                 for (int j = 0; j < c.Length; j++)
   //                 {
   //                     if (names.Contains(OptimisationTemplate.Constraints[j].Name))
   //                         continue;
   //                     cmd1.Parameters.AddWithValue("@" + OptimisationTemplate.Constraints[j].Name, c[j]);
   //                     names.Add(OptimisationTemplate.Constraints[j].Name);
   //                 }
   //                 try
   //                 {
   //                     cmd1.ExecuteNonQuery();
   //                 }
   //                 catch (SqlCeException sqlexception)
   //                 {
			//			Console.WriteLine(sqlexception.Message);
			//		}
			//		catch (Exception ex)
   //                 {
			//			Console.WriteLine(ex.Message);
			//		}
			//	}
   //             // Writing to file (all solutions file)
   //             for (int i = 0; i < childPopulation.Size; i++)
   //             {
   //                 x = ((MultiObjectiveChromosome)(childPopulation[i])).Genes;
   //                 y = ((MultiObjectiveChromosome)(childPopulation[i])).ObjectiveValues;
   //                 c = ((MultiObjectiveChromosome)(childPopulation[i])).ConstraintValues;
   //                 result = "\"" + generationID + "\",";
   //                 for (int j = 0; j < x.Length; j++)
   //                     result += "\"" + x[j] + "\",";
   //                 for (int j = 0; j < y.Length; j++)
   //                     result += "\"" + y[j] + "\",";
   //                 for (int j = 0; j < c.Length; j++)
   //                     result += "\"" + c[j] + "\",";
   //                 allSolutionsFileWriter.WriteLine(result);
   //                 solutionsFileWriter.WriteLine(result);
   //             }


   //             //will be uncommented later on
   //             //correct place (this is best place to declare combined population, the one above is for comparison purposes with cpp results)
   //             //combined population (correct place)
   //             //MultiObjectivePopulation combinedPopulation = new MultiObjectivePopulation(this.Template, this.gaParameters, this.parentPopulation.Size*2, true);

   //             for (int i = 0; i < parentPopulation.Size; i++)
   //             {
   //                 //                    combinedPopulation[i].Genes = this.parentPopulation[i].Genes;
   //                 for (int j = 0; j < ((MultiObjectiveChromosome)(parentPopulation[i])).Genes.Length; j++)
   //                     ((MultiObjectiveChromosome)(combinedPopulation[i])).Genes[j] = parentPopulation[i].Genes[j];

   //                 for (int j = 0; j < ((MultiObjectiveChromosome)(parentPopulation[i])).ObjectiveValues.Length; j++)
   //                     ((MultiObjectiveChromosome)(combinedPopulation[i])).ObjectiveValues[j] = ((MultiObjectiveChromosome)(parentPopulation[i])).ObjectiveValues[j];
   //                 for (int j = 0; j < ((MultiObjectiveChromosome)(parentPopulation[i])).FitnessValues.Length; j++)
   //                     ((MultiObjectiveChromosome)(combinedPopulation[i])).FitnessValues[j] = ((MultiObjectiveChromosome)(parentPopulation[i])).FitnessValues[j];

   //                 for (int j = 0; j < ((MultiObjectiveChromosome)(parentPopulation[i])).ConstraintValues.Length; j++)
   //                     ((MultiObjectiveChromosome)(combinedPopulation[i])).ConstraintValues[j] = ((MultiObjectiveChromosome)(parentPopulation[i])).ConstraintValues[j];
   //                 for (int j = 0; j < ((MultiObjectiveChromosome)(parentPopulation[i])).ConstraintViolationValues.Length; j++)
   //                     ((MultiObjectiveChromosome)(combinedPopulation[i])).ConstraintViolationValues[j] = ((MultiObjectiveChromosome)(parentPopulation[i])).ConstraintViolationValues[j];
   //                 ((MultiObjectiveChromosome)(combinedPopulation[i])).NoOfConstraintViolations = ((MultiObjectiveChromosome)(parentPopulation[i])).NoOfConstraintViolations;
   //                 ((MultiObjectiveChromosome)(combinedPopulation[i])).Penalty = ((MultiObjectiveChromosome)(parentPopulation[i])).Penalty;

   //                 ((MultiObjectiveChromosome)(combinedPopulation[i])).Rank = ((MultiObjectiveChromosome)(parentPopulation[i])).Rank;
   //                 ((MultiObjectiveChromosome)(combinedPopulation[i])).CrowdingDistance = ((MultiObjectiveChromosome)(parentPopulation[i])).CrowdingDistance;
   //             }
   //             for (int i = 0; i < childPopulation.Size; i++)
   //             {
   //                 for (int j = 0; j < ((MultiObjectiveChromosome)(parentPopulation[i])).Genes.Length; j++)
   //                     ((MultiObjectiveChromosome)(combinedPopulation[i + parentPopulation.Size])).Genes[j] = childPopulation[i].Genes[j];

   //                 for (int j = 0; j < ((MultiObjectiveChromosome)(parentPopulation[i])).ObjectiveValues.Length; j++)
   //                     ((MultiObjectiveChromosome)(combinedPopulation[i + parentPopulation.Size])).ObjectiveValues[j] = ((MultiObjectiveChromosome)(childPopulation[i])).ObjectiveValues[j];
   //                 for (int j = 0; j < ((MultiObjectiveChromosome)(parentPopulation[i])).FitnessValues.Length; j++)
   //                     ((MultiObjectiveChromosome)(combinedPopulation[i + parentPopulation.Size])).FitnessValues[j] = ((MultiObjectiveChromosome)(childPopulation[i])).FitnessValues[j];

   //                 for (int j = 0; j < ((MultiObjectiveChromosome)(parentPopulation[i])).ConstraintValues.Length; j++)
   //                     ((MultiObjectiveChromosome)(combinedPopulation[i + parentPopulation.Size])).ConstraintValues[j] = ((MultiObjectiveChromosome)(childPopulation[i])).ConstraintValues[j];
   //                 for (int j = 0; j < ((MultiObjectiveChromosome)(parentPopulation[i])).ConstraintViolationValues.Length; j++)
   //                     ((MultiObjectiveChromosome)(combinedPopulation[i + parentPopulation.Size])).ConstraintViolationValues[j] = ((MultiObjectiveChromosome)(childPopulation[i])).ConstraintViolationValues[j];
   //                 ((MultiObjectiveChromosome)(combinedPopulation[i + parentPopulation.Size])).NoOfConstraintViolations = ((MultiObjectiveChromosome)(childPopulation[i])).NoOfConstraintViolations;
   //                 ((MultiObjectiveChromosome)(combinedPopulation[i + parentPopulation.Size])).Penalty = ((MultiObjectiveChromosome)(childPopulation[i])).Penalty;

   //                 ((MultiObjectiveChromosome)(combinedPopulation[i + parentPopulation.Size])).Rank = ((MultiObjectiveChromosome)(childPopulation[i])).Rank;
   //                 ((MultiObjectiveChromosome)(combinedPopulation[i + parentPopulation.Size])).CrowdingDistance = ((MultiObjectiveChromosome)(childPopulation[i])).CrowdingDistance;
   //             }

   //             //                combinedPopulation.Evaluate();

   //             combinedPopulation.ComputeMaximumFitness();
   //             combinedPopulation.ComputeRanks();
   //             combinedPopulation.ComputeCrowdingDistances();

   //             //*****copy best chromosomes
   //             int noOfCopiedInds = 0, frontID = 0;
   //             int noOfIndsInFront = combinedPopulation.NoOfCromInAllFronts[frontID];
   //             int indID;
   //             //copying the fronts starting with best front (ID = 0) as long as the whole front can be copied in the parentPopulation
   //             while ((noOfCopiedInds + noOfIndsInFront) <= parentPopulation.Size)
   //             {
   //                 for (int i = 0; i < noOfIndsInFront; i++)
   //                 {
   //                     indID = (combinedPopulation.Fronts[frontID])[i];
			//			parentPopulation[noOfCopiedInds + i].Copy(combinedPopulation[indID]);
   //                 }
   //                 noOfCopiedInds += noOfIndsInFront;
   //                 frontID++;
   //                 noOfIndsInFront = combinedPopulation.NoOfCromInAllFronts[frontID];
   //             }
   //             //if whole front can not be copied
   //             if (noOfCopiedInds < parentPopulation.Size)
   //             {
   //                 double[] crowdingDistances = new double[noOfIndsInFront];
   //                 int[] index = new int[noOfIndsInFront];
   //                 int[] frontSortedIndex = new int[noOfIndsInFront];

   //                 for (int i = 0; i < noOfIndsInFront; i++)
   //                 {
   //                     indID = (combinedPopulation.Fronts[frontID])[i];
   //                     index[i] = indID;
   //                     crowdingDistances[i] = ((MultiObjectiveChromosome)combinedPopulation[indID]).CrowdingDistance;
   //                     frontSortedIndex[i] = i;
   //                 }

   //                 FrontQuickSorting(crowdingDistances, frontSortedIndex, 0, noOfIndsInFront);

   //                 for (int i = noOfCopiedInds; i < parentPopulation.Size; i++)
   //                 {
   //                     indID = index[frontSortedIndex[noOfIndsInFront + noOfCopiedInds - i - 1]];
			//			parentPopulation[i].Copy(combinedPopulation[indID]);
   //                 }
   //             }


   //             /*
   //             //******************************************************************************************************************************************************************
   //                             //*****copy best chromosomes
   //                             int noOfCopiedInds = 0, frontID = 0;
   //                             int noOfIndsInFront = combinedPopulation.NoOfCromInAllFronts[frontID];
   //                             int indID;
   //                             //copying the fronts starting with best front (ID = 0) as long as the whole front can be copied in the parentPopulation
   //                             while ((noOfCopiedInds + noOfIndsInFront) <= this.parentPopulation.Size)
   //                             {
   //                                 for (int i = 0; i < noOfIndsInFront; i++)
   //                                 {
   //                                     indID = (combinedPopulation.Fronts[frontID])[i];
   //                                     System.Console.WriteLine("atif" + indID);
   //                                     this.parentPopulation[noOfCopiedInds + i].Copy(combinedPopulation[indID]);
   //                                 }
   //                                 noOfCopiedInds += noOfIndsInFront;
   //                                 frontID++;
   //                                 noOfIndsInFront = combinedPopulation.NoOfCromInAllFronts[frontID];
   //                             }
   //                             //if whole front can not be copied
   //                             if (noOfCopiedInds < this.parentPopulation.Size)
   //                             {
   //                                 double[] crowdingDistances = new double[noOfIndsInFront];
   //                                 int[] index = new int[noOfIndsInFront];
   //                                 int[] frontSortedIndex = new int[noOfIndsInFront];

   //                                 for (int i = 0; i < noOfIndsInFront; i++)
   //                                 {
   //                                     indID = (combinedPopulation.Fronts[frontID])[i];
   //                                     index[i] = indID;
   //                                     crowdingDistances[i] = ((MultiObjectiveChromosome)combinedPopulation[indID]).CrowdingDistance;
   //                                     frontSortedIndex[i] = i;
   //                                 }

   //                                 frontQuickSorting(crowdingDistances, frontSortedIndex, 0, noOfIndsInFront);

   //                                 for (int i = 0; i < this.parentPopulation.Size - noOfCopiedInds; i++)
   //                                 {
   //                                     indID = index[frontSortedIndex[i]];
   //                                     System.Console.WriteLine("atif" + indID);
   //                                     this.parentPopulation[i + noOfCopiedInds].Copy(combinedPopulation[indID]);
   //                                 }

   //                             }
   //             //*********************************************************************************************************************************************************************************
   //             */

   //             generationID++;

   //             //// Added by Marco: ============================================================================================================================================
   //             //OptimisationProgress.GenerationsProgressBar(generationID); //Updating the Generations Progress-bar
   //             //OptimisationProgress.Activate();
   //             //// ============================================================================================================================================================


   //             // Storing next generation, i.e. parent population (parent population has been copied from child population) to optimResult object
   //             for (int i = 0; i < parentPopulation.Size; i++)
   //             {
   //                 x = ((MultiObjectiveChromosome)(parentPopulation[i])).Genes;
   //                 y = ((MultiObjectiveChromosome)(parentPopulation[i])).ObjectiveValues;
   //                 c = ((MultiObjectiveChromosome)(parentPopulation[i])).ConstraintValues;
   //                 try
   //                 {
   //                     cmd2 = new SqlCeCommand(sql2, connection2);
   //                 }
   //                 catch (SqlCeException sqlexception)
   //                 {
			//			Console.WriteLine(sqlexception.Message);
			//		}
			//		catch (Exception ex)
   //                 {
			//			Console.WriteLine(ex.Message);
			//		}
			//		names.Clear();
   //                 cmd2.Parameters.AddWithValue("@ID", ++table2ID);
   //                 cmd2.Parameters.AddWithValue("@Category", generationID);
   //                 for (int j = 0; j < x.Length; j++)
   //                 {
   //                     cmd2.Parameters.AddWithValue("@" + OptimisationTemplate.DesignVariables[j].Name, x[j]);
   //                     names.Add(OptimisationTemplate.DesignVariables[j].Name);
   //                 }
   //                 for (int j = 0; j < y.Length; j++)
   //                 {
   //                     cmd2.Parameters.AddWithValue("@" + OptimisationTemplate.Objectives[j].Name, y[j]);
   //                     names.Add(OptimisationTemplate.Objectives[j].Name);
   //                 }
   //                 for (int j = 0; j < c.Length; j++)
   //                 {
   //                     if (names.Contains(OptimisationTemplate.Constraints[j].Name))
   //                         continue;
   //                     cmd2.Parameters.AddWithValue("@" + OptimisationTemplate.Constraints[j].Name, c[j]);
   //                     names.Add(OptimisationTemplate.Constraints[j].Name);
   //                 }

   //                 try
   //                 {
   //                     cmd2.ExecuteNonQuery();
   //                 }
   //                 catch (SqlCeException sqlexception)
   //                 {
			//			Console.WriteLine(sqlexception.Message);
			//		}
			//		catch (Exception ex)
   //                 {
			//			Console.WriteLine(ex.Message);
			//		}
			//	}
   //             // Writing to file (generations file)
   //             for (int i = 0; i < parentPopulation.Size; i++)
   //             {
   //                 x = ((MultiObjectiveChromosome)(parentPopulation[i])).Genes;
   //                 y = ((MultiObjectiveChromosome)(parentPopulation[i])).ObjectiveValues;
   //                 c = ((MultiObjectiveChromosome)(parentPopulation[i])).ConstraintValues;
   //                 result = "\"" + generationID + "\",";
   //                 for (int j = 0; j < x.Length; j++)
   //                     result += "\"" + x[j] + "\",";
   //                 for (int j = 0; j < y.Length; j++)
   //                     result += "\"" + y[j] + "\",";
   //                 for (int j = 0; j < c.Length; j++)
   //                     result += "\"" + c[j] + "\",";
   //                 generationsFileWriter.WriteLine(result);
   //                 System.Console.WriteLine(result);
   //             }



   //         }

			//Result.IsComplete = true;

   //         connection1.Close();
   //         connection2.Close();
   //         allSolutionsFileWriter.Close();
   //         generationsFileWriter.Close();
   //         solutionsFileWriter.Close();

            return true;
        }


        //front sorting in decending order based on crowding distance
        //array crowdingDistances stores the crowding distances of all the individuals in a particular front
        //array frontSortedIndex will stores the indices of the sorted array of crowding distances
        //change the two greater than and smaller than operators for ascending order
        private void FrontQuickSorting(double[] crowdingDistances, int[] frontSortedIndex, int left, int right)
        {
            if (right > left)
            {
                double target = crowdingDistances[frontSortedIndex[right - 1]];
                int i = left - 1;
                int j = right - 1;
                while (true)
                {
                    while (crowdingDistances[frontSortedIndex[++i]] < target)
                    {
                        if (i >= right - 1)
                            break;
                    }
                    if (i >= j)
                        break;
                    while (crowdingDistances[frontSortedIndex[--j]] > target)
                    {
                        if (j <= 0)
                            break;
                    }
                    if (i >= j)
                        break;
                    //swaping
                    Swap(ref frontSortedIndex[i], ref frontSortedIndex[j]);
                }
                //swaping
                Swap(ref frontSortedIndex[i], ref frontSortedIndex[right - 1]);

                FrontQuickSorting(crowdingDistances, frontSortedIndex, left, i);
                FrontQuickSorting(crowdingDistances, frontSortedIndex, i + 1, right);
            }
        }

        private void Swap(ref int a, ref int b)
        {
            int temp = a;
            a = b;
            b = temp;
        }





        public override void CreateFolder()
        {
			//base.CreateFolder();


            folderPath = Path.Combine(Project.ProjectPath, "Studies", studyName) + "\\";

            // Create a folder for the study
            Directory.CreateDirectory(folderPath);

            projectPath = $"{Project.ProjectPath}\\{Project.ProjectName}.explorer";


            #region All Points

            XMLMetadataFiler metadataAll = new XMLMetadataFiler(studyName + "All", folderPath, projectPath);

            metadataAll.AddAttribute("Type", "DesignsStudy");
			metadataAll.AddAttribute("WorkflowName", OptimisationTemplate.ExecutableComponent.Name);

			metadataAll.AddTag("Name", "Optimization");

			metadataAll.AddParameter(new IntegerData("ID"));

			foreach (Parameter parameter in OptimisationTemplate.Parameters)
				metadataAll.AddParameter(parameter.Data, "OptimizationParameter",
					("Value", parameter.Value));

			foreach (BoundedDesignVariableNoInital variable in OptimisationTemplate.DesignVariables)
				metadataAll.AddParameter(variable.Data, "OptimizationDesignVariable",
					("LowerBound", variable.LowerBound),
					("UpperBound", variable.UpperBound));

			foreach (Objective objective in OptimisationTemplate.Objectives)
				metadataAll.AddParameter(objective.Data, "OptimizationObjective",
					("Type", objective.Type));

			foreach (Constraint constraint in OptimisationTemplate.Constraints)
				metadataAll.AddParameter(constraint.Data, "OptimizationConstraint",
					("Type", constraint.Type),
					("Value", constraint.Value));

            metadataAll.Save();

            #endregion All Points







            #region Generations

            XMLMetadataFiler metadataGen = new XMLMetadataFiler(studyName + "Gen", folderPath, projectPath);

            metadataGen.AddAttribute("Type", "DesignsStudy");
            metadataGen.AddAttribute("WorkflowName", OptimisationTemplate.ExecutableComponent.Name);

            metadataGen.AddTag("Name", "Optimization");

            metadataGen.AddParameter(new IntegerData("ID"));
            metadataGen.AddParameter(new IntegerData("Generation"));

            foreach (Parameter parameter in OptimisationTemplate.Parameters)
                metadataGen.AddParameter(parameter.Data, "OptimizationParameter",
                    ("Value", parameter.Value));

            foreach (BoundedDesignVariableNoInital variable in OptimisationTemplate.DesignVariables)
                metadataGen.AddParameter(variable.Data, "OptimizationDesignVariable",
                    ("LowerBound", variable.LowerBound),
                    ("UpperBound", variable.UpperBound));

            foreach (Objective objective in OptimisationTemplate.Objectives)
                metadataGen.AddParameter(objective.Data, "OptimizationObjective",
                    ("Type", objective.Type));

            foreach (Constraint constraint in OptimisationTemplate.Constraints)
                metadataGen.AddParameter(constraint.Data, "OptimizationConstraint",
                    ("Type", constraint.Type),
                    ("Value", constraint.Value));

            metadataGen.Save();

            #endregion Generations
        }
    }
}
