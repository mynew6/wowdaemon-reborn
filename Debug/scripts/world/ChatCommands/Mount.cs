using System;
using WoWDaemon.Common.Attributes;
using WoWDaemon.World;
namespace WorldScripts.ChatCommands
{
	/// <summary>
	/// Summary description for Mount.
	/// </summary>
	[ChatCmdHandler()]
	public class Mount
	{
		[ChatCmdAttribute("mount", "No usage.")]
		static bool OnMount(WorldClient client, string input)
		{
			if(client.Player.MountDisplayID != 0)
			{
				Chat.System(client, "Please '!dismount' first.");
				return true;
			}
			client.Player.MountDisplayID = 0x00E5;
			client.Player.Flags |= 0x3000;
			client.Player.UpdateData();
			Chat.System(client, "You mount your horsey. Giddaap! Wooppaaa!");
			return true;
		}

		[ChatCmdAttribute("dismount", "No usage.")]
		[ChatCmdAttribute("unmount", "No usage.")]
		static bool OnDismount(WorldClient client, string input)
		{
			if(client.Player.MountDisplayID == 0)
			{
				Chat.System(client, "You dismount thin air.");
				return true;
			}
			client.Player.MountDisplayID = 0;
			client.Player.Flags &= ~(uint)0x3000;
			client.Player.UpdateData();
			return true;
		}

		
	}
}
