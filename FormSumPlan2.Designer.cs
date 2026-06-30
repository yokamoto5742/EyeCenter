namespace EyeCenter
{
    partial class FormSumPlan2
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSumPlan2));
            this.CloseButton = new System.Windows.Forms.Button();
            this.InputButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.StaffBox = new System.Windows.Forms.ComboBox();
            this.KensaDate = new System.Windows.Forms.DateTimePicker();
            this.ClearButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(134, 65);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(60, 23);
            this.CloseButton.TabIndex = 12;
            this.CloseButton.Text = "閉じる";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // InputButton
            // 
            this.InputButton.Location = new System.Drawing.Point(7, 65);
            this.InputButton.Name = "InputButton";
            this.InputButton.Size = new System.Drawing.Size(60, 23);
            this.InputButton.TabIndex = 11;
            this.InputButton.Text = "入力";
            this.InputButton.UseVisualStyleBackColor = true;
            this.InputButton.Click += new System.EventHandler(this.InputButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 10;
            this.label2.Text = "担当者";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 9;
            this.label1.Text = "予定日";
            // 
            // StaffBox
            // 
            this.StaffBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.StaffBox.FormattingEnabled = true;
            this.StaffBox.Location = new System.Drawing.Point(53, 34);
            this.StaffBox.Name = "StaffBox";
            this.StaffBox.Size = new System.Drawing.Size(135, 20);
            this.StaffBox.TabIndex = 8;
            // 
            // KensaDate
            // 
            this.KensaDate.Location = new System.Drawing.Point(54, 10);
            this.KensaDate.Name = "KensaDate";
            this.KensaDate.Size = new System.Drawing.Size(109, 19);
            this.KensaDate.TabIndex = 7;
            // 
            // ClearButton
            // 
            this.ClearButton.Location = new System.Drawing.Point(71, 65);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = new System.Drawing.Size(60, 23);
            this.ClearButton.TabIndex = 13;
            this.ClearButton.Text = "クリア";
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // FormSumPlan2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(202, 93);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.InputButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.StaffBox);
            this.Controls.Add(this.KensaDate);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormSumPlan2";
            this.Text = "検査予定入力";
            this.Load += new System.EventHandler(this.FormSumPlan2_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.Button InputButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox StaffBox;
        private System.Windows.Forms.DateTimePicker KensaDate;
        private System.Windows.Forms.Button ClearButton;
    }
}