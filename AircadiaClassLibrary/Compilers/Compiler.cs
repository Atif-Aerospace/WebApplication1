using Aircadia.ObjectModel.Models;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Aircadia.Services.Compilers
{
	public class Compiler
	{
		private static Dictionary<string, ICompilable> models = new Dictionary<string, ICompilable>();
		private static Dictionary<string, ICompilable> auxModels = new Dictionary<string, ICompilable>();

		public static string ProjectPath { get; set; }

		public static CompilerResults CompileFunction(string className, string function)
		{

			string code = "";
			code += "using System;\n";
			code += "using System.IO;\n";
			code += "using System.Collections;\n";
			code += "using System.Collections.Generic;\n";
			code += "using System.Text;\n";
			code += "using System.Runtime.InteropServices;\n";
			code += "using System.ComponentModel;\n";
			code += "using System.Reflection;\n\n";


			code += "namespace Aircadia\n";
			code += "{\n";
			code += $"\tpublic class {className}Class\n";
			code += "\t{\n";

			code += function;

			code += "\t}\n";
			code += "}\n";


			// Create C# file for this project
			//File.WriteAllText(ProjectName + ".cs", lcCode);

			return Compile(code, className);
		}

		public static CompilerResults CompileAll(string name)
		{
			string className = "AircadiaProjectClass";

			string code = "";
			code += "using System;\n";
			code += "using System.IO;\n";
			code += "using System.Collections;\n";
			code += "using System.Collections.Generic;\n";
			code += "using System.Text;\n";
			code += "using System.Runtime.InteropServices;\n";
			code += "using System.ComponentModel;\n";
			code += "using System.Reflection;\n";

			//lcCode += "using MathWorks.MATLAB.NET.Arrays;\n";

			code += $"namespace Aircadia\n";
			code += "{\n";
			code += $"\tpublic class {className}\n";
			code += "\t{\n";

			foreach (string key in models.Keys)
			{
				code += models[key].Code;
			}
			foreach (string key in auxModels.Keys)
			{
				code += auxModels[key].Code;
			}

			code += "\n";
			code += "\t}\n";
			code += "}\n";

			CompilerResults results = Compile(code, name);
			string pathToAssembly = results.PathToAssembly;

			if (results.Errors.HasErrors)
			{
				throw new CompilationException("Model Compilation failed", results);
			}
			else if (!File.Exists(pathToAssembly))
			{
				throw new FileNotFoundException("The .dll has not been created", pathToAssembly);
			}

			foreach (ICompilable model in models.Values)
			{
				model.LinkToCompiledAssembly(pathToAssembly, className);
			}
			foreach (ICompilable model in auxModels.Values)
			{
				model.LinkToCompiledAssembly(pathToAssembly, className);
			}

			return results;
		}

		public static CompilerResults Compile(string code, string outputName)
		{
			using (var compiler = new CSharpCodeProvider())
			{
				var parameters = new CompilerParameters();

				// *** Start by adding any referenced assemblies
				parameters.ReferencedAssemblies.Add("System.dll");
				parameters.ReferencedAssemblies.Add("System.Data.dll");
				parameters.ReferencedAssemblies.Add("System.Windows.Forms.dll");

				// *** Save the results into a file 
				parameters.GenerateInMemory = false;

				// *** Create a folder to save the results
				var modelsPath = Path.Combine(ProjectPath /*?? Directory.GetCurrentDirectory()*/, "Models");
				if (!Directory.Exists(modelsPath))
				{
					Directory.CreateDirectory(modelsPath);
				}

				for (int i = 0; i < 20; i++)
				{
					// *** Set the path of the resulting file 
					string dllPath = Path.Combine(modelsPath, $"{outputName}.{i}.dll");
					parameters.OutputAssembly = dllPath;

					try
					{
						// *** Now compile the whole thing
						return compiler.CompileAssemblyFromSource(parameters, code);
					}
					catch (System.Exception)
					{
						if (i == 19)
						{
							throw;
						}
					}
				}

				throw new System.Exception();
			}
		}

		public static void AddModel(ICompilable model)
		{
			if (!models.ContainsKey(model.Id))
				models.Add(model.Id, model);
			else models[model.Id] = model;
		}

		public static void AddAuxModel(ICompilable model)
		{
			if (!auxModels.ContainsKey(model.Id))
				auxModels.Add(model.Id, model);
			else auxModels[model.Id] = model;
		}
	}
}
