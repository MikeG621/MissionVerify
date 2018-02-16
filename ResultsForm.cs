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
 * [UPD] Implemented Idmr.Platform
 * [UPD YOGEME/#15] Removed Containers, probes, satellites, backdrops, etc from AI and Orders checks
 * [UPD #3] AI messages changed "no AI" to "basic AI", removed failure indicator
 * [UPD #2] results string[] replaced with List<string>
 * [UPD] Multiple player FGs now flagged as * instead of ** due to advanced creation techniques
 */
using Idmr.Platform;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace Idmr.MissionVerify
{
	public partial class ResultsForm : Form
	{
		public ResultsForm(string file)
		{
			InitializeComponent();
			FileStream fsFile;
			try 
			{
				fsFile = File.Open(file, FileMode.Open, FileAccess.Read);
			}
			catch
			{
				MessageBox.Show("Cannot open file. Make sure file is currently not being used.", "Error");
				return;
			}
			lblResult.Text = "File is VALID";
			switch(MissionFile.GetPlatform(fsFile))
			{
				case MissionFile.Platform.TIE:
					lblName.Text = "TIE " + file;
					txtResults.Lines = TIE(fsFile);
					if (txtResults.Text.IndexOf("*") != -1) lblResult.Text = "File may not be valid";
					if (txtResults.Text.IndexOf("**") != -1) lblResult.Text = "File is NOT valid";
					break;
				case MissionFile.Platform.XvT:
					lblName.Text = "XvT " + file;
					txtResults.Lines = XvT(fsFile, false);
					if (txtResults.Text.IndexOf("*") != -1) lblResult.Text = "File may not be valid";
					if (txtResults.Text.IndexOf("**") != -1) lblResult.Text = "File is NOT valid";
					break;
				case MissionFile.Platform.BoP:
					lblName.Text = "BoP " + file;
					txtResults.Lines = XvT(fsFile, true);
					if (txtResults.Text.IndexOf("*") != -1) lblResult.Text = "File may not be valid";
					if (txtResults.Text.IndexOf("**") != -1) lblResult.Text = "File is NOT valid";
					break;
				case MissionFile.Platform.XWA:
					lblName.Text = "XWA " + file;
					txtResults.Lines = XWA(fsFile);
					if (txtResults.Text.IndexOf("*") != -1) lblResult.Text = "File may not be valid";
					if (txtResults.Text.IndexOf("**") != -1) lblResult.Text = "File is NOT valid";
					break;
				default:
					MessageBox.Show("File is not a valid mission file.", "Error");
					return;
			}
			fsFile.Close();
			Show();
		}

		private void cmdOK_Click(object sender, System.EventArgs e)
		{
			Dispose();
		}

		private string[] TIE(FileStream fsFile)
		{
			Platform.Tie.Mission mission = new Platform.Tie.Mission(fsFile);
			fsFile.Close();
			int iCraft = 0;	//consts and temp
			List<string> results = new List<string>();
			string playerFG = "";
			bool bFail = false;
			if (mission.FlightGroups.Count > Platform.Tie.Mission.FlightGroupLimit)
				results.Add("**Warning, mission has more than 48 Flight Groups, mission is NOT flyable.");
			if (mission.Messages.Count == 0) 
				results.Add("Mission has no in-flight messages.");
			if (mission.Messages.Count > Platform.Tie.Mission.MessageLimit)
				results.Add("**Mission has more than 16 in-flight messages, errors will occur.");
			if (mission.EndOfMissionMessages[0] == "")
				results.Add("No Mission Complete message");
			foreach (Platform.Tie.FlightGroup fg in mission.FlightGroups)
			{
				iCraft += fg.NumberOfCraft;
				if (fg.PlayerCraft != 0)
				{
					results.Add((bFail ? "*" : "") + "Flight Group " + fg.Name + " is player craft");
					if (!bFail) playerFG = fg.Name;
					bFail = true;
				}
			}
			if (playerFG == "") results.Add("**No player craft");
			if (iCraft > Platform.Tie.Mission.CraftLimit)
				results.Add("*More than 28 craft, ensure not all exist concurrently");
			bFail = false;
			foreach (Platform.Tie.FlightGroup fg in mission.FlightGroups)
			{
				if (fg.AI == 0 && (fg.CraftType < 0x1A || fg.CraftType > 0x1D) && (fg.CraftType < 0x37 || fg.CraftType > 0x3B) && fg.CraftType < 0x50)
					results.Add("Flight Group " + fg.Name + " has basic AI" + (fg.Name == playerFG ? " (player" : ""));
				if (fg.Orders[0].Command == 0 && (fg.CraftType < 0x1A || fg.CraftType > 0x1D) && (fg.CraftType < 0x37 || fg.CraftType > 0x3B) && fg.CraftType < 0x50)
				{
					results.Add((bFail ? "*" : "") + "Flight Group " + fg.Name + " has no orders" + (fg.Name == playerFG ? " (player" : ""));
					bFail = true;
				}
			}
			if (mission.GlobalGoals.Goals[0].Triggers[0].Condition != 10 &&
				mission.GlobalGoals.Goals[0].Triggers[1].Condition == 10 &&
				mission.GlobalGoals.Goals[0].T1AndOrT2 == false)
			{
				results.Add("**Primary Global goals are not completable");
			}
			if (mission.GlobalGoals.Goals[1].Triggers[0].Condition != 10 &&
				mission.GlobalGoals.Goals[1].Triggers[1].Condition == 10 &&
				mission.GlobalGoals.Goals[1].T1AndOrT2 == false)
			{
				results.Add("**Secondary Global goals are not completable");
			}
			if (mission.GlobalGoals.Goals[2].Triggers[0].Condition != 10 &&
				mission.GlobalGoals.Goals[2].Triggers[1].Condition == 10 &&
				mission.GlobalGoals.Goals[2].T1AndOrT2 == false)
			{
				results.Add("**Bonus Global goals are not completable");
			}
			if (mission.Briefing.Length == 9998 || mission.Briefing.Length == 0x21C)
				results.Add("*Briefing has default length");
			if (mission.Briefing.Events[0] == 9999 || mission.Briefing.Events[8] == 9999)
				results.Add("*No briefing");
			if (mission.BriefingQuestions.PreMissQuestions[0] == "")
				results.Add("*No pre-mission Officer questions");
			if (mission.BriefingQuestions.PostMissQuestions[0] == "")
				results.Add("No post-mission Officer questions");

			string [] strRes = new string [results.Count];
			for (int i = 0; i < strRes.Length; i++)
			{
				strRes[i] = results[i];
			}
			return strRes;
		}

		private string[] XvT(FileStream fsFile, bool BoP)
		{
			Platform.Xvt.Mission mission = new Platform.Xvt.Mission(fsFile);
			fsFile.Close();
			int iCraft = 0;	//consts and temp
			List<string> results = new List<string>();
			bool bFail = false;
			string playerFG = "";
			if (mission.FlightGroups.Count > Platform.Xvt.Mission.FlightGroupLimit)
				results.Add("**Warning, more than 46 Flight Groups, mission may not be stable.");
			if (mission.Messages.Count == 0) 
				results.Add("Mission has no in-flight messages.");
			if (mission.Messages.Count > Platform.Xvt.Mission.MessageLimit)
				results.Add("**Mission has more than 64 in-flight messages, errors will occur.");
			if (mission.MissionSuccessful == "")
				results.Add("No Mission Complete message.");
			foreach (Platform.Xvt.FlightGroup fg in mission.FlightGroups)
			{
				iCraft += fg.NumberOfCraft;
				if (fg.PlayerCraft != 0)
				{
					results.Add((bFail ? "*" : "") + "Flight Group " + fg.Name + " is player craft.");
					if (!bFail) playerFG = fg.Name;
					bFail = true;
				}
			}
			if (playerFG == "") results.Add("**No player craft.");
			if (iCraft > Platform.Xvt.Mission.CraftLimit)
				results.Add("*More than 36 craft in mission, ensure all do not exist concurrently.");
			bFail = false;
			foreach (Platform.Xvt.FlightGroup fg in mission.FlightGroups)
			{
				if (fg.AI == 255 && (fg.CraftType < 0x1A || fg.CraftType > 0x1D) && (fg.CraftType < 0x37 || fg.CraftType > 0x3B) && fg.CraftType != 0x46 && fg.CraftType != 47 && (fg.CraftType < 0x50 || fg.CraftType > 0x59))
				{
					results.Add("Flight Group " + fg.Name + " has basic AI." + (fg.Name == playerFG ? " (player)" : ""));
				}
				if (fg.Orders[0].Command == 0 && (fg.CraftType < 0x1A || fg.CraftType > 0x1D) && (fg.CraftType < 0x37 || fg.CraftType > 0x3B) && fg.CraftType != 0x46 && fg.CraftType != 47 && (fg.CraftType < 0x50 || fg.CraftType > 0x59))
				{
					results.Add((bFail ? "*" : "") + "Flight Group " + fg.Name + " has no orders." + (fg.Name == playerFG ? " (player)" : ""));
					bFail = true;
				}
			}
			if (mission.Briefings[0].Events[0] == 9999 || mission.Briefings[0].Events[8] == 9999)
				results.Add("*No briefing");
			if (mission.MissionDescription == "") results.Add("*No Mission description");

			string[] strRes = new string[results.Count];
			for (int i = 0; i < strRes.Length; i++)
			{
				strRes[i] = results[i];
			}
			return strRes;
		}

		private string[] XWA(FileStream fsFile)
		{
			Platform.Xwa.Mission mission = new Platform.Xwa.Mission(fsFile);
			fsFile.Close();
			int iCraft = 0;	//consts and temp
			List<string> results = new List<string>();
			bool bFail = false;
			string playerFG = "";
			if (mission.FlightGroups.Count > Platform.Xwa.Mission.FlightGroupLimit)
				results.Add("**Warning, over 100 Flight Groups, mission may be unstable.");
			if (mission.Messages.Count == 0)  results.Add("Mission has no in-flight messages.");
			if (mission.Messages.Count > Platform.Xwa.Mission.MessageLimit)
				results.Add("**Warning, over 65 in-flight messages, mission may be unstable.");
			if (mission.MissionSuccessful == "") results.Add("No Mission Complete message.");
			foreach (Platform.Xwa.FlightGroup fg in mission.FlightGroups)
			{
				iCraft += fg.NumberOfCraft;
				if (fg.PlayerNumber != 0)
				{
					results.Add((bFail ? "*" : "") + "Flight Group " + fg.Name + " is player craft.");
					if (!bFail) playerFG = fg.Name;
					if (fg.ArrivalCraft1 == 0) 
						results.Add("**Flight Group " + fg.Name + " has no mothership.");
					bFail = true;
				}
			}
			if (playerFG == "") results.Add("**No player craft.");
			if (iCraft > Platform.Xwa.Mission.CraftLimit)
				results.Add("*More than 96 craft. Ensure not all exist in the same region concurrently.");
			bFail = false;
			foreach (Platform.Xwa.FlightGroup fg in mission.FlightGroups)
			{
				if (fg.AI == 0 && (fg.CraftType < 0x1A || fg.CraftType > 0x1D) && (fg.CraftType < 0x37 || fg.CraftType > 0x3B) && (fg.CraftType < 0x46 || fg.CraftType > 0x4A) && (fg.CraftType < 0x50 || fg.CraftType > 0x59) && fg.CraftType != 0x86 && (fg.CraftType < 0x98 || fg.CraftType > 0x9B) && fg.CraftType != 0xB7)
				{
					results.Add("Flight Group " + fg.Name + " has basic AI." + (fg.Name == playerFG ? " (player)" : ""));
				}
				if (fg.Orders[fg.Waypoints[0].Region, 0].Command == 0 && (fg.CraftType < 0x1A || fg.CraftType > 0x1D) && (fg.CraftType < 0x37 || fg.CraftType > 0x3B) && (fg.CraftType < 0x46 || fg.CraftType > 0x4A) && (fg.CraftType < 0x50 || fg.CraftType > 0x59) && fg.CraftType != 0x86 && (fg.CraftType < 0x98 || fg.CraftType > 0x9B) && fg.CraftType != 0xB7)
				{
					results.Add((bFail ? "*" : "") + "Flight Group " + fg.Name + " has no orders." + (fg.Name == playerFG ? " (player)" : ""));
					bFail = true;
				}
			}

			if (mission.Briefings[0].Events[0] == 9999 || mission.Briefings[0].Events[8] == 9999)
				results.Add("*No briefing");
			if (mission.MissionDescription == "") results.Add("*No Mission description.");

			string[] strRes = new string[results.Count];
			for (int i = 0; i < strRes.Length; i++)
			{
				strRes[i] = results[i];
			}
			return strRes;
		}
	}
}
