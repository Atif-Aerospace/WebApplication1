using System.CodeDom.Compiler;

namespace Aircadia.ObjectModel.Models
{
	public interface ICompilable
	{
		CompilerResults Compile();
		void LinkToCompiledAssembly(string pathToAssembly, string className);
		string Code { get; }
		string Name { get; }
		bool Compiled { get; set; }
		string Id { get; }
	}
}