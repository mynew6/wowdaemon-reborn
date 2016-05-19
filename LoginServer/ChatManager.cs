using System;
using System.Collections;
using System.Reflection;
using WoWDaemon.Common;
using WoWDaemon.Database;
using WoWDaemon.Database.DataTables;
namespace WoWDaemon.Login
{
	public delegate bool ChatCmdDelegate(LoginClient client, string input);

	/// <summary>
	/// Summary description for ChatManager.
	/// </summary>
	[LoginPacketHandler()]
	public class ChatManager
	{
		
		class ChatCommand
		{
			public string cmd;
			public string usage;
			public ChatCmdDelegate func;
		}

		static Hashtable cmds = new Hashtable();

		public static void RegisterChatCommand(string cmd, string usage, ChatCmdDelegate func)
		{
			ChatCommand chatcmd = new ChatCommand();
			chatcmd.cmd = cmd.ToLower();
			chatcmd.usage = usage;
			chatcmd.func = func;
			cmds[chatcmd.cmd] = chatcmd;
		}

		internal static void ClearChatCmds()
		{
			cmds.Clear();
		}

		static bool OnChatCommand(LoginClient client, string msg)
		{
			string[] split = msg.Split(' ');
			string cmd = split[0].ToLower();
			ChatCommand chatcmd = (ChatCommand)cmds[cmd];
			if(chatcmd == null)
				return false;
			if(chatcmd.func(client, msg) == false)
			{
				Chat.System(client, chatcmd.usage);
			}
			return true;
		}

		[LoginPacketDelegate(CMSG.MESSAGECHAT)]
		static bool OnMessageChat(LoginClient client, CMSG msgID, BinReader data)
		{
			CHATMESSAGETYPE type = (CHATMESSAGETYPE)data.ReadInt32();
			/*int language =*/ data.ReadInt32();
			string target = string.Empty;
			if(type == CHATMESSAGETYPE.WHISPER)
				target = data.ReadString(0x100);
			string msg = data.ReadString(0x100);
			if(msg.StartsWith("!") || msg.StartsWith("%"))
				return OnChatCommand(client, msg.Substring(1));
			switch(type)
			{
				case CHATMESSAGETYPE.SAY:
				case CHATMESSAGETYPE.YELL:
				case CHATMESSAGETYPE.EMOTE:
					return false; // let worldserver handle it
				case CHATMESSAGETYPE.WHISPER:
				{
					DataObject[] objs = DataServer.Database.SelectObjects(typeof(DBCharacter), "Name = '" + target + "'");
					if(objs.Length == 0)
					{
						Chat.System(client, "No such player.");
						return true;
					}
					LoginClient targetClient = LoginServer.GetLoginClientByCharacterID(objs[0].ObjectId);
					if(targetClient == null || targetClient.Character == null)
					{
						Chat.System(client, "That player is not online.");
						return true;
					}
					Chat.Whisper(client, targetClient, msg);
					break;
				}
				default:
					Chat.System(client, "Received " + type + ": " + msg);
					break;
			}
			return true;
		}
	}

	public class Chat
	{
		public static void Whisper(LoginClient from, LoginClient to, string msg)
		{
			BinWriter pkg = LoginClient.NewPacket(SMSG.MESSAGECHAT);
			pkg.Write((byte)CHATMESSAGETYPE.WHISPER);
			pkg.Write((int)0); // language
			pkg.Write((ulong)from.Character.ObjectId); // guid
			pkg.Write(msg);
			pkg.Write((byte)0); // status flags 1: afk, 2: dnd, 3: gm
			to.Send(pkg);

			pkg = LoginClient.NewPacket(SMSG.MESSAGECHAT);
			pkg.Write((byte)CHATMESSAGETYPE.WHISPER_INFORM);
			pkg.Write((int)0);
			pkg.Write((ulong)to.Character.ObjectId);
			pkg.Write(msg);
			pkg.Write((byte)0);
			from.Send(pkg);
		}

		public static void System(string msg)
		{
			BinWriter pkg = LoginClient.NewPacket(SMSG.MESSAGECHAT);
			pkg.Write((byte)CHATMESSAGETYPE.SYSTEM);
			pkg.Write((int)0);
			pkg.Write((ulong)0);
			pkg.Write(msg);
			pkg.Write((byte)0);
			LoginServer.BroadcastPacket(pkg);
		}

		public static void System(LoginClient client, string msg)
		{
			BinWriter pkg = LoginClient.NewPacket(SMSG.MESSAGECHAT);
			pkg.Write((byte)CHATMESSAGETYPE.SYSTEM);
			pkg.Write((int)0);
			pkg.Write((ulong)0);
			pkg.Write(msg);
			pkg.Write((byte)0);
			client.Send(pkg);
		}
	}
}
