using Aircadia.Services.Serializers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aircadia.ObjectModel
{
	[Serializable]
    public abstract class AircadiaComponent : NotifyPropertyChangedBase, IAircadiaComponent, IComparable<IAircadiaComponent>
	{
		[Serialize]
        public string Name { get; private set; }
		[Serialize(Type = SerializationType.Lines)]
		public string Description { get; }

        public string Id { get; }

        [Serialize]
        public string ParentName { get; } = "";

		public string ParentDisplayName
		{
			get
			{
				string[] parentName = this.ParentName.Split('.');
				return parentName[parentName.Length - 1];
			}
		}

		public string FullName => String.IsNullOrWhiteSpace(ParentName) ? Name : ParentName + "." + Name;

		public AircadiaComponent(string name, string description, string parentName)
        {
			Name = name?.Trim() ?? throw new ArgumentNullException(nameof(name));
			Description = description?.Trim() ?? String.Empty;
			Id = Name;
			ParentName = parentName;

		}

		public void Rename(string name) => Name = name;

		public override string ToString() => Id;

		//public virtual string PropertiesSummaryText() => ToString();

		public int CompareTo(IAircadiaComponent other) => Name.CompareTo(other.Name);
	}

	public static class AircadiaComponentExtensions
	{
		public static IEnumerable<string> GetNames<T>(this IEnumerable<T> dataObjects)
			where T : INamedComponent => dataObjects.Select(d => d.Name);
	}

	public interface IAircadiaComponent : INamedComponent
	{
		string Description { get; }
    }

	public interface INamedComponent
	{
		string Name { get; }
	}
}
