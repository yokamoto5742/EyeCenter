namespace EyeCenter
{
    partial class FormExport
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormExport));
            this.ExeButton = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.OpeRecordButton = new System.Windows.Forms.RadioButton();
            this.SummaryButton = new System.Windows.Forms.RadioButton();
            this.CloseButton = new System.Windows.Forms.Button();
            this.KensaButton = new System.Windows.Forms.RadioButton();
            this.PatButton = new System.Windows.Forms.RadioButton();
            this.RsvButton = new System.Windows.Forms.RadioButton();
            this.Utf8Check = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // ExeButton
            // 
            this.ExeButton.Location = new System.Drawing.Point(65, 238);
            this.ExeButton.Name = "ExeButton";
            this.ExeButton.Size = new System.Drawing.Size(75, 23);
            this.ExeButton.TabIndex = 0;
            this.ExeButton.Text = "実行";
            this.ExeButton.UseVisualStyleBackColor = true;
            this.ExeButton.Click += new System.EventHandler(this.ExeButton_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Title = "保存先";
            // 
            // OpeRecordButton
            // 
            this.OpeRecordButton.AutoSize = true;
            this.OpeRecordButton.Location = new System.Drawing.Point(65, 35);
            this.OpeRecordButton.Name = "OpeRecordButton";
            this.OpeRecordButton.Size = new System.Drawing.Size(181, 16);
            this.OpeRecordButton.TabIndex = 1;
            this.OpeRecordButton.TabStop = true;
            this.OpeRecordButton.Text = "手術記録（EYE_OPE_RECORD）";
            this.OpeRecordButton.UseVisualStyleBackColor = true;
            // 
            // SummaryButton
            // 
            this.SummaryButton.AutoSize = true;
            this.SummaryButton.Location = new System.Drawing.Point(65, 57);
            this.SummaryButton.Name = "SummaryButton";
            this.SummaryButton.Size = new System.Drawing.Size(142, 16);
            this.SummaryButton.TabIndex = 2;
            this.SummaryButton.TabStop = true;
            this.SummaryButton.Text = "サマリ（EYE_SUMMARY）";
            this.SummaryButton.UseVisualStyleBackColor = true;
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(146, 238);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 3;
            this.CloseButton.Text = "閉じる";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // KensaButton
            // 
            this.KensaButton.AutoSize = true;
            this.KensaButton.Location = new System.Drawing.Point(65, 79);
            this.KensaButton.Name = "KensaButton";
            this.KensaButton.Size = new System.Drawing.Size(121, 16);
            this.KensaButton.TabIndex = 4;
            this.KensaButton.TabStop = true;
            this.KensaButton.Text = "検査（EYE_KENSA）";
            this.KensaButton.UseVisualStyleBackColor = true;
            //
            // PatButton
            //
            this.PatButton.AutoSize = true;
            this.PatButton.Location = new System.Drawing.Point(65, 101);
            this.PatButton.Name = "PatButton";
            this.PatButton.Size = new System.Drawing.Size(121, 16);
            this.PatButton.TabIndex = 5;
            this.PatButton.TabStop = true;
            this.PatButton.Text = "患者（患者マスタ）";
            this.PatButton.UseVisualStyleBackColor = true;
            //
            // RsvButton
            //
            this.RsvButton.AutoSize = true;
            this.RsvButton.Location = new System.Drawing.Point(65, 123);
            this.RsvButton.Name = "RsvButton";
            this.RsvButton.Size = new System.Drawing.Size(141, 16);
            this.RsvButton.TabIndex = 6;
            this.RsvButton.TabStop = true;
            this.RsvButton.Text = "手術予約（EYE_OPE）";
            this.RsvButton.UseVisualStyleBackColor = true;
            //
            // Utf8Check
            //
            this.Utf8Check.AutoSize = true;
            this.Utf8Check.Location = new System.Drawing.Point(65, 160);
            this.Utf8Check.Name = "Utf8Check";
            this.Utf8Check.Size = new System.Drawing.Size(151, 16);
            this.Utf8Check.TabIndex = 7;
            this.Utf8Check.Text = "UTF-8（BOM付き）で出力";
            this.Utf8Check.UseVisualStyleBackColor = true;
            //
            // FormExport
            //
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.Utf8Check);
            this.Controls.Add(this.RsvButton);
            this.Controls.Add(this.PatButton);
            this.Controls.Add(this.KensaButton);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.SummaryButton);
            this.Controls.Add(this.OpeRecordButton);
            this.Controls.Add(this.ExeButton);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormExport";
            this.Text = "エクスポート";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button ExeButton;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.RadioButton OpeRecordButton;
        private System.Windows.Forms.RadioButton SummaryButton;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.RadioButton KensaButton;
        private System.Windows.Forms.RadioButton PatButton;
        private System.Windows.Forms.RadioButton RsvButton;
        private System.Windows.Forms.CheckBox Utf8Check;
    }
}