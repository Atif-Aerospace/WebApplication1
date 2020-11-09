using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;
using System;
using System.Collections.Generic;

namespace Aircadia.ObjectModel.Models.Legacy
{
	[Serializable()]
	public class ModelCDll : ModelCSharp
	{
		[Serialize(Type = SerializationType.Path)]
		public string DllPath { get; }
		[Serialize]
		public string MethodName { get; }

		[DeserializeConstructor]
		public ModelCDll(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, string dllPath, string methodName) : base(name?.Replace(" ", ""), description, modelDataInputs, modelDataOutputs)
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

			foreach (Data dt in ModelDataInputs)
			{
				if (dt is DoubleData)
					lcCodeIn = lcCodeIn + "double " + dt.Name + ",";
				else if (dt is DoubleVectorData)
					lcCodeIn = lcCodeIn + "double[] " + dt.Name + ",";
				else if (dt is DoubleMatrixData)
					lcCodeIn = lcCodeIn + "double[,] " + dt.Name + ",";
				else if (dt is IntegerData)
					lcCodeIn = lcCodeIn + "int " + dt.Name + ",";
				else if (dt is IntegerVectorData)
					lcCodeIn = lcCodeIn + "int[] " + dt.Name + ",";
			}
			if (lcCodeIn != "")
			{
				//lcCodeIn = lcCodeIn.Remove(lcCodeIn.Length - 1);
				//lcCodeInNme = lcCodeInNme.Remove(lcCodeInNme.Length - 1);
				lcCodeIn = lcCodeIn.Remove(lcCodeIn.Length - 1);
			}


			lcCode += "\t\tpublic object " + Name + "(" + lcCodeIn + ")\n";
			lcCode += "\t\t{\n";

			/*
            // initialize the object attributes
            lcCode += "\t\t\tstring initParam = File.ReadAllText(\"C:/Cranfield/AtifWork/initialParameters.ini\", Encoding.Default);\n";

            // prepare for .mos file HelloWorld.mos
            lcCode += "\t\t\tstring mos = File.ReadAllText(\"C:/Cranfield/AtifWork/dllbatchmode.mos\", Encoding.Default);\n";
            lcCode += "\t\t\tmos = mos.Replace(\"Variable__declarations\", initParam);\n";
            lcCode += "\t\t\tFile.WriteAllText(\"C:/Cranfield/AtifWork/ttt.mos\", mos, Encoding.Default);\n";
            */



			lcCode += "\t\t\tSystem.Diagnostics.Process p = new System.Diagnostics.Process();\n";
			lcCode += "\t\t\tp.StartInfo.UseShellExecute = false;\n";
			//p.StartInfo.RedirectStandardOutput = true;
			lcCode += "\t\t\tp.StartInfo.FileName = @\"C:/Program Files (x86)/Dymola 2014/bin64/dymola.exe\";\n";
			//lcCode += "p.StartInfo.Arguments = \"/nowindow \" + \"C:/Cranfield/AtifWork/ttt.mos\";";
			lcCode += "\t\t\tstring fn = \"D:/Shared Folder/Yogesh/New folder/ttt.mos\";\n";
			lcCode += "\t\t\tp.StartInfo.Arguments = \"\\\"\" + fn + \"\\\"\";\n";
			lcCode += "\t\t\tp.Start();\n";
			lcCode += "\t\t\tp.WaitForExit();\n";


			// Read Output

			lcCode += "\t\t\tstring CSVFilePathName = \"D:/Shared Folder/Yogesh/New folder/SelectedVariables.csv\";\n";
			lcCode += "\t\t\tstring[] Lines = File.ReadAllLines(CSVFilePathName);\n";
			lcCode += "\t\t\tstring[] Fields;\n";
			lcCode += "\t\t\tFields = Lines[0].Split(new char[] { ';' });\n";
			lcCode += "\t\t\tint Cols = Fields.GetLength(0);\n";

			lcCode += "\t\t\tdouble[,] data = new double[Lines.Length - 1, Cols];\n";
			lcCode += "\t\t\tfor (int j = 1; j < Lines.GetLength(0); j++)\n";
			lcCode += "\t\t\t{\n";
			lcCode += "\t\t\t\tFields = Lines[j].Split(new char[] { ';' });\n";
			lcCode += "\t\t\t\tfor (int f = 0; f < Cols; f++)\n";
			lcCode += "\t\t\t\t\tdata[j - 1,f] = Convert.ToDouble(Fields[f]);\n";
			lcCode += "\t\t\t}\n";


			lcCode += "\t\t\tobject[] outputs_all=new object[" + Convert.ToString(ModelDataOutputs.Count) + "];\n";
			lcCode += "\t\t\tfor (int i = 0; i < Cols; i++)\n";
			lcCode += "\t\t\t{\n";
			lcCode += "\t\t\t\tdouble[] temp = new double[data.GetLength(0)];\n";
			lcCode += "\t\t\t\tfor (int j = 0; j < data.GetLength(0); j++)\n";
			lcCode += "\t\t\t\t{\n";
			lcCode += "\t\t\t\t\ttemp[j] = data[j, i];\n";
			lcCode += "\t\t\t\t}\n";
			lcCode += "\t\t\t\toutputs_all[i] = " + "temp;\n";
			lcCode += "\t\t\t}\n";

			lcCode += "\t\t\tobject outputs_return=outputs_all;\n";
			lcCode += "\t\t\treturn outputs_return;\n";

			lcCode += "\t\t}\n";


			Code = lcCode;

			Compile(AircadiaProject.Instance.ProjectPath);
		}

		public override string ModelType => "CDll";
	}
}
