namespace MissionVerify
{
	partial class frmMain
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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(frmMain));
			this.lblMain = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// lblMain
			// 
			this.lblMain.AllowDrop = true;
			this.lblMain.Font = new System.Drawing.Font("Microsoft Sans Serif", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblMain.Location = new System.Drawing.Point(0, 0);
			this.lblMain.Name = "lblMain";
			this.lblMain.Size = new System.Drawing.Size(304, 120);
			this.lblMain.TabIndex = 0;
			this.lblMain.Text = "Drag a mission file into this space to check mission validity.";
			this.lblMain.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.lblMain.DragEnter += new System.Windows.Forms.DragEventHandler(this.lblMain_DragEnter);
			this.lblMain.DragDrop += new System.Windows.Forms.DragEventHandler(this.lblMain_DragDrop);
			// 
			// frmMain
			// 
			this.AllowDrop = true;
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(304, 108);
			this.Controls.Add(this.lblMain);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "frmMain";
			this.Text = "Mission Verification";
			this.ResumeLayout(false);

		}
		#endregion
		
		private System.Windows.Forms.Label lblMain;
	}
}