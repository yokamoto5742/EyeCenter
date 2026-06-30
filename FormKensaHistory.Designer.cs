namespace EyeCenter
{
    partial class FormKensaHistory
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormKensaHistory));
            this.CloseButton = new System.Windows.Forms.Button();
            this.ContentPanel = new System.Windows.Forms.Panel();
            this.DescLabel = new System.Windows.Forms.Label();
            this.ExcelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(235, 5);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(50, 20);
            this.CloseButton.TabIndex = 0;
            this.CloseButton.Text = "閉じる";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // ContentPanel
            // 
            this.ContentPanel.AutoScroll = true;
            this.ContentPanel.BackColor = System.Drawing.SystemColors.Control;
            this.ContentPanel.Location = new System.Drawing.Point(100, 30);
            this.ContentPanel.Name = "ContentPanel";
            this.ContentPanel.Size = new System.Drawing.Size(160, 222);
            this.ContentPanel.TabIndex = 1;
            // 
            // DescLabel
            // 
            this.DescLabel.AutoSize = true;
            this.DescLabel.Location = new System.Drawing.Point(8, 9);
            this.DescLabel.Name = "DescLabel";
            this.DescLabel.Size = new System.Drawing.Size(167, 12);
            this.DescLabel.TabIndex = 2;
            this.DescLabel.Text = "ピンクの日の結果が表示されます。";
            // 
            // ExcelButton
            // 
            this.ExcelButton.Location = new System.Drawing.Point(180, 5);
            this.ExcelButton.Name = "ExcelButton";
            this.ExcelButton.Size = new System.Drawing.Size(50, 20);
            this.ExcelButton.TabIndex = 3;
            this.ExcelButton.Text = "Excel";
            this.ExcelButton.UseVisualStyleBackColor = true;
            this.ExcelButton.Click += new System.EventHandler(this.ExcelButton_Click);
            // 
            // FormKensaHistory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.ExcelButton);
            this.Controls.Add(this.DescLabel);
            this.Controls.Add(this.ContentPanel);
            this.Controls.Add(this.CloseButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormKensaHistory";
            this.Text = "検査履歴";
            this.Load += new System.EventHandler(this.FormKensaHistory_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Panel ContentPanel;
        private System.Windows.Forms.Label DescLabel;
        private System.Windows.Forms.Button ExcelButton;
    }
}