using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NI4SLCB {
    public partial class WaitForm : Form {
        public Action Worker { get; set; }
        public WaitForm(Action Worker) {
            InitializeComponent();
            if (Worker == null)
                throw new ArgumentNullException();
            this.Worker = Worker;
        }

        protected override void OnLoad(EventArgs e) {
            base.OnLoad(e);
            Task.Factory.StartNew(Worker).ContinueWith(t => { this.Close(); }, TaskScheduler.FromCurrentSynchronizationContext());
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
    }
}
