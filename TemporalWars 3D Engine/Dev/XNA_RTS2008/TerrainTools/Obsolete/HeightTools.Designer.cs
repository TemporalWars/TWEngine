namespace TWEngine.TerrainTools
{
    partial class HeightTools
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(HeightTools));
            this.checkBox1 = new System.Windows.Forms.CheckBox();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.checkBox2 = new System.Windows.Forms.CheckBox();
            this.checkBox3 = new System.Windows.Forms.CheckBox();
            this.checkBox4 = new System.Windows.Forms.CheckBox();
            this.checkBox5 = new System.Windows.Forms.CheckBox();
            this.hScrollBar1 = new System.Windows.Forms.HScrollBar();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.hScrollBar2 = new System.Windows.Forms.HScrollBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.trackBar1 = new System.Windows.Forms.TrackBar();
            this.cbConstantFeet = new System.Windows.Forms.CheckBox();
            this.txtConstantFeet = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtFloodErrorMessages = new System.Windows.Forms.TextBox();
            this.btnGeneratePerlinNoise = new System.Windows.Forms.Button();
            this.pictureBox = new System.Windows.Forms.PictureBox();
            this.btnApplyHeightMap = new System.Windows.Forms.Button();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.cbPass2Enable = new System.Windows.Forms.CheckBox();
            this.nudRandomSeedValue_p2 = new System.Windows.Forms.NumericUpDown();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.nudOctaves_p2 = new System.Windows.Forms.NumericUpDown();
            this.nudNoiseSize_p2 = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.nudPersistence_p2 = new System.Windows.Forms.NumericUpDown();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.cbPass1Enable = new System.Windows.Forms.CheckBox();
            this.nudRandomSeedValue_p1 = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.nudOctaves_p1 = new System.Windows.Forms.NumericUpDown();
            this.nudNoiseSize_p1 = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.nudPersistence_p1 = new System.Windows.Forms.NumericUpDown();
            this.btnCreateMap1024x1024 = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).BeginInit();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRandomSeedValue_p2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOctaves_p2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNoiseSize_p2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPersistence_p2)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRandomSeedValue_p1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOctaves_p1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNoiseSize_p1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPersistence_p1)).BeginInit();
            this.SuspendLayout();
            // 
            // checkBox1
            // 
            this.checkBox1.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.checkBox1.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBox1.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox1.Checked = true;
            this.checkBox1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox1.Cursor = System.Windows.Forms.Cursors.Default;
            this.checkBox1.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.checkBox1.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(213)))), ((int)(((byte)(155)))));
            this.checkBox1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox1.ImageKey = "paintTool_arrow.bmp";
            this.checkBox1.ImageList = this.imageList1;
            this.checkBox1.Location = new System.Drawing.Point(2, 0);
            this.checkBox1.Name = "checkBox1";
            this.checkBox1.Size = new System.Drawing.Size(62, 62);
            this.checkBox1.TabIndex = 1;
            this.checkBox1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox1.UseVisualStyleBackColor = true;
            this.checkBox1.CheckedChanged += new System.EventHandler(this.checkBox1_CheckedChanged);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "paintTool_arrow.bmp");
            this.imageList1.Images.SetKeyName(1, "terrain_FlattenIcon.bmp");
            this.imageList1.Images.SetKeyName(2, "terrain_LowerIcon.bmp");
            this.imageList1.Images.SetKeyName(3, "terrain_RiseIcon.bmp");
            this.imageList1.Images.SetKeyName(4, "terrain_SmoothIcon.bmp");
            // 
            // checkBox2
            // 
            this.checkBox2.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.checkBox2.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBox2.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox2.Checked = true;
            this.checkBox2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox2.Cursor = System.Windows.Forms.Cursors.Default;
            this.checkBox2.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(213)))), ((int)(((byte)(155)))));
            this.checkBox2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox2.ImageKey = "terrain_RiseIcon.bmp";
            this.checkBox2.ImageList = this.imageList1;
            this.checkBox2.Location = new System.Drawing.Point(64, 0);
            this.checkBox2.Name = "checkBox2";
            this.checkBox2.Size = new System.Drawing.Size(62, 62);
            this.checkBox2.TabIndex = 2;
            this.checkBox2.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox2.UseVisualStyleBackColor = true;
            this.checkBox2.CheckedChanged += new System.EventHandler(this.checkBox2_CheckedChanged);
            // 
            // checkBox3
            // 
            this.checkBox3.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.checkBox3.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBox3.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox3.Checked = true;
            this.checkBox3.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox3.Cursor = System.Windows.Forms.Cursors.Default;
            this.checkBox3.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(213)))), ((int)(((byte)(155)))));
            this.checkBox3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox3.ImageKey = "terrain_LowerIcon.bmp";
            this.checkBox3.ImageList = this.imageList1;
            this.checkBox3.Location = new System.Drawing.Point(126, 0);
            this.checkBox3.Name = "checkBox3";
            this.checkBox3.Size = new System.Drawing.Size(62, 62);
            this.checkBox3.TabIndex = 3;
            this.checkBox3.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox3.UseVisualStyleBackColor = true;
            this.checkBox3.CheckedChanged += new System.EventHandler(this.checkBox3_CheckedChanged);
            // 
            // checkBox4
            // 
            this.checkBox4.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.checkBox4.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBox4.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox4.Checked = true;
            this.checkBox4.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox4.Cursor = System.Windows.Forms.Cursors.Default;
            this.checkBox4.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(213)))), ((int)(((byte)(155)))));
            this.checkBox4.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox4.ImageKey = "terrain_SmoothIcon.bmp";
            this.checkBox4.ImageList = this.imageList1;
            this.checkBox4.Location = new System.Drawing.Point(188, 0);
            this.checkBox4.Name = "checkBox4";
            this.checkBox4.Size = new System.Drawing.Size(62, 62);
            this.checkBox4.TabIndex = 4;
            this.checkBox4.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox4.UseVisualStyleBackColor = true;
            this.checkBox4.CheckedChanged += new System.EventHandler(this.checkBox4_CheckedChanged);
            // 
            // checkBox5
            // 
            this.checkBox5.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.checkBox5.Appearance = System.Windows.Forms.Appearance.Button;
            this.checkBox5.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.checkBox5.Checked = true;
            this.checkBox5.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox5.Cursor = System.Windows.Forms.Cursors.Default;
            this.checkBox5.FlatAppearance.CheckedBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(213)))), ((int)(((byte)(155)))));
            this.checkBox5.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkBox5.ImageKey = "terrain_FlattenIcon.bmp";
            this.checkBox5.ImageList = this.imageList1;
            this.checkBox5.Location = new System.Drawing.Point(250, 0);
            this.checkBox5.Name = "checkBox5";
            this.checkBox5.Size = new System.Drawing.Size(62, 62);
            this.checkBox5.TabIndex = 5;
            this.checkBox5.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.checkBox5.UseVisualStyleBackColor = true;
            this.checkBox5.CheckedChanged += new System.EventHandler(this.checkBox5_CheckedChanged);
            // 
            // hScrollBar1
            // 
            this.hScrollBar1.Cursor = System.Windows.Forms.Cursors.Default;
            this.hScrollBar1.LargeChange = 1;
            this.hScrollBar1.Location = new System.Drawing.Point(365, 10);
            this.hScrollBar1.Minimum = 50;
            this.hScrollBar1.Name = "hScrollBar1";
            this.hScrollBar1.Size = new System.Drawing.Size(141, 17);
            this.hScrollBar1.TabIndex = 6;
            this.hScrollBar1.Value = 50;
            this.hScrollBar1.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBar1_Scroll);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(329, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(33, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Size :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(322, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(40, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Force :";
            // 
            // hScrollBar2
            // 
            this.hScrollBar2.Cursor = System.Windows.Forms.Cursors.Default;
            this.hScrollBar2.Location = new System.Drawing.Point(365, 36);
            this.hScrollBar2.Minimum = 1;
            this.hScrollBar2.Name = "hScrollBar2";
            this.hScrollBar2.Size = new System.Drawing.Size(141, 17);
            this.hScrollBar2.TabIndex = 7;
            this.hScrollBar2.Value = 20;
            this.hScrollBar2.Scroll += new System.Windows.Forms.ScrollEventHandler(this.hScrollBar2_Scroll);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(40, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(146, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Amount to raise/lower (Feet) :";
            // 
            // trackBar1
            // 
            this.trackBar1.Location = new System.Drawing.Point(248, 68);
            this.trackBar1.Maximum = 255;
            this.trackBar1.Minimum = 1;
            this.trackBar1.Name = "trackBar1";
            this.trackBar1.Size = new System.Drawing.Size(258, 40);
            this.trackBar1.TabIndex = 12;
            this.trackBar1.TickFrequency = 10;
            this.trackBar1.Value = 1;
            this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
            // 
            // cbConstantFeet
            // 
            this.cbConstantFeet.AutoSize = true;
            this.cbConstantFeet.Location = new System.Drawing.Point(19, 81);
            this.cbConstantFeet.Name = "cbConstantFeet";
            this.cbConstantFeet.Size = new System.Drawing.Size(15, 14);
            this.cbConstantFeet.TabIndex = 13;
            this.cbConstantFeet.UseVisualStyleBackColor = true;
            this.cbConstantFeet.CheckedChanged += new System.EventHandler(this.cbConstantFeet_CheckedChanged);
            // 
            // txtConstantFeet
            // 
            this.txtConstantFeet.Location = new System.Drawing.Point(194, 81);
            this.txtConstantFeet.Name = "txtConstantFeet";
            this.txtConstantFeet.Size = new System.Drawing.Size(48, 20);
            this.txtConstantFeet.TabIndex = 14;
            this.txtConstantFeet.Text = "1";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(16, 122);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(357, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Note: Right-click on a Quad to force Tessellation to Lower Level Of Detail.";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtFloodErrorMessages);
            this.groupBox1.Controls.Add(this.btnGeneratePerlinNoise);
            this.groupBox1.Controls.Add(this.pictureBox);
            this.groupBox1.Controls.Add(this.btnApplyHeightMap);
            this.groupBox1.Controls.Add(this.groupBox3);
            this.groupBox1.Controls.Add(this.groupBox2);
            this.groupBox1.Location = new System.Drawing.Point(14, 151);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(494, 482);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "HeightMap Generator";
            // 
            // txtFloodErrorMessages
            // 
            this.txtFloodErrorMessages.BackColor = System.Drawing.SystemColors.Menu;
            this.txtFloodErrorMessages.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtFloodErrorMessages.ForeColor = System.Drawing.Color.Red;
            this.txtFloodErrorMessages.Location = new System.Drawing.Point(4, 216);
            this.txtFloodErrorMessages.Name = "txtFloodErrorMessages";
            this.txtFloodErrorMessages.ReadOnly = true;
            this.txtFloodErrorMessages.Size = new System.Drawing.Size(484, 13);
            this.txtFloodErrorMessages.TabIndex = 54;
            // 
            // btnGeneratePerlinNoise
            // 
            this.btnGeneratePerlinNoise.Location = new System.Drawing.Point(4, 244);
            this.btnGeneratePerlinNoise.Name = "btnGeneratePerlinNoise";
            this.btnGeneratePerlinNoise.Size = new System.Drawing.Size(75, 23);
            this.btnGeneratePerlinNoise.TabIndex = 53;
            this.btnGeneratePerlinNoise.Text = "Generate";
            this.btnGeneratePerlinNoise.UseVisualStyleBackColor = true;
            this.btnGeneratePerlinNoise.Click += new System.EventHandler(this.btnGeneratePerlinNoise_Click);
            // 
            // pictureBox
            // 
            this.pictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.pictureBox.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.pictureBox.Location = new System.Drawing.Point(134, 244);
            this.pictureBox.Name = "pictureBox";
            this.pictureBox.Size = new System.Drawing.Size(225, 225);
            this.pictureBox.TabIndex = 52;
            this.pictureBox.TabStop = false;
            // 
            // btnApplyHeightMap
            // 
            this.btnApplyHeightMap.Location = new System.Drawing.Point(413, 446);
            this.btnApplyHeightMap.Name = "btnApplyHeightMap";
            this.btnApplyHeightMap.Size = new System.Drawing.Size(75, 23);
            this.btnApplyHeightMap.TabIndex = 10;
            this.btnApplyHeightMap.Text = "Apply";
            this.btnApplyHeightMap.UseVisualStyleBackColor = true;
            this.btnApplyHeightMap.Click += new System.EventHandler(this.btnApplyHeightMap_Click);
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.cbPass2Enable);
            this.groupBox3.Controls.Add(this.nudRandomSeedValue_p2);
            this.groupBox3.Controls.Add(this.label9);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.nudOctaves_p2);
            this.groupBox3.Controls.Add(this.nudNoiseSize_p2);
            this.groupBox3.Controls.Add(this.label11);
            this.groupBox3.Controls.Add(this.label12);
            this.groupBox3.Controls.Add(this.nudPersistence_p2);
            this.groupBox3.Location = new System.Drawing.Point(264, 29);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(200, 181);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Pass 2";
            // 
            // cbPass2Enable
            // 
            this.cbPass2Enable.AutoSize = true;
            this.cbPass2Enable.Location = new System.Drawing.Point(69, 158);
            this.cbPass2Enable.Name = "cbPass2Enable";
            this.cbPass2Enable.Size = new System.Drawing.Size(59, 17);
            this.cbPass2Enable.TabIndex = 9;
            this.cbPass2Enable.Text = "Enable";
            this.cbPass2Enable.UseVisualStyleBackColor = true;
            // 
            // nudRandomSeedValue_p2
            // 
            this.nudRandomSeedValue_p2.Location = new System.Drawing.Point(6, 19);
            this.nudRandomSeedValue_p2.Maximum = new decimal(new int[] {
                                                                           50,
                                                                           0,
                                                                           0,
                                                                           0});
            this.nudRandomSeedValue_p2.Name = "nudRandomSeedValue_p2";
            this.nudRandomSeedValue_p2.Size = new System.Drawing.Size(64, 20);
            this.nudRandomSeedValue_p2.TabIndex = 0;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(76, 115);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(106, 13);
            this.label9.TabIndex = 7;
            this.label9.Text = "Perlin Octaves Value";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(76, 21);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(105, 13);
            this.label10.TabIndex = 1;
            this.label10.Text = "Random Seed Value";
            // 
            // nudOctaves_p2
            // 
            this.nudOctaves_p2.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.nudOctaves_p2.Location = new System.Drawing.Point(6, 113);
            this.nudOctaves_p2.Maximum = new decimal(new int[] {
                                                                   50,
                                                                   0,
                                                                   0,
                                                                   0});
            this.nudOctaves_p2.Name = "nudOctaves_p2";
            this.nudOctaves_p2.Size = new System.Drawing.Size(64, 20);
            this.nudOctaves_p2.TabIndex = 6;
            // 
            // nudNoiseSize_p2
            // 
            this.nudNoiseSize_p2.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.nudNoiseSize_p2.DecimalPlaces = 2;
            this.nudNoiseSize_p2.Increment = new decimal(new int[] {
                                                                       25,
                                                                       0,
                                                                       0,
                                                                       131072});
            this.nudNoiseSize_p2.Location = new System.Drawing.Point(6, 49);
            this.nudNoiseSize_p2.Maximum = new decimal(new int[] {
                                                                     50,
                                                                     0,
                                                                     0,
                                                                     0});
            this.nudNoiseSize_p2.Name = "nudNoiseSize_p2";
            this.nudNoiseSize_p2.Size = new System.Drawing.Size(64, 20);
            this.nudNoiseSize_p2.TabIndex = 2;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(76, 82);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(121, 13);
            this.label11.TabIndex = 5;
            this.label11.Text = "Perlin Persistence Value";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(76, 51);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(116, 13);
            this.label12.TabIndex = 3;
            this.label12.Text = "Perlin Noise Size Value";
            // 
            // nudPersistence_p2
            // 
            this.nudPersistence_p2.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.nudPersistence_p2.DecimalPlaces = 2;
            this.nudPersistence_p2.Increment = new decimal(new int[] {
                                                                         25,
                                                                         0,
                                                                         0,
                                                                         131072});
            this.nudPersistence_p2.Location = new System.Drawing.Point(6, 80);
            this.nudPersistence_p2.Maximum = new decimal(new int[] {
                                                                       50,
                                                                       0,
                                                                       0,
                                                                       0});
            this.nudPersistence_p2.Minimum = new decimal(new int[] {
                                                                       50,
                                                                       0,
                                                                       0,
                                                                       -2147483648});
            this.nudPersistence_p2.Name = "nudPersistence_p2";
            this.nudPersistence_p2.Size = new System.Drawing.Size(64, 20);
            this.nudPersistence_p2.TabIndex = 4;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.cbPass1Enable);
            this.groupBox2.Controls.Add(this.nudRandomSeedValue_p1);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.nudOctaves_p1);
            this.groupBox2.Controls.Add(this.nudNoiseSize_p1);
            this.groupBox2.Controls.Add(this.label7);
            this.groupBox2.Controls.Add(this.label6);
            this.groupBox2.Controls.Add(this.nudPersistence_p1);
            this.groupBox2.Location = new System.Drawing.Point(34, 29);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(200, 181);
            this.groupBox2.TabIndex = 8;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Pass 1";
            // 
            // cbPass1Enable
            // 
            this.cbPass1Enable.AutoSize = true;
            this.cbPass1Enable.Location = new System.Drawing.Point(67, 158);
            this.cbPass1Enable.Name = "cbPass1Enable";
            this.cbPass1Enable.Size = new System.Drawing.Size(59, 17);
            this.cbPass1Enable.TabIndex = 8;
            this.cbPass1Enable.Text = "Enable";
            this.cbPass1Enable.UseVisualStyleBackColor = true;
            // 
            // nudRandomSeedValue_p1
            // 
            this.nudRandomSeedValue_p1.Location = new System.Drawing.Point(6, 19);
            this.nudRandomSeedValue_p1.Maximum = new decimal(new int[] {
                                                                           50,
                                                                           0,
                                                                           0,
                                                                           0});
            this.nudRandomSeedValue_p1.Name = "nudRandomSeedValue_p1";
            this.nudRandomSeedValue_p1.Size = new System.Drawing.Size(64, 20);
            this.nudRandomSeedValue_p1.TabIndex = 0;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(76, 115);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(106, 13);
            this.label8.TabIndex = 7;
            this.label8.Text = "Perlin Octaves Value";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(76, 21);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 13);
            this.label5.TabIndex = 1;
            this.label5.Text = "Random Seed Value";
            // 
            // nudOctaves_p1
            // 
            this.nudOctaves_p1.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.nudOctaves_p1.Location = new System.Drawing.Point(6, 113);
            this.nudOctaves_p1.Maximum = new decimal(new int[] {
                                                                   50,
                                                                   0,
                                                                   0,
                                                                   0});
            this.nudOctaves_p1.Name = "nudOctaves_p1";
            this.nudOctaves_p1.Size = new System.Drawing.Size(64, 20);
            this.nudOctaves_p1.TabIndex = 6;
            // 
            // nudNoiseSize_p1
            // 
            this.nudNoiseSize_p1.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.nudNoiseSize_p1.DecimalPlaces = 2;
            this.nudNoiseSize_p1.Increment = new decimal(new int[] {
                                                                       25,
                                                                       0,
                                                                       0,
                                                                       131072});
            this.nudNoiseSize_p1.Location = new System.Drawing.Point(6, 49);
            this.nudNoiseSize_p1.Maximum = new decimal(new int[] {
                                                                     50,
                                                                     0,
                                                                     0,
                                                                     0});
            this.nudNoiseSize_p1.Name = "nudNoiseSize_p1";
            this.nudNoiseSize_p1.Size = new System.Drawing.Size(64, 20);
            this.nudNoiseSize_p1.TabIndex = 2;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(76, 82);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(121, 13);
            this.label7.TabIndex = 5;
            this.label7.Text = "Perlin Persistence Value";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(76, 51);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(116, 13);
            this.label6.TabIndex = 3;
            this.label6.Text = "Perlin Noise Size Value";
            // 
            // nudPersistence_p1
            // 
            this.nudPersistence_p1.AccessibleRole = System.Windows.Forms.AccessibleRole.None;
            this.nudPersistence_p1.DecimalPlaces = 2;
            this.nudPersistence_p1.Increment = new decimal(new int[] {
                                                                         25,
                                                                         0,
                                                                         0,
                                                                         131072});
            this.nudPersistence_p1.Location = new System.Drawing.Point(6, 80);
            this.nudPersistence_p1.Maximum = new decimal(new int[] {
                                                                       50,
                                                                       0,
                                                                       0,
                                                                       0});
            this.nudPersistence_p1.Minimum = new decimal(new int[] {
                                                                       50,
                                                                       0,
                                                                       0,
                                                                       -2147483648});
            this.nudPersistence_p1.Name = "nudPersistence_p1";
            this.nudPersistence_p1.Size = new System.Drawing.Size(64, 20);
            this.nudPersistence_p1.TabIndex = 4;
            // 
            // btnCreateMap1024x1024
            // 
            this.btnCreateMap1024x1024.Location = new System.Drawing.Point(433, 112);
            this.btnCreateMap1024x1024.Name = "btnCreateMap1024x1024";
            this.btnCreateMap1024x1024.Size = new System.Drawing.Size(75, 23);
            this.btnCreateMap1024x1024.TabIndex = 17;
            this.btnCreateMap1024x1024.Text = "1024x1024";
            this.btnCreateMap1024x1024.UseVisualStyleBackColor = true;
            this.btnCreateMap1024x1024.Click += new System.EventHandler(this.btnCreateMap1024x1024_Click);
            // 
            // HeightTools
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(524, 645);
            this.Controls.Add(this.btnCreateMap1024x1024);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtConstantFeet);
            this.Controls.Add(this.cbConstantFeet);
            this.Controls.Add(this.trackBar1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.hScrollBar2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.hScrollBar1);
            this.Controls.Add(this.checkBox5);
            this.Controls.Add(this.checkBox4);
            this.Controls.Add(this.checkBox3);
            this.Controls.Add(this.checkBox2);
            this.Controls.Add(this.checkBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "HeightTools";
            this.Text = "HeightTools";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.HeightTools_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HeightTools_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.trackBar1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox)).EndInit();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRandomSeedValue_p2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOctaves_p2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNoiseSize_p2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPersistence_p2)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudRandomSeedValue_p1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudOctaves_p1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudNoiseSize_p1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudPersistence_p1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.CheckBox checkBox2;
        private System.Windows.Forms.CheckBox checkBox3;
        private System.Windows.Forms.CheckBox checkBox4;
        private System.Windows.Forms.CheckBox checkBox5;
        private System.Windows.Forms.HScrollBar hScrollBar1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.HScrollBar hScrollBar2;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TrackBar trackBar1;
        private System.Windows.Forms.CheckBox cbConstantFeet;
        private System.Windows.Forms.TextBox txtConstantFeet;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.NumericUpDown nudRandomSeedValue_p1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.NumericUpDown nudNoiseSize_p1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown nudPersistence_p1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown nudOctaves_p1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.NumericUpDown nudRandomSeedValue_p2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown nudOctaves_p2;
        private System.Windows.Forms.NumericUpDown nudNoiseSize_p2;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.NumericUpDown nudPersistence_p2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnApplyHeightMap;
        private System.Windows.Forms.CheckBox cbPass2Enable;
        private System.Windows.Forms.CheckBox cbPass1Enable;
        private System.Windows.Forms.Button btnCreateMap1024x1024;
        private System.Windows.Forms.Button btnGeneratePerlinNoise;
        private System.Windows.Forms.PictureBox pictureBox;
        private System.Windows.Forms.TextBox txtFloodErrorMessages;

    }
}