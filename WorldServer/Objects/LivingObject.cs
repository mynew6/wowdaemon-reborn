using System;
using WoWDaemon.Common;
using WoWDaemon.Common.Attributes;

namespace WoWDaemon.World
{
	public enum UNITSTANDSTATE : byte
	{
		STANDING = 0,
		SITTING = 1,
		SITTINGCHAIR = 2,
		SLEEPING = 3,
		SITTINGCHAIRLOW = 4,
		SITTINGCHAIRMEDIUM = 5,
		SITTINGCHAIRHIGH = 6,
		DEAD = 7,
		KNEEL = 8
	};

	public abstract class LivingObject : WorldObject
	{

		[UpdateValueAttribute(UNITFIELDS.FLAGS)]
		protected uint m_flags = 0;
		[UpdateValueAttribute(UNITFIELDS.MOUNT_DISPLAYID)]
		protected int m_mountDisplayID = 0;
		[UpdateValueAttribute(UNITFIELDS.BYTES_1, BytesIndex=0)]
		protected UNITSTANDSTATE m_standState = 0;

		public LivingObject() : base()
		{

		}

		public override void PreCreateObject(bool isClient)
		{
			base.PreCreateObject(isClient);

			UpdateValue(UNITFIELDS.HEALTH);
			UpdateValue(UNITFIELDS.MAX_HEALTH);
			UpdateValue((UNITFIELDS)((int)(UNITFIELDS.POWER0))+(int)PowerType);
			UpdateValue((UNITFIELDS)((int)(UNITFIELDS.MAX_POWER0))+(int)PowerType);
			UpdateValue(UNITFIELDS.LEVEL);
			UpdateValue(UNITFIELDS.FACTION);
			UpdateValue(UNITFIELDS.DISPLAYID);
			UpdateValue(UNITFIELDS.BYTES_0);
			UpdateValue(UNITFIELDS.BYTES_1);
			UpdateValue(UNITFIELDS.FLAGS);
			UpdateValue(UNITFIELDS.MOUNT_DISPLAYID);
			if(isClient)
			{
				UpdateValue(UNITFIELDS.STRENGTH);
				UpdateValue(UNITFIELDS.AGILITY);
				UpdateValue(UNITFIELDS.STAMINA);
				UpdateValue(UNITFIELDS.INTELLECT);
				UpdateValue(UNITFIELDS.SPIRIT);
				UpdateValue(UNITFIELDS.BASE_STRENGTH);
				UpdateValue(UNITFIELDS.BASE_AGILITY);
				UpdateValue(UNITFIELDS.BASE_STAMINA);
				UpdateValue(UNITFIELDS.BASE_INTELLECT);
				UpdateValue(UNITFIELDS.BASE_SPIRIT);
			}
		}

		public abstract string Name
		{
			get;
		}

		#region UNITFIELDS
		[UpdateValueAttribute(UNITFIELDS.HEALTH)]
		public abstract int Health
		{
			get;
			set;
		}

		[UpdateValueAttribute(UNITFIELDS.MAX_HEALTH)]
		public abstract int MaxHealth
		{
			get;
			set;
		}
		[UpdateValueAttribute(UNITFIELDS.BYTES_0, BytesIndex=3)]
		public abstract POWERTYPE PowerType
		{
			get;
			set;
		}

		[UpdateValueAttribute(UNITFIELDS.MANA)]
		[UpdateValueAttribute(UNITFIELDS.RAGE)]
		[UpdateValueAttribute(UNITFIELDS.FOCUS)]
		[UpdateValueAttribute(UNITFIELDS.ENERGY)]
		public abstract int Power
		{
			get;
			set;
		}

		[UpdateValueAttribute(UNITFIELDS.MAX_MANA)]
		[UpdateValueAttribute(UNITFIELDS.MAX_RAGE)]
		[UpdateValueAttribute(UNITFIELDS.MAX_FOCUS)]
		[UpdateValueAttribute(UNITFIELDS.MAX_ENERGY)]
		public abstract int MaxPower
		{
			get;
			set;
		}

		[UpdateValueAttribute(UNITFIELDS.BASE_STRENGTH)]
		public abstract int BaseStrength
		{
			get;
			set;
		}

		[UpdateValueAttribute(UNITFIELDS.BASE_AGILITY)]
		public abstract int BaseAgility
		{
			get;
			set;
		}

		[UpdateValueAttribute(UNITFIELDS.BASE_STAMINA)]
		public abstract int BaseStamina
		{
			get;
			set;
		}

		[UpdateValueAttribute(UNITFIELDS.BASE_INTELLECT)]
		public abstract int BaseIntellect
		{
			get;
			set;
		}

		[UpdateValueAttribute(UNITFIELDS.BASE_SPIRIT)]
		public abstract int BaseSpirit
		{
			get;
			set;
		}


		[UpdateValueAttribute(UNITFIELDS.STRENGTH)]
		public abstract int Strength
		{
			get;
			set;
		}

		[UpdateValueAttribute(UNITFIELDS.AGILITY)]
		public abstract int Agility
		{
			get;
			set;
		}

		[UpdateValueAttribute(UNITFIELDS.STAMINA)]
		public abstract int Stamina
		{
			get;
			set;
		}

		[UpdateValueAttribute(UNITFIELDS.INTELLECT)]
		public abstract int Intellect
		{
			get;
			set;
		}

		[UpdateValueAttribute(UNITFIELDS.SPIRIT)]
		public abstract int Spirit
		{
			get;
			set;
		}


		[UpdateValueAttribute(UNITFIELDS.LEVEL)]
		public abstract int Level
		{
			get;
			set;
		}

		[UpdateValueAttribute(UNITFIELDS.FACTION)]
		public abstract int Faction
		{
			get;
			set;
		}

		[UpdateValueAttribute(UNITFIELDS.DISPLAYID)]
		public abstract int DisplayID
		{
			get;
			set;
		}

		public int MountDisplayID
		{
			get { return m_mountDisplayID;}
			set { m_mountDisplayID = value; UpdateValue(UNITFIELDS.MOUNT_DISPLAYID);}
		}

		public uint Flags
		{
			get { return m_flags;}
			set { m_flags = value; UpdateValue(UNITFIELDS.FLAGS);}
		}

		public UNITSTANDSTATE StandState
		{
			get { return m_standState;}
			set { m_standState = value; UpdateValue(UNITFIELDS.BYTES_1);}
		}

		#endregion

	}
}
