using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;


namespace SharpCvsAddIn
{
	/// <summary>
	/// Summary description for FormGetPassword.
	/// </summary>
	public class FormGetPassword : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button okBtn;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.Label passwordLabel;
		internal System.Windows.Forms.TextBox passwordTextBox;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		//private bool allowClose = true;

		public FormGetPassword(IController cont)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
				
			this.Text = cont.Model.ResourceManager.GetString("FORM_SET_CVS_PASSWORD");
			okBtn.Text = cont.Model.ResourceManager.GetString("FORM_OK_BUTTON");
			cancelBtn.Text = cont.Model.ResourceManager.GetString("FORM_CANCEL_BUTTON");
			passwordLabel.Text = cont.Model.ResourceManager.GetString("FORM_CVS_PASSWORD");

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
			this.passwordLabel = new System.Windows.Forms.Label();
			this.passwordTextBox = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// okBtn
			// 
			this.okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okBtn.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.okBtn.Location = new System.Drawing.Point(176, 48);
			this.okBtn.Name = "okBtn";
			this.okBtn.TabIndex = 2;
			this.okBtn.Text = "&OK";
			// 
			// cancelBtn
			// 
			this.cancelBtn.CausesValidation = false;
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.cancelBtn.Location = new System.Drawing.Point(264, 48);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.TabIndex = 3;
			this.cancelBtn.Text = "Cancel";
			// 
			// passwordLabel
			// 
			this.passwordLabel.Location = new System.Drawing.Point(8, 8);
			this.passwordLabel.Name = "passwordLabel";
			this.passwordLabel.Size = new System.Drawing.Size(104, 24);
			this.passwordLabel.TabIndex = 9;
			this.passwordLabel.Text = "label1";
			// 
			// passwordTextBox
			// 
			this.passwordTextBox.Location = new System.Drawing.Point(136, 8);
			this.passwordTextBox.Name = "passwordTextBox";
			this.passwordTextBox.PasswordChar = '*';
			this.passwordTextBox.Size = new System.Drawing.Size(200, 20);
			this.passwordTextBox.TabIndex = 1;
			this.passwordTextBox.Text = "";
			// 
			// FormGetPassword
			// 
			this.AcceptButton = this.okBtn;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(360, 86);
			this.ControlBox = false;
			this.Controls.Add(this.passwordTextBox);
			this.Controls.Add(this.passwordLabel);
			this.Controls.Add(this.okBtn);
			this.Controls.Add(this.cancelBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "FormGetPassword";
			this.Text = "FormGetPassword";
			this.Load += new System.EventHandler(this.FormGetPassword_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void FormGetPassword_Load(object sender, System.EventArgs e)
		{
		
		}
	}
}
