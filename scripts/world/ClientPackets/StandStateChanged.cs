using System;
using WoWDaemon.Common;
using WoWDaemon.Common.Attributes;
using WoWDaemon.World;

namespace WorldScripts.ClientPackets
{
	/// <summary>
	/// Summary description for TextEmote.
	/// </summary>
	[WorldPacketHandler]
	public class StandStateChanged
	{
		[WorldPacketDelegate(CMSG.STANDSTATECHANGED)]
		static void OnStandStateChanged(WorldClient client, CMSG msgID, BinReader data)
		{
			if(client.Player.MountDisplayID != 0)
				return;
			client.Player.StandState = (UNITSTANDSTATE)data.ReadByte();
			client.Player.UpdateData();
		}
	}
}
