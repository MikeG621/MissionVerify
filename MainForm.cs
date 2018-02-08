/*
 * MissionVerify.exe, X-wing series mission validation utility, TIE95-XWA
 * Copyright (C) 2006-2018 Michael Gaisser (mjgaisser@gmail.com)
 * Licensed under the MPL v2.0 or later
 * 
 * Version: 2.0 (pending)
 */

/* CHANGELOG
* v2.0,
* [UPD] Converted to current standards
*/

using System.Windows.Forms;

namespace Idmr.MissionVerify
{
	/// <summary>
	/// This program is to be used as a validation check for custom missions
	/// for the X-wing series. A mission that fails this check will not be 
	/// eligible for use in the Combat Chamber
	/// 
	/// Current list of errors:
	/// 
	/// **More than MAX FGs
	/// No messages
	/// *More than MAX MESS
	/// (**) player craft
	/// ** No player craft
	/// *More than MAX Craft
	/// (*) No AI
	/// (*) No orders
	/// **player craft has no mothership (XWA)
	/// **Incompletable Global Goals
	/// *No briefing
	/// *No pre-mission officer questions/Mission description
	/// No post-mission questions
	/// </summary>
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
