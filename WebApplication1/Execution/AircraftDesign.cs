using System;
using System.Collections.Generic;
using System.Linq;
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
    }
}
