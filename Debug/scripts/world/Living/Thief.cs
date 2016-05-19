using System;
using WoWDaemon.Common;
using WoWDaemon.World;
using WoWDaemon.Database.DataTables;
namespace WorldScripts.Living
{
	public class Thief : UnitBase
	{
		public Thief(DBCreature creature) : base(creature)
		{
			MaxHealth = Health = 100;
			MaxPower = Power = 0;
			PowerType = POWERTYPE.RAGE;
			Level = 1;
			Faction = 0;
			DisplayID = 508;
		}
	}
}
