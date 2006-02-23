using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Net;
using FlexWikiEditor.FlexWikiServices;

namespace FlexWikiEditor
{
	/// <summary>
	/// Summary description for MainForm.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
		private System.ComponentModel.IContainer components;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPagePreview;
		private System.Windows.Forms.TabPage tabPageText;
		private onlyconnect.HtmlEditor htmlEditor1;
		private System.Windows.Forms.RichTextBox richTextBox1;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Panel panelMainRight;
		private DockingSuite.DockHost dockHost1;
		private DockingSuite.DockPanel dockPanel1;
		private DockingSuite.DockControl dockControl1;
		private System.Windows.Forms.Panel panelNamespaces;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ComboBox comboBoxContentBase;
		private System.Windows.Forms.ListView listViewTopicNames;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private TD.SandBar.SandBarManager sandBarManager1;
		private TD.SandBar.ToolBarContainer leftSandBarDock;
		private TD.SandBar.ToolBarContainer rightSandBarDock;
		private TD.SandBar.ToolBarContainer bottomSandBarDock;
		private TD.SandBar.ToolBarContainer topSandBarDock;
		private TD.SandBar.ToolBar toolBarWikiServer;
		private TD.SandBar.MenuBar menuBar1;
		private TD.SandBar.MenuBarItem menuBarItem1;
		private TD.SandBar.MenuBarItem menuBarItem2;
		private TD.SandBar.MenuBarItem menuBarItem3;
		private TD.SandBar.MenuBarItem menuBarItem4;
		private TD.SandBar.MenuBarItem menuBarItem5;
		private TD.SandBar.ButtonItem buttonItemSave;
		private System.Windows.Forms.ImageList imageListToolbar;
		private TD.SandBar.ComboBoxItem comboBoxAttribution;
		private TD.SandBar.ToolBar toolBarFormatting;
		private TD.SandBar.ButtonItem buttonItemBold;
		private TD.SandBar.ButtonItem buttonItemConnect;
		private TD.SandBar.ComboBoxItem comboBoxItemWikiServer;
		private TD.SandBar.ButtonItem buttonItemRestore;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Label label1;
		private TD.SandBar.FlatComboBox flatComboBoxVersion;
		private TD.SandBar.ButtonItem buttonItemItalic;
		private TD.SandBar.ButtonItem buttonItemUnderline;
		private TD.SandBar.ButtonItem buttonItemOrderedList;
		private TD.SandBar.ButtonItem buttonItemUnorderedList;
		private TD.SandBar.ButtonItem buttonItemInsertLink;
		private TD.SandBar.ButtonItem buttonItemInsertHorizonalRule;
		private TD.SandBar.MenuButtonItem menuButtonItemTopicNames;
		private TD.SandBar.ToolBar toolBarTopicName;

		private string currentWikiServer;
		private ArrayList contentBases = new ArrayList();
		private ContentBase currentContentBase;
		private AbsoluteTopicName currentTopicName;
		private bool isLoading;
		private FlexWikiServices.EditService editService;

		public MainForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			TD.SandBar.Office2003Renderer renderer = 
				(TD.SandBar.Office2003Renderer)sandBarManager1.Renderer;
			renderer.ColorScheme = TD.SandBar.Office2003Renderer.Office2003ColorScheme.Standard;


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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(MainForm));
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPagePreview = new System.Windows.Forms.TabPage();
			this.htmlEditor1 = new onlyconnect.HtmlEditor();
			this.panel1 = new System.Windows.Forms.Panel();
			this.label1 = new System.Windows.Forms.Label();
			this.flatComboBoxVersion = new TD.SandBar.FlatComboBox();
			this.tabPageText = new System.Windows.Forms.TabPage();
			this.richTextBox1 = new System.Windows.Forms.RichTextBox();
			this.imageListToolbar = new System.Windows.Forms.ImageList(this.components);
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.panelMainRight = new System.Windows.Forms.Panel();
			this.dockHost1 = new DockingSuite.DockHost();
			this.dockPanel1 = new DockingSuite.DockPanel();
			this.dockControl1 = new DockingSuite.DockControl();
			this.listViewTopicNames = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.panelNamespaces = new System.Windows.Forms.Panel();
			this.label2 = new System.Windows.Forms.Label();
			this.comboBoxContentBase = new System.Windows.Forms.ComboBox();
			this.sandBarManager1 = new TD.SandBar.SandBarManager();
			this.bottomSandBarDock = new TD.SandBar.ToolBarContainer();
			this.leftSandBarDock = new TD.SandBar.ToolBarContainer();
			this.rightSandBarDock = new TD.SandBar.ToolBarContainer();
			this.topSandBarDock = new TD.SandBar.ToolBarContainer();
			this.menuBar1 = new TD.SandBar.MenuBar();
			this.menuBarItem1 = new TD.SandBar.MenuBarItem();
			this.menuBarItem2 = new TD.SandBar.MenuBarItem();
			this.menuBarItem3 = new TD.SandBar.MenuBarItem();
			this.menuBarItem4 = new TD.SandBar.MenuBarItem();
			this.menuButtonItemTopicNames = new TD.SandBar.MenuButtonItem();
			this.menuBarItem5 = new TD.SandBar.MenuBarItem();
			this.toolBarWikiServer = new TD.SandBar.ToolBar();
			this.comboBoxItemWikiServer = new TD.SandBar.ComboBoxItem();
			this.buttonItemConnect = new TD.SandBar.ButtonItem();
			this.toolBarFormatting = new TD.SandBar.ToolBar();
			this.buttonItemBold = new TD.SandBar.ButtonItem();
			this.buttonItemItalic = new TD.SandBar.ButtonItem();
			this.buttonItemUnderline = new TD.SandBar.ButtonItem();
			this.buttonItemOrderedList = new TD.SandBar.ButtonItem();
			this.buttonItemUnorderedList = new TD.SandBar.ButtonItem();
			this.buttonItemInsertLink = new TD.SandBar.ButtonItem();
			this.buttonItemInsertHorizonalRule = new TD.SandBar.ButtonItem();
			this.toolBarTopicName = new TD.SandBar.ToolBar();
			this.buttonItemSave = new TD.SandBar.ButtonItem();
			this.buttonItemRestore = new TD.SandBar.ButtonItem();
			this.comboBoxAttribution = new TD.SandBar.ComboBoxItem();
			this.tabControl1.SuspendLayout();
			this.tabPagePreview.SuspendLayout();
			this.panel1.SuspendLayout();
			this.tabPageText.SuspendLayout();
			this.panelMainRight.SuspendLayout();
			this.dockHost1.SuspendLayout();
			this.dockPanel1.SuspendLayout();
			this.dockControl1.SuspendLayout();
			this.panelNamespaces.SuspendLayout();
			this.topSandBarDock.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPagePreview);
			this.tabControl1.Controls.Add(this.tabPageText);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.ImageList = this.imageListToolbar;
			this.tabControl1.Location = new System.Drawing.Point(3, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(562, 511);
			this.tabControl1.TabIndex = 2;
			this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
			// 
			// tabPagePreview
			// 
			this.tabPagePreview.Controls.Add(this.htmlEditor1);
			this.tabPagePreview.Controls.Add(this.panel1);
			this.tabPagePreview.ImageIndex = 10;
			this.tabPagePreview.Location = new System.Drawing.Point(4, 23);
			this.tabPagePreview.Name = "tabPagePreview";
			this.tabPagePreview.Size = new System.Drawing.Size(554, 484);
			this.tabPagePreview.TabIndex = 0;
			this.tabPagePreview.Text = "Preview";
			// 
			// htmlEditor1
			// 
			this.htmlEditor1.DefaultComposeSettings.BackColor = System.Drawing.Color.White;
			this.htmlEditor1.DefaultComposeSettings.DefaultFont = new System.Drawing.Font("Arial", 10F);
			this.htmlEditor1.DefaultComposeSettings.Enabled = false;
			this.htmlEditor1.DefaultComposeSettings.ForeColor = System.Drawing.Color.Black;
			this.htmlEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.htmlEditor1.DocumentEncoding = onlyconnect.EncodingType.WindowsCurrent;
			this.htmlEditor1.Location = new System.Drawing.Point(0, 32);
			this.htmlEditor1.Name = "htmlEditor1";
			this.htmlEditor1.OpenLinksInNewWindow = true;
			this.htmlEditor1.SelectionAlignment = System.Windows.Forms.HorizontalAlignment.Left;
			this.htmlEditor1.SelectionBackColor = System.Drawing.Color.Empty;
			this.htmlEditor1.SelectionBullets = false;
			this.htmlEditor1.SelectionFont = null;
			this.htmlEditor1.SelectionForeColor = System.Drawing.Color.Empty;
			this.htmlEditor1.SelectionNumbering = false;
			this.htmlEditor1.Size = new System.Drawing.Size(554, 452);
			this.htmlEditor1.TabIndex = 3;
			this.htmlEditor1.TabStop = false;
			this.htmlEditor1.Text = "htmlEditor1";
			this.htmlEditor1.BeforeNavigate += new onlyconnect.BeforeNavigateEventHandler(this.htmlEditor1_BeforeNavigate);
			this.htmlEditor1.ReadyStateChanged += new onlyconnect.ReadyStateChangedHandler(this.htmlEditor1_ReadyStateChanged);
			// 
			// panel1
			// 
			this.panel1.BackColor = System.Drawing.SystemColors.Window;
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.flatComboBoxVersion);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(554, 32);
			this.panel1.TabIndex = 4;
			// 
			// label1
			// 
			this.label1.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
			this.label1.Location = new System.Drawing.Point(8, 6);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(48, 21);
			this.label1.TabIndex = 5;
			this.label1.Text = "Version:";
			this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// flatComboBoxVersion
			// 
			this.flatComboBoxVersion.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.flatComboBoxVersion.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.flatComboBoxVersion.ForeColor = System.Drawing.SystemColors.ControlDark;
			this.flatComboBoxVersion.Location = new System.Drawing.Point(64, 6);
			this.flatComboBoxVersion.Name = "flatComboBoxVersion";
			this.flatComboBoxVersion.Size = new System.Drawing.Size(480, 21);
			this.flatComboBoxVersion.TabIndex = 4;
			this.flatComboBoxVersion.SelectedIndexChanged += new System.EventHandler(this.flatComboBoxVersion_SelectedIndexChanged);
			// 
			// tabPageText
			// 
			this.tabPageText.Controls.Add(this.richTextBox1);
			this.tabPageText.DockPadding.All = 3;
			this.tabPageText.ImageIndex = 11;
			this.tabPageText.Location = new System.Drawing.Point(4, 23);
			this.tabPageText.Name = "tabPageText";
			this.tabPageText.Size = new System.Drawing.Size(554, 563);
			this.tabPageText.TabIndex = 1;
			this.tabPageText.Text = "Text";
			// 
			// richTextBox1
			// 
			this.richTextBox1.AcceptsTab = true;
			this.richTextBox1.AutoWordSelection = true;
			this.richTextBox1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.richTextBox1.Font = new System.Drawing.Font("Courier New", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.richTextBox1.Location = new System.Drawing.Point(3, 3);
			this.richTextBox1.Name = "richTextBox1";
			this.richTextBox1.Size = new System.Drawing.Size(548, 557);
			this.richTextBox1.TabIndex = 0;
			this.richTextBox1.Text = "";
			// 
			// imageListToolbar
			// 
			this.imageListToolbar.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageListToolbar.ImageSize = new System.Drawing.Size(16, 16);
			this.imageListToolbar.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageListToolbar.ImageStream")));
			this.imageListToolbar.TransparentColor = System.Drawing.Color.FromArgb(((System.Byte)(238)), ((System.Byte)(238)), ((System.Byte)(238)));
			// 
			// panelMainRight
			// 
			this.panelMainRight.Controls.Add(this.tabControl1);
			this.panelMainRight.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panelMainRight.DockPadding.Left = 3;
			this.panelMainRight.DockPadding.Right = 3;
			this.panelMainRight.ForeColor = System.Drawing.SystemColors.ControlText;
			this.panelMainRight.Location = new System.Drawing.Point(208, 79);
			this.panelMainRight.Name = "panelMainRight";
			this.panelMainRight.Size = new System.Drawing.Size(568, 511);
			this.panelMainRight.TabIndex = 5;
			// 
			// dockHost1
			// 
			this.dockHost1.Controls.Add(this.dockPanel1);
			this.dockHost1.Dock = System.Windows.Forms.DockStyle.Left;
			this.dockHost1.Location = new System.Drawing.Point(0, 79);
			this.dockHost1.Name = "dockHost1";
			this.dockHost1.Size = new System.Drawing.Size(208, 511);
			this.dockHost1.TabIndex = 9;
			// 
			// dockPanel1
			// 
			this.dockPanel1.AutoHide = false;
			this.dockPanel1.Controls.Add(this.dockControl1);
			this.dockPanel1.DockedHeight = 511;
			this.dockPanel1.DockedWidth = 0;
			this.dockPanel1.Location = new System.Drawing.Point(0, 0);
			this.dockPanel1.Name = "dockPanel1";
			this.dockPanel1.SelectedTab = this.dockControl1;
			this.dockPanel1.Size = new System.Drawing.Size(204, 511);
			this.dockPanel1.TabIndex = 0;
			this.dockPanel1.Text = "Docked Panel";
			this.dockPanel1.SizeChanged += new System.EventHandler(this.dockPanel1_SizeChanged);
			// 
			// dockControl1
			// 
			this.dockControl1.Controls.Add(this.listViewTopicNames);
			this.dockControl1.Controls.Add(this.panelNamespaces);
			this.dockControl1.Guid = new System.Guid("2d0a7eb3-9426-48a7-bb12-fa94a6b10763");
			this.dockControl1.Location = new System.Drawing.Point(0, 20);
			this.dockControl1.Name = "dockControl1";
			this.dockControl1.PrimaryControl = null;
			this.dockControl1.Size = new System.Drawing.Size(204, 468);
			this.dockControl1.TabImage = null;
			this.dockControl1.TabIndex = 0;
			this.dockControl1.Text = "Topic Names";
			// 
			// listViewTopicNames
			// 
			this.listViewTopicNames.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																								 this.columnHeader1});
			this.listViewTopicNames.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listViewTopicNames.HideSelection = false;
			this.listViewTopicNames.Location = new System.Drawing.Point(0, 32);
			this.listViewTopicNames.MultiSelect = false;
			this.listViewTopicNames.Name = "listViewTopicNames";
			this.listViewTopicNames.Size = new System.Drawing.Size(204, 436);
			this.listViewTopicNames.TabIndex = 5;
			this.listViewTopicNames.View = System.Windows.Forms.View.Details;
			this.listViewTopicNames.SelectedIndexChanged += new System.EventHandler(this.listViewTopicNames_SelectedIndexChanged);
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Topic Names";
			this.columnHeader1.Width = 188;
			// 
			// panelNamespaces
			// 
			this.panelNamespaces.Controls.Add(this.label2);
			this.panelNamespaces.Controls.Add(this.comboBoxContentBase);
			this.panelNamespaces.Dock = System.Windows.Forms.DockStyle.Top;
			this.panelNamespaces.Location = new System.Drawing.Point(0, 0);
			this.panelNamespaces.Name = "panelNamespaces";
			this.panelNamespaces.Size = new System.Drawing.Size(204, 32);
			this.panelNamespaces.TabIndex = 4;
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(8, 11);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(78, 16);
			this.label2.TabIndex = 1;
			this.label2.Text = "Namespaces:";
			// 
			// comboBoxContentBase
			// 
			this.comboBoxContentBase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.comboBoxContentBase.Location = new System.Drawing.Point(88, 8);
			this.comboBoxContentBase.Name = "comboBoxContentBase";
			this.comboBoxContentBase.Size = new System.Drawing.Size(112, 21);
			this.comboBoxContentBase.TabIndex = 2;
			this.comboBoxContentBase.SelectedIndexChanged += new System.EventHandler(this.comboBoxContentBase_SelectedIndexChanged);
			// 
			// sandBarManager1
			// 
			this.sandBarManager1.BottomContainer = this.bottomSandBarDock;
			this.sandBarManager1.LeftContainer = this.leftSandBarDock;
			this.sandBarManager1.OwnerForm = this;
			this.sandBarManager1.RightContainer = this.rightSandBarDock;
			this.sandBarManager1.TopContainer = this.topSandBarDock;
			// 
			// bottomSandBarDock
			// 
			this.bottomSandBarDock.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomSandBarDock.Location = new System.Drawing.Point(0, 590);
			this.bottomSandBarDock.Manager = this.sandBarManager1;
			this.bottomSandBarDock.Name = "bottomSandBarDock";
			this.bottomSandBarDock.Size = new System.Drawing.Size(776, 0);
			this.bottomSandBarDock.TabIndex = 12;
			// 
			// leftSandBarDock
			// 
			this.leftSandBarDock.Dock = System.Windows.Forms.DockStyle.Left;
			this.leftSandBarDock.Location = new System.Drawing.Point(0, 79);
			this.leftSandBarDock.Manager = this.sandBarManager1;
			this.leftSandBarDock.Name = "leftSandBarDock";
			this.leftSandBarDock.Size = new System.Drawing.Size(0, 511);
			this.leftSandBarDock.TabIndex = 10;
			// 
			// rightSandBarDock
			// 
			this.rightSandBarDock.Dock = System.Windows.Forms.DockStyle.Right;
			this.rightSandBarDock.Location = new System.Drawing.Point(776, 79);
			this.rightSandBarDock.Manager = this.sandBarManager1;
			this.rightSandBarDock.Name = "rightSandBarDock";
			this.rightSandBarDock.Size = new System.Drawing.Size(0, 511);
			this.rightSandBarDock.TabIndex = 11;
			// 
			// topSandBarDock
			// 
			this.topSandBarDock.Controls.Add(this.menuBar1);
			this.topSandBarDock.Controls.Add(this.toolBarWikiServer);
			this.topSandBarDock.Controls.Add(this.toolBarFormatting);
			this.topSandBarDock.Controls.Add(this.toolBarTopicName);
			this.topSandBarDock.Dock = System.Windows.Forms.DockStyle.Top;
			this.topSandBarDock.Location = new System.Drawing.Point(0, 0);
			this.topSandBarDock.Manager = this.sandBarManager1;
			this.topSandBarDock.Name = "topSandBarDock";
			this.topSandBarDock.Size = new System.Drawing.Size(776, 79);
			this.topSandBarDock.TabIndex = 13;
			// 
			// menuBar1
			// 
			this.menuBar1.Buttons.AddRange(new TD.SandBar.ToolbarItemBase[] {
																				this.menuBarItem1,
																				this.menuBarItem2,
																				this.menuBarItem3,
																				this.menuBarItem4,
																				this.menuBarItem5});
			this.menuBar1.Guid = new System.Guid("93588b8e-b092-4601-851a-63ee29c6f34f");
			this.menuBar1.ImageList = null;
			this.menuBar1.Location = new System.Drawing.Point(2, 0);
			this.menuBar1.Name = "menuBar1";
			this.menuBar1.Size = new System.Drawing.Size(774, 25);
			this.menuBar1.TabIndex = 1;
			// 
			// menuBarItem1
			// 
			this.menuBarItem1.Icon = null;
			this.menuBarItem1.Tag = null;
			this.menuBarItem1.Text = "&File";
			// 
			// menuBarItem2
			// 
			this.menuBarItem2.Icon = null;
			this.menuBarItem2.Tag = null;
			this.menuBarItem2.Text = "&Edit";
			// 
			// menuBarItem3
			// 
			this.menuBarItem3.Icon = null;
			this.menuBarItem3.Tag = null;
			this.menuBarItem3.Text = "&View";
			// 
			// menuBarItem4
			// 
			this.menuBarItem4.Icon = null;
			this.menuBarItem4.MenuItems.AddRange(new TD.SandBar.MenuButtonItem[] {
																					 this.menuButtonItemTopicNames});
			this.menuBarItem4.Tag = null;
			this.menuBarItem4.Text = "&Window";
			// 
			// menuButtonItemTopicNames
			// 
			this.menuButtonItemTopicNames.Icon = null;
			this.menuButtonItemTopicNames.Shortcut = System.Windows.Forms.Shortcut.None;
			this.menuButtonItemTopicNames.Tag = null;
			this.menuButtonItemTopicNames.Text = "Topic Names";
			// 
			// menuBarItem5
			// 
			this.menuBarItem5.Icon = null;
			this.menuBarItem5.Tag = null;
			this.menuBarItem5.Text = "&Help";
			// 
			// toolBarWikiServer
			// 
			this.toolBarWikiServer.Buttons.AddRange(new TD.SandBar.ToolbarItemBase[] {
																						 this.comboBoxItemWikiServer,
																						 this.buttonItemConnect});
			this.toolBarWikiServer.DockLine = 1;
			this.toolBarWikiServer.Guid = new System.Guid("81fe7604-43a4-46c8-9964-46e672a85bda");
			this.toolBarWikiServer.ImageList = this.imageListToolbar;
			this.toolBarWikiServer.Location = new System.Drawing.Point(2, 25);
			this.toolBarWikiServer.Name = "toolBarWikiServer";
			this.toolBarWikiServer.Size = new System.Drawing.Size(324, 27);
			this.toolBarWikiServer.TabIndex = 0;
			this.toolBarWikiServer.ButtonClick += new TD.SandBar.ToolBar.ButtonClickEventHandler(this.toolBarWikiServer_ButtonClick);
			// 
			// comboBoxItemWikiServer
			// 
			this.comboBoxItemWikiServer.ControlWidth = 160;
			this.comboBoxItemWikiServer.DefaultText = "http://localhost/flexwiki.web";
			this.comboBoxItemWikiServer.Items.AddRange(new object[] {
																		"http://www.flexwiki.com",
																		"http://localhost/flexwiki.web",
																		"http://localhost/flexwiki"});
			this.comboBoxItemWikiServer.Padding.Left = 1;
			this.comboBoxItemWikiServer.Padding.Right = 1;
			this.comboBoxItemWikiServer.Tag = null;
			this.comboBoxItemWikiServer.Text = "Wiki Server:";
			// 
			// buttonItemConnect
			// 
			this.buttonItemConnect.BuddyMenu = null;
			this.buttonItemConnect.Icon = null;
			this.buttonItemConnect.ImageIndex = 9;
			this.buttonItemConnect.Tag = null;
			this.buttonItemConnect.Text = "Connect";
			// 
			// toolBarFormatting
			// 
			this.toolBarFormatting.Buttons.AddRange(new TD.SandBar.ToolbarItemBase[] {
																						 this.buttonItemBold,
																						 this.buttonItemItalic,
																						 this.buttonItemUnderline,
																						 this.buttonItemOrderedList,
																						 this.buttonItemUnorderedList,
																						 this.buttonItemInsertLink,
																						 this.buttonItemInsertHorizonalRule});
			this.toolBarFormatting.DockLine = 2;
			this.toolBarFormatting.Enabled = false;
			this.toolBarFormatting.Guid = new System.Guid("9e362bb2-ef19-4cca-9fdc-fee2a70b3b3b");
			this.toolBarFormatting.ImageList = this.imageListToolbar;
			this.toolBarFormatting.Location = new System.Drawing.Point(421, 52);
			this.toolBarFormatting.Name = "toolBarFormatting";
			this.toolBarFormatting.Size = new System.Drawing.Size(198, 27);
			this.toolBarFormatting.TabIndex = 2;
			this.toolBarFormatting.ButtonClick += new TD.SandBar.ToolBar.ButtonClickEventHandler(this.toolBarFormatting_ButtonClick);
			// 
			// buttonItemBold
			// 
			this.buttonItemBold.BuddyMenu = null;
			this.buttonItemBold.Icon = null;
			this.buttonItemBold.ImageIndex = 1;
			this.buttonItemBold.Tag = null;
			// 
			// buttonItemItalic
			// 
			this.buttonItemItalic.BuddyMenu = null;
			this.buttonItemItalic.Icon = null;
			this.buttonItemItalic.ImageIndex = 2;
			this.buttonItemItalic.Tag = null;
			// 
			// buttonItemUnderline
			// 
			this.buttonItemUnderline.BuddyMenu = null;
			this.buttonItemUnderline.Icon = null;
			this.buttonItemUnderline.ImageIndex = 3;
			this.buttonItemUnderline.Tag = null;
			// 
			// buttonItemOrderedList
			// 
			this.buttonItemOrderedList.BeginGroup = true;
			this.buttonItemOrderedList.BuddyMenu = null;
			this.buttonItemOrderedList.Icon = null;
			this.buttonItemOrderedList.ImageIndex = 4;
			this.buttonItemOrderedList.Tag = null;
			// 
			// buttonItemUnorderedList
			// 
			this.buttonItemUnorderedList.BuddyMenu = null;
			this.buttonItemUnorderedList.Icon = null;
			this.buttonItemUnorderedList.ImageIndex = 5;
			this.buttonItemUnorderedList.Tag = null;
			// 
			// buttonItemInsertLink
			// 
			this.buttonItemInsertLink.BeginGroup = true;
			this.buttonItemInsertLink.BuddyMenu = null;
			this.buttonItemInsertLink.Icon = null;
			this.buttonItemInsertLink.ImageIndex = 7;
			this.buttonItemInsertLink.Tag = null;
			// 
			// buttonItemInsertHorizonalRule
			// 
			this.buttonItemInsertHorizonalRule.BuddyMenu = null;
			this.buttonItemInsertHorizonalRule.Icon = null;
			this.buttonItemInsertHorizonalRule.ImageIndex = 6;
			this.buttonItemInsertHorizonalRule.Tag = null;
			// 
			// toolBarTopicName
			// 
			this.toolBarTopicName.Buttons.AddRange(new TD.SandBar.ToolbarItemBase[] {
																						this.buttonItemSave,
																						this.buttonItemRestore,
																						this.comboBoxAttribution});
			this.toolBarTopicName.DockLine = 2;
			this.toolBarTopicName.Guid = new System.Guid("f00294ea-0283-4b0f-8f28-892528563d56");
			this.toolBarTopicName.ImageList = this.imageListToolbar;
			this.toolBarTopicName.Location = new System.Drawing.Point(2, 52);
			this.toolBarTopicName.Name = "toolBarTopicName";
			this.toolBarTopicName.Size = new System.Drawing.Size(417, 27);
			this.toolBarTopicName.TabIndex = 3;
			this.toolBarTopicName.ButtonClick += new TD.SandBar.ToolBar.ButtonClickEventHandler(this.toolBarTopicName_ButtonClick);
			// 
			// buttonItemSave
			// 
			this.buttonItemSave.BuddyMenu = null;
			this.buttonItemSave.Icon = null;
			this.buttonItemSave.ImageIndex = 0;
			this.buttonItemSave.Tag = null;
			this.buttonItemSave.Text = "Save";
			// 
			// buttonItemRestore
			// 
			this.buttonItemRestore.BuddyMenu = null;
			this.buttonItemRestore.Enabled = false;
			this.buttonItemRestore.Icon = null;
			this.buttonItemRestore.ImageIndex = 8;
			this.buttonItemRestore.Tag = null;
			this.buttonItemRestore.Text = "Restore";
			// 
			// comboBoxAttribution
			// 
			this.comboBoxAttribution.BeginGroup = true;
			this.comboBoxAttribution.ControlWidth = 200;
			this.comboBoxAttribution.Padding.Left = 1;
			this.comboBoxAttribution.Padding.Right = 1;
			this.comboBoxAttribution.Tag = null;
			this.comboBoxAttribution.Text = "Attribution:";
			// 
			// MainForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 14);
			this.ClientSize = new System.Drawing.Size(776, 590);
			this.Controls.Add(this.panelMainRight);
			this.Controls.Add(this.dockHost1);
			this.Controls.Add(this.leftSandBarDock);
			this.Controls.Add(this.rightSandBarDock);
			this.Controls.Add(this.bottomSandBarDock);
			this.Controls.Add(this.topSandBarDock);
			this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.Name = "MainForm";
			this.Text = "FlexWikiEditor";
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.tabControl1.ResumeLayout(false);
			this.tabPagePreview.ResumeLayout(false);
			this.panel1.ResumeLayout(false);
			this.tabPageText.ResumeLayout(false);
			this.panelMainRight.ResumeLayout(false);
			this.dockHost1.ResumeLayout(false);
			this.dockPanel1.ResumeLayout(false);
			this.dockControl1.ResumeLayout(false);
			this.panelNamespaces.ResumeLayout(false);
			this.topSandBarDock.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main() 
		{
			Application.EnableVisualStyles();
			Application.DoEvents();
			Application.Run(new MainForm());
		}

		private void MainForm_Load(object sender, System.EventArgs e)
		{
//			string ipAddress = System.Net.Dns.GetHostName();
//			System.Net.IPHostEntry ipEntry =  System.Net.Dns.GetHostByName (ipAddress);
//			System.Net.IPAddress[] addr = ipEntry.AddressList;

//			comboBoxAttribution.Items.Add(addr[0].ToString());
//			comboBoxAttribution.DefaultText = addr[0].ToString();

			listViewTopicNames.Columns[0].Width = listViewTopicNames.Width - 4;

			// load the embeded StyleSheet from the assembly resource and write it to the executing assemblies location
			System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
			Stream rsrc = asm.GetManifestResourceStream("FlexWikiEditor.wiki.css");
			StreamReader sr = new StreamReader(rsrc);

			if (!File.Exists("wiki.css")) 
			{
				using (StreamWriter sw = new StreamWriter("wiki.css"))
				{
					sw.Write(sr.ReadToEnd());
				}
			}
		}

		private void ConnectToWiki()
		{
			flatComboBoxVersion.Items.Clear();

			if (comboBoxItemWikiServer.ComboBox.SelectedIndex == -1)
			{
				if (comboBoxItemWikiServer.ComboBox.Text != "")
				{
					currentWikiServer = comboBoxItemWikiServer.ComboBox.Text;
				}
				else
				{
					currentWikiServer = comboBoxItemWikiServer.DefaultText;
				}
			}
			else
			{
				currentWikiServer = comboBoxItemWikiServer.ComboBox.SelectedItem.ToString();
			}

			try
			{
				editService = new EditService();
				editService.Url = (currentWikiServer.EndsWith("/") == false) ? currentWikiServer += "/EditService.asmx" : currentWikiServer += "EditService.asmx";
				// Assign the credentials of the logged in user or the user being impersonated.
				editService.Credentials = CredentialCache.DefaultCredentials;

				string credentials = editService.CanEdit();

				comboBoxAttribution.Items.Add(credentials);
				comboBoxAttribution.DefaultText = credentials;

				if (credentials != null)
				{
					this.comboBoxAttribution.Items.Clear();
					this.comboBoxAttribution.Items.Add(credentials);
					this.comboBoxAttribution.DefaultText = credentials;
					//this.comboBoxAttribution.DropDownStyle = ComboBoxStyle.DropDownList;
				}


				currentContentBase = editService.GetDefaultNamespace();

				LoadTopics(currentContentBase);
				LoadNamespaces();
			}
			catch (WebException ex)
			{
				MessageBox.Show(String.Format("There was a problem connecting to the FlexWiki server: {0}", ex.Message), "An error occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			catch (Exception ex)
			{
				MessageBox.Show(String.Format("There was a problem connecting to the FlexWiki server: {0}", ex.Message), "An error occured", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void LoadNamespaces()
		{
			isLoading = true;

			contentBases.Clear();
			contentBases.AddRange(editService.GetAllNamespaces());

			comboBoxContentBase.DataSource = Leadit.Utils.WebServices.WebServiceWrapper.GetArrayList(contentBases.ToArray());
			comboBoxContentBase.DisplayMember = "Namespace";
			comboBoxContentBase.ValueMember = "Namespace";
			comboBoxContentBase.SelectedValue = currentContentBase.Namespace;

			isLoading = false;
		}

		private void LoadTopics(ContentBase cb)
		{
			isLoading = true;

			currentContentBase = cb;

			listViewTopicNames.BeginUpdate();
			listViewTopicNames.Items.Clear();

			foreach(AbsoluteTopicName topicName in editService.GetAllTopics(cb))
			{
				ListViewItem lvi = new ListViewItem(topicName.Name);
				lvi.Tag = topicName;
				listViewTopicNames.Items.Add(lvi);
			}

			listViewTopicNames.EndUpdate();

			// clear the Preview and Text areas
			richTextBox1.Clear();
			htmlEditor1.LoadDocument("");
			flatComboBoxVersion.Items.Clear();

			isLoading = false;
		}

		private void LoadTopic(AbsoluteTopicName topicName)
		{
			isLoading = true;

			currentTopicName = topicName;

			// HACK: need to add this div tag so that the ToolTip's work.
			htmlEditor1.LoadDocument("<div id='TopicTip' class='TopicTip' ></div>" + editService.GetHtmlForTopic(topicName));
			richTextBox1.Text = editService.GetTextForTopic(topicName);

			GetVersionsForTopic(topicName);

			isLoading = false;
		}

		private void GetVersionsForTopic(AbsoluteTopicName topicName)
		{
			flatComboBoxVersion.Items.Clear();
			buttonItemRestore.Enabled = false;

			string[] versions = editService.GetVersionsForTopic(topicName);

			if (versions.Length > 0)
			{
				foreach (string s in versions)
				{
					flatComboBoxVersion.Items.Add(s);
				}

				flatComboBoxVersion.SelectedIndex = 0;
			}
		}

		private void SetStyleSheet()
		{
//			string serverUrl =  comboBoxItemWikiServer.Text;	
//			string styleSheet = (serverUrl.EndsWith("/") == false) ? serverUrl += "/wiki.css" : serverUrl += "wiki.css";
			
			string currentPath = Path.Combine(Directory.GetCurrentDirectory(), "wiki.css") ;
			
			htmlEditor1.setStyleSheet(currentPath);
		}

		private void tabControl1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (tabControl1.SelectedIndex == 0 && listViewTopicNames.SelectedItems.Count == 1)
			{
				ListViewItem lvi = listViewTopicNames.SelectedItems[0];
				htmlEditor1.LoadDocument(editService.GetPreviewForTopic((AbsoluteTopicName)lvi.Tag, richTextBox1.Text));
				toolBarFormatting.Enabled = false;
				if (flatComboBoxVersion.Items.Count > 0)
					flatComboBoxVersion.SelectedIndex = 0;
			}
			else if (tabControl1.SelectedIndex == 1)
			{
				toolBarFormatting.Enabled = true;
			}
		}

		private void comboBoxContentBase_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if (!isLoading)
			{
				isLoading = true;

				ContentBase cb = new ContentBase();
				cb.Namespace = comboBoxContentBase.SelectedValue.ToString();

				LoadTopics(cb);
				
				isLoading = false;
			}
		}

		private void listViewTopicNames_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// prevent the event from firing if we are loading the listView
			if(!isLoading)
			{
				if (listViewTopicNames.SelectedItems.Count == 1)
				{
					ListViewItem lvi = listViewTopicNames.SelectedItems[0];
					LoadTopic((AbsoluteTopicName)lvi.Tag);
				}
			}
		}

		private void flatComboBoxVersion_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			if(!isLoading)
			{
				string version = flatComboBoxVersion.SelectedItem.ToString();
				ListViewItem lvi = listViewTopicNames.SelectedItems[0];
				htmlEditor1.LoadDocument(editService.GetHtmlForTopicVersion((AbsoluteTopicName)listViewTopicNames.SelectedItems[0].Tag, version));
				
				if (flatComboBoxVersion.SelectedIndex > 0)
					buttonItemRestore.Enabled = true;
				else
					buttonItemRestore.Enabled = false;
			}
		}

		private void toolBarWikiServer_ButtonClick(object sender, TD.SandBar.ToolBarItemEventArgs e)
		{
			if(e.Item == buttonItemConnect)
			{
				ConnectToWiki();
			}
		}

		private void toolBarFormatting_ButtonClick(object sender, TD.SandBar.ToolBarItemEventArgs e)
		{
			string selectedText = richTextBox1.SelectedText;


			if (e.Item == buttonItemBold)
			{
				richTextBox1.SelectedText = String.Format("{0}{1}{2}", "*", selectedText, "*");
			}
			else if (e.Item == buttonItemItalic)
			{
				richTextBox1.SelectedText = String.Format("{0}{1}{2}", "_", selectedText, "_");
			}
			else if (e.Item == buttonItemUnderline)
			{
				richTextBox1.SelectedText = String.Format("{0}{1}{2}", "+", selectedText, "+");
			}
			else if (e.Item == buttonItemOrderedList)
			{
				richTextBox1.SelectedText = FormatList(selectedText, "1. ");
			}
			else if (e.Item == buttonItemUnorderedList)
			{
				richTextBox1.SelectedText = FormatList(selectedText, "* ");
			}
			else if (e.Item == buttonItemInsertLink)
			{
				richTextBox1.SelectedText = String.Format("\"{0}\":{1}", selectedText, "<enterUrl>");
			}
			else if (e.Item == buttonItemInsertHorizonalRule)
			{
				richTextBox1.SelectedText = String.Format("{0}\n{1}", "----", selectedText);
			}
		
		}

		private void toolBarTopicName_ButtonClick(object sender, TD.SandBar.ToolBarItemEventArgs e)
		{
			if (e.Item == buttonItemSave)
			{
				string attribution = (comboBoxAttribution.ComboBox.Text == "") ? comboBoxAttribution.DefaultText : comboBoxAttribution.ComboBox.Text;

				editService.SetTextForTopic((AbsoluteTopicName)listViewTopicNames.SelectedItems[0].Tag, richTextBox1.Text, attribution);
				
				// reload the version list and refresh the Preview and Text tabs.
				LoadTopic((AbsoluteTopicName)listViewTopicNames.SelectedItems[0].Tag);
			}
			else if (e.Item == buttonItemRestore)
			{
				if (flatComboBoxVersion.SelectedIndex > 0)
				{
					// restore the topic to a previous version
					string version = flatComboBoxVersion.SelectedItem.ToString();

					string attribution = (comboBoxAttribution.ComboBox.Text == "") ? comboBoxAttribution.DefaultText : comboBoxAttribution.ComboBox.Text;
					editService.RestoreTopic((AbsoluteTopicName)listViewTopicNames.SelectedItems[0].Tag, attribution, version);

					// reload the version list and refresh the Preview and Text tabs.
					LoadTopic((AbsoluteTopicName)listViewTopicNames.SelectedItems[0].Tag);
				}
			}
		}

		private string FormatList(string selectedText, string token)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();

			if(selectedText != "")
			{
				string delimStr = "\n";
				char[] delimiter = delimStr.ToCharArray();
				string[] lines = selectedText.Split(delimiter);

				foreach(string line in lines)
				{
					if (line != "")
						sb.AppendFormat("\t{0} {1}\n",token, line);
				}
			}
			else
			{
				sb.AppendFormat("\t{0} {1}\n", token, "<Item 1>");
				sb.AppendFormat("\t\t{0} {1}\n", token, "<SubItem 1>");
				sb.AppendFormat("\t{0} {1}\n", token, "<Item 2>");
				sb.AppendFormat("\t{0} {1}\n", token, "<Item 3>");
			}

			return sb.ToString();
		}

		private void dockPanel1_SizeChanged(object sender, System.EventArgs e)
		{
			listViewTopicNames.Columns[0].Width = listViewTopicNames.Width - 4;
		}

		private void htmlEditor1_ReadyStateChanged(object sender, onlyconnect.ReadyStateChangedEventArgs e)
		{
			if (e.ReadyState == "complete")
			{
				SetStyleSheet();
			}
		}

		private void htmlEditor1_BeforeNavigate(object s, onlyconnect.BeforeNavigateEventArgs e)
		{
			Console.WriteLine(e.NewTarget);

			string[] splitUrl = e.Target.Split('/');
			string fullName = splitUrl[splitUrl.Length-1];
			string[] split = fullName.Split('.');

			if (split.Length == 2)
			{
				AbsoluteTopicName currentTopicName = new AbsoluteTopicName();
				currentTopicName.Fullname = fullName;

				currentTopicName.Namespace = split[0];
				currentTopicName.Name = split[1];

				if (currentTopicName.Namespace == currentContentBase.Namespace)
				{
					foreach (ListViewItem lvi in listViewTopicNames.Items)
					{
						AbsoluteTopicName topicName = (AbsoluteTopicName)lvi.Tag;
						if (currentTopicName.Name  == topicName.Name)
						{
							lvi.Selected = true;
							e.Cancel = true;
							break;
						}
					}
				}
				else
				{
					foreach(ContentBase cb in contentBases)
					{
						if (cb.Namespace == currentTopicName.Namespace )
						{
							comboBoxContentBase.SelectedValue = currentTopicName.Namespace;

							foreach (ListViewItem lvi in listViewTopicNames.Items)
							{
								AbsoluteTopicName topicName = (AbsoluteTopicName)lvi.Tag;
								if (currentTopicName.Name  == topicName.Name)
								{
									lvi.Selected = true;
									e.Cancel = true;
									break;
								}
							}
						}
					}
				}
			}
		}
	}
}
