using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aircadia.ObjectModel.Models.Legacy
{
	public class ModelMatlabDll_7_8 : ModelCSharp, IModelMatlabLegacy
	{
		public string DllPath { get; }
		public string MethodName { get; }
		public static int Counter { get; set; }

		[DeserializeConstructor]
		public ModelMatlabDll_7_8(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, string dllPath, string methodName) : base(name?.Replace(" ", ""), description, modelDataInputs, modelDataOutputs)
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
			lcCode = "\n[DllImport(@\"" + Directory.GetCurrentDirectory() + "\\" + DllPath.Substring(DllPath.LastIndexOf("\\") + 1) + "\")]" + "\n";
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
			lcCode = lcCode + "public static extern void _mlf" + methodTempName + "(" + "[In]int nargout," +
											" " + lcCodeOut + lcCodeIn + "); \n";

			// main MATLAB functional intialization method
			lcCode = lcCode + "[DllImport(@\"" + Directory.GetCurrentDirectory() + "\\" + DllPath.Substring(DllPath.LastIndexOf("\\") + 1) + "\")]" + "\n";
			lcCode = lcCode + "public static extern void _" + MethodName + "Initialize(); \n";

			// main MATLAB functional termination method
			lcCode = lcCode + "[DllImport(@\"" + Directory.GetCurrentDirectory() + "\\" + DllPath.Substring(DllPath.LastIndexOf("\\") + 1) + "\")]" + "\n";
			lcCode = lcCode + "public static extern void _" + MethodName + "Terminate(); \n";

			if (Counter == 0)
			{
				// load MATLAB engine methods needed
				lcCode = lcCode + "[DllImport(\"mclmcrrt78.dll\")] \n";
				lcCode = lcCode + "public static extern IntPtr mxCreateDoubleScalar_proxy(double value); \n";

				lcCode = lcCode + "[DllImport(\"mclmcrrt78.dll\")] \n";
				lcCode = lcCode + "public static extern void mxDestroyArray_proxy(IntPtr value); \n";

				lcCode = lcCode + "[DllImport(\"mclmcrrt78.dll\")] \n";
				lcCode = lcCode + "public static extern double mxGetScalar_proxy(IntPtr value); \n";

				lcCode = lcCode + "[DllImport(\"mclmcrrt78.dll\")] \n";
				lcCode = lcCode + "public static extern bool mclInitializeApplication_proxy(string options, Int32 count); \n";

				lcCode = lcCode + "[DllImport(\"mclmcrrt78.dll\")] \n";
				lcCode = lcCode + "public static extern void mclTerminateApplication_proxy(); \n";
				Counter++;
			}

			lcCode = lcCode + "public object " + Name + "(" + lcCodeInOnly + " ) \n";
			lcCode = lcCode + "{ \n";

			//lcCode = lcCode + "bool RetVal = mclInitializeApplication(\"NULL\", 0); \n";

			// intialise the MATLAB functional method
			lcCode = lcCode + "_" + MethodName + "Initialize(); \n";
			lcCode = lcCode + "int nargout = " + nargout + "; \n";
			//lcCode = lcCode + "ArrayList outputsNames = new ArrayList(); \n"; 
			foreach (Data dt in ModelDataInputs)
			{
				lcCode = lcCode + "IntPtr " + dt.Name + "_1" + " = mxCreateDoubleScalar_proxy(" + dt.Name + "); \n";
				// outputsNames.Add(dt.name);
			}

			foreach (Data dt in ModelDataOutputs)
			{
				lcCode = lcCode + "IntPtr " + dt.Name + "_1" + " = mxCreateDoubleScalar_proxy(" + (double)(dt.Value) + "); \n";
			}

			// creating results storage fields
			//int resultSize = outputs.Count;
			//lcCode = lcCode + "Dictionary<string, double> results = new Dictionary<string, double>("
			//                                                          + resultSize + "); \n";


			// begin execution of MATLAB functional method - TRY
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
			lcCode = lcCode + "_mlf" + methodTempName + "(" + "nargout, "
											+ actualParamsOut + actualParamsIn + "); \n";
			foreach (Data dt in ModelDataOutputs)
			{
				lcCode = lcCode + "double " + dt.Name + "= mxGetScalar_proxy(" + dt.Name + "_1" + ");\n";

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
			lcCode = lcCode + "_" + MethodName + "Terminate(); \n";
			//lcCode = lcCode + "mclTerminateApplication(); \n";
			lcCode = lcCode + "} \n";
			//System.Console.WriteLine(lcCode);
			Code = lcCode;

			Compile(AircadiaProject.Instance.ProjectPath);
		}

		public override string ModelType => "MatlabDll_7_8";
	}
}
