using System;
using System.Collections.Generic;
using System.Reflection;
using Aircadia.ObjectModel.DataObjects;


namespace Aircadia.ObjectModel.Models
{
	[Serializable()]
	public class ModelMatlabDll : ModelDotNetDll
	{
		public ModelMatlabDll(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, 
			Assembly assembly, Type type, MethodInfo methodInfo, string parentName = "", string displayName = "") 
			: base(name?.Replace(" ", ""), description, modelDataInputs, modelDataOutputs, assembly, type, methodInfo, parentName: parentName, displayName: displayName) { }

		public override bool Execute()
		{
			//int nIn = ModelDataInputs.Count;
			//int nOut = ModelDataOutputs.Count;
			//var inputsM = new MWArray[nIn];

			//int idx = 0;
			//foreach (Data input in ModelDataInputs)
			//{
			//	switch (input)
			//	{
			//		case DoubleData d:
			//			inputsM[idx] = new MWNumericArray((double)d.Value);
			//			break;
			//		case DoubleVectorData d:
			//			inputsM[idx] = new MWNumericArray((double[])d.Value);
			//			break;
			//		case DoubleMatrixData d:
			//			inputsM[idx] = new MWNumericArray((double[,])d.Value);
			//			break;
			//		case IntegerData d:
			//			inputsM[idx] = new MWNumericArray((int)d.Value);
			//			break;
			//		case IntegerVectorData d:
			//			inputsM[idx] = new MWNumericArray((int[])d.Value);
			//			break;
			//		case StringData d:
			//			inputsM[idx] = new MWCharArray(d.Value as string);
			//			break;
			//		default:
			//			break;
			//	}
			//	idx++;
			//}

			//var types = new Type[nOut];
			//idx = 0;
			//foreach (Data output in ModelDataOutputs)
			//{
			//	switch (output)
			//	{
			//		case DoubleData d:
			//			types[idx] = typeof(double);
			//			break;
			//		case DoubleVectorData d:
			//			types[idx] = typeof(double[]);
			//			break;
			//		case DoubleMatrixData d:
			//			types[idx] = typeof(double[,]);
			//			break;
			//		case IntegerData d:
			//			types[idx] = typeof(int);
			//			break;
			//		case IntegerVectorData d:
			//			types[idx] = typeof(int[]);
			//			break;
			//		case StringData d:
			//			types[idx] = typeof(string);
			//			break;
			//		default:
			//			break;
			//	}
			//	idx++;
			//}
			//var outputsM = new MWArray[nOut];

			
			//try
			//{
			//	object[] parameters = new object[] { nOut, outputsM, inputsM };
			//	methodInfo.Invoke(classInstance, new object[] { nOut, outputsM, inputsM });

			//	outputsM = parameters[1] as MWArray[];
			//	idx = 0;
			//	foreach (object output in MWArray.ConvertToNativeTypes(outputsM, types))
			//	{
			//		ModelDataOutputs[idx].Value = output;
			//		idx++;
			//	}
			//}
			//catch (Exception)
			//{
			//	return false;
			//}

			return true;
		}

		public override Model Copy(string id, string name = null, string parentName = null)
			=> new ModelMatlabDll(id, Description, ModelDataInputs, ModelDataOutputs, assembly, type, methodInfo, parentName ?? ParentName, name ?? Name);
	}
}
