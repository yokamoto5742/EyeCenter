namespace EyeCenter
{
    partial class FormSumPlan1
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormSumPlan1));
            this.KensaDate = new System.Windows.Forms.DateTimePicker();
            this.StaffBox = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.KensaPanel = new System.Windows.Forms.Panel();
            this.InputButton = new System.Windows.Forms.Button();
            this.CloseButton = new System.Windows.Forms.Button();
            this.SetPanel = new System.Windows.Forms.FlowLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // KensaDate
            // 
            this.KensaDate.Location = new System.Drawing.Point(54, 10);
            this.KensaDate.Name = "KensaDate";
            this.KensaDate.Size = new System.Drawing.Size(109, 19);
            this.KensaDate.TabIndex = 0;
            // 
            // StaffBox
            // 
            this.StaffBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.StaffBox.FormattingEnabled = true;
            this.StaffBox.Location = new System.Drawing.Point(53, 34);
            this.StaffBox.Name = "StaffBox";
            this.StaffBox.Size = new System.Drawing.Size(135, 20);
            this.StaffBox.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "予定日";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 39);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "担当者";
            // 
            // KensaPanel
            // 
            this.KensaPanel.AutoScroll = true;
            this.KensaPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.KensaPanel.Location = new System.Drawing.Point(6, 127);
            this.KensaPanel.Name = "KensaPanel";
            this.KensaPanel.Size = new System.Drawing.Size(280, 400);
            this.KensaPanel.TabIndex = 4;
            // 
            // InputButton
            // 
            this.InputButton.Location = new System.Drawing.Point(209, 32);
            this.InputButton.Name = "InputButton";
            this.InputButton.Size = new System.Drawing.Size(75, 23);
            this.InputButton.TabIndex = 5;
            this.InputButton.Text = "入力";
            this.InputButton.UseVisualStyleBackColor = true;
            this.InputButton.Click += new System.EventHandler(this.InputButton_Click);
            // 
            // CloseButton
            // 
            this.CloseButton.Location = new System.Drawing.Point(209, 7);
            this.CloseButton.Name = "CloseButton";
            this.CloseButton.Size = new System.Drawing.Size(75, 23);
            this.CloseButton.TabIndex = 6;
            this.CloseButton.Text = "閉じる";
            this.CloseButton.UseVisualStyleBackColor = true;
            this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
            // 
            // SetPanel
            // 
            this.SetPanel.AutoScroll = true;
            this.SetPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.SetPanel.Location = new System.Drawing.Point(54, 61);
            this.SetPanel.Name = "SetPanel";
            this.SetPanel.Size = new System.Drawing.Size(229, 60);
            this.SetPanel.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(30, 12);
            this.label3.TabIndex = 8;
            this.label3.Text = "セット";
            // 
            // FormSumPlan1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(292, 533);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.SetPanel);
            this.Controls.Add(this.CloseButton);
            this.Controls.Add(this.InputButton);
            this.Controls.Add(this.KensaPanel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.StaffBox);
            this.Controls.Add(this.KensaDate);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormSumPlan1";
            this.Text = "検査予定入力";
            this.Load += new System.EventHandler(this.FormSumPlan1_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DateTimePicker KensaDate;
        private System.Windows.Forms.ComboBox StaffBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel KensaPanel;
        private System.Windows.Forms.Button InputButton;
        private System.Windows.Forms.Button CloseButton;
        private System.Windows.Forms.FlowLayoutPanel SetPanel;
        private System.Windows.Forms.Label label3;
    }
}