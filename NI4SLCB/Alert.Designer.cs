namespace NI4SLCB {
    partial class Alert {
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
            this.label_alert = new System.Windows.Forms.Label();
            this.button_close = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label_alert
            // 
            this.label_alert.AutoEllipsis = true;
            this.label_alert.AutoSize = true;
            this.label_alert.Dock = System.Windows.Forms.DockStyle.Top;
            this.label_alert.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_alert.Location = new System.Drawing.Point(0, 0);
            this.label_alert.Name = "label_alert";
            this.label_alert.Padding = new System.Windows.Forms.Padding(10, 10, 10, 50);
            this.label_alert.Size = new System.Drawing.Size(54, 76);
            this.label_alert.TabIndex = 0;
            this.label_alert.Text = "Text";
            // 
            // button_close
            // 
            this.button_close.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button_close.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_close.Location = new System.Drawing.Point(196, 49);
            this.button_close.Name = "button_close";
            this.button_close.Size = new System.Drawing.Size(75, 23);
            this.button_close.TabIndex = 2;
            this.button_close.Text = "Close";
            this.button_close.UseVisualStyleBackColor = true;
            this.button_close.Click += new System.EventHandler(this.Button_close_Click_1);
            // 
            // Alert
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.button_close;
            this.ClientSize = new System.Drawing.Size(464, 84);
            this.ControlBox = false;
            this.Controls.Add(this.button_close);
            this.Controls.Add(this.label_alert);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Alert";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Alert";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label_alert;
        private System.Windows.Forms.Button button_close;
    }
}