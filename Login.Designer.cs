namespace ADDMS2
{
    partial class fLogin
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges1 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            Guna.UI2.WinForms.Suite.CustomizableEdges customizableEdges2 = new Guna.UI2.WinForms.Suite.CustomizableEdges();
            label1 = new Label();
            txtUsername = new TextBox();
            txtPassword = new TextBox();
            btnLogin = new Button();
            label2 = new Label();
            label3 = new Label();
            label4 = new Label();
            label5 = new Label();
            label6 = new Label();
            label7 = new Label();
            label8 = new Label();
            guna2Button1 = new Guna.UI2.WinForms.Guna2Button();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(101, 111);
            label1.Name = "label1";
            label1.Size = new Size(0, 25);
            label1.TabIndex = 0;
            // 
            // txtUsername
            // 
            txtUsername.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtUsername.Location = new Point(61, 161);
            txtUsername.Name = "txtUsername";
            txtUsername.Size = new Size(372, 37);
            txtUsername.TabIndex = 1;
            txtUsername.Text = "Username";
            // 
            // txtPassword
            // 
            txtPassword.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtPassword.Location = new Point(61, 216);
            txtPassword.Name = "txtPassword";
            txtPassword.Size = new Size(372, 37);
            txtPassword.TabIndex = 2;
            txtPassword.Text = "Password";
            // 
            // btnLogin
            // 
            btnLogin.Location = new Point(0, 0);
            btnLogin.Name = "btnLogin";
            btnLogin.Size = new Size(75, 23);
            btnLogin.TabIndex = 12;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point, 0);
            label2.ForeColor = Color.FromArgb(0, 0, 64);
            label2.Location = new Point(185, 44);
            label2.Name = "label2";
            label2.Size = new Size(122, 38);
            label2.TabIndex = 4;
            label2.Text = "ADDMS";
            label2.Click += label2_Click;
            // 
            // label3
            // 
            label3.AutoSize = true;
            label3.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label3.Location = new Point(183, 97);
            label3.Name = "label3";
            label3.Size = new Size(124, 21);
            label3.TabIndex = 5;
            label3.Text = "DRONE SYSTEM";
            // 
            // label4
            // 
            label4.AutoSize = true;
            label4.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label4.Location = new Point(61, 376);
            label4.Name = "label4";
            label4.Size = new Size(132, 21);
            label4.TabIndex = 6;
            label4.Text = "DEMO ACCOUNT";
            // 
            // label5
            // 
            label5.AutoSize = true;
            label5.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label5.ForeColor = Color.Navy;
            label5.Location = new Point(61, 434);
            label5.Name = "label5";
            label5.Size = new Size(56, 21);
            label5.TabIndex = 7;
            label5.Text = "Admin";
            // 
            // label6
            // 
            label6.AutoSize = true;
            label6.Location = new Point(61, 466);
            label6.Name = "label6";
            label6.Size = new Size(159, 25);
            label6.TabIndex = 8;
            label6.Text = "admin / admin123";
            // 
            // label7
            // 
            label7.AutoSize = true;
            label7.Font = new Font("Segoe UI", 8F, FontStyle.Regular, GraphicsUnit.Point, 0);
            label7.ForeColor = Color.Blue;
            label7.Location = new Point(267, 434);
            label7.Name = "label7";
            label7.Size = new Size(73, 21);
            label7.TabIndex = 9;
            label7.Text = "Operator";
            // 
            // label8
            // 
            label8.AutoSize = true;
            label8.Location = new Point(267, 466);
            label8.Name = "label8";
            label8.Size = new Size(150, 25);
            label8.TabIndex = 10;
            label8.Text = "operator / op123";
            // 
            // guna2Button1
            // 
            guna2Button1.BackColor = Color.Transparent;
            guna2Button1.BorderRadius = 20;
            guna2Button1.CustomizableEdges = customizableEdges1;
            guna2Button1.DisabledState.BorderColor = Color.DarkGray;
            guna2Button1.DisabledState.CustomBorderColor = Color.DarkGray;
            guna2Button1.DisabledState.FillColor = Color.FromArgb(169, 169, 169);
            guna2Button1.DisabledState.ForeColor = Color.FromArgb(141, 141, 141);
            guna2Button1.FillColor = Color.Navy;
            guna2Button1.Font = new Font("Segoe UI", 9F);
            guna2Button1.ForeColor = Color.White;
            guna2Button1.Location = new Point(61, 284);
            guna2Button1.Name = "guna2Button1";
            guna2Button1.ShadowDecoration.CustomizableEdges = customizableEdges2;
            guna2Button1.Size = new Size(372, 68);
            guna2Button1.TabIndex = 11;
            guna2Button1.Text = "Sign In";
            guna2Button1.Click += this.guna2Button1_Click;
            // 
            // fLogin
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(489, 571);
            Controls.Add(guna2Button1);
            Controls.Add(label8);
            Controls.Add(label7);
            Controls.Add(label6);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(btnLogin);
            Controls.Add(txtPassword);
            Controls.Add(txtUsername);
            Controls.Add(label1);
            Name = "fLogin";
            Text = "Login";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label label1;
        private TextBox txtUsername;
        private TextBox txtPassword;
        private Button btnLogin;
        private Label label2;
        private Label label3;
        private Label label4;
        private Label label5;
        private Label label6;
        private Label label7;
        private Label label8;
        private Guna.UI2.WinForms.Guna2Button guna2Button1;
    }
}
