using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GMap.NET.WindowsForms;

namespace ADDMS2
{
    public partial class MainForm : Form
    {

        private void LoadUC(UserControl uc)
        {
            foreach (Control ctrl in panelMain.Controls)
            {
                ctrl.Dispose();
            }
            panelMain.Controls.Clear();

            uc.Dock = DockStyle.Fill;

            panelMain.Controls.Add(uc);
        }
        public MainForm()
        {
            InitializeComponent();

            lblU.Text = "Admin";
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            LoadUC(new UC_Dashboard());
        }
        private void btnDash_Click(object sender, EventArgs e)
        {
            LoadUC(new UC_Dashboard());
        }

        private void btnQuanly_Click(object sender, EventArgs e)
        {
            LoadUC(new UC_UAV());
        }

        private void btnVattu_Click(object sender, EventArgs e)
        {
            LoadUC(new UC_Vattu());
        }
        private void btnBaocao_Click(object sender, EventArgs e)
        {
            LoadUC(new UC_Baocao());
        }
        private void btnTheodoi_Click(object sender, EventArgs e)
        {
            LoadUC(new UC_TheodoiUAV());
        }
        private void btnLenh_Click(object sender, EventArgs e)
        {
            LoadUC(new UC_lenh());
        }

    }
}
