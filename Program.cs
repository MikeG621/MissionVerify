using System;
using System.Windows.Forms;

namespace MissionVerify
{
	static class Program
	{
		/// <summary>The main entry point for the application.</summary>
		[STAThread]
		static void Main(string[] Args)
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			if (Args.Length != 1) Application.Run(new frmMain());
			else Application.Run(new frmResults(Args[0]));
		}
	}
}
