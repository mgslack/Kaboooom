
namespace Kaboooom
{
    partial class OptionsDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.cbConfirm = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.nudMineCount = new System.Windows.Forms.NumericUpDown();
            this.lblNumBombs = new System.Windows.Forms.Label();
            this.cbRandomBombs = new System.Windows.Forms.CheckBox();
            this.cbVerbose = new System.Windows.Forms.CheckBox();
            this.cbSounds = new System.Windows.Forms.CheckBox();
            this.cbSafeGame = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMineCount)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.cbConfirm);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.nudMineCount);
            this.panel1.Controls.Add(this.lblNumBombs);
            this.panel1.Controls.Add(this.cbRandomBombs);
            this.panel1.Controls.Add(this.cbVerbose);
            this.panel1.Controls.Add(this.cbSounds);
            this.panel1.Controls.Add(this.cbSafeGame);
            this.panel1.Location = new System.Drawing.Point(12, 12);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(288, 191);
            this.panel1.TabIndex = 0;
            // 
            // cbConfirm
            // 
            this.cbConfirm.AutoSize = true;
            this.cbConfirm.Location = new System.Drawing.Point(13, 94);
            this.cbConfirm.Name = "cbConfirm";
            this.cbConfirm.Size = new System.Drawing.Size(139, 21);
            this.cbConfirm.TabIndex = 7;
            this.cbConfirm.Text = "Con&firmations On";
            this.cbConfirm.UseVisualStyleBackColor = true;
            this.cbConfirm.CheckedChanged += new System.EventHandler(this.cbConfirm_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(201, 163);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 17);
            this.label1.TabIndex = 6;
            this.label1.Text = "(10 - 40)";
            this.label1.UseMnemonic = false;
            // 
            // nudMineCount
            // 
            this.nudMineCount.Location = new System.Drawing.Point(150, 161);
            this.nudMineCount.Maximum = new decimal(new int[] {
            40,
            0,
            0,
            0});
            this.nudMineCount.Minimum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudMineCount.Name = "nudMineCount";
            this.nudMineCount.Size = new System.Drawing.Size(45, 22);
            this.nudMineCount.TabIndex = 5;
            this.nudMineCount.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudMineCount.ValueChanged += new System.EventHandler(this.nudMineCount_ValueChanged);
            // 
            // lblNumBombs
            // 
            this.lblNumBombs.AutoSize = true;
            this.lblNumBombs.Location = new System.Drawing.Point(19, 163);
            this.lblNumBombs.Name = "lblNumBombs";
            this.lblNumBombs.Size = new System.Drawing.Size(125, 17);
            this.lblNumBombs.TabIndex = 4;
            this.lblNumBombs.Text = "&Number of Bombs:";
            // 
            // cbRandomBombs
            // 
            this.cbRandomBombs.AutoSize = true;
            this.cbRandomBombs.Location = new System.Drawing.Point(13, 138);
            this.cbRandomBombs.Name = "cbRandomBombs";
            this.cbRandomBombs.Size = new System.Drawing.Size(251, 21);
            this.cbRandomBombs.TabIndex = 3;
            this.cbRandomBombs.Text = "&Random Number of Bombs (mines)";
            this.cbRandomBombs.UseVisualStyleBackColor = true;
            this.cbRandomBombs.CheckedChanged += new System.EventHandler(this.cbRandomBombs_CheckedChanged);
            // 
            // cbVerbose
            // 
            this.cbVerbose.AutoSize = true;
            this.cbVerbose.Location = new System.Drawing.Point(13, 67);
            this.cbVerbose.Name = "cbVerbose";
            this.cbVerbose.Size = new System.Drawing.Size(145, 21);
            this.cbVerbose.TabIndex = 2;
            this.cbVerbose.Text = "&Verbose Evalution";
            this.cbVerbose.UseVisualStyleBackColor = true;
            this.cbVerbose.CheckedChanged += new System.EventHandler(this.cbVerbose_CheckedChanged);
            // 
            // cbSounds
            // 
            this.cbSounds.AutoSize = true;
            this.cbSounds.Location = new System.Drawing.Point(13, 40);
            this.cbSounds.Name = "cbSounds";
            this.cbSounds.Size = new System.Drawing.Size(101, 21);
            this.cbSounds.TabIndex = 1;
            this.cbSounds.Text = "So&unds On";
            this.cbSounds.UseVisualStyleBackColor = true;
            this.cbSounds.CheckedChanged += new System.EventHandler(this.cbSounds_CheckedChanged);
            // 
            // cbSafeGame
            // 
            this.cbSafeGame.AutoSize = true;
            this.cbSafeGame.Location = new System.Drawing.Point(13, 13);
            this.cbSafeGame.Name = "cbSafeGame";
            this.cbSafeGame.Size = new System.Drawing.Size(124, 21);
            this.cbSafeGame.TabIndex = 0;
            this.cbSafeGame.Text = "&Safe Game On";
            this.cbSafeGame.UseVisualStyleBackColor = true;
            this.cbSafeGame.CheckedChanged += new System.EventHandler(this.cbSafeGame_CheckedChanged);
            // 
            // btnOK
            // 
            this.btnOK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.btnOK.Location = new System.Drawing.Point(27, 209);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(84, 34);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "&OK";
            this.btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(117, 209);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(85, 34);
            this.btnCancel.TabIndex = 2;
            this.btnCancel.Text = "&Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 130);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(265, 17);
            this.label2.TabIndex = 8;
            this.label2.Text = "Options below take effect next new game";
            this.label2.UseMnemonic = false;
            // 
            // OptionsDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(310, 251);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "OptionsDlg";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Options";
            this.Load += new System.EventHandler(this.OptionsDlg_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudMineCount)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox cbRandomBombs;
        private System.Windows.Forms.CheckBox cbVerbose;
        private System.Windows.Forms.CheckBox cbSounds;
        private System.Windows.Forms.CheckBox cbSafeGame;
        private System.Windows.Forms.NumericUpDown nudMineCount;
        private System.Windows.Forms.Label lblNumBombs;
        private System.Windows.Forms.CheckBox cbConfirm;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label label2;
    }
}