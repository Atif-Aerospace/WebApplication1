using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;
using System;
using System.Collections.Generic;

namespace Aircadia.ObjectModel.Models.Legacy
{
	public class ModelFortranDll : ModelCSharp
	{
		[Serialize(Type = SerializationType.Path)]
		public string DllPath { get; }
		[Serialize]
		public string MethodName { get; }

		[DeserializeConstructor]
		public ModelFortranDll(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, string dllPath, string methodName) : base(name?.Replace(" ", ""), description, modelDataInputs, modelDataOutputs)
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
			string lcCodeInOnly = "";
			string lcCodeInNme = "";
			foreach (Data dt in ModelDataInputs)
			{
				lcCodeIn = lcCodeIn + "[In]ref double " + dt.Name + ",";
				lcCodeInNme = lcCodeInNme + "ref " + dt.Name + ",";
				lcCodeInOnly = lcCodeInOnly + "double " + dt.Name + ",";
			}
			foreach (Data dt in ModelDataOutputs)
			{
				lcCode = lcCode + "double " + dt.Name + " = " + Convert.ToString((double)(dt.Value)) + ";\n";
				lcCodeIn = lcCodeIn + "[In,Out] ref double " + dt.Name + ",";
				lcCodeInNme = lcCodeInNme + "ref " + dt.Name + ",";
			}
			if (lcCodeIn != "")
			{
				lcCodeIn = lcCodeIn.Remove(lcCodeIn.Length - 1);
				lcCodeInNme = lcCodeInNme.Remove(lcCodeInNme.Length - 1);
				lcCodeInOnly = lcCodeInOnly.Remove(lcCodeInOnly.Length - 1);
			}
			lcCode = lcCode + MethodName.ToUpper() + "(" + lcCodeInNme + ");" + "\n";
			lcCode += "object[] outputs_all=new object[" + Convert.ToString(ModelDataOutputs.Count) + "];";
			int ncount = 0;
			foreach (Data dt in ModelDataOutputs)
			{
				lcCode += "outputs_all[" + Convert.ToString(ncount) + "]=" + dt.Name + ";\n";
				ncount++;
				//lcCode = lcCode +"return " + dt.name + ";";
			}
			lcCode += "object outputs_return=outputs_all;";
			lcCode = lcCode + "return outputs_return;";
			// *** Must create a fully functional assembly

			lcCode = "[DllImport(@\"" + DllPath + "\"" + ", CallingConvention = CallingConvention.Cdecl" + ")]" +
					 "private static extern void " + MethodName.ToUpper() + "(" + lcCodeIn + ");" +
					 "public static object " + Name + @"(" + lcCodeInOnly + @") {
                     " + lcCode +
					 "}   ";

			Code = lcCode;

			Compile(AircadiaProject.Instance.ProjectPath);
		}

		public override string ModelType => "FortranDll";
	}
}
