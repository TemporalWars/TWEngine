namespace TWEngine.TerrainTools
{
    partial class LoadMapsTool
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtBoxMapLoadName = new System.Windows.Forms.TextBox();
            this.btnLoadMap = new System.Windows.Forms.Button();
            this.lvLoadMaps = new System.Windows.Forms.ListView();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtBoxMapLoadName);
            this.groupBox1.Controls.Add(this.btnLoadMap);
            this.groupBox1.Controls.Add(this.lvLoadMaps);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(494, 163);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Load  Maps";
            // 
            // txtBoxMapLoadName
            // 
            this.txtBoxMapLoadName.ForeColor = System.Drawing.Color.Blue;
            this.txtBoxMapLoadName.Location = new System.Drawing.Point(260, 101);
            this.txtBoxMapLoadName.Name = "txtBoxMapLoadName";
            this.txtBoxMapLoadName.ReadOnly = true;
            this.txtBoxMapLoadName.Size = new System.Drawing.Size(219, 20);
            this.txtBoxMapLoadName.TabIndex = 5;
            // 
            // btnLoadMap
            // 
            this.btnLoadMap.Location = new System.Drawing.Point(260, 127);
            this.btnLoadMap.Name = "btnLoadMap";
            this.btnLoadMap.Size = new System.Drawing.Size(75, 23);
            this.btnLoadMap.TabIndex = 1;
            this.btnLoadMap.Text = "Load Map";
            this.btnLoadMap.UseVisualStyleBackColor = true;
            
            // 
            // lvLoadMaps
            // 
            this.lvLoadMaps.AllowColumnReorder = true;
            this.lvLoadMaps.Location = new System.Drawing.Point(18, 24);
            this.lvLoadMaps.MultiSelect = false;
            this.lvLoadMaps.Name = "lvLoadMaps";
            this.lvLoadMaps.ShowGroups = false;
            this.lvLoadMaps.Size = new System.Drawing.Size(218, 126);
            this.lvLoadMaps.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lvLoadMaps.TabIndex = 0;
            this.lvLoadMaps.UseCompatibleStateImageBehavior = false;
            this.lvLoadMaps.SelectedIndexChanged += new System.EventHandler(this.lvLoadMaps_SelectedIndexChanged);
            // 
            // LoadMapsTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(518, 194);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "LoadMapsTool";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Load Maps Tool";
            this.TopMost = true;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox txtBoxMapLoadName;
        private System.Windows.Forms.Button btnLoadMap;
        private System.Windows.Forms.ListView lvLoadMaps;
    }
}