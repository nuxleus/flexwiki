#region License Statement
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
// The use and distribution terms for this software are covered by the 
// Common Public License 1.0 (http://opensource.org/licenses/cpl.php)
// which can be found in the file CPL.TXT at the root of this distribution.
// By using this software in any fashion, you are agreeing to be bound by 
// the terms of this license.
//
// You must not remove this notice, or any other, from this software.
#endregion

using System;
using System.Data;
using System.IO;

namespace FlexWikiSecurity
{
	/// <summary>
	/// This provider persists the user information in an XML file.
	/// </summary>
	public class SimpleUserDataProvider : IUserDataProvider
	{
		string xmlFileName = "Users.config";
		/// <summary>
		/// Default Constructor
		/// </summary>
		public SimpleUserDataProvider()
		{

		}
		/// <summary>
		/// File Name location of the users data
		/// </summary>
		/// <param name="connection"></param>
		public void SetConnection(string connection)
		{
			xmlFileName = connection;
		}
		/// <summary>
		/// Get All users for this DataProvider
		/// </summary>
		/// <returns>User DataSet</returns>
		public DataSet GetUsers()
		{
			DataSet dataSet = LoadUserDataSet();
			return dataSet;
		}
		/// <summary>
		/// Get a Specific User by ID
		/// </summary>
		/// <param name="userID">ID of User</param>
		/// <returns>User DataSet</returns>
		public DataSet GetUser(long userID)
		{
			DataSet dataSet = LoadUserDataSet();
			// filter the data by userID and return the result
			DataRow[] rows = dataSet.Tables["Users"].Select("UserID<>" + userID );
			foreach (DataRow row in rows)
			{
				row.Delete();
			}
			dataSet.AcceptChanges();
			return dataSet;
		}
		/// <summary>
		/// Get a specific User by Email
		/// </summary>
		/// <param name="email">Email</param>
		/// <returns>User DataSet</returns>
		public DataSet GetUser(string email)
		{
			DataSet dataSet = LoadUserDataSet();
			// filter the data by email and return the result
			// filter the data by userID and return the result
			DataRow[] rows = dataSet.Tables["Users"].Select("UserEmail<>'" + email +"'" );
			foreach (DataRow row in rows)
			{
				row.Delete();
			}
			dataSet.AcceptChanges();
			return dataSet;
		}
		/// <summary>
		/// Add a User 
		/// </summary>
		/// <param name="name">Full Name</param>
		/// <param name="email">Email</param>
		/// <param name="password">Password</param>
		/// <returns>The unique ID of the user added</returns>
		public long AddUser(string name,string email,string password)
		{
			int maxID=0;
			DataSet dataSet = LoadUserDataSet();

			// Verify the user does not already exist
			DataRow[] rows = dataSet.Tables["Users"].Select("UserEmail='" + email + "'");
			if (rows.Length==0)
			{
				// Find the Max Id to return
				DataRow row;
				for(int x=0;x<dataSet.Tables["Users"].Rows.Count;x++)
				{
					row = dataSet.Tables["Users"].Rows[x];
					if (maxID<(int)row["UserID"])
					{
						maxID=(int)row["UserID"];
					}
				}
				if (maxID==0)
				{
					maxID=1;
				}
				// Add the user to the DataSet
				row = dataSet.Tables["Users"].NewRow();
				row["UserEmail"] = email;
				row["UserPassword"] = password;
				row["UserName"] = name;
				row["UserID"] = maxID;
				dataSet.Tables["Users"].Rows.Add(row);

				// Save the DataSet
				dataSet.WriteXml(xmlFileName,System.Data.XmlWriteMode.WriteSchema);
			}
			else
			{
				return 0;
			}

			return maxID;
		}
		/// <summary>
		/// Persist the User 
		/// </summary>
		/// <param name="userID">ID of User to persist</param>
		/// <param name="name">Full Name</param>
		/// <param name="email">Email</param>
		/// <param name="password">Password</param>
		/// <returns>The number of rows affected</returns>
		public int UpdateUser(long userID,string name,string email,string password)
		{
			DataSet dataSet = LoadUserDataSet();

			// Verify the user does exist
			DataRow[] rows = dataSet.Tables["Users"].Select("UserID=" + userID);
			if (rows.Length==1)
			{
				DataRow row = rows[0];
				row["UserEmail"] = email;
				row["UserPassword"] = password;
				row["UserName"] = name;

				// Save the DataSet
				dataSet.WriteXml(xmlFileName,System.Data.XmlWriteMode.WriteSchema);
				return 1;
			}
			return 0;
		}
		/// <summary>
		/// Deletes a specific user
		/// </summary>
		/// <param name="userID">ID of User</param>
		/// <returns>The number of rows affected</returns>
		public int DeleteUser(long userID)
		{

			DataSet dataSet = LoadUserDataSet();
			// Verify the user does exist
			DataRow[] rows = dataSet.Tables["Users"].Select("UserID=" + userID );
			if (rows.Length==1)
			{
				// Delete the user from the DataSet
				rows[0].Delete();

				// Save the DataSet
				dataSet.WriteXml(xmlFileName,System.Data.XmlWriteMode.WriteSchema);
				return 1;
			}

			return 0;
		}
		private DataSet LoadUserDataSet()
		{
			DataSet dataSet = new DataSet("Users");
			// If the dataset won't load then create a new one
			if (File.Exists(xmlFileName))
			{
				dataSet.ReadXml(xmlFileName,System.Data.XmlReadMode.ReadSchema);
			}
			else
			{
				DataTable dataTable = new DataTable("Users");
				DataColumn userIDColumn = new DataColumn("UserID",System.Type.GetType("System.Int32"));
				DataColumn userEmailColumn = new DataColumn("UserEmail",System.Type.GetType("System.String"));
				DataColumn userPasswordColumn = new DataColumn("UserPassword",System.Type.GetType("System.String"));
				DataColumn userNameColumn = new DataColumn("UserName",System.Type.GetType("System.String"));
				dataTable.Columns.Add(userIDColumn);
				dataTable.Columns.Add(userEmailColumn);
				dataTable.Columns.Add(userPasswordColumn);
				dataSet.Tables.Add(dataTable);
			}
			return dataSet;
		}
	}
}
