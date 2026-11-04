namespace DatabaseTools;

partial class ConnectionProfileEditForm
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
        nameLabel = new Label();
        nameTextBox = new TextBox();
        serverLabel = new Label();
        serverTextBox = new TextBox();
        userLabel = new Label();
        userTextBox = new TextBox();
        passwordLabel = new Label();
        passwordTextBox = new TextBox();
        okButton = new Button();
        cancelButton = new Button();
        SuspendLayout();
        //
        // nameLabel
        //
        nameLabel.Location = new Point(12, 16);
        nameLabel.Name = "nameLabel";
        nameLabel.Size = new Size(90, 22);
        nameLabel.TabIndex = 0;
        nameLabel.Text = "Name";
        nameLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // nameTextBox
        //
        nameTextBox.Location = new Point(108, 16);
        nameTextBox.Name = "nameTextBox";
        nameTextBox.Size = new Size(290, 23);
        nameTextBox.TabIndex = 1;
        //
        // serverLabel
        //
        serverLabel.Location = new Point(12, 44);
        serverLabel.Name = "serverLabel";
        serverLabel.Size = new Size(90, 22);
        serverLabel.TabIndex = 2;
        serverLabel.Text = "Server";
        serverLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // serverTextBox
        //
        serverTextBox.Location = new Point(108, 44);
        serverTextBox.Name = "serverTextBox";
        serverTextBox.Size = new Size(290, 23);
        serverTextBox.TabIndex = 3;
        //
        // userLabel
        //
        userLabel.Location = new Point(12, 72);
        userLabel.Name = "userLabel";
        userLabel.Size = new Size(90, 22);
        userLabel.TabIndex = 4;
        userLabel.Text = "User";
        userLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // userTextBox
        //
        userTextBox.Location = new Point(108, 72);
        userTextBox.Name = "userTextBox";
        userTextBox.Size = new Size(290, 23);
        userTextBox.TabIndex = 5;
        //
        // passwordLabel
        //
        passwordLabel.Location = new Point(12, 100);
        passwordLabel.Name = "passwordLabel";
        passwordLabel.Size = new Size(90, 22);
        passwordLabel.TabIndex = 6;
        passwordLabel.Text = "Password";
        passwordLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // passwordTextBox
        //
        passwordTextBox.Location = new Point(108, 100);
        passwordTextBox.Name = "passwordTextBox";
        passwordTextBox.Size = new Size(290, 23);
        passwordTextBox.TabIndex = 7;
        passwordTextBox.UseSystemPasswordChar = true;
        //
        // okButton
        //
        okButton.DialogResult = DialogResult.OK;
        okButton.Location = new Point(218, 136);
        okButton.Name = "okButton";
        okButton.Size = new Size(90, 26);
        okButton.TabIndex = 8;
        okButton.Text = "OK";
        okButton.UseVisualStyleBackColor = true;
        //
        // cancelButton
        //
        cancelButton.DialogResult = DialogResult.Cancel;
        cancelButton.Location = new Point(314, 136);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(90, 26);
        cancelButton.TabIndex = 9;
        cancelButton.Text = "Cancel";
        cancelButton.UseVisualStyleBackColor = true;
        //
        // ConnectionProfileEditForm
        //
        AcceptButton = okButton;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = cancelButton;
        ClientSize = new Size(420, 176);
        Controls.Add(nameLabel);
        Controls.Add(nameTextBox);
        Controls.Add(serverLabel);
        Controls.Add(serverTextBox);
        Controls.Add(userLabel);
        Controls.Add(userTextBox);
        Controls.Add(passwordLabel);
        Controls.Add(passwordTextBox);
        Controls.Add(okButton);
        Controls.Add(cancelButton);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "ConnectionProfileEditForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Connection profile";
        ResumeLayout(false);
        PerformLayout();
    }

    private Label nameLabel;
    private TextBox nameTextBox;
    private Label serverLabel;
    private TextBox serverTextBox;
    private Label userLabel;
    private TextBox userTextBox;
    private Label passwordLabel;
    private TextBox passwordTextBox;
    private Button okButton;
    private Button cancelButton;
}
