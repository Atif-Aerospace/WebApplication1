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
using System.Collections;

namespace Aircadia
{
    [Serializable()]
    ////This object of this class are the inputs and outputs of the 'treatment'
    public class Treatment_InOut
    {
        public ArrayList setuplist = new ArrayList();
        public string output;
        public ArrayList outputarray;
        /// <summary>
        /// Constructer
        /// </summary>
        public Treatment_InOut()
        {
        }
        /// <summary>
        /// Constructer
        /// </summary>
        /// <param name="sl"></param>
        public Treatment_InOut(ArrayList sl)
        {
            setuplist.AddRange(sl.ToArray());
        }
        /// <summary>
        /// Constructer
        /// </summary>
        /// <param name="ot"></param>
        public Treatment_InOut(string ot)
        {
            output = ot;
        }
        /// <summary>
        /// Constructer
        /// </summary>
        /// <param name="wht"></param>
        /// <param name="sl"></param>
        public Treatment_InOut(string wht, ArrayList sl)
        {
            outputarray.AddRange(sl.ToArray());
        }
    }
}
