using System;
using System.Collections.Generic;
using System.Reflection;
using Aircadia.ObjectModel.DataObjects;

namespace Aircadia.ObjectModel.Models
{
	[Serializable]
    public class DLLModel : Model
	{
		public string DLLFilePath { get; set; }
		public DLLModelTypes DLLModelType { get; set; }

		public override string ModelType => "DLL";


		public DLLModel(string name, List<Data> dataInputs, List<Data> dataOutputs, string dllFilePath, DLLModelTypes dllModelType, string parentName = "")
            : base(name, "", dataInputs, dataOutputs, parentName: parentName)
        {
            DLLFilePath = dllFilePath;
            DLLModelType = dllModelType;

            // Generate code of the current model for "aircadia project dll"
            PrepareForExecution();
        }


        public override void PrepareForExecution()
        {
            if (DLLModelType == DLLModelTypes.C)
            {
            }
            else if (DLLModelType == DLLModelTypes.CPP)
            {
            }
            else if (DLLModelType == DLLModelTypes.CS)
            {
				ExecutableCode = "";
				ExecutableCode += "\t\tpublic object " + Name + "(";
                foreach (Data inputData in ModelDataInputs)
                {
                    if (inputData is IntegerData)
                    {
						ExecutableCode += "int " + inputData.Name + ", ";
                    }
                    else if (inputData is DoubleData)
                    {
						ExecutableCode += "double " + inputData.Name + ", ";
                    }
                    else if (inputData is IntegerVectorData)
                    {
						ExecutableCode += "int[] " + inputData.Name + ", ";
                    }
                    else if (inputData is DoubleVectorData)
                    {
						ExecutableCode += "double[] " + inputData.Name + ", ";
                    }
                }
				ExecutableCode = ExecutableCode.TrimEnd(',', ' ');
				ExecutableCode += ")\n\t\t";

				ExecutableCode += "{\n";


				//DElMethis.DLLFilePath = "IRW1_RNG_RMP\\AircadiaCSLib\\AircadiaCSLib\\bin\\Debug\\AircadiaCSLib.dll";

				// Get type from the "aircadia project dll" (aircadia project dll assembly contains only one type, i.e. only one class) and then create object of that type
				AircadiaProject Project = AircadiaProject.Instance;
				ExecutableCode += "\t\t\tAssembly assembly = Assembly.LoadFile(@\"" + System.IO.Path.Combine(Project.ProjectPath, DLLFilePath) + "\");\n";
				ExecutableCode += "\t\t\tType[] aircadiaProjDLLTypes = assembly.GetTypes();\n";
				ExecutableCode += "\t\t\tobject aircadiaProjDLLObject = Activator.CreateInstance(aircadiaProjDLLTypes[0]);\n";



				// Get method object to be executed in "aircadia project dll"
				ExecutableCode += "\t\t\tMethodInfo methodInfo = aircadiaProjDLLTypes[0].GetMethod(\"" + Name + "\");\n";
				ExecutableCode += "\t\t\tMethodInfo[] methodInfoA = aircadiaProjDLLTypes[0].GetMethods();\n";

				// initialise input arguments for the method (arguments also contain outputs of the model)
				ExecutableCode += "\t\t\tobject[] inputArgs = new object[" + (ModelDataInputs.Count + ModelDataOutputs.Count) + "];\n";

                int ncount = 0;
                foreach (Data data in ModelDataInputs)
                {
                    if (data is IntegerData)
                    {
						ExecutableCode += "\t\t\tinputArgs[" + ncount + "] = " + ((IntegerData)data).Name + ";\n";
                    }
                    else if (data is DoubleData)
                    {
						ExecutableCode += "\t\t\tinputArgs[" + ncount + "] = " + ((DoubleData)data).Name + ";\n";
                    }
                    else if (data is IntegerVectorData)
                    {
						ExecutableCode += "\t\t\tinputArgs[" + ncount + "] = " + ((IntegerVectorData)data).Name + ";\n";
                    }
                    else if (data is DoubleVectorData)
                    {
						ExecutableCode += "\t\t\tinputArgs[" + ncount + "] = " + ((DoubleVectorData)data).Name + ";\n";
                    }
                    ncount++;
                }

				// Execute the model (call method of the Aircadia project dll)
				ExecutableCode += "\t\t\ttry\n";
				ExecutableCode += "\t\t\t{\n";
				ExecutableCode += "\t\t\t\tmethodInfo.Invoke(aircadiaProjDLLObject, inputArgs);\n";
				ExecutableCode += "\t\t\t}\n";
				ExecutableCode += "\t\t\tcatch (Exception e)\n";
				ExecutableCode += "\t\t\t{\n";
				ExecutableCode += "\t\t\t\tSystem.Console.WriteLine(e.Source);\n";
				ExecutableCode += "\t\t\t\tSystem.Console.WriteLine(e.StackTrace);\n";
				//this.ExecutableCode += "\t\t\t\tConsole.WriteLine(\"Execution of model (" + this.Name + ") failed in C# dll file " + Path.GetDirectoryName(this.DLLFilePath) + "\\" + this.dllFileName + ".dll\", \"DLL Model Execution Failed\", MessageBoxButton.OK, MessageBoxImage.Information);\n";
				//status = false;
				ExecutableCode += "\t\t\t}\n";

				// Output
				ExecutableCode += "\t\t\tobject modelResultObject = null;\n";
				ExecutableCode += "\t\t\tobject[] outputs = new object[" + ModelDataOutputs.Count + "];\n";
				ExecutableCode += "\t\t\tfor (int i = 0; i < " + ModelDataOutputs.Count + "; i++)\n";
				ExecutableCode += "\t\t\t{\n";
				ExecutableCode += "\t\t\t\toutputs[i] = inputArgs[" + ModelDataInputs.Count + " + i];\n";
				ExecutableCode += "\t\t\t}\n";
				ExecutableCode += "\t\t\tmodelResultObject = outputs;\n";
				ExecutableCode += "\t\t\treturn modelResultObject;\n";
				ExecutableCode += "\t\t}\n";
            }
            else if (DLLModelType == DLLModelTypes.FORTRAN)
            {
            }
            else if (DLLModelType == DLLModelTypes.MATLAB)
            {
            }
        }

        public override bool Execute()
        {
            bool status = true; // model execution status

			// Get type from the "aircadia project dll" (aircadia project dll assembly contains only one type, i.e. only one class) and then create object of that type
			AircadiaProject Project = AircadiaProject.Instance;
			//Type[] aircadiaProjDLLTypes = Project.assemblyacd.GetTypes();
			Type[] aircadiaProjDLLTypes = new Type[0];
            object aircadiaProjDLLObject = Activator.CreateInstance(aircadiaProjDLLTypes[0]);

            // Get method object to be executed in "aircadia project dll"
            MethodInfo methodInfo = aircadiaProjDLLTypes[0].GetMethod(Name);

            // initialise input arguments for the method
            object[] inputArgs = new object[ModelDataInputs.Count];
            int ncount = 0;
            foreach (Data data in ModelDataInputs)
            {
                if (data is IntegerData)
                {
                    inputArgs[ncount] = (Convert.ToInt32(data.Value));
                }
                else if (data is DoubleData)
                {
                    inputArgs[ncount] = (double)data.Value;
                }
                else if (data is IntegerVectorData)
                {
                    inputArgs[ncount] = (int[])data.Value;
                }
                else if (data is DoubleVectorData)
                {
                    inputArgs[ncount] = (double[])data.Value;
                }
                ncount++;
            }

            // Execute the model (call method of the Aircadia project dll)
            object modelResultObject = null;
            try
            {
                modelResultObject = methodInfo.Invoke(aircadiaProjDLLObject, inputArgs);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e.StackTrace);
                status = false;
            }

            // Convert modelResultObject into array of objects
            object[] modelResultObjects = (object[])modelResultObject;

            // Update/store the values of the model's outputs after execution of the model
            ncount = 0;
            foreach (Data data in ModelDataOutputs)
            {
				if (modelResultObjects[ncount] is int dataOutputValue)
				{
					data.Value = dataOutputValue;
				}
				else if (modelResultObjects[ncount] is double dataOutputValueD)
				{
					if (Double.IsNaN(dataOutputValueD) || Double.IsInfinity(dataOutputValueD) || Double.IsNegativeInfinity(dataOutputValueD) || Double.IsPositiveInfinity(dataOutputValueD))
					{
						Console.WriteLine("Invalid value in data" + data.Name + ".\r\n" + "Execution may continue");
						status = false;
					}
					data.Value = dataOutputValueD;
				}
				else if (modelResultObjects[ncount] is int[] dataOutputValueIA)
				{
					data.Value = dataOutputValueIA;
				}
				else if (modelResultObjects[ncount] is double[] dataOutputValueDA)
				{
					data.Value = dataOutputValueDA;
				}
				ncount++;
            }

            // Return with status of model execution
            return status;
        }
    }
}
