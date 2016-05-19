using System;
using System.Collections;
using WoWDaemon.Common;
using WoWDaemon.Database.DataTables;
using WoWDaemon.Common.Attributes;

namespace WoWDaemon.World
{
	[UpdateObjectAttribute(MaxFields=(int)PLAYERFIELDS.MAX)]
	public class PlayerObject : LivingObject
	{
		protected DBCharacter m_character;
		protected int m_nextLevelExp;
		protected float m_scale;
		protected int m_displayID;
		protected byte m_playerFlags;
		public PlayerObject(DBCharacter c) : base()
		{
			m_character = c;
			m_nextLevelExp = c.Level * 1000;
			m_scale = c.Scale;
			m_displayID = c.DisplayID;
			m_playerFlags = 0;
			Inventory = new PlayerInventory(this);
			if(c.Items != null)
			{
				foreach(DBItem item in c.Items)
					Inventory.CreateItem(item);
			}
			ObjectManager.AddWorldObject(this);
		}

		public uint CharacterID
		{
			get {return m_character.ObjectId;}
		}

		public uint WorldMapID
		{
			get {return m_character.WorldMapID;}
			set {m_character.WorldMapID = value;}
		}

		public uint Continent
		{
			get { return m_character.Continent;}
			set { m_character.Continent = value;}
		}


		public override void Save()
		{
			m_character.Items = new DBItem[Inventory.ItemCount];
			int n = 0;
			for(int i = 0;i < Inventory.NumSlots;i++)
			{
				if(Inventory[i] != null)
				{
					Inventory[i].Save();
					m_character.Items[n] = Inventory[i].DBItem;
					n++;
				}
			}
			DBManager.SaveDBObject(m_character);			
		}

		public override void SaveAndRemove()
		{
			if(this.MapTile != null)
				MapTile.Map.Leave(this);
			m_character.Items = new DBItem[Inventory.ItemCount];
			int n = 0;
			for(int i = 0;i < Inventory.NumSlots;i++)
			{
				if(Inventory[i] != null)
				{
					Inventory[i].SaveAndRemove();
					m_character.Items[n] = Inventory[i].DBItem;
					n++;
				}
			}
			DBManager.SaveDBObject(m_character);
			DBManager.RemoveDBObject(m_character);
			ObjectManager.RemoveWorldObject(this);
		}

		public override string Name
		{
			get
			{
				return m_character.Name;
			}
		}



		#region Object Properties
		public override Vector Position
		{
			get {return m_character.Position;}
			set {m_character.Position = value;}
		}

		public override float Facing
		{
			get {return m_character.Facing;}
			set {m_character.Facing = value;}
		}


		public override uint MovementFlags
		{
			get {return 0;}
			set {}
		}

		public override OBJECTTYPE ObjectType
		{
			get { return OBJECTTYPE.PLAYER;}
		}
		#endregion

		public override void PreCreateObject(bool isClient)
		{
			base.PreCreateObject (isClient);
			UpdateValue(PLAYERFIELDS.BYTES_1);
			UpdateValue(PLAYERFIELDS.BYTES_2);
			Inventory.PreCreateOwner(isClient);
			if(isClient)
			{
				UpdateValue(UNITFIELDS.TARGET);
				UpdateValue(PLAYERFIELDS.SELECTION);
				UpdateValue(PLAYERFIELDS.XP);
				UpdateValue(PLAYERFIELDS.NEXTLEVEL_XP);
			}
		}

		#region OBJECTFIELDS
		public override ulong GUID
		{
			get { return m_character.ObjectId;}
		}

		public override float Scale
		{
			get {return m_scale;}
			set
			{
				m_scale = value;
				UpdateValue(OBJECTFIELDS.SCALE);
			}
		}

		public override HIER_OBJECTTYPE HierType
		{
			get {return HIER_OBJECTTYPE.PLAYER;}
		}
		#endregion

		#region UNITFIELDS
		public override int Health
		{
			get {return m_character.Health;}
			set {m_character.Health = value;UpdateValue(UNITFIELDS.HEALTH);}
		}

		public override int MaxHealth
		{
			get {return m_character.MaxHealth;}
			set {m_character.MaxHealth = value;UpdateValue(UNITFIELDS.MAX_HEALTH);}
		}

		public override POWERTYPE PowerType
		{
			get {return m_character.PowerType;}
			set {m_character.PowerType = value;UpdateValue(UNITFIELDS.BYTES_0);}
		}

		public override int Power
		{
			get {return m_character.Power;}
			set {m_character.Power = value; UpdateValue(UNITFIELDS.POWER0+(int)m_character.PowerType);}
		}

		public override int MaxPower
		{
			get {return m_character.MaxPower;}
			set {m_character.MaxPower = value;UpdateValue(UNITFIELDS.MAX_POWER0+(int)m_character.PowerType);}
		}

		[UpdateValueAttribute(UNITFIELDS.BYTES_0, BytesIndex=0)]
		public RACE Race
		{
			get {return m_character.Race;}
		}
		[UpdateValueAttribute(UNITFIELDS.BYTES_0, BytesIndex=1)]
		public CLASS Class
		{
			get {return m_character.Class;}
		}

		[UpdateValueAttribute(UNITFIELDS.BYTES_0, BytesIndex=2)]
		public byte Gender
		{
			get {return m_character.Gender;}
		}

		public override int Level
		{
			get {return m_character.Level;}
			set {m_character.Level = (byte)value; UpdateValue(UNITFIELDS.LEVEL);}
		}

		public override int BaseStrength
		{
			get { return m_character.BaseStrength;}
			set { m_character.BaseStrength = value;UpdateValue(UNITFIELDS.BASE_STRENGTH);}
		}

		public override int BaseAgility
		{
			get {return m_character.BaseAgility;}
			set {m_character.BaseAgility = value; UpdateValue(UNITFIELDS.BASE_AGILITY);}
		}

		public override int BaseStamina
		{
			get	{return m_character.BaseStamina;}
			set {m_character.BaseStamina = value;UpdateValue(UNITFIELDS.BASE_STAMINA);}
		}

		public override int BaseIntellect
		{
			get {return m_character.BaseIntellect;}
			set {m_character.BaseIntellect = value; UpdateValue(UNITFIELDS.BASE_INTELLECT);}
		}

		public override int BaseSpirit
		{
			get	{return m_character.BaseSpirit;}
			set {m_character.BaseSpirit = value;UpdateValue(UNITFIELDS.BASE_SPIRIT);}
		}

		public override int Strength
		{
			get {return 20;}
			set {}
		}

		public override int Agility
		{
			get {return 20;}
			set {}
		}

		public override int Stamina
		{
			get {return 20;}
			set {}
		}

		public override int Intellect
		{
			get {return 20;}
			set {}
		}

		public override int Spirit
		{
			get {return 20;}
			set {}
		}

		public override int DisplayID
		{
			get { return m_displayID;}
			set {m_displayID = value;UpdateValue(UNITFIELDS.DISPLAYID);}
		}

		public override int Faction
		{
			get {return m_character.Faction;}
			set {m_character.Faction = value;UpdateValue(UNITFIELDS.FACTION);}
		}

		[UpdateValueAttribute(UNITFIELDS.TARGET)]
		protected ulong m_target = 0;
		
		public ulong Target
		{
			get {return m_target;}
		}
		#endregion

		#region PLAYERFIELDS
		[UpdateValueAttribute(PLAYERFIELDS.XP)]
		public int Exp
		{
			get { return m_character.Exp;}
			set
			{
				m_character.Exp = value;
				UpdateValue(PLAYERFIELDS.XP);
			}
		}

		[UpdateValueAttribute(PLAYERFIELDS.NEXTLEVEL_XP)]
		public int NextLevelExp
		{
			get { return m_nextLevelExp;}
			set { UpdateValue(PLAYERFIELDS.NEXTLEVEL_XP); m_nextLevelExp = value;}
		}

		[UpdateValueAttribute(PLAYERFIELDS.BYTES_1, BytesIndex=0)]
		public byte Skin
		{
			get { return m_character.Skin;}
		}

		[UpdateValueAttribute(PLAYERFIELDS.BYTES_1, BytesIndex=1)]
		public byte Face
		{
			get { return m_character.Face;}
		}

		[UpdateValueAttribute(PLAYERFIELDS.BYTES_1, BytesIndex=2)]
		public byte HairStyle
		{
			get { return m_character.HairStyle;}
		}

		[UpdateValueAttribute(PLAYERFIELDS.BYTES_1, BytesIndex=3)]
		public byte HairColor
		{
			get { return m_character.HairColor;}
		}

		[UpdateValueAttribute(PLAYERFIELDS.BYTES_2, BytesIndex=0)]
		public byte PlayerFlags
		{
			get { return m_playerFlags;}
			set { UpdateValue(PLAYERFIELDS.BYTES_2); m_playerFlags = value;}
		}

		[UpdateValueAttribute(PLAYERFIELDS.BYTES_2, BytesIndex=1)]
		public byte FacialHairStyle
		{
			get { return m_character.FacialHairStyle;}
		}

		[UpdateValueAttribute(PLAYERFIELDS.BYTES_2, BytesIndex=2)]
		public byte NumBankSlots
		{
			get { return 0;}
		}

		[UpdateValueAttribute(PLAYERFIELDS.BYTES_2, BytesIndex=3)]
		public RESTEDSTATE RestedState
		{
			get { return m_character.RestedState;}
			set { UpdateValue(PLAYERFIELDS.BYTES_2); m_character.RestedState = value;}
		}

		[UpdateValueAttribute(PLAYERFIELDS.SELECTION)]
		protected ulong m_selection;
		public ulong Selection
		{
			get { return m_selection;}
			set { UpdateValue(PLAYERFIELDS.SELECTION); m_selection = value;}
		}

		[UpdateValueAttribute]
		public readonly PlayerInventory Inventory;
		#endregion
	}
}
