namespace EyeCenter
{
    partial class FormOpeRsvList
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormOpeRsvList));
            this.NewButton = new System.Windows.Forms.Button();
            this.ModButton = new System.Windows.Forms.Button();
            this.DelButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.OpeDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.TimeBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.KindBox = new System.Windows.Forms.ComboBox();
            this.RsvGridView = new System.Windows.Forms.DataGridView();
            this.ShowButton = new System.Windows.Forms.Button();
            this.PrintButton = new System.Windows.Forms.Button();
            this.printDocument1 = new System.Drawing.Printing.PrintDocument();
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.printPreviewDialog1 = new System.Windows.Forms.PrintPreviewDialog();
            this.CountLabel = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.RsvGridView)).BeginInit();
            this.SuspendLayout();
            // 
            // NewButton
            // 
            this.NewButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.NewButton.Location = new System.Drawing.Point(12, 586);
            this.NewButton.Name = "NewButton";
            this.NewButton.Size = new System.Drawing.Size(60, 23);
            this.NewButton.TabIndex = 1;
            this.NewButton.Text = "追加";
            this.NewButton.UseVisualStyleBackColor = true;
            this.NewButton.Click += new System.EventHandler(this.NewButton_Click);
            // 
            // ModButton
            // 
            this.ModButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ModButton.Location = new System.Drawing.Point(78, 586);
            this.ModButton.Name = "ModButton";
            this.ModButton.Size = new System.Drawing.Size(90, 23);
            this.ModButton.TabIndex = 2;
            this.ModButton.Text = "参照・修正";
            this.ModButton.UseVisualStyleBackColor = true;
            this.ModButton.Click += new System.EventHandler(this.ModButton_Click);
            // 
            // DelButton
            // 
            this.DelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.DelButton.Location = new System.Drawing.Point(174, 586);
            this.DelButton.Name = "DelButton";
            this.DelButton.Size = new System.Drawing.Size(60, 23);
            this.DelButton.TabIndex = 3;
            this.DelButton.Text = "削除";
            this.DelButton.UseVisualStyleBackColor = true;
            this.DelButton.Click += new System.EventHandler(this.DelButton_Click);
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.Location = new System.Drawing.Point(976, 7);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(59, 23);
            this.CloseButton.TabIndex = 4;
            this.CloseButton.Text = "閉じる";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "予約日";
            // 
            // OpeDateTimePicker
            // 
            this.OpeDateTimePicker.Location = new System.Drawing.Point(55, 9);
            this.OpeDateTimePicker.Name = "OpeDateTimePicker";
            this.OpeDateTimePicker.Size = new System.Drawing.Size(110, 19);
            this.OpeDateTimePicker.TabIndex = 7;
            this.OpeDateTimePicker.ValueChanged += new System.EventHandler(this.OpeDateTimePicker_ValueChanged);
            // 
            // TimeBox
            // 
            this.TimeBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TimeBox.FormattingEnabled = true;
            this.TimeBox.Location = new System.Drawing.Point(394, 8);
            this.TimeBox.Name = "TimeBox";
            this.TimeBox.Size = new System.Drawing.Size(94, 20);
            this.TimeBox.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(362, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(29, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "時間";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(180, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(29, 12);
            this.label3.TabIndex = 11;
            this.label3.Text = "種別";
            // 
            // KindBox
            // 
            this.KindBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.KindBox.FormattingEnabled = true;
            this.KindBox.Location = new System.Drawing.Point(212, 8);
            this.KindBox.Name = "KindBox";
            this.KindBox.Size = new System.Drawing.Size(138, 20);
            this.KindBox.TabIndex = 10;
            this.KindBox.SelectedIndexChanged += new System.EventHandler(this.KindBox_SelectedIndexChanged);
            // 
            // RsvGridView
            // 
            this.RsvGridView.AllowUserToAddRows = false;
            this.RsvGridView.AllowUserToDeleteRows = false;
            this.RsvGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RsvGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("ＭＳ Ｐゴシック", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(224)))), ((int)(((byte)(192)))));
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.RsvGridView.DefaultCellStyle = dataGridViewCellStyle1;
            this.RsvGridView.Location = new System.Drawing.Point(14, 35);
            this.RsvGridView.MultiSelect = false;
            this.RsvGridView.Name = "RsvGridView";
            this.RsvGridView.ReadOnly = true;
            this.RsvGridView.RowHeadersVisible = false;
            this.RsvGridView.RowTemplate.Height = 21;
            this.RsvGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.RsvGridView.Size = new System.Drawing.Size(1022, 545);
            this.RsvGridView.TabIndex = 13;
            this.RsvGridView.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.RsvGridView_CellDoubleClick);
            // 
            // ShowButton
            // 
            this.ShowButton.BackColor = System.Drawing.SystemColors.Control;
            this.ShowButton.Location = new System.Drawing.Point(500, 8);
            this.ShowButton.Name = "ShowButton";
            this.ShowButton.Size = new System.Drawing.Size(71, 21);
            this.ShowButton.TabIndex = 14;
            this.ShowButton.Text = "表示";
            this.ShowButton.UseVisualStyleBackColor = false;
            this.ShowButton.Click += new System.EventHandler(this.ShowButton_Click);
            // 
            // PrintButton
            // 
            this.PrintButton.Location = new System.Drawing.Point(582, 8);
            this.PrintButton.Name = "PrintButton";
            this.PrintButton.Size = new System.Drawing.Size(71, 21);
            this.PrintButton.TabIndex = 15;
            this.PrintButton.Text = "一覧印刷";
            this.PrintButton.UseVisualStyleBackColor = true;
            this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
            // 
            // printDocument1
            // 
            this.printDocument1.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.printDocument1_PrintPage);
            // 
            // printDialog1
            // 
            this.printDialog1.UseEXDialog = true;
            // 
            // printPreviewDialog1
            // 
            this.printPreviewDialog1.AutoScrollMargin = new System.Drawing.Size(0, 0);
            this.printPreviewDialog1.AutoScrollMinSize = new System.Drawing.Size(0, 0);
            this.printPreviewDialog1.ClientSize = new System.Drawing.Size(400, 300);
            this.printPreviewDialog1.Enabled = true;
            this.printPreviewDialog1.Icon = ((System.Drawing.Icon)(resources.GetObject("printPreviewDialog1.Icon")));
            this.printPreviewDialog1.Name = "printPreviewDialog1";
            this.printPreviewDialog1.Visible = false;
            // 
            // CountLabel
            // 
            this.CountLabel.AutoSize = true;
            this.CountLabel.Location = new System.Drawing.Point(736, 13);
            this.CountLabel.Name = "CountLabel";
            this.CountLabel.Size = new System.Drawing.Size(65, 12);
            this.CountLabel.TabIndex = 16;
            this.CountLabel.Text = "件数　　　件";
            // 
            // FormOpeRsvList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1048, 613);
            this.Controls.Add(this.CountLabel);
            this.Controls.Add(this.PrintButton);
            this.Controls.Add(this.ShowButton);
            this.Controls.Add(this.RsvGridView);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.KindBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.TimeBox);
            this.Controls.Add(this.OpeDateTimePicker);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.DelButton);
            this.Controls.Add(this.ModButton);
            this.Controls.Add(this.NewButton);
            this.Font = new System.Drawing.Font("ＭＳ Ｐゴシック", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormOpeRsvList";
            this.Text = "（ ログイン中）";
            this.Load += new System.EventHandler(this.FormOpeRsvList_Load);
            ((System.ComponentModel.ISupportInitialize)(this.RsvGridView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button NewButton;
        private System.Windows.Forms.Button ModButton;
        private System.Windows.Forms.Button DelButton;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker OpeDateTimePicker;
        private System.Windows.Forms.ComboBox TimeBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox KindBox;
        private System.Windows.Forms.DataGridView RsvGridView;
        private System.Windows.Forms.Button ShowButton;
        private System.Windows.Forms.Button PrintButton;
        private System.Drawing.Printing.PrintDocument printDocument1;
        private System.Windows.Forms.PrintDialog printDialog1;
        private System.Windows.Forms.PrintPreviewDialog printPreviewDialog1;
        private System.Windows.Forms.Label CountLabel;
    }
}