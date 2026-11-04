namespace DatabaseTools;

partial class ConnectionProfilesForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null)
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        rootLayout = new TableLayoutPanel();
        toolStrip = new ToolStrip();
        newToolStripButton = new ToolStripButton();
        toolStripDivider = new Panel();
        profilesGrid = new DataGridView();
        nameColumn = new DataGridViewTextBoxColumn();
        serverColumn = new DataGridViewTextBoxColumn();
        userColumn = new DataGridViewTextBoxColumn();
        profileContextMenu = new ContextMenuStrip();
        editProfileMenuItem = new ToolStripMenuItem();
        deleteProfileMenuItem = new ToolStripMenuItem();
        rootLayout.SuspendLayout();
        toolStrip.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)profilesGrid).BeginInit();
        SuspendLayout();
        //
        // rootLayout
        //
        rootLayout.ColumnCount = 1;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.Controls.Add(toolStrip, 0, 0);
        rootLayout.Controls.Add(toolStripDivider, 0, 1);
        rootLayout.Controls.Add(profilesGrid, 0, 2);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.Location = new Point(0, 0);
        rootLayout.Name = "rootLayout";
        rootLayout.Padding = new Padding(0);
        rootLayout.RowCount = 3;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 31F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 1F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.Size = new Size(460, 300);
        rootLayout.TabIndex = 0;
        //
        // toolStrip
        //
        toolStrip.Dock = DockStyle.Fill;
        toolStrip.Items.AddRange(new ToolStripItem[] { newToolStripButton });
        toolStrip.Location = new Point(0, 0);
        toolStrip.Name = "toolStrip";
        toolStrip.Padding = new Padding(6, 0, 6, 0);
        toolStrip.Size = new Size(460, 31);
        toolStrip.TabIndex = 0;
        //
        // newToolStripButton
        //
        newToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
        newToolStripButton.ImageTransparentColor = Color.Magenta;
        newToolStripButton.Name = "newToolStripButton";
        newToolStripButton.Size = new Size(160, 28);
        newToolStripButton.Text = "Add connection profile";
        newToolStripButton.Click += newToolStripButton_Click;
        //
        // toolStripDivider
        //
        toolStripDivider.BackColor = SystemColors.ControlDark;
        toolStripDivider.Dock = DockStyle.Fill;
        toolStripDivider.Location = new Point(0, 31);
        toolStripDivider.Margin = new Padding(0);
        toolStripDivider.Name = "toolStripDivider";
        toolStripDivider.Size = new Size(460, 1);
        toolStripDivider.TabIndex = 1;
        //
        // profilesGrid
        //
        profilesGrid.AllowUserToAddRows = false;
        profilesGrid.AllowUserToDeleteRows = false;
        profilesGrid.AllowUserToResizeRows = false;
        profilesGrid.AutoGenerateColumns = false;
        profilesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        profilesGrid.BorderStyle = BorderStyle.None;
        profilesGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        profilesGrid.Columns.AddRange(new DataGridViewColumn[] { nameColumn, serverColumn, userColumn });
        profilesGrid.ContextMenuStrip = profileContextMenu;
        profilesGrid.Dock = DockStyle.Fill;
        profilesGrid.Location = new Point(0, 32);
        profilesGrid.Margin = new Padding(0);
        profilesGrid.MultiSelect = false;
        profilesGrid.Name = "profilesGrid";
        profilesGrid.ReadOnly = true;
        profilesGrid.RowHeadersVisible = false;
        profilesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        profilesGrid.Size = new Size(460, 268);
        profilesGrid.TabIndex = 2;
        profilesGrid.CellDoubleClick += profilesGrid_CellDoubleClick;
        profilesGrid.SelectionChanged += profilesGrid_SelectionChanged;
        profilesGrid.MouseDown += profilesGrid_MouseDown;
        //
        // nameColumn
        //
        nameColumn.DataPropertyName = "Name";
        nameColumn.HeaderText = "Name";
        nameColumn.Name = "nameColumn";
        nameColumn.ReadOnly = true;
        //
        // serverColumn
        //
        serverColumn.DataPropertyName = "Server";
        serverColumn.HeaderText = "Server";
        serverColumn.Name = "serverColumn";
        serverColumn.ReadOnly = true;
        //
        // userColumn
        //
        userColumn.DataPropertyName = "UserName";
        userColumn.HeaderText = "User";
        userColumn.Name = "userColumn";
        userColumn.ReadOnly = true;
        //
        // profileContextMenu
        //
        profileContextMenu.Items.AddRange(new ToolStripItem[] { editProfileMenuItem, deleteProfileMenuItem });
        profileContextMenu.Name = "profileContextMenu";
        profileContextMenu.Size = new Size(120, 48);
        //
        // editProfileMenuItem
        //
        editProfileMenuItem.Name = "editProfileMenuItem";
        editProfileMenuItem.Size = new Size(119, 22);
        editProfileMenuItem.Text = "Edit";
        editProfileMenuItem.Click += editProfileMenuItem_Click;
        //
        // deleteProfileMenuItem
        //
        deleteProfileMenuItem.Name = "deleteProfileMenuItem";
        deleteProfileMenuItem.Size = new Size(119, 22);
        deleteProfileMenuItem.Text = "Delete";
        deleteProfileMenuItem.Click += deleteProfileMenuItem_Click;
        //
        // ConnectionProfilesForm
        //
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(460, 300);
        Controls.Add(rootLayout);
        MinimumSize = new Size(400, 260);
        Name = "ConnectionProfilesForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Connection profiles";
        rootLayout.ResumeLayout(false);
        rootLayout.PerformLayout();
        toolStrip.ResumeLayout(false);
        toolStrip.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)profilesGrid).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private TableLayoutPanel rootLayout;
    private ToolStrip toolStrip;
    private ToolStripButton newToolStripButton;
    private Panel toolStripDivider;
    private DataGridView profilesGrid;
    private DataGridViewTextBoxColumn nameColumn;
    private DataGridViewTextBoxColumn serverColumn;
    private DataGridViewTextBoxColumn userColumn;
    private ContextMenuStrip profileContextMenu;
    private ToolStripMenuItem editProfileMenuItem;
    private ToolStripMenuItem deleteProfileMenuItem;
}
