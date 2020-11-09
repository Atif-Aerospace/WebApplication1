using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;
using System;
using System.Collections.Generic;

namespace Aircadia.ObjectModel.Models.Legacy
{
	[Serializable()]
	public class ModelCSharpLegacy : ModelCSharp
	{
		[Serialize("Code", SerializationType.Lines)]
		public string CSharpCode { get; }

		[DeserializeConstructor]
		public ModelCSharpLegacy(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, string cSharpCode) : base(name?.Replace(" ", ""), description, modelDataInputs, modelDataOutputs)
		{
			CSharpCode = cSharpCode;

			// Generate model code
			PrepareForExecution();
		}

		public override void PrepareForExecution()
		{
			string lcCode = "";
			string lcCodeIn = "";
			foreach (Data dt in ModelDataInputs)
			{
				if (dt is DoubleData)
					lcCodeIn = lcCodeIn + "double " + dt.Name + ",";
				else if (dt is IntegerData)
					lcCodeIn = lcCodeIn + "int " + dt.Name + ",";
				else if (dt is StringData)
					lcCodeIn = lcCodeIn + "string " + dt.Name + ",";
				else if (dt is DoubleVectorData)
					lcCodeIn = lcCodeIn + "double[] " + dt.Name + ",";
				else if (dt is DoubleMatrixData)
					lcCodeIn = lcCodeIn + "double[,] " + dt.Name + ",";
			}
			if (lcCodeIn != "")
				lcCodeIn = lcCodeIn.Remove(lcCodeIn.Length - 1);
			foreach (Data dt in ModelDataOutputs)
			{
				if (dt is DoubleData)
					lcCode = lcCode + "double " + dt.Name + " = " + dt.GetValueAsDouble() + ";\n";
				else if (dt is IntegerData)
					lcCode = lcCode + "int " + dt.Name + " = " + dt.GetValueAsDouble() + ";\n";
				else if (dt is StringData)
					lcCode = lcCode + "string " + dt.Name + " = \"" + Convert.ToString(dt.ValueAsString) + "\";\n";
				else if (dt is DoubleVectorData)
					lcCode = lcCode + "double[] " + dt.Name + " = {" + dt.ValueAsString + "};\n";
				else if (dt is DoubleMatrixData)
				{
					//string aaa = "\r\n";
					//string[] sdfg = dt.valueinstring.Split(aaa.ToCharArray());
					//string StringValues = "";
					//foreach (string Element in sdfg)
					//{
					//    if (Element != "")
					//        StringValues = StringValues + "{" + Element + "},";
					//}
					//StringValues = StringValues.TrimEnd(',');
					//lcCode = lcCode + "double[,] " + dt.Name + " = {" + StringValues + "};\n";
				}
			}
			lcCode = lcCode + CSharpCode + "\n";
			if ((IsAuxiliary == false) | (ModelDataOutputs.Count > 1))
				lcCode = CreateModelCSDLL_ForNormalModel(lcCode, lcCodeIn);
			else
				lcCode = CreateModelCSDLL_ForAuxModel(lcCode, lcCodeIn);
			Code = lcCode;
			FunctionBody = "";
			Compile(AircadiaProject.Instance.ProjectPath);
		}

		/// <summary>
		/// Creates C#DLL For Normal Models. 
		/// </summary>
		/// <param name="lcCode"></param>
		/// <param name="lcCodeIn"></param>
		/// <returns></returns>
		private string CreateModelCSDLL_ForNormalModel(string lcCode, string lcCodeIn)
		{
			//Creates the DLLS for model

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
			lcCode = "public object " + Name + @"(" + lcCodeIn + @") {
		    " + lcCode +
			"}  ";
			return lcCode;
		}

		/// <summary>
		/// Creates C#DLL For Auxiliary Models. 
		/// </summary>
		/// <param name="lcCode"></param>
		/// <param name="lcCodeIn"></param>
		/// <returns></returns>
		private string CreateModelCSDLL_ForAuxModel(string lcCode, string lcCodeIn)
		{
			// It looks like only one output is assumed (Atif's Comment)
			lcCode += "return " + (ModelDataOutputs[0] as Data).Name + ";";
			if (ModelDataOutputs[0] is DoubleData)
			{
				lcCode = "public double " + Name + @"(" + lcCodeIn + @") {
            " + lcCode +
				"}  ";
			}
			else if (ModelDataOutputs[0] is IntegerData)
			{
				lcCode = "public int " + Name + @"(" + lcCodeIn + @") {
            " + lcCode +
				"}  ";
			}
			return lcCode;
		}


		public override string ModelType => "CSharpLegacy";
	}
}
