using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Resources;


namespace SharpCvsAddIn
{
	/// <summary>
	/// Summary description for FormSetCvsConnection.
	/// </summary>
	public class FormSetCvsConnection : System.Windows.Forms.Form
	{
		/// <summary>
		/// Class used to format cvs connection protocol information
		/// in drop down lists
		/// </summary>
		public class Protocol
		{
			public readonly string Name;
			private string description_;

			public Protocol( string name, string description )
			{
				Name = name;
				description_ = description;
			}

			public override string ToString()
			{
				return string.Format( "{0} ({1})", Name, description_ );
			}

		}

		private System.Windows.Forms.Button okBtn;
		private System.Windows.Forms.Button cancelBtn;
		internal System.Windows.Forms.TextBox cvsPasswordTxtBox;
		private System.Windows.Forms.Label cvsProtocolLabel;
		private System.Windows.Forms.Label userNameLabel;
		internal System.Windows.Forms.TextBox userNameTextBox;
		internal System.Windows.Forms.Label cvsPasswordLabel;
		internal System.Windows.Forms.TextBox cvsHostTxtBox;
		internal System.Windows.Forms.Label cvsHostNameLabel;
		internal System.Windows.Forms.Label cvsPortLabel;
		internal System.Windows.Forms.Label cvsRootLabel;
		internal System.Windows.Forms.TextBox cvsRootTextBox;
		internal System.Windows.Forms.TextBox cvsPortTextBox;
		internal System.Windows.Forms.Label workingDirLabel;
		internal System.Windows.Forms.TextBox workingDirTextBox;
		internal System.Windows.Forms.ComboBox protocolList;

		private bool allowClose_ = true;
		private System.Windows.Forms.Button slnPathBtn;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private IController controller_;

		public FormSetCvsConnection(IController controller)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			controller_ = controller;
			ResourceManager rm = controller_.Model.ResourceManager;

			this.Text = rm.GetString("FORM_SET_CVS_REPOSITORY_TITLE");
			cvsProtocolLabel.Text = rm.GetString("FORM_CVS_PROTOCOL");
			userNameLabel.Text = rm.GetString("FORM_USER_NAME" );
			cvsPasswordLabel.Text = rm.GetString("FORM_CVS_PASSWORD");
			cvsHostNameLabel.Text = rm.GetString("FORM_CVS_HOST" );
			cvsPortLabel.Text = rm.GetString("FORM_CVS_PORT");
			cvsRootLabel.Text = rm.GetString("FORM_CVS_ROOT");
			workingDirLabel.Text = rm.GetString("FORM_WORKING_DIRECTORY");
			okBtn.Text = rm.GetString("FORM_OK_BUTTON");
			cancelBtn.Text = rm.GetString("FORM_CANCEL_BUTTON");


			// load up the list box with connection protocols
			protocolList.Items.Add( new Protocol( "pserver", rm.GetString( "FORM_PSERVER_DESC" ) ) );
			protocolList.Items.Add( new Protocol( "ext", rm.GetString("FORM_EXT_DESC" ) ) );
			protocolList.Items.Add( new Protocol( "ssh", rm.GetString( "FORM_SSH_DESC" ) ) );
			protocolList.Items.Add( new Protocol( "gserver", rm.GetString("FORM_GSERVER_DESC")));
			protocolList.Items.Add( new Protocol( "local", rm.GetString("FORM_LOCAL_DESC")));
			protocolList.Items.Add( new Protocol( "fork", rm.GetString("FORM_LOCAL_DESC")));	
			protocolList.Items.Add( new Protocol( "kserver", rm.GetString( "FORM_KERBOROS_DESC" ) ) );
			protocolList.Items.Add( new Protocol( "sspi", rm.GetString("FORM_SSPI_DESC")));

			if( controller_.CurrentConnection != null )
			{
				Persistance.Connection conn = controller_.CurrentConnection;

				userNameTextBox.Text = conn.User;
				cvsHostTxtBox.Text = conn.Host;
				cvsPortTextBox.Text = conn.Port.ToString();
				cvsRootTextBox.Text = conn.Repository;
				workingDirTextBox.Text = conn.WorkingDirectory;
				cvsPasswordTxtBox.Text = conn.Password;
				if( conn.Protocol != string.Empty )
				{
					foreach( object protocol in protocolList.Items )
					{
						if( ((Protocol)protocol).Name == conn.Protocol )
						{
							protocolList.SelectedItem = protocol;
							break;
						}
					}
				}
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
			this.okBtn = new System.Windows.Forms.Button();
			this.cancelBtn = new System.Windows.Forms.Button();
			this.cvsPasswordLabel = new System.Windows.Forms.Label();
			this.cvsPasswordTxtBox = new System.Windows.Forms.TextBox();
			this.cvsHostTxtBox = new System.Windows.Forms.TextBox();
			this.cvsProtocolLabel = new System.Windows.Forms.Label();
			this.userNameLabel = new System.Windows.Forms.Label();
			this.userNameTextBox = new System.Windows.Forms.TextBox();
			this.cvsHostNameLabel = new System.Windows.Forms.Label();
			this.cvsPortLabel = new System.Windows.Forms.Label();
			this.cvsPortTextBox = new System.Windows.Forms.TextBox();
			this.cvsRootTextBox = new System.Windows.Forms.TextBox();
			this.cvsRootLabel = new System.Windows.Forms.Label();
			this.workingDirTextBox = new System.Windows.Forms.TextBox();
			this.workingDirLabel = new System.Windows.Forms.Label();
			this.protocolList = new System.Windows.Forms.ComboBox();
			this.slnPathBtn = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// okBtn
			// 
			this.okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okBtn.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.okBtn.Location = new System.Drawing.Point(376, 240);
			this.okBtn.Name = "okBtn";
			this.okBtn.TabIndex = 9;
			this.okBtn.Text = "&OK";
			this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.CausesValidation = false;
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.cancelBtn.Location = new System.Drawing.Point(464, 240);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.TabIndex = 10;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// cvsPasswordLabel
			// 
			this.cvsPasswordLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.cvsPasswordLabel.Location = new System.Drawing.Point(8, 72);
			this.cvsPasswordLabel.Name = "cvsPasswordLabel";
			this.cvsPasswordLabel.Size = new System.Drawing.Size(152, 20);
			this.cvsPasswordLabel.TabIndex = 17;
			this.cvsPasswordLabel.Text = "CVS Password";
			// 
			// cvsPasswordTxtBox
			// 
			this.cvsPasswordTxtBox.CausesValidation = false;
			this.cvsPasswordTxtBox.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.cvsPasswordTxtBox.Location = new System.Drawing.Point(168, 72);
			this.cvsPasswordTxtBox.Name = "cvsPasswordTxtBox";
			this.cvsPasswordTxtBox.PasswordChar = '*';
			this.cvsPasswordTxtBox.Size = new System.Drawing.Size(376, 20);
			this.cvsPasswordTxtBox.TabIndex = 3;
			this.cvsPasswordTxtBox.Text = "";
			// 
			// cvsHostTxtBox
			// 
			this.cvsHostTxtBox.CausesValidation = false;
			this.cvsHostTxtBox.Location = new System.Drawing.Point(168, 104);
			this.cvsHostTxtBox.Name = "cvsHostTxtBox";
			this.cvsHostTxtBox.Size = new System.Drawing.Size(376, 20);
			this.cvsHostTxtBox.TabIndex = 4;
			this.cvsHostTxtBox.Text = "";
			// 
			// cvsProtocolLabel
			// 
			this.cvsProtocolLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.cvsProtocolLabel.Location = new System.Drawing.Point(8, 8);
			this.cvsProtocolLabel.Name = "cvsProtocolLabel";
			this.cvsProtocolLabel.Size = new System.Drawing.Size(152, 20);
			this.cvsProtocolLabel.TabIndex = 15;
			this.cvsProtocolLabel.Text = "Connection Protocol";
			// 
			// userNameLabel
			// 
			this.userNameLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.userNameLabel.Location = new System.Drawing.Point(8, 40);
			this.userNameLabel.Name = "userNameLabel";
			this.userNameLabel.Size = new System.Drawing.Size(152, 20);
			this.userNameLabel.TabIndex = 20;
			this.userNameLabel.Text = "User Name";
			// 
			// userNameTextBox
			// 
			this.userNameTextBox.CausesValidation = false;
			this.userNameTextBox.Location = new System.Drawing.Point(168, 40);
			this.userNameTextBox.Name = "userNameTextBox";
			this.userNameTextBox.Size = new System.Drawing.Size(376, 20);
			this.userNameTextBox.TabIndex = 2;
			this.userNameTextBox.Text = "";
			// 
			// cvsHostNameLabel
			// 
			this.cvsHostNameLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.cvsHostNameLabel.Location = new System.Drawing.Point(8, 104);
			this.cvsHostNameLabel.Name = "cvsHostNameLabel";
			this.cvsHostNameLabel.Size = new System.Drawing.Size(152, 20);
			this.cvsHostNameLabel.TabIndex = 22;
			this.cvsHostNameLabel.Text = "CVS Host";
			// 
			// cvsPortLabel
			// 
			this.cvsPortLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.cvsPortLabel.Location = new System.Drawing.Point(8, 136);
			this.cvsPortLabel.Name = "cvsPortLabel";
			this.cvsPortLabel.Size = new System.Drawing.Size(152, 20);
			this.cvsPortLabel.TabIndex = 23;
			this.cvsPortLabel.Text = "CVS Port";
			// 
			// cvsPortTextBox
			// 
			this.cvsPortTextBox.CausesValidation = false;
			this.cvsPortTextBox.Location = new System.Drawing.Point(168, 136);
			this.cvsPortTextBox.Name = "cvsPortTextBox";
			this.cvsPortTextBox.Size = new System.Drawing.Size(96, 20);
			this.cvsPortTextBox.TabIndex = 5;
			this.cvsPortTextBox.Text = "2401";
			// 
			// cvsRootTextBox
			// 
			this.cvsRootTextBox.CausesValidation = false;
			this.cvsRootTextBox.Location = new System.Drawing.Point(168, 168);
			this.cvsRootTextBox.Name = "cvsRootTextBox";
			this.cvsRootTextBox.Size = new System.Drawing.Size(376, 20);
			this.cvsRootTextBox.TabIndex = 6;
			this.cvsRootTextBox.Text = "";
			// 
			// cvsRootLabel
			// 
			this.cvsRootLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.cvsRootLabel.Location = new System.Drawing.Point(8, 168);
			this.cvsRootLabel.Name = "cvsRootLabel";
			this.cvsRootLabel.Size = new System.Drawing.Size(152, 20);
			this.cvsRootLabel.TabIndex = 26;
			this.cvsRootLabel.Text = "CVS Root";
			// 
			// workingDirTextBox
			// 
			this.workingDirTextBox.CausesValidation = false;
			this.workingDirTextBox.Location = new System.Drawing.Point(168, 200);
			this.workingDirTextBox.Name = "workingDirTextBox";
			this.workingDirTextBox.Size = new System.Drawing.Size(376, 20);
			this.workingDirTextBox.TabIndex = 7;
			this.workingDirTextBox.Text = "";
			// 
			// workingDirLabel
			// 
			this.workingDirLabel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.workingDirLabel.Location = new System.Drawing.Point(8, 200);
			this.workingDirLabel.Name = "workingDirLabel";
			this.workingDirLabel.Size = new System.Drawing.Size(152, 20);
			this.workingDirLabel.TabIndex = 28;
			this.workingDirLabel.Text = "Working Directory";
			// 
			// protocolList
			// 
			this.protocolList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.protocolList.Location = new System.Drawing.Point(168, 8);
			this.protocolList.Name = "protocolList";
			this.protocolList.Size = new System.Drawing.Size(376, 21);
			this.protocolList.TabIndex = 1;
			// 
			// slnPathBtn
			// 
			this.slnPathBtn.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.slnPathBtn.Location = new System.Drawing.Point(552, 200);
			this.slnPathBtn.Name = "slnPathBtn";
			this.slnPathBtn.Size = new System.Drawing.Size(24, 20);
			this.slnPathBtn.TabIndex = 8;
			this.slnPathBtn.Text = "...";
			this.slnPathBtn.Click += new System.EventHandler(this.slnPathBtn_Click);
			// 
			// FormSetCvsConnection
			// 
			this.AcceptButton = this.okBtn;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(610, 280);
			this.ControlBox = false;
			this.Controls.Add(this.slnPathBtn);
			this.Controls.Add(this.protocolList);
			this.Controls.Add(this.workingDirLabel);
			this.Controls.Add(this.workingDirTextBox);
			this.Controls.Add(this.cvsRootTextBox);
			this.Controls.Add(this.cvsPortTextBox);
			this.Controls.Add(this.userNameTextBox);
			this.Controls.Add(this.cvsPasswordTxtBox);
			this.Controls.Add(this.cvsHostTxtBox);
			this.Controls.Add(this.cvsRootLabel);
			this.Controls.Add(this.cvsPortLabel);
			this.Controls.Add(this.cvsHostNameLabel);
			this.Controls.Add(this.userNameLabel);
			this.Controls.Add(this.cvsPasswordLabel);
			this.Controls.Add(this.cvsProtocolLabel);
			this.Controls.Add(this.okBtn);
			this.Controls.Add(this.cancelBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.MinimizeBox = false;
			this.Name = "FormSetCvsConnection";
			this.Text = "FormSetCvsConnection";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.FormSetCvsConnection_Closing);
			this.Load += new System.EventHandler(this.FormSetCvsConnection_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void FormSetCvsConnection_Load(object sender, System.EventArgs e)
		{
		
		}

		private void okBtn_Click(object sender, System.EventArgs e)
		{
			// block close until all validation succeeds
			allowClose_ = false;

			if( cvsRootTextBox.Text == string.Empty )
			{
				controller_.UIShell.ExclamationMessage(this, "MSGBOX_CVS_ROOT_MISSING" );
				return;
			}

			if( workingDirTextBox.Text == string.Empty )
			{
				controller_.UIShell.ExclamationMessage(this, "MSGBOX_SOLUTION_PATH_MISSING" );
				return;
			}
			
			// validate directory
			try
			{
				string unused = Path.GetDirectoryName( workingDirTextBox.Text );
			}
			catch(Exception)
			{
				controller_.UIShell.ExclamationMessage( this, "MSGBOX_DIRECTORY_INVALID" );

			}



			allowClose_ = true;
		
		}

		private void FormSetCvsConnection_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = !allowClose_;
		}

		private void cancelBtn_Click(object sender, System.EventArgs e)
		{
			allowClose_ = true;
		}

		private void slnPathBtn_Click(object sender, System.EventArgs e)
		{
			// allow the use to select the path where solution files will be placed
			FolderBrowserDialog browser = new FolderBrowserDialog();
			browser.ShowNewFolderButton = true;			
			
			if( browser.ShowDialog(this) == DialogResult.OK )
			{
				workingDirTextBox.Text = browser.SelectedPath;
			}		
		}

	}
}
