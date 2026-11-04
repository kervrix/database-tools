namespace DatabaseTools;

partial class RestoreForm
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
        sourceLabel = new System.Windows.Forms.Label();
        sourcePanel = new System.Windows.Forms.FlowLayoutPanel();
        s3SourceRadioButton = new System.Windows.Forms.RadioButton();
        localSourceRadioButton = new System.Windows.Forms.RadioButton();
        s3ProfileLabel = new System.Windows.Forms.Label();
        s3ProfilePanel = new System.Windows.Forms.TableLayoutPanel();
        backupFileLabel = new System.Windows.Forms.Label();
        backupPanel = new System.Windows.Forms.TableLayoutPanel();
        backupComboBox = new System.Windows.Forms.ComboBox();
        localFileLabel = new System.Windows.Forms.Label();
        localFilePanel = new System.Windows.Forms.TableLayoutPanel();
        localFileTextBox = new System.Windows.Forms.TextBox();
        browseLocalFileButton = new System.Windows.Forms.Button();
        optionsLabel = new System.Windows.Forms.Label();
        replaceCheckBox = new System.Windows.Forms.CheckBox();
        restoreButton = new System.Windows.Forms.Button();
        rootPanel.SuspendLayout();
        sourcePanel.SuspendLayout();
        backupPanel.SuspendLayout();
        localFilePanel.SuspendLayout();
        SuspendLayout();
        // 
        // rootPanel
        // 
        rootPanel.Controls.Add(sourceLabel);
        rootPanel.Controls.Add(sourcePanel);
        rootPanel.Controls.Add(s3ProfileLabel);
        rootPanel.Controls.Add(s3ProfilePanel);
        rootPanel.Controls.Add(backupFileLabel);
        rootPanel.Controls.Add(backupPanel);
        rootPanel.Controls.Add(localFileLabel);
        rootPanel.Controls.Add(localFilePanel);
        rootPanel.Controls.Add(optionsLabel);
        rootPanel.Controls.Add(replaceCheckBox);
        rootPanel.Controls.Add(restoreButton);
        rootPanel.Dock = System.Windows.Forms.DockStyle.Fill;
        rootPanel.Location = new System.Drawing.Point(0, 0);
        rootPanel.Name = "rootPanel";
        rootPanel.Size = new System.Drawing.Size(560, 444);
        rootPanel.TabIndex = 0;
        // 
        // sourceLabel
        // 
        sourceLabel.Location = new System.Drawing.Point(14, 18);
        sourceLabel.Name = "sourceLabel";
        sourceLabel.Size = new System.Drawing.Size(100, 24);
        sourceLabel.TabIndex = 0;
        sourceLabel.Text = "Source";
        sourceLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // sourcePanel
        // 
        sourcePanel.Controls.Add(s3SourceRadioButton);
        sourcePanel.Controls.Add(localSourceRadioButton);
        sourcePanel.Location = new System.Drawing.Point(120, 16);
        sourcePanel.Name = "sourcePanel";
        sourcePanel.Size = new System.Drawing.Size(426, 26);
        sourcePanel.TabIndex = 1;
        // 
        // s3SourceRadioButton
        // 
        s3SourceRadioButton.AutoSize = true;
        s3SourceRadioButton.Location = new System.Drawing.Point(0, 3);
        s3SourceRadioButton.Margin = new System.Windows.Forms.Padding(0, 3, 10, 0);
        s3SourceRadioButton.Name = "s3SourceRadioButton";
        s3SourceRadioButton.Size = new System.Drawing.Size(37, 19);
        s3SourceRadioButton.TabIndex = 0;
        s3SourceRadioButton.Text = "S3";
        s3SourceRadioButton.UseVisualStyleBackColor = true;
        // 
        // localSourceRadioButton
        // 
        localSourceRadioButton.AutoSize = true;
        localSourceRadioButton.Location = new System.Drawing.Point(47, 3);
        localSourceRadioButton.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
        localSourceRadioButton.Name = "localSourceRadioButton";
        localSourceRadioButton.Size = new System.Drawing.Size(72, 19);
        localSourceRadioButton.TabIndex = 1;
        localSourceRadioButton.Text = "Local file";
        localSourceRadioButton.UseVisualStyleBackColor = true;
        // 
        // s3ProfileLabel
        // 
        s3ProfileLabel.Location = new System.Drawing.Point(14, 52);
        s3ProfileLabel.Name = "s3ProfileLabel";
        s3ProfileLabel.Size = new System.Drawing.Size(100, 24);
        s3ProfileLabel.TabIndex = 2;
        s3ProfileLabel.Text = "S3 profile";
        s3ProfileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // s3ProfilePanel
        // 
        s3ProfilePanel.ColumnCount = 2;
        s3ProfilePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        s3ProfilePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 86F));
        s3ProfilePanel.Location = new System.Drawing.Point(120, 50);
        s3ProfilePanel.Name = "s3ProfilePanel";
        s3ProfilePanel.RowCount = 1;
        s3ProfilePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        s3ProfilePanel.Size = new System.Drawing.Size(426, 24);
        s3ProfilePanel.TabIndex = 3;
        // 
        // backupFileLabel
        // 
        backupFileLabel.Location = new System.Drawing.Point(14, 86);
        backupFileLabel.Name = "backupFileLabel";
        backupFileLabel.Size = new System.Drawing.Size(100, 24);
        backupFileLabel.TabIndex = 4;
        backupFileLabel.Text = "Backup file";
        backupFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // backupPanel
        //
        backupPanel.ColumnCount = 1;
        backupPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        backupPanel.Controls.Add(backupComboBox, 0, 0);
        backupPanel.Location = new System.Drawing.Point(120, 84);
        backupPanel.Name = "backupPanel";
        backupPanel.RowCount = 1;
        backupPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        backupPanel.Size = new System.Drawing.Size(426, 24);
        backupPanel.TabIndex = 5;
        //
        // backupComboBox
        //
        backupComboBox.Dock = System.Windows.Forms.DockStyle.Fill;
        backupComboBox.FormattingEnabled = true;
        backupComboBox.Location = new System.Drawing.Point(0, 1);
        backupComboBox.Margin = new System.Windows.Forms.Padding(0, 1, 0, 0);
        backupComboBox.Name = "backupComboBox";
        backupComboBox.Size = new System.Drawing.Size(426, 23);
        backupComboBox.TabIndex = 0;
        //
        // localFileLabel
        // 
        localFileLabel.Location = new System.Drawing.Point(14, 120);
        localFileLabel.Name = "localFileLabel";
        localFileLabel.Size = new System.Drawing.Size(100, 24);
        localFileLabel.TabIndex = 6;
        localFileLabel.Text = "Local file";
        localFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // localFilePanel
        // 
        localFilePanel.ColumnCount = 2;
        localFilePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        localFilePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 86F));
        localFilePanel.Controls.Add(localFileTextBox, 0, 0);
        localFilePanel.Controls.Add(browseLocalFileButton, 1, 0);
        localFilePanel.Location = new System.Drawing.Point(120, 118);
        localFilePanel.Name = "localFilePanel";
        localFilePanel.RowCount = 1;
        localFilePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
        localFilePanel.Size = new System.Drawing.Size(426, 24);
        localFilePanel.TabIndex = 7;
        // 
        // localFileTextBox
        // 
        localFileTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
        localFileTextBox.Location = new System.Drawing.Point(0, 1);
        localFileTextBox.Margin = new System.Windows.Forms.Padding(0, 1, 6, 0);
        localFileTextBox.Name = "localFileTextBox";
        localFileTextBox.Size = new System.Drawing.Size(334, 23);
        localFileTextBox.TabIndex = 0;
        // 
        // browseLocalFileButton
        // 
        browseLocalFileButton.Dock = System.Windows.Forms.DockStyle.Fill;
        browseLocalFileButton.Location = new System.Drawing.Point(340, 0);
        browseLocalFileButton.Margin = new System.Windows.Forms.Padding(0);
        browseLocalFileButton.Name = "browseLocalFileButton";
        browseLocalFileButton.Size = new System.Drawing.Size(86, 24);
        browseLocalFileButton.TabIndex = 1;
        browseLocalFileButton.Text = "...";
        browseLocalFileButton.UseVisualStyleBackColor = true;
        // 
        // optionsLabel
        // 
        optionsLabel.Location = new System.Drawing.Point(14, 154);
        optionsLabel.Name = "optionsLabel";
        optionsLabel.Size = new System.Drawing.Size(100, 24);
        optionsLabel.TabIndex = 8;
        optionsLabel.Text = "Options";
        optionsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        // 
        // replaceCheckBox
        // 
        replaceCheckBox.AutoSize = true;
        replaceCheckBox.Checked = true;
        replaceCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
        replaceCheckBox.Location = new System.Drawing.Point(120, 158);
        replaceCheckBox.Name = "replaceCheckBox";
        replaceCheckBox.Size = new System.Drawing.Size(107, 19);
        replaceCheckBox.TabIndex = 9;
        replaceCheckBox.Text = "Clean schemas before restore";
        replaceCheckBox.UseVisualStyleBackColor = true;
        //
        // restoreButton
        //
        restoreButton.Location = new System.Drawing.Point(200, 194);
        restoreButton.Name = "restoreButton";
        restoreButton.Size = new System.Drawing.Size(160, 26);
        restoreButton.TabIndex = 10;
        restoreButton.Text = "Restore";
        restoreButton.UseVisualStyleBackColor = true;
        //
        // RestoreForm
        //
        AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        ClientSize = new System.Drawing.Size(560, 236);
        Controls.Add(rootPanel);
        FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        MinimumSize = new System.Drawing.Size(576, 275);
        StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        Text = "Restore";
        rootPanel.ResumeLayout(false);
        rootPanel.PerformLayout();
        sourcePanel.ResumeLayout(false);
        sourcePanel.PerformLayout();
        backupPanel.ResumeLayout(false);
        localFilePanel.ResumeLayout(false);
        localFilePanel.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.Panel rootPanel;
    private System.Windows.Forms.Label sourceLabel;
    private System.Windows.Forms.FlowLayoutPanel sourcePanel;
    private System.Windows.Forms.RadioButton s3SourceRadioButton;
    private System.Windows.Forms.RadioButton localSourceRadioButton;
    private System.Windows.Forms.Label s3ProfileLabel;
    private System.Windows.Forms.TableLayoutPanel s3ProfilePanel;
    private System.Windows.Forms.Label backupFileLabel;
    private System.Windows.Forms.TableLayoutPanel backupPanel;
    private System.Windows.Forms.ComboBox backupComboBox;
    private System.Windows.Forms.Label localFileLabel;
    private System.Windows.Forms.TableLayoutPanel localFilePanel;
    private System.Windows.Forms.TextBox localFileTextBox;
    private System.Windows.Forms.Button browseLocalFileButton;
    private System.Windows.Forms.Label optionsLabel;
    private System.Windows.Forms.CheckBox replaceCheckBox;
    private System.Windows.Forms.Button restoreButton;
}
