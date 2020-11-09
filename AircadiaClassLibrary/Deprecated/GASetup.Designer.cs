namespace Aircadia.ObjectModel.Studies
{
    partial class GASetup
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.labelTotalGenerations = new System.Windows.Forms.Label();
            this.textBoxTotalGenerations = new System.Windows.Forms.TextBox();
            this.labelPopulationSize = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // labelTotalGenerations
            // 
            this.labelTotalGenerations.AutoSize = true;
            this.labelTotalGenerations.Location = new System.Drawing.Point(44, 40);
            this.labelTotalGenerations.Name = "labelTotalGenerations";
            this.labelTotalGenerations.Size = new System.Drawing.Size(91, 13);
            this.labelTotalGenerations.TabIndex = 0;
            this.labelTotalGenerations.Text = "Total Generations";
            // 
            // textBoxTotalGenerations
            // 
            this.textBoxTotalGenerations.Location = new System.Drawing.Point(141, 37);
            this.textBoxTotalGenerations.Name = "textBoxTotalGenerations";
            this.textBoxTotalGenerations.Size = new System.Drawing.Size(100, 20);
            this.textBoxTotalGenerations.TabIndex = 1;
            // 
            // labelPopulationSize
            // 
            this.labelPopulationSize.AutoSize = true;
            this.labelPopulationSize.Location = new System.Drawing.Point(47, 67);
            this.labelPopulationSize.Name = "labelPopulationSize";
            this.labelPopulationSize.Size = new System.Drawing.Size(80, 13);
            this.labelPopulationSize.TabIndex = 2;
            this.labelPopulationSize.Text = "Population Size";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(141, 67);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 3;
            // 
            // buttonOK
            // 
            this.buttonOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOK.Location = new System.Drawing.Point(60, 158);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(151, 157);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // GASetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(288, 220);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.labelPopulationSize);
            this.Controls.Add(this.textBoxTotalGenerations);
            this.Controls.Add(this.labelTotalGenerations);
            this.Name = "GASetup";
            this.Text = "GASetup";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label labelTotalGenerations;
        private System.Windows.Forms.TextBox textBoxTotalGenerations;
        private System.Windows.Forms.Label labelPopulationSize;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
    }
}