using System;
using WoWDaemon.Common;
using WoWDaemon.Common.Attributes;
using WoWDaemon.Database.DataTables;
using WoWDaemon.World;
using WorldScripts.Living;
namespace WorldScripts.ScriptPackets
{
	/// <summary>
	/// Summary description for Spawn.
	/// </summary>
	[ScriptPacketHandler()]
	public class Spawn
	{
		[ScriptPacketHandler(MsgID=0x01)]
		static void OnSpawn(int msgID, BinReader data)
		{
			uint charID = data.ReadUInt32();
			uint creatureID = data.ReadUInt32();
			int displayID = data.ReadInt32();

			WorldClient client = WorldServer.GetClientByCharacterID(charID);
			if(client == null)
				return;
			DBCreature creature = (DBCreature)DBManager.GetDBObject(typeof(DBCreature), creatureID);
			MonsterBase unit = new MonsterBase(creature);
			unit.Position = client.Player.Position;
			unit.Facing = client.Player.Facing;
			unit.DisplayID = displayID;
			unit.MaxHealth = unit.Health = 100;
			unit.MaxPower = unit.Power = 100;
			unit.PowerType = POWERTYPE.MANA;
			unit.Level = new Random().Next(10);
			unit.Faction = 0;
			
			client.Player.MapTile.Map.Enter(unit);
			unit.StartGetNodes();
		}
	}
}
