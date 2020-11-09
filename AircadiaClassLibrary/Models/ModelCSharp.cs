using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Compilers;
using System.CodeDom.Compiler;
using System.IO;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Models
{
	[Serializable()]
	public class ModelCSharp : ModelDotNetDll, ICompilable
	{

		[Serialize("Body", SerializationType.Lines)]
		public string FunctionBody { get; protected set; }
		public string Signature => GenerateSignature(Name, ModelDataInputs, ModelDataOutputs);
		public string Code { get; protected set; }

		public bool Compiled { get; set; } = false;
		private const string namespaceName = "Aircadia";
		private CompilerResults compilerResults;

		// Constructor for legacy models
		public ModelCSharp(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, string parentName = "") 
			: base(name?.Replace(" ", ""), description, modelDataInputs, modelDataOutputs, parentName: parentName) { }

		[DeserializeConstructor]
		public ModelCSharp(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, string functionBody, 
			bool isAuxiliary = false, string parentName = "", string displayName = "")
			: base(name?.Replace(" ", ""), description, modelDataInputs, modelDataOutputs, isAuxiliary, parentName, displayName)
		{
			FunctionBody = functionBody ?? throw new ArgumentNullException(nameof(functionBody));
			Code = Signature + "\n{\n" + FunctionBody + "\n}";

            PrepareForExecution();
        }

		public CompilerResults Compile()
		{
			if (!Compiled)
			{
				compilerResults = Compiler.CompileFunction(Name, Code);
				if (!compilerResults.Errors.HasErrors)
				{
					LinkToCompiledAssembly(compilerResults.PathToAssembly, $"{Name}Class");
				}

				return compilerResults;
			}
			else
			{
				base.PrepareForExecution();
			}

			return compilerResults;
		}

		public void LinkToCompiledAssembly(string pathToAssembly, string className)
		{
			AssemblyName = pathToAssembly;
			TypeName = $"{namespaceName}.{className}";
			Method = Id;

			base.PrepareForExecution();

			Compiled = true;
		}

        public override void PrepareForExecution() { } // => Compile();

		public override string ModelType => "CSharp";

		public override void RenameVariable(string oldName, string newName)
		{
			if (ModelDataInputs.Concat(ModelDataOutputs).FirstOrDefault(d => d.Name == newName) == null)
				return;

			var separators = new HashSet<char>(new char[] { '[', ']', '{', '}', ';', '.', ',', '(', ')', '"', '\'', '+', '-', '*', '/', '%', ' ', '\t', '\n' });

			FunctionBody = Replace(FunctionBody);
			Code = Signature + "\n{\n" + FunctionBody + "\n}";

			Compiled = false;

			string Replace(string text)
			{
				int index = -1;
				do
				{
					index = text.IndexOf(oldName, index+1);

					if (index == -1)
						continue;

					if (index > 0 && !separators.Contains(text[index - 1]))
						continue;

					if (index < text.Length - 1 - oldName.Length && !separators.Contains(text[index + oldName.Length]))
						continue;

					text = text.Substring(0, index) + newName + text.Substring(index + oldName.Length);
				} while (index != -1);
				

				return text;
			}
		}

		public static string GenerateSignature(string name, IEnumerable<Data> inputs, IEnumerable<Data> outputs)
		{
			var sb = new StringBuilder($"private void {name}(");
			sb = inputs.Aggregate(sb, (b, i) => b.Append($"{TypeString(i)} {i.Name.Replace('.', '_')}, "));
			sb = outputs.Aggregate(sb, (b, o) => b.Append($"out {TypeString(o)} {o.Name.Replace('.', '_')}, "));
			return sb.Remove(sb.Length - 2, 2).Append(")").ToString();
		}

		private static string TypeString(Data data)
		{
			switch (data)
			{
				case IntegerData i:
					return "int";
				case IntegerVectorData iv:
					return "int[]";
				case DoubleData d:
					return "double";
				case StringData s:
					return "string";
				case DoubleVectorData dd:
					return "double[]";
				case DoubleMatrixData md:
					return "double[,]";
				default:
					return "object";
			}
		}

		public override Model Copy(string id, string name = null, string parentName = null)
			=> new ModelCSharp(id, Description, ModelDataInputs, ModelDataOutputs, FunctionBody, IsAuxiliary, parentName ?? ParentName, name ?? Name);

	}
}
