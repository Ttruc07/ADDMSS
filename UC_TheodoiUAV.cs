using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADDMS2
{
    public partial class UC_TheodoiUAV : UserControl
    {
        private UAV_giaohangcuutro.Form1? _embeddedForm;

        public UC_TheodoiUAV()
        {
            InitializeComponent();
            this.Load += UC_TheodoiUAV_Load;
            this.Disposed += UC_TheodoiUAV_Disposed;

            // Load ảnh xem trước khi ở chế độ Designer của Visual Studio
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || 
                System.Diagnostics.Process.GetCurrentProcess().ProcessName.Equals("devenv", StringComparison.OrdinalIgnoreCase))
            {
                LoadDesignPreview();
            }
        }

        private void LoadDesignPreview([System.Runtime.CompilerServices.CallerFilePath] string sourceFilePath = "")
        {
            try
            {
                if (!string.IsNullOrEmpty(sourceFilePath))
                {
                    string projectDir = System.IO.Path.GetDirectoryName(sourceFilePath) ?? "";
                    string filePath = System.IO.Path.Combine(projectDir, "gcs_preview.png");
                    if (System.IO.File.Exists(filePath))
                    {
                        this.BackgroundImage = Image.FromFile(filePath);
                        this.BackgroundImageLayout = ImageLayout.Stretch;
                    }
                }
            }
            catch
            {
                // Bỏ qua để không crash Designer
            }
        }

        private void UC_TheodoiUAV_Load(object? sender, EventArgs e)
        {
            // Xóa ảnh nền xem trước tĩnh khi chạy ứng dụng thực tế
            this.BackgroundImage = null;
            
            try
            {
                _embeddedForm = new UAV_giaohangcuutro.Form1();
                _embeddedForm.TopLevel = false;
                _embeddedForm.FormBorderStyle = FormBorderStyle.None;
                _embeddedForm.Dock = DockStyle.Fill;
                
                this.Controls.Add(_embeddedForm);
                _embeddedForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi nhúng giao diện UAV: {ex.Message}", "Lỗi Tích hợp", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UC_TheodoiUAV_Disposed(object? sender, EventArgs e)
        {
            if (_embeddedForm != null && !_embeddedForm.IsDisposed)
            {
                _embeddedForm.Close();
                _embeddedForm.Dispose();
                _embeddedForm = null;
            }
        }
    }
}
