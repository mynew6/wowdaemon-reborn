using System;
using WoWDaemon.Common;
using WoWDaemon.Database.DataTables;
using WoWDaemon.Common.Attributes;
namespace WoWDaemon.World
{
	public abstract class ObjectInventory
	{
		protected WorldObject m_owner;
		[UpdateValueAttribute(PLAYERFIELDS.INV_SLOTS, ArraySize=69, OnlyForType=typeof(PlayerObject))]
		[UpdateValueAttribute(CONTAINERFIELDS.SLOTS, ArraySize=20, OnlyForType=typeof(ContainerObject))]
		protected ulong[] m_slots;
		protected ItemObject[] m_invObjects;
		int m_baseField;
		int m_itemCount;
		public ObjectInventory(WorldObject owner, int baseField)
		{
			m_owner = owner;
			m_baseField = baseField;
			m_itemCount = 0;
		}

		public ItemObject CreateItem(DBItem dbItem)
		{
			ItemObject item;
			if(dbItem.Template == null)
			{
				Console.WriteLine("DBItem " + dbItem.ObjectId + " is missing Item template on worldserver.");
				return null;
			}
			if(dbItem.Template.InvType == INVTYPE.BAG)
				item = new ContainerObject(dbItem, this);
			else
				item = new ItemObject(dbItem, this);
			m_slots[dbItem.OwnerSlot] = item.GUID;
			m_invObjects[dbItem.OwnerSlot] = item;
			m_owner.UpdateValue(m_baseField+dbItem.OwnerSlot*2);
			m_itemCount++;
			return item;
		}

		public virtual void PreCreateOwner(bool isClient)
		{
			for(int i = 0;i < m_slots.Length;i++)
				if(m_slots[i] != 0)
					m_owner.UpdateValue(m_baseField+i*2);
		}

		public ItemObject GetItem(int slot)
		{
			if(slot > m_invObjects.Length)
				return null;
			return m_invObjects[slot];
		}

		public ItemObject this[int slot]
		{
			get { return GetItem(slot);}
		}

		public int ItemCount
		{
			get { return m_itemCount;}
		}

		public int NumSlots
		{
			get { return m_slots.Length;}
		}

		public abstract PlayerObject Owner
		{
			get;
		}

		public virtual WorldObject InventoryOwner
		{
			get { return m_owner;}
		}
	}

	public class PlayerInventory : ObjectInventory
	{
		public PlayerInventory(WorldObject owner) : base(owner,(int)PLAYERFIELDS.INV_SLOTS)
		{
			m_slots = new ulong[69];
			m_invObjects = new ItemObject[69];
		}

		public override PlayerObject Owner
		{
			get
			{
				return (PlayerObject)m_owner;
			}
		}
		public ItemObject GetItem(INVSLOT slot)
		{
			return GetItem((int)slot);
		}

		public ItemObject this[INVSLOT slot]
		{
			get { return GetItem((int)slot);}
		}

		public override void PreCreateOwner(bool isClient)
		{
			for(int i = 0;i <= (int)INVSLOT.EQUIPPEDLAST;i++)
				if(m_slots[i] != 0)
					Owner.UpdateValue(((int)PLAYERFIELDS.INV_SLOTS)+i*2);
			if(isClient)
			{
				for(int i = (int)INVSLOT.NONE_EQUIPFIRST;i < (int)INVSLOT.NUM_INVENTORY_SLOTS;i++)
					if(m_slots[i] != 0)
						Owner.UpdateValue(((int)PLAYERFIELDS.INV_SLOTS)+i*2);
			}
		}

		public int AddCreateInventory(BinWriter w, bool isClient)
		{
			int numItems = 0;
			for(int i = 0;i <= (int)INVSLOT.EQUIPPEDLAST;i++)
			{
				if(m_slots[i] != 0)
				{
					m_invObjects[i].AddCreateObject(w, false, true);
					numItems++;
				}
			}
			if(isClient)
			{
				for(int i = (int)INVSLOT.NONE_EQUIPFIRST;i < (int)INVSLOT.NUM_INVENTORY_SLOTS;i++)
				{
					if(m_slots[i] == 0)
						continue;
					numItems++;
					m_invObjects[i].AddCreateObject(w, false, true);
					if(m_invObjects[i].ObjectType == OBJECTTYPE.CONTAINER)
					{
						ContainerObject container = m_invObjects[i] as ContainerObject;
						for(int j = 0;j < container.Template.ContainerSlots;j++)
						{
							ItemObject item = container.Inventory[j];
							if(item != null)
							{
								numItems++;
								item.AddCreateObject(w, false, true);
							}
						}
					}
				}
			}
			return numItems;
		}

		public void SendDestroyInventory(uint toCharacterID)
		{
			for(int i = 0;i <= (int)INVSLOT.EQUIPPEDLAST;i++)
			{
				if(m_slots[i] != 0)
				{
					ServerPacket pkg = new ServerPacket(SMSG.OBJECT_DESTROY);
					pkg.Write(m_slots[i]);
					pkg.Finish();
					pkg.AddDestination(toCharacterID);
					WorldServer.Send(pkg);
				}
			}
		}
	}

	public class ContainerInventory : ObjectInventory
	{
		public ContainerInventory(WorldObject owner) : base(owner, (int)CONTAINERFIELDS.SLOTS)
		{
			m_slots = new ulong[20];
			m_invObjects = new ItemObject[20];
		}

		public override PlayerObject Owner
		{
			get { return ((ContainerObject)m_owner).ContainedIn.Owner;}
		}
	}
}
