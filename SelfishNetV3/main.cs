using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Text;
using Microsoft.Win32;
using System.IO;
using System.Diagnostics;

namespace SelfishNetv3
{
    public partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            CustomizeDesign();
        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        // Color definitions
        int titleBarColor = Color.FromArgb(23, 148, 67).ToArgb();
        int mainColor = Color.FromArgb(29, 185, 84).ToArgb();
        int secondaryColor = Color.FromArgb(29, 210, 84).ToArgb();
        int pressedColor = Color.FromArgb(23, 148, 67).ToArgb();

        #region Form Functionalities
        // RESIZE METHOD TO RESIZE FORM AT RUNTIME ----------------------------------------------------------
        private int tolerance = 12;
        private const int WM_NCHITTEST = 132;
        private const int HTBOTTOMRIGHT = 17;
        private Rectangle sizeGripRectangle;

        // METHOD TO DRAG THE FORM
        [DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        // ADDS "Computerfont" TO THE SYSTEM        
        [DllImport("gdi32", EntryPoint = "AddFontResource")]
        public static extern int AddFontResourceA(string lpFileName);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern int AddFontResource(string lpszFilename);
        [System.Runtime.InteropServices.DllImport("gdi32.dll")]
        private static extern int CreateScalableFontResource(uint fdwHidden, string
        lpszFontRes, string lpszFontFile, string lpszCurrentPath);

        protected override void WndProc(ref Message m)
        {

            switch (m.Msg)
            {
                case WM_NCHITTEST:
                    base.WndProc(ref m);
                    var hitPoint = this.PointToClient(new Point(m.LParam.ToInt32() & 0xffff, m.LParam.ToInt32() >> 16));
                    if (sizeGripRectangle.Contains(hitPoint))
                        m.Result = new IntPtr(HTBOTTOMRIGHT);
                    break;
                default:
                    base.WndProc(ref m);
                    break;
            }
        }

        // DRAW RECTANGLE / EXCLUDE CORNER PANEL 
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            var region = new Region(new Rectangle(0, 0, this.ClientRectangle.Width, this.ClientRectangle.Height));

            sizeGripRectangle = new Rectangle(this.ClientRectangle.Width - tolerance, this.ClientRectangle.Height - tolerance, tolerance, tolerance);

            region.Exclude(sizeGripRectangle);
            this.panelContenedor.Region = region;
            this.Invalidate();
        }

        // COLOR AND GRIP OF BOTTOM RECTANGLE
        protected override void OnPaint(PaintEventArgs e)
        {
            //SolidBrush greenBrush = new SolidBrush(Color.FromArgb(23, 148, 67));
            SolidBrush solidBrush = new SolidBrush(Color.FromArgb(23, 148, 67));
            e.Graphics.FillRectangle(solidBrush, sizeGripRectangle);

            base.OnPaint(e);
            ControlPaint.DrawSizeGrip(e.Graphics, Color.Transparent, sizeGripRectangle);
        }

        // TITLE BAR BUTTONS
        private void btnCerrar_Click(object sender, EventArgs e)
        {
            var confirmExit = MessageBox.Show("Are you sure you want to exit the application?",
                "Confirm Exit", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirmExit == DialogResult.Yes)
            {
                ArpForm.instance.Dispose();
                Environment.Exit(0);
            }
        }

        // Capture position and size before maximizing to restore later
        int lx, ly;
        int sw, sh;
        private void panelBarraTitulo_MouseMove(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void btnMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnRestaurar_Click(object sender, EventArgs e)
        {
            btnMaximizar.Visible = true;
            btnRestaurar.Visible = false;
            this.Size = new Size(sw, sh);
            this.Location = new Point(lx, ly);
        }

        private void btnMaximizar_Click(object sender, EventArgs e)
        {
            lx = this.Location.X;
            ly = this.Location.Y;
            sw = this.Size.Width;
            sh = this.Size.Height;
            btnMaximizar.Visible = false;
            btnRestaurar.Visible = true;
            this.Size = Screen.PrimaryScreen.WorkingArea.Size;
            this.Location = Screen.PrimaryScreen.WorkingArea.Location;
        }

        private static void RegisterFont(string contentFontName)
        {
            // get parent of System folder to have Windows folder
            DirectoryInfo dirWindowsFolder = Directory.GetParent(Environment.GetFolderPath(Environment.SpecialFolder.System));

            // Concatenate Fonts folder onto Windows folder.
            string strFontsFolder = Path.Combine(dirWindowsFolder.FullName, "Fonts");

            // Results in full path e.g. "C:\Windows\Fonts\fontName"
            var fontDestination = Path.Combine(strFontsFolder, contentFontName);

            if (!File.Exists(fontDestination))
            {
                // Copies font to destination
                System.IO.File.Copy(Path.Combine(System.IO.Directory.GetCurrentDirectory(), contentFontName), fontDestination);

                // Retrieves font name
                // Makes sure you reference System.Drawing
                PrivateFontCollection fontCol = new PrivateFontCollection();
                fontCol.AddFontFile(fontDestination);
                var actualFontName = fontCol.Families[0].Name;

                //Add font
                AddFontResource(fontDestination);
                //Add registry entry  
                Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts",
                actualFontName, contentFontName, RegistryValueKind.String);
            }
        }

        //Font computerFont = new Font("Computerfont", 71, FontStyle.Regular);

        private void SetFont()
        {
            titulo.Font = new Font("Computerfont", 71);
        }

        // MENU PANEL FUNCTIONALITIES
        private void CustomizeDesign()
        {
            panelSubMenuARPAttack.Visible = false;
            panelSubMenuInfo.Visible = false;
            RegisterFont("Computerfont.ttf");
            SetFont();
        }

        private void HideSubMenu()
        {
            if (panelSubMenuARPAttack.Visible == true)
            {
                panelSubMenuARPAttack.Visible = false;
                btnCoolThings.BackColor = Color.FromArgb(29, 185, 84);
            }

            if (panelSubMenuInfo.Visible == true)
            {
                panelSubMenuInfo.Visible = false;
                btnInfo.BackColor = Color.FromArgb(29, 185, 84);
            }
        }

        private void ShowSubMenu(Panel subMenu, Button button)
        {
            if (subMenu.Visible == false)
            {
                HideSubMenu();
                subMenu.Visible = true;
            }
            else
            {
                subMenu.Visible = false;
                button.BackColor = Color.FromArgb(29, 185, 84);
            }
        }

        // RIGHT PANEL BUTTONS -----------------------------------------------------------------------
        // PRODUCT BUTTONS AND PRODUCT SUBMENU

        #region ARPATTACK
        private void btnARP_Click(object sender, EventArgs e)
        {
            OpenForm(() => new ArpForm());
        }

        #endregion

        #region Tools

        private void btnCoolThings_Click(object sender, EventArgs e)
        {
            btnCoolThings.BackColor = Color.FromArgb(23, 148, 67);
            ShowSubMenu(panelSubMenuARPAttack, btnCoolThings);
        }

        private void btnWebPage_Click(object sender, EventArgs e)
        {
            if (System.IO.File.Exists("hlpindex.html"))
            {
                Process.Start("hlpindex.html");
            }
            else
            {
                int num = (int)MessageBox.Show("help file is missing !");
            }
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            OpenForm(() => new about());
        }
        #endregion
        //------------------------------------------------------------------------------------------------

        #region Reports SubMenu

        private void btnInfo_Click(object sender, EventArgs e)
        {
            btnInfo.BackColor = Color.FromArgb(23, 148, 67);
            ShowSubMenu(panelSubMenuInfo, btnInfo);
        }

        #endregion

        private void btnExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        [DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);

        #endregion

        // METHOD TO OPEN FORMS INSIDE THE PANEL
        public void OpenForm<TForm>(Func<TForm> formFactory) where TForm : Form
        {
            Form form = panelformularios.Controls.OfType<TForm>().FirstOrDefault();
            
            // if the form/instance does not exist
            if (form == null)
            {
                form = formFactory();
                form.TopLevel = false;
                form.FormBorderStyle = FormBorderStyle.None;
                form.Dock = DockStyle.Fill;
                panelformularios.Controls.Add(form);
                panelformularios.Tag = form;
                form.Show();
                form.BringToFront();
                form.FormClosed += new FormClosedEventHandler(CloseForms);
            }
            // if the form/instance exists
            else
            {
                form.BringToFront();
            }
        }
        private void CloseForms(object sender, FormClosedEventArgs e)
        {
            if (Application.OpenForms["UI_Productos"] == null)
            {
                btnARP.BackColor = Color.FromArgb(29, 185, 84);
            }
            if (Application.OpenForms["UI_Proveedores"] == null)
            {
                btnCoolThings.BackColor = Color.FromArgb(29, 185, 84);
            }
            if (Application.OpenForms["UI_Reportes"] == null)
            {
                btnInfo.BackColor = Color.FromArgb(29, 185, 84);
            }
        }

    }
}
