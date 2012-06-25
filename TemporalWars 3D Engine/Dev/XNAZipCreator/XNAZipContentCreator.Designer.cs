namespace XNAZipCreator
{
    partial class FrmXNAZipContentCreator
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
            this.txtContentDir_p = new System.Windows.Forms.TextBox();
            this.txtContentOutput_p = new System.Windows.Forms.TextBox();
            this.txtResourceFile_p = new System.Windows.Forms.TextBox();
            this.txtResourceNamespace_p = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.txtResourceClassName_p = new System.Windows.Forms.TextBox();
            this.btnCreateZipFile_p = new System.Windows.Forms.Button();
            this.txtVisualStudioEnv = new System.Windows.Forms.TextBox();
            this.txtProjectDir = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.txtContentDir = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btnCreateZipFile_s = new System.Windows.Forms.Button();
            this.txtContentDir_s = new System.Windows.Forms.TextBox();
            this.txtContentOutput_s = new System.Windows.Forms.TextBox();
            this.txtResourceFile_s = new System.Windows.Forms.TextBox();
            this.txtResourceNamespace_s = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.txtResourceClassName_s = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.LblMessages = new System.Windows.Forms.Label();
            this.cmbPlatform = new System.Windows.Forms.ComboBox();
            this.label13 = new System.Windows.Forms.Label();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tbPlayable = new System.Windows.Forms.TabPage();
            this.tbScenary = new System.Windows.Forms.TabPage();
            this.tbTerrainTextures = new System.Windows.Forms.TabPage();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnCreateZipFile_t = new System.Windows.Forms.Button();
            this.txtContentDir_t = new System.Windows.Forms.TextBox();
            this.txtContentOutput_t = new System.Windows.Forms.TextBox();
            this.txtResourceFile_t = new System.Windows.Forms.TextBox();
            this.txtResourceNamespace_t = new System.Windows.Forms.TextBox();
            this.label14 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label16 = new System.Windows.Forms.Label();
            this.label17 = new System.Windows.Forms.Label();
            this.txtResourceClassName_t = new System.Windows.Forms.TextBox();
            this.label18 = new System.Windows.Forms.Label();
            this.label19 = new System.Windows.Forms.Label();
            this.errorProviderName = new System.Windows.Forms.ErrorProvider(this.components);
            this.btnGetVSDirectory = new System.Windows.Forms.Button();
            this.btnCreateItemToolAssetNameList = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tbPlayable.SuspendLayout();
            this.tbScenary.SuspendLayout();
            this.tbTerrainTextures.SuspendLayout();
            this.groupBox3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderName)).BeginInit();
            this.SuspendLayout();
            // 
            // txtContentDir_p
            // 
            this.txtContentDir_p.Location = new System.Drawing.Point(150, 35);
            this.txtContentDir_p.Name = "txtContentDir_p";
            this.txtContentDir_p.Size = new System.Drawing.Size(388, 20);
            this.txtContentDir_p.TabIndex = 0;
            this.txtContentDir_p.Text = "ContentAlleyPack\\*.xnb";
            // 
            // txtContentOutput_p
            // 
            this.txtContentOutput_p.Location = new System.Drawing.Point(149, 61);
            this.txtContentOutput_p.Name = "txtContentOutput_p";
            this.txtContentOutput_p.Size = new System.Drawing.Size(388, 20);
            this.txtContentOutput_p.TabIndex = 1;
            this.txtContentOutput_p.Text = "Content.zip";
            // 
            // txtResourceFile_p
            // 
            this.txtResourceFile_p.AcceptsReturn = true;
            this.txtResourceFile_p.Location = new System.Drawing.Point(149, 87);
            this.txtResourceFile_p.Name = "txtResourceFile_p";
            this.txtResourceFile_p.Size = new System.Drawing.Size(388, 20);
            this.txtResourceFile_p.TabIndex = 2;
            this.txtResourceFile_p.Text = "ResourceId.cs";
            // 
            // txtResourceNamespace_p
            // 
            this.txtResourceNamespace_p.AcceptsTab = true;
            this.txtResourceNamespace_p.Location = new System.Drawing.Point(150, 113);
            this.txtResourceNamespace_p.Name = "txtResourceNamespace_p";
            this.txtResourceNamespace_p.Size = new System.Drawing.Size(388, 20);
            this.txtResourceNamespace_p.TabIndex = 3;
            this.txtResourceNamespace_p.Text = "Spacewar";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 38);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(97, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Content Directories";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 61);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Content Zip Output:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Resource File:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 116);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(131, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Namespace for Resource:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(8, 142);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(130, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Class Name for Resource:";
            // 
            // txtResourceClassName_p
            // 
            this.txtResourceClassName_p.AcceptsReturn = true;
            this.txtResourceClassName_p.Location = new System.Drawing.Point(149, 139);
            this.txtResourceClassName_p.Name = "txtResourceClassName_p";
            this.txtResourceClassName_p.Size = new System.Drawing.Size(388, 20);
            this.txtResourceClassName_p.TabIndex = 8;
            this.txtResourceClassName_p.Text = "ResourceId";
            // 
            // btnCreateZipFile_p
            // 
            this.btnCreateZipFile_p.Location = new System.Drawing.Point(225, 165);
            this.btnCreateZipFile_p.Name = "btnCreateZipFile_p";
            this.btnCreateZipFile_p.Size = new System.Drawing.Size(101, 23);
            this.btnCreateZipFile_p.TabIndex = 10;
            this.btnCreateZipFile_p.Text = "Create Zip File";
            this.btnCreateZipFile_p.UseVisualStyleBackColor = true;
            this.btnCreateZipFile_p.Click += new System.EventHandler(this.btnCreateZipFile_Click);
            // 
            // txtVisualStudioEnv
            // 
            this.errorProviderName.SetError(this.txtVisualStudioEnv, "Enter the Visual Studio path");
            this.errorProviderName.SetIconPadding(this.txtVisualStudioEnv, 5);
            this.txtVisualStudioEnv.Location = new System.Drawing.Point(159, 9);
            this.txtVisualStudioEnv.Name = "txtVisualStudioEnv";
            this.txtVisualStudioEnv.Size = new System.Drawing.Size(388, 20);
            this.txtVisualStudioEnv.TabIndex = 11;
            this.txtVisualStudioEnv.TextChanged += new System.EventHandler(this.txtVisualStudioEnv_TextChanged);
            // 
            // txtProjectDir
            // 
            this.errorProviderName.SetIconPadding(this.txtProjectDir, 5);
            this.txtProjectDir.Location = new System.Drawing.Point(159, 35);
            this.txtProjectDir.Name = "txtProjectDir";
            this.txtProjectDir.Size = new System.Drawing.Size(388, 20);
            this.txtProjectDir.TabIndex = 12;
            this.txtProjectDir.Text = "\\\\Projects\\\\TemporalWars 3D Engine\\\\Dev\\\\XNA_RTS2008\\\\";
            this.txtProjectDir.TextChanged += new System.EventHandler(this.txtProjectDir_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(18, 38);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(107, 13);
            this.label6.TabIndex = 13;
            this.label6.Text = "Project Sub-Directory";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(18, 64);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(111, 13);
            this.label7.TabIndex = 15;
            this.label7.Text = "Content Sub-Directory";
            // 
            // txtContentDir
            // 
            this.errorProviderName.SetIconPadding(this.txtContentDir, 5);
            this.txtContentDir.Location = new System.Drawing.Point(159, 61);
            this.txtContentDir.Name = "txtContentDir";
            this.txtContentDir.Size = new System.Drawing.Size(388, 20);
            this.txtContentDir.TabIndex = 14;
            this.txtContentDir.Text = "\\\\Projects\\\\TemporalWars 3D Engine\\\\Dev\\\\ContentForResources\\\\";
            this.txtContentDir.TextChanged += new System.EventHandler(this.txtContentDir_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.btnCreateZipFile_p);
            this.groupBox1.Controls.Add(this.txtContentDir_p);
            this.groupBox1.Controls.Add(this.txtContentOutput_p);
            this.groupBox1.Controls.Add(this.txtResourceFile_p);
            this.groupBox1.Controls.Add(this.txtResourceNamespace_p);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.txtResourceClassName_p);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(9, 22);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(554, 194);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "PlayableItems";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btnCreateZipFile_s);
            this.groupBox2.Controls.Add(this.txtContentDir_s);
            this.groupBox2.Controls.Add(this.txtContentOutput_s);
            this.groupBox2.Controls.Add(this.txtResourceFile_s);
            this.groupBox2.Controls.Add(this.txtResourceNamespace_s);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.label10);
            this.groupBox2.Controls.Add(this.label11);
            this.groupBox2.Controls.Add(this.txtResourceClassName_s);
            this.groupBox2.Controls.Add(this.label12);
            this.groupBox2.Location = new System.Drawing.Point(9, 22);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(554, 194);
            this.groupBox2.TabIndex = 17;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "ScenaryItems";
            // 
            // btnCreateZipFile_s
            // 
            this.btnCreateZipFile_s.Location = new System.Drawing.Point(225, 165);
            this.btnCreateZipFile_s.Name = "btnCreateZipFile_s";
            this.btnCreateZipFile_s.Size = new System.Drawing.Size(101, 23);
            this.btnCreateZipFile_s.TabIndex = 10;
            this.btnCreateZipFile_s.Text = "Create Zip File";
            this.btnCreateZipFile_s.UseVisualStyleBackColor = true;
            this.btnCreateZipFile_s.Click += new System.EventHandler(this.btnCreateZipFile_s_Click);
            // 
            // txtContentDir_s
            // 
            this.txtContentDir_s.Location = new System.Drawing.Point(150, 35);
            this.txtContentDir_s.Name = "txtContentDir_s";
            this.txtContentDir_s.Size = new System.Drawing.Size(388, 20);
            this.txtContentDir_s.TabIndex = 0;
            this.txtContentDir_s.Text = "ContentAlleyPack\\*.xnb";
            // 
            // txtContentOutput_s
            // 
            this.txtContentOutput_s.Location = new System.Drawing.Point(149, 61);
            this.txtContentOutput_s.Name = "txtContentOutput_s";
            this.txtContentOutput_s.Size = new System.Drawing.Size(388, 20);
            this.txtContentOutput_s.TabIndex = 1;
            this.txtContentOutput_s.Text = "Content.zip";
            // 
            // txtResourceFile_s
            // 
            this.txtResourceFile_s.AcceptsReturn = true;
            this.txtResourceFile_s.Location = new System.Drawing.Point(149, 87);
            this.txtResourceFile_s.Name = "txtResourceFile_s";
            this.txtResourceFile_s.Size = new System.Drawing.Size(388, 20);
            this.txtResourceFile_s.TabIndex = 2;
            this.txtResourceFile_s.Text = "ResourceId.cs";
            // 
            // txtResourceNamespace_s
            // 
            this.txtResourceNamespace_s.AcceptsTab = true;
            this.txtResourceNamespace_s.Location = new System.Drawing.Point(150, 113);
            this.txtResourceNamespace_s.Name = "txtResourceNamespace_s";
            this.txtResourceNamespace_s.Size = new System.Drawing.Size(388, 20);
            this.txtResourceNamespace_s.TabIndex = 3;
            this.txtResourceNamespace_s.Text = "Spacewar";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(9, 38);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(97, 13);
            this.label8.TabIndex = 4;
            this.label8.Text = "Content Directories";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(9, 61);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(100, 13);
            this.label9.TabIndex = 5;
            this.label9.Text = "Content Zip Output:";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(8, 142);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(130, 13);
            this.label10.TabIndex = 9;
            this.label10.Text = "Class Name for Resource:";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(9, 87);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(75, 13);
            this.label11.TabIndex = 6;
            this.label11.Text = "Resource File:";
            // 
            // txtResourceClassName_s
            // 
            this.txtResourceClassName_s.AcceptsReturn = true;
            this.txtResourceClassName_s.Location = new System.Drawing.Point(149, 139);
            this.txtResourceClassName_s.Name = "txtResourceClassName_s";
            this.txtResourceClassName_s.Size = new System.Drawing.Size(388, 20);
            this.txtResourceClassName_s.TabIndex = 8;
            this.txtResourceClassName_s.Text = "ResourceId";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(9, 116);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(131, 13);
            this.label12.TabIndex = 7;
            this.label12.Text = "Namespace for Resource:";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(31, 427);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(554, 23);
            this.progressBar1.TabIndex = 18;
            // 
            // LblMessages
            // 
            this.LblMessages.ForeColor = System.Drawing.Color.Blue;
            this.LblMessages.Location = new System.Drawing.Point(31, 453);
            this.LblMessages.Name = "LblMessages";
            this.LblMessages.Size = new System.Drawing.Size(554, 108);
            this.LblMessages.TabIndex = 19;
            this.LblMessages.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // cmbPlatform
            // 
            this.cmbPlatform.FormattingEnabled = true;
            this.cmbPlatform.Items.AddRange(new object[] {
            "x86",
            "Xbox 360"});
            this.cmbPlatform.Location = new System.Drawing.Point(159, 87);
            this.cmbPlatform.Name = "cmbPlatform";
            this.cmbPlatform.Size = new System.Drawing.Size(71, 21);
            this.cmbPlatform.TabIndex = 20;
            this.cmbPlatform.Text = "x86";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(20, 90);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(88, 13);
            this.label13.TabIndex = 21;
            this.label13.Text = "Content Platform:";
            // 
            // tabControl
            // 
            this.tabControl.Controls.Add(this.tbPlayable);
            this.tabControl.Controls.Add(this.tbScenary);
            this.tabControl.Controls.Add(this.tbTerrainTextures);
            this.tabControl.Location = new System.Drawing.Point(21, 144);
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(583, 264);
            this.tabControl.TabIndex = 22;
            // 
            // tbPlayable
            // 
            this.tbPlayable.Controls.Add(this.groupBox1);
            this.tbPlayable.Location = new System.Drawing.Point(4, 22);
            this.tbPlayable.Name = "tbPlayable";
            this.tbPlayable.Padding = new System.Windows.Forms.Padding(3);
            this.tbPlayable.Size = new System.Drawing.Size(575, 238);
            this.tbPlayable.TabIndex = 0;
            this.tbPlayable.Text = "PlayableItems";
            this.tbPlayable.UseVisualStyleBackColor = true;
            // 
            // tbScenary
            // 
            this.tbScenary.Controls.Add(this.groupBox2);
            this.tbScenary.Location = new System.Drawing.Point(4, 22);
            this.tbScenary.Name = "tbScenary";
            this.tbScenary.Padding = new System.Windows.Forms.Padding(3);
            this.tbScenary.Size = new System.Drawing.Size(575, 238);
            this.tbScenary.TabIndex = 1;
            this.tbScenary.Text = "ScenaryItems";
            this.tbScenary.UseVisualStyleBackColor = true;
            // 
            // tbTerrainTextures
            // 
            this.tbTerrainTextures.Controls.Add(this.groupBox3);
            this.tbTerrainTextures.Location = new System.Drawing.Point(4, 22);
            this.tbTerrainTextures.Name = "tbTerrainTextures";
            this.tbTerrainTextures.Size = new System.Drawing.Size(575, 238);
            this.tbTerrainTextures.TabIndex = 2;
            this.tbTerrainTextures.Text = "TerrainTextures";
            this.tbTerrainTextures.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnCreateZipFile_t);
            this.groupBox3.Controls.Add(this.txtContentDir_t);
            this.groupBox3.Controls.Add(this.txtContentOutput_t);
            this.groupBox3.Controls.Add(this.txtResourceFile_t);
            this.groupBox3.Controls.Add(this.txtResourceNamespace_t);
            this.groupBox3.Controls.Add(this.label14);
            this.groupBox3.Controls.Add(this.label15);
            this.groupBox3.Controls.Add(this.label16);
            this.groupBox3.Controls.Add(this.label17);
            this.groupBox3.Controls.Add(this.txtResourceClassName_t);
            this.groupBox3.Controls.Add(this.label18);
            this.groupBox3.Location = new System.Drawing.Point(9, 22);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(554, 194);
            this.groupBox3.TabIndex = 18;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "TerrainTextures";
            // 
            // btnCreateZipFile_t
            // 
            this.btnCreateZipFile_t.Location = new System.Drawing.Point(225, 165);
            this.btnCreateZipFile_t.Name = "btnCreateZipFile_t";
            this.btnCreateZipFile_t.Size = new System.Drawing.Size(101, 23);
            this.btnCreateZipFile_t.TabIndex = 10;
            this.btnCreateZipFile_t.Text = "Create Zip File";
            this.btnCreateZipFile_t.UseVisualStyleBackColor = true;
            this.btnCreateZipFile_t.Click += new System.EventHandler(this.btnCreateZipFile_t_Click);
            // 
            // txtContentDir_t
            // 
            this.txtContentDir_t.AcceptsReturn = true;
            this.txtContentDir_t.Location = new System.Drawing.Point(150, 35);
            this.txtContentDir_t.Name = "txtContentDir_t";
            this.txtContentDir_t.Size = new System.Drawing.Size(388, 20);
            this.txtContentDir_t.TabIndex = 0;
            this.txtContentDir_t.Text = "ContentAlleyPack\\*.xnb";
            // 
            // txtContentOutput_t
            // 
            this.txtContentOutput_t.Location = new System.Drawing.Point(149, 61);
            this.txtContentOutput_t.Name = "txtContentOutput_t";
            this.txtContentOutput_t.Size = new System.Drawing.Size(388, 20);
            this.txtContentOutput_t.TabIndex = 1;
            this.txtContentOutput_t.Text = "Content.zip";
            // 
            // txtResourceFile_t
            // 
            this.txtResourceFile_t.AcceptsReturn = true;
            this.txtResourceFile_t.Location = new System.Drawing.Point(149, 87);
            this.txtResourceFile_t.Name = "txtResourceFile_t";
            this.txtResourceFile_t.Size = new System.Drawing.Size(388, 20);
            this.txtResourceFile_t.TabIndex = 2;
            this.txtResourceFile_t.Text = "ResourceId.cs";
            // 
            // txtResourceNamespace_t
            // 
            this.txtResourceNamespace_t.AcceptsTab = true;
            this.txtResourceNamespace_t.Location = new System.Drawing.Point(150, 113);
            this.txtResourceNamespace_t.Name = "txtResourceNamespace_t";
            this.txtResourceNamespace_t.Size = new System.Drawing.Size(388, 20);
            this.txtResourceNamespace_t.TabIndex = 3;
            this.txtResourceNamespace_t.Text = "Spacewar";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(9, 38);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(97, 13);
            this.label14.TabIndex = 4;
            this.label14.Text = "Content Directories";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(9, 61);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(100, 13);
            this.label15.TabIndex = 5;
            this.label15.Text = "Content Zip Output:";
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(8, 142);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(130, 13);
            this.label16.TabIndex = 9;
            this.label16.Text = "Class Name for Resource:";
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(9, 87);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(75, 13);
            this.label17.TabIndex = 6;
            this.label17.Text = "Resource File:";
            // 
            // txtResourceClassName_t
            // 
            this.txtResourceClassName_t.AcceptsReturn = true;
            this.txtResourceClassName_t.Location = new System.Drawing.Point(149, 139);
            this.txtResourceClassName_t.Name = "txtResourceClassName_t";
            this.txtResourceClassName_t.Size = new System.Drawing.Size(388, 20);
            this.txtResourceClassName_t.TabIndex = 8;
            this.txtResourceClassName_t.Text = "ResourceId";
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(9, 116);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(131, 13);
            this.label18.TabIndex = 7;
            this.label18.Text = "Namespace for Resource:";
            // 
            // label19
            // 
            this.label19.AutoSize = true;
            this.label19.Location = new System.Drawing.Point(20, 12);
            this.label19.Name = "label19";
            this.label19.Size = new System.Drawing.Size(117, 13);
            this.label19.TabIndex = 23;
            this.label19.Text = "VisualStudio\'s Directory";
            // 
            // errorProviderName
            // 
            this.errorProviderName.ContainerControl = this;
            // 
            // btnGetVSDirectory
            // 
            this.btnGetVSDirectory.Location = new System.Drawing.Point(576, 7);
            this.btnGetVSDirectory.Name = "btnGetVSDirectory";
            this.btnGetVSDirectory.Size = new System.Drawing.Size(34, 23);
            this.btnGetVSDirectory.TabIndex = 24;
            this.btnGetVSDirectory.Text = "Get";
            this.btnGetVSDirectory.UseVisualStyleBackColor = true;
            this.btnGetVSDirectory.Click += new System.EventHandler(this.btnGetVSDirectory_Click);
            // 
            // btnCreateItemToolAssetNameList
            // 
            this.btnCreateItemToolAssetNameList.Location = new System.Drawing.Point(364, 90);
            this.btnCreateItemToolAssetNameList.Name = "btnCreateItemToolAssetNameList";
            this.btnCreateItemToolAssetNameList.Size = new System.Drawing.Size(183, 23);
            this.btnCreateItemToolAssetNameList.TabIndex = 25;
            this.btnCreateItemToolAssetNameList.Text = "ItemTool Create Name List";
            this.btnCreateItemToolAssetNameList.UseVisualStyleBackColor = true;
            this.btnCreateItemToolAssetNameList.Click += new System.EventHandler(this.btnCreateItemToolAssetNameList_Click);
            // 
            // FrmXNAZipContentCreator
            // 
            this.AcceptButton = this.btnCreateZipFile_p;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 570);
            this.Controls.Add(this.btnCreateItemToolAssetNameList);
            this.Controls.Add(this.btnGetVSDirectory);
            this.Controls.Add(this.label19);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.label13);
            this.Controls.Add(this.cmbPlatform);
            this.Controls.Add(this.LblMessages);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.txtContentDir);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.txtProjectDir);
            this.Controls.Add(this.txtVisualStudioEnv);
            this.Name = "FrmXNAZipContentCreator";
            this.Text = "XNA Zip Content Creator";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tbPlayable.ResumeLayout(false);
            this.tbScenary.ResumeLayout(false);
            this.tbTerrainTextures.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.errorProviderName)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtContentDir_p;
        private System.Windows.Forms.TextBox txtContentOutput_p;
        private System.Windows.Forms.TextBox txtResourceFile_p;
        private System.Windows.Forms.TextBox txtResourceNamespace_p;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtResourceClassName_p;
        private System.Windows.Forms.Button btnCreateZipFile_p;
        private System.Windows.Forms.TextBox txtVisualStudioEnv;
        private System.Windows.Forms.TextBox txtProjectDir;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox txtContentDir;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnCreateZipFile_s;
        private System.Windows.Forms.TextBox txtContentDir_s;
        private System.Windows.Forms.TextBox txtContentOutput_s;
        private System.Windows.Forms.TextBox txtResourceFile_s;
        private System.Windows.Forms.TextBox txtResourceNamespace_s;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox txtResourceClassName_s;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label LblMessages;
        private System.Windows.Forms.ComboBox cmbPlatform;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tbPlayable;
        private System.Windows.Forms.TabPage tbScenary;
        private System.Windows.Forms.TabPage tbTerrainTextures;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnCreateZipFile_t;
        private System.Windows.Forms.TextBox txtContentDir_t;
        private System.Windows.Forms.TextBox txtContentOutput_t;
        private System.Windows.Forms.TextBox txtResourceFile_t;
        private System.Windows.Forms.TextBox txtResourceNamespace_t;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.TextBox txtResourceClassName_t;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.Label label19;
        private System.Windows.Forms.ErrorProvider errorProviderName;
        private System.Windows.Forms.Button btnGetVSDirectory;
        private System.Windows.Forms.Button btnCreateItemToolAssetNameList;
    }
}

