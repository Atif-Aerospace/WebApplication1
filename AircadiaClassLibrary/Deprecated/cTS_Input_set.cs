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
    ////This class is defined for the treatment input set for TradeStudy
    public class TS_Input_set: InputSet
    {
        public double Increment;
        public double min;
        public double max;
        public string Data;
        /// <summary>
        /// Constructer. 
        /// </summary>
        /// <param name="list"></param>
        public TS_Input_set(params object[] list)
        {
            int i = 0;
            while (i < list.Length)
            {
                if (list[i] as string == "Increment")
                {
                    Increment = (double)list[i + 1];
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
                else
                {
					throw new ArgumentException($"Item number {i} of the input list '{list[i]}'  is an incorrect options");
                }
                i = i + 2;
            }
        }
    }
}
