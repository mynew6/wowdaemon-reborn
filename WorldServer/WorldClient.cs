using System;
using WoWDaemon.Common;
using WoWDaemon.Database.DataTables;

namespace WoWDaemon.World
{
	/// <summary>
	/// Not a network type client
	/// </summary>
	public class WorldClient
	{
		DBCharacter m_character;
		PlayerObject m_player;
		public WorldClient(DBCharacter character)
		{
			m_character = character;
			m_player = WorldServer.Scripts.GetNewPlayerObject(character);
			
			WorldServer.AddClient(this);
		}

		internal void CreatePlayerObject()
		{
			BinWriter w = new BinWriter();
			w.Write(0);
			m_player.AddCreateObject(w, true, true);
			w.Set(0, m_player.Inventory.AddCreateInventory(w, true)+1);

			BinWriter pkg = new BinWriter();
			pkg.Write((int)w.BaseStream.Length);
			pkg.Write(ZLib.Compress(w.GetBuffer(), 0, (int)w.BaseStream.Length));
			Send(SMSG.COMPRESSED_UPDATE_OBJECT, pkg);
		}

		public uint CharacterID
		{
			get
			{
				return m_character.ObjectId;
			}
		}

		public PlayerObject Player
		{
			get
			{
				return m_player;
			}
		}

		public void LeaveWorld()
		{
			m_player.SaveAndRemove();
			WorldServer.RemoveClient(this);
			WorldPacket pkg = new WorldPacket(WORLDMSG.PLAYER_LEAVE_WORLD);
			pkg.Write(m_character.ObjectId);
			WorldServer.Send(pkg);
		}

		public void Send(SMSG msgID, byte[] data, int index, int count)
		{
			ServerPacket pkg = new ServerPacket(msgID);
			pkg.Write(data, index, count);
			pkg.Finish();
			pkg.AddDestination(m_character.ObjectId);
			WorldServer.Send(pkg);
		}

		public void Send(SMSG msgID, BinWriter data)
		{
			ServerPacket pkg = new ServerPacket(msgID);
			pkg.Write(data.GetBuffer(), 0, (int)data.BaseStream.Length);
			pkg.Finish();
			pkg.AddDestination(m_character.ObjectId);
			WorldServer.Send(pkg);
		}
	}
}
