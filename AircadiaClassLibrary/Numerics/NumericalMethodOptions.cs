using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aircadia.Numerics
{
	public class NumericalMethodOptions : INumericalMethodOptions
	{
		private readonly Dictionary<string, PropertyInfo> options;
		private readonly object managed;

		public virtual List<string> Names => options.Keys.ToList();

		public NumericalMethodOptions(object managed)
		{
			this.managed = managed;

			var optionProps = GetOptionProperties().ToList();

			options = new Dictionary<string, PropertyInfo>(optionProps.Count);

			foreach (PropertyInfo option in optionProps)
			{
				string name = (Attribute.GetCustomAttribute(option, typeof(OptionAttribute)) as OptionAttribute).DisplayName;
				options.Add(name, option);
			}

		}

		public string this[string key]
		{
			get => options[key].GetValue(managed).ToString();
			set => options[key].SetValue(managed, Convert.ChangeType(value, options[key].PropertyType));
		}

		private IEnumerable<PropertyInfo> GetOptionProperties()
		{
			Type solverType = managed.GetType();
			return solverType.GetRuntimeProperties()
				.Where(f => Attribute.GetCustomAttribute(f, typeof(OptionAttribute)) as OptionAttribute != null);
		}
	}
}