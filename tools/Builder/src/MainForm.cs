#region "Copyright"
/*
The MIT License

Copyright (c) 2004-2005 Clayton Harbour

Permission is hereby granted, free of charge, to any person obtaining a copy of this 
software and associated documentation files (the "Software"), to deal in the Software 
without restriction, including without limitation the rights to use, copy, modify, merge, 
publish, distribute, sublicense, and/or sell copies of the Software, and to permit 
persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or 
substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR 
PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE 
FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR 
OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
DEALINGS IN THE SOFTWARE.

 */
#endregion "Copyright"

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Threading;

using Sporadicism.Builder.Event;
using Sporadicism.Builder.NAnt;
using Sporadicism.Builder.ThreadHandler;
using Sporadicism.Builder.Runner;

namespace Sporadicism.Builder {
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
    public class MainForm : System.Windows.Forms.Form {
        private System.Windows.Forms.RichTextBox TargetOutput;
        private System.Windows.Forms.MainMenu MainFormMenu;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItem3;

        private NAntRunner runner;
        private System.Windows.Forms.MenuItem menuItem15;
        private System.Windows.Forms.MenuItem menuItem16;
        private System.Windows.Forms.Button ClearButton;
        private System.Windows.Forms.MenuItem menuItem23;
        private System.Windows.Forms.MenuItem menuItem24;
        private System.Windows.Forms.ListView TargetView;
        private System.Windows.Forms.ColumnHeader TargetColumn;
        private System.Windows.Forms.ContextMenu TargetViewContextMenu;
        private System.Windows.Forms.MenuItem menuItem25;
        private System.Windows.Forms.MenuItem menuItem26;

        private const string Editor = "notepad.exe";
        private System.Windows.Forms.MenuItem menuItem27;
        private System.Windows.Forms.MenuItem menuItem28;
        private System.Windows.Forms.Label TitleLabel;
        private System.Windows.Forms.Button BuildButton;
        private System.Windows.Forms.MenuItem menuItem4;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem menuItem6;
        private System.Windows.Forms.MenuItem menuItem7;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        /// <summary>
        /// Main application entry point.
        /// </summary>
        public MainForm() {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            RefreshTargetView();
            this.TitleLabel.Text = App.Instance.Title;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing ) {
            if( disposing ) {
                if (components != null) {
                    components.Dispose();
                }
            }
            base.Dispose( disposing );
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
            this.TitleLabel = new System.Windows.Forms.Label();
            this.BuildButton = new System.Windows.Forms.Button();
            this.TargetOutput = new System.Windows.Forms.RichTextBox();
            this.MainFormMenu = new System.Windows.Forms.MainMenu();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem24 = new System.Windows.Forms.MenuItem();
            this.menuItem23 = new System.Windows.Forms.MenuItem();
            this.menuItem15 = new System.Windows.Forms.MenuItem();
            this.menuItem16 = new System.Windows.Forms.MenuItem();
            this.ClearButton = new System.Windows.Forms.Button();
            this.TargetView = new System.Windows.Forms.ListView();
            this.TargetColumn = new System.Windows.Forms.ColumnHeader();
            this.TargetViewContextMenu = new System.Windows.Forms.ContextMenu();
            this.menuItem25 = new System.Windows.Forms.MenuItem();
            this.menuItem26 = new System.Windows.Forms.MenuItem();
            this.menuItem28 = new System.Windows.Forms.MenuItem();
            this.menuItem27 = new System.Windows.Forms.MenuItem();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.SuspendLayout();
            // 
            // TitleLabel
            // 
            this.TitleLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            this.TitleLabel.Location = new System.Drawing.Point(0, 0);
            this.TitleLabel.Name = "TitleLabel";
            this.TitleLabel.Size = new System.Drawing.Size(240, 32);
            this.TitleLabel.TabIndex = 0;
            this.TitleLabel.Text = "Builder";
            // 
            // BuildButton
            // 
            this.BuildButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.BuildButton.Location = new System.Drawing.Point(256, 8);
            this.BuildButton.Name = "BuildButton";
            this.BuildButton.TabIndex = 6;
            this.BuildButton.Text = "Build";
            this.BuildButton.Click += new System.EventHandler(this.BuildButton_Click);
            // 
            // TargetOutput
            // 
            this.TargetOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left) 
                | System.Windows.Forms.AnchorStyles.Right)));
            this.TargetOutput.Location = new System.Drawing.Point(168, 40);
            this.TargetOutput.Name = "TargetOutput";
            this.TargetOutput.Size = new System.Drawing.Size(520, 336);
            this.TargetOutput.TabIndex = 9;
            this.TargetOutput.Text = "";
            // 
            // MainFormMenu
            // 
            this.MainFormMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                         this.menuItem1,
                                                                                         this.menuItem3,
                                                                                         this.menuItem4,
                                                                                         this.menuItem15});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem2});
            this.menuItem1.Text = "File";
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 0;
            this.menuItem2.Text = "Exit";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 1;
            this.menuItem3.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem24});
            this.menuItem3.Text = "Edit";
            // 
            // menuItem24
            // 
            this.menuItem24.Index = 0;
            this.menuItem24.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                       this.menuItem23});
            this.menuItem24.Text = "Copy";
            // 
            // menuItem23
            // 
            this.menuItem23.Index = 0;
            this.menuItem23.Text = "To Text File";
            this.menuItem23.Click += new System.EventHandler(this.menuItem23_Click);
            // 
            // menuItem15
            // 
            this.menuItem15.Index = 3;
            this.menuItem15.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                       this.menuItem16});
            this.menuItem15.Text = "Help";
            // 
            // menuItem16
            // 
            this.menuItem16.Index = 0;
            this.menuItem16.Text = "About";
            this.menuItem16.Click += new System.EventHandler(this.menuItem16_Click);
            // 
            // ClearButton
            // 
            this.ClearButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ClearButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ClearButton.Location = new System.Drawing.Point(608, 8);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.TabIndex = 10;
            this.ClearButton.Text = "Clear";
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // TargetView
            // 
            this.TargetView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
                | System.Windows.Forms.AnchorStyles.Left)));
            this.TargetView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
                                                                                         this.TargetColumn});
            this.TargetView.ContextMenu = this.TargetViewContextMenu;
            this.TargetView.FullRowSelect = true;
            this.TargetView.Location = new System.Drawing.Point(8, 40);
            this.TargetView.MultiSelect = false;
            this.TargetView.Name = "TargetView";
            this.TargetView.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.TargetView.Size = new System.Drawing.Size(152, 336);
            this.TargetView.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.TargetView.TabIndex = 12;
            this.TargetView.View = System.Windows.Forms.View.Details;
            this.TargetView.DoubleClick += new System.EventHandler(this.TargetView_DoubleClick);
            // 
            // TargetColumn
            // 
            this.TargetColumn.Text = "Target";
            this.TargetColumn.Width = 149;
            // 
            // TargetViewContextMenu
            // 
            this.TargetViewContextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                                  this.menuItem25,
                                                                                                  this.menuItem26,
                                                                                                  this.menuItem28,
                                                                                                  this.menuItem27});
            // 
            // menuItem25
            // 
            this.menuItem25.Index = 0;
            this.menuItem25.Text = "Open";
            this.menuItem25.Click += new System.EventHandler(this.menuItem25_Click);
            // 
            // menuItem26
            // 
            this.menuItem26.Index = 1;
            this.menuItem26.Text = "Explore";
            this.menuItem26.Click += new System.EventHandler(this.menuItem26_Click);
            // 
            // menuItem28
            // 
            this.menuItem28.Index = 2;
            this.menuItem28.Text = "-";
            // 
            // menuItem27
            // 
            this.menuItem27.Index = 3;
            this.menuItem27.Text = "Run";
            this.menuItem27.Click += new System.EventHandler(this.menuItem27_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 2;
            this.menuItem4.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem5,
                                                                                      this.menuItem6,
                                                                                      this.menuItem7});
            this.menuItem4.Text = "Tools";
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 0;
            this.menuItem5.Text = "Bugs";
            this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click_1);
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 1;
            this.menuItem6.Text = "Project";
            this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 2;
            this.menuItem7.Text = "Lists";
            this.menuItem7.Click += new System.EventHandler(this.menuItem7_Click);
            // 
            // MainForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(712, 389);
            this.Controls.Add(this.TargetView);
            this.Controls.Add(this.ClearButton);
            this.Controls.Add(this.TargetOutput);
            this.Controls.Add(this.BuildButton);
            this.Controls.Add(this.TitleLabel);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.MainFormMenu;
            this.Name = "MainForm";
            this.Text = "Main Form";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.ResumeLayout(false);

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.Run(new MainForm());
        }

        private void RefreshTargetView() {
            Target[] targets = NAntTargets.Instance.GetTargets();
            foreach (Target target in targets) {
                ListViewItem item = new ListViewItem(target.Name);
                item.Tag = target;
                this.TargetView.Items.Add(item);
            }
        }

        private void StartNAnt (string target) {
            runner = new NAntRunner(this, target);
            runner.TargetOutputEvent += new TargetEventHandler(this.UpdateOutput);
            runner.Start();
        }

        private void StopNAnt() {
            this.runner.Stop();
        }

        private void UpdateOutput (object sender, TargetEventArgs args) {
            string textToAppend = string.Format("[{0}] - {1}{2}",
                args.Target, args.Output, Environment.NewLine);
            this.TargetOutput.AppendText(textToAppend);
            int position;

            int startSearch = (this.TargetOutput.Text.Length - textToAppend.Length);
            if (startSearch < 0) {
                startSearch = 0;
            }
            int endSearch = this.TargetOutput.Text.Length;
            if ((position = this.TargetOutput.Find("error", startSearch, 
                endSearch, RichTextBoxFinds.NoHighlight)) > -1) {
                this.TargetOutput.Select(position, "error".Length);
                this.TargetOutput.SelectionColor = Color.Red;
            }

            if ((position = this.TargetOutput.Find("BUILD FAILED", 
                startSearch, endSearch, RichTextBoxFinds.NoHighlight)) > -1) {
                this.TargetOutput.Select(position, "error".Length);
                this.TargetOutput.SelectionColor = Color.Red;
            }

            if ((position = this.TargetOutput.Find("BUILD SUCCEEDED", 
                startSearch, endSearch, RichTextBoxFinds.NoHighlight)) > -1) {
                this.TargetOutput.Select(position, "BUILD SUCCEEDED".Length);
                this.TargetOutput.SelectionColor = Color.Green;
            }

            this.TargetOutput.Focus();
            this.TargetOutput.SelectionStart = this.TargetOutput.Text.Length;
            this.TargetOutput.ScrollToCaret();
        }

        private void menuItem2_Click(object sender, System.EventArgs e) {
            Application.Exit();
        }

        private void menuItem5_Click(object sender, System.EventArgs e) {
            this.StartNAnt("EditLocalBuildConfig");
        }

        private void menuItem8_Click(object sender, System.EventArgs e) {
            this.StartNAnt("-buildfile:schedule.release.xml user-guide");
        }

        private void menuItem16_Click(object sender, System.EventArgs e) {
            MessageBox.Show(string.Format("Version: {0}", App.Instance.Version.ToString()), App.Instance.Name);
        }


        private void ClearButton_Click(object sender, System.EventArgs e) {
            this.TargetOutput.Text = string.Empty;
        }

        private void SetupButton_Click(object sender, System.EventArgs e) {
            this.StartNAnt("setup");
        }

        private void menuItem23_Click(object sender, System.EventArgs e) {
            string tempFile = Path.Combine(System.IO.Path.GetTempPath(), "Builder.txt");
            using (StreamWriter writer = new StreamWriter(tempFile)) {
                writer.Write(this.TargetOutput.Text);
            }
            StartThread thread = new StartThread(tempFile);
        }

        private void TargetView_DoubleClick (object sender, System.EventArgs args) {
            Target target = (Target)this.TargetView.SelectedItems[0].Tag;
            this.StartNAnt(target.Name);
        }

        private void menuItem25_Click(object sender, System.EventArgs e) {
            Target target = (Target)this.TargetView.SelectedItems[0].Tag;
            StartThread thread = new StartThread(Editor, target.File.FullName);
        }

        private void menuItem26_Click(object sender, System.EventArgs e) {
            Target target = (Target)this.TargetView.SelectedItems[0].Tag;
            StartThread thread = new StartThread(target.File.Directory.FullName);
        }

        private void menuItem27_Click(object sender, System.EventArgs e) {
            this.TargetView_DoubleClick(sender, e);
        }

        private void BuildButton_Click(object sender, System.EventArgs e) {
            StartNAnt("build.all");
        }

        private void menuItem5_Click_1(object sender, System.EventArgs e) {
            StartThread thread = new StartThread("http://sourceforge.net/tracker/?group_id=78334&atid=552888");
        }

        private void menuItem6_Click(object sender, System.EventArgs e) {
            StartThread thread = new StartThread("http://sourceforge.net/projects/sharpcvslib/");
        }

        private void menuItem7_Click(object sender, System.EventArgs e) {
            StartThread thread = new StartThread("http://sourceforge.net/mail/?group_id=78334");
        }

    }
}
