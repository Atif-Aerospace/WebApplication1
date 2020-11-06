using Microsoft.AspNetCore.Routing.Constraints;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.Execution
{
    public class AircraftDesign
    {
        public void AddNumbers(double x1, double x2, out double y1)
        {
            y1 = x1 + x2;
        }

        public void MultiplyNumbers(double x1, double x2, out double y1)
        {
            y1 = x1 * x2;
        }


        public void FlopsModel(double SW, double AR, out double Range)
        {
            List<string> flopsInputLines = new List<string>();


            string bbb = Environment.GetEnvironmentVariable("HOME");
            string rootPath = "";
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("HOME")))
                rootPath = Environment.GetEnvironmentVariable("HOME") + "\\site\\wwwroot\\bin";
            else
                rootPath = "";

            string fileName = rootPath + @"..\flops\xAtif.in";
            StreamReader file = new StreamReader(fileName);

            string line;
            while ((line = file.ReadLine()) != null)
            {
                flopsInputLines.Add(line);
            }

            StringBuilder aStringBuilder;
            // SW
            string line_SW = flopsInputLines[17];
            aStringBuilder = new StringBuilder(line_SW);
            aStringBuilder.Remove(5, 7);
            string text_SW = string.Format($"{{0,7:{"#.00"}}}", SW);
            aStringBuilder.Insert(5, text_SW);
            flopsInputLines[17] = aStringBuilder.ToString();
            // AR
            string line_AR = flopsInputLines[17];
            aStringBuilder = new StringBuilder(line_AR);
            aStringBuilder.Remove(17, 5);
            string text_AR = string.Format($"{{0,5:{"#.00"}}}", AR);
            aStringBuilder.Insert(17, text_AR);
            flopsInputLines[17] = aStringBuilder.ToString();


            // Create new flops input file
            File.WriteAllLines(@"..\flops\xAtif_new.in", flopsInputLines);



            // Execute Flops
            System.Diagnostics.Process proc = new System.Diagnostics.Process();
            proc.StartInfo.WorkingDirectory = @"..\flops";
            proc.StartInfo.FileName = @"..\flops\xAtif.bat";
            //proc.StartInfo.Arguments = @\"" + this.EXEArgs + "\";
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardError = false;
            proc.StartInfo.CreateNoWindow = true;
            try
            {
                proc.Start();
                proc.WaitForExit();
            }
            catch
            {
                throw new Exception("Error");
            }
            finally
            {
                proc.Close();
            }



            // Extract output
            List<string> flopsOutputLines = new List<string>();
            string outputFileName = @"..\flops\xAtif.out";
            StreamReader outputFile = new StreamReader(outputFileName);
            while ((line = outputFile.ReadLine()) != null)
            {
                flopsOutputLines.Add(line);
            }
            outputFile.Close();

            // Range
            string AnchorText = "#OBJ/VAR/CONSTR SUMMARY";
            int skipLines = 3;
            int startIndex = 11;
            int endIndex = 17;
            int lineNumber = -1;
            for (int i = 0; i < flopsOutputLines.Count; i++)
            {
                if (flopsOutputLines[i].Contains(AnchorText))
                {
                    lineNumber = i;
                    break;
                }
            }
            line = flopsOutputLines[lineNumber + skipLines];
            string scalarValue = "";
            if (line != null && line.Length > endIndex)
                scalarValue = line.Substring(startIndex, endIndex - startIndex);

            Range = Convert.ToDouble(scalarValue);
        }
    }
}
