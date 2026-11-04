namespace DatabaseTools;

partial class BackupForm
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
        localFileLabel = new Label();
        s3ProfileLabel = new Label();
        fileNameLabel = new Label();
        localRadioButton = new RadioButton();
        s3RadioButton = new RadioButton();
        localFileTextBox = new TextBox();
        browseButton = new Button();
        s3ProfileComboBox = new ComboBox();
        fileNameTextBox = new TextBox();
        initCheckBox = new CheckBox();
        backupButton = new Button();
        cancelButton = new Button();
        SuspendLayout();
        // 
        // sourceLabel
        // 
        sourceLabel.Location = new Point(18, 18);
        sourceLabel.Name = "sourceLabel";
        sourceLabel.Size = new Size(100, 24);
        sourceLabel.TabIndex = 0;
        sourceLabel.Text = "Source";
        sourceLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // localFileLabel
        // 
        localFileLabel.Location = new Point(18, 52);
        localFileLabel.Name = "localFileLabel";
        localFileLabel.Size = new Size(100, 24);
        localFileLabel.TabIndex = 3;
        localFileLabel.Text = "Local file";
        localFileLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // s3ProfileLabel
        // 
        s3ProfileLabel.Location = new Point(18, 86);
        s3ProfileLabel.Name = "s3ProfileLabel";
        s3ProfileLabel.Size = new Size(100, 24);
        s3ProfileLabel.TabIndex = 6;
        s3ProfileLabel.Text = "S3 profile";
        s3ProfileLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // fileNameLabel
        // 
        fileNameLabel.Location = new Point(18, 120);
        fileNameLabel.Name = "fileNameLabel";
        fileNameLabel.Size = new Size(100, 24);
        fileNameLabel.TabIndex = 8;
        fileNameLabel.Text = "File name";
        fileNameLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // localRadioButton
        // 
        localRadioButton.AutoSize = true;
        localRadioButton.Checked = true;
        localRadioButton.Location = new Point(124, 18);
        localRadioButton.Name = "localRadioButton";
        localRadioButton.Size = new Size(72, 19);
        localRadioButton.TabIndex = 1;
        localRadioButton.TabStop = true;
        localRadioButton.Text = "Local file";
        localRadioButton.UseVisualStyleBackColor = true;
        localRadioButton.CheckedChanged += sourceRadioButton_CheckedChanged;
        // 
        // s3RadioButton
        // 
        s3RadioButton.AutoSize = true;
        s3RadioButton.Location = new Point(220, 18);
        s3RadioButton.Name = "s3RadioButton";
        s3RadioButton.Size = new Size(37, 19);
        s3RadioButton.TabIndex = 2;
        s3RadioButton.Text = "S3";
        s3RadioButton.UseVisualStyleBackColor = true;
        s3RadioButton.CheckedChanged += sourceRadioButton_CheckedChanged;
        // 
        // localFileTextBox
        // 
        localFileTextBox.Location = new Point(124, 52);
        localFileTextBox.Name = "localFileTextBox";
        localFileTextBox.Size = new Size(296, 23);
        localFileTextBox.TabIndex = 4;
        // 
        // browseButton
        // 
        browseButton.Location = new Point(426, 51);
        browseButton.Name = "browseButton";
        browseButton.Size = new Size(86, 24);
        browseButton.TabIndex = 5;
        browseButton.Text = "...";
        browseButton.UseVisualStyleBackColor = true;
        browseButton.Click += browseButton_Click;
        // 
        // s3ProfileComboBox
        // 
        s3ProfileComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        s3ProfileComboBox.FormattingEnabled = true;
        s3ProfileComboBox.Location = new Point(124, 86);
        s3ProfileComboBox.Name = "s3ProfileComboBox";
        s3ProfileComboBox.Size = new Size(388, 23);
        s3ProfileComboBox.TabIndex = 7;
        // 
        // fileNameTextBox
        // 
        fileNameTextBox.Location = new Point(124, 120);
        fileNameTextBox.Name = "fileNameTextBox";
        fileNameTextBox.Size = new Size(388, 23);
        fileNameTextBox.TabIndex = 9;
        // 
        // initCheckBox
        // 
        initCheckBox.AutoSize = true;
        initCheckBox.Location = new Point(124, 154);
        initCheckBox.Name = "initCheckBox";
        initCheckBox.Size = new Size(143, 19);
        initCheckBox.TabIndex = 10;
        initCheckBox.Text = "Overwrite existing file";
        initCheckBox.UseVisualStyleBackColor = true;
        // 
        // backupButton
        // 
        backupButton.DialogResult = DialogResult.OK;
        backupButton.Location = new Point(178, 188);
        backupButton.Name = "backupButton";
        backupButton.Size = new Size(82, 26);
        backupButton.TabIndex = 11;
        backupButton.Text = "Backup";
        backupButton.UseVisualStyleBackColor = true;
        // 
        // cancelButton
        // 
        cancelButton.DialogResult = DialogResult.Cancel;
        cancelButton.Location = new Point(268, 188);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(82, 26);
        cancelButton.TabIndex = 12;
        cancelButton.Text = "Cancel";
        cancelButton.UseVisualStyleBackColor = true;
        // 
        // BackupForm
        // 
        AcceptButton = backupButton;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = cancelButton;
        ClientSize = new Size(526, 226);
        Controls.Add(sourceLabel);
        Controls.Add(localRadioButton);
        Controls.Add(s3RadioButton);
        Controls.Add(localFileLabel);
        Controls.Add(localFileTextBox);
        Controls.Add(browseButton);
        Controls.Add(s3ProfileLabel);
        Controls.Add(s3ProfileComboBox);
        Controls.Add(fileNameLabel);
        Controls.Add(fileNameTextBox);
        Controls.Add(initCheckBox);
        Controls.Add(backupButton);
        Controls.Add(cancelButton);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "BackupForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Backup";
        ResumeLayout(false);
        PerformLayout();
    }

    private Label sourceLabel;
    private Label localFileLabel;
    private Label s3ProfileLabel;
    private Label fileNameLabel;
    private RadioButton localRadioButton;
    private RadioButton s3RadioButton;
    private TextBox localFileTextBox;
    private Button browseButton;
    private ComboBox s3ProfileComboBox;
    private TextBox fileNameTextBox;
    private CheckBox initCheckBox;
    private Button backupButton;
    private Button cancelButton;
}
