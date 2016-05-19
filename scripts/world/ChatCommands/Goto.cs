using System;
using System.Text.RegularExpressions;
using System.Collections;
using WoWDaemon.Common;
using WoWDaemon.Common.Attributes;
using WoWDaemon.World;
namespace WorldScripts.ChatCommands
{
	/// <summary>
	/// Summary description for Worldport.
	/// </summary>
	[ChatCmdHandler()]
	public class Goto
	{
		static Goto()
		{
			AddGoto("Undercity 0 1628.3 239.925 64.5006");
			AddGoto("Stormwind 0 -8913.23 554.633 93.7944");
			AddGoto("Orgimmar 1 1484.36 -4417.03 24.4709");
			AddGoto("Ironforge 0 -4981.25 -881.542 501.66");
			AddGoto("Gnomeregan 0 -5179.58 660.421 388.391");
			AddGoto("Dalaran 0 386.938 212.299 43.6994");
			AddGoto("Darkshire 0 -10567.5 -1169.86 29.0826");
			AddGoto("AeriePeak 0 327.814 -1959.99 197.724");
			AddGoto("Lakeshire 0 -9282.98 -2269.64 69.39");
			AddGoto("Ambermill 0 -126.954 815.624 66.0224");
			AddGoto("BootyBay 0 -14406.6 419.353 22.3907");
			AddGoto("Stonard 0 -10452.5 -3263.59 20.1782");
			AddGoto("Brill 0 2260.64 289.021 34.1291");
			AddGoto("Moonbrook 0 -11018.4 1513.69 43.0152");
			AddGoto("Menethil 0 -3740.29 -755.08 10.9643");
			AddGoto("Astrnaar 1 2745.85 -378.33 108.253");
			AddGoto("Aszhara 1 3546.8 -5287.96 109.935");
			AddGoto("BaelModan 1 -4095.7 -2305.74 124.914");
			AddGoto("Crossroads 1 -456.263 -2652.7 95.615");
			AddGoto("Auberdine 1 6439.28 614.957 5.98831");
			AddGoto("Thalanaar 1 -4517.1 -780.415 -40.736");
			AddGoto("Razorhill 1 304.762 -4734.97 9.30458");
			AddGoto("Bloodhoof 1 -2321.74 -378.941 -9.40597");
			AddGoto("Racetrack 1 -6202.16 -3901.68 -60.2858");
			AddGoto("Tanaris 1 -6942.47 -4847.1 0.667853");
			AddGoto("StonetalonPeak 1 2506.3 1470.14 262.722");
			AddGoto("FreewindPost 1 -5437.4 -2437.47 89.3083");
			AddGoto("Darnassus 1 9948.55 2413.59 1327.23");
			AddGoto("Dolanaar 1 9892.57 982.424 1313.83");
			AddGoto("TheramoreIsle 1 -3729.36 -4421.41 30.4474");
			AddGoto("Hyjal 1 4674.88 -3638.37 965.264");
			AddGoto("Thunderbluff 1 -1200 -50 200");
		}
		static Regex parseGoto = new Regex(
			@"(?<name>\S+)\s" +
			@"(?<continent>\S+)\s" +
			@"(?<x>\S+)\s" +
			@"(?<y>\S+)\s" +
			@"(?<z>\S+)", RegexOptions.Compiled);
		class Place
		{
			public string name;
			public uint worldmapID;
			public float x;
			public float y;
			public float z;
		}
		static void AddGoto(string str)
		{
			Match aMatch = parseGoto.Match(str);
			if(aMatch.Success == false)
			{
				Console.WriteLine("Failed to match goto: " + str);
				return;
			}
			Place aPlace = new Place();
			try
			{
				aPlace.name = aMatch.Groups["name"].Value.ToLower();
				aPlace.worldmapID = (uint)( aMatch.Groups["continent"].Value == "0" ? 1 : 2);
				aPlace.x = float.Parse(aMatch.Groups["x"].Value);
				aPlace.y = float.Parse(aMatch.Groups["y"].Value);
				aPlace.z = float.Parse(aMatch.Groups["z"].Value);
			}
			catch(Exception)
			{
				Console.WriteLine("Failed to parse goto: " + str);
				return;
			}
			places[aPlace.name] = aPlace;
		}

		static Hashtable places = new Hashtable();

		[ChatCmdAttribute("goto", "goto <name>")]
		static bool OnGoto(WorldClient client, string input)
		{
			string[] split = input.Split(' ');
			if(split.Length != 2)
				return false;
			Place aPlace = (Place)places[split[1]];
			if(aPlace == null)
			{
				IEnumerator e = places.Keys.GetEnumerator();
				int i = 0;
				string str = "Available places are: ";
				while(e.MoveNext())
				{
					str += (string)e.Current;
					str += ", ";
					i++;
					if(i == 8)
					{
						Chat.System(client, str);
						str = string.Empty;
						i = 0;
					}
				}
				if(i != 0)
					Chat.System(client, str);
				return true;
			}
			client.Player.Position = new Vector(aPlace.x, aPlace.y, aPlace.z);
			client.Player.WorldMapID = aPlace.worldmapID;
			MapManager.ChangeMap(client);
			return true;
		}
	}
}
