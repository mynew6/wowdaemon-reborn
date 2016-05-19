using System;
using WoWDaemon.Database;
using WoWDaemon.Common.Attributes;

namespace WoWDaemon.Database.DataTables
{
	[DataTable(TableName="Account")]
	public class DBAccount : DBObject
	{
		public DBAccount()
		{
		}

		[PrimaryKey(Name="Name")]
		private string m_name = null;
		[DataElement(Name="CreationDate")]
		private DateTime m_creationDate = DateTime.Now;

		public string Name
		{
			get {return m_name;}
			set {Dirty = true; m_name = value;}
		}

		public DateTime CreationDate
		{
			get {return m_creationDate;}
			set {Dirty = true; m_creationDate = value;}
		}

		static bool m_autosave = true;
		public override bool AutoSave
		{
			get {return m_autosave;}
			set {m_autosave = value;}
		}

		[Relation(LocalField="ObjectId", RemoteField="AccountID", AutoLoad=true, AutoDelete=true)]
		public DBCharacter[] Characters = null;
	}
}
