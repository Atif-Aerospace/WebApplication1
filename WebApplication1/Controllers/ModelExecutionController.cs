using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WebApplication1.Execution;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ModelExecutionController : ControllerBase
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



            // Execute model
            AircraftDesign em = new AircraftDesign();
            Type type = em.GetType();
            var methodInfo = type.GetMethod(model.Name);
            object[] ppp = parameters.ToArray();
            methodInfo.Invoke(em, ppp);




            // Set outputs for json
            int inputs_Count = inputsJson.Count();
            for (int i = 0; i < outputs.Count(); i++)
                outputs[i].Value = ppp[inputs_Count + i].ToString();
            
            return model;
        }



        
    }

    public class ExecutionModel
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public string Uri { get; set; }

        public List<Data> Inputs { get; set; }

        //public List<Data> Inputs = new List<Data>();
        public List<Data> Outputs { get; set; }
    }
    public class Data
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
