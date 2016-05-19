using System;
using WoWDaemon.Common;
using WoWDaemon.Database.DataTables;
using WoWDaemon.Common.Attributes;
namespace WoWDaemon.World
{
	/// <summary>
	/// Summary description for UnitBase.
	/// </summary>
	[UpdateObject(MaxFields=(int)UNITFIELDS.MAX)]
	public class UnitBase : LivingObject
	{
		private string m_name;
		private int m_creatureFlags;
		private int m_creatureType;
		private int m_creatureFamily;

		private ulong m_guid;
		private uint m_entry;
		private float m_scale = 1.0f;

		private uint m_movementFlags = 0;
		private Vector m_position = new Vector(0,0,0);
		private float m_facing = 0.0f;

		private int m_health;
		private int m_maxHealth;
		private int m_power;
		private int m_maxPower;
		private POWERTYPE m_powerType;
		private int m_level;
		private int m_faction;
		private int m_displayID;

		public UnitBase(DBCreature creature)
		{
			m_name = creature.Name;
			m_creatureFlags = creature.Flags;
			m_creatureType = creature.CreatureType;
			m_creatureFamily = creature.CreatureFamily;

			m_guid = ObjectManager.NextGUID();
			m_entry = creature.ObjectId;
			ObjectManager.AddWorldObject(this);
		}

		public override void PreCreateObject(bool isClient)
		{
			base.PreCreateObject (isClient);
		}

		public override void SaveAndRemove()
		{
			if(MapTile != null)
				MapTile.Map.Leave(this);
			ObjectManager.RemoveWorldObject(this);
		}


		#region Creature Properties
		public override string Name
		{
			get {return m_name;}
		}

		public int CreatureFlags
		{
			get { return m_creatureFlags;}
		}

		public int CreatureType
		{
			get { return m_creatureType;}
		}

		public int CreatureFamily
		{
			get { return m_creatureFamily;}
		}
		#endregion

		#region Object Properties
		public override OBJECTTYPE ObjectType
		{
			get {return OBJECTTYPE.UNIT;}
		}

		public override uint MovementFlags
		{
			get {return m_movementFlags;}
			set { m_movementFlags = value;}
		}

		public override Vector Position
		{
			get { return m_position;}
			set {m_position = value;}
		}

		public override float Facing
		{
			get { return m_facing;}
			set { m_facing = value;}
		}
		#endregion

		#region OBJECTFIELDS
		public override ulong GUID
		{
			get { return m_guid;}
		}

		public override uint Entry
		{
			get {return m_entry;}
			set {}
		}

		public override HIER_OBJECTTYPE HierType
		{
			get {return HIER_OBJECTTYPE.UNIT;}
		}

		public override float Scale
		{
			get { return m_scale;}
			set { m_scale = value; UpdateValue(OBJECTFIELDS.SCALE);}
		}
		#endregion

		public override int Health
		{
			get {return m_health;}
			set {m_health = value;UpdateValue(UNITFIELDS.HEALTH);}
		}

		public override int MaxHealth
		{
			get {return m_maxHealth;}
			set {m_maxHealth = value;UpdateValue(UNITFIELDS.MAX_HEALTH);}
		}

		public override int Power
		{
			get {return m_power;}
			set {m_power = value;UpdateValue(((int)UNITFIELDS.POWER0) + (int)m_powerType);}
		}

		public override int MaxPower
		{
			get {return m_maxPower;}
			set {m_maxPower = value;UpdateValue(((int)UNITFIELDS.MAX_POWER0) + (int)m_powerType);}
		}

		public override POWERTYPE PowerType
		{
			get {return m_powerType;}
			set {m_powerType = value;UpdateValue(UNITFIELDS.BYTES_0);}
		}

		public override int Level
		{
			get {return m_level;}
			set {m_level = value;UpdateValue(UNITFIELDS.LEVEL);}
		}

		public override int Faction
		{
			get {return m_faction;}
			set {m_faction = value;UpdateValue(UNITFIELDS.FACTION);}
		}

		public override int DisplayID
		{
			get {return m_displayID;}
			set {m_displayID = value;UpdateValue(UNITFIELDS.DISPLAYID);}
		}

		public override int Agility
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public override int BaseAgility
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public override int BaseIntellect
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public override int BaseSpirit
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public override int BaseStamina
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public override int BaseStrength
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public override int Intellect
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public override int Spirit
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public override int Stamina
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}

		public override int Strength
		{
			get
			{
				return 0;
			}
			set
			{
			}
		}
	}
}
