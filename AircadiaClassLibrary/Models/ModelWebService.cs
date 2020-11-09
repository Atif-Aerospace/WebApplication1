using System;
using System.Collections.Generic;

using Aircadia.ObjectModel.DataObjects;

using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Aircadia.ObjectModel.Models
{
    public class ModelWebService : Model
    {
        static HttpClient client = new HttpClient();

        public string EndPoint;
        public ModelWebService(string name, string description, List<Data> dataInputs, List<Data> dataOutputs, string endPoint, string parentName = "")
            : base(name, "Web Service Model", dataInputs, dataOutputs, parentName: parentName)
        {
            this.EndPoint = endPoint;
        }

        public override string ModelType => "WebService";

        public override bool Execute()
        {
            Task task = Task.Run(async () => await Test());
            task.Wait();

            return true;
        }

        public override void PrepareForExecution()
        {

        }

        public override Model Copy(string id, string name = null, string parentName = null) => throw new NotImplementedException();





        async Task Test()
        {
            dynamic jsonObj = new JObject();
            jsonObj.Name = "FlopsModel"; // this.Name;

            JArray inp = new JArray();
            foreach (Data data in ModelDataInputs)
            {
                dynamic jsonObject = new JObject();
                jsonObject.Name = data.Name;
                jsonObject.Value = (double)(data.Value);
                inp.Add(jsonObject);
            }
            jsonObj.Inputs = inp;

            JArray outp = new JArray();
            foreach (Data data in ModelDataOutputs)
            {
                dynamic jsonObject = new JObject();
                jsonObject.Name = data.Name;
                jsonObject.Value = (double)(data.Value);
                outp.Add(jsonObject);
            }
            jsonObj.Outputs = outp;
            string xxxxx = jsonObj.ToString();

            // Create json
            string json = "{\"Name\": \"FlopsModel\",\"Inputs\": [{\"Name\": \"SW\", \"Value\": 1200.00},{\"Name\": \"AR\", \"Value\": 11}],\"Outputs\": [{\"Name\": \"Range\", \"Value\": 0}]}";
            json = xxxxx;


            


            // Call web api
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(EndPoint),
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };

            HttpResponseMessage response = await client.SendAsync(request);
            string jsonData = "";
            if (response.IsSuccessStatusCode)
            {
                string sss = response.StatusCode.ToString();
                jsonData = response.Content.ReadAsStringAsync().Result;
            }


            JObject o = JObject.Parse(jsonData);
            var ooo = o["outputs"];
            for (int i = 0; i < ModelDataOutputs.Count; i++)
            {
                ModelDataOutputs[i].Value = Convert.ToDouble(ooo[i]["value"]);
            }



        }

    }
}