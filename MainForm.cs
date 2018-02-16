/*
 * MissionVerify.exe, X-wing series mission validation utility, TIE95-XWA
 * Copyright (C) 2006-2018 Michael Gaisser (mjgaisser@gmail.com)
 * Licensed under the MPL v2.0 or later
 * 
 * Version: 2.0
 */

/* CHANGELOG
 * v2.0, 180215
 * [UPD] Converted to current standards
 */

using System.Windows.Forms;

namespace Idmr.MissionVerify
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void lblMain_DragDrop(object sender, DragEventArgs e)
		{
			string[] args = (string[])e.Data.GetData(DataFormats.FileDrop);	//get the info from the Drop
			if (args.Length > 1)	//if more than one file..
			{
				MessageBox.Show("Please check only one file at a time.", "Error");
				return;
			}
			ResultsForm frmRes = new ResultsForm(args[0]);
		}

		private void lblMain_DragEnter(object sender, DragEventArgs e)
		{
			// make sure they're actually dropping files, and allow
			if(e.Data.GetDataPresent(DataFormats.FileDrop, false) == true) e.Effect = DragDropEffects.All;
		}
	}
}
