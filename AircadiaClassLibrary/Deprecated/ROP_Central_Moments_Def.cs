/**************************************************************************************************
AIRCADIA - Multiphysics model-based design tool                                                   *
Engineering Design Group                                                                          *
Department of Aerospace Engineering, School of Engineering, Cranfield University                  *
Cranfield, Bedfordshire, MK43 0AL, UK                                                             *
/**************************************************************************************************/

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Aircadia.ObjectModel.DataObjects;

namespace Aircadia
{
    [Serializable()]
    public partial class ROP_Central_Moments_Def : Form
    {
        public Data dataObj;

        public ROP_Central_Moments_Def(Data lio)
        {
            dataObj = lio;
            InitializeComponent();
            label_ROP_CMdef_Name.Text = dataObj.Name;
        }

        private void button_ROP_CMdef_Set_Click(object sender, EventArgs e)
        {

        }
    }
}
