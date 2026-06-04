using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Net.Http;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;

namespace ADDMS2
{
    public partial class UC_lenh : UserControl
    {
        private class DroneInfo
        {
            public string Ma { get; set; } = "";
            public string Ten { get; set; } = "";
            public double TaiTrong { get; set; }
            public string TrangThai { get; set; } = "";
        }

        private class VatTuInfo
        {
            public string Ma { get; set; } = "";
            public string Ten { get; set; } = "";
            public double TrongLuong { get; set; }
            public int TonKho { get; set; }
        }

        private class ChiTietLenh
        {
            public string TenVatTu { get; set; } = "";
            public int SoLuong { get; set; }
            public double TrongLuong { get; set; }
        }

        private readonly List<DroneInfo> dsDrone = new()
        {
            new DroneInfo { Ma = "Drone001", Ten = "DJI Matrice 300", TaiTrong = 20, TrangThai = "san_sang" },
            new DroneInfo { Ma = "Drone004", Ten = "Parrot Anafi",    TaiTrong = 8,  TrangThai = "san_sang" },
            new DroneInfo { Ma = "Drone002", Ten = "DJI Mavic 3",     TaiTrong = 10, TrangThai = "dang_bay" },
            new DroneInfo { Ma = "Drone003", Ten = "Autel EVO II",    TaiTrong = 15, TrangThai = "bao_tri"  },
        };

        private readonly List<VatTuInfo> dsVatTu = new()
        {
            new VatTuInfo { Ma = "SUP001", Ten = "Nước suối",      TrongLuong = 0.5,   TonKho = 1000 },
            new VatTuInfo { Ma = "SUP002", Ten = "Mì tôm",         TrongLuong = 0.075, TonKho = 500  },
            new VatTuInfo { Ma = "SUP006", Ten = "Pin AA",         TrongLuong = 0.05,  TonKho = 400  },
            new VatTuInfo { Ma = "SUP004", Ten = "Thuốc men",      TrongLuong = 0.1,   TonKho = 300  },
            new VatTuInfo { Ma = "SUP003", Ten = "Bông băng y tế", TrongLuong = 0.2,   TonKho = 200  },
            new VatTuInfo { Ma = "SUP005", Ten = "Đèn pin",        TrongLuong = 0.3,   TonKho = 150  },
        };

        private readonly List<ChiTietLenh> dsChiTiet = new();

        private GMapOverlay? markersOverlay;
        public PointLatLng SelectedLocation { get; private set; }
        public string? SelectedAddress { get; private set; }
        private DroneInfo? selectedDrone;

        public UC_lenh()
        {
            InitializeComponent();
            this.Load += UC_lenh_Load;
        }

        private void UC_lenh_Load(object? sender, EventArgs e)
        {
            InitMap();
            LoadDrone();
            LoadVatTu();
            StyleTable();
            RefreshTable();
        }

        // ── Bản đồ ───────────────────────────────────────────────────
        private void InitMap()
        {
            try
            {
                GMaps.Instance.Mode = AccessMode.ServerAndCache;
                GMapProvider.WebProxy = WebRequest.DefaultWebProxy;
                GMapProvider.WebProxy.Credentials = CredentialCache.DefaultCredentials;
                GMapProvider.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36";

                // OpenStreetMap — ổn định, không lag, không cần key
                gMap1.MapProvider = GMapProviders.GoogleMap;         // Google
                gMap1.Position = new PointLatLng(10.7769, 106.7009);
                gMap1.MinZoom = 5;
                gMap1.MaxZoom = 18;
                gMap1.Zoom = 13;
                gMap1.ShowCenter = false;
                gMap1.DragButton = MouseButtons.Left;
                gMap1.RetryLoadTile = 2;
                gMap1.MouseWheelZoomType = MouseWheelZoomType.MousePositionAndCenter;
                gMap1.MouseWheelZoomEnabled = true;
                // Chặn scroll của form cha khi đang dùng map
                gMap1.MouseEnter += (s, e) => { gMap1.Focus(); };
                gMap1.MouseWheel += (s, e) =>
                {
                    ((HandledMouseEventArgs)e).Handled = true;
                };

                // Cache tile để giảm lag
                gMap1.Manager.Mode = AccessMode.ServerAndCache;

                markersOverlay = new GMapOverlay("markers");
                gMap1.Overlays.Add(markersOverlay);
                gMap1.OnMapClick += GMap1_OnMapClick;

                // Force load
                gMap1.ReloadMap();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo bản đồ: " + ex.Message);
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            // Chặn scroll của UserControl khi chuột đang trên map
            if (gMap1.ClientRectangle.Contains(gMap1.PointToClient(MousePosition)))
                return;
            base.OnMouseWheel(e);
        }

        // ── Drone ─────────────────────────────────────────────────────
        private void LoadDrone()
        {
            cboDrone.Items.Clear();
            cboDrone.Items.Add(new ComboItem("--Chọn Drone--", null));

            foreach (var d in dsDrone.Where(d => d.TrangThai == "san_sang"))
                cboDrone.Items.Add(new ComboItem($"{d.Ma} - {d.Ten}  |  Tải: {d.TaiTrong} kg", d));

            cboDrone.DisplayMember = "Label";
            cboDrone.SelectedIndex = 0;
            lblDroneInfo.Text = "Chỉ hiển thị Drone đã sẵn sàng";
            lblDroneInfo.ForeColor = Color.Gray;
            cboDrone.SelectedIndexChanged += CboDrone_SelectedIndexChanged;
        }

        private void CboDrone_SelectedIndexChanged(object? sender, EventArgs e)
        {
            if (cboDrone.SelectedItem is ComboItem item && item.Value is DroneInfo drone)
            {
                selectedDrone = drone;
                lblDroneInfo.Text = $"Tải trọng tối đa: {drone.TaiTrong} kg";
                lblDroneInfo.ForeColor = Color.FromArgb(30, 130, 80);
            }
            else
            {
                selectedDrone = null;
                lblDroneInfo.Text = "Chỉ hiển thị Drone đã sẵn sàng";
                lblDroneInfo.ForeColor = Color.Gray;
            }
            UpdateTongTrongLuong();
        }

        // ── Vật tư ────────────────────────────────────────────────────
        private void LoadVatTu()
        {
            cboVatTu.Items.Clear();
            cboVatTu.Items.Add(new ComboItem("--Chọn vật tư--", null));
            foreach (var v in dsVatTu)
                cboVatTu.Items.Add(new ComboItem($"{v.Ten}  ({v.TrongLuong} kg/đv)  —  Tồn: {v.TonKho}", v));
            cboVatTu.DisplayMember = "Label";
            cboVatTu.SelectedIndex = 0;
        }

        private void btnThem_Click(object sender, EventArgs e)
        {
            if (cboVatTu.SelectedItem is not ComboItem { Value: VatTuInfo vatTu })
            {
                MessageBox.Show("Vui lòng chọn vật tư!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }
            if (!int.TryParse(txtSoLuong.Text.Trim(), out int sl) || sl < 1)
            {
                MessageBox.Show("Số lượng phải là số nguyên lớn hơn 0!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error); return;
            }
            if (sl > vatTu.TonKho)
            {
                MessageBox.Show($"Số lượng vượt tồn kho!\nTồn kho hiện tại: {vatTu.TonKho}", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error); return;
            }

            var existing = dsChiTiet.FirstOrDefault(x => x.TenVatTu == vatTu.Ten);
            if (existing != null)
            {
                int newSl = existing.SoLuong + sl;
                if (newSl > vatTu.TonKho)
                {
                    MessageBox.Show($"Tổng số lượng vượt tồn kho!\nĐã thêm: {existing.SoLuong}", "Lỗi",
                        MessageBoxButtons.OK, MessageBoxIcon.Error); return;
                }
                existing.SoLuong = newSl;
                existing.TrongLuong = Math.Round(newSl * vatTu.TrongLuong, 3);
            }
            else
            {
                dsChiTiet.Add(new ChiTietLenh
                {
                    TenVatTu = vatTu.Ten,
                    SoLuong = sl,
                    TrongLuong = Math.Round(sl * vatTu.TrongLuong, 3)
                });
            }

            RefreshTable();
            UpdateTongTrongLuong();
            txtSoLuong.Text = "";
            cboVatTu.SelectedIndex = 0;
        }

        // ── Bảng ──────────────────────────────────────────────────────
        private void StyleTable()
        {
            dgvVatTu.BorderStyle = BorderStyle.None;
            dgvVatTu.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
            dgvVatTu.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
            dgvVatTu.GridColor = Color.FromArgb(230, 230, 238);
            dgvVatTu.BackgroundColor = Color.White;
            dgvVatTu.RowHeadersVisible = false;
            dgvVatTu.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvVatTu.AllowUserToAddRows = false;
            dgvVatTu.AllowUserToResizeRows = false;
            dgvVatTu.ReadOnly = true;
            dgvVatTu.Font = new Font("Segoe UI", 10f);
            dgvVatTu.EnableHeadersVisualStyles = false;

            // Header
            dgvVatTu.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(248, 248, 252);
            dgvVatTu.ColumnHeadersDefaultCellStyle.ForeColor = Color.FromArgb(60, 60, 90);
            dgvVatTu.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10f, FontStyle.Bold);
            dgvVatTu.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dgvVatTu.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(248, 248, 252);
            dgvVatTu.ColumnHeadersHeight = 44;

            // Row
            dgvVatTu.DefaultCellStyle.BackColor = Color.White;
            dgvVatTu.DefaultCellStyle.ForeColor = Color.FromArgb(40, 40, 60);
            dgvVatTu.DefaultCellStyle.SelectionBackColor = Color.FromArgb(237, 240, 255);
            dgvVatTu.DefaultCellStyle.SelectionForeColor = Color.FromArgb(40, 40, 60);
            dgvVatTu.DefaultCellStyle.Padding = new Padding(10, 0, 0, 0);
            dgvVatTu.RowTemplate.Height = 44;
            dgvVatTu.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 250, 254);

            // Cột Xóa
            dgvVatTu.Columns["colXoa"].DefaultCellStyle.ForeColor = Color.FromArgb(210, 50, 50);
            dgvVatTu.Columns["colXoa"].DefaultCellStyle.Font = new Font("Segoe UI", 14f);
            dgvVatTu.Columns["colXoa"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgvVatTu.Columns["colXoa"].DefaultCellStyle.SelectionForeColor = Color.FromArgb(210, 50, 50);
            dgvVatTu.Columns["colXoa"].DefaultCellStyle.SelectionBackColor = Color.FromArgb(237, 240, 255);

            // Độ rộng
            dgvVatTu.Columns["colSTT"].Width = 55;
            dgvVatTu.Columns["colTen"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
            dgvVatTu.Columns["colTen"].MinimumWidth = 150;
            dgvVatTu.Columns["colSL"].Width = 100;
            dgvVatTu.Columns["colTL"].Width = 130;
            dgvVatTu.Columns["colXoa"].Width = 70;
            dgvVatTu.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
        }

        private void RefreshTable()
        {
            dgvVatTu.Rows.Clear();
            for (int i = 0; i < dsChiTiet.Count; i++)
            {
                var ct = dsChiTiet[i];
                int idx = dgvVatTu.Rows.Add(i + 1, ct.TenVatTu, ct.SoLuong, $"{ct.TrongLuong:F3}", "🗑");
            }
        }

        private void dgvVatTu_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 4) return;
            dsChiTiet.RemoveAt(e.RowIndex);
            RefreshTable();
            UpdateTongTrongLuong();
        }

        private void UpdateTongTrongLuong()
        {
            double tong = dsChiTiet.Sum(x => x.TrongLuong);
            lblTongTL.Text = $"{tong:F2} kg";

            if (selectedDrone != null)
            {
                double con = selectedDrone.TaiTrong - tong;
                if (tong > selectedDrone.TaiTrong)
                {
                    lblTaiTrongCon.Text = $"⚠ Vượt tải trọng! ({Math.Abs(con):F2} kg)";
                    lblTaiTrongCon.ForeColor = Color.FromArgb(200, 50, 50);
                }
                else
                {
                    lblTaiTrongCon.Text = $"✓ An toàn  (Còn {con:F2} kg)";
                    lblTaiTrongCon.ForeColor = Color.FromArgb(30, 130, 80);
                }
            }
            else
            {
                lblTaiTrongCon.Text = "";
            }
        }

        // ── Lưu lệnh ──────────────────────────────────────────────────
        private void btnLuuLenh_Click(object sender, EventArgs e)
        {
            if (selectedDrone == null)
            {
                MessageBox.Show("Vui lòng chọn Drone!", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }
            if (string.IsNullOrEmpty(SelectedAddress))
            {
                MessageBox.Show("Vui lòng chọn điểm đến trên bản đồ!", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }
            if (dsChiTiet.Count == 0)
            {
                MessageBox.Show("Vui lòng thêm ít nhất 1 vật tư!", "Thiếu thông tin",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning); return;
            }

            double tong = dsChiTiet.Sum(x => x.TrongLuong);
            if (tong > selectedDrone.TaiTrong)
            {
                MessageBox.Show("Tổng trọng lượng vượt tải trọng Drone!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error); return;
            }

            var sb = new StringBuilder();
            sb.AppendLine("✅ Lệnh cứu trợ đã lưu thành công!");
            sb.AppendLine($"Drone      : {selectedDrone.Ma} - {selectedDrone.Ten}");
            sb.AppendLine($"Điểm đến   : {SelectedAddress}");
            sb.AppendLine($"GPS        : {SelectedLocation.Lat:F6}, {SelectedLocation.Lng:F6}");
            sb.AppendLine($"Tổng TL    : {tong:F2} kg / {selectedDrone.TaiTrong} kg");
            MessageBox.Show(sb.ToString(), "Lưu thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Sự kiện bản đồ ────────────────────────────────────────────
        private async void GMap1_OnMapClick(PointLatLng point, MouseEventArgs e)
        {
            SelectedLocation = point;
            markersOverlay?.Markers.Clear();
            markersOverlay?.Markers.Add(new GMarkerGoogle(point, GMarkerGoogleType.red_pushpin));
            lblGPS.Text = $"GPS: {point.Lat:F6}, {point.Lng:F6}";
            lblMap.Text = "Đang tìm địa chỉ...";
            string addr = await GetAddressAsync(point.Lat, point.Lng);
            SelectedAddress = addr;
            lblMap.Text = $"Địa chỉ: {addr}";
        }

        private async Task<string> GetAddressAsync(double lat, double lng)
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                string url = $"https://api.bigdatacloud.net/data/reverse-geocode-client?latitude={lat}&longitude={lng}&localityLanguage=vi";
                var response = await client.GetStringAsync(url);
                var json = System.Text.Json.JsonDocument.Parse(response);
                var root = json.RootElement;

                string road = root.TryGetProperty("locality", out var rd) ? rd.GetString() ?? "" : "";
                string district = root.TryGetProperty("city", out var dt) ? dt.GetString() ?? "" : "";
                string province = root.TryGetProperty("principalSubdivision", out var pv) ? pv.GetString() ?? "" : "";
                string country = root.TryGetProperty("countryName", out var cn) ? cn.GetString() ?? "" : "";

                var parts = new[] { road, district, province, country }.Where(p => !string.IsNullOrEmpty(p));
                return string.Join(", ", parts);
            }
            catch (Exception ex) { return $"Lỗi: {ex.Message}"; }
        }

        private class ComboItem
        {
            public string Label { get; }
            public object? Value { get; }
            public ComboItem(string label, object? value) { Label = label; Value = value; }
            public override string ToString() => Label;
        }
    }
}