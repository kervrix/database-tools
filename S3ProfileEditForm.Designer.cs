namespace DatabaseTools;

partial class S3ProfileEditForm
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
        endpointUrlLabel = new Label();
        endpointUrlTextBox = new TextBox();
        bucketLabel = new Label();
        bucketTextBox = new TextBox();
        accessKeyLabel = new Label();
        accessKeyTextBox = new TextBox();
        secretKeyLabel = new Label();
        secretKeyTextBox = new TextBox();
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
        // endpointUrlLabel
        //
        endpointUrlLabel.Location = new Point(12, 44);
        endpointUrlLabel.Name = "endpointUrlLabel";
        endpointUrlLabel.Size = new Size(90, 22);
        endpointUrlLabel.TabIndex = 2;
        endpointUrlLabel.Text = "Endpoint URL";
        endpointUrlLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // endpointUrlTextBox
        //
        endpointUrlTextBox.Location = new Point(108, 44);
        endpointUrlTextBox.Name = "endpointUrlTextBox";
        endpointUrlTextBox.Size = new Size(290, 23);
        endpointUrlTextBox.TabIndex = 3;
        //
        // bucketLabel
        //
        bucketLabel.Location = new Point(12, 72);
        bucketLabel.Name = "bucketLabel";
        bucketLabel.Size = new Size(90, 22);
        bucketLabel.TabIndex = 4;
        bucketLabel.Text = "Bucket";
        bucketLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // bucketTextBox
        //
        bucketTextBox.Location = new Point(108, 72);
        bucketTextBox.Name = "bucketTextBox";
        bucketTextBox.Size = new Size(290, 23);
        bucketTextBox.TabIndex = 5;
        //
        // accessKeyLabel
        //
        accessKeyLabel.Location = new Point(12, 100);
        accessKeyLabel.Name = "accessKeyLabel";
        accessKeyLabel.Size = new Size(90, 22);
        accessKeyLabel.TabIndex = 6;
        accessKeyLabel.Text = "Access key";
        accessKeyLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // accessKeyTextBox
        //
        accessKeyTextBox.Location = new Point(108, 100);
        accessKeyTextBox.Name = "accessKeyTextBox";
        accessKeyTextBox.Size = new Size(290, 23);
        accessKeyTextBox.TabIndex = 7;
        //
        // secretKeyLabel
        //
        secretKeyLabel.Location = new Point(12, 128);
        secretKeyLabel.Name = "secretKeyLabel";
        secretKeyLabel.Size = new Size(90, 22);
        secretKeyLabel.TabIndex = 8;
        secretKeyLabel.Text = "Secret key";
        secretKeyLabel.TextAlign = ContentAlignment.MiddleLeft;
        //
        // secretKeyTextBox
        //
        secretKeyTextBox.Location = new Point(108, 128);
        secretKeyTextBox.Name = "secretKeyTextBox";
        secretKeyTextBox.Size = new Size(290, 23);
        secretKeyTextBox.TabIndex = 9;
        secretKeyTextBox.UseSystemPasswordChar = true;
        //
        // okButton
        //
        okButton.DialogResult = DialogResult.OK;
        okButton.Location = new Point(218, 160);
        okButton.Name = "okButton";
        okButton.Size = new Size(90, 26);
        okButton.TabIndex = 10;
        okButton.Text = "OK";
        okButton.UseVisualStyleBackColor = true;
        //
        // cancelButton
        //
        cancelButton.DialogResult = DialogResult.Cancel;
        cancelButton.Location = new Point(314, 160);
        cancelButton.Name = "cancelButton";
        cancelButton.Size = new Size(90, 26);
        cancelButton.TabIndex = 11;
        cancelButton.Text = "Cancel";
        cancelButton.UseVisualStyleBackColor = true;
        //
        // S3ProfileEditForm
        //
        AcceptButton = okButton;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = cancelButton;
        ClientSize = new Size(420, 200);
        Controls.Add(nameLabel);
        Controls.Add(nameTextBox);
        Controls.Add(endpointUrlLabel);
        Controls.Add(endpointUrlTextBox);
        Controls.Add(bucketLabel);
        Controls.Add(bucketTextBox);
        Controls.Add(accessKeyLabel);
        Controls.Add(accessKeyTextBox);
        Controls.Add(secretKeyLabel);
        Controls.Add(secretKeyTextBox);
        Controls.Add(okButton);
        Controls.Add(cancelButton);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "S3ProfileEditForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "S3 profile";
        ResumeLayout(false);
        PerformLayout();
    }

    private Label nameLabel;
    private TextBox nameTextBox;
    private Label endpointUrlLabel;
    private TextBox endpointUrlTextBox;
    private Label bucketLabel;
    private TextBox bucketTextBox;
    private Label accessKeyLabel;
    private TextBox accessKeyTextBox;
    private Label secretKeyLabel;
    private TextBox secretKeyTextBox;
    private Button okButton;
    private Button cancelButton;
}
