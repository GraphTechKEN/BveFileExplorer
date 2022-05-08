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
            this.label2 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Enabled = false;
            this.button1.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.button1.Location = new System.Drawing.Point(659, 11);
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
            this.button2.Location = new System.Drawing.Point(751, 11);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(77, 50);
            this.button2.TabIndex = 0;
            this.button2.Text = "自動解除";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // btnOpen
            // 
            this.btnOpen.Enabled = false;
            this.btnOpen.Font = new System.Drawing.Font("MS UI Gothic", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            this.btnOpen.Location = new System.Drawing.Point(24, 72);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(101, 50);
            this.btnOpen.TabIndex = 4;
            this.btnOpen.Text = "2.路線ファイルを\r\n指定";
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.btnOpen_Click);
            // 
            // btnOpenRouteFile
            // 
            this.btnOpenRouteFile.Enabled = false;
            this.btnOpenRouteFile.Location = new System.Drawing.Point(297, 11);
            this.btnOpenRouteFile.Name = "btnOpenRouteFile";
            this.btnOpenRouteFile.Size = new System.Drawing.Size(109, 50);
            this.btnOpenRouteFile.TabIndex = 5;
            this.btnOpenRouteFile.Text = "路線ファイルを開く";
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
            this.btnOpenAtsPluginFile.Size = new System.Drawing.Size(104, 50);
            this.btnOpenAtsPluginFile.TabIndex = 7;
            this.btnOpenAtsPluginFile.Text = "3.ATSプラグイン\r\nファイルを開く";
            this.btnOpenAtsPluginFile.UseVisualStyleBackColor = false;
            this.btnOpenAtsPluginFile.Click += new System.EventHandler(this.btnOpenAtsPluginFile_Click);
            // 
            // btnOpenAtsPluginDirectory
            // 
            this.btnOpenAtsPluginDirectory.Enabled = false;
            this.btnOpenAtsPluginDirectory.Location = new System.Drawing.Point(527, 67);
            this.btnOpenAtsPluginDirectory.Name = "btnOpenAtsPluginDirectory";
            this.btnOpenAtsPluginDirectory.Size = new System.Drawing.Size(104, 50);
            this.btnOpenAtsPluginDirectory.TabIndex = 8;
            this.btnOpenAtsPluginDirectory.Text = "ATSプラグインの\r\n場所を開く";
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
            this.pictureBox1.Location = new System.Drawing.Point(157, 17);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(100, 100);
            this.pictureBox1.TabIndex = 10;
            this.pictureBox1.TabStop = false;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(12, 152);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(816, 469);
            this.textBox1.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(698, 49);
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
            this.lblAtsPluginFileName.Location = new System.Drawing.Point(22, 133);
            this.lblAtsPluginFileName.Name = "lblAtsPluginFileName";
            this.lblAtsPluginFileName.Size = new System.Drawing.Size(89, 12);
            this.lblAtsPluginFileName.TabIndex = 14;
            this.lblAtsPluginFileName.Text = "Target:AtsPlugin";
            // 
            // btnGenerateRelativePath
            // 
            this.btnGenerateRelativePath.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.btnGenerateRelativePath.Enabled = false;
            this.btnGenerateRelativePath.Location = new System.Drawing.Point(659, 67);
            this.btnGenerateRelativePath.Name = "btnGenerateRelativePath";
            this.btnGenerateRelativePath.Size = new System.Drawing.Size(169, 49);
            this.btnGenerateRelativePath.TabIndex = 15;
            this.btnGenerateRelativePath.Text = "4.ATSプラグインパスを生成";
            this.btnGenerateRelativePath.UseVisualStyleBackColor = false;
            this.btnGenerateRelativePath.Click += new System.EventHandler(this.btnGenerateRelativePath_Click);
            // 
            // btnHelp
            // 
            this.btnHelp.Location = new System.Drawing.Point(700, 122);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(86, 24);
            this.btnHelp.TabIndex = 16;
            this.btnHelp.Text = "ヘルプ";
            this.btnHelp.UseVisualStyleBackColor = true;
            this.btnHelp.Click += new System.EventHandler(this.btnHelp_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(605, 624);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(223, 12);
            this.label2.TabIndex = 17;
            this.label2.Text = "Copyright ©  2022 Twitter@GraphTechKEN";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(840, 644);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnHelp);
            this.Controls.Add(this.btnGenerateRelativePath);
            this.Controls.Add(this.lblAtsPluginFileName);
            this.Controls.Add(this.btnAtsPluginDirectory);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox1);
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
            this.Text = "AtsPluginEditor ( Beta ver 0.9.0.1 )";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
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
        private System.Windows.Forms.Label label2;
    }
}

