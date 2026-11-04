namespace DatabaseTools;

public sealed partial class S3ProfileEditForm : Form
{
    public S3ProfileEditForm(string name, string endpointUrl, string bucket, string accessKey, string secretKey, bool isNew)
    {
        InitializeComponent();
        ShowIcon = false;
        bucketLabel.Text = "Bucket / prefix";
        Text = isNew ? "Add S3 profile" : $"Edit S3 profile - {name}";

        nameTextBox.Text = name;
        endpointUrlTextBox.Text = endpointUrl;
        bucketTextBox.Text = bucket;
        accessKeyTextBox.Text = accessKey;
        secretKeyTextBox.Text = secretKey;
    }

    public string ProfileName => nameTextBox.Text.Trim();
    public string EndpointUrl => endpointUrlTextBox.Text.Trim().TrimEnd('/');
    public string Bucket => bucketTextBox.Text.Trim().Trim('/');
    public string AccessKey => accessKeyTextBox.Text.Trim();
    public string SecretKey => secretKeyTextBox.Text;

    public string BuildS3Url() => $"{EndpointUrl}/{Bucket}";

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (DialogResult == DialogResult.OK)
        {
            try
            {
                ValidateInputs();
            }
            catch (Exception ex)
            {
                Ui.ShowError(this, ex.Message);
                e.Cancel = true;
            }
        }

        base.OnFormClosing(e);
    }

    private void ValidateInputs()
    {
        Ui.Require(nameTextBox, "Name is required.");
        Ui.Require(endpointUrlTextBox, "Endpoint URL is required.");
        Ui.Require(bucketTextBox, "Bucket is required.");
    }
}