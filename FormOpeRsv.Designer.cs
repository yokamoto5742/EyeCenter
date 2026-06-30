namespace EyeCenter
{
    partial class FormOpeRsv
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormOpeRsv));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.FileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileCloseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewUpdateMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolPatMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolOpeFindMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.WindowMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.WindowHorizontalMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.WindowVerticalMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.PtIdBox = new System.Windows.Forms.TextBox();
            this.PtInfoBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenuItem,
            this.ViewMenuItem,
            this.ToolMenuItem,
            this.WindowMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1016, 26);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // FileMenuItem
            // 
            this.FileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileCloseMenuItem,
            this.FileExitMenuItem});
            this.FileMenuItem.Name = "FileMenuItem";
            this.FileMenuItem.Size = new System.Drawing.Size(68, 22);
            this.FileMenuItem.Text = "ファイル";
            // 
            // FileCloseMenuItem
            // 
            this.FileCloseMenuItem.Name = "FileCloseMenuItem";
            this.FileCloseMenuItem.Size = new System.Drawing.Size(112, 22);
            this.FileCloseMenuItem.Text = "閉じる";
            this.FileCloseMenuItem.Click += new System.EventHandler(this.FileCloseMenuItem_Click);
            // 
            // FileExitMenuItem
            // 
            this.FileExitMenuItem.Name = "FileExitMenuItem";
            this.FileExitMenuItem.Size = new System.Drawing.Size(112, 22);
            this.FileExitMenuItem.Text = "終了";
            this.FileExitMenuItem.Click += new System.EventHandler(this.FileExitMenuItem_Click);
            // 
            // ViewMenuItem
            // 
            this.ViewMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ViewUpdateMenuItem});
            this.ViewMenuItem.Name = "ViewMenuItem";
            this.ViewMenuItem.Size = new System.Drawing.Size(44, 22);
            this.ViewMenuItem.Text = "表示";
            // 
            // ViewUpdateMenuItem
            // 
            this.ViewUpdateMenuItem.Name = "ViewUpdateMenuItem";
            this.ViewUpdateMenuItem.ShortcutKeys = System.Windows.Forms.Keys.F5;
            this.ViewUpdateMenuItem.Size = new System.Drawing.Size(122, 22);
            this.ViewUpdateMenuItem.Text = "更新";
            this.ViewUpdateMenuItem.Click += new System.EventHandler(this.ViewUpdateMenuItem_Click);
            // 
            // ToolMenuItem
            // 
            this.ToolMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolPatMenuItem,
            this.ToolOpeFindMenuItem});
            this.ToolMenuItem.Name = "ToolMenuItem";
            this.ToolMenuItem.Size = new System.Drawing.Size(56, 22);
            this.ToolMenuItem.Text = "ツール";
            // 
            // ToolPatMenuItem
            // 
            this.ToolPatMenuItem.Name = "ToolPatMenuItem";
            this.ToolPatMenuItem.Size = new System.Drawing.Size(148, 22);
            this.ToolPatMenuItem.Text = "患者台帳";
            this.ToolPatMenuItem.Click += new System.EventHandler(this.ToolPatMenuItem_Click);
            // 
            // ToolOpeFindMenuItem
            // 
            this.ToolOpeFindMenuItem.Name = "ToolOpeFindMenuItem";
            this.ToolOpeFindMenuItem.Size = new System.Drawing.Size(148, 22);
            this.ToolOpeFindMenuItem.Text = "手術記録検索";
            this.ToolOpeFindMenuItem.Click += new System.EventHandler(this.ToolOpeFindMenuItem_Click);
            // 
            // WindowMenuItem
            // 
            this.WindowMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.WindowHorizontalMenuItem,
            this.WindowVerticalMenuItem});
            this.WindowMenuItem.Name = "WindowMenuItem";
            this.WindowMenuItem.Size = new System.Drawing.Size(80, 22);
            this.WindowMenuItem.Text = "ウィンドウ";
            // 
            // WindowHorizontalMenuItem
            // 
            this.WindowHorizontalMenuItem.Name = "WindowHorizontalMenuItem";
            this.WindowHorizontalMenuItem.Size = new System.Drawing.Size(160, 22);
            this.WindowHorizontalMenuItem.Text = "縦に並べて表示";
            this.WindowHorizontalMenuItem.Click += new System.EventHandler(this.WindowHorizontalMenuItem_Click);
            // 
            // WindowVerticalMenuItem
            // 
            this.WindowVerticalMenuItem.Name = "WindowVerticalMenuItem";
            this.WindowVerticalMenuItem.Size = new System.Drawing.Size(160, 22);
            this.WindowVerticalMenuItem.Text = "横に並べて表示";
            this.WindowVerticalMenuItem.Click += new System.EventHandler(this.WindowVerticalMenuItem_Click);
            // 
            // PtIdBox
            // 
            this.PtIdBox.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.PtIdBox.Location = new System.Drawing.Point(593, 3);
            this.PtIdBox.MaxLength = 9;
            this.PtIdBox.Name = "PtIdBox";
            this.PtIdBox.Size = new System.Drawing.Size(74, 19);
            this.PtIdBox.TabIndex = 3;
            this.PtIdBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.PtIdBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PtIdBox_KeyDown);
            // 
            // PtInfoBox
            // 
            this.PtInfoBox.BackColor = System.Drawing.Color.LightYellow;
            this.PtInfoBox.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.PtInfoBox.Location = new System.Drawing.Point(669, 3);
            this.PtInfoBox.MaxLength = 100;
            this.PtInfoBox.Name = "PtInfoBox";
            this.PtInfoBox.ReadOnly = true;
            this.PtInfoBox.Size = new System.Drawing.Size(297, 19);
            this.PtInfoBox.TabIndex = 6;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(549, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 12);
            this.label1.TabIndex = 8;
            this.label1.Text = "患者ID";
            // 
            // FormOpeRsv
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1016, 741);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PtInfoBox);
            this.Controls.Add(this.PtIdBox);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("ＭＳ Ｐゴシック", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "FormOpeRsv";
            this.Text = "（ ログイン中）";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.Load += new System.EventHandler(this.FormOpeRsv_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem FileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FileCloseMenuItem;
        private System.Windows.Forms.ToolStripMenuItem WindowMenuItem;
        private System.Windows.Forms.ToolStripMenuItem WindowHorizontalMenuItem;
        private System.Windows.Forms.ToolStripMenuItem WindowVerticalMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewUpdateMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToolMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToolPatMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ToolOpeFindMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FileExitMenuItem;
        private System.Windows.Forms.TextBox PtIdBox;
        private System.Windows.Forms.TextBox PtInfoBox;
        private System.Windows.Forms.Label label1;
    }
}