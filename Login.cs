namespace ADDMS2
{
    public partial class fLogin : Form
    {
        public fLogin()
        {
            InitializeComponent();
            AddDbSettingsButton();
            
            // Cập nhật thông tin tài khoản mẫu hiển thị trên giao diện
            label6.Text = "admin / admin";
            label8.Text = "operator / operator";
        }

        private void AddDbSettingsButton()
        {
            var btnDb = new System.Windows.Forms.Button
            {
                Text = "⚙ Cấu hình DB",
                Location = new System.Drawing.Point(320, 520),
                Size = new System.Drawing.Size(110, 30),
                BackColor = System.Drawing.Color.LightGray,
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                Font = new System.Drawing.Font("Segoe UI", 8.5F)
            };
            btnDb.FlatAppearance.BorderSize = 0;
            btnDb.Click += (s, e) =>
            {
                using (var settingsForm = new fDbSettings())
                {
                    settingsForm.ShowDialog();
                }
            };
            this.Controls.Add(btnDb);
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
            // XÁC THỰC QUA DATABASE
            // =========================
            try
            {
                string sql = "SELECT FullName, Role, Status FROM Users WHERE Username = @u AND Password = @p";
                var parameters = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "@u", username },
                    { "@p", password }
                };

                System.Data.DataTable dt = DbHelper.ExecuteQuery(sql, parameters);
                if (dt.Rows.Count > 0)
                {
                    System.Data.DataRow row = dt.Rows[0];
                    int status = Convert.ToInt32(row["Status"]);
                    if (status == 0)
                    {
                        MessageBox.Show("Tài khoản đã bị khóa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string fullName = row["FullName"].ToString() ?? "";
                    int role = Convert.ToInt32(row["Role"]); // 0 = Admin, 1 = Operator

                    MessageBox.Show($"Đăng nhập thành công! Xin chào {fullName}", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    MainForm f = new MainForm();
                    f.Show();
                    this.Hide();
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi kết nối cơ sở dữ liệu: {ex.Message}\nVui lòng kiểm tra lại cấu hình kết nối SQL Server!", "Lỗi Kết Nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                
                // Mở tự động màn hình cấu hình
                using (var settingsForm = new fDbSettings())
                {
                    settingsForm.ShowDialog();
                }
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
