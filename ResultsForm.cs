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
 */
using Idmr.Platform;
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
			int temp, iCraft = 0;	//consts and temp
			int i = 0;		//string counter
			string[] strResults = new string[50];
			string playerFG = "";
			bool bFail = false;
			if (mission.FlightGroups.Count > Platform.Tie.Mission.FlightGroupLimit)
				strResults[i++] = "**Warning, mission has more than 48 Flight Groups, mission is NOT flyable.";
			if (mission.Messages.Count == 0) 
				strResults[i++] = "Mission has no in-flight messages.";
			if (mission.Messages.Count > Platform.Tie.Mission.MessageLimit)
				strResults[i++] = "**Mission has more than 16 in-flight messages, errors will occur.";
			if (mission.EndOfMissionMessages[0] == "")
				strResults[i++] = "No Mission Complete message";
			foreach (Platform.Tie.FlightGroup fg in mission.FlightGroups)
			{
				iCraft += fg.NumberOfCraft;
				if (fg.PlayerCraft != 0)
				{
					strResults[i++] = (bFail ? "**" : "") + "Flight Group " + fg.Name + " is player craft";
					if (!bFail) playerFG = fg.Name;
					bFail = true;
				}
			}
			if (playerFG == "")
				strResults[i++] = "**No player craft";
			if (iCraft > Platform.Tie.Mission.CraftLimit)
				strResults[i++] = "*More than 28 craft, ensure not all exist concurrently";
			bFail = false;
			foreach (Platform.Tie.FlightGroup fg in mission.FlightGroups)
			{
				if (fg.AI == 0 && (fg.CraftType < 0x1A || fg.CraftType > 0x1D) && (fg.CraftType < 0x37 || fg.CraftType > 0x3B) && fg.CraftType < 0x50)
				{
					strResults[i++] = (bFail ? "*" : "") + "Flight Group " + fg.Name + " has no AI" + (fg.Name == playerFG ? " (player" : "");
					bFail = true;
				}
			}
			bFail = false;
			foreach (Platform.Tie.FlightGroup fg in mission.FlightGroups)
			{
				if (fg.Orders[0].Command == 0 && (fg.CraftType < 0x1A || fg.CraftType > 0x1D) && (fg.CraftType < 0x37 || fg.CraftType > 0x3B) && fg.CraftType < 0x50)
				{
					strResults[i] = (bFail ? "*" : "") + "Flight Group " + fg.Name + " has no orders" + (fg.Name == playerFG ? " (player" : "");
					bFail = true;
				}
			}
			bFail = false;
			if (mission.GlobalGoals.Goals[0].Triggers[0].Condition != 10 &&
				mission.GlobalGoals.Goals[0].Triggers[1].Condition == 10 &&
				mission.GlobalGoals.Goals[0].T1AndOrT2 == false)
			{
				strResults[i++] = "**Primary Global goals are not completable";
			}
			if (mission.GlobalGoals.Goals[1].Triggers[0].Condition != 10 &&
				mission.GlobalGoals.Goals[1].Triggers[1].Condition == 10 &&
				mission.GlobalGoals.Goals[1].T1AndOrT2 == false)
			{
				strResults[i++] = "**Secondary Global goals are not completable";
			}
			if (mission.GlobalGoals.Goals[2].Triggers[0].Condition != 10 &&
				mission.GlobalGoals.Goals[2].Triggers[1].Condition == 10 &&
				mission.GlobalGoals.Goals[2].T1AndOrT2 == false)
			{
				strResults[i++] = "**Bonus Global goals are not completable";
			}
			if (mission.Briefing.Length == 9998 || mission.Briefing.Length == 0x21C)
			{
				strResults[i++] = "*Briefing has default length";
			}
			if (mission.Briefing.Events[0] == 9999 || mission.Briefing.Events[8] == 9999)
			{
				strResults[i++] = "*No briefing";
			}
			if (mission.BriefingQuestions.PreMissQuestions[0] == "")
			{
				strResults[i++] = "*No pre-mission Officer questions";
			}
			if (mission.BriefingQuestions.PostMissQuestions[0] == "")
			{
				strResults[i++] = "No post-mission Officer questions";
			}
			//this portion merely reduces the size of the text returned
			string [] strRes = new string [i];
			for (temp = 0; temp < i; temp++)
			{
				strRes[temp] = strResults[temp];
			}
			return strRes;
		}

		private string[] XvT(FileStream fsFile, bool BoP)
		{
			BinaryReader br = new BinaryReader(fsFile);
			int FG, MESSAGE, temp, iCraft = 0;	//consts and temp
			int i = 0;		//string counter
			int iFG;		//position counters
			string[] strResults = new string[50];
			bool bFail = false;
			int playerFG = -1;
			fsFile.Position = 2;
			FG = br.ReadInt16();
			MESSAGE = br.ReadInt16();
			if (FG > 46)
			{
				strResults[i] = "**Warning, more than 46 Flight Groups, mission may not be stable.";
				i++;
			}
			if (MESSAGE == 0) 
			{
				strResults[i] = "Mission has no in-flight messages.";
				i++;
			}
			if (MESSAGE > 64)
			{
				strResults[i] = "**Mission has more than 64 in-flight messages, errors will occur.";
				i++;
			}
			fsFile.Position = 1480 + FG * 1378 + MESSAGE * 116;
			temp = fsFile.ReadByte();
			if (temp == 0)
			{
				strResults[i] = "No Mission Complete message.";
				i++;
			}
			for (iFG = 1; iFG <= FG; iFG++)
			{
				fsFile.Position = iFG * 1378 - 1131;
				temp = fsFile.ReadByte();
				iCraft += temp + 1;
				fsFile.Position += 16;
				temp = fsFile.ReadByte();
				if (temp != 0)
				{
					strResults[i] = "Flight Group " + iFG + " is player craft.";
					if (bFail == true) strResults[i] = "**" + strResults[i];
					else playerFG = iFG;
					i++;
					bFail = true;
				}
			}
			if (playerFG == -1)
			{
				strResults[i] = "**No player craft.";
				i++;
			}
			if (iCraft > 36)
			{
				strResults[i] = "*More than 36 craft in mission, ensure all do not exist concurrently.";
				i++;
			}
			bFail = false;
			for (iFG = 1; iFG <= FG; iFG++)
			{
				fsFile.Position = iFG * 1378 - 1125;
				temp = fsFile.ReadByte();
				if (temp == 255)
				{
					strResults[i] = "Flight Group " + iFG + " has no AI.";
					if (iFG == playerFG) strResults[i] += " (player)";
					if (bFail == true) strResults[i] = "*" + strResults[i];
					i++;
					bFail = true;
				}
			}
			bFail = false;
			for (iFG = 1; iFG <= FG; iFG++)
			{
				fsFile.Position = iFG * 1378 - 1052;
				temp = fsFile.ReadByte();
				if (temp == 0)
				{
					strResults[i] = "Flight Group " + iFG + " has no orders.";
					if (iFG == playerFG) strResults[i] += " (player)";
					if (bFail == true) strResults[i] = "*" + strResults[i];
					i++;
					bFail = true;
				}
			}
			bFail = false;
			fsFile.Position = 6324 + FG * 1378 + MESSAGE * 116;
			temp = fsFile.ReadByte();
			if (temp == 15)
			{
				strResults[i] = "*No briefing";
				i++;
			}
			//check Descrip, different positions depending on platform
			if (!BoP) fsFile.Position = fsFile.Length - 1024;
			else fsFile.Position = fsFile.Length - 4096;
			temp = fsFile.ReadByte();
			if (temp == 0)
			{
				strResults[i] = "*No Mission description";
				i++;
			}
			//this portion merely reduces the size of the text returned
			string [] strRes = new string [i];
			for (temp = 0; temp < i; temp++)
			{
				strRes[temp] = strResults[temp];
			}
			return strRes;
		}

		private string[] XWA(FileStream fsFile)
		{
			BinaryReader br = new BinaryReader(fsFile);
			int FG, MESSAGE, temp, iCraft = 0;	//consts and temp
			int i = 0;		//string counter
			int iFG;		//position counters
			string[] strResults = new string[50];
			bool bFail = false;
			int playerFG = -1;
			fsFile.Position = 2;
			FG = br.ReadInt16();
			MESSAGE = br.ReadInt16();
			if (FG > 100)
			{
				strResults[i] = "**Warning, over 100 Flight Groups, mission may be unstable.";
				i++;
			}
			if (MESSAGE == 0) 
			{
				strResults[i] = "Mission has no in-flight messages.";
				i++;
			}
			if (MESSAGE > 65)
			{
				strResults[i] = "**Warning, over 65 in-flight messages, mission may be unstable.";
				i++;
			}
			fsFile.Position = 12916 + FG * 3646 + MESSAGE * 162;
			temp = fsFile.ReadByte();
			if (temp == 0)
			{
				strResults[i] = "No Mission Complete message.";
				i++;
			}
			for (iFG = 1; iFG <= FG; iFG++)
			{
				fsFile.Position = 5662 + iFG * 3646;
				temp = fsFile.ReadByte();
				iCraft += temp + 1;
				fsFile.Position += 16;
				temp = fsFile.ReadByte();
				if (temp != 0)
				{
					strResults[i] = "Flight Group " + iFG + " is player craft.";
					if (bFail == true) strResults[i] = "**" + strResults[i];
					else playerFG = iFG;
					i++;
					fsFile.Position += 69;
					if (fsFile.ReadByte() == 0) 
					{
						strResults[i] = "**Flight Group " + iFG + " has no mothership.";
						i++;
					}
					bFail = true;
				}
			}
			if (playerFG == -1)
			{
				strResults[i] = "**No player craft.";
				i++;
			}
			if (iCraft > 96)
			{
				strResults[i] = "*More than 96 craft. Ensure not all exist in the same region concurrently.";
				i++;
			}
			bFail = false;
			for (iFG = 1; iFG <= FG; iFG++)
			{
				fsFile.Position = 5668 + iFG * 3646;
				temp = fsFile.ReadByte();
				if (temp == 0)
				{
					strResults[i] = "Flight Group " + iFG + " has no AI.";
					if (iFG == playerFG) strResults[i] += " (player)";
					if (bFail == true) strResults[i] = "*" + strResults[i];
					i++;
					bFail = true;
				}
			}
			bFail = false;
			for (iFG = 1; iFG <= FG; iFG++)
			{
				fsFile.Position = 5756 + iFG * 3646;
				temp = fsFile.ReadByte();
				if (temp == 0)
				{
					strResults[i] = "Flight Group " + iFG + " has no orders.";
					if (iFG == playerFG) strResults[i] += " (player)";
					if (bFail == true) strResults[i] = "*" + strResults[i];
					i++;
					bFail = true;
				}
			}
			bFail = false;
			fsFile.Position = 17760 + FG * 3646 + MESSAGE * 162;
			temp = fsFile.ReadByte();
			if (temp == 15)
			{
				strResults[i] = "*No briefing.";
				i++;
			}
			//check Descrip
			fsFile.Position = fsFile.Length - 12288;
			temp = fsFile.ReadByte();
			if (temp == 0)
			{
				strResults[i] = "*No Mission description.";
				i++;
			}
			//this portion merely reduces the size of the text returned
			string [] strRes = new string [i];
			for (temp = 0; temp < i; temp++)
			{
				strRes[temp] = strResults[temp];
			}
			return strRes;
		}
	}
}
