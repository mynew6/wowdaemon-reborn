using System;
using System.Collections;
using WoWDaemon.Common;
namespace WoWDaemon.World
{
	/// <summary>
	/// Summary description for ObjectManager.
	/// </summary>
	[WorldPacketHandler()]
	public class ObjectManager
	{
		static ObjectManager()
		{
			m_worldObjects = new Hashtable[(int)OBJECTTYPE.MAX];
			for(int i = 0;i < (int)OBJECTTYPE.MAX;i++)
				m_worldObjects[i] = new Hashtable();
		}
		static Queue m_guidpool = new Queue();
		static ulong m_currentGUID = 0;
		static ulong m_currentMax = 0;
		static Hashtable[] m_worldObjects;

		[WorldPacketDelegate(WORLDMSG.INIT_GUIDS)]
		static void OnInitGuids(WORLDMSG msgID, BinReader data)
		{
			m_currentGUID = data.ReadUInt64();
			m_currentMax = data.ReadUInt64();
			m_guidpool.Enqueue(data.ReadUInt64());
			m_guidpool.Enqueue(data.ReadUInt64());
		}

		[WorldPacketDelegate(WORLDMSG.ACQUIRE_GUIDS_REPLY)]
		static void OnAcquireGuids(WORLDMSG msgID, BinReader data)
		{
			m_guidpool.Enqueue(data.ReadUInt64());
			m_guidpool.Enqueue(data.ReadUInt64());
		}

		public static ulong NextGUID()
		{
			ulong guid = m_currentGUID++;
			if(m_currentGUID == m_currentMax)
			{
				m_currentGUID = (ulong)m_guidpool.Dequeue();
				m_currentMax = (ulong)m_guidpool.Dequeue();
				WorldServer.Send(new WorldPacket(WORLDMSG.ACQUIRE_GUIDS));
			}
			return guid;
		}

		public static void AddWorldObject(WorldObject obj)
		{
			m_worldObjects[(int)obj.ObjectType][obj.GUID] = obj;
		}
		
		public static void RemoveWorldObject(WorldObject obj)
		{
			m_worldObjects[(int)obj.ObjectType].Remove(obj.GUID);
		}

		public static WorldObject GetWorldObject(OBJECTTYPE type, ulong guid)
		{
			return (WorldObject)m_worldObjects[(int)type][guid];
		}

		public static ArrayList GetAllObjects(OBJECTTYPE type)
		{
			return new ArrayList(m_worldObjects[(int)type].Values);
		}

		public static void CheckBeforeShutdown()
		{
			for(int i = 0;i < (int)OBJECTTYPE.MAX;i++)
			{
				if(m_worldObjects[i].Count > 0)
				{
					Console.WriteLine("Still " + m_worldObjects[i].Count + " " + ((OBJECTTYPE)i).ToString() + " in ObjectManager before shutting down.");
				}
			}
		}
	}
}
