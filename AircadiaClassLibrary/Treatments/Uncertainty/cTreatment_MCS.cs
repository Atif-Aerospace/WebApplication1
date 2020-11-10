using System;
using System.Collections.Generic;
using System.Linq;



using System.IO;

using Aircadia.ObjectModel;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Treatments;
using Aircadia.ObjectModel.Workflows;
using System.Data;

using System.Xml.Linq;

namespace Aircadia
{
	[Serializable()]
	public class Treatment_MCS : Treatment
    {
        public Workflow workflow;

        public List<string[]> RandomVariables = new List<string[]>();
        public List<Data> factors = new List<Data>();
        public List<Data> responses = new List<Data>();
        public List<string> Targets = new List<string>();

        public int Number_of_RV;
        public int Number_of_Re;
        public int Number_of_Sim;
        public bool Random_Switch;

        //results
        public double[][] ResultMatrixA;
        public double[] re_Mean;
        public double[] re_Std;


        /// <summary>
        /// Supply the Random Variables and Targets of the Sobol Study.
        /// </summary>
        public Treatment_MCS(List<string[]> RandomVariables, List<Data> factors, List<Data> Targets, bool Random_Switch)
			: base("Sobol", "Sobol")
        {
            this.RandomVariables = RandomVariables;
            this.factors = factors;
			responses = Targets;
            this.Random_Switch = Random_Switch;
			Number_of_RV = RandomVariables.Count;
			Number_of_Re = Targets.Count;
        }


        public override string ToString()
        {
            string output = "";
            output += "Random Variables: \r\n";

            foreach (string[] rv in RandomVariables)
            {
                output += rv[0] + "\r\n";
            }

            output += "Targets: \r\n";
            foreach (Data data in responses)
            {
                output += data.Name + "\r\n";
            }

            output += "Random Sampling: " + Random_Switch.ToString() + "\r\n";
            return output;
        }

        public override bool ApplyOn(ExecutableComponent ec)
        {
            if (ec is WorkflowComponent)
            {
				workflow = (Workflow)ec;
				ApplyOn();
            }
            return true;
        }




        public override bool ApplyOn()
        {
            bool status = true;

            //#region Inputs
            //var MyRV = new RandomVariable[Number_of_RV];                            //creat object array MyRV form RandomVariable class
            //double[][] SampleMatrix = new double[Number_of_RV][];                                //to store the samples



            ////fSobol_ExecutionSetupForm ses = new fSobol_ExecutionSetupForm();
            ////ses.ShowDialog();
            ////if (ses.numberOfSims > 0)
            ////{
            ////    Number_of_Sim = ses.numberOfSims;
            ////}
            ////else
            ////{
            ////    return false;
            ////}
            //Number_of_Sim = 1000;


            //ResultMatrixA = new double[Number_of_Sim][];                          //to store the results

            //var MyRand = new Random();

            //for (int i = 0; i < Number_of_RV; i++)                     //define the fields in MyRV 
            //{
            //    string[] rv_required = RandomVariables[i];

            //    //MyRV[i] = new cRandomVariable(RV[i, 0], RV[i, 1], RV[i, 2], RV[i, 3], RV[i, 4]);
            //    string[] vals_to_pass = new string[8] { "0", "0", "0", "0", "0", "0", "0", "PureRandom" };
            //    int counter = 0;
            //    foreach (string info in rv_required)
            //    {
            //        if (counter != 0)
            //        {
            //            vals_to_pass[counter - 1] = info;
            //        }
            //        counter++;

            //    }

            //    MyRV[i] = new RandomVariable(vals_to_pass[0], vals_to_pass[1], vals_to_pass[2], vals_to_pass[3], vals_to_pass[4], vals_to_pass[5], vals_to_pass[6], vals_to_pass[7]);
            //    SampleMatrix[i] = MyRV[i].GenSamples_PureRandom(Number_of_Sim, Random_Switch, MyRand);
            //    ///////////
            //    #region store the data to .csv
            //    string file_path = Project.ProjectPath + "\\temp_Sobol_RV_" + rv_required[0].ToString() + ".csv";
            //    var FileStream = new FileStream(file_path, FileMode.Append);
            //    var sw = new StreamWriter(FileStream, System.Text.Encoding.UTF8);
            //    for (int j = 0; j < Number_of_Sim; j++)
            //    {
            //        sw.WriteLine(SampleMatrix[i][j].ToString());
            //    }
            //    sw.Flush();
            //    sw.Close();
            //    FileStream.Close();
            //    #endregion
            //    //////////////////////
            //}

            ////constructing calculation matrix for sobol
            //double[,] A;                                  //Samples from fisrt group

            //try
            //{
            //    A = new double[Number_of_Sim, Number_of_RV];

            //}
            //catch (OutOfMemoryException ome)
            //{
            //    System.Console.WriteLine("Number of simulations too large. Error: " + ome.Message);
            //    return false;
            //}

            //for (int i = 0; i < (Number_of_Sim); i++)
            //{

            //    for (int j = 0; j < Number_of_RV; j++)
            //    {
            //        A[i, j] = SampleMatrix[j][i];

            //    }
            //}
            //#endregion Inputs



            ////// Prepare results database
            ////for (int i = 0; i < this.factors.Count; i++)
            ////{
            ////    this.Result.MinValues.Add((double)(this.startingValues[i]));
            ////    if (this.stepSizes == null)
            ////        this.Result.MaxValues.Add((double)(this.arr[i][this.arr[i].Count() - 1]));
            ////    else
            ////        this.Result.MaxValues.Add((double)(this.startingValues[i] + (this.noOfLevels[i] - 1) * stepSizes[i]));
            ////}
            ////foreach (Data data in this.responses)
            ////{
            ////    // Minimum and maximum values for result will be added later after execution of the workflow
            ////    this.responsesMinValues.Add(Double.PositiveInfinity);
            ////    this.responsesMaxValues.Add(Double.NegativeInfinity);
            ////}










            //string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(databaseFileName);
            //string connectionString;
            //connectionString = String.Format("DataSource=\"{0}\"", databaseFileName);

            //#region Create tables
            //var connection = new SqlCeConnection(connectionString);
            //if (connection.State == ConnectionState.Closed)
            //{
            //    connection.Open();
            //}
            //string createTableSQL = "create table " + fileNameWithoutExtension + " (ID int, ";
            //for (int i = 0; i < RandomVariables.Count(); i++)
            //{
            //    string columnHeader = RandomVariables[i][0];
            //    createTableSQL += columnHeader + " ";
            //    //if (this.RandomVariables[i] is IntegerData)
            //    //{
            //    //    createTableSQL += "int, ";
            //    //}
            //    //else if (this.RandomVariables[i] is DoubleData)
            //    //{
            //        createTableSQL += "float, ";
            //    //}
            //}
            //for (int i = 0; i < responses.Count(); i++)
            //{
            //    string columnHeader = responses[i].Name;
            //    createTableSQL += columnHeader + " ";
            //    //if ((this.Targets[i]) is IntegerData)
            //    //{
            //    //    createTableSQL += "int, ";
            //    //}
            //    //else if (this.Targets[i] is DoubleData)
            //    //{
            //        createTableSQL += "float, ";
            //    //}
            //}
            //if (RandomVariables.Count + responses.Count > 0)
            //{
            //    createTableSQL = createTableSQL.Remove(createTableSQL.Length - 2);
            //}
            //createTableSQL += ")";

            //// Create SQL create table command for "SQL Server Compact Edition"
            //var createTableSQLCmd = new SqlCeCommand(createTableSQL, connection);
            //try
            //{
            //    createTableSQLCmd.ExecuteNonQuery();
            //}
            //catch (SqlCeException sqlexception)
            //{
            //    Console.WriteLine(sqlexception.Message, "Oh Crap.");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.Message, "Oh Crap.");
            //}

            //#endregion







            //#region Execute and Insert into database

            //SqlCeCommand insertCmd = null;

            //string sql = "insert into " + fileNameWithoutExtension + " (ID, ";
            //string valuesString = "values (@ID, ";
            //for (int i = 0; i < RandomVariables.Count; i++)
            //{
            //    sql += RandomVariables[i][0] + ", ";
            //    valuesString += "@" + RandomVariables[i][0] + ", ";
            //}
            //for (int i = 0; i < responses.Count; i++)
            //{
            //    sql += responses[i].Name + ", ";
            //    valuesString += "@" + responses[i].Name + ", ";
            //}
            //if (RandomVariables.Count + responses.Count > 0)
            //{
            //    sql = sql.Remove(sql.Length - 2);
            //    valuesString = valuesString.Remove(valuesString.Length - 2);
            //}
            //sql += ")";
            //valuesString += ")";
            //sql += (" " + valuesString);


            //int tableID = 0;
            //for (int ii = 0; ii < Number_of_Sim; ii++)
            //{
            //    try
            //    {
            //        insertCmd = new SqlCeCommand(sql, connection);
            //        insertCmd.Parameters.AddWithValue("@ID", ++tableID);

            //        for (int i = 0; i < RandomVariables.Count; i++)
            //        {
            //            // Set factors as workflow data inputs
            //            Data workflowInput = workflow.ModelDataInputs.Find(delegate (Data d) { return d.Name == RandomVariables[i][0]; });
            //            if (workflowInput is IntegerData)
            //                workflowInput.Value = (int)A[ii, i];
            //            if (workflowInput is DoubleData)
            //                workflowInput.Value = (double)A[ii, i];
            //            // Update database insert command
            //            insertCmd.Parameters.AddWithValue("@" + RandomVariables[i][0], A[ii, i]);
            //        }



            //        // Execute workflow
            //        bool statusToCheck = workflow.Execute();



            //        double[] result_temp = new double[responses.Count];
            //        for (int i = 0; i < responses.Count; i++)
            //        {
            //            // Store workflow data outputs as responses
            //            Data workflowData = null;
            //            workflowData = workflow.ModelDataInputs.Find(delegate (Data d) { return d.Name == responses[i].Name; });
            //            if (workflowData == null)
            //            {
            //                workflowData = workflow.ModelDataOutputs.Find(delegate (Data d) { return d.Name == responses[i].Name; });
            //            }

            //            if (workflowData != null)
            //            {
            //                if (workflowData is IntegerData)
            //                {
            //                    int valueobject = (int)(workflowData.Value);
            //                    // Update database insert command
            //                    insertCmd.Parameters.AddWithValue("@" + responses[i].Name, valueobject);
            //                    result_temp[i] = valueobject;
            //                }
            //                if (workflowData is DoubleData)
            //                {
            //                    double valueobject = Convert.ToDouble(workflowData.Value);                      //atif and xin 29042016
            //                    // Update database insert command
            //                    insertCmd.Parameters.AddWithValue("@" + responses[i].Name, valueobject);
            //                    result_temp[i] = valueobject;
            //                }
            //            }

                        
            //        }

            //        ResultMatrixA[ii] = result_temp;

            //        // Execute database insert command
            //        if (statusToCheck)
            //            insertCmd.ExecuteNonQuery();



            //    }
            //    catch (SqlCeException sqlexception)
            //    {
            //        Console.WriteLine(sqlexception.Message, "Oh Crap.");
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine(ex.Message, "Oh Crap.");
            //    }
            //}


            //connection.Close();

            //#endregion  Execute and Insert into database





            //#region Last
            //double sum = 0;


            ////to store intermedient sum
            //double sum_sqr = 0;
            //double[] sum_con = new double[Number_of_RV];
            //double[] sum_con_T = new double[Number_of_RV];


            //re_Mean = new double[Number_of_Re];
            //double[] re_Variance = new double[Number_of_Re];
            //re_Std = new double[Number_of_Re];


            //for (int r = 0; r < Number_of_Re; r++)                                                                              //for rth result (output)
            //{
            //    sum = 0;
            //    sum_sqr = 0;
            //    for (int k = 0; k < Number_of_RV; k++)
            //    {
            //        sum_con[k] = 0;
            //        sum_con_T[k] = 0;
            //    }

            //    for (int i = 0; i < (Number_of_Sim / 2); i++)
            //    {
            //        sum = sum + ResultMatrixA[i][r];

            //        sum_sqr = sum_sqr + (ResultMatrixA[i][r]) * (ResultMatrixA[i][r]);


            //        ///////////

            //        #region store the data to .csv

            //        string file_path = Project.ProjectPath + "\\" + Name + "_temp_Sobol_res.csv";
            //        var FileStream = new FileStream(file_path, FileMode.Append);
            //        var sw = new StreamWriter(FileStream, System.Text.Encoding.UTF8);
            //        for (int j = 0; j < responses.Count; j++)
            //        {
            //            sw.WriteLine(ResultMatrixA[i][j].ToString());
            //        }
            //        sw.Flush();
            //        sw.Close();
            //        FileStream.Close();

            //        #endregion
            //    }
            //    re_Mean[r] = sum / (Number_of_Sim / 2);
            //    re_Variance[r] = sum_sqr / (Number_of_Sim / 2) - (re_Mean[r]) * (re_Mean[r]);                                //sobol's original approach (satelli's approach to be added)
            //    re_Std[r] = Math.Sqrt(re_Variance[r]);


            //}

            //#endregion Last

            return status;
        }




        public override void CreateFolder()
        {
            // Create a folder for the study
            string studyDirectory = System.IO.Path.Combine(Project.ProjectPath, "Studies", studyName);
            Directory.CreateDirectory(studyDirectory);



            var metadata = new XDocument();

            var resultElement = new XElement("Result");
            resultElement.Add(new XAttribute("Type", "DesignsStudy"));


            var parametersElement = new XElement("Parameters");


            var idParameterElement = new XElement("Parameter");
            idParameterElement.Add(new XAttribute("Name", "ID"));
            idParameterElement.Add(new XAttribute("Type", "Integer"));
            idParameterElement.Add(new XAttribute("Unit", ""));
            parametersElement.Add(idParameterElement);

            for (int i = 0; i < factors.Count; i++)
            {
                var parameterElement = new XElement("Parameter");
                string columnHeader = factors[i].Name;
                parameterElement.Add(new XAttribute("Name", columnHeader));
                if (factors[i] is IntegerData)
                {
                    parameterElement.Add(new XAttribute("Type", "Integer"));
                    parameterElement.Add(new XAttribute("Unit", ((IntegerData)factors[i]).Unit));
                }
                else if (factors[i] is DoubleData)
                {
                    parameterElement.Add(new XAttribute("Type", "Double"));
                    parameterElement.Add(new XAttribute("DecimalPlaces", ((DoubleData)factors[i]).DecimalPlaces));
                    parameterElement.Add(new XAttribute("Unit", ((DoubleData)factors[i]).Unit));
                }
                //XElement fullFactorialFactorElement = new XElement("FullFactorialFactor");
                //fullFactorialFactorElement.Add(new XAttribute("StartingValue", this.startingValues[i]));
                //fullFactorialFactorElement.Add(new XAttribute("StepSize", this.stepSizes[i]));
                //fullFactorialFactorElement.Add(new XAttribute("NoOfLevels", this.noOfLevels[i]));
                //parameterElement.Add(fullFactorialFactorElement);
                parametersElement.Add(parameterElement);
            }
            for (int i = 0; i < responses.Count; i++)
            {
                var parameterElement = new XElement("Parameter");
                string columnHeader = responses[i].Name;
                parameterElement.Add(new XAttribute("Name", columnHeader));
                if (responses[i] is IntegerData)
                {
                    parameterElement.Add(new XAttribute("Type", "Integer"));
                    parameterElement.Add(new XAttribute("Unit", ((IntegerData)responses[i]).Unit));
                }
                else if (responses[i] is DoubleData)
                {
                    parameterElement.Add(new XAttribute("Type", "Double"));
                    parameterElement.Add(new XAttribute("DecimalPlaces", ((DoubleData)responses[i]).DecimalPlaces));
                    parameterElement.Add(new XAttribute("Unit", ((DoubleData)responses[i]).Unit));
                }
                else if (responses[i] is DoubleVectorData)
                {
                    parameterElement.Add(new XAttribute("Type", "DoubleArray"));
                    parameterElement.Add(new XAttribute("DecimalPlaces", ((DoubleData)responses[i]).DecimalPlaces));
                    parameterElement.Add(new XAttribute("Unit", ((DoubleData)responses[i]).Unit));
                }
                //XElement fullFactorialFactorElement = new XElement("FullFactorialResponse");
                //parameterElement.Add(fullFactorialFactorElement);
                parametersElement.Add(parameterElement);
            }

            resultElement.Add(parametersElement);

            metadata.Add(resultElement);

            string resFilePath = System.IO.Path.Combine(Project.ProjectPath, "Studies", studyName, studyName + ".xml");
            metadata.Save(resFilePath);
        }

    }
}
