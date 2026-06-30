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
            this.RsvListButton = new System.Windows.Forms.Button();
            this.CanonButton = new System.Windows.Forms.Button();
            this.NidekButton = new System.Windows.Forms.Button();
            this.MultiPatBox = new System.Windows.Forms.CheckBox();
            this.MAFormPatListButton1 = new System.Windows.Forms.Button();
            this.SummaryFindButton = new System.Windows.Forms.Button();
            this.KensaFindButton = new System.Windows.Forms.Button();
            this.OpeOrderButton = new System.Windows.Forms.Button();
            this.ExportButton = new System.Windows.Forms.Button();
            this.PrintButton = new System.Windows.Forms.Button();
            this.OpeFindButton = new System.Windows.Forms.Button();
            this.ExitButton = new System.Windows.Forms.Button();
            this.RsvButton = new System.Windows.Forms.Button();
            this.PatButton = new System.Windows.Forms.Button();
            this.ListButton = new System.Windows.Forms.Button();
            this.MAFormPatButton1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // RsvListButton
            // 
            this.RsvListButton.Location = new System.Drawing.Point(95, 176);
            this.RsvListButton.Name = "RsvListButton";
            this.RsvListButton.Size = new System.Drawing.Size(85, 23);
            this.RsvListButton.TabIndex = 17;
            this.RsvListButton.Text = "予約一覧";
            this.RsvListButton.UseVisualStyleBackColor = true;
            this.RsvListButton.Click += new System.EventHandler(this.RsvListButton_Click);
            // 
            // CanonButton
            // 
            this.CanonButton.Location = new System.Drawing.Point(5, 148);
            this.CanonButton.Name = "CanonButton";
            this.CanonButton.Size = new System.Drawing.Size(85, 23);
            this.CanonButton.TabIndex = 16;
            this.CanonButton.Text = "Canon RKF";
            this.CanonButton.UseVisualStyleBackColor = true;
            this.CanonButton.Click += new System.EventHandler(this.CanonButton_Click);
            // 
            // NidekButton
            // 
            this.NidekButton.Location = new System.Drawing.Point(95, 148);
            this.NidekButton.Name = "NidekButton";
            this.NidekButton.Size = new System.Drawing.Size(85, 23);
            this.NidekButton.TabIndex = 15;
            this.NidekButton.Text = "Nidek ARK";
            this.NidekButton.UseVisualStyleBackColor = true;
            this.NidekButton.Click += new System.EventHandler(this.NidekButton_Click);
            // 
            // MultiPatBox
            // 
            this.MultiPatBox.AutoSize = true;
            this.MultiPatBox.Location = new System.Drawing.Point(20, 238);
            this.MultiPatBox.Name = "MultiPatBox";
            this.MultiPatBox.Size = new System.Drawing.Size(143, 16);
            this.MultiPatBox.TabIndex = 14;
            this.MultiPatBox.Text = "患者台帳 多人数モード";
            this.MultiPatBox.UseVisualStyleBackColor = true;
            this.MultiPatBox.CheckedChanged += new System.EventHandler(this.MultiPatBox_CheckedChanged);
            // 
            // MAFormPatListButton1
            // 
            this.MAFormPatListButton1.Location = new System.Drawing.Point(5, 120);
            this.MAFormPatListButton1.Name = "MAFormPatListButton1";
            this.MAFormPatListButton1.Size = new System.Drawing.Size(85, 23);
            this.MAFormPatListButton1.TabIndex = 12;
            this.MAFormPatListButton1.Text = "MA外来";
            this.MAFormPatListButton1.UseVisualStyleBackColor = true;
            this.MAFormPatListButton1.Click += new System.EventHandler(this.MAFormPatListButton1_Click);
            // 
            // SummaryFindButton
            // 
            this.SummaryFindButton.Location = new System.Drawing.Point(5, 176);
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
            // OpeOrderButton
            // 
            this.OpeOrderButton.Location = new System.Drawing.Point(5, 92);
            this.OpeOrderButton.Name = "OpeOrderButton";
            this.OpeOrderButton.Size = new System.Drawing.Size(85, 23);
            this.OpeOrderButton.TabIndex = 9;
            this.OpeOrderButton.Text = "手術指示";
            this.OpeOrderButton.UseVisualStyleBackColor = true;
            this.OpeOrderButton.Click += new System.EventHandler(this.OpeOrderButton_Click);
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
            this.ExitButton.Location = new System.Drawing.Point(95, 204);
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
            this.PatButton.Location = new System.Drawing.Point(95, 8);
            this.PatButton.Name = "PatButton";
            this.PatButton.Size = new System.Drawing.Size(85, 23);
            this.PatButton.TabIndex = 1;
            this.PatButton.Text = "患者台帳";
            this.PatButton.UseVisualStyleBackColor = true;
            this.PatButton.Click += new System.EventHandler(this.PatButton_Click);
            // 
            // ListButton
            // 
            this.ListButton.Location = new System.Drawing.Point(5, 8);
            this.ListButton.Name = "ListButton";
            this.ListButton.Size = new System.Drawing.Size(85, 23);
            this.ListButton.TabIndex = 0;
            this.ListButton.Text = "患者一覧";
            this.ListButton.UseVisualStyleBackColor = true;
            this.ListButton.Click += new System.EventHandler(this.ListButton_Click);
            // 
            // MAFormPatButton1
            // 
            this.MAFormPatButton1.Location = new System.Drawing.Point(95, 120);
            this.MAFormPatButton1.Name = "MAFormPatButton1";
            this.MAFormPatButton1.Size = new System.Drawing.Size(85, 23);
            this.MAFormPatButton1.TabIndex = 13;
            this.MAFormPatButton1.Text = "MAカルテ";
            this.MAFormPatButton1.UseVisualStyleBackColor = true;
            this.MAFormPatButton1.Click += new System.EventHandler(this.MAFormPatButton1_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(185, 258);
            this.Controls.Add(this.RsvListButton);
            this.Controls.Add(this.CanonButton);
            this.Controls.Add(this.NidekButton);
            this.Controls.Add(this.MultiPatBox);
            this.Controls.Add(this.MAFormPatButton1);
            this.Controls.Add(this.MAFormPatListButton1);
            this.Controls.Add(this.SummaryFindButton);
            this.Controls.Add(this.KensaFindButton);
            this.Controls.Add(this.OpeOrderButton);
            this.Controls.Add(this.ExportButton);
            this.Controls.Add(this.PrintButton);
            this.Controls.Add(this.OpeFindButton);
            this.Controls.Add(this.ExitButton);
            this.Controls.Add(this.RsvButton);
            this.Controls.Add(this.PatButton);
            this.Controls.Add(this.ListButton);
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

        private System.Windows.Forms.Button ListButton;
        private System.Windows.Forms.Button PatButton;
        private System.Windows.Forms.Button RsvButton;
        private System.Windows.Forms.Button ExitButton;
        private System.Windows.Forms.Button OpeFindButton;
        private System.Windows.Forms.Button PrintButton;
        private System.Windows.Forms.Button ExportButton;
        private System.Windows.Forms.Button OpeOrderButton;
        private System.Windows.Forms.Button KensaFindButton;
        private System.Windows.Forms.Button SummaryFindButton;
        private System.Windows.Forms.Button MAFormPatListButton1;
        private System.Windows.Forms.CheckBox MultiPatBox;
        private System.Windows.Forms.Button NidekButton;
        private System.Windows.Forms.Button CanonButton;
        private System.Windows.Forms.Button RsvListButton;
        private System.Windows.Forms.Button MAFormPatButton1;
    }
}