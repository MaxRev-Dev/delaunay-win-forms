namespace DelaunayMethod
{
    partial class Main
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Main));
            this.generatePointsBtn = new System.Windows.Forms.Button();
            this.fillDelaunayBtn = new System.Windows.Forms.Button();
            this.layer = new System.Windows.Forms.Panel();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.importBtn = new System.Windows.Forms.Button();
            this.voronoiFillBtn = new System.Windows.Forms.Button();
            this.clearBtn = new System.Windows.Forms.Button();
            this.exportBtn = new System.Windows.Forms.Button();
            this.colorSelectorCB = new System.Windows.Forms.ComboBox();
            this.dPoints = new System.Windows.Forms.TextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.colorSiteCB = new System.Windows.Forms.ComboBox();
            this.speedBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.immediateChk = new System.Windows.Forms.CheckBox();
            this.panel3 = new System.Windows.Forms.Panel();
            this.resolutionCB = new System.Windows.Forms.ComboBox();
            this.aspectRatioCB = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.status = new System.Windows.Forms.Label();
            this.mposition = new System.Windows.Forms.Label();
            this.panel5 = new System.Windows.Forms.Panel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.flowLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel5.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // generatePointsBtn
            // 
            this.generatePointsBtn.Location = new System.Drawing.Point(84, 3);
            this.generatePointsBtn.Name = "generatePointsBtn";
            this.generatePointsBtn.Size = new System.Drawing.Size(98, 23);
            this.generatePointsBtn.TabIndex = 0;
            this.generatePointsBtn.Text = "Generate Points";
            this.generatePointsBtn.UseVisualStyleBackColor = true;
            this.generatePointsBtn.Click += new System.EventHandler(this.GeneratePointsBtn_Click);
            // 
            // fillDelaunayBtn
            // 
            this.fillDelaunayBtn.Location = new System.Drawing.Point(188, 3);
            this.fillDelaunayBtn.Name = "fillDelaunayBtn";
            this.fillDelaunayBtn.Size = new System.Drawing.Size(75, 23);
            this.fillDelaunayBtn.TabIndex = 0;
            this.fillDelaunayBtn.Text = "Fill Delaunay";
            this.fillDelaunayBtn.UseVisualStyleBackColor = true;
            this.fillDelaunayBtn.Click += new System.EventHandler(this.FillDelaunayBtn_Click);
            // 
            // layer
            // 
            this.layer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.layer.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.layer.Location = new System.Drawing.Point(12, 42);
            this.layer.Name = "layer";
            this.layer.Size = new System.Drawing.Size(660, 381);
            this.layer.TabIndex = 1;
            this.layer.SizeChanged += new System.EventHandler(this.Layer_SizeChanged);
            this.layer.Click += new System.EventHandler(this.Layer_Click);
            this.layer.Paint += new System.Windows.Forms.PaintEventHandler(this.Layer_Paint);
            this.layer.MouseDown += new System.Windows.Forms.MouseEventHandler(this.Layer_MouseDown);
            this.layer.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Layer_MouseMove);
            this.layer.MouseUp += new System.Windows.Forms.MouseEventHandler(this.Layer_MouseUp);
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.Controls.Add(this.importBtn);
            this.flowLayoutPanel1.Controls.Add(this.generatePointsBtn);
            this.flowLayoutPanel1.Controls.Add(this.fillDelaunayBtn);
            this.flowLayoutPanel1.Controls.Add(this.voronoiFillBtn);
            this.flowLayoutPanel1.Controls.Add(this.clearBtn);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(0, 5);
            this.flowLayoutPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(428, 29);
            this.flowLayoutPanel1.TabIndex = 2;
            // 
            // importBtn
            // 
            this.importBtn.Location = new System.Drawing.Point(3, 3);
            this.importBtn.Name = "importBtn";
            this.importBtn.Size = new System.Drawing.Size(75, 23);
            this.importBtn.TabIndex = 0;
            this.importBtn.Text = "Import CSV";
            this.importBtn.UseMnemonic = false;
            this.importBtn.UseVisualStyleBackColor = true;
            this.importBtn.Click += new System.EventHandler(this.ImportBtn_Click);
            // 
            // voronoiFillBtn
            // 
            this.voronoiFillBtn.Location = new System.Drawing.Point(269, 3);
            this.voronoiFillBtn.Name = "voronoiFillBtn";
            this.voronoiFillBtn.Size = new System.Drawing.Size(75, 23);
            this.voronoiFillBtn.TabIndex = 0;
            this.voronoiFillBtn.Text = "Fill Voronoi";
            this.voronoiFillBtn.UseVisualStyleBackColor = true;
            this.voronoiFillBtn.Click += new System.EventHandler(this.VoronoiFillBtn_Click);
            // 
            // clearBtn
            // 
            this.clearBtn.Location = new System.Drawing.Point(350, 3);
            this.clearBtn.Name = "clearBtn";
            this.clearBtn.Size = new System.Drawing.Size(75, 23);
            this.clearBtn.TabIndex = 0;
            this.clearBtn.Text = "Clear All";
            this.clearBtn.UseMnemonic = false;
            this.clearBtn.UseVisualStyleBackColor = true;
            this.clearBtn.Click += new System.EventHandler(this.ClearBtn_Click);
            // 
            // exportBtn
            // 
            this.exportBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.exportBtn.Location = new System.Drawing.Point(232, 7);
            this.exportBtn.Name = "exportBtn";
            this.exportBtn.Size = new System.Drawing.Size(70, 44);
            this.exportBtn.TabIndex = 0;
            this.exportBtn.Text = "Export";
            this.exportBtn.UseVisualStyleBackColor = true;
            this.exportBtn.Click += new System.EventHandler(this.ExportBtn_Click);
            // 
            // colorSelectorCB
            // 
            this.colorSelectorCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.colorSelectorCB.FormattingEnabled = true;
            this.colorSelectorCB.Location = new System.Drawing.Point(110, 5);
            this.colorSelectorCB.Name = "colorSelectorCB";
            this.colorSelectorCB.Size = new System.Drawing.Size(88, 21);
            this.colorSelectorCB.TabIndex = 3;
            this.colorSelectorCB.SelectedIndexChanged += new System.EventHandler(this.MeshColor_SelectedIndexChanged);
            this.colorSelectorCB.DropDownClosed += new System.EventHandler(this.ColorSiteCB_DropDownClosed);
            // 
            // dPoints
            // 
            this.dPoints.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.dPoints.Location = new System.Drawing.Point(98, 34);
            this.dPoints.Name = "dPoints";
            this.dPoints.Size = new System.Drawing.Size(100, 20);
            this.dPoints.TabIndex = 4;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.colorSiteCB);
            this.panel1.Controls.Add(this.speedBox);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.colorSelectorCB);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.dPoints);
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(207, 92);
            this.panel1.TabIndex = 5;
            // 
            // colorSiteCB
            // 
            this.colorSiteCB.FormattingEnabled = true;
            this.colorSiteCB.Location = new System.Drawing.Point(3, 5);
            this.colorSiteCB.Name = "colorSiteCB";
            this.colorSiteCB.Size = new System.Drawing.Size(101, 21);
            this.colorSiteCB.TabIndex = 7;
            this.colorSiteCB.SelectedIndexChanged += new System.EventHandler(this.ColorSiteCB_SelectedIndexChanged);
            this.colorSiteCB.DropDownClosed += new System.EventHandler(this.ColorSiteCB_DropDownClosed);
            // 
            // speedBox
            // 
            this.speedBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.speedBox.Location = new System.Drawing.Point(77, 62);
            this.speedBox.Name = "speedBox";
            this.speedBox.Size = new System.Drawing.Size(121, 21);
            this.speedBox.TabIndex = 6;
            this.speedBox.TabStop = false;
            this.speedBox.SelectedIndexChanged += new System.EventHandler(this.SpeedBox_SelectedIndexChanged);
            this.speedBox.DropDownClosed += new System.EventHandler(this.ColorSiteCB_DropDownClosed);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(38, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Speed";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 13);
            this.label1.TabIndex = 5;
            this.label1.Text = "Min distance";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.immediateChk);
            this.panel2.Controls.Add(this.panel3);
            this.panel2.Location = new System.Drawing.Point(3, 37);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(436, 58);
            this.panel2.TabIndex = 6;
            // 
            // immediateChk
            // 
            this.immediateChk.AutoSize = true;
            this.immediateChk.Location = new System.Drawing.Point(332, 23);
            this.immediateChk.Name = "immediateChk";
            this.immediateChk.Size = new System.Drawing.Size(100, 17);
            this.immediateChk.TabIndex = 7;
            this.immediateChk.Text = "Immediate paint";
            this.immediateChk.UseVisualStyleBackColor = true;
            this.immediateChk.CheckedChanged += new System.EventHandler(this.CheckBox1_CheckedChanged);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.resolutionCB);
            this.panel3.Controls.Add(this.aspectRatioCB);
            this.panel3.Controls.Add(this.label5);
            this.panel3.Controls.Add(this.label4);
            this.panel3.Controls.Add(this.exportBtn);
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(313, 58);
            this.panel3.TabIndex = 6;
            // 
            // resolutionCB
            // 
            this.resolutionCB.FormattingEnabled = true;
            this.resolutionCB.Location = new System.Drawing.Point(94, 30);
            this.resolutionCB.Name = "resolutionCB";
            this.resolutionCB.Size = new System.Drawing.Size(121, 21);
            this.resolutionCB.TabIndex = 2;
            this.resolutionCB.DropDownClosed += new System.EventHandler(this.ColorSiteCB_DropDownClosed);
            // 
            // aspectRatioCB
            // 
            this.aspectRatioCB.CausesValidation = false;
            this.aspectRatioCB.Location = new System.Drawing.Point(94, 7);
            this.aspectRatioCB.Name = "aspectRatioCB";
            this.aspectRatioCB.Size = new System.Drawing.Size(121, 21);
            this.aspectRatioCB.TabIndex = 2;
            this.aspectRatioCB.SelectedIndexChanged += new System.EventHandler(this.AspectRatioCB_SelectedIndexChanged);
            this.aspectRatioCB.DropDownClosed += new System.EventHandler(this.ColorSiteCB_DropDownClosed);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(31, 33);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(57, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Resolution";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(20, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(68, 13);
            this.label4.TabIndex = 1;
            this.label4.Text = "Aspect Ratio";
            // 
            // status
            // 
            this.status.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.status.Location = new System.Drawing.Point(347, 0);
            this.status.Name = "status";
            this.status.Size = new System.Drawing.Size(310, 27);
            this.status.TabIndex = 7;
            this.status.TextAlign = System.Drawing.ContentAlignment.BottomRight;
            // 
            // mposition
            // 
            this.mposition.Location = new System.Drawing.Point(3, 0);
            this.mposition.Name = "mposition";
            this.mposition.Size = new System.Drawing.Size(156, 24);
            this.mposition.TabIndex = 7;
            this.mposition.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // panel5
            // 
            this.panel5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel5.Controls.Add(this.mposition);
            this.panel5.Controls.Add(this.status);
            this.panel5.Location = new System.Drawing.Point(12, 12);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(660, 27);
            this.panel5.TabIndex = 10;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.flowLayoutPanel2.Controls.Add(this.panel1);
            this.flowLayoutPanel2.Controls.Add(this.flowLayoutPanel3);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(13, 429);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(657, 100);
            this.flowLayoutPanel2.TabIndex = 11;
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.Controls.Add(this.flowLayoutPanel1);
            this.flowLayoutPanel3.Controls.Add(this.panel2);
            this.flowLayoutPanel3.Location = new System.Drawing.Point(213, 0);
            this.flowLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Padding = new System.Windows.Forms.Padding(0, 5, 0, 0);
            this.flowLayoutPanel3.Size = new System.Drawing.Size(440, 95);
            this.flowLayoutPanel3.TabIndex = 6;
            // 
            // Main
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 541);
            this.Controls.Add(this.flowLayoutPanel2);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.layer);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(700, 580);
            this.Name = "Main";
            this.Text = "Delanay Method";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Main_FormClosing);
            this.Click += new System.EventHandler(this.Main_Click);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Form1_KeyUp);
            this.flowLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel5.ResumeLayout(false);
            this.flowLayoutPanel2.ResumeLayout(false);
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button generatePointsBtn;
        private System.Windows.Forms.Button fillDelaunayBtn;
        private System.Windows.Forms.Panel layer;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.ComboBox colorSelectorCB;
        private System.Windows.Forms.TextBox dPoints;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button exportBtn;
        private System.Windows.Forms.ComboBox speedBox;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label status;
        private System.Windows.Forms.Button importBtn;
        private System.Windows.Forms.Button clearBtn;
        private System.Windows.Forms.Label mposition;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.ComboBox resolutionCB;
        private System.Windows.Forms.ComboBox aspectRatioCB;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button voronoiFillBtn;
        private System.Windows.Forms.CheckBox immediateChk;
        private System.Windows.Forms.Panel panel5;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.ComboBox colorSiteCB;
    }
}

