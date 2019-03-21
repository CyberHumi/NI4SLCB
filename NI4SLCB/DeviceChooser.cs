using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NI4SLCB {
    public partial class DeviceChooser : Form {
        private int DevNo;
        private MainForm Mf;

        public DeviceChooser(MainForm Mf, int DevNo) {
            this.Mf = Mf;
            this.DevNo = DevNo;
            InitializeComponent();
            ArrayList devs = Mf.GetDisvoveredDevices();
            foreach (string dev in devs) {
                comboBox_device.Items.Add(dev);
            }
        }

        protected override void WndProc(ref Message m) {
            const int WM_NCLBUTTONDOWN = 161;
            const int WM_SYSCOMMAND = 274;
            const int HTCAPTION = 2;
            const int SC_MOVE = 61456;
            if ((m.Msg == WM_SYSCOMMAND) && (m.WParam.ToInt32() == SC_MOVE))
                return;
            if ((m.Msg == WM_NCLBUTTONDOWN) && (m.WParam.ToInt32() == HTCAPTION))
                return;
            base.WndProc(ref m);
        }

        private void Button_ok_Click(object sender, EventArgs e) {
            Mf.SetDeviceLocation(DevNo, comboBox_device.Text);
            Close();
        }
    }
}
