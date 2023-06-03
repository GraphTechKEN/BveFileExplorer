namespace AtsPluginEditor
{
    partial class Form1
    {
        /// <summary>
        /// 必要なデザイナー変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースを破棄する場合は true を指定し、その他の場合は false を指定します。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナーで生成されたコード

        /// <summary>
        /// デザイナー サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディターで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.btnOpen = new System.Windows.Forms.Button();
            this.btnOpenRouteFile = new System.Windows.Forms.Button();
            this.btnOpenVehicleFile = new System.Windows.Forms.Button();
            this.btnOpenAtsPluginFile = new System.Windows.Forms.Button();
            this.btnOpenAtsPluginDirectory = new System.Windows.Forms.Button();
            this.btnOpenVehicleDirectory = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.btnAtsPluginDirectory = new System.Windows.Forms.Button();
            this.lblAtsPluginFileName = new System.Windows.Forms.Label();
            this.btnGenerateRelativePath = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tpVehicle = new System.Windows.Forms.TabPage();
            this.cbxVehicle = new System.Windows.Forms.ComboBox();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tbAts64 = new System.Windows.Forms.TextBox();
            this.tbAts32 = new System.Windows.Forms.TextBox();
            this.tbMotorNoise = new System.Windows.Forms.TextBox();
            this.tbSound = new System.Windows.Forms.TextBox();
            this.tbPanel = new System.Windows.Forms.TextBox();
            this.btnAts64Open = new System.Windows.Forms.Button();
            this.btnAts32Open = new System.Windows.Forms.Button();
            this.btnMotorNoiseOpen = new System.Windows.Forms.Button();
            this.btnSoundOpen = new System.Windows.Forms.Button();
            this.btnPanelOpen = new System.Windows.Forms.Button();
            this.tbParameters = new System.Windows.Forms.TextBox();
            this.btnParametersOpen = new System.Windows.Forms.Button();
            this.btnVehicleOpen = new System.Windows.Forms.Button();
            this.tbPerfoemanceCurve = new System.Windows.Forms.TextBox();
            this.btnPerfoemanceCurveOpen = new System.Windows.Forms.Button();
            this.tpRoute = new System.Windows.Forms.TabPage();
            this.label16 = new System.Windows.Forms.Label();
            this.label15 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.label12 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.tbTrain = new System.Windows.Forms.TextBox();
            this.tbSoundList = new System.Windows.Forms.TextBox();
            this.btnTrainOpen = new System.Windows.Forms.Button();
            this.tbStation = new System.Windows.Forms.TextBox();
            this.btnSoundListOpen = new System.Windows.Forms.Button();
            this.btnStationOpen = new System.Windows.Forms.Button();
            this.tbSignal = new System.Windows.Forms.TextBox();
            this.btnSignalOpen = new System.Windows.Forms.Button();
            this.tbStructure = new System.Windows.Forms.TextBox();
            this.btnStructureOpen = new System.Windows.Forms.Button();
            this.tbMapFilePath = new System.Windows.Forms.TextBox();
            this.btnMapOpen = new System.Windows.Forms.Button();
            this.tpLog = new System.Windows.Forms.TabPage();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnBootBVE = new System.Windows.Forms.Button();
            this.lblSeinarioFileName = new System.Windows.Forms.Label();
            this.cbMessageDisp = new System.Windows.Forms.CheckBox();
            this.btnBveBootChooseVehicle = new System.Windows.Forms.Button();
            this.lblVehicleTitle = new System.Windows.Forms.Label();
            this.lblRouteTitle = new System.Windows.Forms.Label();
            this.lblAuthor = new System.Windows.Forms.Label();
            this.lblTitle = new System.Windows.Forms.Label();
            this.lblComment = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tpVehicle.SuspendLayout();
            this.tpRoute.SuspendLayout();
            this.tpLog.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button1.Location = new System.Drawing.Point(698, 11);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(86, 50);
            this.button1.TabIndex = 0;
            this.button1.Text = "自動追記";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Enabled = false;
            this.button2.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button2.Location = new System.Drawing.Point(790, 11);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(77, 50);
            this.button2.TabIndex = 0;
            this.button2.Text = "自動解除";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // btnOpen
            // 
            this.btnOpen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            this.btnOpen.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnOpen.Location = new System.Drawing.Point(24, 72);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(101, 50);
            this.btnOpen.TabIndex = 4;
            this.btnOpen.Text = "2.シナリオファイル\r\nを指定";
            this.btnOpen.UseVisualStyleBackColor = false;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnOpenRouteFile
            // 
            this.btnOpenRouteFile.Enabled = false;
            this.btnOpenRouteFile.Location = new System.Drawing.Point(297, 11);
            this.btnOpenRouteFile.Name = "btnOpenRouteFile";
            this.btnOpenRouteFile.Size = new System.Drawing.Size(109, 50);
            this.btnOpenRouteFile.TabIndex = 5;
            this.btnOpenRouteFile.Text = "シナリオファイル\r\nを開く";
            this.btnOpenRouteFile.UseVisualStyleBackColor = true;
            this.btnOpenRouteFile.Click += new System.EventHandler(this.btnOpenRouteFile_Click);
            // 
            // btnOpenVehicleFile
            // 
            this.btnOpenVehicleFile.Enabled = false;
            this.btnOpenVehicleFile.Location = new System.Drawing.Point(412, 11);
            this.btnOpenVehicleFile.Name = "btnOpenVehicleFile";
            this.btnOpenVehicleFile.Size = new System.Drawing.Size(108, 50);
            this.btnOpenVehicleFile.TabIndex = 6;
            this.btnOpenVehicleFile.Text = "車両ファイルを開く";
            this.btnOpenVehicleFile.UseVisualStyleBackColor = true;
            this.btnOpenVehicleFile.Click += new System.EventHandler(this.btnOpenVehicleFile_Click);
            // 
            // btnOpenAtsPluginFile
            // 
            this.btnOpenAtsPluginFile.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.btnOpenAtsPluginFile.Enabled = false;
            this.btnOpenAtsPluginFile.Location = new System.Drawing.Point(527, 11);
            this.btnOpenAtsPluginFile.Name = "btnOpenAtsPluginFile";
            this.btnOpenAtsPluginFile.Size = new System.Drawing.Size(113, 50);
            this.btnOpenAtsPluginFile.TabIndex = 7;
            this.btnOpenAtsPluginFile.Text = "3.ATSプラグイン\r\n(detailmodules.txt)\r\n設定ファイルを開く";
            this.btnOpenAtsPluginFile.UseVisualStyleBackColor = false;
            this.btnOpenAtsPluginFile.Click += new System.EventHandler(this.btnOpenAtsPluginFile_Click);
            // 
            // btnOpenAtsPluginDirectory
            // 
            this.btnOpenAtsPluginDirectory.Enabled = false;
            this.btnOpenAtsPluginDirectory.Location = new System.Drawing.Point(527, 67);
            this.btnOpenAtsPluginDirectory.Name = "btnOpenAtsPluginDirectory";
            this.btnOpenAtsPluginDirectory.Size = new System.Drawing.Size(113, 50);
            this.btnOpenAtsPluginDirectory.TabIndex = 8;
            this.btnOpenAtsPluginDirectory.Text = "ATSプラグイン設定\r\nファイルの場所を開く";
            this.btnOpenAtsPluginDirectory.UseVisualStyleBackColor = true;
            this.btnOpenAtsPluginDirectory.Click += new System.EventHandler(this.btnOpenAtsPluginDirectory_Click);
            // 
            // btnOpenVehicleDirectory
            // 
            this.btnOpenVehicleDirectory.Enabled = false;
            this.btnOpenVehicleDirectory.Location = new System.Drawing.Point(412, 67);
            this.btnOpenVehicleDirectory.Name = "btnOpenVehicleDirectory";
            this.btnOpenVehicleDirectory.Size = new System.Drawing.Size(107, 49);
            this.btnOpenVehicleDirectory.TabIndex = 9;
            this.btnOpenVehicleDirectory.Text = "車両ファイルの\r\n場所を開く";
            this.btnOpenVehicleDirectory.UseVisualStyleBackColor = true;
            this.btnOpenVehicleDirectory.Click += new System.EventHandler(this.btnOpenVehicleDirectory_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(158, 12);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(110, 110);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox1.TabIndex = 10;
            this.pictureBox1.TabStop = false;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(8, 6);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(882, 412);
            this.textBox1.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(737, 49);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 12);
            this.label1.TabIndex = 12;
            this.label1.Text = "↑まだできてません^^;";
            // 
            // btnAtsPluginDirectory
            // 
            this.btnAtsPluginDirectory.Location = new System.Drawing.Point(24, 11);
            this.btnAtsPluginDirectory.Name = "btnAtsPluginDirectory";
            this.btnAtsPluginDirectory.Size = new System.Drawing.Size(101, 55);
            this.btnAtsPluginDirectory.TabIndex = 13;
            this.btnAtsPluginDirectory.Text = "1.ATSプラグイン\r\nの場所を指定\r\n";
            this.btnAtsPluginDirectory.UseVisualStyleBackColor = true;
            this.btnAtsPluginDirectory.Click += new System.EventHandler(this.btnAtsPluginDirectory_Click);
            // 
            // lblAtsPluginFileName
            // 
            this.lblAtsPluginFileName.AutoSize = true;
            this.lblAtsPluginFileName.Location = new System.Drawing.Point(22, 222);
            this.lblAtsPluginFileName.Name = "lblAtsPluginFileName";
            this.lblAtsPluginFileName.Size = new System.Drawing.Size(89, 12);
            this.lblAtsPluginFileName.TabIndex = 14;
            this.lblAtsPluginFileName.Text = "Target:AtsPlugin";
            // 
            // btnGenerateRelativePath
            // 
            this.btnGenerateRelativePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnGenerateRelativePath.Enabled = false;
            this.btnGenerateRelativePath.Location = new System.Drawing.Point(698, 67);
            this.btnGenerateRelativePath.Name = "btnGenerateRelativePath";
            this.btnGenerateRelativePath.Size = new System.Drawing.Size(169, 49);
            this.btnGenerateRelativePath.TabIndex = 15;
            this.btnGenerateRelativePath.Text = "4.ATSプラグインパスを生成";
            this.btnGenerateRelativePath.UseVisualStyleBackColor = false;
            this.btnGenerateRelativePath.Click += new System.EventHandler(this.btnGenerateRelativePath_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.Location = new System.Drawing.Point(698, 122);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(86, 24);
            this.btnHelp.TabIndex = 16;
            this.btnHelp.Text = "ヘルプ";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tpVehicle);
            this.tabControl1.Controls.Add(this.tpRoute);
            this.tabControl1.Controls.Add(this.tpLog);
            this.tabControl1.Location = new System.Drawing.Point(12, 264);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(904, 411);
            this.tabControl1.TabIndex = 18;
            // 
            // tpVehicle
            // 
            this.tpVehicle.Controls.Add(this.cbxVehicle);
            this.tpVehicle.Controls.Add(this.label10);
            this.tpVehicle.Controls.Add(this.label9);
            this.tpVehicle.Controls.Add(this.label8);
            this.tpVehicle.Controls.Add(this.label7);
            this.tpVehicle.Controls.Add(this.label6);
            this.tpVehicle.Controls.Add(this.label5);
            this.tpVehicle.Controls.Add(this.label11);
            this.tpVehicle.Controls.Add(this.label4);
            this.tpVehicle.Controls.Add(this.tbAts64);
            this.tpVehicle.Controls.Add(this.tbAts32);
            this.tpVehicle.Controls.Add(this.tbMotorNoise);
            this.tpVehicle.Controls.Add(this.tbSound);
            this.tpVehicle.Controls.Add(this.tbPanel);
            this.tpVehicle.Controls.Add(this.btnAts64Open);
            this.tpVehicle.Controls.Add(this.btnAts32Open);
            this.tpVehicle.Controls.Add(this.btnMotorNoiseOpen);
            this.tpVehicle.Controls.Add(this.btnSoundOpen);
            this.tpVehicle.Controls.Add(this.btnPanelOpen);
            this.tpVehicle.Controls.Add(this.tbParameters);
            this.tpVehicle.Controls.Add(this.btnParametersOpen);
            this.tpVehicle.Controls.Add(this.btnVehicleOpen);
            this.tpVehicle.Controls.Add(this.tbPerfoemanceCurve);
            this.tpVehicle.Controls.Add(this.btnPerfoemanceCurveOpen);
            this.tpVehicle.Location = new System.Drawing.Point(4, 22);
            this.tpVehicle.Name = "tpVehicle";
            this.tpVehicle.Padding = new System.Windows.Forms.Padding(3);
            this.tpVehicle.Size = new System.Drawing.Size(896, 385);
            this.tpVehicle.TabIndex = 1;
            this.tpVehicle.Text = "車両ファイル関連";
            this.tpVehicle.UseVisualStyleBackColor = true;
            // 
            // cbxVehicle
            // 
            this.cbxVehicle.FormattingEnabled = true;
            this.cbxVehicle.Location = new System.Drawing.Point(247, 47);
            this.cbxVehicle.Name = "cbxVehicle";
            this.cbxVehicle.Size = new System.Drawing.Size(626, 20);
            this.cbxVehicle.TabIndex = 3;
            this.cbxVehicle.SelectionChangeCommitted += new System.EventHandler(this.cbxVehicle_SelectionChangeCommitted);
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(19, 350);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(35, 12);
            this.label10.TabIndex = 2;
            this.label10.Text = "Ats64";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(19, 313);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(71, 12);
            this.label9.TabIndex = 2;
            this.label9.Text = "Ats or Ats32";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(19, 276);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(120, 12);
            this.label8.TabIndex = 2;
            this.label8.Text = "モーター音(MotorNoise)";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(19, 239);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(81, 12);
            this.label7.TabIndex = 2;
            this.label7.Text = "サウンド(Sound)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(19, 202);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 12);
            this.label6.TabIndex = 2;
            this.label6.Text = "パネル(Panel)";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 165);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(115, 12);
            this.label5.TabIndex = 2;
            this.label5.Text = "パラメータ(Parameters)";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(19, 50);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(109, 12);
            this.label11.TabIndex = 2;
            this.label11.Text = "車両ファイル(Vehicle)";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(19, 128);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(155, 12);
            this.label4.TabIndex = 2;
            this.label4.Text = "性能曲線(PerformanceCurve)";
            // 
            // tbAts64
            // 
            this.tbAts64.Location = new System.Drawing.Point(247, 347);
            this.tbAts64.Name = "tbAts64";
            this.tbAts64.Size = new System.Drawing.Size(626, 19);
            this.tbAts64.TabIndex = 1;
            // 
            // tbAts32
            // 
            this.tbAts32.Location = new System.Drawing.Point(247, 310);
            this.tbAts32.Name = "tbAts32";
            this.tbAts32.Size = new System.Drawing.Size(626, 19);
            this.tbAts32.TabIndex = 1;
            // 
            // tbMotorNoise
            // 
            this.tbMotorNoise.Location = new System.Drawing.Point(247, 273);
            this.tbMotorNoise.Name = "tbMotorNoise";
            this.tbMotorNoise.Size = new System.Drawing.Size(626, 19);
            this.tbMotorNoise.TabIndex = 1;
            // 
            // tbSound
            // 
            this.tbSound.Location = new System.Drawing.Point(247, 236);
            this.tbSound.Name = "tbSound";
            this.tbSound.Size = new System.Drawing.Size(626, 19);
            this.tbSound.TabIndex = 1;
            // 
            // tbPanel
            // 
            this.tbPanel.Location = new System.Drawing.Point(247, 199);
            this.tbPanel.Name = "tbPanel";
            this.tbPanel.Size = new System.Drawing.Size(626, 19);
            this.tbPanel.TabIndex = 1;
            // 
            // btnAts64Open
            // 
            this.btnAts64Open.Enabled = false;
            this.btnAts64Open.Location = new System.Drawing.Point(187, 343);
            this.btnAts64Open.Name = "btnAts64Open";
            this.btnAts64Open.Size = new System.Drawing.Size(54, 26);
            this.btnAts64Open.TabIndex = 0;
            this.btnAts64Open.Text = "開く";
            this.btnAts64Open.UseVisualStyleBackColor = true;
            this.btnAts64Open.Visible = false;
            this.btnAts64Open.Click += new System.EventHandler(this.btnAts64Open_Click);
            // 
            // btnAts32Open
            // 
            this.btnAts32Open.Enabled = false;
            this.btnAts32Open.Location = new System.Drawing.Point(187, 306);
            this.btnAts32Open.Name = "btnAts32Open";
            this.btnAts32Open.Size = new System.Drawing.Size(54, 26);
            this.btnAts32Open.TabIndex = 0;
            this.btnAts32Open.Text = "開く";
            this.btnAts32Open.UseVisualStyleBackColor = true;
            this.btnAts32Open.Visible = false;
            this.btnAts32Open.Click += new System.EventHandler(this.btnOpenAtsPluginFile_Click);
            // 
            // btnMotorNoiseOpen
            // 
            this.btnMotorNoiseOpen.Enabled = false;
            this.btnMotorNoiseOpen.Location = new System.Drawing.Point(187, 269);
            this.btnMotorNoiseOpen.Name = "btnMotorNoiseOpen";
            this.btnMotorNoiseOpen.Size = new System.Drawing.Size(54, 26);
            this.btnMotorNoiseOpen.TabIndex = 0;
            this.btnMotorNoiseOpen.Text = "開く";
            this.btnMotorNoiseOpen.UseVisualStyleBackColor = true;
            this.btnMotorNoiseOpen.Click += new System.EventHandler(this.btnMotorNoiseOpen_Click);
            // 
            // btnSoundOpen
            // 
            this.btnSoundOpen.Enabled = false;
            this.btnSoundOpen.Location = new System.Drawing.Point(187, 232);
            this.btnSoundOpen.Name = "btnSoundOpen";
            this.btnSoundOpen.Size = new System.Drawing.Size(54, 26);
            this.btnSoundOpen.TabIndex = 0;
            this.btnSoundOpen.Text = "開く";
            this.btnSoundOpen.UseVisualStyleBackColor = true;
            this.btnSoundOpen.Click += new System.EventHandler(this.btnSoundOpen_Click);
            // 
            // btnPanelOpen
            // 
            this.btnPanelOpen.Enabled = false;
            this.btnPanelOpen.Location = new System.Drawing.Point(187, 195);
            this.btnPanelOpen.Name = "btnPanelOpen";
            this.btnPanelOpen.Size = new System.Drawing.Size(54, 26);
            this.btnPanelOpen.TabIndex = 0;
            this.btnPanelOpen.Text = "開く";
            this.btnPanelOpen.UseVisualStyleBackColor = true;
            this.btnPanelOpen.Click += new System.EventHandler(this.btnPanelOpen_Click);
            // 
            // tbParameters
            // 
            this.tbParameters.Location = new System.Drawing.Point(247, 162);
            this.tbParameters.Name = "tbParameters";
            this.tbParameters.Size = new System.Drawing.Size(626, 19);
            this.tbParameters.TabIndex = 1;
            // 
            // btnParametersOpen
            // 
            this.btnParametersOpen.Enabled = false;
            this.btnParametersOpen.Location = new System.Drawing.Point(187, 158);
            this.btnParametersOpen.Name = "btnParametersOpen";
            this.btnParametersOpen.Size = new System.Drawing.Size(54, 26);
            this.btnParametersOpen.TabIndex = 0;
            this.btnParametersOpen.Text = "開く";
            this.btnParametersOpen.UseVisualStyleBackColor = true;
            this.btnParametersOpen.Click += new System.EventHandler(this.btnParametersOpen_Click);
            // 
            // btnVehicleOpen
            // 
            this.btnVehicleOpen.Enabled = false;
            this.btnVehicleOpen.Location = new System.Drawing.Point(187, 43);
            this.btnVehicleOpen.Name = "btnVehicleOpen";
            this.btnVehicleOpen.Size = new System.Drawing.Size(54, 26);
            this.btnVehicleOpen.TabIndex = 0;
            this.btnVehicleOpen.Text = "開く";
            this.btnVehicleOpen.UseVisualStyleBackColor = true;
            this.btnVehicleOpen.Click += new System.EventHandler(this.btnOpenVehicleFile_Click);
            // 
            // tbPerfoemanceCurve
            // 
            this.tbPerfoemanceCurve.Location = new System.Drawing.Point(247, 125);
            this.tbPerfoemanceCurve.Name = "tbPerfoemanceCurve";
            this.tbPerfoemanceCurve.Size = new System.Drawing.Size(626, 19);
            this.tbPerfoemanceCurve.TabIndex = 1;
            // 
            // btnPerfoemanceCurveOpen
            // 
            this.btnPerfoemanceCurveOpen.Enabled = false;
            this.btnPerfoemanceCurveOpen.Location = new System.Drawing.Point(187, 121);
            this.btnPerfoemanceCurveOpen.Name = "btnPerfoemanceCurveOpen";
            this.btnPerfoemanceCurveOpen.Size = new System.Drawing.Size(54, 26);
            this.btnPerfoemanceCurveOpen.TabIndex = 0;
            this.btnPerfoemanceCurveOpen.Text = "開く";
            this.btnPerfoemanceCurveOpen.UseVisualStyleBackColor = true;
            this.btnPerfoemanceCurveOpen.Click += new System.EventHandler(this.btnPerfoemanceCurveOpen_Click);
            // 
            // tpRoute
            // 
            this.tpRoute.Controls.Add(this.label16);
            this.tpRoute.Controls.Add(this.label15);
            this.tpRoute.Controls.Add(this.label13);
            this.tpRoute.Controls.Add(this.label14);
            this.tpRoute.Controls.Add(this.label12);
            this.tpRoute.Controls.Add(this.label3);
            this.tpRoute.Controls.Add(this.tbTrain);
            this.tpRoute.Controls.Add(this.tbSoundList);
            this.tpRoute.Controls.Add(this.btnTrainOpen);
            this.tpRoute.Controls.Add(this.tbStation);
            this.tpRoute.Controls.Add(this.btnSoundListOpen);
            this.tpRoute.Controls.Add(this.btnStationOpen);
            this.tpRoute.Controls.Add(this.tbSignal);
            this.tpRoute.Controls.Add(this.btnSignalOpen);
            this.tpRoute.Controls.Add(this.tbStructure);
            this.tpRoute.Controls.Add(this.btnStructureOpen);
            this.tpRoute.Controls.Add(this.tbMapFilePath);
            this.tpRoute.Controls.Add(this.btnMapOpen);
            this.tpRoute.Location = new System.Drawing.Point(4, 22);
            this.tpRoute.Name = "tpRoute";
            this.tpRoute.Size = new System.Drawing.Size(896, 385);
            this.tpRoute.TabIndex = 2;
            this.tpRoute.Text = "マップファイル関連";
            this.tpRoute.UseVisualStyleBackColor = true;
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(19, 276);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(109, 12);
            this.label16.TabIndex = 5;
            this.label16.Text = "他列車ファイル(Train)";
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(19, 239);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(139, 12);
            this.label15.TabIndex = 5;
            this.label15.Text = "サウンドリストファイル(Sound)";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(19, 165);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(119, 12);
            this.label13.TabIndex = 5;
            this.label13.Text = "停車場ファイル(Staiton)";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(19, 202);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(104, 12);
            this.label14.TabIndex = 5;
            this.label14.Text = "地上信号機(Signal)";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(19, 128);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(144, 12);
            this.label12.TabIndex = 5;
            this.label12.Text = "ストラクチャファイル(Structure)";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(19, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(102, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "マップファイル(Route)";
            // 
            // tbTrain
            // 
            this.tbTrain.Location = new System.Drawing.Point(247, 273);
            this.tbTrain.Name = "tbTrain";
            this.tbTrain.Size = new System.Drawing.Size(626, 19);
            this.tbTrain.TabIndex = 4;
            // 
            // tbSoundList
            // 
            this.tbSoundList.Location = new System.Drawing.Point(247, 236);
            this.tbSoundList.Name = "tbSoundList";
            this.tbSoundList.Size = new System.Drawing.Size(626, 19);
            this.tbSoundList.TabIndex = 4;
            // 
            // btnTrainOpen
            // 
            this.btnTrainOpen.Enabled = false;
            this.btnTrainOpen.Location = new System.Drawing.Point(187, 269);
            this.btnTrainOpen.Name = "btnTrainOpen";
            this.btnTrainOpen.Size = new System.Drawing.Size(54, 26);
            this.btnTrainOpen.TabIndex = 3;
            this.btnTrainOpen.Text = "開く";
            this.btnTrainOpen.UseVisualStyleBackColor = true;
            this.btnTrainOpen.Click += new System.EventHandler(this.btnTrainOpen_Click);
            // 
            // tbStation
            // 
            this.tbStation.Location = new System.Drawing.Point(247, 162);
            this.tbStation.Name = "tbStation";
            this.tbStation.Size = new System.Drawing.Size(626, 19);
            this.tbStation.TabIndex = 4;
            // 
            // btnSoundListOpen
            // 
            this.btnSoundListOpen.Enabled = false;
            this.btnSoundListOpen.Location = new System.Drawing.Point(187, 232);
            this.btnSoundListOpen.Name = "btnSoundListOpen";
            this.btnSoundListOpen.Size = new System.Drawing.Size(54, 26);
            this.btnSoundListOpen.TabIndex = 3;
            this.btnSoundListOpen.Text = "開く";
            this.btnSoundListOpen.UseVisualStyleBackColor = true;
            this.btnSoundListOpen.Click += new System.EventHandler(this.btnSoundListOpen_Click);
            // 
            // btnStationOpen
            // 
            this.btnStationOpen.Enabled = false;
            this.btnStationOpen.Location = new System.Drawing.Point(187, 158);
            this.btnStationOpen.Name = "btnStationOpen";
            this.btnStationOpen.Size = new System.Drawing.Size(54, 26);
            this.btnStationOpen.TabIndex = 3;
            this.btnStationOpen.Text = "開く";
            this.btnStationOpen.UseVisualStyleBackColor = true;
            this.btnStationOpen.Click += new System.EventHandler(this.btnStationOpen_Click);
            // 
            // tbSignal
            // 
            this.tbSignal.Location = new System.Drawing.Point(247, 199);
            this.tbSignal.Name = "tbSignal";
            this.tbSignal.Size = new System.Drawing.Size(626, 19);
            this.tbSignal.TabIndex = 4;
            // 
            // btnSignalOpen
            // 
            this.btnSignalOpen.Enabled = false;
            this.btnSignalOpen.Location = new System.Drawing.Point(187, 195);
            this.btnSignalOpen.Name = "btnSignalOpen";
            this.btnSignalOpen.Size = new System.Drawing.Size(54, 26);
            this.btnSignalOpen.TabIndex = 3;
            this.btnSignalOpen.Text = "開く";
            this.btnSignalOpen.UseVisualStyleBackColor = true;
            this.btnSignalOpen.Click += new System.EventHandler(this.btnSignalOpen_Click);
            // 
            // tbStructure
            // 
            this.tbStructure.Location = new System.Drawing.Point(247, 125);
            this.tbStructure.Name = "tbStructure";
            this.tbStructure.Size = new System.Drawing.Size(626, 19);
            this.tbStructure.TabIndex = 4;
            // 
            // btnStructureOpen
            // 
            this.btnStructureOpen.Enabled = false;
            this.btnStructureOpen.Location = new System.Drawing.Point(187, 121);
            this.btnStructureOpen.Name = "btnStructureOpen";
            this.btnStructureOpen.Size = new System.Drawing.Size(54, 26);
            this.btnStructureOpen.TabIndex = 3;
            this.btnStructureOpen.Text = "開く";
            this.btnStructureOpen.UseVisualStyleBackColor = true;
            this.btnStructureOpen.Click += new System.EventHandler(this.btnStructureOpen_Click);
            // 
            // tbMapFilePath
            // 
            this.tbMapFilePath.Location = new System.Drawing.Point(247, 47);
            this.tbMapFilePath.Name = "tbMapFilePath";
            this.tbMapFilePath.Size = new System.Drawing.Size(626, 19);
            this.tbMapFilePath.TabIndex = 4;
            // 
            // btnMapOpen
            // 
            this.btnMapOpen.Enabled = false;
            this.btnMapOpen.Location = new System.Drawing.Point(187, 43);
            this.btnMapOpen.Name = "btnMapOpen";
            this.btnMapOpen.Size = new System.Drawing.Size(54, 26);
            this.btnMapOpen.TabIndex = 3;
            this.btnMapOpen.Text = "開く";
            this.btnMapOpen.UseVisualStyleBackColor = true;
            this.btnMapOpen.Click += new System.EventHandler(this.btnMapOpen_Click_1);
            // 
            // tpLog
            // 
            this.tpLog.Controls.Add(this.textBox1);
            this.tpLog.Location = new System.Drawing.Point(4, 22);
            this.tpLog.Name = "tpLog";
            this.tpLog.Padding = new System.Windows.Forms.Padding(3);
            this.tpLog.Size = new System.Drawing.Size(896, 385);
            this.tpLog.TabIndex = 0;
            this.tpLog.Text = "読込ログ";
            this.tpLog.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 676);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(920, 22);
            this.statusStrip1.TabIndex = 19;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(265, 17);
            this.toolStripStatusLabel1.Text = "Copyright ©  2022 ぐらふ(Twitter@GraphTechKEN)";
            // 
            // btnBootBVE
            // 
            this.btnBootBVE.Enabled = false;
            this.btnBootBVE.Location = new System.Drawing.Point(297, 67);
            this.btnBootBVE.Name = "btnBootBVE";
            this.btnBootBVE.Size = new System.Drawing.Size(109, 49);
            this.btnBootBVE.TabIndex = 20;
            this.btnBootBVE.Text = "BVEを起動";
            this.btnBootBVE.UseVisualStyleBackColor = true;
            this.btnBootBVE.Click += new System.EventHandler(this.btnBootBVE_Click);
            // 
            // lblSeinarioFileName
            // 
            this.lblSeinarioFileName.AutoSize = true;
            this.lblSeinarioFileName.Location = new System.Drawing.Point(22, 240);
            this.lblSeinarioFileName.Name = "lblSeinarioFileName";
            this.lblSeinarioFileName.Size = new System.Drawing.Size(81, 12);
            this.lblSeinarioFileName.TabIndex = 14;
            this.lblSeinarioFileName.Text = "Target:Seinario";
            // 
            // cbMessageDisp
            // 
            this.cbMessageDisp.AutoSize = true;
            this.cbMessageDisp.Checked = true;
            this.cbMessageDisp.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbMessageDisp.Location = new System.Drawing.Point(790, 127);
            this.cbMessageDisp.Name = "cbMessageDisp";
            this.cbMessageDisp.Size = new System.Drawing.Size(93, 16);
            this.cbMessageDisp.TabIndex = 21;
            this.cbMessageDisp.Text = "メッセージ表示";
            this.cbMessageDisp.UseVisualStyleBackColor = true;
            this.cbMessageDisp.CheckedChanged += new System.EventHandler(this.cbMessage_CheckedChanged);
            // 
            // btnBveBootChooseVehicle
            // 
            this.btnBveBootChooseVehicle.Enabled = false;
            this.btnBveBootChooseVehicle.Location = new System.Drawing.Point(297, 122);
            this.btnBveBootChooseVehicle.Name = "btnBveBootChooseVehicle";
            this.btnBveBootChooseVehicle.Size = new System.Drawing.Size(109, 49);
            this.btnBveBootChooseVehicle.TabIndex = 20;
            this.btnBveBootChooseVehicle.Text = "選択車両で\r\nBVEを起動";
            this.btnBveBootChooseVehicle.UseVisualStyleBackColor = true;
            this.btnBveBootChooseVehicle.Click += new System.EventHandler(this.btnBveBootChooseVehicle_Click);
            // 
            // lblVehicleTitle
            // 
            this.lblVehicleTitle.AutoSize = true;
            this.lblVehicleTitle.Location = new System.Drawing.Point(32, 168);
            this.lblVehicleTitle.Name = "lblVehicleTitle";
            this.lblVehicleTitle.Size = new System.Drawing.Size(66, 12);
            this.lblVehicleTitle.TabIndex = 22;
            this.lblVehicleTitle.Text = "VehicleTitle";
            // 
            // lblRouteTitle
            // 
            this.lblRouteTitle.AutoSize = true;
            this.lblRouteTitle.Location = new System.Drawing.Point(32, 154);
            this.lblRouteTitle.Name = "lblRouteTitle";
            this.lblRouteTitle.Size = new System.Drawing.Size(62, 12);
            this.lblRouteTitle.TabIndex = 23;
            this.lblRouteTitle.Text = "Route Title";
            // 
            // lblAuthor
            // 
            this.lblAuthor.AutoSize = true;
            this.lblAuthor.Location = new System.Drawing.Point(32, 182);
            this.lblAuthor.Name = "lblAuthor";
            this.lblAuthor.Size = new System.Drawing.Size(39, 12);
            this.lblAuthor.TabIndex = 23;
            this.lblAuthor.Text = "Author";
            // 
            // lblTitle
            // 
            this.lblTitle.AutoSize = true;
            this.lblTitle.Location = new System.Drawing.Point(32, 140);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(28, 12);
            this.lblTitle.TabIndex = 23;
            this.lblTitle.Text = "Title";
            // 
            // lblComment
            // 
            this.lblComment.AutoSize = true;
            this.lblComment.Location = new System.Drawing.Point(33, 196);
            this.lblComment.Name = "lblComment";
            this.lblComment.Size = new System.Drawing.Size(53, 12);
            this.lblComment.TabIndex = 22;
            this.lblComment.Text = "Comment";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(920, 698);
            this.Controls.Add(this.lblAuthor);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.lblRouteTitle);
            this.Controls.Add(this.lblComment);
            this.Controls.Add(this.lblVehicleTitle);
            this.Controls.Add(this.cbMessageDisp);
            this.Controls.Add(this.btnBveBootChooseVehicle);
            this.Controls.Add(this.btnBootBVE);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnGenerateRelativePath);
            this.Controls.Add(this.lblSeinarioFileName);
            this.Controls.Add(this.lblAtsPluginFileName);
            this.Controls.Add(this.btnAtsPluginDirectory);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.btnOpenVehicleDirectory);
            this.Controls.Add(this.btnOpenAtsPluginDirectory);
            this.Controls.Add(this.btnOpenAtsPluginFile);
            this.Controls.Add(this.btnOpenVehicleFile);
            this.Controls.Add(this.btnOpenRouteFile);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Name = "Form1";
            this.Text = "BVE File Explorer ( Beta ver 0.9.4.1 )";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tpVehicle.ResumeLayout(false);
            this.tpVehicle.PerformLayout();
            this.tpRoute.ResumeLayout(false);
            this.tpRoute.PerformLayout();
            this.tpLog.ResumeLayout(false);
            this.tpLog.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.Button btnOpenRouteFile;
        private System.Windows.Forms.Button btnOpenVehicleFile;
        private System.Windows.Forms.Button btnOpenAtsPluginFile;
        private System.Windows.Forms.Button btnOpenAtsPluginDirectory;
        private System.Windows.Forms.Button btnOpenVehicleDirectory;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAtsPluginDirectory;
        private System.Windows.Forms.Label lblAtsPluginFileName;
        private System.Windows.Forms.Button btnGenerateRelativePath;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tpLog;
        private System.Windows.Forms.TabPage tpVehicle;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbAts64;
        private System.Windows.Forms.TextBox tbAts32;
        private System.Windows.Forms.TextBox tbMotorNoise;
        private System.Windows.Forms.TextBox tbSound;
        private System.Windows.Forms.TextBox tbPanel;
        private System.Windows.Forms.Button btnAts64Open;
        private System.Windows.Forms.Button btnAts32Open;
        private System.Windows.Forms.Button btnMotorNoiseOpen;
        private System.Windows.Forms.Button btnSoundOpen;
        private System.Windows.Forms.Button btnPanelOpen;
        private System.Windows.Forms.TextBox tbParameters;
        private System.Windows.Forms.Button btnParametersOpen;
        private System.Windows.Forms.TextBox tbPerfoemanceCurve;
        private System.Windows.Forms.Button btnPerfoemanceCurveOpen;
        private System.Windows.Forms.TabPage tpRoute;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox tbMapFilePath;
        private System.Windows.Forms.Button btnMapOpen;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Button btnVehicleOpen;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox tbStructure;
        private System.Windows.Forms.Button btnStructureOpen;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox tbStation;
        private System.Windows.Forms.Button btnStationOpen;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox tbSoundList;
        private System.Windows.Forms.Button btnSoundListOpen;
        private System.Windows.Forms.TextBox tbSignal;
        private System.Windows.Forms.Button btnSignalOpen;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.TextBox tbTrain;
        private System.Windows.Forms.Button btnTrainOpen;
        private System.Windows.Forms.ComboBox cbxVehicle;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.Button btnBootBVE;
        private System.Windows.Forms.Label lblSeinarioFileName;
        private System.Windows.Forms.CheckBox cbMessageDisp;
        private System.Windows.Forms.Button btnBveBootChooseVehicle;
        private System.Windows.Forms.Label lblVehicleTitle;
        private System.Windows.Forms.Label lblRouteTitle;
        private System.Windows.Forms.Label lblAuthor;
        private System.Windows.Forms.Label lblTitle;
        private System.Windows.Forms.Label lblComment;
    }
}

