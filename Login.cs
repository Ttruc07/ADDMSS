namespace ADDMS2
{
    public partial class fLogin : Form
    {
        public fLogin()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void guna2Button1_Click(object sender, EventArgs e)
        {


            string username = txtUsername.Text.Trim();



            string password = txtPassword.Text.Trim();



            // =========================

            // KIỂM TRA RỖNG

            // =========================



            if (username == "" || password == "")

            {

                MessageBox.Show(

                    "Vui lòng nhập đầy đủ tài khoản và mật khẩu",

                    "Thông báo",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Warning

                );



                return;

            }



            // =========================

            // ADMIN

            // =========================



            if (username == "admin" && password == "admin123")

            {

                MessageBox.Show(

                    "Đăng nhập Admin thành công",

                    "Success",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Information

                );



                MainForm f = new MainForm();



                f.Show();



                this.Hide();



                return;

            }



            // =========================

            // OPERATOR

            // =========================



            if (username == "operator" && password == "op123")

            {

                MessageBox.Show(

                    "Đăng nhập Operator thành công",

                    "Success",

                    MessageBoxButtons.OK,

                    MessageBoxIcon.Information

                );



                MainForm f = new MainForm();



                f.Show();



                this.Hide();



                return;

            }



            // =========================

            // SAI TÀI KHOẢN

            // =========================



            MessageBox.Show(

                "Sai tài khoản hoặc mật khẩu",

                "Error",

                MessageBoxButtons.OK,

                MessageBoxIcon.Error

            );

        
    }

    }
}
