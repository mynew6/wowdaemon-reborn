using System;
using System.Collections;
using WoWDaemon.Common;
using WoWDaemon.World;
using WoWDaemon.Database.DataTables;
namespace WorldScripts.Living
{
	public class GetWalkNodeEvent : WorldEvent
	{
		MonsterBase m_monster;
		Vector m_position;
		static Random random = new Random();
		public GetWalkNodeEvent(MonsterBase monster) : base(TimeSpan.FromSeconds(0.1))
		{
			m_monster = monster;
			m_position = new Vector(monster.Position.X, monster.Position.Y, monster.Position.Z);
		}
		
		ArrayList nodes = new ArrayList();
		void AddNode(Vector position)
		{
			for(int i = 0;i < nodes.Count;i++)
			{
				Vector v = (Vector)nodes[i];
				if(v.X == position.X &&
					v.Y == position.Y &&
					v.Z == position.Z)
					return;
			}
			nodes.Add(new Vector(position.X, position.Y, position.Z));
		}
		int NumNodes
		{
			get { return nodes.Count;}
		}

		public override void FireEvent()
		{
			if(m_monster.MapTile == null)
			{
				m_monster = null;
				m_position = null;
				return;
			}
			ArrayList list = m_monster.MapTile.Map.GetObjectsInRange(OBJECTTYPE.PLAYER, m_position, 25.0f);
			if(list.Count > 0)
			{
				PlayerObject plr = (PlayerObject)list[random.Next(list.Count)];
				AddNode(plr.Position);
				if(NumNodes == 10)
				{
					EventManager.AddEvent(new RandomWalkEvent(m_monster, nodes));
					nodes = null;
					m_position = null;
					m_monster = null;
					return;
				}
			}
			eventTime = DateTime.Now.Add(TimeSpan.FromSeconds(0.5));
			EventManager.AddEvent(this);
		}
	}

	public class RandomWalkEvent : WorldEvent
	{
		MonsterBase m_monster;
		ArrayList m_nodes;
		static Random random = new Random();
		public RandomWalkEvent(MonsterBase monster, ArrayList nodes) : base(TimeSpan.FromMilliseconds(1))
		{
			m_monster = monster;
			m_nodes = nodes;
		}

		void WalkToVector(Vector v, int time)
		{
			ServerPacket pkg = new ServerPacket(SMSG.MONSTER_MOVE);
			pkg.Write(m_monster.GUID);
			pkg.WriteVector(m_monster.Position);
			pkg.Write(m_monster.Facing);
			pkg.Write((byte)0);
			pkg.Write(0x000);
			pkg.Write(time);
			pkg.Write(1);
			pkg.WriteVector(v);
			pkg.Finish();
			m_monster.MapTile.SendSurrounding(pkg);
		}

		public override void FireEvent()
		{
			if(m_monster.MapTile == null)
				return;
			Vector v = (Vector)m_nodes[random.Next(m_nodes.Count)];
			float distance = m_monster.Position.Distance(v);
			int time = (int)((distance/m_monster.WalkSpeed)*1000);
			WalkToVector(v, time);
			m_monster.Facing = m_monster.Position.Angle(v);
			m_monster.Position = v;
			MapManager.Move(m_monster);
			eventTime = DateTime.Now.Add(TimeSpan.FromMilliseconds(time + 5000 + random.Next(6)*1000));
			EventManager.AddEvent(this);
		}

	}


	public class MonsterBase : UnitBase
	{
		public MonsterBase(DBCreature creature) : base(creature)
		{
			MaxHealth = Health = 100;
			MaxPower = Power = 0;
			PowerType = POWERTYPE.RAGE;
			Level = 1;
			Faction = 0;
			DisplayID = 508;
		}

		public void StartGetNodes()
		{
			EventManager.AddEvent(new GetWalkNodeEvent(this));
		}
	}
}
