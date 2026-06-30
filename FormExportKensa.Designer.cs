namespace EyeCenter
{
    partial class FormExportKensa
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormExportKensa));
            this.KensaListBox = new System.Windows.Forms.CheckedListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ExeButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.SuspendLayout();
            // 
            // KensaListBox
            // 
            this.KensaListBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.KensaListBox.CheckOnClick = true;
            this.KensaListBox.FormattingEnabled = true;
            this.KensaListBox.Location = new System.Drawing.Point(12, 27);
            this.KensaListBox.Name = "KensaListBox";
            this.KensaListBox.Size = new System.Drawing.Size(208, 270);
            this.KensaListBox.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(200, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "エクスポートする検査項目を選んでください";
            // 
            // ExeButton
            // 
            this.ExeButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.ExeButton.Location = new System.Drawing.Point(23, 307);
            this.ExeButton.Name = "ExeButton";
            this.ExeButton.Size = new System.Drawing.Size(75, 23);
            this.ExeButton.TabIndex = 2;
            this.ExeButton.Text = "実行";
            this.ExeButton.UseVisualStyleBackColor = true;
            this.ExeButton.Click += new System.EventHandler(this.ExeButton_Click);
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.CloseButton.Location = new System.Drawing.Point(121, 307);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 3;
            this.CloseButton.Text = "閉じる";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // FormExportKensa
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(232, 333);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.ExeButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.KensaListBox);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormExportKensa";
            this.Text = "検査結果エクスポート";
            this.Load += new System.EventHandler(this.FormExportKensa_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckedListBox KensaListBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ExeButton;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}