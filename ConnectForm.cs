namespace DatabaseTools;

public partial class ConnectForm : Form
{
    private readonly SettingsService _settingsService = new();
    private AppSettings _settings = new();
    private bool _isLoadingProfile;
    private readonly OperationUi _operation;

    public DatabaseConnectionOptions? ConnectionOptions { get; private set; }

    public ConnectForm()
    {
        InitializeComponent();
        _operation = new OperationUi(this, SetBusy);
        ShowIcon = false;
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await _operation.RunAsync((token, _) => LoadSettingsAsync(token));
    }

    private async Task LoadSettingsAsync(CancellationToken cancellationToken)
    {
        _settings = await _settingsService.LoadAsync(cancellationToken);
        BindProfiles();
    }

    private void BindProfiles()
    {
        _isLoadingProfile = true;
        var profiles = _settings.SqlConnections
            .OrderBy(profile => profile.Name)
            .Select(profile => profile.Name);
        Ui.BindComboBox(profileComboBox, profiles, _settings.LastSqlConnectionName);

        _isLoadingProfile = false;
        ApplySelectedProfile();
    }

    private void profileComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_isLoadingProfile)
            return;

        try
        {
            ApplySelectedProfile();
        }
        catch (Exception ex)
        {
            Ui.ShowError(this, ex.Message);
        }
    }

    private void windowsAuthCheckBox_CheckedChanged(object sender, EventArgs e)
    {
        ToggleSqlAuthFields();
    }

    private void ApplySelectedProfile()
    {
        var profile = _settings.FindSqlConnection(profileComboBox.Text.Trim());
        if (profile is null)
        {
            ToggleSqlAuthFields();
            return;
        }

        serverTextBox.Text = string.IsNullOrWhiteSpace(profile.Server) ? "localhost" : profile.Server;
        windowsAuthCheckBox.Checked = false;
        sqlUserTextBox.Text = string.IsNullOrWhiteSpace(profile.UserName) ? "postgres" : profile.UserName;
        sqlPasswordTextBox.Text = _settingsService.UnprotectSecret(profile.ProtectedPassword);
        ToggleSqlAuthFields();
    }

    private void ToggleSqlAuthFields()
    {
        windowsAuthCheckBox.Checked = false;
        sqlUserTextBox.Enabled = true;
        sqlPasswordTextBox.Enabled = true;
    }

    private async void connectButton_Click(object sender, EventArgs e)
    {
        var connected = await _operation.RunAsync(async (cancellationToken, _) =>
        {
            ValidateConnectionInputs();
            var options = GetConnectionOptions();
            await DatabaseService.TestConnectionAsync(options, cancellationToken);
            await SaveCurrentProfileAsync(cancellationToken);
            ConnectionOptions = options;
        });

        if (connected)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    private async Task SaveCurrentProfileAsync(CancellationToken cancellationToken)
    {
        var profileName = profileComboBox.Text.Trim();
        var profile = _settings.FindSqlConnection(profileName);
        if (profile is null)
        {
            profile = new SqlConnectionProfile { Name = profileName };
            _settings.SqlConnections.Add(profile);
        }

        profile.Server = serverTextBox.Text.Trim();
        profile.WindowsAuth = false;
        profile.UserName = sqlUserTextBox.Text.Trim();
        profile.ProtectedPassword = _settingsService.ProtectSecret(sqlPasswordTextBox.Text);
        _settings.LastSqlConnectionName = profile.Name;

        await _settingsService.SaveAsync(_settings, cancellationToken);
        BindProfiles();
        profileComboBox.SelectedItem = profile.Name;
    }

    private void SetBusy(bool busy)
    {
        profileComboBox.Enabled = !busy;
        serverTextBox.Enabled = !busy;
        sqlUserTextBox.Enabled = !busy;
        sqlPasswordTextBox.Enabled = !busy;
        connectButton.Enabled = !busy;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }

    private DatabaseConnectionOptions GetConnectionOptions()
    {
        return new DatabaseConnectionOptions(
            serverTextBox.Text.Trim(),
            false,
            sqlUserTextBox.Text.Trim(),
            sqlPasswordTextBox.Text);
    }

    private void ValidateConnectionInputs()
    {
        Ui.Require(profileComboBox, "Profile name is required.");
        Ui.Require(serverTextBox, "PostgreSQL server is required.");

        Ui.Require(sqlUserTextBox, "PostgreSQL user is required.");
        Ui.Require(sqlPasswordTextBox, "PostgreSQL password is required.");
    }
}