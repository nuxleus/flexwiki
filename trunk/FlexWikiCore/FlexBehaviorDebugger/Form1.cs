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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Text;
using FlexWiki.BeL;
using FlexWiki.Formatting;
using FlexWiki.Newsletters;

namespace FlexWiki.BeL.Debugger
{
	/// <summary>
	/// Summary description for Form1.
	/// </summary>
	public class Form1 : System.Windows.Forms.Form, IWikiToPresentation, IDeliveryBoy
	{
		private System.Windows.Forms.TextBox textBoxInput;
		private System.Windows.Forms.TreeView treeViewParseTree;
		private System.Windows.Forms.TextBox textBoxLog;
		private System.Windows.Forms.ListBox listBoxTokens;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ListBox listBoxCache;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.TabPage tabParser;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.TabControl tabControl;
		private System.Windows.Forms.TabPage tabOutput;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox textBoxFederationPath;
		private System.Windows.Forms.Label label7;
		private System.Windows.Forms.ComboBox comboBoxTopic;
		private System.Windows.Forms.ErrorProvider errorProviderFederationFile;
		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Panel panel4;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Panel panel5;
		private System.Windows.Forms.Panel panel6;
		private System.Windows.Forms.Splitter splitter2;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.TextBox textBoxNewsletterDelivery;
		private System.ComponentModel.IContainer components;

		public Form1()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
			UpdateFederationInfo();
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
			this.components = new System.ComponentModel.Container();
			this.textBoxInput = new System.Windows.Forms.TextBox();
			this.treeViewParseTree = new System.Windows.Forms.TreeView();
			this.textBoxLog = new System.Windows.Forms.TextBox();
			this.listBoxTokens = new System.Windows.Forms.ListBox();
			this.button3 = new System.Windows.Forms.Button();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.label1 = new System.Windows.Forms.Label();
			this.label2 = new System.Windows.Forms.Label();
			this.label3 = new System.Windows.Forms.Label();
			this.label4 = new System.Windows.Forms.Label();
			this.listBoxCache = new System.Windows.Forms.ListBox();
			this.label5 = new System.Windows.Forms.Label();
			this.tabControl = new System.Windows.Forms.TabControl();
			this.tabParser = new System.Windows.Forms.TabPage();
			this.panel5 = new System.Windows.Forms.Panel();
			this.splitter2 = new System.Windows.Forms.Splitter();
			this.panel6 = new System.Windows.Forms.Panel();
			this.panel4 = new System.Windows.Forms.Panel();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.panel2 = new System.Windows.Forms.Panel();
			this.panel3 = new System.Windows.Forms.Panel();
			this.tabOutput = new System.Windows.Forms.TabPage();
			this.panel1 = new System.Windows.Forms.Panel();
			this.comboBoxTopic = new System.Windows.Forms.ComboBox();
			this.textBoxFederationPath = new System.Windows.Forms.TextBox();
			this.label6 = new System.Windows.Forms.Label();
			this.label7 = new System.Windows.Forms.Label();
			this.errorProviderFederationFile = new System.Windows.Forms.ErrorProvider();
			this.mainMenu1 = new System.Windows.Forms.MainMenu();
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.button1 = new System.Windows.Forms.Button();
			this.textBoxNewsletterDelivery = new System.Windows.Forms.TextBox();
			this.tabControl.SuspendLayout();
			this.tabParser.SuspendLayout();
			this.panel5.SuspendLayout();
			this.panel6.SuspendLayout();
			this.panel4.SuspendLayout();
			this.panel2.SuspendLayout();
			this.panel3.SuspendLayout();
			this.tabOutput.SuspendLayout();
			this.panel1.SuspendLayout();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.SuspendLayout();
			// 
			// textBoxInput
			// 
			this.textBoxInput.Dock = System.Windows.Forms.DockStyle.Fill;
			this.textBoxInput.Location = new System.Drawing.Point(0, 32);
			this.textBoxInput.Multiline = true;
			this.textBoxInput.Name = "textBoxInput";
			this.textBoxInput.Size = new System.Drawing.Size(760, 68);
			this.textBoxInput.TabIndex = 0;
			this.textBoxInput.Text = "textBox1";
			this.toolTip1.SetToolTip(this.textBoxInput, "Enter behavior expression");
			this.textBoxInput.TextChanged += new System.EventHandler(this.textBoxInput_TextChanged);
			// 
			// treeViewParseTree
			// 
			this.treeViewParseTree.Dock = System.Windows.Forms.DockStyle.Fill;
			this.treeViewParseTree.ImageIndex = -1;
			this.treeViewParseTree.Location = new System.Drawing.Point(0, 0);
			this.treeViewParseTree.Name = "treeViewParseTree";
			this.treeViewParseTree.SelectedImageIndex = -1;
			this.treeViewParseTree.Size = new System.Drawing.Size(448, 176);
			this.treeViewParseTree.TabIndex = 1;
			// 
			// textBoxLog
			// 
			this.textBoxLog.Location = new System.Drawing.Point(8, 24);
			this.textBoxLog.Multiline = true;
			this.textBoxLog.Name = "textBoxLog";
			this.textBoxLog.Size = new System.Drawing.Size(616, 64);
			this.textBoxLog.TabIndex = 0;
			this.textBoxLog.Text = "";
			// 
			// listBoxTokens
			// 
			this.listBoxTokens.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listBoxTokens.Location = new System.Drawing.Point(0, 23);
			this.listBoxTokens.Name = "listBoxTokens";
			this.listBoxTokens.Size = new System.Drawing.Size(312, 147);
			this.listBoxTokens.TabIndex = 3;
			this.listBoxTokens.SelectedIndexChanged += new System.EventHandler(this.listBoxTokens_SelectedIndexChanged);
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(104, 6);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(72, 20);
			this.button3.TabIndex = 2;
			this.button3.Text = "Run";
			this.toolTip1.SetToolTip(this.button3, "parse and evaluate the expression");
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// label1
			// 
			this.label1.Dock = System.Windows.Forms.DockStyle.Top;
			this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label1.Location = new System.Drawing.Point(0, 0);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(312, 23);
			this.label1.TabIndex = 5;
			this.label1.Text = "Lexical Analysis";
			// 
			// label2
			// 
			this.label2.Dock = System.Windows.Forms.DockStyle.Top;
			this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label2.Location = new System.Drawing.Point(0, 0);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(448, 23);
			this.label2.TabIndex = 5;
			this.label2.Text = "Parse Tree";
			// 
			// label3
			// 
			this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label3.Location = new System.Drawing.Point(8, 8);
			this.label3.Name = "label3";
			this.label3.TabIndex = 5;
			this.label3.Text = "Results";
			// 
			// label4
			// 
			this.label4.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label4.Location = new System.Drawing.Point(8, 88);
			this.label4.Name = "label4";
			this.label4.TabIndex = 5;
			this.label4.Text = "Cache Rules";
			// 
			// listBoxCache
			// 
			this.listBoxCache.Location = new System.Drawing.Point(8, 112);
			this.listBoxCache.Name = "listBoxCache";
			this.listBoxCache.Size = new System.Drawing.Size(616, 56);
			this.listBoxCache.TabIndex = 6;
			// 
			// label5
			// 
			this.label5.Dock = System.Windows.Forms.DockStyle.Top;
			this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label5.Location = new System.Drawing.Point(0, 0);
			this.label5.Name = "label5";
			this.label5.Size = new System.Drawing.Size(760, 32);
			this.label5.TabIndex = 5;
			this.label5.Text = "Input Expression";
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.tabParser);
			this.tabControl.Controls.Add(this.tabOutput);
			this.tabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl.Location = new System.Drawing.Point(0, 0);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = new System.Drawing.Size(768, 302);
			this.tabControl.TabIndex = 7;
			// 
			// tabParser
			// 
			this.tabParser.Controls.Add(this.panel5);
			this.tabParser.Location = new System.Drawing.Point(4, 22);
			this.tabParser.Name = "tabParser";
			this.tabParser.Size = new System.Drawing.Size(760, 276);
			this.tabParser.TabIndex = 0;
			this.tabParser.Text = "Parser";
			// 
			// panel5
			// 
			this.panel5.Controls.Add(this.splitter2);
			this.panel5.Controls.Add(this.panel6);
			this.panel5.Controls.Add(this.panel4);
			this.panel5.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel5.Location = new System.Drawing.Point(0, 0);
			this.panel5.Name = "panel5";
			this.panel5.Size = new System.Drawing.Size(760, 276);
			this.panel5.TabIndex = 9;
			// 
			// splitter2
			// 
			this.splitter2.BackColor = System.Drawing.SystemColors.ControlLight;
			this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter2.Location = new System.Drawing.Point(0, 97);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = new System.Drawing.Size(760, 3);
			this.splitter2.TabIndex = 11;
			this.splitter2.TabStop = false;
			// 
			// panel6
			// 
			this.panel6.Controls.Add(this.button3);
			this.panel6.Controls.Add(this.textBoxInput);
			this.panel6.Controls.Add(this.label5);
			this.panel6.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel6.Location = new System.Drawing.Point(0, 0);
			this.panel6.Name = "panel6";
			this.panel6.Size = new System.Drawing.Size(760, 100);
			this.panel6.TabIndex = 10;
			// 
			// panel4
			// 
			this.panel4.Controls.Add(this.splitter1);
			this.panel4.Controls.Add(this.panel2);
			this.panel4.Controls.Add(this.panel3);
			this.panel4.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel4.Location = new System.Drawing.Point(0, 100);
			this.panel4.Name = "panel4";
			this.panel4.Size = new System.Drawing.Size(760, 176);
			this.panel4.TabIndex = 8;
			// 
			// splitter1
			// 
			this.splitter1.BackColor = System.Drawing.SystemColors.ControlLight;
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
			this.splitter1.Location = new System.Drawing.Point(309, 0);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(3, 176);
			this.splitter1.TabIndex = 7;
			this.splitter1.TabStop = false;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.listBoxTokens);
			this.panel2.Controls.Add(this.label1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(312, 176);
			this.panel2.TabIndex = 6;
			// 
			// panel3
			// 
			this.panel3.Controls.Add(this.label2);
			this.panel3.Controls.Add(this.treeViewParseTree);
			this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
			this.panel3.Location = new System.Drawing.Point(312, 0);
			this.panel3.Name = "panel3";
			this.panel3.Size = new System.Drawing.Size(448, 176);
			this.panel3.TabIndex = 7;
			// 
			// tabOutput
			// 
			this.tabOutput.Controls.Add(this.textBoxLog);
			this.tabOutput.Controls.Add(this.label3);
			this.tabOutput.Controls.Add(this.label4);
			this.tabOutput.Controls.Add(this.listBoxCache);
			this.tabOutput.Location = new System.Drawing.Point(4, 22);
			this.tabOutput.Name = "tabOutput";
			this.tabOutput.Size = new System.Drawing.Size(760, 276);
			this.tabOutput.TabIndex = 1;
			this.tabOutput.Text = "Output";
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.comboBoxTopic);
			this.panel1.Controls.Add(this.textBoxFederationPath);
			this.panel1.Controls.Add(this.label6);
			this.panel1.Controls.Add(this.label7);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(776, 78);
			this.panel1.TabIndex = 8;
			// 
			// comboBoxTopic
			// 
			this.comboBoxTopic.Location = new System.Drawing.Point(376, 30);
			this.comboBoxTopic.Name = "comboBoxTopic";
			this.comboBoxTopic.Size = new System.Drawing.Size(360, 21);
			this.comboBoxTopic.TabIndex = 7;
			this.comboBoxTopic.SelectedIndexChanged += new System.EventHandler(this.comboBoxTopic_SelectedIndexChanged);
			// 
			// textBoxFederationPath
			// 
			this.textBoxFederationPath.Location = new System.Drawing.Point(8, 32);
			this.textBoxFederationPath.Name = "textBoxFederationPath";
			this.textBoxFederationPath.Size = new System.Drawing.Size(360, 20);
			this.textBoxFederationPath.TabIndex = 6;
			this.textBoxFederationPath.Text = "C:\\Safe\\Keep\\Dev\\Flex\\FlexWiki.Web\\WikiBases\\NamespaceMap.xml";
			this.textBoxFederationPath.TextChanged += new System.EventHandler(this.textBox1_TextChanged);
			// 
			// label6
			// 
			this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label6.Location = new System.Drawing.Point(3, 8);
			this.label6.Name = "label6";
			this.label6.TabIndex = 5;
			this.label6.Text = "Federation File";
			// 
			// label7
			// 
			this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.label7.Location = new System.Drawing.Point(368, 6);
			this.label7.Name = "label7";
			this.label7.TabIndex = 5;
			this.label7.Text = "Topic";
			this.label7.Click += new System.EventHandler(this.label7_Click);
			// 
			// errorProviderFederationFile
			// 
			this.errorProviderFederationFile.ContainerControl = this;
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.tabControl1.Location = new System.Drawing.Point(0, 78);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(776, 328);
			this.tabControl1.TabIndex = 8;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.tabControl);
			this.tabPage1.Location = new System.Drawing.Point(4, 22);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Size = new System.Drawing.Size(768, 302);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "WikiTalk";
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.textBoxNewsletterDelivery);
			this.tabPage2.Controls.Add(this.button1);
			this.tabPage2.Location = new System.Drawing.Point(4, 22);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Size = new System.Drawing.Size(768, 302);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Newsletter";
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(24, 16);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(144, 23);
			this.button1.TabIndex = 0;
			this.button1.Text = "Process newsletters";
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// textBoxNewsletterDelivery
			// 
			this.textBoxNewsletterDelivery.Location = new System.Drawing.Point(192, 16);
			this.textBoxNewsletterDelivery.Multiline = true;
			this.textBoxNewsletterDelivery.Name = "textBoxNewsletterDelivery";
			this.textBoxNewsletterDelivery.Size = new System.Drawing.Size(520, 200);
			this.textBoxNewsletterDelivery.TabIndex = 1;
			this.textBoxNewsletterDelivery.Text = "";
			// 
			// Form1
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(776, 406);
			this.Controls.Add(this.panel1);
			this.Controls.Add(this.tabControl1);
			this.Menu = this.mainMenu1;
			this.Name = "Form1";
			this.Text = "BEL Debugger";
			this.tabControl.ResumeLayout(false);
			this.tabParser.ResumeLayout(false);
			this.panel5.ResumeLayout(false);
			this.panel6.ResumeLayout(false);
			this.panel4.ResumeLayout(false);
			this.panel2.ResumeLayout(false);
			this.panel3.ResumeLayout(false);
			this.tabOutput.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.tabPage2.ResumeLayout(false);
			this.ResumeLayout(false);

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

		void Lex()
		{
			listBoxTokens.Items.Clear();
			Scanner scanner = new Scanner(textBoxInput.Text);
			Token token;
			while (true)
			{
				token = scanner.Next();
				if (token.Type == TokenType.TokenEndOfInput)
					break;
				string s = token.Type.ToString();
				if (token.Value != null)
					s += "(" + token.Value + ")";
				listBoxTokens.Items.Add(s);
			}
		}

		private void listBoxTokens_SelectedIndexChanged(object sender, System.EventArgs e)
		{
		
		}

		void LexAndParse()
		{
			BehaviorInterpreter interpreter = new BehaviorInterpreter(textBoxInput.Text, CurrentTopicContext.CurrentFederation, 1, this);
			Lex();	
			Parse(interpreter);
		}

		ParseTreeNode Parse(BehaviorInterpreter interpreter)
		{
			textBoxLog.Text = "";
			ParseTreeNode result = null;
			if (interpreter.Parse())
				result = interpreter.ParseTree;
			treeViewParseTree.Nodes.Clear();
			if (result == null)
			{
				textBoxLog.Text = interpreter.ErrorString;
			}
			else
			{
				treeViewParseTree.Nodes.Add(TreeNodesFor(result));
				treeViewParseTree.ExpandAll();
			}
			return result;
		}

		TreeNode TreeNodesFor(ParseTreeNode obj)
		{
			TreeNode answer = new TreeNode(obj.ToString());
			answer.Tag = obj;

			foreach (ParseTreeNode child in obj.Children)
				answer.Nodes.Add(TreeNodesFor(child));
			return answer;
		}

		Federation _CurrentFederation = null;
		Federation CurrentFederation
		{
			get
			{
				if (_CurrentFederation != null)
					return _CurrentFederation;
				UpdateFederationInfo();
				return _CurrentFederation;
			}
		}

		void UpdateFederationInfo()
		{
			_CurrentFederation = null;
			errorProviderFederationFile.SetError(textBoxFederationPath, "");
			string fn = textBoxFederationPath.Text;
			if (!System.IO.File.Exists(fn))
			{
				errorProviderFederationFile.SetError(textBoxFederationPath, "File not found");
				return;
			}
			_CurrentFederation = new Federation(fn, OutputFormat.Testing, null);
			comboBoxTopic.Items.Clear();
			foreach (ContentBase cb in _CurrentFederation.ContentBases)
			{
				foreach (AbsoluteTopicName tn in cb.AllTopics(false))
				{
					comboBoxTopic.Items.Add(tn.ToString());
				}
			}
			if (comboBoxTopic.Items.Count > 0 && (comboBoxTopic.SelectedValue == null || (string)(comboBoxTopic.SelectedValue) == ""))
				comboBoxTopic.SelectedValue = comboBoxTopic.Items[0];
		}


		TopicContext _CurrentTopicContext = null;
		TopicContext CurrentTopicContext
		{
			get
			{
				if (_CurrentTopicContext != null)
					return _CurrentTopicContext;
				AbsoluteTopicName tn = new AbsoluteTopicName(comboBoxTopic.Text);
				_CurrentTopicContext = new TopicContext(CurrentFederation, CurrentFederation.ContentBaseForTopic(tn), new TopicInfo(CurrentFederation, tn));
				return _CurrentTopicContext;
			}
		}

		private void button3_Click(object sender, System.EventArgs e)
		{
			Federation fed = new Federation(textBoxFederationPath.Text, OutputFormat.Testing, null);
			BehaviorInterpreter interpreter = new BehaviorInterpreter(textBoxInput.Text, CurrentTopicContext.CurrentFederation, 1, this);
					
			ClearCacheView();

			if (Parse(interpreter) == null)
			{
				Fail("Unable to run; parse failed:" + interpreter.ErrorString);
				tabControl.SelectedTab = tabParser;
			}

			// Begin execution
			tabControl.SelectedTab = tabOutput;
			ExecutionContext ctx = new ExecutionContext(CurrentTopicContext);
			string final = null;
			if (!interpreter.EvaluateToPresentation(CurrentTopicContext, new Hashtable()))
			{
				Fail(interpreter.ErrorString);
				return;
			}
			WikiOutput output = WikiOutput.ForFormat(OutputFormat.Testing, null);
			interpreter.Value.ToPresentation(this).OutputTo(output);
			final = output.ToString();
			Success(final);
			UpdateCacheView(interpreter.CacheRules);
		}

		void Fail(string s)
		{
			textBoxLog.Text = s;
			textBoxLog.BackColor = Color.Red;
			textBoxLog.ForeColor = Color.White;
		}

		void Success(string s)
		{
			textBoxLog.Text = s;
			textBoxLog.BackColor = Color.Green;
			textBoxLog.ForeColor = Color.White;
		}
		#region IWikiToPresentation Members

		public string WikiToPresentation(string s)
		{
			return "P(" + s + ")";
		}

		#endregion

		void ClearCacheView()
		{
			listBoxCache.Items.Clear();
		}

		void UpdateCacheView(ICollection rules)
		{
			ClearCacheView();
			foreach (CacheRule rule in rules)
				listBoxCache.Items.Add(rule.Description);
		}

		private void textBoxInput_TextChanged(object sender, System.EventArgs e)
		{
			LexAndParse();	
		}

		private void textBox1_TextChanged(object sender, System.EventArgs e)
		{
			UpdateFederationInfo();
		}

		private void comboBoxTopic_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			_CurrentTopicContext = null;
		}

		private void label7_Click(object sender, System.EventArgs e)
		{
		
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			ProcessNewsletters();
		}

		DateTime NewsletterStartStamp;
		DateTime NewsletterEndStamp;

		void NewsletterStart()
		{
			NewsletterStartStamp = DateTime.Now;
			NLog("Start newsletter processing");
		}

		void NewsletterEnd()
		{
			NewsletterEndStamp = DateTime.Now;
			NLog("End newsletter processing");
			NLog("Duration: " + NewsletterEndStamp.Subtract(NewsletterStartStamp));
		}

		void ProcessNewsletters()
		{
			NewsletterStart();
			LinkMaker lm = new LinkMaker("http://dummy");
			Federation fed = new Federation(textBoxFederationPath.Text, FlexWiki.Formatting.OutputFormat.HTML, lm);
			fed.FederationCache = new NewsletterDaemon.Cache();
			NewsletterManager manager = new NewsletterManager(fed, lm, this, "test@test.com", "");
			StringBuilder sb = new StringBuilder();
			StringWriter sw = new StringWriter(sb);
			manager.Notify(sw);
			textBoxNewsletterDelivery.Text += sb.ToString();
			NewsletterEnd();
		}
		#region IDeliveryBoy Members

		public void Deliver(string to, string from, string subject, string body)
		{
			String s = "";
			
			s += "To: " + to + Environment.NewLine;
			s += "From: " + from + Environment.NewLine;
			s += "Subject: " + subject + Environment.NewLine + Environment.NewLine;
			s += body + Environment.NewLine;
			NLog(s);
		}

		#endregion

		void NLog(string s)
		{
			textBoxNewsletterDelivery.Text += s;
		}
	}
}
