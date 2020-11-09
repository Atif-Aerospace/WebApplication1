namespace Aircadia
{
    partial class ROP_ConstantSetup
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
            this.textBox_ROP_constant_Value = new System.Windows.Forms.TextBox();
            this.button_ROP_constant_set = new System.Windows.Forms.Button();
            this.label_ROP_constant_Name = new System.Windows.Forms.Label();
            this.label_ROP_constant_equal = new System.Windows.Forms.Label();
            this.radioButton_ROP_CertainConstant = new System.Windows.Forms.RadioButton();
            this.radioButton_ROP_UncertainConstant = new System.Windows.Forms.RadioButton();
            this.button_ROP_constant_cancel = new System.Windows.Forms.Button();
            this.textBox_ROP_ConstSetup_PDFparameter2 = new System.Windows.Forms.TextBox();
            this.textBox_ROP_ConstSetup_PDFparameter1 = new System.Windows.Forms.TextBox();
            this.label_ROP_VarSetup_PDFparameter2 = new System.Windows.Forms.Label();
            this.label_ROP_VarSetup_PDFparameter1 = new System.Windows.Forms.Label();
            this.comboBox_ROP_VarSetup_PDF = new System.Windows.Forms.ComboBox();
            this.pictureBox_ROP_VarSetup_PDFimage = new System.Windows.Forms.PictureBox();
            this.label_ROP_VarSetup_PDF = new System.Windows.Forms.Label();
            this.textBox_ROP_ConstSetup_PDFparameter3 = new System.Windows.Forms.TextBox();
            this.label_ROP_VarSetup_PDFparameter3 = new System.Windows.Forms.Label();
            this.textBox_ROP_ConstSetup_PDFparameter4 = new System.Windows.Forms.TextBox();
            this.label_ROP_VarSetup_PDFparameter4 = new System.Windows.Forms.Label();
            this.label_ROP_VarSetup_PDFparameter6 = new System.Windows.Forms.Label();
            this.textBox_ROP_ConstSetup_PDFparameter6 = new System.Windows.Forms.TextBox();
            this.textBox_ROP_ConstSetup_PDFparameter5 = new System.Windows.Forms.TextBox();
            this.label_ROP_VarSetup_PDFparameter5 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_ROP_VarSetup_PDFimage)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox_ROP_constant_Value
            // 
            this.textBox_ROP_constant_Value.Location = new System.Drawing.Point(251, 54);
            this.textBox_ROP_constant_Value.Name = "textBox_ROP_constant_Value";
            this.textBox_ROP_constant_Value.Size = new System.Drawing.Size(68, 20);
            this.textBox_ROP_constant_Value.TabIndex = 10;
            this.textBox_ROP_constant_Value.Text = "0";
            // 
            // button_ROP_constant_set
            // 
            this.button_ROP_constant_set.Location = new System.Drawing.Point(123, 332);
            this.button_ROP_constant_set.Name = "button_ROP_constant_set";
            this.button_ROP_constant_set.Size = new System.Drawing.Size(75, 23);
            this.button_ROP_constant_set.TabIndex = 9;
            this.button_ROP_constant_set.Text = "Set";
            this.button_ROP_constant_set.UseVisualStyleBackColor = true;
            this.button_ROP_constant_set.Click += new System.EventHandler(this.button__ROP_constant_set_Click);
            // 
            // label_ROP_constant_Name
            // 
            this.label_ROP_constant_Name.AutoSize = true;
            this.label_ROP_constant_Name.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_ROP_constant_Name.Location = new System.Drawing.Point(151, 55);
            this.label_ROP_constant_Name.Name = "label_ROP_constant_Name";
            this.label_ROP_constant_Name.Size = new System.Drawing.Size(51, 16);
            this.label_ROP_constant_Name.TabIndex = 8;
            this.label_ROP_constant_Name.Text = "label1";
            // 
            // label_ROP_constant_equal
            // 
            this.label_ROP_constant_equal.AutoSize = true;
            this.label_ROP_constant_equal.Location = new System.Drawing.Point(228, 57);
            this.label_ROP_constant_equal.Name = "label_ROP_constant_equal";
            this.label_ROP_constant_equal.Size = new System.Drawing.Size(13, 13);
            this.label_ROP_constant_equal.TabIndex = 11;
            this.label_ROP_constant_equal.Text = "=";
            // 
            // radioButton_ROP_CertainConstant
            // 
            this.radioButton_ROP_CertainConstant.AutoSize = true;
            this.radioButton_ROP_CertainConstant.Checked = true;
            this.radioButton_ROP_CertainConstant.Location = new System.Drawing.Point(13, 22);
            this.radioButton_ROP_CertainConstant.Name = "radioButton_ROP_CertainConstant";
            this.radioButton_ROP_CertainConstant.Size = new System.Drawing.Size(190, 17);
            this.radioButton_ROP_CertainConstant.TabIndex = 12;
            this.radioButton_ROP_CertainConstant.TabStop = true;
            this.radioButton_ROP_CertainConstant.Text = "Constant unaffected by uncertainty";
            this.radioButton_ROP_CertainConstant.UseVisualStyleBackColor = true;
            this.radioButton_ROP_CertainConstant.CheckedChanged += new System.EventHandler(this.radioButton_ROP_CertainConstant_CheckedChanged);
            // 
            // radioButton_ROP_UncertainConstant
            // 
            this.radioButton_ROP_UncertainConstant.AutoSize = true;
            this.radioButton_ROP_UncertainConstant.Location = new System.Drawing.Point(13, 93);
            this.radioButton_ROP_UncertainConstant.Name = "radioButton_ROP_UncertainConstant";
            this.radioButton_ROP_UncertainConstant.Size = new System.Drawing.Size(178, 17);
            this.radioButton_ROP_UncertainConstant.TabIndex = 13;
            this.radioButton_ROP_UncertainConstant.Text = "Constant affected by uncertainty";
            this.radioButton_ROP_UncertainConstant.UseVisualStyleBackColor = true;
            // 
            // button_ROP_constant_cancel
            // 
            this.button_ROP_constant_cancel.Location = new System.Drawing.Point(223, 332);
            this.button_ROP_constant_cancel.Name = "button_ROP_constant_cancel";
            this.button_ROP_constant_cancel.Size = new System.Drawing.Size(75, 23);
            this.button_ROP_constant_cancel.TabIndex = 14;
            this.button_ROP_constant_cancel.Text = "Cancel";
            this.button_ROP_constant_cancel.UseVisualStyleBackColor = true;
            this.button_ROP_constant_cancel.Click += new System.EventHandler(this.button_ROP_constant_cancel_Click);
            // 
            // textBox_ROP_ConstSetup_PDFparameter2
            // 
            this.textBox_ROP_ConstSetup_PDFparameter2.Enabled = false;
            this.textBox_ROP_ConstSetup_PDFparameter2.Location = new System.Drawing.Point(75, 200);
            this.textBox_ROP_ConstSetup_PDFparameter2.Name = "textBox_ROP_ConstSetup_PDFparameter2";
            this.textBox_ROP_ConstSetup_PDFparameter2.Size = new System.Drawing.Size(53, 20);
            this.textBox_ROP_ConstSetup_PDFparameter2.TabIndex = 26;
            // 
            // textBox_ROP_ConstSetup_PDFparameter1
            // 
            this.textBox_ROP_ConstSetup_PDFparameter1.Enabled = false;
            this.textBox_ROP_ConstSetup_PDFparameter1.Location = new System.Drawing.Point(75, 176);
            this.textBox_ROP_ConstSetup_PDFparameter1.Name = "textBox_ROP_ConstSetup_PDFparameter1";
            this.textBox_ROP_ConstSetup_PDFparameter1.Size = new System.Drawing.Size(53, 20);
            this.textBox_ROP_ConstSetup_PDFparameter1.TabIndex = 25;
            // 
            // label_ROP_VarSetup_PDFparameter2
            // 
            this.label_ROP_VarSetup_PDFparameter2.AutoSize = true;
            this.label_ROP_VarSetup_PDFparameter2.Enabled = false;
            this.label_ROP_VarSetup_PDFparameter2.Location = new System.Drawing.Point(38, 203);
            this.label_ROP_VarSetup_PDFparameter2.Name = "label_ROP_VarSetup_PDFparameter2";
            this.label_ROP_VarSetup_PDFparameter2.Size = new System.Drawing.Size(34, 13);
            this.label_ROP_VarSetup_PDFparameter2.TabIndex = 30;
            this.label_ROP_VarSetup_PDFparameter2.Text = "sigma";
            // 
            // label_ROP_VarSetup_PDFparameter1
            // 
            this.label_ROP_VarSetup_PDFparameter1.AutoSize = true;
            this.label_ROP_VarSetup_PDFparameter1.Enabled = false;
            this.label_ROP_VarSetup_PDFparameter1.Location = new System.Drawing.Point(38, 179);
            this.label_ROP_VarSetup_PDFparameter1.Name = "label_ROP_VarSetup_PDFparameter1";
            this.label_ROP_VarSetup_PDFparameter1.Size = new System.Drawing.Size(21, 13);
            this.label_ROP_VarSetup_PDFparameter1.TabIndex = 29;
            this.label_ROP_VarSetup_PDFparameter1.Text = "mu";
            // 
            // comboBox_ROP_VarSetup_PDF
            // 
            this.comboBox_ROP_VarSetup_PDF.Enabled = false;
            this.comboBox_ROP_VarSetup_PDF.FormattingEnabled = true;
            this.comboBox_ROP_VarSetup_PDF.Items.AddRange(new object[] {
            "Normal",
            "Triangular",
            "Uniform",
            "Rayleigh",
            "Mixture Gaussian",
            "User defined"});
            this.comboBox_ROP_VarSetup_PDF.Location = new System.Drawing.Point(43, 148);
            this.comboBox_ROP_VarSetup_PDF.Name = "comboBox_ROP_VarSetup_PDF";
            this.comboBox_ROP_VarSetup_PDF.Size = new System.Drawing.Size(121, 21);
            this.comboBox_ROP_VarSetup_PDF.TabIndex = 24;
            this.comboBox_ROP_VarSetup_PDF.Text = "Normal";
            this.comboBox_ROP_VarSetup_PDF.SelectedIndexChanged += new System.EventHandler(this.comboBox_ROP_VarSetup_PDF_SelectedIndexChanged);
            // 
            // pictureBox_ROP_VarSetup_PDFimage
            // 
            this.pictureBox_ROP_VarSetup_PDFimage.BackColor = System.Drawing.Color.White;
            this.pictureBox_ROP_VarSetup_PDFimage.Enabled = false;
            this.pictureBox_ROP_VarSetup_PDFimage.Image = global::AircadiaExplorer.Properties.Resources.PDF_Normal;
            this.pictureBox_ROP_VarSetup_PDFimage.ImageLocation = "";
            this.pictureBox_ROP_VarSetup_PDFimage.Location = new System.Drawing.Point(185, 125);
            this.pictureBox_ROP_VarSetup_PDFimage.Name = "pictureBox_ROP_VarSetup_PDFimage";
            this.pictureBox_ROP_VarSetup_PDFimage.Size = new System.Drawing.Size(227, 145);
            this.pictureBox_ROP_VarSetup_PDFimage.TabIndex = 27;
            this.pictureBox_ROP_VarSetup_PDFimage.TabStop = false;
            // 
            // label_ROP_VarSetup_PDF
            // 
            this.label_ROP_VarSetup_PDF.AutoSize = true;
            this.label_ROP_VarSetup_PDF.Enabled = false;
            this.label_ROP_VarSetup_PDF.Location = new System.Drawing.Point(43, 133);
            this.label_ROP_VarSetup_PDF.Name = "label_ROP_VarSetup_PDF";
            this.label_ROP_VarSetup_PDF.Size = new System.Drawing.Size(72, 13);
            this.label_ROP_VarSetup_PDF.TabIndex = 31;
            this.label_ROP_VarSetup_PDF.Text = "Variable PDF:";
            // 
            // textBox_ROP_ConstSetup_PDFparameter3
            // 
            this.textBox_ROP_ConstSetup_PDFparameter3.Location = new System.Drawing.Point(75, 225);
            this.textBox_ROP_ConstSetup_PDFparameter3.Name = "textBox_ROP_ConstSetup_PDFparameter3";
            this.textBox_ROP_ConstSetup_PDFparameter3.Size = new System.Drawing.Size(53, 20);
            this.textBox_ROP_ConstSetup_PDFparameter3.TabIndex = 32;
            this.textBox_ROP_ConstSetup_PDFparameter3.Visible = false;
            // 
            // label_ROP_VarSetup_PDFparameter3
            // 
            this.label_ROP_VarSetup_PDFparameter3.AutoSize = true;
            this.label_ROP_VarSetup_PDFparameter3.Location = new System.Drawing.Point(38, 228);
            this.label_ROP_VarSetup_PDFparameter3.Name = "label_ROP_VarSetup_PDFparameter3";
            this.label_ROP_VarSetup_PDFparameter3.Size = new System.Drawing.Size(34, 13);
            this.label_ROP_VarSetup_PDFparameter3.TabIndex = 33;
            this.label_ROP_VarSetup_PDFparameter3.Text = "sigma";
            this.label_ROP_VarSetup_PDFparameter3.Visible = false;
            // 
            // textBox_ROP_ConstSetup_PDFparameter4
            // 
            this.textBox_ROP_ConstSetup_PDFparameter4.Location = new System.Drawing.Point(75, 250);
            this.textBox_ROP_ConstSetup_PDFparameter4.Name = "textBox_ROP_ConstSetup_PDFparameter4";
            this.textBox_ROP_ConstSetup_PDFparameter4.Size = new System.Drawing.Size(53, 20);
            this.textBox_ROP_ConstSetup_PDFparameter4.TabIndex = 34;
            this.textBox_ROP_ConstSetup_PDFparameter4.Visible = false;
            // 
            // label_ROP_VarSetup_PDFparameter4
            // 
            this.label_ROP_VarSetup_PDFparameter4.AutoSize = true;
            this.label_ROP_VarSetup_PDFparameter4.Location = new System.Drawing.Point(38, 253);
            this.label_ROP_VarSetup_PDFparameter4.Name = "label_ROP_VarSetup_PDFparameter4";
            this.label_ROP_VarSetup_PDFparameter4.Size = new System.Drawing.Size(34, 13);
            this.label_ROP_VarSetup_PDFparameter4.TabIndex = 35;
            this.label_ROP_VarSetup_PDFparameter4.Text = "sigma";
            this.label_ROP_VarSetup_PDFparameter4.Visible = false;
            // 
            // label_ROP_VarSetup_PDFparameter6
            // 
            this.label_ROP_VarSetup_PDFparameter6.AutoSize = true;
            this.label_ROP_VarSetup_PDFparameter6.Location = new System.Drawing.Point(38, 307);
            this.label_ROP_VarSetup_PDFparameter6.Name = "label_ROP_VarSetup_PDFparameter6";
            this.label_ROP_VarSetup_PDFparameter6.Size = new System.Drawing.Size(19, 13);
            this.label_ROP_VarSetup_PDFparameter6.TabIndex = 43;
            this.label_ROP_VarSetup_PDFparameter6.Text = "p2";
            this.label_ROP_VarSetup_PDFparameter6.Visible = false;
            // 
            // textBox_ROP_ConstSetup_PDFparameter6
            // 
            this.textBox_ROP_ConstSetup_PDFparameter6.Location = new System.Drawing.Point(75, 304);
            this.textBox_ROP_ConstSetup_PDFparameter6.Name = "textBox_ROP_ConstSetup_PDFparameter6";
            this.textBox_ROP_ConstSetup_PDFparameter6.Size = new System.Drawing.Size(53, 20);
            this.textBox_ROP_ConstSetup_PDFparameter6.TabIndex = 42;
            this.textBox_ROP_ConstSetup_PDFparameter6.Visible = false;
            // 
            // textBox_ROP_ConstSetup_PDFparameter5
            // 
            this.textBox_ROP_ConstSetup_PDFparameter5.Location = new System.Drawing.Point(75, 278);
            this.textBox_ROP_ConstSetup_PDFparameter5.Name = "textBox_ROP_ConstSetup_PDFparameter5";
            this.textBox_ROP_ConstSetup_PDFparameter5.Size = new System.Drawing.Size(53, 20);
            this.textBox_ROP_ConstSetup_PDFparameter5.TabIndex = 41;
            this.textBox_ROP_ConstSetup_PDFparameter5.Visible = false;
            // 
            // label_ROP_VarSetup_PDFparameter5
            // 
            this.label_ROP_VarSetup_PDFparameter5.AutoSize = true;
            this.label_ROP_VarSetup_PDFparameter5.Location = new System.Drawing.Point(38, 281);
            this.label_ROP_VarSetup_PDFparameter5.Name = "label_ROP_VarSetup_PDFparameter5";
            this.label_ROP_VarSetup_PDFparameter5.Size = new System.Drawing.Size(19, 13);
            this.label_ROP_VarSetup_PDFparameter5.TabIndex = 40;
            this.label_ROP_VarSetup_PDFparameter5.Text = "p1";
            this.label_ROP_VarSetup_PDFparameter5.Visible = false;
            // 
            // ROP_ConstantSetup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(421, 370);
            this.Controls.Add(this.label_ROP_VarSetup_PDFparameter6);
            this.Controls.Add(this.textBox_ROP_ConstSetup_PDFparameter6);
            this.Controls.Add(this.textBox_ROP_ConstSetup_PDFparameter5);
            this.Controls.Add(this.label_ROP_VarSetup_PDFparameter5);
            this.Controls.Add(this.textBox_ROP_ConstSetup_PDFparameter4);
            this.Controls.Add(this.label_ROP_VarSetup_PDFparameter4);
            this.Controls.Add(this.textBox_ROP_ConstSetup_PDFparameter3);
            this.Controls.Add(this.label_ROP_VarSetup_PDFparameter3);
            this.Controls.Add(this.textBox_ROP_ConstSetup_PDFparameter2);
            this.Controls.Add(this.textBox_ROP_ConstSetup_PDFparameter1);
            this.Controls.Add(this.label_ROP_VarSetup_PDFparameter2);
            this.Controls.Add(this.label_ROP_VarSetup_PDFparameter1);
            this.Controls.Add(this.comboBox_ROP_VarSetup_PDF);
            this.Controls.Add(this.pictureBox_ROP_VarSetup_PDFimage);
            this.Controls.Add(this.label_ROP_VarSetup_PDF);
            this.Controls.Add(this.button_ROP_constant_cancel);
            this.Controls.Add(this.radioButton_ROP_UncertainConstant);
            this.Controls.Add(this.radioButton_ROP_CertainConstant);
            this.Controls.Add(this.textBox_ROP_constant_Value);
            this.Controls.Add(this.button_ROP_constant_set);
            this.Controls.Add(this.label_ROP_constant_Name);
            this.Controls.Add(this.label_ROP_constant_equal);
            this.Name = "ROP_ConstantSetup";
            this.Text = "ROP_ConstantSetup";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_ROP_VarSetup_PDFimage)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_ROP_constant_Value;
        private System.Windows.Forms.Button button_ROP_constant_set;
        private System.Windows.Forms.Label label_ROP_constant_Name;
        private System.Windows.Forms.Label label_ROP_constant_equal;
        private System.Windows.Forms.RadioButton radioButton_ROP_CertainConstant;
        private System.Windows.Forms.RadioButton radioButton_ROP_UncertainConstant;
        private System.Windows.Forms.Button button_ROP_constant_cancel;
        private System.Windows.Forms.TextBox textBox_ROP_ConstSetup_PDFparameter2;
        private System.Windows.Forms.TextBox textBox_ROP_ConstSetup_PDFparameter1;
        private System.Windows.Forms.Label label_ROP_VarSetup_PDFparameter2;
        private System.Windows.Forms.Label label_ROP_VarSetup_PDFparameter1;
        private System.Windows.Forms.ComboBox comboBox_ROP_VarSetup_PDF;
        private System.Windows.Forms.PictureBox pictureBox_ROP_VarSetup_PDFimage;
        private System.Windows.Forms.Label label_ROP_VarSetup_PDF;
        private System.Windows.Forms.TextBox textBox_ROP_ConstSetup_PDFparameter3;
        private System.Windows.Forms.Label label_ROP_VarSetup_PDFparameter3;
        private System.Windows.Forms.TextBox textBox_ROP_ConstSetup_PDFparameter4;
        private System.Windows.Forms.Label label_ROP_VarSetup_PDFparameter4;
        private System.Windows.Forms.Label label_ROP_VarSetup_PDFparameter6;
        private System.Windows.Forms.TextBox textBox_ROP_ConstSetup_PDFparameter6;
        private System.Windows.Forms.TextBox textBox_ROP_ConstSetup_PDFparameter5;
        private System.Windows.Forms.Label label_ROP_VarSetup_PDFparameter5;
    }
}