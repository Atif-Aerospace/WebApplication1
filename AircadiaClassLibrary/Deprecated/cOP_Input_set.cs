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
    //// This is subclass for (Specially created for optimiser) treatment input/output
    public class OP_Input_set : InputSet
    {
        
        public double min;
        public double max;
        public string Data;
        public double IICM;
        public double IIICM;
        public double IVCM;
        public string desobj;
        public string k_coeff;
        public string LossFuncSign;
        public double Probability;
        public string Distribution;
        public string Metric;
        public double Parameter1;
        public double Parameter2;
        public OP_Input_set(params object[] list)
        {
            int i = 0;
            while (i < list.Length)
            {
                if (list[i] as string =="desobj")
                {
                    desobj = list[i + 1] as string;
                }
                else if (list[i] as string == "min")
                {
                    min = (double)list[i + 1];
                }
                else if (list[i] as string == "max")
                {
                    max = (double)list[i + 1];
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
                    if (list[i + 1] as string != "")
                        Probability = Convert.ToDouble(list[i + 1]);
                }
                else if (list[i] as string == "Distribution")
                {
                    Distribution = list[i + 1] as string;
                }
                    else if (list[i] as string == "Metric")
                {
                    Metric = list[i + 1] as string;
                }
                else if (list[i] as string == "Parameter1")
                {
                    Parameter1 = Convert.ToDouble(list[i + 1]);
                }
                else if (list[i] as string == "Parameter2")
                {
                    Parameter2 = Convert.ToDouble(list[i + 1]);
                }
                else
                {
					throw new ArgumentException($"Item number {i} of the input list '{list[i]}'  is an incorrect options");
                }
                i = i + 2;
            }
        }
    }
}
