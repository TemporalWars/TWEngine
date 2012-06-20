namespace TWEngine.TerrainTools
{
    partial class SaveMapsTool
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
            this.label1 = new System.Windows.Forms.Label();
            this.chkSaveSelectableItems = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.btnSaveMap = new System.Windows.Forms.Button();
            this.txtBoxMapSaveName = new System.Windows.Forms.TextBox();
            this.lvLoadMaps = new System.Windows.Forms.ListView();
            this.cmbMapType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.chkSaveSelectableItems);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.btnSaveMap);
            this.groupBox1.Controls.Add(this.txtBoxMapSaveName);
            this.groupBox1.Controls.Add(this.lvLoadMaps);
            this.groupBox1.Location = new System.Drawing.Point(12, 174);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(494, 208);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Save Maps";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(257, 44);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(216, 39);
            this.label1.TabIndex = 6;
            this.label1.Text = "Check the box above when you plan to use the \'SelectableItems\' in a single player" +
                               " level, via scripting conditions.";
            // 
            // chkSaveSelectableItems
            // 
            this.chkSaveSelectableItems.AutoSize = true;
            this.chkSaveSelectableItems.Location = new System.Drawing.Point(260, 24);
            this.chkSaveSelectableItems.Name = "chkSaveSelectableItems";
            this.chkSaveSelectableItems.Size = new System.Drawing.Size(233, 17);
            this.chkSaveSelectableItems.TabIndex = 5;
            this.chkSaveSelectableItems.Text = "Save All SelectableItems currently on map? ";
            this.chkSaveSelectableItems.UseVisualStyleBackColor = true;
            this.chkSaveSelectableItems.CheckedChanged += new System.EventHandler(this.chkSaveSelectableItems_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 163);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(97, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Enter a Map name:";
            // 
            // btnSaveMap
            // 
            this.btnSaveMap.Location = new System.Drawing.Point(8, 179);
            this.btnSaveMap.Name = "btnSaveMap";
            this.btnSaveMap.Size = new System.Drawing.Size(75, 23);
            this.btnSaveMap.TabIndex = 3;
            this.btnSaveMap.Text = "Save Map";
            this.btnSaveMap.UseVisualStyleBackColor = true;
            this.btnSaveMap.Click += new System.EventHandler(this.btnSaveMap_Click);
            // 
            // txtBoxMapSaveName
            // 
            this.txtBoxMapSaveName.Location = new System.Drawing.Point(89, 182);
            this.txtBoxMapSaveName.Name = "txtBoxMapSaveName";
            this.txtBoxMapSaveName.Size = new System.Drawing.Size(147, 20);
            this.txtBoxMapSaveName.TabIndex = 2;
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
            // 
            // cmbMapType
            // 
            this.cmbMapType.FormattingEnabled = true;
            this.cmbMapType.Items.AddRange(new object[] {
                                                            "Single Player (SP)",
                                                            "MultiPlayer (MP)"});
            this.cmbMapType.Location = new System.Drawing.Point(260, 67);
            this.cmbMapType.Name = "cmbMapType";
            this.cmbMapType.Size = new System.Drawing.Size(130, 21);
            this.cmbMapType.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(257, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(103, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Maptype (Sp or Mp):";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(15, 40);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(216, 67);
            this.label3.TabIndex = 9;
            this.label3.Text = "Choose the proper Maptype to use; either MP or SP.  This will save the current ma" +
                               "p in either the MP folder or SP folder, where MP is for multiplayer games, and S" +
                               "P is for singleplayer games.";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.cmbMapType);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(12, 12);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(493, 156);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "MapType";
            // 
            // SaveMapsTool
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(518, 396);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "SaveMapsTool";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Save Maps Tool";
            this.TopMost = true;
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnSaveMap;
        private System.Windows.Forms.TextBox txtBoxMapSaveName;
        private System.Windows.Forms.ListView lvLoadMaps;
        private System.Windows.Forms.CheckBox chkSaveSelectableItems;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cmbMapType;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}