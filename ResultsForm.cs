/*
 * MissionVerify.exe, X-wing series mission validation utility, TIE95-XWA
 * Copyright (C) 2006-2021 Michael Gaisser (mjgaisser@gmail.com)
 * Licensed under the MPL v2.0 or later
 * 
 * Version: 2.1
 */

/* CHANGELOG
 * v2.1, 210315
 * [ADD] Trigger "OR true" and "AND false" detection. NOTE: checking 1ao2 and 3ao4, but not 12ao34 [YOGEME/#48]
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
					txtResults.Lines = tie(fsFile);
					if (txtResults.Text.IndexOf("*") != -1) lblResult.Text = "File may not be valid";
					if (txtResults.Text.IndexOf("**") != -1) lblResult.Text = "File is NOT valid";
					break;
				case MissionFile.Platform.XvT:
					lblName.Text = "XvT " + file;
					txtResults.Lines = xvt(fsFile, false);
					if (txtResults.Text.IndexOf("*") != -1) lblResult.Text = "File may not be valid";
					if (txtResults.Text.IndexOf("**") != -1) lblResult.Text = "File is NOT valid";
					break;
				case MissionFile.Platform.BoP:
					lblName.Text = "BoP " + file;
					txtResults.Lines = xvt(fsFile, true);
					if (txtResults.Text.IndexOf("*") != -1) lblResult.Text = "File may not be valid";
					if (txtResults.Text.IndexOf("**") != -1) lblResult.Text = "File is NOT valid";
					break;
				case MissionFile.Platform.XWA:
					lblName.Text = "XWA " + file;
					txtResults.Lines = xwa(fsFile);
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

		private string[] tie(FileStream fsFile)
		{
			Platform.Tie.Mission mission = new Platform.Tie.Mission(fsFile);
			fsFile.Close();
			int craftCount = 0;	//consts and temp
			List<string> results = new List<string>();
			string playerFG = "";
			bool duplicate = false;
			if (mission.FlightGroups.Count > Platform.Tie.Mission.FlightGroupLimit)
				results.Add("**Warning, mission has more than 48 Flight Groups, mission is NOT flyable.");
			if (mission.Messages.Count == 0) 
				results.Add("Mission has no in-flight messages.");
			if (mission.Messages.Count > Platform.Tie.Mission.MessageLimit)
				results.Add("**Mission has more than 16 in-flight messages, errors will occur.");
			if (mission.EndOfMissionMessages[0] == "")
				results.Add("No Mission Complete message.");
			foreach (Platform.Tie.FlightGroup fg in mission.FlightGroups)
			{
				craftCount += fg.NumberOfCraft;
				if (fg.PlayerCraft != 0)
				{
					results.Add((duplicate ? "*" : "") + "Flight Group " + fg.Name + " is player craft.");
					if (!duplicate) playerFG = fg.Name;
					duplicate = true;
				}
			}
			if (playerFG == "") results.Add("**No player craft.");
			if (craftCount > Platform.Tie.Mission.CraftLimit)
				results.Add("*More than 28 craft, ensure not all exist concurrently.");
			duplicate = false;
			foreach (Platform.Tie.FlightGroup fg in mission.FlightGroups)
			{
				if (fg.AI == 0 && (fg.CraftType < 0x1A || fg.CraftType > 0x1D) && (fg.CraftType < 0x37 || fg.CraftType > 0x3B) && fg.CraftType < 0x50)
					results.Add("Flight Group " + fg.Name + " has basic AI" + (fg.Name == playerFG ? " (player" : ""));
				if (isBadTrigger(fg.ArrDepTriggers[0], fg.ArrDepTriggers[1], fg.AT1AndOrAT2))
					results.Add("*Flight Group " + fg.Name + " has ArrDep trigger errors.");
				if (fg.Orders[0].Command == 0 && (fg.CraftType < 0x1A || fg.CraftType > 0x1D) && (fg.CraftType < 0x37 || fg.CraftType > 0x3B) && fg.CraftType < 0x50)
				{
					results.Add((duplicate ? "*" : "") + "Flight Group " + fg.Name + " has no orders" + (fg.Name == playerFG ? " (player" : ""));
					duplicate = true;
				}
			}
			for (int i = 0; i < mission.Messages.Count; i++)
			{
				if (isBadTrigger(mission.Messages[i].Triggers[0], mission.Messages[i].Triggers[1], mission.Messages[i].Trig1AndOrTrig2))
					results.Add("*Message " + i + " has trigger errors.");
			}

			for (int i = 0; i < 3; i++)
			{
				if (isBadTrigger(mission.GlobalGoals.Goals[i].Triggers[0], mission.GlobalGoals.Goals[i].Triggers[1], mission.GlobalGoals.Goals[i].T1AndOrT2))
					results.Add("**" + (i == 0 ? "Primary" : (i == 1 ? "Secondary" : "Bonus")) + " Global goals are not completable.");
			}

			if (mission.Briefing.Length == 9998 || mission.Briefing.Length == 0x21C)
				results.Add("*Briefing has default length.");
			if (mission.Briefing.Events[0] == 9999 || mission.Briefing.Events[8] == 9999)
				results.Add("*No briefing.");
			if (mission.BriefingQuestions.PreMissQuestions[0] == "")
				results.Add("*No pre-mission Officer questions.");
			if (mission.BriefingQuestions.PostMissQuestions[0] == "")
				results.Add("No post-mission Officer questions.");

			string [] strRes = new string [results.Count];
			for (int i = 0; i < strRes.Length; i++)
			{
				strRes[i] = results[i];
			}
			return strRes;
		}

		private string[] xvt(FileStream fsFile, bool bop)
		{
			Platform.Xvt.Mission mission = new Platform.Xvt.Mission(fsFile);
			fsFile.Close();
			int craftCount = 0;	//consts and temp
			List<string> results = new List<string>();
			bool duplicate = false;
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
				craftCount += fg.NumberOfCraft;
				if (fg.PlayerNumber != 0)
				{
					results.Add((duplicate ? "*" : "") + "Flight Group " + fg.Name + " is player craft.");
					if (!duplicate) playerFG = fg.Name;
					duplicate = true;
				}
			}
			if (playerFG == "") results.Add("**No player craft.");
			if (craftCount > Platform.Xvt.Mission.CraftLimit)
				results.Add("*More than 36 craft in mission, ensure all do not exist concurrently.");
			duplicate = false;
			foreach (Platform.Xvt.FlightGroup fg in mission.FlightGroups)
			{
				if (fg.AI == 255 && (fg.CraftType < 0x1A || fg.CraftType > 0x1D) && (fg.CraftType < 0x37 || fg.CraftType > 0x3B) && fg.CraftType != 0x46 && fg.CraftType != 47 && (fg.CraftType < 0x50 || fg.CraftType > 0x59))
				{
					results.Add("Flight Group " + fg.Name + " has basic AI." + (fg.Name == playerFG ? " (player)" : ""));
				}
				if (isBadTrigger(fg.ArrDepTriggers[0], fg.ArrDepTriggers[1], fg.ArrDepAO[0]) ||
					isBadTrigger(fg.ArrDepTriggers[2], fg.ArrDepTriggers[3], fg.ArrDepAO[1]) ||
					isBadTrigger(fg.ArrDepTriggers[4], fg.ArrDepTriggers[5], fg.ArrDepAO[2]))
					results.Add("*Flight Group " + fg.Name + " has ArrDep trigger errors.");
				if (fg.Orders[0].Command == 0 && (fg.CraftType < 0x1A || fg.CraftType > 0x1D) && (fg.CraftType < 0x37 || fg.CraftType > 0x3B) && fg.CraftType != 0x46 && fg.CraftType != 47 && (fg.CraftType < 0x50 || fg.CraftType > 0x59))
				{
					results.Add((duplicate ? "*" : "") + "Flight Group " + fg.Name + " has no orders." + (fg.Name == playerFG ? " (player)" : ""));
					duplicate = true;
				}
				if (isBadTrigger(fg.SkipToOrder4Trigger[0], fg.SkipToOrder4Trigger[1], fg.SkipToO4T1AndOrT2))
					results.Add("*Flight Group " + fg.Name + " has Skip to Order trigger errors.");
			}

			for (int i = 0; i < mission.Messages.Count; i++)
			{
				if (isBadTrigger(mission.Messages[i].Triggers[0], mission.Messages[i].Triggers[1], mission.Messages[i].T1AndOrT2) ||
					isBadTrigger(mission.Messages[i].Triggers[2], mission.Messages[i].Triggers[3], mission.Messages[i].T3AndOrT4))
					results.Add("*Message " + i + " has trigger errors.");
			}

			for (int t = 0; t < 10; t++)
			{
				for (int g = 0; g < 3; g++)
				{
					if (isBadTrigger(mission.Globals[t].Goals[g].Triggers[0].GoalTrigger, mission.Globals[t].Goals[g].Triggers[1].GoalTrigger, mission.Globals[t].Goals[g].T1AndOrT2) ||
						isBadTrigger(mission.Globals[t].Goals[g].Triggers[0].GoalTrigger, mission.Globals[t].Goals[g].Triggers[1].GoalTrigger, mission.Globals[t].Goals[g].T1AndOrT2))
						results.Add("*Team " + (t + 1) + " " + (g == 0 ? "Primary" : (g == 1 ? "Prevent" : "Secondary")) + " Global goals are not completable.");
				}
			}
			if (mission.Briefings[0].Events[0] == 9999 || mission.Briefings[0].Events[8] == 9999)
				results.Add("*No briefing.");
			if (mission.MissionDescription == "") results.Add("*No Mission description.");

			string[] strRes = new string[results.Count];
			for (int i = 0; i < strRes.Length; i++)
			{
				strRes[i] = results[i];
			}
			return strRes;
		}

		private string[] xwa(FileStream fsFile)
		{
			Platform.Xwa.Mission mission = new Platform.Xwa.Mission(fsFile);
			fsFile.Close();
			int craftCount = 0;	//consts and temp
			List<string> results = new List<string>();
			bool duplicate = false;
			string playerFG = "";
			if (mission.FlightGroups.Count > Platform.Xwa.Mission.FlightGroupLimit)
				results.Add("**Warning, over 100 Flight Groups, mission may be unstable.");
			if (mission.Messages.Count == 0)  results.Add("Mission has no in-flight messages.");
			if (mission.Messages.Count > Platform.Xwa.Mission.MessageLimit)
				results.Add("**Warning, over 65 in-flight messages, mission may be unstable.");
			if (mission.MissionSuccessful == "") results.Add("No Mission Complete message.");
			foreach (Platform.Xwa.FlightGroup fg in mission.FlightGroups)
			{
				craftCount += fg.NumberOfCraft;
				if (fg.PlayerNumber != 0)
				{
					results.Add((duplicate ? "*" : "") + "Flight Group " + fg.Name + " is player craft.");
					if (!duplicate) playerFG = fg.Name;
					if (fg.ArrivalCraft1 == 0) 
						results.Add("**Flight Group " + fg.Name + " has no mothership.");
					duplicate = true;
				}
			}
			if (playerFG == "") results.Add("**No player craft.");
			if (craftCount > Platform.Xwa.Mission.CraftLimit)
				results.Add("*More than 96 craft. Ensure not all exist in the same region concurrently.");
			duplicate = false;
			foreach (Platform.Xwa.FlightGroup fg in mission.FlightGroups)
			{
				if (fg.AI == 0 && (fg.CraftType < 0x1A || fg.CraftType > 0x1D) && (fg.CraftType < 0x37 || fg.CraftType > 0x3B) && (fg.CraftType < 0x46 || fg.CraftType > 0x4A) && (fg.CraftType < 0x50 || fg.CraftType > 0x59) && fg.CraftType != 0x86 && (fg.CraftType < 0x98 || fg.CraftType > 0x9B) && fg.CraftType != 0xB7)
				{
					results.Add("Flight Group " + fg.Name + " has basic AI." + (fg.Name == playerFG ? " (player)" : ""));
				}
				if (isBadTrigger(fg.ArrDepTriggers[0], fg.ArrDepTriggers[1], fg.ArrDepAndOr[0]) ||
					isBadTrigger(fg.ArrDepTriggers[2], fg.ArrDepTriggers[3], fg.ArrDepAndOr[1]) ||
					isBadTrigger(fg.ArrDepTriggers[4], fg.ArrDepTriggers[5], fg.ArrDepAndOr[2]))
					results.Add("*Flight Group " + fg.Name + " has ArrDep trigger errors.");
				if (fg.Orders[fg.Waypoints[0].Region, 0].Command == 0 && (fg.CraftType < 0x1A || fg.CraftType > 0x1D) && (fg.CraftType < 0x37 || fg.CraftType > 0x3B) && (fg.CraftType < 0x46 || fg.CraftType > 0x4A) && (fg.CraftType < 0x50 || fg.CraftType > 0x59) && fg.CraftType != 0x86 && (fg.CraftType < 0x98 || fg.CraftType > 0x9B) && fg.CraftType != 0xB7)
				{
					results.Add((duplicate ? "*" : "") + "Flight Group " + fg.Name + " has no orders." + (fg.Name == playerFG ? " (player)" : ""));
					duplicate = true;
				}
				for (int o = 0; o < 16; o++)
				{
					if (isBadTrigger(fg.Orders[o / 4, o % 4].SkipTriggers[0], fg.Orders[o / 4, o % 4].SkipTriggers[1], fg.Orders[o / 4, o % 4].SkipT1AndOrT2))
						results.Add("*Region \"" + mission.Regions[o / 4] + "\", Skip to Order " + ((o % 4) + 1) + " has trigger errors.");
				}
			}

			for (int i = 0; i < mission.Messages.Count; i++)
			{
				if (isBadTrigger(mission.Messages[i].Triggers[0], mission.Messages[i].Triggers[1], mission.Messages[i].T1AndOrT2) ||
					isBadTrigger(mission.Messages[i].Triggers[2], mission.Messages[i].Triggers[3], mission.Messages[i].T3AndOrT4) ||
					isBadTrigger(mission.Messages[i].Triggers[4], mission.Messages[i].Triggers[5], mission.Messages[i].CancelT1AndOrT2))
					results.Add("*Message " + i + " has trigger errors.");
			}

			for (int t = 0; t < 10; t++)
			{
				for (int g = 0; g < 3; g++)
				{
					if (isBadTrigger(mission.Globals[t].Goals[g].Triggers[0], mission.Globals[t].Goals[g].Triggers[1], mission.Globals[t].Goals[g].T1AndOrT2) ||
						isBadTrigger(mission.Globals[t].Goals[g].Triggers[0], mission.Globals[t].Goals[g].Triggers[1], mission.Globals[t].Goals[g].T1AndOrT2))
						results.Add("*Team " + (t + 1) + " " + (g == 0 ? "Primary" : (g == 1 ? "Prevent" : "Secondary")) + " Global goals are not completable.");
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

		bool isBadTrigger(BaseTrigger trig1, BaseTrigger trig2, bool andOr)
		{
			// reminder: AND = false, OR = true
			if (((trig1.Condition == 0 || trig2.Condition == 0) && andOr) ||
				((trig1.Condition == 10 || trig2.Condition == 10) && !andOr)) return true;
			return false;
		}
	}
}
