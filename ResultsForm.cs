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
 * [UPD] Implemented Idmr.Platform
 * [UPD] Removed Containers, probes, satellites, backdrops, etc from AI and Orders checks
 * [UPD #3] AI messages changed "no AI" to "basic AI", removed failure indicator
 * [UPD #2] results string[] replaced with List<string>
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
					results.Add((bFail ? "**" : "") + "Flight Group " + fg.Name + " is player craft");
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
			}
			bFail = false;
			foreach (Platform.Tie.FlightGroup fg in mission.FlightGroups)
			{
				if (fg.Orders[0].Command == 0 && (fg.CraftType < 0x1A || fg.CraftType > 0x1D) && (fg.CraftType < 0x37 || fg.CraftType > 0x3B) && fg.CraftType < 0x50)
				{
					results.Add((bFail ? "*" : "") + "Flight Group " + fg.Name + " has no orders" + (fg.Name == playerFG ? " (player" : ""));
					bFail = true;
				}
			}
			bFail = false;
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
			BinaryReader br = new BinaryReader(fsFile);
			int FG, MESSAGE, temp, iCraft = 0;	//consts and temp
			int iFG;        //position counters
			List<string> results = new List<string>();
			bool bFail = false;
			int playerFG = -1;
			fsFile.Position = 2;
			FG = br.ReadInt16();
			MESSAGE = br.ReadInt16();
			if (FG > 46)
				results.Add("**Warning, more than 46 Flight Groups, mission may not be stable.");
			if (MESSAGE == 0) 
				results.Add("Mission has no in-flight messages.");
			if (MESSAGE > 64)
				results.Add("**Mission has more than 64 in-flight messages, errors will occur.");
			fsFile.Position = 1480 + FG * 1378 + MESSAGE * 116;
			temp = fsFile.ReadByte();
			if (temp == 0) results.Add("No Mission Complete message.");
			for (iFG = 1; iFG <= FG; iFG++)
			{
				fsFile.Position = iFG * 1378 - 1131;
				temp = fsFile.ReadByte();
				iCraft += temp + 1;
				fsFile.Position += 16;
				temp = fsFile.ReadByte();
				if (temp != 0)
				{
					results.Add((bFail ? "**" : "") + "Flight Group " + iFG + " is player craft.");
					if (!bFail) playerFG = iFG;
					bFail = true;
				}
			}
			if (playerFG == -1)
				results.Add("**No player craft.");
			if (iCraft > 36)
				results.Add("*More than 36 craft in mission, ensure all do not exist concurrently.");
			bFail = false;
			for (iFG = 1; iFG <= FG; iFG++)
			{
				fsFile.Position = iFG * 1378 - 1125;
				temp = fsFile.ReadByte();
				if (temp == 255) results.Add("Flight Group " + iFG + " has basic AI." + (iFG == playerFG ? " (player)" : ""));
			}
			for (iFG = 1; iFG <= FG; iFG++)
			{
				fsFile.Position = iFG * 1378 - 1052;
				temp = fsFile.ReadByte();
				if (temp == 0)
				{
					results.Add((bFail ? "*" : "") + "Flight Group " + iFG + " has no orders." + (iFG == playerFG ? " (player)" : ""));
					bFail = true;
				}
			}
			bFail = false;
			fsFile.Position = 6324 + FG * 1378 + MESSAGE * 116;
			temp = fsFile.ReadByte();
			if (temp == 15) results.Add("*No briefing");
			//check Descrip, different positions depending on platform
			if (!BoP) fsFile.Position = fsFile.Length - 1024;
			else fsFile.Position = fsFile.Length - 4096;
			temp = fsFile.ReadByte();
			if (temp == 0) results.Add("*No Mission description");

			string[] strRes = new string[results.Count];
			for (int i = 0; i < strRes.Length; i++)
			{
				strRes[i] = results[i];
			}
			return strRes;
		}

		private string[] XWA(FileStream fsFile)
		{
			BinaryReader br = new BinaryReader(fsFile);
			int FG, MESSAGE, temp, iCraft = 0;	//consts and temp
			int iFG;        //position counters
			List<string> results = new List<string>();
			bool bFail = false;
			int playerFG = -1;
			fsFile.Position = 2;
			FG = br.ReadInt16();
			MESSAGE = br.ReadInt16();
			if (FG > 100) results.Add("**Warning, over 100 Flight Groups, mission may be unstable.");
			if (MESSAGE == 0)  results.Add("Mission has no in-flight messages.");
			if (MESSAGE > 65) results.Add("**Warning, over 65 in-flight messages, mission may be unstable.");
			fsFile.Position = 12916 + FG * 3646 + MESSAGE * 162;
			temp = fsFile.ReadByte();
			if (temp == 0) results.Add("No Mission Complete message.");
			for (iFG = 1; iFG <= FG; iFG++)
			{
				fsFile.Position = 5662 + iFG * 3646;
				temp = fsFile.ReadByte();
				iCraft += temp + 1;
				fsFile.Position += 16;
				temp = fsFile.ReadByte();
				if (temp != 0)
				{
					results.Add((bFail ? "**" : "") + "Flight Group " + iFG + " is player craft.");
					if (!bFail) playerFG = iFG;
					fsFile.Position += 69;
					if (fsFile.ReadByte() == 0) 
						results.Add("**Flight Group " + iFG + " has no mothership.");
					bFail = true;
				}
			}
			if (playerFG == -1)
				results.Add("**No player craft.");
			if (iCraft > 96)
				results.Add("*More than 96 craft. Ensure not all exist in the same region concurrently.");
			bFail = false;
			for (iFG = 1; iFG <= FG; iFG++)
			{
				fsFile.Position = 5668 + iFG * 3646;
				temp = fsFile.ReadByte();
				if (temp == 0) results.Add("Flight Group " + iFG + " has basic AI." + (iFG == playerFG ? " (player)" : ""));
			}
			for (iFG = 1; iFG <= FG; iFG++)
			{
				fsFile.Position = 5756 + iFG * 3646;
				temp = fsFile.ReadByte();
				if (temp == 0)
				{
					results.Add((bFail ? "*" : "") + "Flight Group " + iFG + " has no orders." + (iFG == playerFG ? " (player)" : ""));
					bFail = true;
				}
			}
			bFail = false;
			fsFile.Position = 17760 + FG * 3646 + MESSAGE * 162;
			temp = fsFile.ReadByte();
			if (temp == 15) 	results.Add("*No briefing.");
			//check Descrip
			fsFile.Position = fsFile.Length - 12288;
			temp = fsFile.ReadByte();
			if (temp == 0) 	results.Add("*No Mission description.");

			string[] strRes = new string[results.Count];
			for (int i = 0; i < strRes.Length; i++)
			{
				strRes[i] = results[i];
			}
			return strRes;
		}
	}
}
