﻿using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;
using System;
using System.Collections.Generic;

namespace Aircadia.ObjectModel.Models.Legacy
{
	public class ModelMatlabDll_7_13 : ModelCSharp, IModelMatlabLegacy
	{
		public string DllPath { get; }
		public string MethodName { get; }
		public static int Counter {get; set;}

		[DeserializeConstructor]
		public ModelMatlabDll_7_13(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, string dllPath, string methodName) : base(name?.Replace(" ", ""), description, modelDataInputs, modelDataOutputs)
		{
			DllPath = dllPath;
			MethodName = methodName;

			// Generate model code
			PrepareForExecution();
		}

		public override void PrepareForExecution()
		{
			//Creates the DLLS for model
			string lcCode = "";
			string lcCodeIn = "";
			string lcCodeOut = "";
			string lcCodeInOnly = "";
			//string lcCodeInNme = "";
			int nargout = 0;

			//creates output parameter data
			nargout = ModelDataOutputs.Count;
			foreach (Data dt in ModelDataOutputs)
			{
				string outParamsIdent = "[In, Out] ref IntPtr ";
				string outParamsName = dt.Name + "_1" + ",";
				lcCodeOut = lcCodeOut + outParamsIdent + outParamsName;

			}
			//creates input parameter data
			foreach (Data dt in ModelDataInputs)
			{
				string inParamsIdent = "[In]IntPtr ";
				string inParamsName = dt.Name + "_1" + ",";
				lcCodeIn = lcCodeIn + inParamsIdent + inParamsName;
				//lcCodeInOnly = lcCodeInOnly + "double " + dt.name + ",";
				if (dt is DoubleData)
					lcCodeInOnly = lcCodeInOnly + "double " + dt.Name + ",";
				if (dt is DoubleVectorData)
					lcCodeInOnly = lcCodeInOnly + "double[] " + dt.Name + ",";
				if (dt is DoubleMatrixData)
					lcCodeInOnly = lcCodeInOnly + "double[,] " + dt.Name + ",";
				if (dt is IntegerData)
					lcCodeInOnly = lcCodeInOnly + "int " + dt.Name + ",";
				if (dt is IntegerVectorData)
					lcCodeInOnly = lcCodeInOnly + "int[] " + dt.Name + ",";
				if (dt is StringData)
					lcCodeInOnly = lcCodeInOnly + "string " + dt.Name + ",";
			}
			if (lcCodeIn != "")
			{
				lcCodeIn = lcCodeIn.Remove(lcCodeIn.Length - 1);

				lcCodeInOnly = lcCodeInOnly.Remove(lcCodeInOnly.Length - 1);
			}

			// main MATLAB functional method
			//Replaced by the line below - 17April2012// lcCode = "\n[DllImport(@\"" + Directory.GetCurrentDirectory() + "\\" + MatlabDllPath.Substring(MatlabDllPath.LastIndexOf("\\") + 1) + "\")]" + "\n";
			lcCode = "\n[DllImport(@\"" + DllPath + "\")]" + "\n";
			string methodTempName = MethodName;
			if (methodTempName.Length <= 1)
			{
				Code = methodTempName.ToUpper();
			}
			else
			{
				char[] letters = methodTempName.ToCharArray();
				letters[0] = Char.ToUpper(letters[0]);
				methodTempName = new string(letters);
			}
			lcCode = lcCode + "public static extern void mlf" + methodTempName + "(" + "[In]int nargout," +
											" " + lcCodeOut + lcCodeIn + "); \n";
			// main MATLAB functional intialization method
			//Replaced by the line below - 17April2012// lcCode = lcCode + "[DllImport(@\"" + Directory.GetCurrentDirectory() + "\\" + MatlabDllPath.Substring(MatlabDllPath.LastIndexOf("\\") + 1) + "\")]" + "\n";
			lcCode = lcCode + "[DllImport(@\"" + DllPath + "\")]" + "\n";
			lcCode = lcCode + "public static extern void " + MethodName + "Initialize(); \n";

			// main MATLAB functional termination method
			//Replaced by the line below - 17April2012// lcCode = lcCode + "[DllImport(@\"" + Directory.GetCurrentDirectory() + "\\" + MatlabDllPath.Substring(MatlabDllPath.LastIndexOf("\\") + 1) + "\")]" + "\n";
			lcCode = lcCode + "[DllImport(@\"" + DllPath + "\")]" + "\n";
			lcCode = lcCode + "public static extern void " + MethodName + "Terminate(); \n";

			if (Counter == 0)
			{
				// load MATLAB engine methods needed
				lcCode = lcCode + "[DllImport(@\"mclmcrrt713.dll\")] \n";
				lcCode = lcCode + "public static extern IntPtr mxCreateDoubleScalar_proxy(double value); \n";

				lcCode = lcCode + "[DllImport(@\"mclmcrrt713.dll\")] \n";
				lcCode = lcCode + "public static extern void mxDestroyArray_proxy(IntPtr value); \n";

				lcCode = lcCode + "[DllImport(@\"mclmcrrt713.dll\")] \n";
				lcCode = lcCode + "public static extern double mxGetScalar_proxy(IntPtr value); \n";

				lcCode = lcCode + "[DllImport(@\"mclmcrrt713.dll\")] \n";
				lcCode = lcCode + "public static extern bool mclInitializeApplication_proxy(string options, Int32 count); \n";

				lcCode = lcCode + "[DllImport(@\"mclmcrrt713.dll\")] \n";
				lcCode = lcCode + "public static extern void mclTerminateApplication_proxy(); \n";

				lcCode = lcCode + "[DllImport(\"mclmcrrt713.dll\")] \n";
				lcCode = lcCode + "private static extern IntPtr mxCreateDoubleMatrix_730_proxy(int noOfRows, int noOfCols, string realOrComplex); \n";

				lcCode = lcCode + "[DllImport(\"mclmcrrt713.dll\")] \n";
				lcCode = lcCode + "public static extern IntPtr mxCreateString_proxy(string value); \n";

				lcCode = lcCode + "[DllImport(\"mclmcrrt713.dll\")] \n";
				lcCode = lcCode + "public static extern string mxGetString_730_proxy(IntPtr value); \n";

				lcCode = lcCode + "[DllImport(\"mclmcrrt713.dll\")] \n";
				lcCode = lcCode + "private static extern IntPtr mxGetPr_proxy(IntPtr value); \n";

				Counter++;
			}

			lcCode = lcCode + "public object " + Name + "(" + lcCodeInOnly + " ) \n";
			lcCode = lcCode + "{ \n";
			//lcCode = lcCode + "Console.WriteLine(\"Step_0 OK...\"); \n";//DEBUGGING//
			if (Counter == 0)//Added 17April2012//
			{//Added 17April2012//
				lcCode = lcCode + "try { \n";//Added 24May2012// vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv  Try - start
				lcCode = lcCode + "bool RetVal = mclInitializeApplication_proxy(\"NULL\", 0); \n";
				//lcCode = lcCode + "Console.WriteLine(RetVal); \n";//Added 24May2012//
				lcCode = lcCode + "} \n";//Added 24May2012//
				lcCode = lcCode + "catch (Exception e) \n";//Added 24May2012//
				lcCode = lcCode + "{ \n";//Added 24May2012//
				lcCode = lcCode + "Console.WriteLine(e.Message); \n";//Added 24May2012//
				lcCode = lcCode + "return null; \n";//Added 24May2012//
				lcCode = lcCode + "} \n";//Added 24May2012// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  Try - end
			}//Added 17April2012//

			// intialise the MATLAB functional method
			//lcCode = lcCode + "Console.WriteLine(\"Step_1 OK...\"); \n";//DEBUGGING//
			lcCode = lcCode + "try { \n";//Added 24May2012// vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv  Try - start
			lcCode = lcCode + MethodName + "Initialize(); \n";
			lcCode = lcCode + "} \n";//Added 24May2012//
			lcCode = lcCode + "catch (Exception e) \n";//Added 24May2012//
			lcCode = lcCode + "{ \n";//Added 24May2012//
			lcCode = lcCode + "Console.WriteLine(e.Message); \n";//Added 24May2012//
			lcCode = lcCode + "Console.WriteLine(\"Not initialised\"); \n";//Added 10Jul2013
																		   //lcCode = lcCode + "return null; \n";//Added 24May2012//Commented 10Jul2013
			lcCode = lcCode + "} \n";//Added 24May2012// ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  Try - end

			//Creating storage fields for inputs and outputs
			//lcCode = lcCode + "Console.WriteLine(\"Step_2 OK...\"); \n";//DEBUGGING//
			lcCode = lcCode + "int nargout = " + nargout + "; \n";
			//lcCode = lcCode + "ArrayList outputsNames = new ArrayList(); \n"; 


			foreach (Data dt in ModelDataInputs)
			{
				if (dt is DoubleData)
				{
					lcCode = lcCode + "IntPtr " + dt.Name + "_1" + " = mxCreateDoubleScalar_proxy(" + dt.Name + "); \n";
					// outputsNames.Add(dt.name);
				}
				else if (dt is DoubleVectorData)
				{
					lcCode = lcCode + "int " + dt.Name + "_" + "NoElements = " + (dt.Value as double[]).GetLength(0) + "; \n";
					lcCode = lcCode + "IntPtr " + dt.Name + "_1" + " = mxCreateDoubleMatrix_730_proxy(1, " + dt.Name + "_" + "NoElements, \"mxREAL\"); \n";
					lcCode = lcCode + "Marshal.Copy(" + dt.Name + ", 0, mxGetPr_proxy(" + dt.Name + "_1" + "), " + dt.Name + "_" + "NoElements);\n";
				}
				else if (dt is DoubleMatrixData)
				{
					lcCode = lcCode + "int " + dt.Name + "_" + "NoInElements_x = " + (dt.Value as double[,]).GetLength(0) + "; \n";
					lcCode = lcCode + "int " + dt.Name + "_" + "NoInElements_y = " + (dt.Value as double[,]).GetLength(1) + "; \n";
					lcCode = lcCode + "IntPtr " + dt.Name + "_1" + " = mxCreateDoubleMatrix_730_proxy(" + dt.Name + "_" + "NoInElements_x, " + dt.Name + "_" + "NoInElements_y, \"mxREAL\"); \n";
					lcCode = lcCode + "double[] " + dt.Name + "_ArrayConversion" + " = new double[" + dt.Name + "_" + "NoInElements_x * " + dt.Name + "_" + "NoInElements_y];\n";

					lcCode = lcCode + "int " + dt.Name + "_" + "ArrayConversionCounter = 0; \n";
					lcCode = lcCode + "foreach (double MatrixElement in (" + dt.Name + " as double[,])) \n";
					lcCode = lcCode + "{ \n";
					lcCode = lcCode + dt.Name + "_ArrayConversion[" + dt.Name + "_ArrayConversionCounter] = MatrixElement; \n";
					lcCode = lcCode + "        " + dt.Name + "_ArrayConversionCounter++; \n";
					lcCode = lcCode + "} \n";

					lcCode = lcCode + "Marshal.Copy(" + dt.Name + "_ArrayConversion, 0, mxGetPr_proxy(" + dt.Name + "_1" + "), " + dt.Name + "_" + "NoInElements_x * " + dt.Name + "_" + "NoInElements_y);\n";
				}
				else if (dt is StringData)
				{
					//lcCode = lcCode + "Console.WriteLine(\"Input string ...\"); \n";
					lcCode = lcCode + "IntPtr " + dt.Name + "_1" + " = mxCreateString_proxy(" + Convert.ToString(dt.Name) + "); \n";
					//lcCode = lcCode + "Console.WriteLine(\"Input string ok!\"); \n";
				}
			}

			foreach (Data dt in ModelDataOutputs)
			{
				if (dt is DoubleData)
				{
					lcCode = lcCode + "IntPtr " + dt.Name + "_1" + " = mxCreateDoubleScalar_proxy(" + (double)(dt.Value) + "); \n";
				}
				else if (dt is DoubleVectorData)
				{
					lcCode = lcCode + "int " + dt.Name + "_" + "NoElements = " + (dt.Value as double[]).GetLength(0) + "; \n";
					lcCode = lcCode + "double[] " + dt.Name + " = new double[" + dt.Name + "_" + "NoElements]; \n";
					lcCode = lcCode + "IntPtr " + dt.Name + "_1" + " = mxCreateDoubleMatrix_730_proxy(1, " + dt.Name + "_" + "NoElements, \"mxREAL\"); \n";
				}
				else if (dt is DoubleMatrixData)
				{
					lcCode = lcCode + "int " + dt.Name + "_" + "NoOutElements_x = " + (dt.Value as double[,]).GetLength(0) + "; \n";
					lcCode = lcCode + "int " + dt.Name + "_" + "NoOutElements_y = " + (dt.Value as double[,]).GetLength(1) + "; \n";
					lcCode = lcCode + "double[,] " + dt.Name + " = new double[" + dt.Name + "_" + "NoOutElements_x," + dt.Name + "_" + "NoOutElements_y]; \n";
					lcCode = lcCode + "IntPtr " + dt.Name + "_1" + " = mxCreateDoubleMatrix_730_proxy(" + dt.Name + "_" + "NoOutElements_x, " + dt.Name + "_" + "NoOutElements_y, \"mxREAL\"); \n";
					lcCode = lcCode + "double[] " + dt.Name + "_ArrayConversion" + " = new double[" + dt.Name + "_" + "NoOutElements_x * " + dt.Name + "_" + "NoOutElements_y];\n";
				}
				else if (dt is StringData)
				{
					//lcCode = lcCode + "Console.WriteLine(\"Output string ...\"); \n";
					lcCode = lcCode + "IntPtr " + dt.Name + "_1" + " = mxCreateString_proxy(" + "Convert.ToString(0)" + "); \n";
					//lcCode = lcCode + "Console.WriteLine(\"Output string ok!\"); \n";
				}


			}


			// creating results storage fields
			//int resultSize = outputs.Count;
			//lcCode = lcCode + "Dictionary<string, double> results = new Dictionary<string, double>("
			//                                                          + resultSize + "); \n";


			// begin execution of MATLAB functional method - TRY
			//lcCode = lcCode + "Console.WriteLine(\"Step_4 OK...\"); \n";//DEBUGGING//
			lcCode = lcCode + "try { \n";

			// get actual OUTPUT parameters to supply to MATLAB function method
			string actualParamsOut = "";
			foreach (Data dt in ModelDataOutputs)
			{
				actualParamsOut = actualParamsOut + "ref " + dt.Name + "_1" + ",";
			}

			// get actual INPUT parameters to supply to MATLAB function method
			string actualParamsIn = "";
			foreach (Data dt in ModelDataInputs)
			{
				actualParamsIn = actualParamsIn + dt.Name + "_1" + ",";
			}
			if (actualParamsIn != "")
			{
				actualParamsIn = actualParamsIn.Remove(actualParamsIn.Length - 1);
			}
			//lcCode = lcCode + "Console.WriteLine(\"Step_5 OK...\"); \n";//DEBUGGING//
			lcCode = lcCode + "mlf" + methodTempName + "(" + "nargout, "
											+ actualParamsOut + actualParamsIn + "); \n";
			//lcCode = lcCode + "Console.WriteLine(\"Step_6 OK...\"); \n";//DEBUGGING//
			foreach (Data dt in ModelDataOutputs)
			{
				if (dt is DoubleData)
				{
					lcCode = lcCode + "double " + dt.Name + "= mxGetScalar_proxy(" + dt.Name + "_1" + ");\n";
				}
				else if (dt is DoubleVectorData)
				{
					//lcCode = lcCode + "int " + dt.name + "_" + "NoElements = " + (dt.values as double[]).GetLength(0) + "; \n";
					lcCode = lcCode + "Marshal.Copy(mxGetPr_proxy(" + dt.Name + "_1), " + dt.Name + ", 0," + dt.Name + "_NoElements); \n";
					//System.Console.WriteLine("Output for ARRAY IO test = [" + actualOut3[0] + ", " + actualOut3[1] + "]");
				}
				else if (dt is DoubleMatrixData)
				{
					lcCode = lcCode + "Marshal.Copy(mxGetPr_proxy(" + dt.Name + "_1), " + dt.Name + "_ArrayConversion, 0," + dt.Name + "_NoOutElements_x *" + dt.Name + "_NoOutElements_y); \n";
					lcCode = lcCode + "for (int IthRow = 0; IthRow <" + dt.Name + "_NoOutElements_x; IthRow++) \n";
					lcCode = lcCode + "{ \n";
					lcCode = lcCode + "for (int JthCol = 0; JthCol <" + dt.Name + "_NoOutElements_y; JthCol++) \n";
					lcCode = lcCode + "{ \n";
					lcCode = lcCode + dt.Name + "[IthRow,JthCol] = " + dt.Name + "_ArrayConversion[IthRow * " + dt.Name + "_NoOutElements_y + JthCol]; \n";
					lcCode = lcCode + "} \n";
					lcCode = lcCode + "} \n";
				}
				else if (dt is StringData)
				{
					lcCode = lcCode + "string " + dt.Name + "= mxGetString_730_proxy(" + dt.Name + "_1" + ");\n";
					//lcCode = lcCode + "Console.WriteLine(\"Check2 ok!\"); \n";
				}
			}

			//lcCode = lcCode + "foreach (string s1 in outputsNames) \n";
			//lcCode = lcCode + "{ \n";
			//lcCode = lcCode + "double s1 = mxGetScalar(s1);";
			//lcCode = lcCode + "} \n";
			lcCode += "object[] outputs_all=new object[" + Convert.ToString(ModelDataOutputs.Count) + "]; \n";
			int ncount = 0;
			foreach (Data dt in ModelDataOutputs)
			{
				lcCode += "outputs_all[" + Convert.ToString(ncount) + "]=" + dt.Name + ";\n";
				ncount++;
				//lcCode = lcCode +"return " + dt.name + ";";
			}
			lcCode += "object outputs_return=outputs_all; \n";
			lcCode = lcCode + "return outputs_return; \n";
			// end of TRY bracket
			lcCode = lcCode + "} \n";
			lcCode = lcCode + "catch (Exception e) \n";
			lcCode = lcCode + "{ \n";
			lcCode = lcCode + "Console.WriteLine(e.Message); \n";
			lcCode = lcCode + "return null; \n";
			lcCode = lcCode + "} \n";

			// Terminate libraries and MCR
			//foreach (Data dt in outputs)
			// {
			//     lcCode = lcCode + "mxDestroyArray(dt.name); \n";
			//}

			lcCode = lcCode + MethodName + "Terminate(); \n";

			//lcCode = lcCode + "mclTerminateApplication(); \n";
			lcCode = lcCode + "} \n";
			//System.Console.WriteLine(lcCode);
			Code = lcCode;

			Compile(AircadiaProject.Instance.ProjectPath);
		}
		
		public override string ModelType => "MatlabDll_7_13";
	}
}
