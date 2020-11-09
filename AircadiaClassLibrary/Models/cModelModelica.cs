//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;


//using System.IO;

//using System.Reflection;

//using System.Collections.ObjectModel;

//using Aircadia.ObjectModel.DataObjects;
//using Aircadia.UI.Models;


//namespace Aircadia.ObjectModel.Models
//{
//    [Serializable()]
//    public class ModelModelica : Model
//	{
//        public string EXEFilePath;
//        public string EXEArgs;

//        private readonly List<string> InitialOutputFileText = new List<string>();


//        public List<string> InputFilePath;
//        public List<string> OutputFilePath;

//        public List<Dictionary<string, ExeModelDataLocation>> inputLocations = new List<Dictionary<string, ExeModelDataLocation>>();
//        public List<Dictionary<string, ExeModelDataLocation>> outputLocations = new List<Dictionary<string, ExeModelDataLocation>>();
//		readonly string Path = "";

//		public override string ModelType => "Modelica";

//		public ModelModelica(string name, List<Data> inp, List<Data> outp, string path, string entry)
//            : base(name, "", inp, outp)
//        {
//			Path = path;

//            string absoluteScriptFileName = System.IO.Path.Combine(AircadiaProject.ProjectPath, path);
//            #region Create Script File

//            string initialNames = "initNames={";
//            string initialValues = "initValues={";
//            foreach (Data d in inp)
//            {
//                initialNames += ("\"" + d.Name + "\",");
//                if (d is IntegerData)
//                    initialValues += (int)(d.Value);
//                else if (d is DoubleData)
//                    initialValues += (double)(d.Value);
//                initialValues += ",";
//            }
//            initialNames = initialNames.TrimEnd(',');
//            initialNames += "};\n";
//            initialValues = initialValues.TrimEnd(',');
//            initialValues += "};\n";
//            string fName = "fNames={";
//            foreach (Data d in outp)
//            {
//                fName += ("\"" + d.Name + "\",");
//            }
//            fName = fName.TrimEnd(',');
//            fName += "};\n";

//            string scriptFileText = "" +
//            "modelFile=\"" + absoluteScriptFileName.Replace('\\','/') + "\";\n" + 
//            "modelName=\"" + entry + "\";\n" +
//            "resFile=\"" + System.IO.Path.Combine(Directory.GetParent(absoluteScriptFileName).FullName, "test1").Replace('\\', '/') + "\";\n" +
//            "CSVfile=\"" + System.IO.Path.Combine(Directory.GetParent(absoluteScriptFileName).FullName, "SelectedVariables.csv").Replace('\\', '/') + "\";\n" +

//            initialNames +
//            initialValues + 
//            fName + 

//            "openModel(modelFile);\n" + 

//            "simulateExtendedModel(modelName, initialNames=initNames, initialValues=initValues, finalNames=fNames, resultFile=resFile);\n" + 


//            // Read the size of the trajectories in the result file and
//            // store in 'n'
//            "n=readTrajectorySize(resFile+\"" + ".mat" + "\");\n" + 
//            // Define what variables should be included

//            // Read the trajectories 'names' (and store in 'traj')
//            "traj=readTrajectory(resFile+\"" + ".mat" + "\",fNames,n);\n" + 
//            // transpose traj
//            "traj_transposed=transpose(traj);\n" +
//            // write the .csv file using the package 'DataFiles'
//            "DataFiles.writeCSVmatrix(CSVfile, fNames, traj_transposed);\n" + 

//            "exit\n";


//            File.WriteAllText(System.IO.Path.Combine(Directory.GetParent(absoluteScriptFileName).FullName, Name + ".mos"), scriptFileText);

//            #endregion Create Script File

//            // Generate model code
//            PrepareForExecution();
//        }

//        public override void PrepareForExecution()
//        {
//			string lcCode = "";
//            string lcCodeIn = "";

            
//            foreach (Data dt in ModelDataInputs)
//            {
//                if (dt is DoubleData)
//                    lcCodeIn = lcCodeIn + "double " + dt.Name + ",";
//                else if (dt is DoubleVectorData)
//                    lcCodeIn = lcCodeIn + "double[] " + dt.Name + ",";
//                else if (dt is DoubleMatrixData)
//                    lcCodeIn = lcCodeIn + "double[,] " + dt.Name + ",";
//                else if (dt is IntegerData)
//                    lcCodeIn = lcCodeIn + "int " + dt.Name + ",";
//                else if (dt is IntegerVectorData)
//                    lcCodeIn = lcCodeIn + "int[] " + dt.Name + ",";
//            }
//            if (lcCodeIn != "")
//            {
//                //lcCodeIn = lcCodeIn.Remove(lcCodeIn.Length - 1);
//                //lcCodeInNme = lcCodeInNme.Remove(lcCodeInNme.Length - 1);
//                lcCodeIn = lcCodeIn.Remove(lcCodeIn.Length - 1);
//            }
            


//            lcCode += "\t\tpublic object " + Name + "(" + lcCodeIn + ")\n";
//            lcCode += "\t\t{\n";












//            /*
//            // initialize the object attributes
//            lcCode += "\t\t\tstring initParam = File.ReadAllText(\"C:/Cranfield/AtifWork/initialParameters.ini\", Encoding.Default);\n";

//            // prepare for .mos file HelloWorld.mos
//            lcCode += "\t\t\tstring mos = File.ReadAllText(\"C:/Cranfield/AtifWork/dllbatchmode.mos\", Encoding.Default);\n";
//            lcCode += "\t\t\tmos = mos.Replace(\"Variable__declarations\", initParam);\n";
//            lcCode += "\t\t\tFile.WriteAllText(\"C:/Cranfield/AtifWork/ttt.mos\", mos, Encoding.Default);\n";
//            */



//            #region Execution

//            lcCode += "\t\t\tSystem.Diagnostics.Process p = new System.Diagnostics.Process();\n";
//            lcCode += "\t\t\tp.StartInfo.UseShellExecute = false;\n";
//            //p.StartInfo.RedirectStandardOutput = true;
//            lcCode += "\t\t\tp.StartInfo.FileName = @\"C:/Program Files (x86)/Dymola 2015/bin64/dymola.exe\";\n";
//            //lcCode += "p.StartInfo.Arguments = \"/nowindow \" + \"C:/Cranfield/AtifWork/ttt.mos\";";
//            string absoluteScriptFileName = System.IO.Path.Combine(AircadiaProject.ProjectPath, Path);
//            lcCode += "\t\t\tstring fn = @\"" + System.IO.Path.Combine(System.IO.Directory.GetParent(absoluteScriptFileName).FullName, Name + ".mos") + "\";\n";
//            lcCode += "\t\t\tp.StartInfo.Arguments = \"\\\"\" + fn + \"\\\"\";\n";
//            lcCode += "\t\t\tp.Start();\n";
//            lcCode += "\t\t\tp.WaitForExit();\n";

//            #endregion Execution




//            #region Outputs

//            lcCode += "\t\t\tstring CSVFilePathName = @\"" + System.IO.Path.Combine(System.IO.Directory.GetParent(absoluteScriptFileName).FullName, "SelectedVariables.csv") + "\";\n";
//            lcCode += "\t\t\tstring[] Lines = File.ReadAllLines(CSVFilePathName);\n";
//            lcCode += "\t\t\tstring[] Fields;\n";
//            lcCode += "\t\t\tFields = Lines[0].Split(new char[] { ';' });\n";
//            lcCode += "\t\t\tint Cols = Fields.GetLength(0);\n";

//            lcCode += "\t\t\tdouble[,] data = new double[Lines.Length - 1, Cols];\n";
//            lcCode += "\t\t\tfor (int j = 1; j < Lines.GetLength(0); j++)\n";
//            lcCode += "\t\t\t{\n";
//            lcCode += "\t\t\t\tFields = Lines[j].Split(new char[] { ';' });\n";
//            lcCode += "\t\t\t\tfor (int f = 0; f < Cols; f++)\n";
//            lcCode += "\t\t\t\t\tdata[j - 1,f] = Convert.ToDouble(Fields[f]);\n";
//            lcCode += "\t\t\t}\n";


//            lcCode += "\t\t\tobject[] outputs_all=new object[" + Convert.ToString(ModelDataOutputs.Count) + "];\n";
//            lcCode += "\t\t\tfor (int i = 0; i < Cols; i++)\n";
//            lcCode += "\t\t\t{\n";
//            if (false) // Vector (array) outputs
//            {
//                lcCode += "\t\t\t\tdouble[] temp = new double[data.GetLength(0)];\n";
//                lcCode += "\t\t\t\tfor (int j = 0; j < data.GetLength(0); j++)\n";
//                lcCode += "\t\t\t\t{\n";
//                lcCode += "\t\t\t\t\ttemp[j] = data[j, i];\n";
//                lcCode += "\t\t\t\t}\n";
//            }
//            else if (true) // Scalar outputs
//            {
//                lcCode += "\t\t\t\tdouble temp = data[0, i];\n";
//            }
//            lcCode += "\t\t\t\toutputs_all[i] = " + "temp;\n";
//            lcCode += "\t\t\t}\n";

//            lcCode += "\t\t\tobject outputs_return=outputs_all;\n";
//            lcCode += "\t\t\treturn outputs_return;\n";
//            #endregion Outputs

//            lcCode += "\t\t}\n";



//			ExecutableCode = lcCode;
































            

//            /*
//            executableCode = string.Empty;


//            //Creates the DLLS for model
//            bool status = true;
//            string lcCode = "";
//            string lcCodeIn = "";
//            string lcCodeInOnly = "";
//            string lcCodeInNme = "";

//            #region Inputs

//            foreach (Data dt in ModelDataInputs)
//            {
//                if (dt is DoubleData)
//                    lcCodeInOnly = lcCodeInOnly + "double " + dt.Name + ",";
//                else if (dt is DoubleVectorData)
//                    lcCodeInOnly = lcCodeInOnly + "double[] " + dt.Name + ",";
//                else if (dt is IntegerData)
//                    lcCodeInOnly = lcCodeInOnly + "int " + dt.Name + ",";
//                else if (dt is IntegerVectorData)
//                    lcCodeInOnly = lcCodeInOnly + "int[] " + dt.Name + ",";
//            }
//            if (lcCodeInOnly != "")
//                lcCodeInOnly = lcCodeInOnly.Remove(lcCodeInOnly.Length - 1);


//            lcCode += "\t\tpublic object " + this.Name + "(" + lcCodeInOnly + ")\n";
//            lcCode += "\t\t{\n";

//            lcCode += "\t\t\tobject[] dataValues = new object[" + this.ModelDataInputs.Count + "];\n";
//            for (int j = 0; j < this.ModelDataInputs.Count; j++)
//                lcCode += "\t\t\tdataValues[" + j + "] = " + this.ModelDataInputs[j].Name + ";\n";
//            lcCode += "\n";




//            lcCode += "\t\t\tList<List<string>> initialFileText = new List<List<string>>();\n";
//            lcCode += "\t\t\tList<List<string>> runFileText = new List<List<string>>();\n";
//            int inputDataCounter = 0;
//            for (int i = 0; i < this.InputFilePath.Count; i++)
//            {
//                // Write input data values to input file
//                lcCode += "\t\t\tinitialFileText.Add(new List<string>());\n";
//                lcCode += "\t\t\trunFileText.Add(new List<string>());\n";
//                lcCode += "\t\t\tforeach (string str in System.IO.File.ReadAllLines(@\"" + System.IO.Path.Combine(AircadiaProject.ProjectPath, InputFilePath[i]) + "\"))\n";
//                lcCode += "\t\t\t{\n";
//                lcCode += "\t\t\t\tinitialFileText[" + i + "].Add(str);\n";
//                lcCode += "\t\t\t\trunFileText[" + i + "].Add(str);\n";
//                lcCode += "\t\t\t}\n\n";


//                lcCode += "\t\t\t// Inputs\n";
//                for (int j = 0; j < this.inputLocations.Count; j++)
//                {
//                    foreach (ExeModelDataLocation dataLocation in this.inputLocations[j].Values)
//                    {
//                        if (dataLocation.fileIndex == i)
//                        {
//                            if (this.ModelDataInputs[inputDataCounter] is DoubleData)
//                                lcCode += "\t\t\trunFileText[" + i + "][" + dataLocation.rowIndex + "] = runFileText[" + i + "][" + dataLocation.rowIndex + "].Remove(" + dataLocation.startColumnIndex + ", " + dataLocation.endColumnIndex + " - " + dataLocation.startColumnIndex + " + 1).Insert(" + dataLocation.startColumnIndex + ", FormatNumDigits((double)dataValues[" + inputDataCounter + "], " + (dataLocation.endColumnIndex - dataLocation.startColumnIndex + 1 - 2).ToString() + "));\n";
//                            else if (this.ModelDataInputs[inputDataCounter] is IntegerData)
//                                lcCode += "\t\t\trunFileText[" + i + "][" + dataLocation.rowIndex + "] = runFileText[" + i + "][" + dataLocation.rowIndex + "].Remove(" + dataLocation.startColumnIndex + ", " + dataLocation.endColumnIndex + " - " + dataLocation.startColumnIndex + " + 1).Insert(" + dataLocation.startColumnIndex + ", FormatNumDigits((int)(inputs[" + inputDataCounter + "], " + (dataLocation.endColumnIndex - dataLocation.startColumnIndex + 1 - 2).ToString() + "));\n";
//                            inputDataCounter++;
//                        }
//                    }
//                }

//                lcCode += "\t\t\tusing (System.IO.StreamWriter w = new System.IO.StreamWriter(@\"" + System.IO.Path.Combine(AircadiaProject.ProjectPath, InputFilePath[i]) + "\"))\n";
//                lcCode += "\t\t\t{\n";
//                lcCode += "\t\t\t\tforeach (string line in runFileText[" + i + "])\n";
//                lcCode += "\t\t\t\t\tw.WriteLine(line);\n";
//                lcCode += "\t\t\t\tw.Close();\n";
//                lcCode += "\t\t\t}\n\n";
//            }
//            #endregion Inputs

//            #region Execution
//            //lcCode += "\t\t\t#region Execution\n\n";
//            lcCode += "\t\t\tSystem.Diagnostics.Process proc = new System.Diagnostics.Process();\n";
//            lcCode += "\t\t\tproc.StartInfo.WorkingDirectory = System.IO.Path.GetDirectoryName(@\"" + this.EXEFilePath + "\");\n";
//            lcCode += "\t\t\tproc.StartInfo.FileName = @\"" + System.IO.Path.Combine(AircadiaProject.ProjectPath, this.EXEFilePath) + "\";\n";
//            lcCode += "\t\t\tproc.StartInfo.Arguments = @\"" + this.EXEArgs + "\";\n";
//            lcCode += "\t\t\tproc.StartInfo.UseShellExecute = true;\n";
//            lcCode += "\t\t\tproc.StartInfo.RedirectStandardError = false;\n";
//            lcCode += "\t\t\tproc.Start();\n";
//            lcCode += "\t\t\tproc.WaitForExit();\n\n";

            
//            //lcCode += "\t\t\t// Reset input files\n";
//            //lcCode += "\t\t\tusing (System.IO.StreamWriter w = new System.IO.StreamWriter(@\"" + System.IO.Path.Combine(AircadiaProject.ProjectPath, InputFilePath) + "\"))\n";
//            //lcCode += "\t\t\t{\n";
//            //lcCode += "\t\t\t\tforeach (string line in initialFileText)\n";
//            //lcCode += "\t\t\t\t\tw.WriteLine(line);\n";
//            //lcCode += "\t\t\t\tw.Close();\n";
//            //lcCode += "\t\t\t}\n";
            
//            lcCode += "\t\t\tproc.Close();\n\n";
//            //lcCode += "\t\t\t#endregion Execution\n\n";
//            #endregion Execution


//            #region Outputs
//            for (int j = 0; j < this.outputLocations.Count; j++)
//            {
//                // Write input data values to input file
//                lcCode += "\t\t\tList<string> outputFileText = new List<string>();\n";
//                lcCode += "\t\t\tforeach (string str in System.IO.File.ReadAllLines(@\"" + System.IO.Path.Combine(AircadiaProject.ProjectPath, OutputFilePath[j]) + "\"))\n";
//                lcCode += "\t\t\t{\n";
//                lcCode += "\t\t\t\toutputFileText.Add(str);\n";
//                lcCode += "\t\t\t}\n";


//                lcCode += "\t\t\tobject[] outputs_all=new object[" + Convert.ToString(ModelDataOutputs.Count) + "];\n";
//                int ncount = 0;

//                foreach (ExeModelDataLocation dataLocation in this.outputLocations[j].Values)
//                {
//                    lcCode += "\t\t\toutputs_all[" + Convert.ToString(ncount) + "] = Convert.ToDouble(outputFileText[" + dataLocation.rowIndex + "].Substring(" + dataLocation.startColumnIndex + ", " + dataLocation.endColumnIndex + " - " + dataLocation.startColumnIndex + " + 1));\n";
//                    ncount++;
//                }
//            }
//            lcCode += "\t\t\tobject outputs_return = outputs_all;\n";
//            lcCode += "\t\t\treturn outputs_return;\n";
//            #endregion Outputs


//            lcCode += "\t\t}\n";

//            this.executableCode = lcCode;
//            */


//        }



//        public override bool Execute()
//        {
            








//            // executes the dll of the models
//            bool status = true;
//            object[] initin = Data.GetValues(ModelDataInputs);
//            object[] initout = Data.GetValues(ModelDataOutputs);
//            Type t = AircadiaProject.assemblyacd.GetType(AircadiaProject.ProjectName + ".AircadiaProjectClass");
//            MethodInfo mi = t.GetMethod(Name);
//            object o = Activator.CreateInstance(t);
//            object[] inputs_vl = new object[ModelDataInputs.Count];
//            int ncount = 0;
//            foreach (Data dt in ModelDataInputs)
//            {
//                if ((dt as Data) is IntegerData)
//                {
//                    inputs_vl[ncount] = Convert.ToInt32(dt.Value);
//                    ncount++;
//                }
//                else
//                {
//                    inputs_vl[ncount] = dt.Value;
//                    ncount++;
//                }
//            }

//            string NewValues = "";
//            foreach (object ooo in inputs_vl)
//                NewValues += ooo.ToString() + ",";
//            NewValues = NewValues.TrimEnd(',');

//            string[] mos = File.ReadAllLines(System.IO.Path.Combine(Directory.GetParent(System.IO.Path.Combine(AircadiaProject.ProjectPath, Path)).FullName, Name + ".mos"), Encoding.Default);
//            mos[5] = "initValues={" + NewValues + "}";
//            File.WriteAllLines(System.IO.Path.Combine(Directory.GetParent(System.IO.Path.Combine(AircadiaProject.ProjectPath, Path)).FullName, Name + ".mos"), mos);

//            try
//            {
//                ncount = 0;
//                object loResult = mi.Invoke(o, inputs_vl);
//                //Console.WriteLine("Invoked!");;//DEBUGGING//
//                //Console.WriteLine(loResult as string);;//DEBUGGING//
//                foreach (Data dto in ModelDataOutputs)
//                {
//                    if ((loResult as object[])[ncount] is double)
//                    {
//                        if ((Double.IsNaN((double)(loResult as object[])[ncount])) | (Double.IsInfinity((double)(loResult as object[])[ncount])) | (Double.IsNegativeInfinity((double)(loResult as object[])[ncount])) | (Double.IsPositiveInfinity((double)(loResult as object[])[ncount])))
//                        {
//                            Console.WriteLine("Infinite Value in " + dto.Name + "\r\n" + " Execution may continue");
//                            status = false;
//                        }
//                    }
//                    dto.Value = (loResult as object[])[ncount];
//                    ncount++;
//                }
//            }
//            catch (Exception)
//			{

//                System.Windows.Forms.MessageBox.Show("Point Failed");
//                status = false;
//            }
//            if (status == false)
//            {
//                Console.WriteLine("Values of data reset to initial in model " + Name + "\r\n");
//                Data.SetValues(ModelDataInputs, initin);
//                Data.SetValues(ModelDataOutputs, initout);
//            }
//            return status;

//        }




        
//    }
//}
