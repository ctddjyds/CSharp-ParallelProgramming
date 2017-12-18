namespace Listing9_4
{
    partial class MainForm
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
            this.butConcatTextFiles = new System.Windows.Forms.Button();
            this.pgbFiles = new System.Windows.Forms.ProgressBar();
            this.txtAllText = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // butConcatTextFiles
            // 
            this.butConcatTextFiles.Location = new System.Drawing.Point(13, 13);
            this.butConcatTextFiles.Name = "butConcatTextFiles";
            this.butConcatTextFiles.Size = new System.Drawing.Size(162, 38);
            this.butConcatTextFiles.TabIndex = 0;
            this.butConcatTextFiles.Text = "Concatenate Text Files";
            this.butConcatTextFiles.UseVisualStyleBackColor = true;
            this.butConcatTextFiles.Click += new System.EventHandler(this.butConcatTextFiles_Click);
            // 
            // pgbFiles
            // 
            this.pgbFiles.Location = new System.Drawing.Point(12, 293);
            this.pgbFiles.Name = "pgbFiles";
            this.pgbFiles.Size = new System.Drawing.Size(495, 20);
            this.pgbFiles.TabIndex = 1;
            // 
            // txtAllText
            // 
            this.txtAllText.Location = new System.Drawing.Point(12, 57);
            this.txtAllText.Multiline = true;
            this.txtAllText.Name = "txtAllText";
            this.txtAllText.Size = new System.Drawing.Size(495, 230);
            this.txtAllText.TabIndex = 2;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(519, 325);
            this.Controls.Add(this.txtAllText);
            this.Controls.Add(this.pgbFiles);
            this.Controls.Add(this.butConcatTextFiles);
            this.Name = "MainForm";
            this.Text = "Performing Concurrent Read Operations";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button butConcatTextFiles;
        private System.Windows.Forms.ProgressBar pgbFiles;
        private System.Windows.Forms.TextBox txtAllText;
    }
}

