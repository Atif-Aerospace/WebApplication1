/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
namespace Aircadia
{
    [Serializable()]
    //// This is subclass for (Specially created for constraints in optimiser) treatment input/output
    public class OP_C_Input_set : InputSet
    {
        public int Constraint;
        public double value;
        public string Data;
        public double IICM;
        public double IIICM;
        public double IVCM;
        public string k_coeff;
        public string LossFuncSign;
        public string Probability;
        public string Distribution;
        public string Metric;
        public double Parameter1;
        public double Parameter2;
        public string InequalitySign;
        public OP_C_Input_set(params object[] list)
        {
            int i = 0;
            while (i < list.Length)
            {
                if (list[i] as string == "Constraint")
                {
                    Constraint = (int)list[i + 1];
                }
                else if (list[i] as string == "Value" && list[i + 1] as string != "")
                {
                    value = Convert.ToDouble(list[i + 1]);
                }
                else if (list[i] as string == "Data")
                {
                    Data = list[i + 1] as string;
                }
                else if (list[i] as string == "IICM")
                {
                    IICM = Convert.ToDouble(list[i + 1]);
                }
                else if (list[i] as string == "IIICM")
                {
                    IIICM = Convert.ToDouble(list[i + 1]);
                }
                else if (list[i] as string == "IVCM")
                {
                    IVCM = Convert.ToDouble(list[i + 1]);
                }
                else if (list[i] as string == "k_coeff")
                {
                    k_coeff = list[i + 1] as string;
                }
                else if (list[i] as string == "LossFuncSign")
                {
                    LossFuncSign = list[i + 1] as string;
                }
                else if (list[i] as string == "Probability")
                {
                    Probability = Convert.ToString(list[i + 1]);
                }
                else if (list[i] as string == "Distribution")
                {
                    Distribution = Convert.ToString(list[i + 1]);
                }
                else if (list[i] as string == "Metric")
                {
                    Metric = Convert.ToString(list[i + 1]);
                }
                else if (list[i] as string == "Parameter1")
                {
                    Parameter1 = Convert.ToDouble(list[i + 1]);
                }
                else if (list[i] as string == "Parameter2")
                {
                    Parameter2 = Convert.ToDouble(list[i + 1]);
                }
                else if (list[i] as string == "InequalitySign")
                {
                    InequalitySign = list[i + 1] as string;
                }
                else
                {
                    throw new ArgumentException($"Item number {i} of the input list '{list[i]}'  is an incorrect options");
                }
                i = i + 2;
            }
            if (list.Length <= 4)
            {
                Constraint = 2;
            }
            else if (list.Length == 10)
            {
                Constraint = 3;
            }
        }
    }
}
