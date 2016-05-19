using System;
using WoWDaemon.Database.DataTables;
using WoWDaemon.World;
namespace WorldScripts.Living
{
	public class GamePlayerSaveEvent : WorldEvent
	{
		GamePlayer m_player;
		public GamePlayerSaveEvent(GamePlayer player) : base(TimeSpan.FromMinutes(1.0))
		{
			m_player = player;
		}

		public override void FireEvent()
		{
			m_player.Save();
			Chat.System(m_player, "You have been saved.");
			eventTime = DateTime.Now.Add(TimeSpan.FromMinutes(15.0));
			EventManager.AddEvent(this);
		}

	}
	/// <summary>
	/// Summary description for GamePlayer.
	/// </summary>
	public class GamePlayer : PlayerObject
	{
		GamePlayerSaveEvent saveEvent;
		public GamePlayer(DBCharacter c) : base(c)
		{
			saveEvent = new GamePlayerSaveEvent(this);
			EventManager.AddEvent(saveEvent);
		}

		public override void SaveAndRemove()
		{
			base.SaveAndRemove ();
			EventManager.RemoveEvent(saveEvent);
		}

	}
}
