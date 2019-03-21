namespace NI4SLCB {
    partial class WaitForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.progressBar_searching = new System.Windows.Forms.ProgressBar();
            this.label_searching = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressBar_searching
            // 
            this.progressBar_searching.Location = new System.Drawing.Point(16, 32);
            this.progressBar_searching.MarqueeAnimationSpeed = 10;
            this.progressBar_searching.Name = "progressBar_searching";
            this.progressBar_searching.Size = new System.Drawing.Size(326, 23);
            this.progressBar_searching.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            this.progressBar_searching.TabIndex = 0;
            // 
            // label_searching
            // 
            this.label_searching.AutoSize = true;
            this.label_searching.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_searching.Location = new System.Drawing.Point(12, 9);
            this.label_searching.Name = "label_searching";
            this.label_searching.Size = new System.Drawing.Size(229, 20);
            this.label_searching.TabIndex = 1;
            this.label_searching.Text = "Searching for Nanoleaf devices";
            // 
            // WaitForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(354, 71);
            this.ControlBox = false;
            this.Controls.Add(this.label_searching);
            this.Controls.Add(this.progressBar_searching);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WaitForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Please wait ...";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar_searching;
        private System.Windows.Forms.Label label_searching;
    }
}