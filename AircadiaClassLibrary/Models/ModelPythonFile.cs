using System;
using System.Collections.Generic;
using System.IO;
using Aircadia.ObjectModel.DataObjects;
using Aircadia.Services.Serializers;

using System.Diagnostics;

namespace Aircadia.ObjectModel.Models
{
	[Serializable()]
	public class ModelPythonFile : ModelPython
	{
		[Serialize(Type = SerializationType.Path)]
		public string FilePath { get; set; }


        public string WorkingDirectory { get; protected set; }
        public string ExecutableFilePath { get; protected set; }
        public string ExecutableFileArguments { get; protected set; }
        public static bool CreateNoWindow { get; set; }

        [DeserializeConstructor]
		public ModelPythonFile(string name, string description, List<Data> modelDataInputs, List<Data> modelDataOutputs, 
			string filePath, string parentName = "", string displayName = "") 
			: base(name, description, modelDataInputs, modelDataOutputs, parentName, displayName)
		{
			FilePath = filePath;

			Code= File.ReadAllText(FilePath);
		
			// Generate model code
			PrepareForExecution();
		}

        public override bool Execute()
        {
            this.WorkingDirectory = @"D:\TestPython";
            this.ExecutableFilePath = "python";
            this.ExecutableFileArguments = "Add.py";


            //foreach (Data data in ModelDataInputs)
            //{
            //    scope.SetVariable(data.Name, data.Value);
            //}

            //engine.Execute(Code, scope);

            //foreach (Data data in ModelDataOutputs)
            //{
            //    data.Value = scope.GetVariable(data.Name);
            //}

            Process proc = new Process();
            proc.StartInfo.WorkingDirectory = WorkingDirectory;
            proc.StartInfo.FileName = ExecutableFilePath;
            proc.StartInfo.Arguments = ExecutableFileArguments;
            proc.StartInfo.UseShellExecute = true;
            proc.StartInfo.RedirectStandardError = false;
            proc.StartInfo.CreateNoWindow = CreateNoWindow;
            proc.Start();
            proc.WaitForExit();
            proc.Close();

            return true;
        }

        public override string ModelType => "Python File";

		public override Model Copy(string id, string name = null, string parentName = null)
			=> new ModelPythonFile(id, Description, ModelDataInputs, ModelDataOutputs, FilePath, parentName ?? ParentName, name ?? Name);
	}
}
