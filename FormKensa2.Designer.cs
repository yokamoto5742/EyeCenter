namespace EyeCenter
{
    partial class FormKensa2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormKensa2));
            this.CloseButton = new System.Windows.Forms.Button();
            this.ContPanel = new System.Windows.Forms.Panel();
            this.DescLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // CloseButton
            // 
            this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CloseButton.Location = new System.Drawing.Point(235, 5);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(50, 20);
            this.CloseButton.TabIndex = 3;
            this.CloseButton.Text = "閉じる";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // ContPanel
            // 
            this.ContPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ContPanel.AutoScroll = true;
            this.ContPanel.Location = new System.Drawing.Point(5, 31);
            this.ContPanel.Name = "ContPanel";
            this.ContPanel.Size = new System.Drawing.Size(280, 236);
            this.ContPanel.TabIndex = 4;
            // 
            // DescLabel
            // 
            this.DescLabel.BackColor = System.Drawing.Color.LightYellow;
            this.DescLabel.Location = new System.Drawing.Point(24, 6);
            this.DescLabel.Name = "DescLabel";
            this.DescLabel.Size = new System.Drawing.Size(129, 18);
            this.DescLabel.TabIndex = 5;
            this.DescLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // FormKensa2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.DescLabel);
            this.Controls.Add(this.ContPanel);
            this.Controls.Add(this.CloseButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormKensa2";
            this.Text = "検査データ";
            this.Load += new System.EventHandler(this.FormKensa2_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Panel ContPanel;
        private System.Windows.Forms.Label DescLabel;
    }
}