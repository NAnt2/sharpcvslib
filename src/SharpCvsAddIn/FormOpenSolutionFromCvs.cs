using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Resources;

using System.IO;

namespace SharpCvsAddIn
{
	/// <summary>
	/// Summary description for FormOpenSolutionFromCvs.
	/// </summary>
	public class FormOpenSolutionFromCvs : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.Button okBtn;
		private System.Windows.Forms.Label cvsModuleLabel;
		private System.Windows.Forms.Label slnPathLabel;
		internal System.Windows.Forms.TextBox destPathTextBox;
		private System.Windows.Forms.Label cvsTagLbl;
		private System.Windows.Forms.Button slnPathBtn;
		internal System.Windows.Forms.ComboBox cvsModuleDropDown;
		internal System.Windows.Forms.ComboBox cvsTagDropDown;
		private System.Windows.Forms.Label cvsConnectionStringLabel;
		private bool allowClose = true;
		private System.Windows.Forms.Label solutionLocationLabel;		// use this if validation fails 
		private string workingDir_ = string.Empty;
		private string solutionMsg_ = string.Empty;
		private IController cont_;

		public FormOpenSolutionFromCvs(IController cont, string connectionString, string workingDir )
		{
			InitializeComponent();

			cont_ = cont;
			workingDir_ = workingDir;

			this.Text = cont.Model.ResourceManager.GetString( "FORM_OPEN_SOLUTION_FROM_CVS_TITLE" );
			cvsConnectionStringLabel.Text = 
				string.Format( "{0} - {1}", cont.Model.ResourceManager.GetString("FORM_CVS_CONNECTION_STRING"),
				connectionString ) ;
			cvsModuleLabel.Text = cont.Model.ResourceManager.GetString("FORM_CVS_MODULE");
			slnPathLabel.Text = cont.Model.ResourceManager.GetString("FORM_SOLUTION_PATH");
			destPathTextBox.Text = workingDir_;
			okBtn.Text = cont.Model.ResourceManager.GetString("FORM_OK_BUTTON");
			cancelBtn.Text = cont.Model.ResourceManager.GetString("FORM_CANCEL_BUTTON");
			cvsTagLbl.Text = cont.Model.ResourceManager.GetString( "FORM_CVS_TAG" );
			okBtn.Text = cont.Model.ResourceManager.GetString("FORM_OK_BUTTON");
			cancelBtn.Text = cont.Model.ResourceManager.GetString("FORM_CANCEL_BUTTON");

			solutionMsg_ = cont.Model.ResourceManager.GetString("FORM_SOLUTION_DIRECTORY");

			CvsHistory hist = cont.Model.CurrentHistory;
			string[] modules = hist.ModuleNames;

			if(modules  != null )
			{
				cvsModuleDropDown.Items.AddRange( modules );
			}



			


		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
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
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(FormOpenSolutionFromCvs));
			this.cancelBtn = new System.Windows.Forms.Button();
			this.okBtn = new System.Windows.Forms.Button();
			this.cvsModuleLabel = new System.Windows.Forms.Label();
			this.slnPathLabel = new System.Windows.Forms.Label();
			this.destPathTextBox = new System.Windows.Forms.TextBox();
			this.cvsTagLbl = new System.Windows.Forms.Label();
			this.slnPathBtn = new System.Windows.Forms.Button();
			this.cvsModuleDropDown = new System.Windows.Forms.ComboBox();
			this.cvsTagDropDown = new System.Windows.Forms.ComboBox();
			this.cvsConnectionStringLabel = new System.Windows.Forms.Label();
			this.solutionLocationLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// cancelBtn
			// 
			this.cancelBtn.AccessibleDescription = resources.GetString("cancelBtn.AccessibleDescription");
			this.cancelBtn.AccessibleName = resources.GetString("cancelBtn.AccessibleName");
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cancelBtn.Anchor")));
			this.cancelBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cancelBtn.BackgroundImage")));
			this.cancelBtn.CausesValidation = false;
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cancelBtn.Dock")));
			this.cancelBtn.Enabled = ((bool)(resources.GetObject("cancelBtn.Enabled")));
			this.cancelBtn.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("cancelBtn.FlatStyle")));
			this.cancelBtn.Font = ((System.Drawing.Font)(resources.GetObject("cancelBtn.Font")));
			this.cancelBtn.Image = ((System.Drawing.Image)(resources.GetObject("cancelBtn.Image")));
			this.cancelBtn.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cancelBtn.ImageAlign")));
			this.cancelBtn.ImageIndex = ((int)(resources.GetObject("cancelBtn.ImageIndex")));
			this.cancelBtn.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cancelBtn.ImeMode")));
			this.cancelBtn.Location = ((System.Drawing.Point)(resources.GetObject("cancelBtn.Location")));
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cancelBtn.RightToLeft")));
			this.cancelBtn.Size = ((System.Drawing.Size)(resources.GetObject("cancelBtn.Size")));
			this.cancelBtn.TabIndex = ((int)(resources.GetObject("cancelBtn.TabIndex")));
			this.cancelBtn.Text = resources.GetString("cancelBtn.Text");
			this.cancelBtn.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cancelBtn.TextAlign")));
			this.cancelBtn.Visible = ((bool)(resources.GetObject("cancelBtn.Visible")));
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// okBtn
			// 
			this.okBtn.AccessibleDescription = resources.GetString("okBtn.AccessibleDescription");
			this.okBtn.AccessibleName = resources.GetString("okBtn.AccessibleName");
			this.okBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("okBtn.Anchor")));
			this.okBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("okBtn.BackgroundImage")));
			this.okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okBtn.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("okBtn.Dock")));
			this.okBtn.Enabled = ((bool)(resources.GetObject("okBtn.Enabled")));
			this.okBtn.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("okBtn.FlatStyle")));
			this.okBtn.Font = ((System.Drawing.Font)(resources.GetObject("okBtn.Font")));
			this.okBtn.Image = ((System.Drawing.Image)(resources.GetObject("okBtn.Image")));
			this.okBtn.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("okBtn.ImageAlign")));
			this.okBtn.ImageIndex = ((int)(resources.GetObject("okBtn.ImageIndex")));
			this.okBtn.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("okBtn.ImeMode")));
			this.okBtn.Location = ((System.Drawing.Point)(resources.GetObject("okBtn.Location")));
			this.okBtn.Name = "okBtn";
			this.okBtn.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("okBtn.RightToLeft")));
			this.okBtn.Size = ((System.Drawing.Size)(resources.GetObject("okBtn.Size")));
			this.okBtn.TabIndex = ((int)(resources.GetObject("okBtn.TabIndex")));
			this.okBtn.Text = resources.GetString("okBtn.Text");
			this.okBtn.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("okBtn.TextAlign")));
			this.okBtn.Visible = ((bool)(resources.GetObject("okBtn.Visible")));
			this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
			// 
			// cvsModuleLabel
			// 
			this.cvsModuleLabel.AccessibleDescription = resources.GetString("cvsModuleLabel.AccessibleDescription");
			this.cvsModuleLabel.AccessibleName = resources.GetString("cvsModuleLabel.AccessibleName");
			this.cvsModuleLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cvsModuleLabel.Anchor")));
			this.cvsModuleLabel.AutoSize = ((bool)(resources.GetObject("cvsModuleLabel.AutoSize")));
			this.cvsModuleLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cvsModuleLabel.Dock")));
			this.cvsModuleLabel.Enabled = ((bool)(resources.GetObject("cvsModuleLabel.Enabled")));
			this.cvsModuleLabel.Font = ((System.Drawing.Font)(resources.GetObject("cvsModuleLabel.Font")));
			this.cvsModuleLabel.Image = ((System.Drawing.Image)(resources.GetObject("cvsModuleLabel.Image")));
			this.cvsModuleLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cvsModuleLabel.ImageAlign")));
			this.cvsModuleLabel.ImageIndex = ((int)(resources.GetObject("cvsModuleLabel.ImageIndex")));
			this.cvsModuleLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cvsModuleLabel.ImeMode")));
			this.cvsModuleLabel.Location = ((System.Drawing.Point)(resources.GetObject("cvsModuleLabel.Location")));
			this.cvsModuleLabel.Name = "cvsModuleLabel";
			this.cvsModuleLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cvsModuleLabel.RightToLeft")));
			this.cvsModuleLabel.Size = ((System.Drawing.Size)(resources.GetObject("cvsModuleLabel.Size")));
			this.cvsModuleLabel.TabIndex = ((int)(resources.GetObject("cvsModuleLabel.TabIndex")));
			this.cvsModuleLabel.Text = resources.GetString("cvsModuleLabel.Text");
			this.cvsModuleLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cvsModuleLabel.TextAlign")));
			this.cvsModuleLabel.Visible = ((bool)(resources.GetObject("cvsModuleLabel.Visible")));
			// 
			// slnPathLabel
			// 
			this.slnPathLabel.AccessibleDescription = resources.GetString("slnPathLabel.AccessibleDescription");
			this.slnPathLabel.AccessibleName = resources.GetString("slnPathLabel.AccessibleName");
			this.slnPathLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("slnPathLabel.Anchor")));
			this.slnPathLabel.AutoSize = ((bool)(resources.GetObject("slnPathLabel.AutoSize")));
			this.slnPathLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("slnPathLabel.Dock")));
			this.slnPathLabel.Enabled = ((bool)(resources.GetObject("slnPathLabel.Enabled")));
			this.slnPathLabel.Font = ((System.Drawing.Font)(resources.GetObject("slnPathLabel.Font")));
			this.slnPathLabel.Image = ((System.Drawing.Image)(resources.GetObject("slnPathLabel.Image")));
			this.slnPathLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("slnPathLabel.ImageAlign")));
			this.slnPathLabel.ImageIndex = ((int)(resources.GetObject("slnPathLabel.ImageIndex")));
			this.slnPathLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("slnPathLabel.ImeMode")));
			this.slnPathLabel.Location = ((System.Drawing.Point)(resources.GetObject("slnPathLabel.Location")));
			this.slnPathLabel.Name = "slnPathLabel";
			this.slnPathLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("slnPathLabel.RightToLeft")));
			this.slnPathLabel.Size = ((System.Drawing.Size)(resources.GetObject("slnPathLabel.Size")));
			this.slnPathLabel.TabIndex = ((int)(resources.GetObject("slnPathLabel.TabIndex")));
			this.slnPathLabel.Text = resources.GetString("slnPathLabel.Text");
			this.slnPathLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("slnPathLabel.TextAlign")));
			this.slnPathLabel.Visible = ((bool)(resources.GetObject("slnPathLabel.Visible")));
			// 
			// destPathTextBox
			// 
			this.destPathTextBox.AccessibleDescription = resources.GetString("destPathTextBox.AccessibleDescription");
			this.destPathTextBox.AccessibleName = resources.GetString("destPathTextBox.AccessibleName");
			this.destPathTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("destPathTextBox.Anchor")));
			this.destPathTextBox.AutoSize = ((bool)(resources.GetObject("destPathTextBox.AutoSize")));
			this.destPathTextBox.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("destPathTextBox.BackgroundImage")));
			this.destPathTextBox.CausesValidation = false;
			this.destPathTextBox.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("destPathTextBox.Dock")));
			this.destPathTextBox.Enabled = ((bool)(resources.GetObject("destPathTextBox.Enabled")));
			this.destPathTextBox.Font = ((System.Drawing.Font)(resources.GetObject("destPathTextBox.Font")));
			this.destPathTextBox.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("destPathTextBox.ImeMode")));
			this.destPathTextBox.Location = ((System.Drawing.Point)(resources.GetObject("destPathTextBox.Location")));
			this.destPathTextBox.MaxLength = ((int)(resources.GetObject("destPathTextBox.MaxLength")));
			this.destPathTextBox.Multiline = ((bool)(resources.GetObject("destPathTextBox.Multiline")));
			this.destPathTextBox.Name = "destPathTextBox";
			this.destPathTextBox.PasswordChar = ((char)(resources.GetObject("destPathTextBox.PasswordChar")));
			this.destPathTextBox.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("destPathTextBox.RightToLeft")));
			this.destPathTextBox.ScrollBars = ((System.Windows.Forms.ScrollBars)(resources.GetObject("destPathTextBox.ScrollBars")));
			this.destPathTextBox.Size = ((System.Drawing.Size)(resources.GetObject("destPathTextBox.Size")));
			this.destPathTextBox.TabIndex = ((int)(resources.GetObject("destPathTextBox.TabIndex")));
			this.destPathTextBox.Text = resources.GetString("destPathTextBox.Text");
			this.destPathTextBox.TextAlign = ((System.Windows.Forms.HorizontalAlignment)(resources.GetObject("destPathTextBox.TextAlign")));
			this.destPathTextBox.Visible = ((bool)(resources.GetObject("destPathTextBox.Visible")));
			this.destPathTextBox.WordWrap = ((bool)(resources.GetObject("destPathTextBox.WordWrap")));
			this.destPathTextBox.TextChanged += new System.EventHandler(this.destPathTextBox_TextChanged);
			// 
			// cvsTagLbl
			// 
			this.cvsTagLbl.AccessibleDescription = resources.GetString("cvsTagLbl.AccessibleDescription");
			this.cvsTagLbl.AccessibleName = resources.GetString("cvsTagLbl.AccessibleName");
			this.cvsTagLbl.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cvsTagLbl.Anchor")));
			this.cvsTagLbl.AutoSize = ((bool)(resources.GetObject("cvsTagLbl.AutoSize")));
			this.cvsTagLbl.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cvsTagLbl.Dock")));
			this.cvsTagLbl.Enabled = ((bool)(resources.GetObject("cvsTagLbl.Enabled")));
			this.cvsTagLbl.Font = ((System.Drawing.Font)(resources.GetObject("cvsTagLbl.Font")));
			this.cvsTagLbl.Image = ((System.Drawing.Image)(resources.GetObject("cvsTagLbl.Image")));
			this.cvsTagLbl.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cvsTagLbl.ImageAlign")));
			this.cvsTagLbl.ImageIndex = ((int)(resources.GetObject("cvsTagLbl.ImageIndex")));
			this.cvsTagLbl.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cvsTagLbl.ImeMode")));
			this.cvsTagLbl.Location = ((System.Drawing.Point)(resources.GetObject("cvsTagLbl.Location")));
			this.cvsTagLbl.Name = "cvsTagLbl";
			this.cvsTagLbl.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cvsTagLbl.RightToLeft")));
			this.cvsTagLbl.Size = ((System.Drawing.Size)(resources.GetObject("cvsTagLbl.Size")));
			this.cvsTagLbl.TabIndex = ((int)(resources.GetObject("cvsTagLbl.TabIndex")));
			this.cvsTagLbl.Text = resources.GetString("cvsTagLbl.Text");
			this.cvsTagLbl.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cvsTagLbl.TextAlign")));
			this.cvsTagLbl.Visible = ((bool)(resources.GetObject("cvsTagLbl.Visible")));
			// 
			// slnPathBtn
			// 
			this.slnPathBtn.AccessibleDescription = resources.GetString("slnPathBtn.AccessibleDescription");
			this.slnPathBtn.AccessibleName = resources.GetString("slnPathBtn.AccessibleName");
			this.slnPathBtn.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("slnPathBtn.Anchor")));
			this.slnPathBtn.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("slnPathBtn.BackgroundImage")));
			this.slnPathBtn.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("slnPathBtn.Dock")));
			this.slnPathBtn.Enabled = ((bool)(resources.GetObject("slnPathBtn.Enabled")));
			this.slnPathBtn.FlatStyle = ((System.Windows.Forms.FlatStyle)(resources.GetObject("slnPathBtn.FlatStyle")));
			this.slnPathBtn.Font = ((System.Drawing.Font)(resources.GetObject("slnPathBtn.Font")));
			this.slnPathBtn.Image = ((System.Drawing.Image)(resources.GetObject("slnPathBtn.Image")));
			this.slnPathBtn.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("slnPathBtn.ImageAlign")));
			this.slnPathBtn.ImageIndex = ((int)(resources.GetObject("slnPathBtn.ImageIndex")));
			this.slnPathBtn.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("slnPathBtn.ImeMode")));
			this.slnPathBtn.Location = ((System.Drawing.Point)(resources.GetObject("slnPathBtn.Location")));
			this.slnPathBtn.Name = "slnPathBtn";
			this.slnPathBtn.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("slnPathBtn.RightToLeft")));
			this.slnPathBtn.Size = ((System.Drawing.Size)(resources.GetObject("slnPathBtn.Size")));
			this.slnPathBtn.TabIndex = ((int)(resources.GetObject("slnPathBtn.TabIndex")));
			this.slnPathBtn.Text = resources.GetString("slnPathBtn.Text");
			this.slnPathBtn.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("slnPathBtn.TextAlign")));
			this.slnPathBtn.Visible = ((bool)(resources.GetObject("slnPathBtn.Visible")));
			this.slnPathBtn.Click += new System.EventHandler(this.slnPathBtn_Click);
			// 
			// cvsModuleDropDown
			// 
			this.cvsModuleDropDown.AccessibleDescription = resources.GetString("cvsModuleDropDown.AccessibleDescription");
			this.cvsModuleDropDown.AccessibleName = resources.GetString("cvsModuleDropDown.AccessibleName");
			this.cvsModuleDropDown.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cvsModuleDropDown.Anchor")));
			this.cvsModuleDropDown.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cvsModuleDropDown.BackgroundImage")));
			this.cvsModuleDropDown.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cvsModuleDropDown.Dock")));
			this.cvsModuleDropDown.Enabled = ((bool)(resources.GetObject("cvsModuleDropDown.Enabled")));
			this.cvsModuleDropDown.Font = ((System.Drawing.Font)(resources.GetObject("cvsModuleDropDown.Font")));
			this.cvsModuleDropDown.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cvsModuleDropDown.ImeMode")));
			this.cvsModuleDropDown.IntegralHeight = ((bool)(resources.GetObject("cvsModuleDropDown.IntegralHeight")));
			this.cvsModuleDropDown.ItemHeight = ((int)(resources.GetObject("cvsModuleDropDown.ItemHeight")));
			this.cvsModuleDropDown.Location = ((System.Drawing.Point)(resources.GetObject("cvsModuleDropDown.Location")));
			this.cvsModuleDropDown.MaxDropDownItems = ((int)(resources.GetObject("cvsModuleDropDown.MaxDropDownItems")));
			this.cvsModuleDropDown.MaxLength = ((int)(resources.GetObject("cvsModuleDropDown.MaxLength")));
			this.cvsModuleDropDown.Name = "cvsModuleDropDown";
			this.cvsModuleDropDown.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cvsModuleDropDown.RightToLeft")));
			this.cvsModuleDropDown.Size = ((System.Drawing.Size)(resources.GetObject("cvsModuleDropDown.Size")));
			this.cvsModuleDropDown.TabIndex = ((int)(resources.GetObject("cvsModuleDropDown.TabIndex")));
			this.cvsModuleDropDown.Text = resources.GetString("cvsModuleDropDown.Text");
			this.cvsModuleDropDown.Visible = ((bool)(resources.GetObject("cvsModuleDropDown.Visible")));
			this.cvsModuleDropDown.TextChanged += new System.EventHandler(this.cvsModuleDropDown_TextChanged);
			// 
			// cvsTagDropDown
			// 
			this.cvsTagDropDown.AccessibleDescription = resources.GetString("cvsTagDropDown.AccessibleDescription");
			this.cvsTagDropDown.AccessibleName = resources.GetString("cvsTagDropDown.AccessibleName");
			this.cvsTagDropDown.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cvsTagDropDown.Anchor")));
			this.cvsTagDropDown.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("cvsTagDropDown.BackgroundImage")));
			this.cvsTagDropDown.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cvsTagDropDown.Dock")));
			this.cvsTagDropDown.Enabled = ((bool)(resources.GetObject("cvsTagDropDown.Enabled")));
			this.cvsTagDropDown.Font = ((System.Drawing.Font)(resources.GetObject("cvsTagDropDown.Font")));
			this.cvsTagDropDown.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cvsTagDropDown.ImeMode")));
			this.cvsTagDropDown.IntegralHeight = ((bool)(resources.GetObject("cvsTagDropDown.IntegralHeight")));
			this.cvsTagDropDown.ItemHeight = ((int)(resources.GetObject("cvsTagDropDown.ItemHeight")));
			this.cvsTagDropDown.Location = ((System.Drawing.Point)(resources.GetObject("cvsTagDropDown.Location")));
			this.cvsTagDropDown.MaxDropDownItems = ((int)(resources.GetObject("cvsTagDropDown.MaxDropDownItems")));
			this.cvsTagDropDown.MaxLength = ((int)(resources.GetObject("cvsTagDropDown.MaxLength")));
			this.cvsTagDropDown.Name = "cvsTagDropDown";
			this.cvsTagDropDown.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cvsTagDropDown.RightToLeft")));
			this.cvsTagDropDown.Size = ((System.Drawing.Size)(resources.GetObject("cvsTagDropDown.Size")));
			this.cvsTagDropDown.TabIndex = ((int)(resources.GetObject("cvsTagDropDown.TabIndex")));
			this.cvsTagDropDown.Text = resources.GetString("cvsTagDropDown.Text");
			this.cvsTagDropDown.Visible = ((bool)(resources.GetObject("cvsTagDropDown.Visible")));
			// 
			// cvsConnectionStringLabel
			// 
			this.cvsConnectionStringLabel.AccessibleDescription = resources.GetString("cvsConnectionStringLabel.AccessibleDescription");
			this.cvsConnectionStringLabel.AccessibleName = resources.GetString("cvsConnectionStringLabel.AccessibleName");
			this.cvsConnectionStringLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("cvsConnectionStringLabel.Anchor")));
			this.cvsConnectionStringLabel.AutoSize = ((bool)(resources.GetObject("cvsConnectionStringLabel.AutoSize")));
			this.cvsConnectionStringLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("cvsConnectionStringLabel.Dock")));
			this.cvsConnectionStringLabel.Enabled = ((bool)(resources.GetObject("cvsConnectionStringLabel.Enabled")));
			this.cvsConnectionStringLabel.Font = ((System.Drawing.Font)(resources.GetObject("cvsConnectionStringLabel.Font")));
			this.cvsConnectionStringLabel.Image = ((System.Drawing.Image)(resources.GetObject("cvsConnectionStringLabel.Image")));
			this.cvsConnectionStringLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cvsConnectionStringLabel.ImageAlign")));
			this.cvsConnectionStringLabel.ImageIndex = ((int)(resources.GetObject("cvsConnectionStringLabel.ImageIndex")));
			this.cvsConnectionStringLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("cvsConnectionStringLabel.ImeMode")));
			this.cvsConnectionStringLabel.Location = ((System.Drawing.Point)(resources.GetObject("cvsConnectionStringLabel.Location")));
			this.cvsConnectionStringLabel.Name = "cvsConnectionStringLabel";
			this.cvsConnectionStringLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("cvsConnectionStringLabel.RightToLeft")));
			this.cvsConnectionStringLabel.Size = ((System.Drawing.Size)(resources.GetObject("cvsConnectionStringLabel.Size")));
			this.cvsConnectionStringLabel.TabIndex = ((int)(resources.GetObject("cvsConnectionStringLabel.TabIndex")));
			this.cvsConnectionStringLabel.Text = resources.GetString("cvsConnectionStringLabel.Text");
			this.cvsConnectionStringLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("cvsConnectionStringLabel.TextAlign")));
			this.cvsConnectionStringLabel.Visible = ((bool)(resources.GetObject("cvsConnectionStringLabel.Visible")));
			// 
			// solutionLocationLabel
			// 
			this.solutionLocationLabel.AccessibleDescription = resources.GetString("solutionLocationLabel.AccessibleDescription");
			this.solutionLocationLabel.AccessibleName = resources.GetString("solutionLocationLabel.AccessibleName");
			this.solutionLocationLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(resources.GetObject("solutionLocationLabel.Anchor")));
			this.solutionLocationLabel.AutoSize = ((bool)(resources.GetObject("solutionLocationLabel.AutoSize")));
			this.solutionLocationLabel.Dock = ((System.Windows.Forms.DockStyle)(resources.GetObject("solutionLocationLabel.Dock")));
			this.solutionLocationLabel.Enabled = ((bool)(resources.GetObject("solutionLocationLabel.Enabled")));
			this.solutionLocationLabel.Font = ((System.Drawing.Font)(resources.GetObject("solutionLocationLabel.Font")));
			this.solutionLocationLabel.Image = ((System.Drawing.Image)(resources.GetObject("solutionLocationLabel.Image")));
			this.solutionLocationLabel.ImageAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("solutionLocationLabel.ImageAlign")));
			this.solutionLocationLabel.ImageIndex = ((int)(resources.GetObject("solutionLocationLabel.ImageIndex")));
			this.solutionLocationLabel.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("solutionLocationLabel.ImeMode")));
			this.solutionLocationLabel.Location = ((System.Drawing.Point)(resources.GetObject("solutionLocationLabel.Location")));
			this.solutionLocationLabel.Name = "solutionLocationLabel";
			this.solutionLocationLabel.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("solutionLocationLabel.RightToLeft")));
			this.solutionLocationLabel.Size = ((System.Drawing.Size)(resources.GetObject("solutionLocationLabel.Size")));
			this.solutionLocationLabel.TabIndex = ((int)(resources.GetObject("solutionLocationLabel.TabIndex")));
			this.solutionLocationLabel.Text = resources.GetString("solutionLocationLabel.Text");
			this.solutionLocationLabel.TextAlign = ((System.Drawing.ContentAlignment)(resources.GetObject("solutionLocationLabel.TextAlign")));
			this.solutionLocationLabel.Visible = ((bool)(resources.GetObject("solutionLocationLabel.Visible")));
			// 
			// FormOpenSolutionFromCvs
			// 
			this.AcceptButton = this.okBtn;
			this.AccessibleDescription = resources.GetString("$this.AccessibleDescription");
			this.AccessibleName = resources.GetString("$this.AccessibleName");
			this.AutoScaleBaseSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScaleBaseSize")));
			this.AutoScroll = ((bool)(resources.GetObject("$this.AutoScroll")));
			this.AutoScrollMargin = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMargin")));
			this.AutoScrollMinSize = ((System.Drawing.Size)(resources.GetObject("$this.AutoScrollMinSize")));
			this.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("$this.BackgroundImage")));
			this.CancelButton = this.cancelBtn;
			this.ClientSize = ((System.Drawing.Size)(resources.GetObject("$this.ClientSize")));
			this.ControlBox = false;
			this.Controls.Add(this.solutionLocationLabel);
			this.Controls.Add(this.cvsConnectionStringLabel);
			this.Controls.Add(this.cvsTagDropDown);
			this.Controls.Add(this.cvsModuleDropDown);
			this.Controls.Add(this.cvsTagLbl);
			this.Controls.Add(this.slnPathBtn);
			this.Controls.Add(this.destPathTextBox);
			this.Controls.Add(this.slnPathLabel);
			this.Controls.Add(this.cvsModuleLabel);
			this.Controls.Add(this.okBtn);
			this.Controls.Add(this.cancelBtn);
			this.Enabled = ((bool)(resources.GetObject("$this.Enabled")));
			this.Font = ((System.Drawing.Font)(resources.GetObject("$this.Font")));
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.ImeMode = ((System.Windows.Forms.ImeMode)(resources.GetObject("$this.ImeMode")));
			this.Location = ((System.Drawing.Point)(resources.GetObject("$this.Location")));
			this.MaximumSize = ((System.Drawing.Size)(resources.GetObject("$this.MaximumSize")));
			this.MinimumSize = ((System.Drawing.Size)(resources.GetObject("$this.MinimumSize")));
			this.Name = "FormOpenSolutionFromCvs";
			this.RightToLeft = ((System.Windows.Forms.RightToLeft)(resources.GetObject("$this.RightToLeft")));
			this.StartPosition = ((System.Windows.Forms.FormStartPosition)(resources.GetObject("$this.StartPosition")));
			this.Text = resources.GetString("$this.Text");
			this.Closing += new System.ComponentModel.CancelEventHandler(this.FormOpenSolutionFromCvs_Closing);
			this.Load += new System.EventHandler(this.FormOpenSolutionFromCvs_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void FormOpenSolutionFromCvs_Load(object sender, System.EventArgs e)
		{
		
		}

		private void okBtn_Click(object sender, System.EventArgs e)
		{
			// perform some validation before we decide to allow user to 
			// close dialog

			allowClose = false;

			try
			{
				string destPath = Path.GetDirectoryName( destPathTextBox.Text );
			}
			catch(ArgumentException)
			{
				cont_.UIShell.ExclamationMessage(this, "MSGBOX_DIRECTORY_INVALID" );
				destPathTextBox.Focus();
				return;
			}


			// everythings groovy, close the dialog
			allowClose = true;
		}

		private void FormOpenSolutionFromCvs_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = !allowClose;
		
		}

		private void cancelBtn_Click(object sender, System.EventArgs e)
		{
			allowClose = true;
		}

		private void slnPathBtn_Click(object sender, System.EventArgs e)
		{
			// allow the use to select the path where solution files will be placed
			FolderBrowserDialog browser = new FolderBrowserDialog();
			if( destPathTextBox.Text != string.Empty )
			{
				browser.SelectedPath = destPathTextBox.Text;
			}
			else
			{
				browser.SelectedPath = cont_.Model.CurrentConnection.WorkingDirectory;
			}

			browser.ShowNewFolderButton = true;			
			
			if( browser.ShowDialog() == DialogResult.OK )
			{
				destPathTextBox.Text = browser.SelectedPath;
			}
		}
		// update caption on for to tell users where solution will be placed
		private void cvsModuleDropDown_TextChanged(object sender, System.EventArgs e)
		{
			solutionLocationLabel.Text = string.Format( "{0} {1}", solutionMsg_, this.SolutionPath );
		}

		private void destPathTextBox_TextChanged(object sender, System.EventArgs e)
		{
			solutionLocationLabel.Text = string.Format( "{0} {1}", solutionMsg_, this.SolutionPath );		
		}

		/// <summary>
		/// Returns the path where cvs will put the solution, this is typically
		/// workingdir\module for example, if workingdir = c:\foo\bar and module = baz/booble
		/// then c:\foo\bar\baz\booble will be returned.
		/// </summary>
		internal string SolutionPath
		{
			get
			{
				return Path.Combine( destPathTextBox.Text, cvsModuleDropDown.Text.Replace("/", @"\"));
			}
		}

	}
}
