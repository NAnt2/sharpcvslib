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
	/// Summary description for FormPickSolution.
	/// </summary>
	public class FormPickSolution : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button okBtn;
		private System.Windows.Forms.Button cancelBtn;
		private System.Windows.Forms.ListBox solutionList;
		private bool allowClose_ = true;
		private string selectedSolution = string.Empty;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		IController cont_;

		public FormPickSolution(IController cont, FileInfo[] files )
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			cont_ = cont;

			foreach( FileInfo file in files )
			{
				solutionList.Items.Add( file.Name );
			}

			ResourceManager rm = cont_.Model.ResourceManager;
			this.Text = rm.GetString("FORM_PICK_SOLUTION_TITLE");
			okBtn.Text = rm.GetString("FORM_OK_BUTTON");
			cancelBtn.Text = rm.GetString("FORM_CANCEL_BUTTON");
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
			this.solutionList = new System.Windows.Forms.ListBox();
			this.SuspendLayout();
			// 
			// okBtn
			// 
			this.okBtn.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okBtn.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.okBtn.Location = new System.Drawing.Point(240, 136);
			this.okBtn.Name = "okBtn";
			this.okBtn.TabIndex = 2;
			this.okBtn.Text = "&OK";
			this.okBtn.Click += new System.EventHandler(this.okBtn_Click);
			// 
			// cancelBtn
			// 
			this.cancelBtn.CausesValidation = false;
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.ImeMode = System.Windows.Forms.ImeMode.NoControl;
			this.cancelBtn.Location = new System.Drawing.Point(328, 136);
			this.cancelBtn.Name = "cancelBtn";
			this.cancelBtn.TabIndex = 3;
			this.cancelBtn.Text = "Cancel";
			this.cancelBtn.Click += new System.EventHandler(this.cancelBtn_Click);
			// 
			// solutionList
			// 
			this.solutionList.Location = new System.Drawing.Point(16, 8);
			this.solutionList.Name = "solutionList";
			this.solutionList.Size = new System.Drawing.Size(384, 121);
			this.solutionList.TabIndex = 1;
			// 
			// FormPickSolution
			// 
			this.AcceptButton = this.okBtn;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cancelBtn;
			this.ClientSize = new System.Drawing.Size(418, 168);
			this.ControlBox = false;
			this.Controls.Add(this.solutionList);
			this.Controls.Add(this.okBtn);
			this.Controls.Add(this.cancelBtn);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "FormPickSolution";
			this.Text = "FormPickSolution";
			this.Closing += new System.ComponentModel.CancelEventHandler(this.FormPickSolution_Closing);
			this.Load += new System.EventHandler(this.FormPickSolution_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void FormPickSolution_Load(object sender, System.EventArgs e)
		{
		
		}

		private void FormPickSolution_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = !allowClose_;
		}

		private void cancelBtn_Click(object sender, System.EventArgs e)
		{
			allowClose_ = true;
		}

		private void okBtn_Click(object sender, System.EventArgs e)
		{
			allowClose_ = (solutionList.SelectedIndex != -1);
		}

		public string SolutionFile
		{
			get
			{
				return (string)solutionList.SelectedItem;
			}
		}

	}
}
