namespace DatabaseTools;

partial class MainForm
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

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        rootLayout = new System.Windows.Forms.TableLayoutPanel();
        toolStrip = new System.Windows.Forms.ToolStrip();
        connectToolStripButton = new System.Windows.Forms.ToolStripButton();
        addDatabaseToolStripButton = new System.Windows.Forms.ToolStripButton();
        refreshToolStripButton = new System.Windows.Forms.ToolStripButton();
        toolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
        settingsToolStripDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
        s3ProfilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        connectionProfilesMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        toolStripDivider = new System.Windows.Forms.Panel();
        databasesGrid = new System.Windows.Forms.DataGridView();
        databaseContextMenu = new System.Windows.Forms.ContextMenuStrip();
        backupMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        restoreMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        shrinkMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        contextMenuSeparator = new System.Windows.Forms.ToolStripSeparator();
        deleteDatabaseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
        logTextBox = new System.Windows.Forms.TextBox();
        rootLayout.SuspendLayout();
        toolStrip.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)databasesGrid).BeginInit();
        SuspendLayout();
        //
        // rootLayout
        //
        rootLayout.ColumnCount = 1;
        rootLayout.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        rootLayout.Controls.Add(toolStrip, 0, 0);
        rootLayout.Controls.Add(toolStripDivider, 0, 1);
        rootLayout.Controls.Add(databasesGrid, 0, 2);
        rootLayout.Controls.Add(logTextBox, 0, 3);
        rootLayout.Dock = System.Windows.Forms.DockStyle.Fill;
        rootLayout.Location = new System.Drawing.Point(0, 0);
        rootLayout.Name = "rootLayout";
        rootLayout.RowCount = 4;
        rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 31F));
        rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 1F));
        rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 116F));
        rootLayout.Size = new System.Drawing.Size(780, 500);
        rootLayout.TabIndex = 0;
        //
        // toolStrip
        //
        toolStrip.Dock = System.Windows.Forms.DockStyle.Fill;
        toolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { connectToolStripButton, addDatabaseToolStripButton, refreshToolStripButton, toolStripSeparator, settingsToolStripDropDownButton });
        toolStrip.Location = new System.Drawing.Point(0, 0);
        toolStrip.Name = "toolStrip";
        toolStrip.Padding = new System.Windows.Forms.Padding(6, 0, 6, 0);
        toolStrip.Size = new System.Drawing.Size(780, 31);
        toolStrip.TabIndex = 0;
        //
        // connectToolStripButton
        //
        connectToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
        connectToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        connectToolStripButton.Name = "connectToolStripButton";
        connectToolStripButton.Size = new System.Drawing.Size(76, 28);
        connectToolStripButton.Text = "Connect";
        connectToolStripButton.Click += connectToolStripButton_Click;
        //
        // addDatabaseToolStripButton
        //
        addDatabaseToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
        addDatabaseToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        addDatabaseToolStripButton.Name = "addDatabaseToolStripButton";
        addDatabaseToolStripButton.Size = new System.Drawing.Size(110, 28);
        addDatabaseToolStripButton.Text = "Add database";
        addDatabaseToolStripButton.Click += addDatabaseToolStripButton_Click;
        //
        // refreshToolStripButton
        //
        refreshToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
        refreshToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        refreshToolStripButton.Name = "refreshToolStripButton";
        refreshToolStripButton.Size = new System.Drawing.Size(72, 28);
        refreshToolStripButton.Text = "Refresh";
        refreshToolStripButton.Click += refreshToolStripButton_Click;
        //
        // toolStripSeparator
        //
        toolStripSeparator.Name = "toolStripSeparator";
        toolStripSeparator.Size = new System.Drawing.Size(6, 31);
        //
        // settingsToolStripDropDownButton
        //
        settingsToolStripDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.ImageAndText;
        settingsToolStripDropDownButton.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] { s3ProfilesMenuItem, connectionProfilesMenuItem });
        settingsToolStripDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
        settingsToolStripDropDownButton.Name = "settingsToolStripDropDownButton";
        settingsToolStripDropDownButton.Size = new System.Drawing.Size(86, 28);
        settingsToolStripDropDownButton.Text = "Settings";
        //
        // s3ProfilesMenuItem
        //
        s3ProfilesMenuItem.Name = "s3ProfilesMenuItem";
        s3ProfilesMenuItem.Size = new System.Drawing.Size(190, 22);
        s3ProfilesMenuItem.Text = "S3 profiles";
        s3ProfilesMenuItem.Click += s3ProfilesMenuItem_Click;
        //
        // connectionProfilesMenuItem
        //
        connectionProfilesMenuItem.Name = "connectionProfilesMenuItem";
        connectionProfilesMenuItem.Size = new System.Drawing.Size(190, 22);
        connectionProfilesMenuItem.Text = "Connection profiles";
        connectionProfilesMenuItem.Click += connectionProfilesMenuItem_Click;
        //
        // toolStripDivider
        //
        toolStripDivider.Dock = System.Windows.Forms.DockStyle.Fill;
        toolStripDivider.Location = new System.Drawing.Point(0, 31);
        toolStripDivider.Margin = new System.Windows.Forms.Padding(0);
        toolStripDivider.Name = "toolStripDivider";
        toolStripDivider.Size = new System.Drawing.Size(780, 1);
        toolStripDivider.TabIndex = 1;
        //
        // databasesGrid
        //
        databasesGrid.AllowUserToAddRows = false;
        databasesGrid.AllowUserToDeleteRows = false;
        databasesGrid.AllowUserToResizeRows = false;
        databasesGrid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        databasesGrid.BorderStyle = System.Windows.Forms.BorderStyle.None;
        databasesGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        databasesGrid.ContextMenuStrip = databaseContextMenu;
        databasesGrid.Dock = System.Windows.Forms.DockStyle.Fill;
        databasesGrid.Location = new System.Drawing.Point(0, 32);
        databasesGrid.Margin = new System.Windows.Forms.Padding(0);
        databasesGrid.MultiSelect = false;
        databasesGrid.Name = "databasesGrid";
        databasesGrid.ReadOnly = true;
        databasesGrid.RowHeadersVisible = false;
        databasesGrid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        databasesGrid.Size = new System.Drawing.Size(780, 351);
        databasesGrid.TabIndex = 2;
        databasesGrid.SelectionChanged += databasesGrid_SelectionChanged;
        databasesGrid.MouseDown += databasesGrid_MouseDown;
        //
        // databaseContextMenu
        //
        databaseContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] { backupMenuItem, restoreMenuItem, shrinkMenuItem, contextMenuSeparator, deleteDatabaseMenuItem });
        databaseContextMenu.Name = "databaseContextMenu";
        databaseContextMenu.Size = new System.Drawing.Size(180, 98);
        //
        // backupMenuItem
        //
        backupMenuItem.Name = "backupMenuItem";
        backupMenuItem.Size = new System.Drawing.Size(179, 22);
        backupMenuItem.Text = "Backup...";
        backupMenuItem.Click += backupMenuItem_Click;
        //
        // restoreMenuItem
        //
        restoreMenuItem.Name = "restoreMenuItem";
        restoreMenuItem.Size = new System.Drawing.Size(179, 22);
        restoreMenuItem.Text = "Restore...";
        restoreMenuItem.Click += restoreMenuItem_Click;
        //
        // shrinkMenuItem
        //
        shrinkMenuItem.Name = "shrinkMenuItem";
        shrinkMenuItem.Size = new System.Drawing.Size(179, 22);
        shrinkMenuItem.Text = "Shrink (VACUUM)";
        shrinkMenuItem.Click += shrinkMenuItem_Click;
        //
        // contextMenuSeparator
        //
        contextMenuSeparator.Name = "contextMenuSeparator";
        contextMenuSeparator.Size = new System.Drawing.Size(176, 6);
        //
        // deleteDatabaseMenuItem
        //
        deleteDatabaseMenuItem.Name = "deleteDatabaseMenuItem";
        deleteDatabaseMenuItem.Size = new System.Drawing.Size(179, 22);
        deleteDatabaseMenuItem.Text = "Delete database";
        deleteDatabaseMenuItem.Click += deleteDatabaseMenuItem_Click;
        //
        // logTextBox
        //
        logTextBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
        logTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        logTextBox.Location = new System.Drawing.Point(0, 384);
        logTextBox.Margin = new System.Windows.Forms.Padding(0);
        logTextBox.Multiline = true;
        logTextBox.Name = "logTextBox";
        logTextBox.ReadOnly = true;
        logTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
        logTextBox.Size = new System.Drawing.Size(780, 116);
        logTextBox.TabIndex = 3;
        //
        // DatabaseListForm
        //
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(780, 500);
        Controls.Add(rootLayout);
        MinimumSize = new System.Drawing.Size(796, 460);
        StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        Text = "Databases";
        rootLayout.ResumeLayout(false);
        rootLayout.PerformLayout();
        toolStrip.ResumeLayout(false);
        toolStrip.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)databasesGrid).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel rootLayout;
    private System.Windows.Forms.ToolStrip toolStrip;
    private System.Windows.Forms.ToolStripButton connectToolStripButton;
    private System.Windows.Forms.ToolStripButton addDatabaseToolStripButton;
    private System.Windows.Forms.ToolStripButton refreshToolStripButton;
    private System.Windows.Forms.ToolStripSeparator toolStripSeparator;
    private System.Windows.Forms.ToolStripDropDownButton settingsToolStripDropDownButton;
    private System.Windows.Forms.ToolStripMenuItem s3ProfilesMenuItem;
    private System.Windows.Forms.ToolStripMenuItem connectionProfilesMenuItem;
    private System.Windows.Forms.Panel toolStripDivider;
    private System.Windows.Forms.DataGridView databasesGrid;
    private System.Windows.Forms.ContextMenuStrip databaseContextMenu;
    private System.Windows.Forms.ToolStripMenuItem backupMenuItem;
    private System.Windows.Forms.ToolStripMenuItem restoreMenuItem;
    private System.Windows.Forms.ToolStripMenuItem shrinkMenuItem;
    private System.Windows.Forms.ToolStripSeparator contextMenuSeparator;
    private System.Windows.Forms.ToolStripMenuItem deleteDatabaseMenuItem;
    private System.Windows.Forms.TextBox logTextBox;
}
