namespace NI4SLCB {
    partial class DeviceChooser {
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
            this.comboBox_device = new System.Windows.Forms.ComboBox();
            this.button_ok = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // comboBox_device
            // 
            this.comboBox_device.FormattingEnabled = true;
            this.comboBox_device.Location = new System.Drawing.Point(12, 12);
            this.comboBox_device.Name = "comboBox_device";
            this.comboBox_device.Size = new System.Drawing.Size(330, 21);
            this.comboBox_device.TabIndex = 0;
            // 
            // button_ok
            // 
            this.button_ok.Location = new System.Drawing.Point(266, 36);
            this.button_ok.Name = "button_ok";
            this.button_ok.Size = new System.Drawing.Size(75, 23);
            this.button_ok.TabIndex = 1;
            this.button_ok.Text = "Continue";
            this.button_ok.UseVisualStyleBackColor = true;
            this.button_ok.Click += new System.EventHandler(this.Button_ok_Click);
            // 
            // DeviceChooser
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(354, 71);
            this.ControlBox = false;
            this.Controls.Add(this.button_ok);
            this.Controls.Add(this.comboBox_device);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DeviceChooser";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Choose your device";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox_device;
        private System.Windows.Forms.Button button_ok;
    }
}