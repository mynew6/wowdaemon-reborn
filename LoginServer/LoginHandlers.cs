using System;
using System.IO;
using WoWDaemon.Common;
using WoWDaemon.Database.DataTables;
using WoWDaemon.Database.MemberValues;
using ICSharpCode.SharpZipLib.Zip.Compression;
namespace WoWDaemon.Login
{
	/// <summary>
	/// Summary description for LoginHandlers.
	/// </summary>
	[LoginPacketHandler()]
	public class LoginHandlers
	{
		static SerializeValue[] itemTemplateValues;
		static LoginHandlers()
		{
			MemberValue[] values = MemberValue.GetMemberValues(typeof(DBItemTemplate), typeof(Common.Attributes.DataElement), true, true);
			itemTemplateValues = SerializeValue.GetSerializeValues(values);
		}

		[LoginPacketDelegate(WORLDMSG.PLAYER_LEAVE_WORLD)]
		static void OnPlayerLeaveWorld(WorldConnection connection, WORLDMSG msgID, BinReader data)
		{
			LoginClient client = LoginServer.GetLoginClientByCharacterID(data.ReadUInt32());
			if(client == null)
				return;
			if(client.IsLoggingOut)
			{
				LoginServer.RemoveCharacter(client);
				BinWriter pkg = LoginClient.NewPacket(SMSG.LOGOUT_COMPLETE);
				client.Send(pkg);
				client.IsLoggingOut = false;
				client.WorldConnection = null;
			}
			else if(client.IsChangingMap)
			{

			}
			else
			{
				client.Close("Kicked from worldserver.");
			}
		}

		[LoginPacketDelegate(CMSG.LOGOUT_REQUEST)]
		static bool OnPlayerLogout(LoginClient client, CMSG msgID, BinReader data)
		{
			client.IsLoggingOut = true;
			LoginServer.LeaveWorld(client);
			return true;
		}


		[LoginPacketDelegate(CMSG.REQUEST_UI_CONFIG)]
		static bool OnRequestUIConfig(LoginClient client, CMSG msgID, BinReader data)
		{
			client.SendConfig(data.ReadInt32());
			return true;
		}
		
		[LoginPacketDelegate(CMSG.SAVE_UI_CONFIG)]
		static bool OnSaveUIConfig(LoginClient client, CMSG msgID, BinReader data)
		{
			if(client.Character == null)
				return true;
			uint type = data.ReadUInt32();
			int len = data.ReadInt32();
			string conf = string.Empty;
			if(len > 0)
			{
				try
				{
					byte[] compressed = data.ReadBytes((int)(data.BaseStream.Length-data.BaseStream.Position));
					Inflater inflater = new Inflater();
					inflater.SetInput(compressed);
					byte[] decompressed = new byte[len];
					inflater.Inflate(decompressed);
					conf = System.Text.ASCIIEncoding.ASCII.GetString(decompressed);
				}
				catch(Exception e)
				{
					Console.WriteLine("Failed to decompress config type " + type + ": " + e.Message);
					return true;
				}
			}
			switch(type)
			{
				case 0:
					client.Character.UIConfig0 = conf;
					client.Character.Dirty = true;
					break;
				case 1:
					client.Character.UIConfig1 = conf;
					client.Character.Dirty = true;
					break;
				case 2:
					client.Character.UIConfig2 = conf;
					client.Character.Dirty = true;
					break;
				case 3:
					client.Character.UIConfig3 = conf;
					client.Character.Dirty = true;
					break;
				case 4:
					client.Character.UIConfig4 = conf;
					client.Character.Dirty = true;
					break;
				default:
					Console.WriteLine("Unknown config type: " + type);
					Console.WriteLine(conf);
					return true;
			}
			DataServer.Database.SaveObject(client.Character);
			return true;
		}
		[LoginPacketDelegate(CMSG.CREATURE_QUERY)]
		static bool OnCreatureQuery(LoginClient client, CMSG msgID, BinReader data)
		{
			uint id = data.ReadUInt32();
			DBCreature creature = (DBCreature)DataServer.Database.FindObjectByKey(typeof(DBCreature), id);
			if(creature == null)
			{
				client.Close("OnCreatureQuery(): id didn't exists.");
				return true;
			}
			BinWriter w = LoginClient.NewPacket(SMSG.CREATURE_QUERY_RESPONSE);
			w.Write(creature.ObjectId);
			w.Write(creature.Name);
			w.Write(creature.Name1);
			w.Write(creature.Name2);
			w.Write(creature.Name3);
			w.Write(creature.Title);
			w.Write(creature.Flags);
			w.Write(creature.CreatureType);
			w.Write(creature.CreatureFamily);
			w.Write(0); // unknown
			client.Send(w);
			return true;
		}

		[LoginPacketDelegate(CMSG.ITEM_QUERY_SINGLE)]
		static bool OnItemQuerySingle(LoginClient client, CMSG msgID, BinReader data)
		{
			uint id = data.ReadUInt32();
			DBItemTemplate template = (DBItemTemplate)DataServer.Database.FindObjectByKey(typeof(DBItemTemplate), id);
			if(template == null)
			{
				client.Close("Client requested an item template that didn't exists.");
				return true;
			}
			BinWriter w = LoginClient.NewPacket(SMSG.ITEM_QUERY_SINGLE_RESPONSE);
			w.Write(id);
			foreach(SerializeValue value in itemTemplateValues)
				value.Serialize(template, w);
			client.Send(w);
			return true;
		}

		[LoginPacketDelegate(CMSG.NAME_QUERY)]
		static bool OnNameQuery(LoginClient client, CMSG msgID, BinReader data)
		{
			uint id = data.ReadUInt32();
			LoginClient other = LoginServer.GetLoginClientByCharacterID(id);
			if(other == null)
			{
				client.Close("Tried to query a char that wasn't online.");
				return true;
			}
			BinWriter pkg = LoginClient.NewPacket(SMSG.NAME_QUERY_RESPONSE);
			pkg.Write(other.Character.ObjectId);
			pkg.Write(0); // high id
			pkg.Write(other.Character.Name);
			pkg.Write((int)other.Character.Race);
			pkg.Write((int)other.Character.Gender);
			pkg.Write((int)other.Character.Class);
			client.Send(pkg);
			return true;
		}		
	}
}