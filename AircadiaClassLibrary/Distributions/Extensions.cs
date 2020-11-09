using Aircadia.Numerics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aircadia.ObjectModel.Distributions
{
	public static class ProbabilityExtensions
	{
        public static IEnumerable<PropertyInfo> GetParameterObjects(this IProbabilityDistribution distribution)
        {
            Type type = distribution.GetType();
            return type.GetProperties().Where(p => Attribute.GetCustomAttribute(p, typeof(OptionAttribute)) is OptionAttribute);
        }


        public static Dictionary<string, double> GetParameterValuePairs(this IProbabilityDistribution distribution) => distribution.GetParameterObjects().ToDictionary(p => p.Name, p => (double)p.GetValue(distribution));

        public static string[] GetParameterNames(this IProbabilityDistribution distribution) => distribution.GetParameterValuePairs().Keys.ToArray();

		public static string GetParameterValueString(this IProbabilityDistribution distribution) => distribution.GetParameterValuePairs().Aggregate(String.Empty, (t, l) => t += $"{l.Key} = {l.Value}, ").TrimEnd(' ', ',');

		public static string GetTypeName(this IProbabilityDistribution distribution) => distribution.GetType().Name;

		public static int GetNParameters(this IProbabilityDistribution distribution) => distribution.GetParameterValuePairs().Count;
	}
}
