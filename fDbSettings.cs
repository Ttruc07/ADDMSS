using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Data.SqlClient;

namespace ADDMS2
{
    public class fDbSettings : Form
    {
        private Label lblTitle = null!;
        private Label lblServer = null!;
        private TextBox txtServer = null!;
        private Label lblAuth = null!;
        private ComboBox cboAuth = null!;
        private Label lblUser = null!;
        private TextBox txtUser = null!;
        private Label lblPass = null!;
        private TextBox txtPass = null!;
        
        private Button btnTest = null!;
        private Button btnInit = null!;
        private Button btnSave = null!;
        private RichTextBox txtLog = null!;

        public fDbSettings()
        {
            InitializeCustomComponents();
            LoadCurrentConfig();
        }

        private void InitializeCustomComponents()
        {
            this.Text = "Cấu hình Cơ sở dữ liệu - ADDMS";
            this.Size = new Size(520, 620);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(240, 244, 248);
            this.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular);

            // Title
            lblTitle = new Label
            {
                Text = "CẤU HÌNH KẾT NỐI DATABASE",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(12, 35, 64),
                Location = new Point(20, 15),
                Size = new Size(460, 35),
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblTitle);

            // Server
            lblServer = new Label { Text = "Tên SQL Server (Server Name / IP):", Location = new Point(30, 65), Size = new Size(440, 25) };
            txtServer = new TextBox { Location = new Point(30, 95), Size = new Size(440, 28) };
            this.Controls.Add(lblServer);
            this.Controls.Add(txtServer);

            // Authentication
            lblAuth = new Label { Text = "Kiểu xác thực (Authentication):", Location = new Point(30, 135), Size = new Size(440, 25) };
            cboAuth = new ComboBox
            {
                Location = new Point(30, 165),
                Size = new Size(440, 28),
                DropDownStyle = ComboBoxStyle.DropDownList
            };
            cboAuth.Items.AddRange(new object[] { "Windows Authentication", "SQL Server Authentication" });
            cboAuth.SelectedIndexChanged += cboAuth_SelectedIndexChanged;
            this.Controls.Add(lblAuth);
            this.Controls.Add(cboAuth);

            // Username
            lblUser = new Label { Text = "Tên đăng nhập (Username):", Location = new Point(30, 205), Size = new Size(200, 25), Enabled = false };
            txtUser = new TextBox { Location = new Point(30, 235), Size = new Size(200, 28), Enabled = false };
            this.Controls.Add(lblUser);
            this.Controls.Add(txtUser);

            // Password
            lblPass = new Label { Text = "Mật khẩu (Password):", Location = new Point(270, 205), Size = new Size(200, 25), Enabled = false };
            txtPass = new TextBox { Location = new Point(270, 235), Size = new Size(200, 28), PasswordChar = '*', Enabled = false };
            this.Controls.Add(lblPass);
            this.Controls.Add(txtPass);

            // Buttons
            btnTest = new Button
            {
                Text = "Kiểm tra kết nối",
                Location = new Point(30, 285),
                Size = new Size(135, 38),
                BackColor = Color.FromArgb(0, 102, 204),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnTest.FlatAppearance.BorderSize = 0;
            btnTest.Click += btnTest_Click;
            this.Controls.Add(btnTest);

            btnInit = new Button
            {
                Text = "Khởi tạo Database",
                Location = new Point(175, 285),
                Size = new Size(160, 38),
                BackColor = Color.FromArgb(46, 117, 89),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnInit.FlatAppearance.BorderSize = 0;
            btnInit.Click += btnInit_Click;
            this.Controls.Add(btnInit);

            btnSave = new Button
            {
                Text = "Lưu & Đóng",
                Location = new Point(345, 285),
                Size = new Size(125, 38),
                BackColor = Color.FromArgb(74, 85, 104),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Click += btnSave_Click;
            this.Controls.Add(btnSave);

            // Log Console
            Label lblLog = new Label { Text = "Console Log:", Location = new Point(30, 340), Size = new Size(440, 20) };
            txtLog = new RichTextBox
            {
                Location = new Point(30, 365),
                Size = new Size(440, 195),
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                Font = new Font("Consolas", 9F)
            };
            this.Controls.Add(lblLog);
            this.Controls.Add(txtLog);
        }

        private void LoadCurrentConfig()
        {
            try
            {
                var connStr = DbHelper.ConnectionString;
                if (string.IsNullOrEmpty(connStr))
                {
                    cboAuth.SelectedIndex = 0;
                    txtServer.Text = ".";
                    return;
                }

                var builder = new SqlConnectionStringBuilder(connStr);
                txtServer.Text = builder.DataSource;
                if (builder.IntegratedSecurity)
                {
                    cboAuth.SelectedIndex = 0;
                }
                else
                {
                    cboAuth.SelectedIndex = 1;
                    txtUser.Text = builder.UserID;
                    txtPass.Text = builder.Password;
                }
            }
            catch
            {
                cboAuth.SelectedIndex = 0;
                txtServer.Text = ".";
            }
        }

        private void cboAuth_SelectedIndexChanged(object? sender, EventArgs e)
        {
            bool isSqlAuth = cboAuth.SelectedIndex == 1;
            lblUser.Enabled = isSqlAuth;
            txtUser.Enabled = isSqlAuth;
            lblPass.Enabled = isSqlAuth;
            txtPass.Enabled = isSqlAuth;
        }

        private void Log(string msg)
        {
            if (this.IsDisposed) return;
            if (txtLog.InvokeRequired)
            {
                txtLog.BeginInvoke(new Action(() => Log(msg)));
            }
            else
            {
                txtLog.AppendText($"{DateTime.Now:HH:mm:ss} - {msg}{Environment.NewLine}");
                txtLog.SelectionStart = txtLog.TextLength;
                txtLog.ScrollToCaret();
            }
        }

        private string BuildTestConnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                DataSource = txtServer.Text.Trim(),
                InitialCatalog = "master", // Thử kết nối tới master trước
                TrustServerCertificate = true,
                ConnectTimeout = 5 // Timeout ngắn cho mục đích test
            };

            if (cboAuth.SelectedIndex == 0)
            {
                builder.IntegratedSecurity = true;
            }
            else
            {
                builder.IntegratedSecurity = false;
                builder.UserID = txtUser.Text.Trim();
                builder.Password = txtPass.Text.Trim();
            }
            return builder.ConnectionString;
        }

        private void btnTest_Click(object? sender, EventArgs e)
        {
            string connStr = BuildTestConnectionString();
            Log($"Đang kiểm tra kết nối tới Server '{txtServer.Text.Trim()}'...");
            
            btnTest.Enabled = false;
            System.Threading.Tasks.Task.Run(() =>
            {
                bool success = DbHelper.TestConnection(connStr, out string error);
                this.BeginInvoke(new Action(() =>
                {
                    btnTest.Enabled = true;
                    if (success)
                    {
                        Log("Kết nối tới SQL Server thành công!");
                        MessageBox.Show("Kết nối tới SQL Server thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        Log($"Kết nối thất bại: {error}");
                        MessageBox.Show($"Kết nối thất bại!\nChi tiết: {error}", "Lỗi kết nối", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }));
            });
        }

        private void btnInit_Click(object? sender, EventArgs e)
        {
            string server = txtServer.Text.Trim();
            string authMode = cboAuth.SelectedIndex == 0 ? "Windows" : "SQL";
            string user = txtUser.Text.Trim();
            string pass = txtPass.Text.Trim();

            if (string.IsNullOrEmpty(server))
            {
                MessageBox.Show("Vui lòng nhập tên SQL Server!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnInit.Enabled = false;
            Log("Bắt đầu khởi tạo cơ sở dữ liệu và dữ liệu mẫu...");

            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    DbInitializer.InitializeDatabase(server, authMode, user, pass, Log);
                    this.BeginInvoke(new Action(() =>
                    {
                        btnInit.Enabled = true;
                        Log(">>> KHỞI TẠO HOÀN THÀNH XUẤT SẮC <<<");
                        MessageBox.Show("Khởi tạo và cấu hình cơ sở dữ liệu thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }));
                }
                catch (Exception ex)
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        btnInit.Enabled = true;
                        Log($"LỖI KHỞI TẠO: {ex.Message}");
                        MessageBox.Show($"Khởi tạo cơ sở dữ liệu thất bại!\nChi tiết: {ex.Message}", "Lỗi khởi tạo", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }));
                }
            });
        }

        private void btnSave_Click(object? sender, EventArgs e)
        {
            try
            {
                var builder = new SqlConnectionStringBuilder
                {
                    DataSource = txtServer.Text.Trim(),
                    InitialCatalog = "UAV_Management",
                    TrustServerCertificate = true
                };

                if (cboAuth.SelectedIndex == 0)
                {
                    builder.IntegratedSecurity = true;
                }
                else
                {
                    builder.IntegratedSecurity = false;
                    builder.UserID = txtUser.Text.Trim();
                    builder.Password = txtPass.Text.Trim();
                }

                DbHelper.SaveConnectionString(builder.ConnectionString);
                MessageBox.Show("Đã lưu cấu hình kết nối mới!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi lưu cấu hình: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
