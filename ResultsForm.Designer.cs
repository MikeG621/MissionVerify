namespace Idmr.MissionVerify
{
	partial class ResultsForm
	{
		/// <summary>Required designer variable.</summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>Clean up any resources being used.</summary>
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
		private void InitializeComponent()
		{
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ResultsForm));
			this.lblResult = new System.Windows.Forms.Label();
			this.cmdOK = new System.Windows.Forms.Button();
			this.txtResults = new System.Windows.Forms.TextBox();
			this.lblName = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblResult
			// 
			this.lblResult.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblResult.Location = new System.Drawing.Point(8, 8);
			this.lblResult.Name = "lblResult";
			this.lblResult.Size = new System.Drawing.Size(304, 40);
			this.lblResult.TabIndex = 0;
			this.lblResult.Text = "File is NOT valid";
			this.lblResult.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// cmdOK
			// 
			this.cmdOK.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cmdOK.Location = new System.Drawing.Point(128, 200);
			this.cmdOK.Name = "cmdOK";
			this.cmdOK.Size = new System.Drawing.Size(64, 24);
			this.cmdOK.TabIndex = 1;
			this.cmdOK.Text = "OK";
			this.cmdOK.Click += new System.EventHandler(this.cmdOK_Click);
			// 
			// txtResults
			// 
			this.txtResults.Location = new System.Drawing.Point(8, 72);
			this.txtResults.Multiline = true;
			this.txtResults.Name = "txtResults";
			this.txtResults.ReadOnly = true;
			this.txtResults.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.txtResults.Size = new System.Drawing.Size(304, 120);
			this.txtResults.TabIndex = 2;
			this.txtResults.Text = "Results go here";
			// 
			// lblName
			// 
			this.lblName.Location = new System.Drawing.Point(8, 48);
			this.lblName.Name = "lblName";
			this.lblName.Size = new System.Drawing.Size(296, 24);
			this.lblName.TabIndex = 3;
			this.lblName.Text = "Filename";
			// 
			// frmResults
			// 
			this.AcceptButton = this.cmdOK;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.CancelButton = this.cmdOK;
			this.ClientSize = new System.Drawing.Size(320, 226);
			this.Controls.Add(this.lblName);
			this.Controls.Add(this.txtResults);
			this.Controls.Add(this.cmdOK);
			this.Controls.Add(this.lblResult);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "frmResults";
			this.Text = "Results";
			this.ResumeLayout(false);

		}
		#endregion
		
		private System.Windows.Forms.Label lblResult;
		private System.Windows.Forms.Button cmdOK;
		private System.Windows.Forms.TextBox txtResults;
		private System.Windows.Forms.Label lblName;
	}
}