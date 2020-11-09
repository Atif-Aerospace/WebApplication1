using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;
using System;
using System.Collections.Generic;
using System.IO;

namespace Aircadia.ObjectModel.Models.Legacy
{
	[Serializable()]
	public class ModelCppDll : ModelCSharp
	{
		[Serialize(Type = SerializationType.Path)]
		public string DllPath { get; }
		[Serialize]
		public string MethodName { get; }

		[DeserializeConstructor]
		public ModelCppDll(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, string dllPath, string methodName) : base(name?.Replace(" ", ""), description, modelDataInputs, modelDataOutputs)
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
			string lcCodeInNme = "";
			foreach (Data dt in ModelDataInputs)
			{
				//lcCode = lcCode + "double " + dt.name + " = " + Convert.ToString(dt.value) + ";\n";
				lcCodeIn = lcCodeIn + "double " + dt.Name + ",";
				lcCodeInNme = lcCodeInNme + dt.Name + ",";
			}
			if (lcCodeIn != "")
			{
				lcCodeIn = lcCodeIn.Remove(lcCodeIn.Length - 1);
				lcCodeInNme = lcCodeInNme.Remove(lcCodeInNme.Length - 1);
			}
			foreach (Data dt in ModelDataOutputs)
			{
				lcCode = lcCode + "double " + dt.Name + " = " + Convert.ToString((double)(dt.Value)) + ";\n";
				/*if (dt.name == "rwswa")
                {
                    Console.Write(dt.name + "=" + Convert.ToString(dt.value) + "\n");
                    this.dllname = "rwswa=phi;";
                }*/

			}
			lcCode = lcCode + (ModelDataOutputs[0] as Data).Name + "=" + MethodName + "(" + lcCodeInNme + ");" + "\n";
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
			lcCode = "[DllImport(@\"" + Directory.GetCurrentDirectory() + "\\" + DllPath.Substring(DllPath.LastIndexOf("\\") + 1) + "\")]" +
				"private static extern double " + MethodName + "(" + lcCodeIn + ");" +
				"public object " + Name + @"(" + lcCodeIn + @") {
				" + lcCode +
				"} ";

			Code = lcCode;

			Compile(AircadiaProject.Instance.ProjectPath);
		}

		public override string ModelType => "CppDll";
	}
}
