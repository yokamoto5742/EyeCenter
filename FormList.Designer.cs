namespace EyeCenter
{
    partial class FormList
    {
        /// <summary>
        /// 必要なデザイナ変数です。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 使用中のリソースをすべてクリーンアップします。
        /// </summary>
        /// <param name="disposing">マネージ リソースが破棄される場合 true、破棄されない場合は false です。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows フォーム デザイナで生成されたコード

        /// <summary>
        /// デザイナ サポートに必要なメソッドです。このメソッドの内容を
        /// コード エディタで変更しないでください。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormList));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.FileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileCloseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolOpeRsvMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolPatMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolOpeFindMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EyeDateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.EyeListView1 = new System.Windows.Forms.DataGridView();
            this.DeptBox1 = new System.Windows.Forms.ComboBox();
            this.ShowEndBox1 = new System.Windows.Forms.CheckBox();
            this.EyeListShowButton1 = new System.Windows.Forms.Button();
            this.LoginChangeButton = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.EyeListTabControl = new System.Windows.Forms.TabControl();
            this.EyeListPage1 = new System.Windows.Forms.TabPage();
            this.label7 = new System.Windows.Forms.Label();
            this.FilterBox1 = new System.Windows.Forms.TextBox();
            this.EyeListPage2 = new System.Windows.Forms.TabPage();
            this.label8 = new System.Windows.Forms.Label();
            this.FilterBox2 = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.WardBox2 = new System.Windows.Forms.ComboBox();
            this.EyeListView2 = new System.Windows.Forms.DataGridView();
            this.EyeListContextMenu2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.WorksheetPrintMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.DeptBox2 = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.EyeListShowButton2 = new System.Windows.Forms.Button();
            this.EyeDateTimePicker2 = new System.Windows.Forms.DateTimePicker();
            this.CloseButton = new System.Windows.Forms.Button();
            this.stdControlPat11 = new MedicalLibrary.Boundary.StdControlPat1();
            this.label6 = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EyeListView1)).BeginInit();
            this.EyeListTabControl.SuspendLayout();
            this.EyeListPage1.SuspendLayout();
            this.EyeListPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EyeListView2)).BeginInit();
            this.EyeListContextMenu2.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenuItem,
            this.ToolMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1016, 26);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // FileMenuItem
            // 
            this.FileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileCloseMenuItem,
            this.FileExitMenuItem});
            this.FileMenuItem.Name = "FileMenuItem";
            this.FileMenuItem.Size = new System.Drawing.Size(68, 22);
            this.FileMenuItem.Text = "ファイル";
            // 
            // FileCloseMenuItem
            // 
            this.FileCloseMenuItem.Name = "FileCloseMenuItem";
            this.FileCloseMenuItem.Size = new System.Drawing.Size(112, 22);
            this.FileCloseMenuItem.Text = "閉じる";
            this.FileCloseMenuItem.Click += new System.EventHandler(this.FileCloseMenuItem_Click);
            // 
            // FileExitMenuItem
            // 
            this.FileExitMenuItem.Name = "FileExitMenuItem";
            this.FileExitMenuItem.Size = new System.Drawing.Size(112, 22);
            this.FileExitMenuItem.Text = "終了";
            this.FileExitMenuItem.Click += new System.EventHandler(this.FileExitMenuItem_Click);
            // 
            // ToolMenuItem
            // 
            this.ToolMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolOpeRsvMenuItem,
            this.ToolPatMenuItem,
            this.ToolOpeFindMenuItem});
            this.ToolMenuItem.Name = "ToolMenuItem";
            this.ToolMenuItem.Size = new System.Drawing.Size(56, 22);
            this.ToolMenuItem.Text = "ツール";
            // 
            // ToolOpeRsvMenuItem
            // 
            this.ToolOpeRsvMenuItem.Name = "ToolOpeRsvMenuItem";
            this.ToolOpeRsvMenuItem.Size = new System.Drawing.Size(148, 22);
            this.ToolOpeRsvMenuItem.Text = "手術予約";
            this.ToolOpeRsvMenuItem.Click += new System.EventHandler(this.ToolOpeRsvMenuItem_Click);
            // 
            // ToolPatMenuItem
            // 
            this.ToolPatMenuItem.Name = "ToolPatMenuItem";
            this.ToolPatMenuItem.Size = new System.Drawing.Size(148, 22);
            this.ToolPatMenuItem.Text = "患者台帳";
            this.ToolPatMenuItem.Click += new System.EventHandler(this.ToolPatMenuItem_Click);
            // 
            // ToolOpeFindMenuItem
            // 
            this.ToolOpeFindMenuItem.Name = "ToolOpeFindMenuItem";
            this.ToolOpeFindMenuItem.Size = new System.Drawing.Size(148, 22);
            this.ToolOpeFindMenuItem.Text = "手術記録検索";
            this.ToolOpeFindMenuItem.Click += new System.EventHandler(this.ToolOpeFindMenuItem_Click);
            // 
            // EyeDateTimePicker1
            // 
            this.EyeDateTimePicker1.Location = new System.Drawing.Point(43, 10);
            this.EyeDateTimePicker1.MaxDate = new System.DateTime(2999, 12, 31, 0, 0, 0, 0);
            this.EyeDateTimePicker1.MinDate = new System.DateTime(2007, 1, 1, 0, 0, 0, 0);
            this.EyeDateTimePicker1.Name = "EyeDateTimePicker1";
            this.EyeDateTimePicker1.Size = new System.Drawing.Size(109, 19);
            this.EyeDateTimePicker1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.Color.Black;
            this.label1.Location = new System.Drawing.Point(8, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "日付";
            // 
            // EyeListView1
            // 
            this.EyeListView1.AllowUserToAddRows = false;
            this.EyeListView1.AllowUserToDeleteRows = false;
            this.EyeListView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EyeListView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.EyeListView1.Location = new System.Drawing.Point(5, 37);
            this.EyeListView1.MultiSelect = false;
            this.EyeListView1.Name = "EyeListView1";
            this.EyeListView1.ReadOnly = true;
            this.EyeListView1.RowHeadersVisible = false;
            this.EyeListView1.RowTemplate.Height = 21;
            this.EyeListView1.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.EyeListView1.Size = new System.Drawing.Size(994, 635);
            this.EyeListView1.TabIndex = 3;
            this.EyeListView1.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.EyeListView1_CellDoubleClick);
            this.EyeListView1.ColumnHeaderMouseClick += new System.Windows.Forms.DataGridViewCellMouseEventHandler(this.EyeListView1_ColumnHeaderMouseClick);
            // 
            // DeptBox1
            // 
            this.DeptBox1.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DeptBox1.FormattingEnabled = true;
            this.DeptBox1.Location = new System.Drawing.Point(180, 9);
            this.DeptBox1.Name = "DeptBox1";
            this.DeptBox1.Size = new System.Drawing.Size(108, 20);
            this.DeptBox1.TabIndex = 4;
            // 
            // ShowEndBox1
            // 
            this.ShowEndBox1.AutoSize = true;
            this.ShowEndBox1.Location = new System.Drawing.Point(310, 12);
            this.ShowEndBox1.Name = "ShowEndBox1";
            this.ShowEndBox1.Size = new System.Drawing.Size(96, 16);
            this.ShowEndBox1.TabIndex = 5;
            this.ShowEndBox1.Text = "診察終了表示";
            this.ShowEndBox1.UseVisualStyleBackColor = true;
            this.ShowEndBox1.CheckedChanged += new System.EventHandler(this.ShowEndBox1_CheckedChanged);
            // 
            // EyeListShowButton1
            // 
            this.EyeListShowButton1.Location = new System.Drawing.Point(920, 8);
            this.EyeListShowButton1.Name = "EyeListShowButton1";
            this.EyeListShowButton1.Size = new System.Drawing.Size(75, 23);
            this.EyeListShowButton1.TabIndex = 6;
            this.EyeListShowButton1.Text = "更新";
            this.EyeListShowButton1.UseVisualStyleBackColor = true;
            this.EyeListShowButton1.Click += new System.EventHandler(this.EyeListShowButton1_Click);
            // 
            // LoginChangeButton
            // 
            this.LoginChangeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.LoginChangeButton.Location = new System.Drawing.Point(816, 30);
            this.LoginChangeButton.Name = "LoginChangeButton";
            this.LoginChangeButton.Size = new System.Drawing.Size(119, 23);
            this.LoginChangeButton.TabIndex = 7;
            this.LoginChangeButton.Text = "ユーザー変更 (F8)";
            this.LoginChangeButton.UseVisualStyleBackColor = true;
            this.LoginChangeButton.Click += new System.EventHandler(this.LoginChangeButton_Click);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 180000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(700, 14);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(191, 12);
            this.label2.TabIndex = 8;
            this.label2.Text = "※リストは3分おきに自動更新されます。";
            // 
            // EyeListTabControl
            // 
            this.EyeListTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EyeListTabControl.Controls.Add(this.EyeListPage1);
            this.EyeListTabControl.Controls.Add(this.EyeListPage2);
            this.EyeListTabControl.Location = new System.Drawing.Point(4, 39);
            this.EyeListTabControl.Name = "EyeListTabControl";
            this.EyeListTabControl.SelectedIndex = 0;
            this.EyeListTabControl.Size = new System.Drawing.Size(1010, 700);
            this.EyeListTabControl.TabIndex = 9;
            this.EyeListTabControl.SelectedIndexChanged += new System.EventHandler(this.EyeListTabControl_SelectedIndexChanged);
            // 
            // EyeListPage1
            // 
            this.EyeListPage1.Controls.Add(this.label7);
            this.EyeListPage1.Controls.Add(this.FilterBox1);
            this.EyeListPage1.Controls.Add(this.EyeListView1);
            this.EyeListPage1.Controls.Add(this.label2);
            this.EyeListPage1.Controls.Add(this.DeptBox1);
            this.EyeListPage1.Controls.Add(this.label1);
            this.EyeListPage1.Controls.Add(this.EyeListShowButton1);
            this.EyeListPage1.Controls.Add(this.EyeDateTimePicker1);
            this.EyeListPage1.Controls.Add(this.ShowEndBox1);
            this.EyeListPage1.Location = new System.Drawing.Point(4, 22);
            this.EyeListPage1.Name = "EyeListPage1";
            this.EyeListPage1.Padding = new System.Windows.Forms.Padding(3);
            this.EyeListPage1.Size = new System.Drawing.Size(1002, 674);
            this.EyeListPage1.TabIndex = 0;
            this.EyeListPage1.Text = "外来";
            this.EyeListPage1.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(450, 13);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(101, 12);
            this.label7.TabIndex = 10;
            this.label7.Text = "絞込み（氏名, カナ）";
            // 
            // FilterBox1
            // 
            this.FilterBox1.Location = new System.Drawing.Point(560, 10);
            this.FilterBox1.MaxLength = 18;
            this.FilterBox1.Name = "FilterBox1";
            this.FilterBox1.Size = new System.Drawing.Size(120, 19);
            this.FilterBox1.TabIndex = 9;
            this.FilterBox1.TextChanged += new System.EventHandler(this.FilterBox1_TextChanged);
            // 
            // EyeListPage2
            // 
            this.EyeListPage2.Controls.Add(this.label8);
            this.EyeListPage2.Controls.Add(this.FilterBox2);
            this.EyeListPage2.Controls.Add(this.label5);
            this.EyeListPage2.Controls.Add(this.label4);
            this.EyeListPage2.Controls.Add(this.WardBox2);
            this.EyeListPage2.Controls.Add(this.EyeListView2);
            this.EyeListPage2.Controls.Add(this.DeptBox2);
            this.EyeListPage2.Controls.Add(this.label3);
            this.EyeListPage2.Controls.Add(this.EyeListShowButton2);
            this.EyeListPage2.Controls.Add(this.EyeDateTimePicker2);
            this.EyeListPage2.Location = new System.Drawing.Point(4, 22);
            this.EyeListPage2.Name = "EyeListPage2";
            this.EyeListPage2.Padding = new System.Windows.Forms.Padding(3);
            this.EyeListPage2.Size = new System.Drawing.Size(1002, 674);
            this.EyeListPage2.TabIndex = 1;
            this.EyeListPage2.Text = "入院";
            this.EyeListPage2.UseVisualStyleBackColor = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(440, 13);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(101, 12);
            this.label8.TabIndex = 16;
            this.label8.Text = "絞込み（氏名, カナ）";
            // 
            // FilterBox2
            // 
            this.FilterBox2.Location = new System.Drawing.Point(550, 10);
            this.FilterBox2.MaxLength = 18;
            this.FilterBox2.Name = "FilterBox2";
            this.FilterBox2.Size = new System.Drawing.Size(120, 19);
            this.FilterBox2.TabIndex = 15;
            this.FilterBox2.TextChanged += new System.EventHandler(this.FilterBox2_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(165, 13);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(29, 12);
            this.label5.TabIndex = 14;
            this.label5.Text = "病棟";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(685, 14);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(210, 12);
            this.label4.TabIndex = 13;
            this.label4.Text = "※表示するには更新ボタンを押してください。";
            // 
            // WardBox2
            // 
            this.WardBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.WardBox2.FormattingEnabled = true;
            this.WardBox2.Location = new System.Drawing.Point(200, 9);
            this.WardBox2.Name = "WardBox2";
            this.WardBox2.Size = new System.Drawing.Size(93, 20);
            this.WardBox2.TabIndex = 12;
            this.WardBox2.SelectedIndexChanged += new System.EventHandler(this.WardBox2_SelectedIndexChanged);
            // 
            // EyeListView2
            // 
            this.EyeListView2.AllowUserToAddRows = false;
            this.EyeListView2.AllowUserToDeleteRows = false;
            this.EyeListView2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.EyeListView2.AutoSizeRowsMode = System.Windows.Forms.DataGridViewAutoSizeRowsMode.AllCells;
            this.EyeListView2.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.EyeListView2.ContextMenuStrip = this.EyeListContextMenu2;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("MS UI Gothic", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.EyeListView2.DefaultCellStyle = dataGridViewCellStyle1;
            this.EyeListView2.Location = new System.Drawing.Point(5, 37);
            this.EyeListView2.MultiSelect = false;
            this.EyeListView2.Name = "EyeListView2";
            this.EyeListView2.ReadOnly = true;
            this.EyeListView2.RowHeadersVisible = false;
            this.EyeListView2.RowTemplate.Height = 21;
            this.EyeListView2.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.EyeListView2.Size = new System.Drawing.Size(994, 635);
            this.EyeListView2.TabIndex = 9;
            this.EyeListView2.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.EyeListView2_CellDoubleClick);
            // 
            // EyeListContextMenu2
            // 
            this.EyeListContextMenu2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.WorksheetPrintMenuItem});
            this.EyeListContextMenu2.Name = "EyeListContextMenu2";
            this.EyeListContextMenu2.Size = new System.Drawing.Size(173, 26);
            // 
            // WorksheetPrintMenuItem
            // 
            this.WorksheetPrintMenuItem.Name = "WorksheetPrintMenuItem";
            this.WorksheetPrintMenuItem.Size = new System.Drawing.Size(172, 22);
            this.WorksheetPrintMenuItem.Text = "ワークシート印刷";
            this.WorksheetPrintMenuItem.Click += new System.EventHandler(this.WorksheetPrintMenuItem_Click);
            // 
            // DeptBox2
            // 
            this.DeptBox2.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DeptBox2.FormattingEnabled = true;
            this.DeptBox2.Location = new System.Drawing.Point(305, 9);
            this.DeptBox2.Name = "DeptBox2";
            this.DeptBox2.Size = new System.Drawing.Size(108, 20);
            this.DeptBox2.TabIndex = 10;
            this.DeptBox2.SelectedIndexChanged += new System.EventHandler(this.DeptBox2_SelectedIndexChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(8, 13);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "日付";
            // 
            // EyeListShowButton2
            // 
            this.EyeListShowButton2.Location = new System.Drawing.Point(920, 8);
            this.EyeListShowButton2.Name = "EyeListShowButton2";
            this.EyeListShowButton2.Size = new System.Drawing.Size(75, 23);
            this.EyeListShowButton2.TabIndex = 11;
            this.EyeListShowButton2.Text = "更新";
            this.EyeListShowButton2.UseVisualStyleBackColor = true;
            this.EyeListShowButton2.Click += new System.EventHandler(this.EyeListShowButton2_Click);
            // 
            // EyeDateTimePicker2
            // 
            this.EyeDateTimePicker2.Location = new System.Drawing.Point(43, 10);
            this.EyeDateTimePicker2.MaxDate = new System.DateTime(2999, 12, 31, 0, 0, 0, 0);
            this.EyeDateTimePicker2.MinDate = new System.DateTime(2007, 1, 1, 0, 0, 0, 0);
            this.EyeDateTimePicker2.Name = "EyeDateTimePicker2";
            this.EyeDateTimePicker2.Size = new System.Drawing.Size(109, 19);
            this.EyeDateTimePicker2.TabIndex = 7;
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.Location = new System.Drawing.Point(941, 30);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(69, 23);
            this.CloseButton.TabIndex = 14;
            this.CloseButton.Text = "閉じる";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // stdControlPat11
            // 
            this.stdControlPat11.Location = new System.Drawing.Point(350, 25);
            this.stdControlPat11.Mode1 = MedicalLibrary.Boundary.StdControlPat1.Mode.Normal;
            this.stdControlPat11.Name = "stdControlPat11";
            this.stdControlPat11.ReadOnly = false;
            this.stdControlPat11.Size = new System.Drawing.Size(450, 30);
            this.stdControlPat11.TabIndex = 15;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(310, 35);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(40, 12);
            this.label6.TabIndex = 16;
            this.label6.Text = "患者ID";
            // 
            // FormList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1016, 741);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.stdControlPat11);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.LoginChangeButton);
            this.Controls.Add(this.EyeListTabControl);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("MS UI Gothic", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormList";
            this.Text = "（ ログイン中）";
            this.Load += new System.EventHandler(this.FormList_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EyeListView1)).EndInit();
            this.EyeListTabControl.ResumeLayout(false);
            this.EyeListPage1.ResumeLayout(false);
            this.EyeListPage1.PerformLayout();
            this.EyeListPage2.ResumeLayout(false);
            this.EyeListPage2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.EyeListView2)).EndInit();
            this.EyeListContextMenu2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem FileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FileExitMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToolMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToolOpeRsvMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToolPatMenuItem;
        private System.Windows.Forms.DateTimePicker EyeDateTimePicker1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DataGridView EyeListView1;
        private System.Windows.Forms.ComboBox DeptBox1;
        private System.Windows.Forms.CheckBox ShowEndBox1;
        private System.Windows.Forms.Button EyeListShowButton1;
        private System.Windows.Forms.Button LoginChangeButton;
        private System.Windows.Forms.ToolStripMenuItem ToolOpeFindMenuItem;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabControl EyeListTabControl;
        private System.Windows.Forms.TabPage EyeListPage1;
        private System.Windows.Forms.TabPage EyeListPage2;
        private System.Windows.Forms.DataGridView EyeListView2;
        private System.Windows.Forms.ComboBox DeptBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button EyeListShowButton2;
        private System.Windows.Forms.DateTimePicker EyeDateTimePicker2;
        private System.Windows.Forms.ComboBox WardBox2;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.ToolStripMenuItem FileCloseMenuItem;
        private System.Windows.Forms.ContextMenuStrip EyeListContextMenu2;
        private System.Windows.Forms.ToolStripMenuItem WorksheetPrintMenuItem;
        private MedicalLibrary.Boundary.StdControlPat1 stdControlPat11;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox FilterBox1;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox FilterBox2;
    }
}

