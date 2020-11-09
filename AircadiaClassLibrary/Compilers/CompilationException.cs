using System;
using System.CodeDom.Compiler;
//using System.Data;

namespace Aircadia.Services.Compilers
{
	public class CompilationException : Exception
	{
		public CompilerResults Results;

		public CompilationException(string message, CompilerResults results)
			: base(message) => Results = results;
	}
}
