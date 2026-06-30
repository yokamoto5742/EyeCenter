namespace EyeCenter
{
    partial class FormOpeCal
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormOpeCal));
            this.opeCalPanel = new System.Windows.Forms.Panel();
            this.CritDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.label1 = new System.Windows.Forms.Label();
            this.RsvMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.RsvMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.RsvMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.RsvMenuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.PrevWeekButton = new System.Windows.Forms.Button();
            this.NextWeekButton = new System.Windows.Forms.Button();
            this.RsvMenuItem4 = new System.Windows.Forms.ToolStripMenuItem();
            this.RsvMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // opeCalPanel
            // 
            this.opeCalPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.opeCalPanel.AutoScroll = true;
            this.opeCalPanel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.opeCalPanel.Location = new System.Drawing.Point(13, 41);
            this.opeCalPanel.Name = "opeCalPanel";
            this.opeCalPanel.Size = new System.Drawing.Size(437, 396);
            this.opeCalPanel.TabIndex = 0;
            // 
            // CritDateTimePicker
            // 
            this.CritDateTimePicker.Location = new System.Drawing.Point(67, 12);
            this.CritDateTimePicker.Name = "CritDateTimePicker";
            this.CritDateTimePicker.Size = new System.Drawing.Size(112, 19);
            this.CritDateTimePicker.TabIndex = 1;
            this.CritDateTimePicker.ValueChanged += new System.EventHandler(this.CritDateTimePicker_ValueChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(19, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(41, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "基準日";
            // 
            // RsvMenu
            // 
            this.RsvMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.RsvMenuItem1,
            this.RsvMenuItem2,
            this.RsvMenuItem3,
            this.toolStripSeparator1,
            this.RsvMenuItem4});
            this.RsvMenu.Name = "contextMenuStrip1";
            this.RsvMenu.Size = new System.Drawing.Size(153, 120);
            this.RsvMenu.Opening += new System.ComponentModel.CancelEventHandler(this.RsvMenu_Opening);
            // 
            // RsvMenuItem1
            // 
            this.RsvMenuItem1.Name = "RsvMenuItem1";
            this.RsvMenuItem1.Size = new System.Drawing.Size(152, 22);
            this.RsvMenuItem1.Text = "枠設定 診療";
            this.RsvMenuItem1.Click += new System.EventHandler(this.RsvMenuItem1_Click);
            // 
            // RsvMenuItem2
            // 
            this.RsvMenuItem2.Name = "RsvMenuItem2";
            this.RsvMenuItem2.Size = new System.Drawing.Size(152, 22);
            this.RsvMenuItem2.Text = "枠設定 休診";
            this.RsvMenuItem2.Click += new System.EventHandler(this.RsvMenuItem2_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(149, 6);
            // 
            // RsvMenuItem3
            // 
            this.RsvMenuItem3.Name = "RsvMenuItem3";
            this.RsvMenuItem3.Size = new System.Drawing.Size(152, 22);
            this.RsvMenuItem3.Text = "枠設定 削除";
            this.RsvMenuItem3.Click += new System.EventHandler(this.RsvMenuItem3_Click);
            // 
            // PrevWeekButton
            // 
            this.PrevWeekButton.Location = new System.Drawing.Point(192, 10);
            this.PrevWeekButton.Name = "PrevWeekButton";
            this.PrevWeekButton.Size = new System.Drawing.Size(75, 23);
            this.PrevWeekButton.TabIndex = 3;
            this.PrevWeekButton.Text = "1W 戻る";
            this.PrevWeekButton.UseVisualStyleBackColor = true;
            this.PrevWeekButton.Click += new System.EventHandler(this.PrevWeekButton_Click);
            // 
            // NextWeekButton
            // 
            this.NextWeekButton.Location = new System.Drawing.Point(273, 10);
            this.NextWeekButton.Name = "NextWeekButton";
            this.NextWeekButton.Size = new System.Drawing.Size(75, 23);
            this.NextWeekButton.TabIndex = 4;
            this.NextWeekButton.Text = "1W 進む";
            this.NextWeekButton.UseVisualStyleBackColor = true;
            this.NextWeekButton.Click += new System.EventHandler(this.NextWeekButton_Click);
            // 
            // RsvMenuItem4
            // 
            this.RsvMenuItem4.Name = "RsvMenuItem4";
            this.RsvMenuItem4.Size = new System.Drawing.Size(152, 22);
            this.RsvMenuItem4.Text = "予約 追加";
            this.RsvMenuItem4.Click += new System.EventHandler(this.RsvMenuItem4_Click);
            // 
            // FormOpeCal
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 451);
            this.Controls.Add(this.NextWeekButton);
            this.Controls.Add(this.PrevWeekButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CritDateTimePicker);
            this.Controls.Add(this.opeCalPanel);
//            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FormOpeCal";
            this.Load += new System.EventHandler(this.FormOpeCal_Load);
            this.RsvMenu.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel opeCalPanel;
        private System.Windows.Forms.DateTimePicker CritDateTimePicker;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ContextMenuStrip RsvMenu;
        private System.Windows.Forms.ToolStripMenuItem RsvMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem RsvMenuItem2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem RsvMenuItem3;
        private System.Windows.Forms.Button PrevWeekButton;
        private System.Windows.Forms.Button NextWeekButton;
        private System.Windows.Forms.ToolStripMenuItem RsvMenuItem4;

    }
}