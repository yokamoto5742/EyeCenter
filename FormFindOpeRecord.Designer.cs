namespace EyeCenter
{
    partial class FormFindOpeRecord
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFindOpeRecord));
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.DiagBox = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.DoctorBox = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.OpeBox = new System.Windows.Forms.ComboBox();
            this.label9 = new System.Windows.Forms.Label();
            this.FindButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.OpeListView = new System.Windows.Forms.DataGridView();
            this.ExcelButton = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.StartDate = new System.Windows.Forms.DateTimePicker();
            this.EndDate = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.RecordBox11 = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.RecordBox12 = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.RecordBox13 = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.RecordBox23 = new System.Windows.Forms.TextBox();
            this.RecordBox22 = new System.Windows.Forms.ComboBox();
            this.label11 = new System.Windows.Forms.Label();
            this.RecordBox21 = new System.Windows.Forms.ComboBox();
            this.CountLabel = new System.Windows.Forms.Label();
            this.CSVButton = new System.Windows.Forms.Button();
            this.PreCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.OpeListView)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "対象期間";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(198, 14);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 12);
            this.label4.TabIndex = 5;
            this.label4.Text = "～";
            // 
            // DiagBox
            // 
            this.DiagBox.FormattingEnabled = true;
            this.DiagBox.Location = new System.Drawing.Point(80, 34);
            this.DiagBox.Name = "DiagBox";
            this.DiagBox.Size = new System.Drawing.Size(257, 20);
            this.DiagBox.TabIndex = 10;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(25, 37);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 12);
            this.label7.TabIndex = 11;
            this.label7.Text = "病名";
            // 
            // DoctorBox
            // 
            this.DoctorBox.FormattingEnabled = true;
            this.DoctorBox.Location = new System.Drawing.Point(80, 87);
            this.DoctorBox.Name = "DoctorBox";
            this.DoctorBox.Size = new System.Drawing.Size(141, 20);
            this.DoctorBox.TabIndex = 12;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(25, 63);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(29, 12);
            this.label8.TabIndex = 14;
            this.label8.Text = "術式";
            // 
            // OpeBox
            // 
            this.OpeBox.FormattingEnabled = true;
            this.OpeBox.Location = new System.Drawing.Point(80, 60);
            this.OpeBox.Name = "OpeBox";
            this.OpeBox.Size = new System.Drawing.Size(257, 20);
            this.OpeBox.TabIndex = 13;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(21, 90);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(41, 12);
            this.label9.TabIndex = 15;
            this.label9.Text = "執刀医";
            // 
            // FindButton
            // 
            this.FindButton.Location = new System.Drawing.Point(654, 89);
            this.FindButton.Name = "FindButton";
            this.FindButton.Size = new System.Drawing.Size(70, 23);
            this.FindButton.TabIndex = 16;
            this.FindButton.Text = "検索";
            this.FindButton.UseVisualStyleBackColor = true;
            this.FindButton.Click += new System.EventHandler(this.FindButton_Click);
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(926, 7);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 17;
            this.CloseButton.Text = "閉じる";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // OpeListView
            // 
            this.OpeListView.AllowUserToAddRows = false;
            this.OpeListView.AllowUserToDeleteRows = false;
            this.OpeListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OpeListView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("MS UI Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.OpeListView.DefaultCellStyle = dataGridViewCellStyle1;
            this.OpeListView.Location = new System.Drawing.Point(5, 115);
            this.OpeListView.MultiSelect = false;
            this.OpeListView.Name = "OpeListView";
            this.OpeListView.ReadOnly = true;
            this.OpeListView.RowHeadersVisible = false;
            this.OpeListView.RowTemplate.Height = 21;
            this.OpeListView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.OpeListView.Size = new System.Drawing.Size(1040, 455);
            this.OpeListView.TabIndex = 18;
            this.OpeListView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.OpeListView_CellDoubleClick);
            // 
            // ExcelButton
            // 
            this.ExcelButton.Location = new System.Drawing.Point(854, 89);
            this.ExcelButton.Name = "ExcelButton";
            this.ExcelButton.Size = new System.Drawing.Size(70, 23);
            this.ExcelButton.TabIndex = 19;
            this.ExcelButton.Text = "Excel出力";
            this.ExcelButton.UseVisualStyleBackColor = true;
            this.ExcelButton.Click += new System.EventHandler(this.ExcelButton_Click);
            // 
            // StartDate
            // 
            this.StartDate.Location = new System.Drawing.Point(80, 9);
            this.StartDate.Name = "StartDate";
            this.StartDate.Size = new System.Drawing.Size(107, 19);
            this.StartDate.TabIndex = 20;
            // 
            // EndDate
            // 
            this.EndDate.Location = new System.Drawing.Point(230, 9);
            this.EndDate.Name = "EndDate";
            this.EndDate.Size = new System.Drawing.Size(107, 19);
            this.EndDate.TabIndex = 21;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(352, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 22;
            this.label1.Text = "手術記録";
            // 
            // RecordBox11
            // 
            this.RecordBox11.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RecordBox11.FormattingEnabled = true;
            this.RecordBox11.Location = new System.Drawing.Point(404, 33);
            this.RecordBox11.Name = "RecordBox11";
            this.RecordBox11.Size = new System.Drawing.Size(123, 20);
            this.RecordBox11.TabIndex = 23;
            this.RecordBox11.SelectedIndexChanged += new System.EventHandler(this.RecordBox11_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(419, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 24;
            this.label2.Text = "種別";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(359, 37);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 12);
            this.label5.TabIndex = 25;
            this.label5.Text = "条件1";
            // 
            // RecordBox12
            // 
            this.RecordBox12.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RecordBox12.FormattingEnabled = true;
            this.RecordBox12.Location = new System.Drawing.Point(533, 33);
            this.RecordBox12.Name = "RecordBox12";
            this.RecordBox12.Size = new System.Drawing.Size(192, 20);
            this.RecordBox12.TabIndex = 26;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(549, 18);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(29, 12);
            this.label6.TabIndex = 27;
            this.label6.Text = "項目";
            // 
            // RecordBox13
            // 
            this.RecordBox13.Location = new System.Drawing.Point(731, 34);
            this.RecordBox13.Name = "RecordBox13";
            this.RecordBox13.Size = new System.Drawing.Size(166, 19);
            this.RecordBox13.TabIndex = 28;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(732, 18);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(165, 12);
            this.label10.TabIndex = 29;
            this.label10.Text = "条件（完全一致または先頭一致）";
            // 
            // RecordBox23
            // 
            this.RecordBox23.Location = new System.Drawing.Point(731, 61);
            this.RecordBox23.Name = "RecordBox23";
            this.RecordBox23.Size = new System.Drawing.Size(166, 19);
            this.RecordBox23.TabIndex = 33;
            // 
            // RecordBox22
            // 
            this.RecordBox22.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RecordBox22.FormattingEnabled = true;
            this.RecordBox22.Location = new System.Drawing.Point(533, 60);
            this.RecordBox22.Name = "RecordBox22";
            this.RecordBox22.Size = new System.Drawing.Size(192, 20);
            this.RecordBox22.TabIndex = 32;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(359, 64);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(35, 12);
            this.label11.TabIndex = 31;
            this.label11.Text = "条件2";
            // 
            // RecordBox21
            // 
            this.RecordBox21.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RecordBox21.FormattingEnabled = true;
            this.RecordBox21.Location = new System.Drawing.Point(404, 60);
            this.RecordBox21.Name = "RecordBox21";
            this.RecordBox21.Size = new System.Drawing.Size(123, 20);
            this.RecordBox21.TabIndex = 30;
            this.RecordBox21.SelectedIndexChanged += new System.EventHandler(this.RecordBox21_SelectedIndexChanged);
            // 
            // CountLabel
            // 
            this.CountLabel.AutoSize = true;
            this.CountLabel.Location = new System.Drawing.Point(754, 94);
            this.CountLabel.Name = "CountLabel";
            this.CountLabel.Size = new System.Drawing.Size(65, 12);
            this.CountLabel.TabIndex = 34;
            this.CountLabel.Text = "件数　　　件";
            // 
            // CSVButton
            // 
            this.CSVButton.Location = new System.Drawing.Point(930, 89);
            this.CSVButton.Name = "CSVButton";
            this.CSVButton.Size = new System.Drawing.Size(70, 23);
            this.CSVButton.TabIndex = 35;
            this.CSVButton.Text = "csv出力";
            this.CSVButton.UseVisualStyleBackColor = true;
            this.CSVButton.Click += new System.EventHandler(this.CSVButton_Click);
            // 
            // PreCheckBox
            // 
            this.PreCheckBox.AutoSize = true;
            this.PreCheckBox.Location = new System.Drawing.Point(248, 90);
            this.PreCheckBox.Name = "PreCheckBox";
            this.PreCheckBox.Size = new System.Drawing.Size(103, 16);
            this.PreCheckBox.TabIndex = 36;
            this.PreCheckBox.Text = "術前チェック完了";
            this.PreCheckBox.UseVisualStyleBackColor = true;
            this.PreCheckBox.CheckedChanged += new System.EventHandler(this.PreCheckBox_CheckedChanged);
            // 
            // FormFindOpeRecord
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1048, 573);
            this.Controls.Add(this.PreCheckBox);
            this.Controls.Add(this.CSVButton);
            this.Controls.Add(this.CountLabel);
            this.Controls.Add(this.RecordBox23);
            this.Controls.Add(this.RecordBox22);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.RecordBox21);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.RecordBox13);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.RecordBox12);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.RecordBox11);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.EndDate);
            this.Controls.Add(this.StartDate);
            this.Controls.Add(this.ExcelButton);
            this.Controls.Add(this.OpeListView);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.FindButton);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.OpeBox);
            this.Controls.Add(this.DoctorBox);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.DiagBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormFindOpeRecord";
            this.Text = "手術記録検索";
            this.Load += new System.EventHandler(this.FormFindOpeRecord_Load);
            ((System.ComponentModel.ISupportInitialize)(this.OpeListView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox DiagBox;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox DoctorBox;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ComboBox OpeBox;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button FindButton;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.DataGridView OpeListView;
        private System.Windows.Forms.Button ExcelButton;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.DateTimePicker StartDate;
        private System.Windows.Forms.DateTimePicker EndDate;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox RecordBox11;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox RecordBox12;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox RecordBox13;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox RecordBox23;
        private System.Windows.Forms.ComboBox RecordBox22;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox RecordBox21;
        private System.Windows.Forms.Label CountLabel;
        private System.Windows.Forms.Button CSVButton;
        private System.Windows.Forms.CheckBox PreCheckBox;
    }
}