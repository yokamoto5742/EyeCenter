namespace EyeCenter
{
    partial class FormFindKensa
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFindKensa));
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.FindButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.KensaListView = new System.Windows.Forms.DataGridView();
            this.ExcelButton = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.StartDate = new System.Windows.Forms.DateTimePicker();
            this.EndDate = new System.Windows.Forms.DateTimePicker();
            this.CountLabel = new System.Windows.Forms.Label();
            this.CSVButton = new System.Windows.Forms.Button();
            this.KensaListBox = new System.Windows.Forms.CheckedListBox();
            ((System.ComponentModel.ISupportInitialize)(this.KensaListView)).BeginInit();
            this.SuspendLayout();
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "期間";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(158, 14);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 12);
            this.label4.TabIndex = 5;
            this.label4.Text = "～";
            // 
            // FindButton
            // 
            this.FindButton.Location = new System.Drawing.Point(300, 6);
            this.FindButton.Name = "FindButton";
            this.FindButton.Size = new System.Drawing.Size(70, 23);
            this.FindButton.TabIndex = 16;
            this.FindButton.Text = "検索";
            this.FindButton.UseVisualStyleBackColor = true;
            this.FindButton.Click += new System.EventHandler(this.FindButton_Click);
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(705, 6);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 17;
            this.CloseButton.Text = "閉じる";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // KensaListView
            // 
            this.KensaListView.AllowUserToAddRows = false;
            this.KensaListView.AllowUserToDeleteRows = false;
            this.KensaListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.KensaListView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("MS UI Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(192)))), ((int)(((byte)(192)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.KensaListView.DefaultCellStyle = dataGridViewCellStyle1;
            this.KensaListView.Location = new System.Drawing.Point(300, 34);
            this.KensaListView.MultiSelect = false;
            this.KensaListView.Name = "KensaListView";
            this.KensaListView.ReadOnly = true;
            this.KensaListView.RowHeadersVisible = false;
            this.KensaListView.RowTemplate.Height = 21;
            this.KensaListView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.KensaListView.Size = new System.Drawing.Size(480, 535);
            this.KensaListView.TabIndex = 18;
            this.KensaListView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.KensaListView_CellDoubleClick);
            // 
            // ExcelButton
            // 
            this.ExcelButton.Location = new System.Drawing.Point(515, 6);
            this.ExcelButton.Name = "ExcelButton";
            this.ExcelButton.Size = new System.Drawing.Size(70, 23);
            this.ExcelButton.TabIndex = 19;
            this.ExcelButton.Text = "Excel出力";
            this.ExcelButton.UseVisualStyleBackColor = true;
            this.ExcelButton.Click += new System.EventHandler(this.ExcelButton_Click);
            // 
            // StartDate
            // 
            this.StartDate.Location = new System.Drawing.Point(46, 9);
            this.StartDate.Name = "StartDate";
            this.StartDate.Size = new System.Drawing.Size(107, 19);
            this.StartDate.TabIndex = 20;
            // 
            // EndDate
            // 
            this.EndDate.Location = new System.Drawing.Point(178, 9);
            this.EndDate.Name = "EndDate";
            this.EndDate.Size = new System.Drawing.Size(107, 19);
            this.EndDate.TabIndex = 21;
            // 
            // CountLabel
            // 
            this.CountLabel.AutoSize = true;
            this.CountLabel.Location = new System.Drawing.Point(397, 12);
            this.CountLabel.Name = "CountLabel";
            this.CountLabel.Size = new System.Drawing.Size(65, 12);
            this.CountLabel.TabIndex = 34;
            this.CountLabel.Text = "人数　　　人";
            // 
            // CSVButton
            // 
            this.CSVButton.Location = new System.Drawing.Point(591, 6);
            this.CSVButton.Name = "CSVButton";
            this.CSVButton.Size = new System.Drawing.Size(70, 23);
            this.CSVButton.TabIndex = 35;
            this.CSVButton.Text = "csv出力";
            this.CSVButton.UseVisualStyleBackColor = true;
            this.CSVButton.Click += new System.EventHandler(this.CSVButton_Click);
            // 
            // KensaListBox
            // 
            this.KensaListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.KensaListBox.CheckOnClick = true;
            this.KensaListBox.FormattingEnabled = true;
            this.KensaListBox.Location = new System.Drawing.Point(14, 34);
            this.KensaListBox.Name = "KensaListBox";
            this.KensaListBox.Size = new System.Drawing.Size(271, 536);
            this.KensaListBox.TabIndex = 36;
            // 
            // FormFindKensa
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(792, 573);
            this.Controls.Add(this.KensaListBox);
            this.Controls.Add(this.CSVButton);
            this.Controls.Add(this.CountLabel);
            this.Controls.Add(this.EndDate);
            this.Controls.Add(this.StartDate);
            this.Controls.Add(this.ExcelButton);
            this.Controls.Add(this.KensaListView);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.FindButton);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormFindKensa";
            this.Text = "検査結果検索";
            this.Load += new System.EventHandler(this.FormFindKensa_Load);
            ((System.ComponentModel.ISupportInitialize)(this.KensaListView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button FindButton;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.DataGridView KensaListView;
        private System.Windows.Forms.Button ExcelButton;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.DateTimePicker StartDate;
        private System.Windows.Forms.DateTimePicker EndDate;
        private System.Windows.Forms.Label CountLabel;
        private System.Windows.Forms.Button CSVButton;
        private System.Windows.Forms.CheckedListBox KensaListBox;
    }
}