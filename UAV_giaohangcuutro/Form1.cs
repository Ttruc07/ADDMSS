using System;
using System.Drawing;
using System.Windows.Forms;
using UAV_giaohangcuutro.Core.Connection;
using UAV_giaohangcuutro.Core.Mavlink;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.MapProviders;

namespace UAV_giaohangcuutro
{
    public partial class Form1 : Form
    {
        private UdpConnection _udpConnection = null!;
        private MavlinkService _mavlinkService = null!;
        
        // UI Controls
        private Panel _pnlSidebar = null!;
        private TableLayoutPanel _tlpDashboard = null!;
        private RichTextBox _txtLog = null!;
        private GMapControl _gmap = null!;
        private GMapOverlay _markersOverlay = null!;
        private GMarkerGoogle? _droneMarker;
        private GMarkerGoogle? _targetMarker;
        
        // Sidebar controls
        private NumericUpDown numPort = null!;
        private Button btnConnect = null!;
        private Label lblConnStatus = null!;
        private NumericUpDown numTakeoffAlt = null!;
        private ComboBox cbMapProviders = null!;
        private CheckBox chkAutoCenter = null!;
        private CheckBox chkClickToFly = null!;
        private NumericUpDown numTargetAlt = null!;
        
        // Mission controls
        private List<MAVLink.mavlink_mission_item_t> _missionItems = new();
        private Button btnCreateMission = null!;
        private Button btnUploadMission = null!;
        private Button btnStartMission = null!;
        private bool _isUploadingMission = false;
        
        // Dashboard labels
        private Label _lblMode = null!;
        private Label _lblArmStatus = null!;
        private Label _lblBattery = null!;
        private Label _lblAltitude = null!;
        private Label _lblSpeed = null!;
        private Label _lblAttitude = null!;
        private Label _lblHeading = null!;
        private Label _lblGPS = null!;
        private Label _lblGpsFix = null!;
        private Label _lblSatellites = null!;
        private Label _lblDistance = null!;
        private Label _lblHome = null!;
        
        // Map and Route controls
        private GMapOverlay _routesOverlay = null!;
        private GMarkerGoogle? _homeMarker;
        private PointLatLng? _homePos;
        private GMapRoute? _trajectoryRoute;
        private GMapRoute? _missionRoute;
        private List<PointLatLng> _flightPoints = new();

        public Form1()
        {
            InitializeComponent();
            SetupCustomUI();
            InitializeMavlink();
        }

        private void SetupCustomUI()
        {
            // Set UserAgent for GMap.NET globally to avoid 403 Access Blocked on OpenStreetMap
            GMapProvider.UserAgent = "UAV_GCS_Delivery_Rescue_App_v1.0";

            // Main Form Styling
            this.Text = "UAV GCS Delivery & Rescue - Control Panel";
            this.Size = new Size(1100, 720);
            this.MinimumSize = new Size(950, 600);
            this.BackColor = Color.FromArgb(20, 20, 20); // Deep Dark Carbon
            this.ForeColor = Color.FromArgb(240, 240, 240);
            this.Font = new Font("Segoe UI", 9F, FontStyle.Regular);

            // ==========================================
            // 1. SIDEBAR (Bên trái, 280px)
            // ==========================================
            _pnlSidebar = new Panel
            {
                Dock = DockStyle.Left,
                Width = 280,
                BackColor = Color.FromArgb(28, 28, 28), // Dark Charcoal
                Padding = new Padding(10)
            };
            this.Controls.Add(_pnlSidebar);

            // App Header inside Sidebar
            Label lblHeader = new Label
            {
                Text = "UAV GCS STATION",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(99, 102, 241), // Neon Indigo Accent
                Dock = DockStyle.Top,
                Height = 45,
                TextAlign = ContentAlignment.MiddleCenter
            };
            _pnlSidebar.Controls.Add(lblHeader);

            // Divider Line
            Panel divider1 = new Panel
            {
                Dock = DockStyle.Top,
                Height = 2,
                BackColor = Color.FromArgb(48, 48, 48),
                Margin = new Padding(0, 5, 0, 5)
            };
            _pnlSidebar.Controls.Add(divider1);

            // Scrollable Flow Panel inside Sidebar for DPI scaling immunity
            FlowLayoutPanel flpSidebar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true,
                BackColor = Color.Transparent,
                Padding = new Padding(10, 5, 10, 0),
                Margin = new Padding(0)
            };
            _pnlSidebar.Controls.Add(flpSidebar);

            // Group 1: Connection Settings
            GroupBox gpConnection = new GroupBox
            {
                Text = "CONNECTION SETTINGS",
                ForeColor = Color.FromArgb(160, 160, 160),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10, 30, 10, 10),
                Margin = new Padding(0, 0, 0, 15),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            flpSidebar.Controls.Add(gpConnection);

            TableLayoutPanel tlpConn = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            tlpConn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tlpConn.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            for (int i = 0; i < 3; i++)
            {
                tlpConn.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            gpConnection.Controls.Add(tlpConn);

            Label lblPort = new Label
            {
                Text = "UDP Port:",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 9F, FontStyle.Regular),
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Height = 25,
                Margin = new Padding(0, 0, 0, 8)
            };
            numPort = new NumericUpDown
            {
                Minimum = 1024,
                Maximum = 65535,
                Value = 14551,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };
            tlpConn.Controls.Add(lblPort, 0, 0);
            tlpConn.Controls.Add(numPort, 1, 0);

            btnConnect = new Button
            {
                Text = "Connect",
                BackColor = Color.FromArgb(99, 102, 241), // Indigo Blue
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };
            btnConnect.FlatAppearance.BorderSize = 0;
            btnConnect.Click += btnConnect_Click;
            tlpConn.Controls.Add(btnConnect, 0, 1);
            tlpConn.SetColumnSpan(btnConnect, 2);

            lblConnStatus = new Label
            {
                Text = "DISCONNECTED",
                ForeColor = Color.FromArgb(239, 68, 68), // Red
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Height = 20,
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
            tlpConn.Controls.Add(lblConnStatus, 0, 2);
            tlpConn.SetColumnSpan(lblConnStatus, 2);

            // Group 2: Drone Commands
            GroupBox gpCommands = new GroupBox
            {
                Text = "DRONE COMMANDS",
                ForeColor = Color.FromArgb(160, 160, 160),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10, 30, 10, 10),
                Margin = new Padding(0, 0, 0, 15),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            flpSidebar.Controls.Add(gpCommands);

            TableLayoutPanel tlpCmds = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 5,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            tlpCmds.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpCmds.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 5; i++)
            {
                tlpCmds.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            gpCommands.Controls.Add(tlpCmds);

            Button btnGuided = new Button
            {
                Text = "SET GUIDED MODE",
                BackColor = Color.FromArgb(48, 48, 48),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };
            btnGuided.FlatAppearance.BorderSize = 0;
            btnGuided.Click += btnGuided_Click;
            tlpCmds.Controls.Add(btnGuided, 0, 0);
            tlpCmds.SetColumnSpan(btnGuided, 2);

            Button btnArm = new Button
            {
                Text = "ARM",
                BackColor = Color.FromArgb(220, 38, 38), // Crimson Red
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 4, 8)
            };
            btnArm.FlatAppearance.BorderSize = 0;
            btnArm.Click += btnArm_Click;

            Button btnDisarm = new Button
            {
                Text = "DISARM",
                BackColor = Color.FromArgb(75, 85, 99), // Slate gray
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(4, 0, 0, 8)
            };
            btnDisarm.FlatAppearance.BorderSize = 0;
            btnDisarm.Click += btnDisarm_Click;
            tlpCmds.Controls.Add(btnArm, 0, 1);
            tlpCmds.Controls.Add(btnDisarm, 1, 1);

            Label lblTakeoffAlt = new Label
            {
                Text = "Takeoff Alt (m):",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                TextAlign = ContentAlignment.MiddleLeft,
                Height = 25,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };
            numTakeoffAlt = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 100,
                Value = 10,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };
            tlpCmds.Controls.Add(lblTakeoffAlt, 0, 2);
            tlpCmds.Controls.Add(numTakeoffAlt, 1, 2);

            Button btnTakeoff = new Button
            {
                Text = "TAKEOFF",
                BackColor = Color.FromArgb(16, 185, 129), // Emerald Green
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };
            btnTakeoff.FlatAppearance.BorderSize = 0;
            btnTakeoff.Click += btnTakeoff_Click;
            tlpCmds.Controls.Add(btnTakeoff, 0, 3);
            tlpCmds.SetColumnSpan(btnTakeoff, 2);

            Button btnLand = new Button
            {
                Text = "LAND",
                BackColor = Color.FromArgb(245, 158, 11), // Amber Orange
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 5, 4, 0)
            };
            btnLand.FlatAppearance.BorderSize = 0;
            btnLand.Click += (s, e) => _mavlinkService.SetMode(9); // 9 = LAND
            
            Button btnRtl = new Button
            {
                Text = "RTL",
                BackColor = Color.FromArgb(99, 102, 241), // Neon Indigo
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(4, 5, 0, 0)
            };
            btnRtl.FlatAppearance.BorderSize = 0;
            btnRtl.Click += (s, e) => _mavlinkService.SetMode(6); // 6 = RTL
            
            tlpCmds.Controls.Add(btnLand, 0, 4);
            tlpCmds.Controls.Add(btnRtl, 1, 4);

            // Group 3: Map Settings Group
            GroupBox gpMapOptions = new GroupBox
            {
                Text = "MAP OPTIONS",
                ForeColor = Color.FromArgb(160, 160, 160),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10, 30, 10, 10),
                Margin = new Padding(0, 0, 0, 15),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            flpSidebar.Controls.Add(gpMapOptions);

            TableLayoutPanel tlpMap = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 4,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            tlpMap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));
            tlpMap.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
            for (int i = 0; i < 4; i++)
            {
                tlpMap.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            gpMapOptions.Controls.Add(tlpMap);

            Label lblMapSource = new Label
            {
                Text = "Map Source:",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                Width = 80,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };
            cbMapProviders = new ComboBox
            {
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                FlatStyle = FlatStyle.Flat,
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };
            cbMapProviders.Items.AddRange(new object[] {
                "OpenStreetMap",
                "Google Map",
                "Google Satellite",
                "Google Hybrid"
            });
            cbMapProviders.SelectedIndex = 0;
            cbMapProviders.SelectedIndexChanged += cbMapProviders_SelectedIndexChanged;
            tlpMap.Controls.Add(lblMapSource, 0, 0);
            tlpMap.Controls.Add(cbMapProviders, 1, 0);

            chkAutoCenter = new CheckBox
            {
                Text = "Auto Center Drone",
                Checked = true,
                AutoSize = true,
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 6)
            };
            tlpMap.Controls.Add(chkAutoCenter, 0, 1);
            tlpMap.SetColumnSpan(chkAutoCenter, 2);

            chkClickToFly = new CheckBox
            {
                Text = "Enable Click-to-Fly",
                Checked = false,
                AutoSize = true,
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };
            tlpMap.Controls.Add(chkClickToFly, 0, 2);
            tlpMap.SetColumnSpan(chkClickToFly, 2);

            Label lblFlyAlt = new Label
            {
                Text = "Fly Alt (m):",
                ForeColor = Color.FromArgb(200, 200, 200),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Regular),
                Width = 90,
                Height = 25,
                TextAlign = ContentAlignment.MiddleLeft,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
            numTargetAlt = new NumericUpDown
            {
                Minimum = 1,
                Maximum = 100,
                Value = 15,
                BackColor = Color.FromArgb(45, 45, 45),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
            tlpMap.Controls.Add(lblFlyAlt, 0, 3);
            tlpMap.Controls.Add(numTargetAlt, 1, 3);

            // Group 4: Mission Planning Group
            GroupBox gpMission = new GroupBox
            {
                Text = "MISSION PLANNING",
                ForeColor = Color.FromArgb(160, 160, 160),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(10, 30, 10, 10),
                Margin = new Padding(0, 0, 0, 15),
                Anchor = AnchorStyles.Left | AnchorStyles.Right
            };
            flpSidebar.Controls.Add(gpMission);

            TableLayoutPanel tlpMission = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(0),
                Margin = new Padding(0)
            };
            tlpMission.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            tlpMission.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            for (int i = 0; i < 3; i++)
            {
                tlpMission.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }
            gpMission.Controls.Add(tlpMission);

            btnCreateMission = new Button
            {
                Text = "CREATE MISSION",
                BackColor = Color.FromArgb(75, 85, 99),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };
            btnCreateMission.FlatAppearance.BorderSize = 0;
            btnCreateMission.Click += btnCreateMission_Click;
            tlpMission.Controls.Add(btnCreateMission, 0, 0);
            tlpMission.SetColumnSpan(btnCreateMission, 2);

            btnUploadMission = new Button
            {
                Text = "UPLOAD MISSION",
                BackColor = Color.FromArgb(79, 70, 229),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Height = 32,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 8)
            };
            btnUploadMission.FlatAppearance.BorderSize = 0;
            btnUploadMission.Click += btnUploadMission_Click;
            tlpMission.Controls.Add(btnUploadMission, 0, 1);
            tlpMission.SetColumnSpan(btnUploadMission, 2);

            btnStartMission = new Button
            {
                Text = "START MISSION",
                BackColor = Color.FromArgb(16, 185, 129),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Height = 35,
                FlatStyle = FlatStyle.Flat,
                Dock = DockStyle.Fill,
                Margin = new Padding(0)
            };
            btnStartMission.FlatAppearance.BorderSize = 0;
            btnStartMission.Click += btnStartMission_Click;
            tlpMission.Controls.Add(btnStartMission, 0, 2);
            tlpMission.SetColumnSpan(btnStartMission, 2);

            // ==========================================
            // 2. MAIN VIEW (Bên phải, Dock Fill)
            // ==========================================
            Panel pnlMain = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(20, 20, 20),
                Padding = new Padding(15)
            };
            this.Controls.Add(pnlMain);
            pnlMain.SendToBack(); // Push main panel to back to respect the docked-left sidebar Z-order

            // Title of Main Screen
            Label lblMainTitle = new Label
            {
                Text = "TELEMETRY DASHBOARD",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(200, 200, 200),
                Dock = DockStyle.Top,
                Height = 25
            };
            pnlMain.Controls.Add(lblMainTitle);

            // Telemetry Cards Table Layout (DPI-Responsive Auto-sizing height)
            _tlpDashboard = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                RowCount = 3,
                ColumnCount = 4,
                BackColor = Color.FromArgb(20, 20, 20),
                Padding = new Padding(0, 5, 0, 5)
            };
            pnlMain.Controls.Add(_tlpDashboard);

            // Define grid styles
            for (int i = 0; i < 4; i++)
            {
                _tlpDashboard.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
            }
            for (int i = 0; i < 3; i++)
            {
                _tlpDashboard.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            }

            // Create cards (Reduced Value Font Size to prevent overlapping at 200% scaling)
            _tlpDashboard.Controls.Add(CreateTelemetryCard("Flight Mode", out _lblMode), 0, 0);
            _tlpDashboard.Controls.Add(CreateTelemetryCard("Arm Status", out _lblArmStatus), 1, 0);
            _tlpDashboard.Controls.Add(CreateTelemetryCard("Battery Status", out _lblBattery), 2, 0);
            _tlpDashboard.Controls.Add(CreateTelemetryCard("Altitude (Rel)", out _lblAltitude), 3, 0);
            
            _tlpDashboard.Controls.Add(CreateTelemetryCard("Airspeed", out _lblSpeed), 0, 1);
            _tlpDashboard.Controls.Add(CreateTelemetryCard("Roll / Pitch", out _lblAttitude), 1, 1);
            _tlpDashboard.Controls.Add(CreateTelemetryCard("Heading (Yaw)", out _lblHeading), 2, 1);
            _tlpDashboard.Controls.Add(CreateTelemetryCard("GPS Coordinate", out _lblGPS), 3, 1);

            _tlpDashboard.Controls.Add(CreateTelemetryCard("GPS Fix Type", out _lblGpsFix), 0, 2);
            _tlpDashboard.Controls.Add(CreateTelemetryCard("Satellites", out _lblSatellites), 1, 2);
            _tlpDashboard.Controls.Add(CreateTelemetryCard("Dist to Target", out _lblDistance), 2, 2);
            _tlpDashboard.Controls.Add(CreateTelemetryCard("Home Position", out _lblHome), 3, 2);

            // Split Container for GMap and Terminal Log
            SplitContainer mainSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Horizontal,
                SplitterDistance = 330, // Default height for map
                BackColor = Color.FromArgb(20, 20, 20)
            };
            pnlMain.Controls.Add(mainSplit);

            // Initialize GMap Control in SplitContainer Panel1
            _gmap = new GMapControl
            {
                Dock = DockStyle.Fill,
                MapProvider = GMapProviders.OpenStreetMap,
                Position = new PointLatLng(21.0285, 105.8542), // Default center (Hanoi)
                MinZoom = 1,
                MaxZoom = 24,
                Zoom = 16,
                DragButton = MouseButtons.Left,
                ShowCenter = false
            };
            _gmap.MouseClick += Gmap_MouseClick;
            mainSplit.Panel1.Controls.Add(_gmap);

            // Initialize Log Panel in SplitContainer Panel2
            Label lblLogTitle = new Label
            {
                Text = "SYSTEM CONSOLE LOG",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(160, 160, 160),
                Dock = DockStyle.Top,
                Height = 20,
                BackColor = Color.FromArgb(20, 20, 20)
            };
            
            _txtLog = new RichTextBox
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BackColor = Color.Black,
                ForeColor = Color.Lime,
                Font = new Font("Consolas", 10F, FontStyle.Regular),
                BorderStyle = BorderStyle.None
            };
            mainSplit.Panel2.Controls.Add(_txtLog);
            mainSplit.Panel2.Controls.Add(lblLogTitle);

            // Initialize GMap overlays
            _routesOverlay = new GMapOverlay("routes");
            _markersOverlay = new GMapOverlay("markers");
            _gmap.Overlays.Add(_routesOverlay);
            _gmap.Overlays.Add(_markersOverlay);
            GMaps.Instance.Mode = AccessMode.ServerAndCache;
        }

        private Panel CreateTelemetryCard(string title, out Label lblValue)
        {
            Panel card = new Panel
            {
                BackColor = Color.FromArgb(32, 32, 32), // Dark gray card background
                Padding = new Padding(10, 8, 10, 8),
                Margin = new Padding(4),
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink
            };

            Label lblTitle = new Label
            {
                Text = title.ToUpper(),
                ForeColor = Color.FromArgb(160, 160, 160), // Muted Gray
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                Dock = DockStyle.Top,
                AutoSize = true,
                Margin = new Padding(0, 0, 0, 4)
            };

            lblValue = new Label
            {
                Text = "---",
                ForeColor = Color.FromArgb(240, 240, 240), // Bright text
                Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold),
                Dock = DockStyle.Top,
                AutoSize = true,
                TextAlign = ContentAlignment.TopLeft,
                Margin = new Padding(0)
            };

            card.Controls.Add(lblValue);
            card.Controls.Add(lblTitle);

            // Custom border draw
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(48, 48, 48), 1))
                {
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
                }
            };

            return card;
        }

        private void InitializeMavlink()
        {
            _udpConnection = new UdpConnection();
            _mavlinkService = new MavlinkService(_udpConnection);

            _udpConnection.OnLogMessage += LogToUI;
            _mavlinkService.OnLogMessage += LogToUI;

            // Wire MAVLink packets events
            _mavlinkService.OnHeartbeatReceived += Heartbeat =>
            {
                var isArmed = (Heartbeat.base_mode & (byte)MAVLink.MAV_MODE_FLAG.SAFETY_ARMED) > 0;
                var mode = (MAVLink.COPTER_MODE)Heartbeat.custom_mode;
                
                if (this.IsDisposed) return;
                this.BeginInvoke(new Action(() => {
                    _lblMode.Text = mode.ToString();
                    _lblArmStatus.Text = isArmed ? "ARMED" : "DISARMED";
                    _lblArmStatus.ForeColor = isArmed ? Color.FromArgb(239, 68, 68) : Color.FromArgb(34, 197, 94);
                }));
            };

            _mavlinkService.OnAttitudeReceived += Attitude =>
            {
                var rollDeg = Attitude.roll * (180.0 / Math.PI);
                var pitchDeg = Attitude.pitch * (180.0 / Math.PI);
                var yawDeg = Attitude.yaw * (180.0 / Math.PI);
                if (yawDeg < 0) yawDeg += 360.0;
                
                if (this.IsDisposed) return;
                this.BeginInvoke(new Action(() => {
                    _lblAttitude.Text = $"R: {rollDeg:F1}° | P: {pitchDeg:F1}°";
                    _lblHeading.Text = $"{yawDeg:F1}°";
                }));
            };

            _mavlinkService.OnSysStatusReceived += SysStatus =>
            {
                var volts = SysStatus.voltage_battery / 1000.0;
                var pct = SysStatus.battery_remaining;
                
                if (this.IsDisposed) return;
                this.BeginInvoke(new Action(() => {
                    _lblBattery.Text = $"{pct}% ({volts:F2}V)";
                    if (pct < 20)
                        _lblBattery.ForeColor = Color.FromArgb(239, 68, 68); // Red
                    else if (pct < 50)
                        _lblBattery.ForeColor = Color.FromArgb(245, 158, 11); // Orange
                    else
                        _lblBattery.ForeColor = Color.FromArgb(240, 240, 240); // Normal
                }));
            };

            _mavlinkService.OnGlobalPositionReceived += GPS =>
            {
                var lat = GPS.lat / 10000000.0;
                var lon = GPS.lon / 10000000.0;
                var alt = GPS.relative_alt / 1000.0; // altitude above home
                var vx = GPS.vx / 100.0; // m/s
                var vy = GPS.vy / 100.0;
                var vz = GPS.vz / 100.0;
                var speed = Math.Sqrt(vx * vx + vy * vy + vz * vz);
                
                var dronePos = new PointLatLng(lat, lon);
                
                if (this.IsDisposed) return;
                this.BeginInvoke(new Action(() => {
                    _lblAltitude.Text = $"{alt:F2} m";
                    _lblSpeed.Text = $"{speed:F2} m/s";
                    _lblGPS.Text = $"{lat:F6}°N\n{lon:F6}°E";
                    
                    // Set Home Position on first GPS position lock
                    if (_homePos == null && lat != 0 && lon != 0)
                    {
                        _homePos = dronePos;
                        _homeMarker = new GMarkerGoogle(_homePos.Value, GMarkerGoogleType.green_pushpin);
                        _homeMarker.ToolTipText = "🏠 HOME";
                        _homeMarker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                        _markersOverlay.Markers.Add(_homeMarker);
                        
                        _lblHome.Text = $"{_homePos.Value.Lat:F6}°N\n{_homePos.Value.Lng:F6}°E";
                        
                        // Center map on home/drone initially
                        _gmap.Position = _homePos.Value;
                    }
                    
                    // Update Drone Marker Position on GMap
                    if (lat != 0 && lon != 0)
                    {
                        if (_droneMarker == null)
                        {
                            _droneMarker = new GMarkerGoogle(dronePos, GMarkerGoogleType.blue_dot);
                            _droneMarker.ToolTipText = "🔵 DRONE";
                            _droneMarker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                            _markersOverlay.Markers.Add(_droneMarker);
                        }
                        else
                        {
                            _droneMarker.Position = dronePos;
                        }
                    }
                    
                    // Update Trajectory path (Draw Flight Path)
                    if (lat != 0 && lon != 0)
                    {
                        // Add point if it is different from the last point to prevent duplicates
                        if (_flightPoints.Count == 0 || _flightPoints[_flightPoints.Count - 1] != dronePos)
                        {
                            _flightPoints.Add(dronePos);
                            
                            if (_trajectoryRoute != null)
                            {
                                _routesOverlay.Routes.Remove(_trajectoryRoute);
                            }
                            _trajectoryRoute = new GMapRoute(_flightPoints, "FlightPath");
                            _trajectoryRoute.Stroke = new Pen(Color.FromArgb(99, 102, 241), 3f); // Neon Indigo (Indigo Blue)
                            _routesOverlay.Routes.Add(_trajectoryRoute);
                        }
                    }
                    
                    // Update Distance to Target
                    if (_targetMarker != null && lat != 0 && lon != 0)
                    {
                        double distKm = _gmap.MapProvider.Projection.GetDistance(dronePos, _targetMarker.Position);
                        double distM = distKm * 1000.0;
                        _lblDistance.Text = $"{distM:F1} m";
                    }
                    else
                    {
                        _lblDistance.Text = "---";
                    }
                    
                    // Auto-pan if option checked
                    if (chkAutoCenter.Checked && lat != 0 && lon != 0)
                    {
                        _gmap.Position = dronePos;
                    }

                    // Force redraw map to update positions immediately
                    _gmap.Refresh();
                }));
            };

            _mavlinkService.OnGpsRawReceived += GpsRaw =>
            {
                string fixStr = "NO FIX";
                switch (GpsRaw.fix_type)
                {
                    case 0: fixStr = "NO GPS"; break;
                    case 1: fixStr = "NO FIX"; break;
                    case 2: fixStr = "2D FIX"; break;
                    case 3: fixStr = "3D FIX"; break;
                    case 4: fixStr = "DGPS FIX"; break;
                    case 5: fixStr = "RTK FLOAT"; break;
                    case 6: fixStr = "RTK FIXED"; break;
                }
                
                if (this.IsDisposed) return;
                this.BeginInvoke(new Action(() => {
                    _lblGpsFix.Text = fixStr;
                    _lblSatellites.Text = GpsRaw.satellites_visible.ToString();
                }));
            };

            // Wire MAVLink Mission handshakes
            _mavlinkService.OnMissionRequestReceived += Req =>
            {
                if (Req.seq < _missionItems.Count)
                {
                    var item = _missionItems[Req.seq];
                    _mavlinkService.SendMissionItem(item);
                }
                else
                {
                    LogToUI($"[MISSION ERROR] Drone requested sequence {Req.seq} which is out of range (Total items: {_missionItems.Count}).");
                }
            };

            _mavlinkService.OnMissionRequestIntReceived += Req =>
            {
                if (Req.seq < _missionItems.Count)
                {
                    var item = _missionItems[Req.seq];
                    _mavlinkService.SendMissionItemInt(item);
                }
                else
                {
                    LogToUI($"[MISSION ERROR] Drone requested sequence {Req.seq} which is out of range (Total items: {_missionItems.Count}).");
                }
            };

            _mavlinkService.OnMissionAckReceived += Ack =>
            {
                string resultStr = Ack.type == 0 ? "MAV_MISSION_ACCEPTED (Success)" : $"Result Code: {Ack.type}";
                LogToUI($"[MISSION ACK] Received MISSION_ACK: {resultStr}");
                
                if (_isUploadingMission)
                {
                    _isUploadingMission = false;
                    
                    if (this.IsDisposed) return;
                    this.BeginInvoke(new Action(() => {
                        if (Ack.type == 0)
                        {
                            MessageBox.Show("Mission Uploaded successfully to Drone!", "Mission Planning", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show($"Mission Upload failed! Drone returned error code: {Ack.type}", "Mission Planning Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }));
                }
            };

            // Start connection automatically
            StartListening();
        }

        private void StartListening()
        {
            int port = (int)numPort.Value;
            _udpConnection.Start(port);
            _mavlinkService.StartHeartbeat();
            
            btnConnect.Text = "Disconnect";
            btnConnect.BackColor = Color.FromArgb(220, 38, 38); // Red
            lblConnStatus.Text = "LISTENING";
            lblConnStatus.ForeColor = Color.FromArgb(34, 197, 94); // Green
        }

        private void StopListening()
        {
            _mavlinkService.StopHeartbeat();
            _udpConnection.Stop();
            
            btnConnect.Text = "Connect";
            btnConnect.BackColor = Color.FromArgb(99, 102, 241); // Indigo
            lblConnStatus.Text = "DISCONNECTED";
            lblConnStatus.ForeColor = Color.FromArgb(239, 68, 68); // Red
            
            ResetDashboard();
        }

        private void btnConnect_Click(object? sender, EventArgs e)
        {
            if (!_udpConnection.IsListening)
            {
                StartListening();
            }
            else
            {
                StopListening();
            }
        }

        private void btnGuided_Click(object? sender, EventArgs e)
        {
            _mavlinkService.SetMode(4); // 4 = GUIDED
        }

        private void btnArm_Click(object? sender, EventArgs e)
        {
            _mavlinkService.ArmDisarm(true);
        }

        private void btnDisarm_Click(object? sender, EventArgs e)
        {
            _mavlinkService.ArmDisarm(false);
        }

        private void btnTakeoff_Click(object? sender, EventArgs e)
        {
            float alt = (float)numTakeoffAlt.Value;
            _mavlinkService.Takeoff(alt);
        }

        private void cbMapProviders_SelectedIndexChanged(object? sender, EventArgs e)
        {
            switch (cbMapProviders.SelectedIndex)
            {
                case 0:
                    _gmap.MapProvider = GMapProviders.OpenStreetMap;
                    break;
                case 1:
                    _gmap.MapProvider = GMapProviders.GoogleMap;
                    break;
                case 2:
                    _gmap.MapProvider = GMapProviders.GoogleSatelliteMap;
                    break;
                case 3:
                    _gmap.MapProvider = GMapProviders.GoogleHybridMap;
                    break;
            }
            _gmap.Refresh();
        }

        private void Gmap_MouseClick(object? sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                PointLatLng clickPoint = _gmap.FromLocalToLatLng(e.X, e.Y);
                
                // Always create or update the red pin target marker on map when right-clicked
                if (_targetMarker == null)
                {
                    _targetMarker = new GMarkerGoogle(clickPoint, GMarkerGoogleType.red_pushpin);
                    _targetMarker.ToolTipText = "📍 TARGET (DROP POINT)";
                    _targetMarker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
                    _markersOverlay.Markers.Add(_targetMarker);
                }
                else
                {
                    _targetMarker.Position = clickPoint;
                }
                
                // Update target distance immediately
                if (_droneMarker != null)
                {
                    double distKm = _gmap.MapProvider.Projection.GetDistance(_droneMarker.Position, clickPoint);
                    double distM = distKm * 1000.0;
                    _lblDistance.Text = $"{distM:F1} m";
                }
                
                // Update mission route polyline
                UpdateMissionRoute();
                
                if (chkClickToFly.Checked)
                {
                    float targetAlt = (float)numTargetAlt.Value;
                    
                    // Send fly-to target position target to Drone
                    _mavlinkService.SetPositionTarget(clickPoint.Lat, clickPoint.Lng, targetAlt);
                    
                    LogToUI($"[GOTO] Sent position target: Lat {clickPoint.Lat:F6}, Lon {clickPoint.Lng:F6} at Alt {targetAlt}m");
                }
                else
                {
                    LogToUI($"Right-click target set: Lat {clickPoint.Lat:F6}, Lon {clickPoint.Lng:F6}. Enable 'Click-to-Fly' checkbox in sidebar to command the drone.");
                }
            }
        }

        private void UpdateMissionRoute()
        {
            if (_homePos != null && _targetMarker != null)
            {
                var points = new List<PointLatLng> { _homePos.Value, _targetMarker.Position };
                
                if (_missionRoute != null)
                {
                    _routesOverlay.Routes.Remove(_missionRoute);
                }
                
                _missionRoute = new GMapRoute(points, "MissionRoute");
                _missionRoute.Stroke = new Pen(Color.FromArgb(239, 68, 68), 2.5f) // Crimson Red
                {
                    DashStyle = System.Drawing.Drawing2D.DashStyle.Dash
                };
                _routesOverlay.Routes.Add(_missionRoute);
                _gmap.Refresh();
            }
        }

        private void btnCreateMission_Click(object? sender, EventArgs e)
        {
            if (_targetMarker == null)
            {
                MessageBox.Show("Please Right-Click on the map to place a red pin as the Drop Point target first!", "Create Mission Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dropPoint = _targetMarker.Position;

            _missionItems.Clear();

            // Item 0: Dummy WAYPOINT representing Home (ArduPilot requirement)
            var itemHome = new MAVLink.mavlink_mission_item_t
            {
                seq = 0,
                frame = (byte)MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT,
                command = (ushort)MAVLink.MAV_CMD.WAYPOINT,
                current = 0,
                autocontinue = 1,
                x = 0, // ArduPilot will overwrite with home Lat
                y = 0, // ArduPilot will overwrite with home Lng
                z = 0, // ArduPilot will overwrite with home Alt
                mission_type = 0
            };
            _missionItems.Add(itemHome);

            // Item 1: TAKEOFF
            var itemTakeoff = new MAVLink.mavlink_mission_item_t
            {
                seq = 1,
                frame = (byte)MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT,
                command = (ushort)MAVLink.MAV_CMD.TAKEOFF,
                current = 0,
                autocontinue = 1,
                z = 10.0f, // 10 meters altitude (mapped to z)
                mission_type = 0
            };
            _missionItems.Add(itemTakeoff);

            // Item 2: WAYPOINT to Drop Point
            var itemWaypoint = new MAVLink.mavlink_mission_item_t
            {
                seq = 2,
                frame = (byte)MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT,
                command = (ushort)MAVLink.MAV_CMD.WAYPOINT,
                current = 0,
                autocontinue = 1,
                x = (float)dropPoint.Lat,
                y = (float)dropPoint.Lng,
                z = 15.0f, // 15 meters altitude
                mission_type = 0
            };
            _missionItems.Add(itemWaypoint);

            // Item 3: LOITER_TIME at Drop Point (5 seconds)
            var itemLoiter = new MAVLink.mavlink_mission_item_t
            {
                seq = 3,
                frame = (byte)MAVLink.MAV_FRAME.GLOBAL_RELATIVE_ALT,
                command = (ushort)MAVLink.MAV_CMD.LOITER_TIME,
                current = 0,
                autocontinue = 1,
                param1 = 5.0f, // 5 seconds
                x = (float)dropPoint.Lat,
                y = (float)dropPoint.Lng,
                z = 15.0f,
                mission_type = 0
            };
            _missionItems.Add(itemLoiter);

            // Item 4: DO_SET_SERVO on channel 9 (PWM 1900)
            var itemServo = new MAVLink.mavlink_mission_item_t
            {
                seq = 4,
                frame = (byte)MAVLink.MAV_FRAME.MISSION,
                command = (ushort)MAVLink.MAV_CMD.DO_SET_SERVO,
                current = 0,
                autocontinue = 1,
                param1 = 9.0f, // servo channel 9
                param2 = 1900.0f, // PWM value 1900
                mission_type = 0
            };
            _missionItems.Add(itemServo);

            // Item 5: RTL (Return to Launch)
            var itemRTL = new MAVLink.mavlink_mission_item_t
            {
                seq = 5,
                frame = (byte)MAVLink.MAV_FRAME.MISSION,
                command = (ushort)MAVLink.MAV_CMD.RETURN_TO_LAUNCH,
                current = 0,
                autocontinue = 1,
                mission_type = 0
            };
            _missionItems.Add(itemRTL);

            LogToUI($"[MISSION] Created 6 items (Item 0: Home dummy, followed by 5-step rescue mission) at Lat {dropPoint.Lat:F6}, Lon {dropPoint.Lng:F6}. Ready to upload.");
            MessageBox.Show($"Mission created successfully (6 items, including Home position) at Lat: {dropPoint.Lat:F6}, Lon: {dropPoint.Lng:F6}!\nClick 'UPLOAD MISSION' to send it to the drone.", "Create Mission Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnUploadMission_Click(object? sender, EventArgs e)
        {
            if (_missionItems.Count == 0)
            {
                MessageBox.Show("Please click 'CREATE MISSION' first!", "Upload Mission Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _isUploadingMission = true;

            // Start handshake by sending count.
            // ArduPilot will cancel any active transaction and start a new write transaction when MISSION_COUNT is received.
            _mavlinkService.SendMissionCount((ushort)_missionItems.Count);
        }

        private void btnStartMission_Click(object? sender, EventArgs e)
        {
            if (!_udpConnection.IsListening)
            {
                MessageBox.Show("Please connect to SITL first!", "Start Mission Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LogToUI("[MISSION] Starting mission sequence...");
            
            // Run in a background thread to prevent UI freezing
            System.Threading.Tasks.Task.Run(() =>
            {
                try
                {
                    // 1. Switch to GUIDED mode first (required to ARM on the ground reliably)
                    LogToUI("[MISSION] Switching to GUIDED mode to ARM...");
                    _mavlinkService.SetMode(4); // 4 = GUIDED
                    System.Threading.Thread.Sleep(500);

                    // 2. Send ARM command
                    LogToUI("[MISSION] Sending ARM command...");
                    _mavlinkService.ArmDisarm(true);

                    // 3. Wait for the drone to report ARMED (up to 5 seconds timeout)
                    int elapsed = 0;
                    bool armed = false;
                    while (elapsed < 5000)
                    {
                        System.Threading.Thread.Sleep(200);
                        elapsed += 200;
                        
                        if (this.IsDisposed) return;
                        
                        bool isLabelArmed = false;
                        this.Invoke(new Action(() => {
                            isLabelArmed = (_lblArmStatus.Text == "ARMED");
                        }));

                        if (isLabelArmed)
                        {
                            armed = true;
                            break;
                        }
                    }

                    if (armed)
                    {
                        LogToUI("[MISSION] Drone is ARMED. Switching to AUTO mode...");
                        _mavlinkService.SetMode(3); // 3 = AUTO
                        System.Threading.Thread.Sleep(500); // Wait for mode switch to register
                        
                        LogToUI("[MISSION] Sending MISSION_START command to execute mission...");
                        _mavlinkService.StartMission();
                    }
                    else
                    {
                        LogToUI("[MISSION ERROR] Arming timed out or was denied. Please check ArduPilot pre-arm messages in Mission Planner.");
                    }
                }
                catch (Exception ex)
                {
                    LogToUI($"[MISSION ERROR] Failed to start mission: {ex.Message}");
                }
            });
        }

        private void ResetDashboard()
        {
            _lblMode.Text = "---";
            _lblArmStatus.Text = "---";
            _lblArmStatus.ForeColor = Color.FromArgb(240, 240, 240);
            _lblBattery.Text = "---";
            _lblBattery.ForeColor = Color.FromArgb(240, 240, 240);
            _lblAltitude.Text = "---";
            _lblSpeed.Text = "---";
            _lblAttitude.Text = "---";
            _lblHeading.Text = "---";
            _lblGPS.Text = "---";
            _lblGpsFix.Text = "---";
            _lblSatellites.Text = "---";
            _lblDistance.Text = "---";
            _lblHome.Text = "---";
            
            // Clear markers and routes
            _markersOverlay.Markers.Clear();
            _routesOverlay.Routes.Clear();
            _droneMarker = null;
            _targetMarker = null;
            _homeMarker = null;
            _homePos = null;
            _trajectoryRoute = null;
            _missionRoute = null;
            _flightPoints.Clear();
        }

        private void LogToUI(string message)
        {
            if (this.IsDisposed) return;

            if (_txtLog.InvokeRequired)
            {
                _txtLog.BeginInvoke(new Action(() => LogToUI(message)));
            }
            else
            {
                Console.WriteLine($"{DateTime.Now:HH:mm:ss.fff} - {message}");
                
                _txtLog.AppendText($"{DateTime.Now:HH:mm:ss.fff} - {message}{Environment.NewLine}");
                
                if (_txtLog.Lines.Length > 500)
                {
                    _txtLog.Select(0, _txtLog.GetFirstCharIndexFromLine(100));
                    _txtLog.SelectedText = "";
                }
                
                _txtLog.SelectionStart = _txtLog.TextLength;
                _txtLog.ScrollToCaret();
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _udpConnection.Dispose();
            _mavlinkService.Dispose();
            base.OnFormClosing(e);
        }
    }
}