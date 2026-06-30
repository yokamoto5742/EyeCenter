namespace EyeCenter
{
    partial class FormFindSummary
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormFindSummary));
            this.SumKindBox3 = new System.Windows.Forms.ComboBox();
            this.label43 = new System.Windows.Forms.Label();
            this.SumKindBox2 = new System.Windows.Forms.ComboBox();
            this.label42 = new System.Windows.Forms.Label();
            this.SumKindBox1 = new System.Windows.Forms.ComboBox();
            this.label40 = new System.Windows.Forms.Label();
            this.SumDiagBox = new System.Windows.Forms.ComboBox();
            this.SumDiagLabel = new System.Windows.Forms.Label();
            this.FindButton = new System.Windows.Forms.Button();
            this.CSVButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.SumListView = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.SumListView)).BeginInit();
            this.SuspendLayout();
            // 
            // SumKindBox3
            // 
            this.SumKindBox3.FormattingEnabled = true;
            this.SumKindBox3.Location = new System.Drawing.Point(530, 5);
            this.SumKindBox3.Name = "SumKindBox3";
            this.SumKindBox3.Size = new System.Drawing.Size(75, 20);
            this.SumKindBox3.TabIndex = 16;
            // 
            // label43
            // 
            this.label43.AutoSize = true;
            this.label43.Location = new System.Drawing.Point(495, 9);
            this.label43.Name = "label43";
            this.label43.Size = new System.Drawing.Size(35, 12);
            this.label43.TabIndex = 15;
            this.label43.Text = "分類3";
            // 
            // SumKindBox2
            // 
            this.SumKindBox2.FormattingEnabled = true;
            this.SumKindBox2.Location = new System.Drawing.Point(410, 5);
            this.SumKindBox2.Name = "SumKindBox2";
            this.SumKindBox2.Size = new System.Drawing.Size(75, 20);
            this.SumKindBox2.TabIndex = 14;
            // 
            // label42
            // 
            this.label42.AutoSize = true;
            this.label42.Location = new System.Drawing.Point(375, 9);
            this.label42.Name = "label42";
            this.label42.Size = new System.Drawing.Size(35, 12);
            this.label42.TabIndex = 13;
            this.label42.Text = "分類2";
            // 
            // SumKindBox1
            // 
            this.SumKindBox1.FormattingEnabled = true;
            this.SumKindBox1.Location = new System.Drawing.Point(290, 5);
            this.SumKindBox1.Name = "SumKindBox1";
            this.SumKindBox1.Size = new System.Drawing.Size(75, 20);
            this.SumKindBox1.TabIndex = 12;
            // 
            // label40
            // 
            this.label40.AutoSize = true;
            this.label40.Location = new System.Drawing.Point(255, 9);
            this.label40.Name = "label40";
            this.label40.Size = new System.Drawing.Size(35, 12);
            this.label40.TabIndex = 11;
            this.label40.Text = "分類1";
            // 
            // SumDiagBox
            // 
            this.SumDiagBox.FormattingEnabled = true;
            this.SumDiagBox.Location = new System.Drawing.Point(53, 5);
            this.SumDiagBox.Name = "SumDiagBox";
            this.SumDiagBox.Size = new System.Drawing.Size(190, 20);
            this.SumDiagBox.TabIndex = 10;
            // 
            // SumDiagLabel
            // 
            this.SumDiagLabel.AutoSize = true;
            this.SumDiagLabel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.SumDiagLabel.Location = new System.Drawing.Point(7, 9);
            this.SumDiagLabel.Name = "SumDiagLabel";
            this.SumDiagLabel.Size = new System.Drawing.Size(41, 12);
            this.SumDiagLabel.TabIndex = 9;
            this.SumDiagLabel.Text = "主病名";
            this.SumDiagLabel.Click += new System.EventHandler(this.SumDiagLabel_Click);
            // 
            // FindButton
            // 
            this.FindButton.Location = new System.Drawing.Point(625, 4);
            this.FindButton.Name = "FindButton";
            this.FindButton.Size = new System.Drawing.Size(60, 22);
            this.FindButton.TabIndex = 17;
            this.FindButton.Text = "検索";
            this.FindButton.UseVisualStyleBackColor = true;
            this.FindButton.Click += new System.EventHandler(this.FindButton_Click);
            // 
            // CSVButton
            // 
            this.CSVButton.Location = new System.Drawing.Point(695, 4);
            this.CSVButton.Name = "CSVButton";
            this.CSVButton.Size = new System.Drawing.Size(60, 22);
            this.CSVButton.TabIndex = 18;
            this.CSVButton.Text = "csv出力";
            this.CSVButton.UseVisualStyleBackColor = true;
            this.CSVButton.Click += new System.EventHandler(this.CSVButton_Click);
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(765, 4);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(60, 22);
            this.CloseButton.TabIndex = 19;
            this.CloseButton.Text = "閉じる";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // SumListView
            // 
            this.SumListView.AllowUserToAddRows = false;
            this.SumListView.AllowUserToDeleteRows = false;
            this.SumListView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SumListView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("MS UI Gothic", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(128)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.SumListView.DefaultCellStyle = dataGridViewCellStyle1;
            this.SumListView.Location = new System.Drawing.Point(5, 30);
            this.SumListView.Name = "SumListView";
            this.SumListView.ReadOnly = true;
            this.SumListView.RowHeadersVisible = false;
            this.SumListView.RowTemplate.Height = 21;
            this.SumListView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.SumListView.Size = new System.Drawing.Size(925, 550);
            this.SumListView.TabIndex = 20;
            this.SumListView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.SumListView_CellDoubleClick);
            // 
            // FormFindSummary
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(934, 582);
            this.Controls.Add(this.SumListView);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.CSVButton);
            this.Controls.Add(this.FindButton);
            this.Controls.Add(this.SumKindBox3);
            this.Controls.Add(this.label43);
            this.Controls.Add(this.SumKindBox2);
            this.Controls.Add(this.label42);
            this.Controls.Add(this.SumKindBox1);
            this.Controls.Add(this.label40);
            this.Controls.Add(this.SumDiagBox);
            this.Controls.Add(this.SumDiagLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormFindSummary";
            this.Text = "サマリ検索";
            this.Load += new System.EventHandler(this.FormFindSummary_Load);
            ((System.ComponentModel.ISupportInitialize)(this.SumListView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        protected internal System.Windows.Forms.ComboBox SumKindBox3;
        private System.Windows.Forms.Label label43;
        protected internal System.Windows.Forms.ComboBox SumKindBox2;
        private System.Windows.Forms.Label label42;
        protected internal System.Windows.Forms.ComboBox SumKindBox1;
        private System.Windows.Forms.Label label40;
        protected internal System.Windows.Forms.ComboBox SumDiagBox;
        private System.Windows.Forms.Label SumDiagLabel;
        private System.Windows.Forms.Button FindButton;
        private System.Windows.Forms.Button CSVButton;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.DataGridView SumListView;
    }
}