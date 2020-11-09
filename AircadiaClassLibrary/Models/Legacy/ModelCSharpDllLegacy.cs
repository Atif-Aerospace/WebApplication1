using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;
using System;
using System.Collections.Generic;

namespace Aircadia.ObjectModel.Models.Legacy
{
	[Serializable()]
	public class ModelCSharpDllLegacy : ModelCSharp
	{
		[Serialize(Type = SerializationType.Path)]
		public string DllPath { get; }
		[Serialize("MethodName")]
		public string DllEntry { get; }

		[DeserializeConstructor]
		public ModelCSharpDllLegacy(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, string dllPath, string dllEntry) : base(name?.Replace(" ", ""), description, modelDataInputs, modelDataOutputs, String.Empty)
		{
			DllPath = dllPath;
			DllEntry = dllEntry;

			// Generate model code
			PrepareForExecution();
		}

		public override void PrepareForExecution()
		{
			string lcCode = "";
			string lcCodeIn = "";
			string lcCodeInOnly = "";
			foreach (Data dt in ModelDataInputs)
			{
				if (dt is DoubleData)
					lcCodeInOnly = lcCodeInOnly + "double " + dt.Name + ",";
				else if (dt is DoubleVectorData)
					lcCodeInOnly = lcCodeInOnly + "double[] " + dt.Name + ",";
				else if (dt is DoubleMatrixData)
					lcCodeInOnly = lcCodeInOnly + "double[,] " + dt.Name + ",";
				else if (dt is IntegerData)
					lcCodeInOnly = lcCodeInOnly + "int " + dt.Name + ",";
				else if (dt is IntegerVectorData)
					lcCodeInOnly = lcCodeInOnly + "int[] " + dt.Name + ",";
			}
			foreach (Data dt in ModelDataOutputs)
			{
				//lcCode = lcCode + "double " + dt.name + " = " + Convert.ToString(dt.value) + ";\n";
				//lcCodeIn = lcCodeIn + "[In,Out] ref double " + dt.name + ",";
				//lcCodeInNme = lcCodeInNme + "ref " + dt.name + ",";
			}
			if (lcCodeIn != "")
			{
				//lcCodeIn = lcCodeIn.Remove(lcCodeIn.Length - 1);
				//lcCodeInNme = lcCodeInNme.Remove(lcCodeInNme.Length - 1);
				lcCodeInOnly = lcCodeInOnly.Remove(lcCodeInOnly.Length - 1);
			}
			//lcCode = lcCode + equation.ToUpper() + "(" + lcCodeInNme + ");" + "\n";
			lcCode += "string[] strs = \"" + DllEntry + "\".Split('_');\n";
			lcCode += "Assembly assembly = Assembly.LoadFile(@\"" + DllPath + "\");\n";
			lcCode += "Type type = assembly.GetType(strs[0]);\n";
			lcCode += "MethodInfo methodInfo = type.GetMethod(strs[1]);\n";
			lcCode += "Object obj = Activator.CreateInstance(type);\n";

			//object[] param = { 3, 5.8 };
			lcCode += "object[] param = new object[" + (ModelDataInputs.Count + ModelDataOutputs.Count) + "];\n";
			int cnt = 0;
			foreach (Data dt in ModelDataInputs)
			{
				lcCode += "param[" + cnt + "] = " + dt.Name + ";\n";
				cnt++;
			}

			lcCode += "methodInfo.Invoke(obj, param);\n";
			lcCode += "object[] outputs_all=new object[" + Convert.ToString(ModelDataOutputs.Count) + "];\n";
			int ncount = 0;
			foreach (Data dt in ModelDataOutputs)
			{
				lcCode += "outputs_all[" + Convert.ToString(ncount) + "]=" + "param[" + (ModelDataInputs.Count + ncount) + "];\n";
				ncount++;
			}
			lcCode += "object outputs_return=outputs_all;\n";
			lcCode = lcCode + "return outputs_return;\n";
			// *** Must create a fully functional assembly

			lcCode = "public object " + Name + @"(" + lcCodeInOnly.TrimEnd(',') + @") {
			" + lcCode +
"}   ";

			Code = lcCode;

			Compile(AircadiaProject.Instance.ProjectPath);
		}

		public override string ModelType => "CSharpDllLegacy";
	}
}
