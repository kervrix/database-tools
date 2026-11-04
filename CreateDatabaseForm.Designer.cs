namespace DatabaseTools;

partial class CreateDatabaseForm
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

    private void InitializeComponent()
    {
        sourceLabel = new Label();
        sourcePanel = new FlowLayoutPanel();
        justNameRadioButton = new RadioButton();
        s3RadioButton = new RadioButton();
        databaseNameLabel = new Label();
        databaseNameTextBox = new TextBox();
        s3ProfileLabel = new Label();
        s3ProfileComboBox = new ComboBox();
        backupLabel = new Label();
        backupComboBox = new ComboBox();
        okButton = new Button();
        cancelButton = new Button();
        sourcePanel.SuspendLayout();
        SuspendLayout();
        //
        // sourceLabel
        //
        sourceLabel.Location = new Point(14, 18);
        sourceLabel.Name = "sourceLabel";
        sourceLabel.Size = new Size(106, 24);
        sourceLabel.TabIndex = 0;
        sourceLabel.Text = "Source";
        sourceLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // sourcePanel
        //
        sourcePanel.Controls.Add(justNameRadioButton);
        sourcePanel.Controls.Add(s3RadioButton);
        sourcePanel.Location = new Point(126, 16);
        sourcePanel.Name = "sourcePanel";
        sourcePanel.Size = new Size(300, 26);
        sourcePanel.TabIndex = 1;
        //
        // justNameRadioButton
        //
        justNameRadioButton.AutoSize = true;
        justNameRadioButton.Checked = true;
        justNameRadioButton.Location = new Point(0, 3);
        justNameRadioButton.Margin = new Padding(0, 3, 14, 0);
        justNameRadioButton.Name = "justNameRadioButton";
        justNameRadioButton.Size = new Size(112, 19);
        justNameRadioButton.TabIndex = 0;
        justNameRadioButton.TabStop = true;
        justNameRadioButton.Text = "Empty database";
        justNameRadioButton.UseVisualStyleBackColor = true;
        //
        // s3RadioButton
        //
        s3RadioButton.AutoSize = true;
        s3RadioButton.Location = new Point(126, 3);
        s3RadioButton.Margin = new Padding(0, 3, 0, 0);
        s3RadioButton.Name = "s3RadioButton";
        s3RadioButton.Size = new Size(162, 19);
        s3RadioButton.TabIndex = 1;
        s3RadioButton.Text = "Restore from S3 backup";
        s3RadioButton.UseVisualStyleBackColor = true;
        //
        // databaseNameLabel
        //
        databaseNameLabel.Location = new Point(14, 52);
        databaseNameLabel.Name = "databaseNameLabel";
        databaseNameLabel.Size = new Size(106, 24);
        databaseNameLabel.TabIndex = 2;
        databaseNameLabel.Text = "Database name";
        databaseNameLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // databaseNameTextBox
        //
        databaseNameTextBox.Location = new Point(126, 52);
        databaseNameTextBox.Name = "databaseNameTextBox";
        databaseNameTextBox.Size = new Size(300, 23);
        databaseNameTextBox.TabIndex = 3;
        //
        // s3ProfileLabel
        //
        s3ProfileLabel.Location = new Point(14, 86);
        s3ProfileLabel.Name = "s3ProfileLabel";
        s3ProfileLabel.Size = new Size(106, 24);
        s3ProfileLabel.TabIndex = 4;
        s3ProfileLabel.Text = "S3 profile";
        s3ProfileLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // s3ProfileComboBox
        //
        s3ProfileComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        s3ProfileComboBox.FormattingEnabled = true;
        s3ProfileComboBox.Location = new Point(126, 86);
        s3ProfileComboBox.Name = "s3ProfileComboBox";
        s3ProfileComboBox.Size = new Size(300, 23);
        s3ProfileComboBox.TabIndex = 5;
        //
        // backupLabel
        //
        backupLabel.Location = new Point(14, 119);
        backupLabel.Name = "backupLabel";
        backupLabel.Size = new Size(106, 24);
        backupLabel.TabIndex = 6;
        backupLabel.Text = "Backup (latest)";
        backupLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // backupComboBox
        //
        backupComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        backupComboBox.FormattingEnabled = true;
        backupComboBox.Location = new Point(126, 119);
        backupComboBox.Name = "backupComboBox";
        backupComboBox.Size = new Size(300, 23);
        backupComboBox.TabIndex = 7;
        //
        // okButton
        //
        okButton.DialogResult = DialogResult.OK;
        okButton.Location = new Point(180, 158);
        okButton.Name = "okButton";
        okButton.Size = new Size(110, 26);
        okButton.TabIndex = 8;
        okButton.Text = "OK";
        okButton.UseVisualStyleBackColor = true;
        //
        // cancelButton
        //
        cancelButton.DialogResult = DialogResult.Cancel;
        cancelButton.Location = new Point(300, 158);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(110, 26);
        cancelButton.TabIndex = 9;
        cancelButton.Text = "Cancel";
        cancelButton.UseVisualStyleBackColor = true;
        //
        // CreateDatabaseForm
        //
        AcceptButton = okButton;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = cancelButton;
        ClientSize = new Size(440, 200);
        Controls.Add(sourceLabel);
        Controls.Add(sourcePanel);
        Controls.Add(databaseNameLabel);
        Controls.Add(databaseNameTextBox);
        Controls.Add(s3ProfileLabel);
        Controls.Add(s3ProfileComboBox);
        Controls.Add(backupLabel);
        Controls.Add(backupComboBox);
        Controls.Add(okButton);
        Controls.Add(cancelButton);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "CreateDatabaseForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Create database";
        sourcePanel.ResumeLayout(false);
        sourcePanel.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    private Label sourceLabel;
    private FlowLayoutPanel sourcePanel;
    private RadioButton justNameRadioButton;
    private RadioButton s3RadioButton;
    private Label databaseNameLabel;
    private TextBox databaseNameTextBox;
    private Label s3ProfileLabel;
    private ComboBox s3ProfileComboBox;
    private Label backupLabel;
    private ComboBox backupComboBox;
    private Button okButton;
    private Button cancelButton;
}
