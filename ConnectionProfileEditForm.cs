namespace DatabaseTools;

public sealed partial class ConnectionProfileEditForm : Form
{
    public ConnectionProfileEditForm(string name, string server, string user, string password, bool isNew)
    {
        InitializeComponent();
        ShowIcon = false;
        Text = isNew ? "Add connection profile" : $"Edit connection profile - {name}";

        nameTextBox.Text = name;
        serverTextBox.Text = string.IsNullOrWhiteSpace(server) ? "localhost" : server;
        userTextBox.Text = string.IsNullOrWhiteSpace(user) ? "postgres" : user;
        passwordTextBox.Text = password;
    }

    public string ProfileName => nameTextBox.Text.Trim();
    public string Server => serverTextBox.Text.Trim();
    public string UserName => userTextBox.Text.Trim();
    public string Password => passwordTextBox.Text;

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
        Ui.Require(serverTextBox, "Server is required.");
        Ui.Require(userTextBox, "User is required.");
    }
}