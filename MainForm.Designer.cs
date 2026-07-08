namespace EyeCenter
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.CanonButton = new System.Windows.Forms.Button();
            this.NidekButton = new System.Windows.Forms.Button();
            this.SummaryFindButton = new System.Windows.Forms.Button();
            this.KensaFindButton = new System.Windows.Forms.Button();
            this.ExportButton = new System.Windows.Forms.Button();
            this.PrintButton = new System.Windows.Forms.Button();
            this.OpeFindButton = new System.Windows.Forms.Button();
            this.ExitButton = new System.Windows.Forms.Button();
            this.RsvButton = new System.Windows.Forms.Button();
            this.PatButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CanonButton
            // 
            this.CanonButton.Location = new System.Drawing.Point(5, 120);
            this.CanonButton.Name = "CanonButton";
            this.CanonButton.Size = new System.Drawing.Size(85, 23);
            this.CanonButton.TabIndex = 16;
            this.CanonButton.Text = "Canon RKF";
            this.CanonButton.UseVisualStyleBackColor = true;
            this.CanonButton.Click += new System.EventHandler(this.CanonButton_Click);
            // 
            // NidekButton
            // 
            this.NidekButton.Location = new System.Drawing.Point(95, 120);
            this.NidekButton.Name = "NidekButton";
            this.NidekButton.Size = new System.Drawing.Size(85, 23);
            this.NidekButton.TabIndex = 15;
            this.NidekButton.Text = "Nidek ARK";
            this.NidekButton.UseVisualStyleBackColor = true;
            this.NidekButton.Click += new System.EventHandler(this.NidekButton_Click);
            // 
            // SummaryFindButton
            // 
            this.SummaryFindButton.Location = new System.Drawing.Point(95, 8);
            this.SummaryFindButton.Name = "SummaryFindButton";
            this.SummaryFindButton.Size = new System.Drawing.Size(85, 23);
            this.SummaryFindButton.TabIndex = 11;
            this.SummaryFindButton.Text = "サマリ検索";
            this.SummaryFindButton.UseVisualStyleBackColor = true;
            this.SummaryFindButton.Click += new System.EventHandler(this.SummaryFindButton_Click);
            // 
            // KensaFindButton
            // 
            this.KensaFindButton.Location = new System.Drawing.Point(95, 64);
            this.KensaFindButton.Name = "KensaFindButton";
            this.KensaFindButton.Size = new System.Drawing.Size(85, 23);
            this.KensaFindButton.TabIndex = 10;
            this.KensaFindButton.Text = "検査検索";
            this.KensaFindButton.UseVisualStyleBackColor = true;
            this.KensaFindButton.Click += new System.EventHandler(this.KensaFindButton_Click);
            // 
            // ExportButton
            // 
            this.ExportButton.Location = new System.Drawing.Point(5, 64);
            this.ExportButton.Name = "ExportButton";
            this.ExportButton.Size = new System.Drawing.Size(85, 23);
            this.ExportButton.TabIndex = 6;
            this.ExportButton.Text = "エクスポート";
            this.ExportButton.UseVisualStyleBackColor = true;
            this.ExportButton.Click += new System.EventHandler(this.ExportButton_Click);
            // 
            // PrintButton
            // 
            this.PrintButton.Location = new System.Drawing.Point(95, 92);
            this.PrintButton.Name = "PrintButton";
            this.PrintButton.Size = new System.Drawing.Size(85, 23);
            this.PrintButton.TabIndex = 5;
            this.PrintButton.Text = "一括印刷";
            this.PrintButton.UseVisualStyleBackColor = true;
            this.PrintButton.Click += new System.EventHandler(this.PrintButton_Click);
            // 
            // OpeFindButton
            // 
            this.OpeFindButton.Location = new System.Drawing.Point(95, 36);
            this.OpeFindButton.Name = "OpeFindButton";
            this.OpeFindButton.Size = new System.Drawing.Size(85, 23);
            this.OpeFindButton.TabIndex = 4;
            this.OpeFindButton.Text = "手術検索";
            this.OpeFindButton.UseVisualStyleBackColor = true;
            this.OpeFindButton.Click += new System.EventHandler(this.OpeFindButton_Click);
            // 
            // ExitButton
            // 
            this.ExitButton.Location = new System.Drawing.Point(95, 148);
            this.ExitButton.Name = "ExitButton";
            this.ExitButton.Size = new System.Drawing.Size(85, 23);
            this.ExitButton.TabIndex = 3;
            this.ExitButton.Text = "終了";
            this.ExitButton.UseVisualStyleBackColor = true;
            this.ExitButton.Click += new System.EventHandler(this.ExitButton_Click);
            // 
            // RsvButton
            // 
            this.RsvButton.Location = new System.Drawing.Point(5, 36);
            this.RsvButton.Name = "RsvButton";
            this.RsvButton.Size = new System.Drawing.Size(85, 23);
            this.RsvButton.TabIndex = 2;
            this.RsvButton.Text = "手術予約";
            this.RsvButton.UseVisualStyleBackColor = true;
            this.RsvButton.Click += new System.EventHandler(this.RsvButton_Click);
            // 
            // PatButton
            // 
            this.PatButton.Location = new System.Drawing.Point(5, 8);
            this.PatButton.Name = "PatButton";
            this.PatButton.Size = new System.Drawing.Size(85, 23);
            this.PatButton.TabIndex = 1;
            this.PatButton.Text = "患者台帳";
            this.PatButton.UseVisualStyleBackColor = true;
            this.PatButton.Click += new System.EventHandler(this.PatButton_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(185, 202);
            this.Controls.Add(this.CanonButton);
            this.Controls.Add(this.NidekButton);
            this.Controls.Add(this.SummaryFindButton);
            this.Controls.Add(this.KensaFindButton);
            this.Controls.Add(this.ExportButton);
            this.Controls.Add(this.PrintButton);
            this.Controls.Add(this.OpeFindButton);
            this.Controls.Add(this.ExitButton);
            this.Controls.Add(this.RsvButton);
            this.Controls.Add(this.PatButton);
            this.Font = new System.Drawing.Font("ＭＳ Ｐゴシック", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "EyeCenter";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button PatButton;
        private System.Windows.Forms.Button RsvButton;
        private System.Windows.Forms.Button ExitButton;
        private System.Windows.Forms.Button OpeFindButton;
        private System.Windows.Forms.Button PrintButton;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.Button KensaFindButton;
        private System.Windows.Forms.Button SummaryFindButton;
        private System.Windows.Forms.Button NidekButton;
        private System.Windows.Forms.Button CanonButton;
    }
}