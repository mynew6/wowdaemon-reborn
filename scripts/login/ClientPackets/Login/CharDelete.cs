using System;
using WoWDaemon.Common;
using WoWDaemon.Common.Attributes;
using WoWDaemon.Login;
using WoWDaemon.Database.DataTables;

namespace LoginScripts.ClientPackets
{
	/// <summary>
	/// Summary description for CharDelete.
	/// </summary>
	[LoginPacketHandler()]
	public class CharDelete
	{
		[LoginPacketDelegate(CMSG.CHAR_DELETE)]
		static bool HandleCharDelete(LoginClient client, CMSG msgID, BinReader data)
		{
			uint id = data.ReadUInt32();
			if(client.Account.Characters == null)
			{
				client.Close(client.Account.Name + " tried to delete a character when there was none on the account.");
				return true;
			}
			foreach(DBCharacter c in client.Account.Characters)
			{
				if(id == c.ObjectId)
				{
					try
					{
						DataServer.Database.DeleteObject(c);
					}
					catch(Exception e)
					{
						Console.WriteLine("Deleting character " + c.ObjectId + " failed! " + e.Message);
					}
					client.Account.Characters = null;
					DataServer.Database.FillObjectRelations(client.Account);
					BinWriter w = LoginClient.NewPacket(SMSG.CHAR_DELETE);
					w.Write((byte)0x28);
					client.Send(w);
					return true;
				}
			}
			client.Close(client.Account.Name + " tried to delete a character that didn't belong to him.");
			return true;
		}
	}
}
