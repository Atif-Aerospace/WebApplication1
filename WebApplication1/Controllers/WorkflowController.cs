using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


using Newtonsoft.Json.Linq;

using Aircadia.Services.Serializers;
using Aircadia;
using Aircadia.ObjectModel.Workflows;
using Aircadia.ObjectModel.Models;
using Aircadia.Services.Compilers;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowController : ControllerBase
    {


        [HttpGet]
        public ActionResult<ExecutionModel> GetData()
        {
            JObject requestBodyJson = null;

            using (var reader = new StreamReader(Request.Body))
            {
                string requestBody = reader.ReadToEnd();
                requestBodyJson = JObject.Parse(requestBody);
            }



            List<object> parameters = new List<object>();


            ExecutionModel model = new ExecutionModel();

            model.Name = (string)(requestBodyJson["Name"]);
            //model.Description = "Add two numbers";
            //model.Uri = "http://127.0.0.1:5000/ExecuteModel";

            // Inputs
            List<Data> inputs = new List<Data>();
            var inputsJson = requestBodyJson["Inputs"];
            for (int i = 0; i < inputsJson.Count(); i++)
            {
                Data data = new Data();
                data.Name = (string)(inputsJson[i]["Name"]);
                data.Value = (string)(inputsJson[i]["Value"]);
                parameters.Add(Convert.ToDouble(data.Value));
                inputs.Add(data);
            }
            model.Inputs = inputs;

            // Outputs
            List<Data> outputs = new List<Data>();
            var outputsJson = requestBodyJson["Outputs"];
            for (int i = 0; i < outputsJson.Count(); i++)
            {
                Data data = new Data();
                data.Name = (string)(outputsJson[i]["Name"]);
                data.Value = (string)(outputsJson[i]["Value"]);
                parameters.Add(Convert.ToDouble(data.Value));
                outputs.Add(data);
            }
            model.Outputs = outputs;







            string FileName = @"C:\home\site\repository\WindTurbine.explorer";

            AircadiaProject.Initialize("WindTurbine", @"C:\home\site\repository");
            AircadiaXmlSerializer.OpenProjectXML(FileName);

            AircadiaProject Project = AircadiaProject.Instance;

            Workflow workflow = null;
            foreach (Workflow wf in Project.WorkflowStore)
            {
                if (wf.Name == model.Name)
                {
                    //wf.PrepareForExecution();
                    workflow = wf;
                    break;
                }
            }
            if (workflow != null)
            {
                workflow.Execute();



                // Set outputs for json
                int inputs_Count = inputsJson.Count();
                for (int i = 0; i < outputs.Count(); i++)
                    outputs[i].Value = workflow.ModelDataOutputs[i].Value.ToString();
            }

            return model;

        }


		//public bool CreateDLL(AircadiaProject Project)
		//{
		//	if (Project is null)
		//	{
		//		return false;
		//	}

		//	foreach (Model md in Project.ModelStore.Where(m => m is ICompilable))
		//	{
		//		Compiler.AddModel(md as ICompilable);
		//	}
		//	foreach (Model md in Project.AuxiliaryModelStore.Where(m => m is ICompilable))
		//	{
		//		Compiler.AddAuxModel(md as ICompilable);
		//	}

		//	System.CodeDom.Compiler.CompilerResults results = Compiler.CompileAll(Project.ProjectName);
		//	string dllfile = results.PathToAssembly;

		//	if (results.Errors.HasErrors)
		//	{
		//		MessageBox.Show($"{results.Errors.Count} errors while creating project dll", "Compilation errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
		//		return false;
		//	}
		//	else if (!File.Exists(dllfile))
		//	{
		//		MessageBox.Show("dll not created");
		//		return false;
		//	}

		//	foreach (Model md in Project.ModelStore)
		//	{
		//		md.PrepareForExecution();
		//	}
		//	foreach (Model md in Project.AuxiliaryModelStore)
		//	{
		//		md.PrepareForExecution();
		//	}

		//	foreach (Study study in Project.StudyStore)
		//	{
		//		study?.Treatment.CreateFolder();
		//	}
		//	return true;
		//}





	}
}
