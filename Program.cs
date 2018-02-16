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


using System;
using System.Windows.Forms;

namespace Idmr.MissionVerify
{
	static class Program
	{
		/// <summary>The main entry point for the application.</summary>
		[STAThread]
		static void Main(string[] Args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			if (Args.Length != 1) Application.Run(new MainForm());
			else Application.Run(new ResultsForm(Args[0]));
		}
	}
}
