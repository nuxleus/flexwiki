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
using System.Diagnostics;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Data;

namespace SqlStoreDatabaseCreationTool
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form
	{
		string databaseName = "FlexWikiSqlStore";
		
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox sqlServerInstanceName;
		private System.Windows.Forms.TextBox databaseLocationTextBox;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.Button folderBroweButton;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Button createDatabaseButton;
		private System.Windows.Forms.Button exitButton;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.TextBox userNameTextBox;
		private System.Windows.Forms.Label label5;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.sqlServerInstanceName = new System.Windows.Forms.TextBox();
			this.databaseLocationTextBox = new System.Windows.Forms.TextBox();
			this.label2 = new System.Windows.Forms.Label();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.folderBroweButton = new System.Windows.Forms.Button();
			this.label3 = new System.Windows.Forms.Label();
			this.createDatabaseButton = new System.Windows.Forms.Button();
			this.exitButton = new System.Windows.Forms.Button();
			this.userNameTextBox = new System.Windows.Forms.TextBox();
			this.label4 = new System.Windows.Forms.Label();
			this.label5 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(56, 23);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(152, 23);
			this.label1.TabIndex = 0;
			this.label1.Text = "Sql Server Instance Name";
			// 
			// sqlServerInstanceName
			// 
			this.sqlServerInstanceName.Location = new System.Drawing.Point(240, 24);
			this.sqlServerInstanceName.Name = "sqlServerInstanceName";
			this.sqlServerInstanceName.Size = new System.Drawing.Size(144, 20);
			this.sqlServerInstanceName.TabIndex = 1;
			this.sqlServerInstanceName.Text = Environment.MachineName;
			// 
			// databaseLocationTextBox
			// 
			this.databaseLocationTextBox.Location = new System.Drawing.Point(240, 57);
			this.databaseLocationTextBox.Name = "databaseLocationTextBox";
			this.databaseLocationTextBox.Size = new System.Drawing.Size(144, 20);
			this.databaseLocationTextBox.TabIndex = 1;
			this.databaseLocationTextBox.Text = "";
			// 
			// label2
			// 
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(32, 56);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(176, 23);
			this.label2.TabIndex = 0;
			this.label2.Text = "Database Physical File Location";
			// 
			// folderBroweButton
			// 
			this.folderBroweButton.Location = new System.Drawing.Point(408, 56);
			this.folderBroweButton.Name = "folderBroweButton";
			this.folderBroweButton.TabIndex = 2;
			this.folderBroweButton.Text = "Browse";
			this.folderBroweButton.Click += new System.EventHandler(this.FolderBroweButtonClick);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(400, 24);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(168, 23);
			this.label3.TabIndex = 0;
			this.label3.Text = "(machine name is default value)";
			// 
			// createDatabaseButton
			// 
			this.createDatabaseButton.Location = new System.Drawing.Point(112, 144);
			this.createDatabaseButton.Name = "createDatabaseButton";
			this.createDatabaseButton.Size = new System.Drawing.Size(120, 23);
			this.createDatabaseButton.TabIndex = 3;
			this.createDatabaseButton.Text = "Create Database";
			this.createDatabaseButton.Click += new System.EventHandler(this.CreateDatabaseButtonClick);
			// 
			// exitButton
			// 
			this.exitButton.Location = new System.Drawing.Point(264, 144);
			this.exitButton.Name = "exitButton";
			this.exitButton.Size = new System.Drawing.Size(120, 23);
			this.exitButton.TabIndex = 3;
			this.exitButton.Text = "Exit";
			this.exitButton.Click += new System.EventHandler(this.ExitButtonClick);
			// 
			// userNameTextBox
			// 
			this.userNameTextBox.Location = new System.Drawing.Point(240, 96);
			this.userNameTextBox.Name = "userNameTextBox";
			this.userNameTextBox.Size = new System.Drawing.Size(144, 20);
			this.userNameTextBox.TabIndex = 1;
			this.userNameTextBox.Text = Environment.MachineName + "\\aspnet";
			// 
			// label4
			// 
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label4.Location = new System.Drawing.Point(72, 96);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(136, 23);
			this.label4.TabIndex = 0;
			this.label4.Text = "Sql Server User Name";
			// 
			// label5
			// 
			this.label5.Location = new System.Drawing.Point(400, 96);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(176, 23);
			this.label5.TabIndex = 0;
			this.label5.Text = "(aspnet account is default value)";
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(616, 182);
			this.Controls.Add(this.createDatabaseButton);
			this.Controls.Add(this.folderBroweButton);
			this.Controls.Add(this.sqlServerInstanceName);
			this.Controls.Add(this.databaseLocationTextBox);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.label3);
			this.Controls.Add(this.exitButton);
			this.Controls.Add(this.userNameTextBox);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.label5);
			this.Name = "Form1";
			this.Text = "Sql Store Database Creation Form";
			this.ResumeLayout(false);

		}

		private void FolderBroweButtonClick(object sender, EventArgs e)
		{
			DialogResult result = this.folderBrowserDialog1.ShowDialog();
			if(this.folderBrowserDialog1.SelectedPath != null && this.folderBrowserDialog1.SelectedPath.Length != 0)
			{
				this.databaseLocationTextBox.Text = this.folderBrowserDialog1.SelectedPath;
			}
		}

		/// <summary>
		/// Creates the database by running the script. 
		/// The name of the database is a constant, but could be parameterized.
		/// </summary>
		private void CreateDatabase()
		{
			string createDatabaseStatement = "IF EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'" + databaseName + "') " +
				" DROP DATABASE [" + databaseName + "] " +
				Environment.NewLine +
				" GO " +
				Environment.NewLine +
				" CREATE DATABASE [" + databaseName +"]  ON (NAME = N'" + databaseName + "_Data', FILENAME = N'" + Path.Combine(this.databaseLocationTextBox.Text, databaseName + ".mdf") + "' , SIZE = 4, FILEGROWTH = 10%) LOG ON (NAME = N'" + databaseName + "_Log', FILENAME = N'" + Path.Combine(this.databaseLocationTextBox.Text, databaseName + "_log.ldf") + "' , FILEGROWTH = 10%) " +
				Environment.NewLine +
				" COLLATE SQL_Latin1_General_CP1_CI_AS " +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'autoclose', N'false'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'bulkcopy', N'false'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'trunc. log', N'true'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'torn page detection', N'true'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'read only', N'false'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'dbo use', N'false'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'single', N'false'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'autoshrink', N'true'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'ANSI null default', N'false'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'recursive triggers', N'false'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'ANSI nulls', N'false'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'concat null yields null', N'false'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'cursor close on commit', N'false'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'default to local cursor', N'false'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'quoted identifier', N'false'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'ANSI warnings', N'false'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'auto create statistics', N'true'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'auto update statistics', N'true'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" if( ( (@@microsoftversion / power(2, 24) = 8) and (@@microsoftversion & 0xffff >= 724) ) or ( (@@microsoftversion / power(2, 24) = 7) and (@@microsoftversion & 0xffff >= 1082) ) )" +
				Environment.NewLine +
				" exec sp_dboption N'" + databaseName + "', N'db chaining', N'false'" +
				Environment.NewLine +
				" GO";
	

			WriteToFile("CreateFlexWikiDatabase.sql", createDatabaseStatement);
			ExecuteOsqlCommand(this.sqlServerInstanceName.Text, string.Empty, "CreateFlexWikiDatabase.sql");

		}

		/// <summary>
		/// Creates the database objects including the Tables and Stored Procedures.
		/// This is thru a .sql script. The script is run using OSql.
		/// </summary>
		private void CreateDatabaseObjects()
		{
			ExecuteOsqlCommand(this.sqlServerInstanceName.Text, this.databaseName, "FlexWikiSqlStoreObjectCreationScript.sql");
		}

		/// <summary>
		/// Grants the specified user Database Owner (DBO) access.
		/// </summary>
		private void GrantUserPermissions()
		{
			string sqlUserName = this.userNameTextBox.Text;
			string grantUserPermission = "use [" + databaseName + "]" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" if not exists (select * from master.dbo.syslogins where loginname = N'" + sqlUserName + "')" +
				Environment.NewLine +
				" exec sp_grantlogin N'" + sqlUserName + "'" +
				Environment.NewLine +
				" exec sp_defaultdb N'" + sqlUserName + "', N'master'" +
				Environment.NewLine +
				" exec sp_defaultlanguage N'" + sqlUserName + "', N'us_english'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" if not exists (select * from dbo.sysusers where name = N'" + sqlUserName + "' and uid < 16382)" +
				Environment.NewLine +
				" EXEC sp_grantdbaccess N'" + sqlUserName + "', N'" + this.sqlServerInstanceName.Text + "\\" + sqlUserName + "'" +
				Environment.NewLine +
				" GO" +
				Environment.NewLine +
				" exec sp_addrolemember N'db_owner', N'" + sqlUserName + "'" +
				Environment.NewLine +
				" GO";

			WriteToFile("GrantUserPermisions.sql", grantUserPermission);
			ExecuteOsqlCommand(this.sqlServerInstanceName.Text, string.Empty, "GrantUserPermisions.sql");

		}

		private void WriteToFile(string fileName, string content)
		{
			if( File.Exists(fileName) )
			{
				File.Delete(fileName);
			}

			using(StreamWriter sr = File.CreateText(fileName))
			{
				sr.Write(content);
			}
		}

		private void ExecuteOsqlCommand(string sqlServerName, string databaseName, string fileName)
		{
			string osqlCommandParameters = sqlServerName + " " + fileName;

			ProcessStartInfo processInfo = new ProcessStartInfo("osqlcmd.cmd");

			if( databaseName != null && databaseName.Length > 0)
			{
				processInfo.FileName = "osqlcmdwithdatabase.cmd";
				osqlCommandParameters += " " + databaseName;
			}

			processInfo.Arguments = osqlCommandParameters;
			Process process = new Process();
			process.StartInfo = processInfo;
			process.Start();
			process.WaitForExit();
		}

		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.Run(new Form1());
		}

		private void CreateDatabaseButtonClick(object sender, EventArgs e)
		{
			this.CreateDatabase();
			this.CreateDatabaseObjects();
			this.GrantUserPermissions();
		}

		private void ExitButtonClick(object sender, System.EventArgs e)
		{
			this.Close();
		}

	}
}
