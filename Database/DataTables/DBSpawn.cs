using System;
using WoWDaemon.Common;
using WoWDaemon.Common.Attributes;
namespace WoWDaemon.Database.DataTables
{
	/// <summary>
	/// Summary description for DBSpawn.
	/// </summary>
	[DataTable(TableName="Spawn")]
	public class DBSpawn : DBObject
	{
		[DataElement(Name="WorldMapID")]
		private uint m_worldMapID;
		[DataElement(Name="Position")]
		private Vector m_position = new Vector();
		[DataElement(Name="Facing")]
		private float m_facing;
		[DataElement(Name="CreatureID")]
		private uint m_creatureID;

		public DBSpawn()
		{
		}

		public uint WorldMapID
		{
			get { return m_worldMapID;}
			set { m_worldMapID = value; Dirty = true;}
		}

		public Vector Position
		{
			get { return m_position;}
			set { m_position = value; Dirty = true;}
		}

		public float Facing
		{
			get { return m_facing;}
			set { m_facing = value; Dirty = true;}
		}

		public uint CreatureID
		{
			get { return m_creatureID;}
			set { m_creatureID = value; Dirty = true;}
		}

		public override bool AutoSave
		{
			get { return true;}
			set {}
		}

		[Relation(LocalField="CreatureID", RemoteField="Creature_ID", AutoLoad=true, AutoDelete=false)]
		public DBCreature Creature;
	}
}
