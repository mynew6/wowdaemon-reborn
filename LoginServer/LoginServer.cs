using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;
using WoWDaemon.Common;
using WoWDaemon.Database;
using WoWDaemon.Database.Connection;
using WoWDaemon.Database.DataTables;
namespace WoWDaemon.Login
{
	public class DataServer
	{
		public static readonly ObjectDatabase Database = new ObjectDatabase(new DataConnection(ConnectionType.DATABASE_XML, "xml_db"));
		static DataServer()
		{
			Database.RegisterDataObject(typeof(DBAccount));
			Database.RegisterDataObject(typeof(DBCharacter));
			Database.RegisterDataObject(typeof(DBCreature));
			Database.RegisterDataObject(typeof(DBItem));
			Database.RegisterDataObject(typeof(DBItemTemplate));
			Database.RegisterDataObject(typeof(DBSpawn));
			Database.RegisterDataObject(typeof(DBWorldMap));

			Database.RegisterDataObject(typeof(DBSpell));
			Database.RegisterDataObject(typeof(DBKnownSpell));
		}
	}

	public class LoginServer : ServerBase
	{
		static LoginServer Instance = new LoginServer();
		static string m_serverName = "Noname";
		public static string ServerName
		{
			get { return m_serverName;}
			set { m_serverName = value;}
		}

		internal static LoginScriptAssembly Scripts = new LoginScriptAssembly();

		static LoginServer()
		{
			LoginPacketManager.SearchAssemblyForHandlers(System.Reflection.Assembly.GetExecutingAssembly());
		}

		static int m_maxUsers = 100;
		static int m_topUsers = 0;
		static Hashtable m_loginCharacters = new Hashtable();
		static ArrayList m_worldServers = new ArrayList();
		static HybridDictionary m_worldMapServer = new HybridDictionary();
		static ScriptManager m_scriptManager = ScriptManager.GetScriptManager();
		static string m_scriptPath = string.Empty;

		public static void AddScriptReference(string module)
		{
			m_scriptManager.AddReference(module);
		}

		public static bool LoadScripts(string path)
		{
			m_scriptPath = path;
			string error;
			if(m_scriptManager.LoadScripts(typeof(LoginScriptAssembly), true, path, out error) == false)
			{
				Console.WriteLine("Loading login scripts failed. " + error);
				return false;
			}
			return true;
		}

		public static void ReloadScripts()
		{
			string error;
			m_scriptManager.UnloadAllScripts();
			if(m_scriptManager.LoadScripts(typeof(LoginScriptAssembly), true, m_scriptPath, out error) == false)
			{
				Console.WriteLine("Reloading login scripts failed. " + error);
			}
		}

		public static int MaxUsers
		{
			get { return m_maxUsers;}
			set { m_maxUsers = value;}
		}

		public static int TopUsers
		{
			get { return m_topUsers;}
		}

		public static int CurrentUsers
		{
			get { return Instance.ClientCount;}
		}

		public static IPEndPoint EndPoint
		{
			get { return Instance.LocalEndPoint;}
		}

		public static void SetWorldServer(ClientBase worldConnection, uint[] worldMapIDs)
		{
			DBWorldMap[] worldMaps = new DBWorldMap[worldMapIDs.Length];
			for(int i = 0;i < worldMapIDs.Length;i++)
			{
				worldMaps[i] = (DBWorldMap)DataServer.Database.FindObjectByKey(typeof(DBWorldMap), worldMapIDs[i]);
				if(worldMaps[i] == null)
					throw new Exception("Missing worldmap " + worldMapIDs[i]);
				if(m_worldMapServer.Contains(worldMaps[i].ObjectId))
					throw new Exception("There's already a worldserver handling worldmap " + worldMaps[i].ObjectId);
			}
			WorldConnection server = new WorldConnection(worldConnection, worldMaps);
			foreach(DBWorldMap map in worldMaps)
				m_worldMapServer[map.ObjectId] = server;
			m_worldServers.Add(server);
		}

		public static bool Start(IPEndPoint iep, int redirectPort)
		{
			return Instance.Start (iep) && RedirectServer.Start(new IPEndPoint(iep.Address, redirectPort));
		}

		public static void Shutdown()
		{
			RedirectServer.Stop();
			Instance.Stop();
		}


		public static void BroadcastPacket(BinWriter pkg)
		{
			IEnumerator e = Instance.Clients.GetEnumerator();
			while(e.MoveNext())
				((LoginClient)e.Current).Send(pkg);
		}

        public static void SendWhoList(LoginClient whoClient)
        {
            BinWriter pkg = LoginClient.NewPacket(SMSG.WHO);
            pkg.Write((int)Instance.ClientCount);
            pkg.Write((int)Instance.ClientCount);
            IEnumerator e = Instance.Clients.GetEnumerator();
            //			int Group = 0; // 0 = No, 1 = Yes
            while (e.MoveNext())
            {
                try
                {
                    pkg.Write((string)((LoginClient)e.Current).Character.Name);
                    //pkg.Write((string)((LoginClient)e.Current).Character.GuildName); //temp disabled
                    pkg.Write((int)((LoginClient)e.Current).Character.Level);
                    pkg.Write((int)((LoginClient)e.Current).Character.Class);
                    pkg.Write((int)((LoginClient)e.Current).Character.Race);
                    pkg.Write((int)((LoginClient)e.Current).Character.Zone);
                    //pkg.Write((int)((LoginClient)e.Current).Character.GroupLook); //temp disabled
                }
                catch (Exception)
                {
                }
            }
            whoClient.Send(pkg);
        }

		public static LoginClient GetLoginClientByCharacterID(uint id)
		{
			return (LoginClient)m_loginCharacters[id];
		}

		public static void PlayerLogin(LoginClient client, uint id)
		{
			if(client.Account == null)
			{
				client.Close("Tried to login with a character without an account.");
				return;
			}
			DataObject[] chars = DataServer.Database.SelectObjects(typeof(DBCharacter), "Character_ID = '" + id + "'");
			if(chars.Length == 0)
			{
				client.Close("Failed to find character in database?");
				return;
			}
			DBCharacter c = (DBCharacter)chars[0];
			if(c.AccountID != client.Account.ObjectId)
			{
				client.Close("Tried to login another account's character.");
				return;
			}
			client.Character = c;
			if(c.WorldMapID == 0)
			{
				client.Close(c.Name + " is missing world map id.");
				return;
			}
			client.WorldConnection = (WorldConnection)m_worldMapServer[c.WorldMapID];
			if(client.WorldConnection == null)
			{
				client.Close("Missing worldserver for world map id " + c.WorldMapID);
				return;
			}
			m_loginCharacters[id] = client;
			client.OnEnterWorld();
		}

		internal static void ChangeMap(LoginClient client)
		{
			client.WorldConnection = (WorldConnection)m_worldMapServer[client.Character.WorldMapID];
			if(client.WorldConnection == null)
			{
				client.Close("(ChangeMap) Missing worldserver for world map id " + client.Character.WorldMapID);
				return;
			}

			DBWorldMap map = (DBWorldMap)DataServer.Database.FindObjectByKey(typeof(DBWorldMap), client.Character.WorldMapID);
			client.Character.Continent = (uint)map.Continent;

			BinWriter pkg = LoginClient.NewPacket(SMSG.NEW_WORLD);
			pkg.Write((byte)client.Character.Continent);
			pkg.WriteVector(client.Character.Position);
			pkg.Write(client.Character.Facing);
			client.Send(pkg);

			client.WorldConnection.OnEnterWorld(client.Character);
		}

		internal static void LeaveWorld(LoginClient client)
		{
			if(client.Character != null)
			{
				WorldPacket pkg = new WorldPacket(WORLDMSG.PLAYER_LEAVE_WORLD);
				pkg.Write(client.Character.ObjectId);
				client.WorldConnection.Send(pkg);
			}
		}

		internal static void RemoveCharacter(LoginClient client)
		{
			if(client.Character != null)
			{
				m_loginCharacters.Remove(client.Character.ObjectId);
				client.Character = null;
			}
		}


		bool m_shutdown = false;
		private LoginServer()
		{
		}

		public override void Stop()
		{	
			m_shutdown = true;
			WorldPacket pkg = new WorldPacket(WORLDMSG.WORLD_SHUTDOWN);
			IEnumerator e = m_worldServers.GetEnumerator();
			while(e.MoveNext())
				((WorldConnection)e.Current).Send(pkg);
		}

		public override bool isBanned(IPAddress address)
		{
			return false;
		}

		public override void OnAcceptSocket(Socket sock)
		{
			if(m_shutdown || m_clients.Count >= MaxUsers)
			{
				sock.Close();
				return;
			}
			AddClient(new LoginClient(sock));
			if(m_clients.Count > m_topUsers)
				m_topUsers = m_clients.Count;
		}

		public override void OnClientData(ClientBase aClient, byte[] data)
		{
			LoginClient client = aClient as LoginClient;
			BinReader read = new BinReader(data);
			read.BaseStream.Position += 2; // skip len
			CMSG msgID = (CMSG)read.ReadInt32();
			if(!LoginPacketManager.HandlePacket(client, msgID, read))
			{
				if(client.WorldConnection != null)
				{
					ClientPacket pkg = new ClientPacket(msgID, client.Character.ObjectId, data, 6, data.Length-6);
					client.SendWorldServer(pkg);
				}
			}
		}

		public override void OnClientLoopStart()
		{

		}

		public override void OnClientLoop(ClientBase aClient)
		{
			/*LoginClient client = aClient as LoginClient;*/
		}


		public override void OnClientLoopStop()
		{
			IEnumerator e = new ArrayList(m_worldServers).GetEnumerator();
			while(e.MoveNext())
			{
				WorldConnection connection = (WorldConnection)e.Current;
				if(!connection.processWorldServerData())
				{
					if(m_shutdown == false)
					{
						Console.WriteLine("Lost connection to world server " + connection.ToString());
						LoginServer.Shutdown();
					}
					else
					{
						m_worldServers.Remove(connection);
					}
				}
			}

			if(m_shutdown && m_worldServers.Count == 0)
			{
				base.Stop();
			}

			Thread.Sleep(5);
		}
	}
}
