namespace DatabaseTools;

partial class ConnectForm
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
        rootPanel = new System.Windows.Forms.Panel();
        profileLabel = new System.Windows.Forms.Label();
        profileComboBox = new System.Windows.Forms.ComboBox();
        serverLabel = new System.Windows.Forms.Label();
        serverTextBox = new System.Windows.Forms.TextBox();
        windowsAuthCheckBox = new System.Windows.Forms.CheckBox();
        sqlUserLabel = new System.Windows.Forms.Label();
        sqlUserTextBox = new System.Windows.Forms.TextBox();
        sqlPasswordLabel = new System.Windows.Forms.Label();
        sqlPasswordTextBox = new System.Windows.Forms.TextBox();
        connectButton = new System.Windows.Forms.Button();
        rootPanel.SuspendLayout();
        SuspendLayout();
        // 
        // rootPanel
        // 
        rootPanel.Controls.Add(profileLabel);
        rootPanel.Controls.Add(profileComboBox);
        rootPanel.Controls.Add(serverLabel);
        rootPanel.Controls.Add(serverTextBox);
        rootPanel.Controls.Add(windowsAuthCheckBox);
        rootPanel.Controls.Add(sqlUserLabel);
        rootPanel.Controls.Add(sqlUserTextBox);
        rootPanel.Controls.Add(sqlPasswordLabel);
        rootPanel.Controls.Add(sqlPasswordTextBox);
        rootPanel.Controls.Add(connectButton);
        rootPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        rootPanel.Location = new System.Drawing.Point(0, 0);
        rootPanel.Name = "rootPanel";
        rootPanel.Size = new System.Drawing.Size(420, 210);
        rootPanel.TabIndex = 0;
        // 
        // profileLabel
        // 
        profileLabel.Location = new System.Drawing.Point(18, 20);
        profileLabel.Name = "profileLabel";
        profileLabel.Size = new System.Drawing.Size(100, 24);
        profileLabel.TabIndex = 0;
        profileLabel.Text = "Profile";
        profileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // profileComboBox
        // 
        profileComboBox.FormattingEnabled = true;
        profileComboBox.Location = new System.Drawing.Point(124, 20);
        profileComboBox.Name = "profileComboBox";
        profileComboBox.Size = new System.Drawing.Size(278, 23);
        profileComboBox.TabIndex = 1;
        profileComboBox.SelectedIndexChanged += profileComboBox_SelectedIndexChanged;
        // 
        // serverLabel
        // 
        serverLabel.Location = new System.Drawing.Point(18, 54);
        serverLabel.Name = "serverLabel";
        serverLabel.Size = new System.Drawing.Size(100, 24);
        serverLabel.TabIndex = 2;
        serverLabel.Text = "Server";
        serverLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // serverTextBox
        // 
        serverTextBox.Location = new System.Drawing.Point(124, 54);
        serverTextBox.Name = "serverTextBox";
        serverTextBox.Size = new System.Drawing.Size(278, 23);
        serverTextBox.TabIndex = 3;
        serverTextBox.Text = "localhost";
        // 
        // windowsAuthCheckBox
        // 
        windowsAuthCheckBox.AutoSize = true;
        windowsAuthCheckBox.Location = new System.Drawing.Point(124, 88);
        windowsAuthCheckBox.Name = "windowsAuthCheckBox";
        windowsAuthCheckBox.Size = new System.Drawing.Size(102, 19);
        windowsAuthCheckBox.TabIndex = 4;
        windowsAuthCheckBox.Text = "Use current OS user";
        windowsAuthCheckBox.UseVisualStyleBackColor = true;
        windowsAuthCheckBox.Visible = false;
        windowsAuthCheckBox.CheckedChanged += windowsAuthCheckBox_CheckedChanged;
        // 
        // sqlUserLabel
        // 
        sqlUserLabel.Location = new System.Drawing.Point(18, 88);
        sqlUserLabel.Name = "sqlUserLabel";
        sqlUserLabel.Size = new System.Drawing.Size(100, 24);
        sqlUserLabel.TabIndex = 5;
        sqlUserLabel.Text = "User";
        sqlUserLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // sqlUserTextBox
        // 
        sqlUserTextBox.Location = new System.Drawing.Point(124, 88);
        sqlUserTextBox.Name = "sqlUserTextBox";
        sqlUserTextBox.Size = new System.Drawing.Size(278, 23);
        sqlUserTextBox.TabIndex = 6;
        sqlUserTextBox.Text = "postgres";
        // 
        // sqlPasswordLabel
        // 
        sqlPasswordLabel.Location = new System.Drawing.Point(18, 122);
        sqlPasswordLabel.Name = "sqlPasswordLabel";
        sqlPasswordLabel.Size = new System.Drawing.Size(100, 24);
        sqlPasswordLabel.TabIndex = 7;
        sqlPasswordLabel.Text = "Password";
        sqlPasswordLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // sqlPasswordTextBox
        // 
        sqlPasswordTextBox.Location = new System.Drawing.Point(124, 122);
        sqlPasswordTextBox.Name = "sqlPasswordTextBox";
        sqlPasswordTextBox.Size = new System.Drawing.Size(278, 23);
        sqlPasswordTextBox.TabIndex = 8;
        sqlPasswordTextBox.UseSystemPasswordChar = true;
        // 
        // connectButton
        // 
        connectButton.Location = new System.Drawing.Point(130, 168);
        connectButton.Name = "connectButton";
        connectButton.Size = new System.Drawing.Size(160, 26);
        connectButton.TabIndex = 9;
        connectButton.Text = "Connect";
        connectButton.UseVisualStyleBackColor = true;
        connectButton.Click += connectButton_Click;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(420, 210);
        Controls.Add(rootPanel);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "Connect to PostgreSQL";
        rootPanel.ResumeLayout(false);
        rootPanel.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.Panel rootPanel;
    private System.Windows.Forms.Label profileLabel;
    private System.Windows.Forms.ComboBox profileComboBox;
    private System.Windows.Forms.Label serverLabel;
    private System.Windows.Forms.TextBox serverTextBox;
    private System.Windows.Forms.CheckBox windowsAuthCheckBox;
    private System.Windows.Forms.Label sqlUserLabel;
    private System.Windows.Forms.TextBox sqlUserTextBox;
    private System.Windows.Forms.Label sqlPasswordLabel;
    private System.Windows.Forms.TextBox sqlPasswordTextBox;
    private System.Windows.Forms.Button connectButton;
}
