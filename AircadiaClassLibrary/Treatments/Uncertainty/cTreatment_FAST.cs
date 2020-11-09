using System;
using System.Collections.Generic;
using System.Linq;




using System.IO;
using System.Data;
using System.Data.SqlServerCe;

using Aircadia.ObjectModel;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Models;
using Aircadia.ObjectModel.Treatments;
using System.Xml.Linq;
using Aircadia.ObjectModel.Workflows;

namespace Aircadia
{
	[Serializable()]
    public class Treatment_FAST : Treatment
    {
        public Workflow workflow;

        public List<string[]> RandomVariables = new List<string[]>();
        public List<Data> factors = new List<Data>();
        public List<Data> responses = new List<Data>();


        //results
        private double[][] ResultMatrix;
        private double[] re_Mean;
        private double[] re_Std;
        private double[,] D_first;
        private double[,] D_total;


        /// <summary>
        /// Supply the Random Variables and Targets of the Sobol Study.
        /// </summary>
        public Treatment_FAST(List<string[]> RandomVariables, List<Data> factors, List<Data> responses)
			: base("FAST", "FAST")
        {
            this.RandomVariables = RandomVariables;
            this.factors = factors;
			this.responses = responses;
        }


        public override string ToString()
        {
            
            string output = "";
            if (responses != null)
            {
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
            }

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
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(databaseFileName);
            string connectionString;
            connectionString = String.Format("DataSource=\"{0}\"", databaseFileName);

            #region Create tables
            var connection = new SqlCeConnection(connectionString);
            if (connection.State == ConnectionState.Closed)
            {
                connection.Open();
            }
            string createTableSQL = "create table " + fileNameWithoutExtension + " (ID int, ";
            for (int i = 0; i < RandomVariables.Count(); i++)
            {
                string columnHeader = RandomVariables[i][0];
                createTableSQL += columnHeader + " ";
                //if (this.RandomVariables[i] is IntegerData)
                //{
                //    createTableSQL += "int, ";
                //}
                //else if (this.RandomVariables[i] is DoubleData)
                //{
                createTableSQL += "float, ";
                //}
            }
            for (int i = 0; i < responses.Count(); i++)
            {
                string columnHeader = responses[i].Name;
                createTableSQL += columnHeader + " ";
                //if ((this.Targets[i]) is IntegerData)
                //{
                //    createTableSQL += "int, ";
                //}
                //else if (this.Targets[i] is DoubleData)
                //{
                createTableSQL += "float, ";
                //}
            }
            if (RandomVariables.Count + responses.Count > 0)
            {
                createTableSQL = createTableSQL.Remove(createTableSQL.Length - 2);
            }
            createTableSQL += ")";

            // Create SQL create table command for "SQL Server Compact Edition"
            var createTableSQLCmd = new SqlCeCommand(createTableSQL, connection);
            try
            {
                createTableSQLCmd.ExecuteNonQuery();
            }
            catch (SqlCeException sqlexception)
            {
                Console.WriteLine(sqlexception.Message, "Oh Crap.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "Oh Crap.");
            }

            #endregion




            var Omiga = new List<int>();
            //generate frequencies    based on Flavio Cannavo's code and ‘A computational implementation of FAST’ [McRae et al.]
            if (this.RandomVariables.Count == 2)
            {
                Omiga.Add(5);
                Omiga.Add(9);
            }
            else if (this.RandomVariables.Count == 3)
            {
                Omiga.Add(1);
                Omiga.Add(9);
                Omiga.Add(15);
            }
            else
            {
                int[] F = { 0, 3, 1, 5, 11, 1, 17, 23, 19, 25, 41, 31,
                              23, 87, 67, 73, 85, 143, 149, 99, 119, 237,
                              267, 283, 151, 385, 157, 215, 449, 163, 337,
                              253, 375, 441, 673, 773, 875, 873, 587, 849,
                              623, 637, 891, 943, 1171, 1225, 1335, 1725, 1663, 2019 };

                int[] DN = { 4,8,6,10,20,22,32,40,38,26,56,62,46,76,96,60,
                               86,126,134,112,92,128,154,196,34,416,106,
                               208,328,198,382,88,348,186,140,170,284,
                               568,302,438,410,248,448,388,596,216,100,488,166,0};


                
                Omiga.Add(F[this.RandomVariables.Count - 1]);
                for (int i = 2; i <= this.RandomVariables.Count; i++)   // the ith input following nature numbering, when i = 1, the Omiga has already been chosen
                {
                    Omiga.Add(Omiga[(i - 1) - 1] + DN[(this.RandomVariables.Count + 1) - i - 1]);   // equivalent to n+1-i  ‘A computational implementation of FAST’ [McRae et al.]
                }


            }

            int M = 4;   //free to interference up up to M order

            /*fFAST_Set_M sem = new fFAST_Set_M();
            sem.ShowDialog();
            M = sem.orderM;*/



            int Omiga_max = Omiga.Max();
            int N_s = 2 * M * Omiga_max + 1;   //number of runs



            var s = new List<double>();
            for (int k = 1; k <= N_s; k++)
            {
                s.Add(Math.PI * (2 * k - N_s - 1) / N_s);
            }


            double[][] SampleMatrix = new double[this.RandomVariables.Count][];                                //to store the samples

            var MyRV = new RandomVariable[this.RandomVariables.Count];                            //creat object array MyRV form RandomVariable class




            ResultMatrix = new double[N_s][];

            for (int i = 0; i < this.RandomVariables.Count; i++)                     //define the fields in MyRV 
            {
                string[] rv_required = RandomVariables[i];

                //MyRV[i] = new cRandomVariable(RV[i, 0], RV[i, 1], RV[i, 2], RV[i, 3], RV[i, 4]);
                string[] vals_to_pass = new string[8] { "0", "0", "0", "0", "0", "0", "0", "FAST" };
                int counter = 0;
                foreach (string info in rv_required)
                {
                    if (counter != 0)
                    {
                        vals_to_pass[counter - 1] = info;
                    }
                    counter++;

                }

                MyRV[i] = new RandomVariable(vals_to_pass[0], vals_to_pass[1], vals_to_pass[2], vals_to_pass[3], vals_to_pass[4], vals_to_pass[5], vals_to_pass[6], vals_to_pass[7]);
                SampleMatrix[i] = MyRV[i].GenSamples_FAST(N_s, Omiga[i], s);
            }













            #region Database table
            SqlCeCommand insertCmd = null;

            string sql = "insert into " + fileNameWithoutExtension + " (ID, ";
            string valuesString = "values (@ID, ";
            for (int i = 0; i < RandomVariables.Count; i++)
            {
                sql += RandomVariables[i][0] + ", ";
                valuesString += "@" + RandomVariables[i][0] + ", ";
            }
            for (int i = 0; i < responses.Count; i++)
            {
                sql += responses[i].Name + ", ";
                valuesString += "@" + responses[i].Name + ", ";
            }
            if (RandomVariables.Count + responses.Count > 0)
            {
                sql = sql.Remove(sql.Length - 2);
                valuesString = valuesString.Remove(valuesString.Length - 2);
            }
            sql += ")";
            valuesString += ")";
            sql += (" " + valuesString);
            #endregion Database table


            int tableID = 0;
			IterationSize = N_s;
            for (int ii = 0; ii < N_s; ii++)
            {
				EndIteratoinIfCancelled();

                try
                {
                    insertCmd = new SqlCeCommand(sql, connection);
                    insertCmd.Parameters.AddWithValue("@ID", ++tableID);

                    for (int i = 0; i < RandomVariables.Count; i++)
                    {
                        // Set factors as workflow data inputs
                        Data workflowInput = workflow.ModelDataInputs.Find(delegate (Data d) { return d.Name == RandomVariables[i][0]; });
                        if (workflowInput is IntegerData)
                            workflowInput.Value = (int)SampleMatrix[i][ii];
                        if (workflowInput is DoubleData)
                            workflowInput.Value = (double)SampleMatrix[i][ii];
                        // Update database insert command
                        insertCmd.Parameters.AddWithValue("@" + RandomVariables[i][0], SampleMatrix[i][ii]);
                    }


                    // Execute workflow
                    bool statusToCheck = workflow.Execute();

                    double[] result_temp = new double[responses.Count];
                    for (int i = 0; i < responses.Count; i++)
                    {
                        // Store workflow data outputs as responses
                        Data workflowData = null;
                        workflowData = workflow.ModelDataInputs.Find(delegate (Data d) { return d.Name == responses[i].Name; });
                        if (workflowData == null)
                        {
                            workflowData = workflow.ModelDataOutputs.Find(delegate (Data d) { return d.Name == responses[i].Name; });
                        }

                        if (workflowData != null)
                        {
                            if (workflowData is IntegerData)
                            {
                                int valueobject = (int)(workflowData.Value);
                                // Update database insert command
                                insertCmd.Parameters.AddWithValue("@" + responses[i].Name, valueobject);
                                result_temp[i] = valueobject;
                            }
                            if (workflowData is DoubleData)
                            {
                                double valueobject = Convert.ToDouble(workflowData.Value);                      //atif and xin 29042016
                                // Update database insert command
                                insertCmd.Parameters.AddWithValue("@" + responses[i].Name, valueobject);
                                result_temp[i] = valueobject;
                            }
                        }

                        ResultMatrix[ii] = result_temp;
                    }

					// Report progress
					ReportProgress(ii);

					// Execute database insert command
					if (statusToCheck)
                        insertCmd.ExecuteNonQuery();
                }
                catch (SqlCeException sqlexception)
                {
                    Console.WriteLine(sqlexception.Message, "Oh Crap.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message, "Oh Crap.");
                }
                
                
                

            }

            connection.Close();

            #region store the data to .csv

            string file_path = Project.ProjectPath + "\\" + Name + "_temp_FAST_res.csv";
            var FileStream = new FileStream(file_path, FileMode.Append);
            var sw = new StreamWriter(FileStream, System.Text.Encoding.UTF8);
            for (int i = 0; i < ResultMatrix.Length; i++)
            {
                for (int j = 0; j < responses.Count; j++)
                {
                    sw.WriteLine(ResultMatrix[i][j].ToString());
                }
            }
            sw.Flush();
            sw.Close();
            FileStream.Close();

            #endregion



            var A = new List<double>();
            var B = new List<double>();
            var Lambda = new List<double>();

            double[] re_Variance = new double[this.responses.Count];
            re_Std = new double[this.responses.Count];
            re_Mean = new double[this.responses.Count];

            int temp_counter;
            D_first = new double[this.responses.Count, this.RandomVariables.Count];
            D_total = new double[this.responses.Count, this.RandomVariables.Count];

            for (int ReIndx = 0; ReIndx < this.responses.Count; ReIndx++)
            {

                re_Variance[ReIndx] = 0;
                Lambda.Clear();
                double sum = 0;

                //for (int j = -(N_s - 1) / 2; j <= (N_s - 1) / 2; j++)   in "A Quantitative Model-Independent Method for Global Sensitivity Analysis of Model Output"
                for (int j = 1; j <= (N_s - 1) / 2; j++)     //seems -(N_s - 1) / 2 ~ 0 are not used
                {

                    double tempFA = 0;
                    double tempFB = 0;


                    for (int k = 0; k < N_s; k++)
                    {
                        tempFA = tempFA + ResultMatrix[k][ReIndx] * Math.Cos(j * s[k]);
                        tempFB = tempFB + ResultMatrix[k][ReIndx] * Math.Sin(j * s[k]);
                    }
                    tempFA = tempFA / N_s;
                    tempFB = tempFB / N_s;



                    A.Add(tempFA);
                    B.Add(tempFB);

                    Lambda.Add(Math.Pow(tempFA, 2) + Math.Pow(tempFB, 2));
                    re_Variance[ReIndx] = re_Variance[ReIndx] + (Math.Pow(tempFA, 2) + Math.Pow(tempFB, 2));

                }
                re_Variance[ReIndx] = 2 * re_Variance[ReIndx];
                re_Std[ReIndx] = Math.Sqrt(re_Variance[ReIndx]);


                for (int k = 0; k < N_s; k++)
                {
                    sum = sum + ResultMatrix[k][ReIndx];
                }
                re_Mean[ReIndx] = sum / N_s;

                for (int InIndx = 0; InIndx < this.RandomVariables.Count; InIndx++)
                {
                    D_first[ReIndx, InIndx] = 0;
                    D_total[ReIndx, InIndx] = 0;   //temp
                    for (int p = 1; p <= M; p++)
                    {
                        temp_counter = p * Omiga[InIndx] - 1;
                        D_first[ReIndx, InIndx] = D_first[ReIndx, InIndx] + Lambda[temp_counter];
                    }
                    D_first[ReIndx, InIndx] = 2 * D_first[ReIndx, InIndx] / re_Variance[ReIndx];




                }

            }








            string ddd = Path.GetDirectoryName(databaseFileName);
            string sss = Path.GetFileNameWithoutExtension(databaseFileName) + "SA.sdf";
            string saFile = Path.Combine(ddd, sss);

            #region Create database
            string ff = System.IO.Path.Combine(Project.ProjectPath, saFile); // Microsoft SQL server compact edition file
            string connectionStringSA;
            if (File.Exists(ff))
            {
                File.Delete(ff);
            }
            connectionStringSA = String.Format("DataSource=\"{0}\"", ff);
            var engine = new SqlCeEngine(connectionStringSA);
            engine.CreateDatabase();
            #endregion



           

            #region Create tables
            var connectionSA = new SqlCeConnection(connectionStringSA);
            if (connectionSA.State == ConnectionState.Closed)
            {
                connectionSA.Open();
            }
            string createTableSASQL = "create table " + (fileNameWithoutExtension + "SA") + " (ID int, Target nvarchar(32), ";
            for (int i = 0; i < RandomVariables.Count(); i++)
            {
                string columnHeader = RandomVariables[i][0];
                createTableSASQL += columnHeader + " ";
                //if (this.RandomVariables[i] is IntegerData)
                //{
                //    createTableSASQL += "int, ";
                //}
                //else if (this.RandomVariables[i] is DoubleData)
                //{
                createTableSASQL += "float, ";
                //}
            }
            createTableSASQL += "Interactions float, ";


            if (RandomVariables.Count > 0)
            {
                createTableSASQL = createTableSASQL.Remove(createTableSASQL.Length - 2);
            }
            createTableSASQL += ")";

            // Create SQL create table command for "SQL Server Compact Edition"
            var createTableSASQLCmd = new SqlCeCommand(createTableSASQL, connectionSA);
            try
            {
                createTableSASQLCmd.ExecuteNonQuery();
            }
            catch (SqlCeException sqlexception)
            {
                Console.WriteLine(sqlexception.Message, "Oh Crap.");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message, "Oh Crap.");
            }

            #endregion





            insertCmd = null;

            sql = "insert into " + (fileNameWithoutExtension + "SA") + " (ID, Target, ";
            valuesString = "values (@ID, @Target, ";
            for (int i = 0; i < RandomVariables.Count; i++)
            {
                sql += RandomVariables[i][0] + ", ";
                valuesString += "@" + RandomVariables[i][0] + ", ";
            }
            sql += "Interactions, ";
            valuesString += "@Interactions, ";
            if (RandomVariables.Count > 0)
            {
                sql = sql.Remove(sql.Length - 2);
                valuesString = valuesString.Remove(valuesString.Length - 2);
            }
            sql += ")";
            valuesString += ")";
            sql += (" " + valuesString);




            tableID = 0;
            for (int ii = 0; ii < D_first.GetLength(0); ii++)
            {
                try
                {
                    insertCmd = new SqlCeCommand(sql, connectionSA);
                    insertCmd.Parameters.AddWithValue("@ID", ++tableID);
                    insertCmd.Parameters.AddWithValue("@Target", responses[ii].Name);

                    double single = 0.0;
                    for (int i = 0; i < D_first.GetLength(1); i++)
                    {
                        // Update database insert command
                        insertCmd.Parameters.AddWithValue("@" + RandomVariables[i][0], D_first[ii, i]);
                        single += D_first[ii, i];
                    }
                    insertCmd.Parameters.AddWithValue("@Interactions", (1 - single));

                    insertCmd.ExecuteNonQuery();
                }
                catch (SqlCeException sqlexception)
                {
                    Console.WriteLine(sqlexception.Message, "Oh Crap.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message, "Oh Crap.");
                }


            }


            connectionSA.Close();
            


            return true;
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












            #region SA
            var metadata2 = new XDocument();

            var resultElement2 = new XElement("Result");
            resultElement2.Add(new XAttribute("Type", "ParametersStudy"));


            var parametersElement2 = new XElement("Parameters");


            var idParameterElement2 = new XElement("Parameter");
            idParameterElement2.Add(new XAttribute("Name", "ID"));
            idParameterElement2.Add(new XAttribute("Type", "Integer"));
            idParameterElement2.Add(new XAttribute("Unit", ""));
            parametersElement2.Add(idParameterElement2);

            var targetParameterElement2 = new XElement("Parameter");
            targetParameterElement2.Add(new XAttribute("Name", "Target"));
            targetParameterElement2.Add(new XAttribute("Type", "String"));
            parametersElement2.Add(targetParameterElement2);

            for (int i = 0; i < factors.Count; i++)
            {
                var parameterElement2 = new XElement("Parameter");
                string columnHeader = factors[i].Name;
                parameterElement2.Add(new XAttribute("Name", columnHeader));
                if (factors[i] is IntegerData)
                {
                    parameterElement2.Add(new XAttribute("Type", "Integer"));
                    parameterElement2.Add(new XAttribute("Unit", ((IntegerData)factors[i]).Unit));
                }
                else if (factors[i] is DoubleData)
                {
                    parameterElement2.Add(new XAttribute("Type", "Double"));
                    parameterElement2.Add(new XAttribute("DecimalPlaces", ((DoubleData)factors[i]).DecimalPlaces));
                    parameterElement2.Add(new XAttribute("Unit", ((DoubleData)factors[i]).Unit));
                }
                //XElement fullFactorialFactorElement = new XElement("FullFactorialFactor");
                //fullFactorialFactorElement.Add(new XAttribute("StartingValue", this.startingValues[i]));
                //fullFactorialFactorElement.Add(new XAttribute("StepSize", this.stepSizes[i]));
                //fullFactorialFactorElement.Add(new XAttribute("NoOfLevels", this.noOfLevels[i]));
                //parameterElement2.Add(fullFactorialFactorElement);
                parametersElement2.Add(parameterElement2);
            }
            

            resultElement2.Add(parametersElement2);

            metadata2.Add(resultElement2);

            string resFilePath2 = System.IO.Path.Combine(Project.ProjectPath, "Studies", studyName, studyName + "SA.xml");
            metadata2.Save(resFilePath2);
            #endregion SA
        }
    }
}
