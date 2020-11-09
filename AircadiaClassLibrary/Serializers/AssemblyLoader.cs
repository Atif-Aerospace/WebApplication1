using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Aircadia.Services.Serializers
{
	public static class AssemblyLoader
	{
		static AssemblyLoader()
		{
			LoadAssembly(Assembly.GetExecutingAssembly()/*typeof(AssemblyLoader).Assembly*/);
		}

		private static readonly Dictionary<string, Type> types = new Dictionary<string, Type>();


		private static void LoadAssembly(string path)
		{
			byte[] bytes = File.ReadAllBytes(path);
			var assembly = Assembly.Load(bytes);
			LoadAssembly(assembly);
		}

		private static void LoadAssembly(Assembly assembly)
		{
			foreach (Type type in assembly.GetTypes())
			{
				types[type.Name] = type;
			}
		}

		public static Type GetType(string typeName)
		{
			if (types.TryGetValue(typeName, out Type type))
			{
				return type;
			}
			return null;
		}
	}
}
