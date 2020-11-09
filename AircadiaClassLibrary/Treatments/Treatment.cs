using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;
using Aircadia.ObjectModel.DataObjects;

namespace Aircadia.ObjectModel.Treatments
{
	[Serializable()]
    //// Class for treatment. All mathematical treatments which could be applied to a model or a subprocess  is saved as an object of this class.
    public abstract class Treatment : AircadiaComponent
    {
		protected AircadiaProject Project = AircadiaProject.Instance;

		public string studyName;

        public string databaseFileName = "";
        public Result Result
        {
            get;
            set;
        }

		private long iterationSize;
		private long updatePeriod;

		public BackgroundWorker ProgressReporter { private get; set; }

		protected long IterationSize
		{
			get => iterationSize;
			set
			{
				iterationSize = value;
				updatePeriod = Math.Max(value / 100, 1);
			}
		}

		public Treatment(string name, string description, string parentName = "")
			: base(name, description, parentName)
		{
		}


		protected void EndIteratoinIfCancelled()
		{
			if (ProgressReporter.CancellationPending)
			{ 
				throw new OperationCanceledException($"Study {studyName} has been cancelled");
			}
		}

		public abstract bool ApplyOn();

		protected void ReportProgress(long iteration)
		{
			if (iteration % updatePeriod == 0)
			{
				int percentProgress = Convert.ToInt32(iteration * 100.0 / IterationSize);
				percentProgress = Math.Min(Math.Max(0, percentProgress), 100);
				ProgressReporter.ReportProgress(percentProgress);
			}
		}

		/// <summary>
		/// Executes the treatment
		/// </summary>
		/// <param name="modsub"></param>
		/// <returns></returns>
		public abstract bool ApplyOn(ExecutableComponent modsub);
		/// <summary>
		/// The string returned by this function is the one which will be printed in the properties window of AirCADia for the selected model object.
		/// </summary>
		/// <returns></returns>
		public override string ToString() => "";



		public virtual void CreateFolder()
		{
			folderPath = Path.Combine(Project.ProjectPath, "Studies", studyName) + "\\";

			// Create a folder for the study
			Directory.CreateDirectory(folderPath);

			projectPath = $"{Project.ProjectPath}\\{Project.ProjectName}.explorer";

			metadata = new XMLMetadataFiler(studyName, folderPath, projectPath);
		}

		protected string folderPath;
		protected string projectPath;
		protected XMLMetadataFiler metadata;
		

		protected string CsvPath
		{
			get
			{
				string directory = Path.GetDirectoryName(databaseFileName);
				string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(databaseFileName);
				string csvPath = Path.Combine(directory, fileNameWithoutExtension + ".csv");
				return csvPath;
			}
		}
	}

	public class XMLMetadataFiler
	{
		public string FilePath {get;}

		private readonly XDocument document;
		private readonly XElement result;
		private readonly XElement tags;
		private readonly XElement parameters;

		public XMLMetadataFiler(string studyName, string folderPath, string projectPath = "")
		{
			// Create a folder for the study
			var studyDirectoryUri = new Uri(folderPath);
			var xmlFilePathUri = new Uri(studyDirectoryUri, studyName + ".xml");
			var projectFileUri = new Uri(projectPath);
			string relativeUri = studyDirectoryUri.MakeRelativeUri(projectFileUri).ToString().Replace('/', Path.DirectorySeparatorChar);
			string relativePath = Uri.UnescapeDataString(relativeUri);

			FilePath = xmlFilePathUri.AbsolutePath.Replace("%20", " ");

			result = new XElement("Result");
			result.Add(new XElement("Project", new XAttribute("Path", relativePath)));

			tags = new XElement("Tags");
			result.Add(tags);

			parameters = new XElement("Parameters");
			result.Add(parameters);

			document = new XDocument(result);
		}

		public void AddAttribute(string name, string value)
		{
			var attribute = new XAttribute(name, value);
			result.Add(attribute);
		}

		public void AddTag(string name, string value)
		{
			var tag = new XElement("Tag", new XAttribute(name, value));
			tags.Add(tag);
		}

		
		public void AddParameter(Data data, string type, params (string name, object value)[] attributes)
		{
			var inner = new XElement(type);
			foreach ((string name, object value) in attributes)
				inner.Add(new XAttribute(name, value));

			AddParameter(data, inner);
		}
		public void AddParameter(Data data, object inner = null)
		{
			var parameter = new XElement("Parameter");
			parameter.Add(new XAttribute("Name", data.Name));
			if (data is IntegerData)
			{
				parameter.Add(new XAttribute("Type", "Integer"));
				parameter.Add(new XAttribute("Unit", ((IntegerData)data).Unit));
			}
			else if (data is DoubleData)
			{
				parameter.Add(new XAttribute("Type", "Double"));
				parameter.Add(new XAttribute("DecimalPlaces", ((DoubleData)data).DecimalPlaces));
				parameter.Add(new XAttribute("Unit", ((DoubleData)data).Unit));
			}
			else if (data is StringData)
			{
				parameter.Add(new XAttribute("Type", "String"));
			}
			else if (data is DoubleVectorData)
			{
				parameter.Add(new XAttribute("Type", "DoubleArray"));
				parameter.Add(new XAttribute("DecimalPlaces", ((DoubleVectorData)data).DecimalPlaces));
				parameter.Add(new XAttribute("Unit", ((DoubleVectorData)data).Unit));
			}
			else if (data is DoubleMatrixData)
			{
				parameter.Add(new XAttribute("Type", "DoubleMatrix"));
				parameter.Add(new XAttribute("DecimalPlaces", ((DoubleMatrixData)data).DecimalPlaces));
				parameter.Add(new XAttribute("Unit", ""));
			}
			else if (data is IntegerVectorData)
			{
				parameter.Add(new XAttribute("Type", "IntegerArray"));
				parameter.Add(new XAttribute("Unit", ((IntegerVectorData)data).Unit));
			}
			if (inner != null)
				parameter.Add(inner);
			parameters.Add(parameter);
		}

		public void Save() => document.Save(FilePath);

		public override string ToString() => document.ToString();
	}

	public class CSVFiler : IDisposable
	{
		private string row;
		private readonly StreamWriter file;

		public CSVFiler(string path)
		{
			file = File.CreateText(path);
		}

		public void NewRow() => row = String.Empty; // CSV file row

		public void AddToRow(double value, int format) => AddToRow(value.ToString($"F{format}"));

		public void AddToRow<T>(T value) => row += $"\"{value}\",";

		public void AddToRow(Data data) => AddToRow(data.ValueAsString);

		public void WriteRow()
		{
			row = row.TrimEnd(',');
			file.WriteLine(row);
		}
		
		public void Dispose() => file.Dispose();
	}
}
