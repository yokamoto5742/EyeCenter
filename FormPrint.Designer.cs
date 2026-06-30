namespace EyeCenter
{
    partial class FormPrint
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormPrint));
            this.PatListButton = new System.Windows.Forms.Button();
            this.KensaListButton = new System.Windows.Forms.Button();
            this.WorksheetButton = new System.Windows.Forms.Button();
            this.printDocument1 = new System.Drawing.Printing.PrintDocument();
            this.printPreviewDialog1 = new System.Windows.Forms.PrintPreviewDialog();
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.PrintDate = new System.Windows.Forms.DateTimePicker();
            this.CloseButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // PatListButton
            // 
            this.PatListButton.Location = new System.Drawing.Point(13, 38);
            this.PatListButton.Name = "PatListButton";
            this.PatListButton.Size = new System.Drawing.Size(108, 23);
            this.PatListButton.TabIndex = 0;
            this.PatListButton.Text = "入院患者一覧";
            this.PatListButton.UseVisualStyleBackColor = true;
            this.PatListButton.Click += new System.EventHandler(this.PatListButton_Click);
            // 
            // KensaListButton
            // 
            this.KensaListButton.Location = new System.Drawing.Point(13, 66);
            this.KensaListButton.Name = "KensaListButton";
            this.KensaListButton.Size = new System.Drawing.Size(108, 23);
            this.KensaListButton.TabIndex = 1;
            this.KensaListButton.Text = "検査予定一覧";
            this.KensaListButton.UseVisualStyleBackColor = true;
            this.KensaListButton.Click += new System.EventHandler(this.KensaListButton_Click);
            // 
            // WorksheetButton
            // 
            this.WorksheetButton.Location = new System.Drawing.Point(13, 94);
            this.WorksheetButton.Name = "WorksheetButton";
            this.WorksheetButton.Size = new System.Drawing.Size(108, 23);
            this.WorksheetButton.TabIndex = 2;
            this.WorksheetButton.Text = "ワークシート";
            this.WorksheetButton.UseVisualStyleBackColor = true;
            this.WorksheetButton.Click += new System.EventHandler(this.WorksheetButton_Click);
            // 
            // printDocument1
            // 
            this.printDocument1.PrintPage += new System.Drawing.Printing.PrintPageEventHandler(this.printDocument1_PrintPage);
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
            // printDialog1
            // 
            this.printDialog1.UseEXDialog = true;
            // 
            // PrintDate
            // 
            this.PrintDate.Location = new System.Drawing.Point(14, 9);
            this.PrintDate.Name = "PrintDate";
            this.PrintDate.Size = new System.Drawing.Size(107, 19);
            this.PrintDate.TabIndex = 3;
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(13, 123);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(108, 23);
            this.CloseButton.TabIndex = 4;
            this.CloseButton.Text = "閉じる";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // FormPrint
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(138, 152);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.PrintDate);
            this.Controls.Add(this.WorksheetButton);
            this.Controls.Add(this.KensaListButton);
            this.Controls.Add(this.PatListButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormPrint";
            this.Text = "一括印刷";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button PatListButton;
        private System.Windows.Forms.Button KensaListButton;
        private System.Windows.Forms.Button WorksheetButton;
        private System.Drawing.Printing.PrintDocument printDocument1;
        private System.Windows.Forms.PrintPreviewDialog printPreviewDialog1;
        private System.Windows.Forms.PrintDialog printDialog1;
        private System.Windows.Forms.DateTimePicker PrintDate;
        private System.Windows.Forms.Button CloseButton;
    }
}