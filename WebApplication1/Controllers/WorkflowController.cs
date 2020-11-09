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
        public void GetData()
        {
            

            string FileName = @"C:\Users\s130030\OneDrive - Cranfield University\Desktop\WebApplication1\WindTurbine.explorer";

            AircadiaProject.Initialize("WindTurbine", @"C: \Users\s130030\OneDrive - Cranfield University\Desktop\WebApplication1");
            AircadiaXmlSerializer.OpenProjectXML(FileName);

            AircadiaProject Project = AircadiaProject.Instance;

            foreach (Workflow wf in Project.WorkflowStore)
            {
                //wf.PrepareForExecution();
                wf.Execute();

            }

            //Workflow wf = AircadiaProject.Instance.WorkflowStore.First;

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
