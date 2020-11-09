using System;
using System.Collections.Generic;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.ObjectModel.Treatments;
using Aircadia.Services.Serializers;

namespace Aircadia.ObjectModel.Studies
{
	[Serializable()]
    //// Class for study. A model or a subprocess with a treatmnet applied is saved as an object of this class.
    public class Study : ExecutableComponent
    {
		protected AircadiaProject Project = AircadiaProject.Instance;

		private Treatment _treatment;
		[Serialize(ConstructorOnly = true)]
        public Treatment Treatment
		{
			get => _treatment;
			set
			{
				_treatment = value;
				if (_treatment != null)
					_treatment.studyName = Name;
			}
		}
		[Serialize("Component", SerializationType.Reference)]
		public virtual ExecutableComponent StudiedComponent { get; set; }
		public List<Result> Results { get; } = new List<Result>();
		public Result ActiveResult
        {
            get;
            set;
        }

		public virtual string StudyType => "Study";


		protected void SetIDColumn()
		{
			ColumnNames.Add("ID");
			ColumnTypes.Add(DataTypes.INTEGER);
			ColumnFormats.Add(0);
			ColumnUnits.Add("");
		}

		protected void SetColumn(string columnHeader, Data data)
		{
			ColumnNames.Add(columnHeader);
			if (data is IntegerData id)
			{
				ColumnTypes.Add(DataTypes.INTEGER);
				ColumnFormats.Add(0);
				ColumnUnits.Add(id.Unit);
			}
			else if (data is DoubleData dd)
			{
				ColumnTypes.Add(DataTypes.DOUBLE);
				ColumnFormats.Add(dd.DecimalPlaces);
				ColumnUnits.Add(dd.Unit);
			}
		}
		public List<string> ColumnNames { get; } = new List<string>();

		public List<DataTypes> ColumnTypes { get; } = new List<DataTypes>();

		public List<string> ColumnUnits { get; } = new List<string>();

		public List<double> MinValues { get; } = new List<double>();

		public List<double> MaxValues { get; } = new List<double>();

		public List<int> ColumnFormats { get; } = new List<int>();

		public List<string> TableNames = new List<string>();



        public Study(string name, string description) :
            base(name, description)
        {
        }

		/// <summary>
		/// Study Constructer
		/// </summary>
		/// <param name="name"></param>
		/// <param name="treatment"></param>
		/// <param name="studiedComponent"></param>
		/// <param name="description"></param>
		[DeserializeConstructor]
		public Study(string name, Treatment treatment, ExecutableComponent studiedComponent, string description = "", string parentName = "") :
            base(name.Replace(" ", ""), description, parentName)
        {
            Treatment = treatment;
            StudiedComponent = studiedComponent;
            ModelDataInputs = studiedComponent?.ModelDataInputs;
            ModelDataOutputs = studiedComponent?.ModelDataOutputs;

			Treatment?.CreateFolder();
        }
        /// <summary>
        /// Study Executer
        /// </summary>
        /// <returns></returns>
        public override bool Execute()
        {
            bool status;
			Treatment.Result = ActiveResult;
            status = Treatment.ApplyOn(StudiedComponent);
            return status;
        }
        /// <summary>
        /// The string returned by this function is the one which will be printed in the properties window of AirCADia for the selected study object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string output = "Name: " + Name + "\r\n";
            output = output + "Treatment: " + Treatment.Name + "\r\n";
            //output = output + "Target: " + modsub.name + "\r\n";
            output = output + "Treatment Info: " + StudiedComponent.Name + "\r\n";
            output = output + Treatment.ToString();
            return output;
        }

		public virtual Study Copy(string id, string name = null, string parentName = null)
		{
			return new Study(id, Treatment, StudiedComponent, Description, parentName ?? parentName);
		}
	}
}
