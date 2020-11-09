using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Aircadia.ObjectModel.DataObjects;
using System.IO;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Models
{
	[Serializable()]
	public class ModelDotNetDll : Model
	{
		[NonSerialized()]
		protected object classInstance;
		[NonSerialized()]
		protected Assembly assembly;
		[NonSerialized()]
		protected Type type;
		[NonSerialized()]
		protected MethodInfo methodInfo;

		[Serialize("DllPath", SerializationType.Path)]
		public string AssemblyName { get; protected set; } = String.Empty;
		[Serialize]
		public string TypeName { get; protected set; } = String.Empty;
		[Serialize]
		public string Method { get; protected set; } = String.Empty;

		private bool loaded = false;

		public ModelDotNetDll(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, 
			bool isAuxiliary = false, string parentName = "", string displayName = "")
			: base(name, description, modelDataInputs, modelDataOutputs, isAuxiliary, parentName, displayName) { }

		[DeserializeConstructor]
		public ModelDotNetDll(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, 
			string assemblyName, string typeName, string method, bool isAuxiliary = false, string parentName = "", string displayName = "")
			: base(name, description, modelDataInputs, modelDataOutputs, isAuxiliary, parentName, displayName)
		{
			AssemblyName = assemblyName;
			TypeName = typeName;
			Method = method;

			PrepareForExecution();
		}

		public ModelDotNetDll(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, 
			Assembly assembly, Type type, MethodInfo methodInfo, bool isAux = false, string parentName = "", string displayName = "")
			: base(name, description, modelDataInputs, modelDataOutputs, isAux, parentName)
		{
			AssemblyName = assembly?.Location;
			TypeName = type?.FullName;
			Method = methodInfo?.Name;

			this.assembly = assembly;
			this.type = type;
			classInstance = type != null ? Activator.CreateInstance(type) : null;
			this.methodInfo = methodInfo;
        }

		public override void PrepareForExecution()
		{
			if (!loaded)
			{
				byte[] bytes = File.ReadAllBytes(AssemblyName);
				assembly = Assembly.Load(bytes);
				type = assembly.GetTypes().Where(t => t.FullName == TypeName).FirstOrDefault();
                var ccc = type.GetRuntimeMethods();


                methodInfo = type.GetRuntimeMethods().Where(m => m.Name == Method).FirstOrDefault()
					?? throw new NullReferenceException($"The method '{Method}' could not be found");
				classInstance = Activator.CreateInstance(type);
				loaded = true;
			}
		}

		public override bool Execute()
        {
			int Ninputs = ModelDataInputs.Count;
			int Nout = Math.Min(methodInfo.GetParameters().Where(p => p.IsOut).Count(), ModelDataOutputs.Count);
			object[] parameters = ModelDataInputs.Concat(ModelDataOutputs.Take(Nout)).Select(i => i.Value).ToArray();
			//for (int i = 0; i < Nout; i++)
			//	inputs.Add(ModelDataOutputs[i].Value);
				//inputs.Add(Activator.CreateInstance(Type.GetType(param.ParameterType.FullName.TrimEnd('&'))));

			try
			{
				//object[] parameters = inputs.ToArray();
				object outputs = methodInfo.Invoke(classInstance, parameters);
				Type outputType = outputs?.GetType() ?? typeof(void);

				for (int i = 0; i < Nout; i++)
				{
					ModelDataOutputs[i].Value = parameters[Ninputs + i];
				}

				int idx = 0;
				if (outputs is object[] arr)
				{
					int N = Math.Min(arr.Length, ModelDataOutputs.Count);
					for (int i = 0; i < N; i++)
					{
						// Assign
						ModelDataOutputs[Nout + i].Value = arr[i];
					}
					idx = N;
				}
				else if (outputType.IsSuported())
				{
					ModelDataOutputs.First().Value = outputs;
					idx = 1;
				}
				else
				{
					PropertyInfo[] properties = outputType.GetProperties().Where(p => p.GetMethod.IsPublic && p.PropertyType.IsSuported()).ToArray();
					FieldInfo[] fields = outputType.GetFields().Where(f => f.IsPublic && f.FieldType.IsSuported()).ToArray();
					int N = Math.Min(properties.Length, ModelDataOutputs.Count);
					int M = Math.Min(fields.Length, ModelDataOutputs.Count - N);
					for (int i = 0; i < N; i++)
					{
						// Assign
						ModelDataOutputs[Nout + i].Value = properties[i].GetValue(outputs);
					}
					for (int i = 0; i < M; i++)
					{
						// Assign 
						ModelDataOutputs[Nout + N + i].Value = fields[i].GetValue(outputs);
					}
					idx = N + M;
				}

				
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return false;
			}

			return true;
        }

		public override Model Copy(string id, string name = null, string parentName = null) 
			=> new ModelDotNetDll(id, Description, ModelDataInputs, ModelDataOutputs, AssemblyName, TypeName, Method, IsAuxiliary, parentName ?? ParentName, name ?? Name);

		public override string ModelType => "DotNetDll";
	}
}
