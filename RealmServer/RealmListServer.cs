using System;
using System.Net;
using System.Net.Sockets;
using System.Collections;
using System.Threading;
using WoWDaemon.Common;

namespace WoWDaemon.Realm
{
	public class RealmWarning : Exception
	{
		public RealmWarning(string msg) : base(msg)
		{
		}
	}


	public class RealmListServer : ServerBase
	{
		public static readonly RealmListServer Instance = new RealmListServer();
		Socket m_realmUpdateListener = null;
		Hashtable m_updateClients = new Hashtable();
		public string UpdaterPassword = string.Empty;
		private RealmListServer()
		{
		}

		public bool Start(IPEndPoint iep, int updaterPort)
		{
			m_realmUpdateListener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			try
			{
				m_realmUpdateListener.Bind(new IPEndPoint(iep.Address, updaterPort));
				m_realmUpdateListener.Listen(5);
				m_realmUpdateListener.Blocking = false;
			}
			catch(SocketException)
			{
				return false;
			}
			return base.Start (iep);
		}

		public override void Stop()
		{
			m_realmUpdateListener.Close();
			if(m_updateClients.Count > 0)
			{
				IEnumerator e = new ArrayList(m_updateClients.Values).GetEnumerator();
				while(e.MoveNext())
					((ClientBase)e.Current).Close("Server shutting down.");
				m_updateClients.Clear();
			}
			realms.Clear();
			base.Stop ();
		}

		public override void OnClientData(ClientBase aClient, byte[] data)
		{
			RealmListClient client = aClient as RealmListClient;
			client.LastActivity = DateTime.Now;
			BinReader read = new BinReader(data);
			REALMLISTOPCODE opCode = (REALMLISTOPCODE)read.ReadByte();
			switch(opCode)
			{
				case REALMLISTOPCODE.CHALLENGE:
				case REALMLISTOPCODE.RECODE_CHALLENGE:
				{
					if(opCode == REALMLISTOPCODE.CHALLENGE)
						client.Send(patch_challenge);
					else
						client.Send(realm_challenge);
					break;
				}
				case REALMLISTOPCODE.PROOF:
				case REALMLISTOPCODE.RECODE_PROOF:
				{
					client.Send(realm_proof);
					break;
				}
				case REALMLISTOPCODE.REALMLIST_REQUEST:
				{
					client.Send(realmList);
					client.Close("Done");
					break;
				}
			}
		}

		public override bool isBanned(IPAddress address)
		{
			return false;
		}

		public override void OnAcceptSocket(Socket sock)
		{
			AddClient(new RealmListClient(sock));
		}

		DateTime LastActivityCheck = DateTime.Now;
		bool DoActivityCheck = false;
		public override void OnClientLoopStart()
		{
			TimeSpan span = DateTime.Now.Subtract(LastActivityCheck);
			if(span.TotalSeconds > 30)
				DoActivityCheck = true;
		}

		public override void OnClientLoop(ClientBase aClient)
		{
			if(DoActivityCheck)
				if(aClient.Timedout)
				{
					aClient.Close("Timed out");
					RemoveClient(aClient);
				}
		}

		public override void OnClientLoopStop()
		{
			if(DoActivityCheck)
				DoActivityCheck = false;
			realmUpdaterLoop();
			TimeSpan span = DateTime.Now.Subtract(LastRealmListUpdate);
			if(span.TotalSeconds > 30)
				UpdateList();
			Thread.Sleep(50);
		}

		#region RealmUpdater
		public void AddRealmUpdateClient(ClientBase client)
		{
			m_updateClients.Add(client.RemoteEndPoint.ToString(), client);
		}

		public void RemoveRealmUpdateClient(ClientBase client)
		{
			m_updateClients.Remove(client.RemoteEndPoint.ToString());
			RemoveRealm(client.RemoteEndPoint.ToString());
		}

		private void HandleRealmUpdateData(ClientBase client, byte[] data)
		{
			BinReader read = new BinReader(data);
			read.BaseStream.Position += 4;
			string pass = read.ReadString();
			if(pass != UpdaterPassword)
			{
				client.Close("Wrong updater password");
				return;
			}
			UpdateRealm(client.RemoteEndPoint.ToString(), read.ReadString(), read.ReadString(), read.ReadInt32());
		}

		#region RealmUpdaterLoop
		private Socket acceptRealmUpdate()
		{
			try
			{
				return m_realmUpdateListener.Accept();
			}
			catch(Exception)
			{
				return null;
			}
		}
		private void realmUpdaterLoop()
		{
			Socket sock = null;
			while((sock = acceptRealmUpdate()) != null)
			{
				if(isBanned(((IPEndPoint)sock.RemoteEndPoint).Address))
				{
					sock.Close();
					continue;
				}
				AddRealmUpdateClient(new ClientBase(sock));
			}
			ArrayList list = new ArrayList(m_updateClients.Values);
			IEnumerator e = list.GetEnumerator();
			byte[] data;
			while(e.MoveNext())
			{
				ClientBase client = (ClientBase)e.Current;
				if(client.PendingSendData)
					client.SendWork();
				if(client.Connected == false)
				{
					RemoveRealmUpdateClient(client);
					continue;
				}
				while((data = client.GetNextPacketData()) != null)
					HandleRealmUpdateData(client, data);
				if(client.Connected == false)
				{
					RemoveRealmUpdateClient(client);
				}
			}
		}
		#endregion


		struct RealmInfo
		{
			public int Users;
			public string Description;
			public string IP;
			public DateTime LastUpdate;
			public RealmInfo(string desc, string ip, int users)
			{
				Users = users;
				Description = desc;
				IP = ip;
				LastUpdate = DateTime.Now;
			}
		}

		byte[] realmList = {0x10, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
		Hashtable realms = new Hashtable();
		bool realmListDirty = true;
		private void UpdateRealm(string UpdaterIP, string Description, string IP, int Users)
		{
			realms[UpdaterIP] = new RealmInfo(Description, IP, Users);
			realmListDirty = true;
			UpdateList();
		}

		private void RemoveRealm(string UpdaterIP)
		{
			if(realms.Contains(UpdaterIP) == false)
				return;
			realms.Remove(UpdaterIP);
			realmListDirty = true;
			UpdateList();
		}

		DateTime LastRealmListUpdate = DateTime.MinValue;
		#region UpdateList
		private void UpdateList()
		{
			LastRealmListUpdate = DateTime.Now;
			if(realms.Count > 0)
			{
				ArrayList list = new ArrayList(realms.Values);
				DateTime now = DateTime.Now;
				TimeSpan span;
				foreach(RealmInfo info in list)
				{
					span = now.Subtract(info.LastUpdate);
					if(span.TotalSeconds > 30)
					{
						realms.Remove(info.IP);
						realmListDirty = true;
					}
				}
			}

			if(realmListDirty)
			{
				realmListDirty = false;
				BinWriter w = new BinWriter();
				w.Write((byte)0x10);
				w.Write((short)0);
				w.Write(0);
				w.Write((byte)realms.Count);
				if(realms.Count > 0)
				{
					IEnumerator e = realms.Values.GetEnumerator();
					while(e.MoveNext())
					{
						RealmInfo info = (RealmInfo)e.Current;
						w.Write(info.Description);
						w.Write(info.IP);
						w.Write(info.Users);
					}
					w.Set(1, (short)(w.BaseStream.Length-3));
					byte[] newList = new byte[w.BaseStream.Length];
					Array.Copy(w.GetBuffer(), 0, newList, 0, newList.Length);
					realmList = newList;
				}
			}
		}
		#endregion

		#endregion

		#region STATIC PACKET DATA
		static byte[] patch_challenge =
			{
				0x00,
				0x00,
				0x00,
				0xD7, 0xB9, 0x1A, 0x0B, 0x09, 0x39, 0x28, 0x45,
				0x48, 0xAE, 0x31, 0x9A, 0x3B, 0x85, 0x7A, 0xF4,
				0xFF, 0x79, 0x21, 0x58, 0xE6, 0x16, 0x5B, 0x35,
				0x21, 0x4C, 0xCE, 0x4B, 0x86, 0xF8, 0x41, 0x60,

				0x01,
				0x07,

				0x20,
				0x89,0x4B,0x64,0x5E,0x89,0xE1,0x53,0x5B,
				0xBD,0xAD,0x5B,0x8B,0x29,0x06,0x50,0x53,
				0x08,0x01,0xB1,0x8E,0xBF,0xBF,0x5E,0x8F,
				0xAB,0x3C,0x82,0x87,0x2A,0x3E,0x9B,0xB7,

				0xF4,0x3C,0xAA,0x7B,0x24,0x39,0x81,0x44,
				0xBF,0xA5,0xB5,0x0C,0x0E,0x07,0x8C,0x41,
				0x03,0x04,0x5B,0x6E,0x57,0x5F,0x37,0x87,
				0x31,0x9F,0xC4,0xF8,0x0D,0x35,0x94,0x29,

				0x2A,0xD5,0x48,0xCC,0x9B,0x9D,0xA1,0x99,
				0xCC,0x04,0x7A,0x60,0x91,0x15,0x6C,0x51
			};
		static byte[] realm_proof = {0x03,0x00};

		static byte[] realm_challenge = {0x02, 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 
											0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10,
											0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 
											0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10};
		#endregion
	}
}
