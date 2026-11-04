namespace DatabaseTools;

public sealed partial class CreateDatabaseForm : Form
{
    private readonly DatabaseConnectionOptions _connectionOptions;
    private readonly Action<string> _log;
    private readonly SettingsService _settingsService = new();
    private AppSettings _settings = new();
    private bool _busy;
    private bool _bindingS3Profiles;
    private readonly OperationUi _operation;

    public bool CreationAttempted { get; private set; }

    public CreateDatabaseForm(DatabaseConnectionOptions connectionOptions, Action<string> log)
    {
        _connectionOptions = connectionOptions;
        _log = log;
        InitializeComponent();
        okButton.DialogResult = DialogResult.None;
        okButton.Text = "Create";
        CancelButton = null;
        _operation = new OperationUi(this, SetBusy, log);
        _operation.AttachProgress(okButton, cancelButton);
        okButton.Click += async (_, _) => await CreateAsync();
        ShowIcon = false;
        Text = "Add database";

        Shown += async (_, _) => await _operation.RunAsync(LoadSettingsAsync);
        justNameRadioButton.CheckedChanged += (_, _) => ToggleModeFields();
        s3RadioButton.CheckedChanged += async (_, _) =>
        {
            ToggleModeFields();
            await RefreshBackupsIfReadyAsync();
        };
        s3ProfileComboBox.SelectedIndexChanged += async (_, _) => await RefreshBackupsIfReadyAsync();
        backupComboBox.SelectedIndexChanged += (_, _) => ApplyBackupDatabaseName();
        ToggleModeFields();
    }

    public string DatabaseName => databaseNameTextBox.Text.Trim();

    public RestoreOptions? GetRestoreOptions(string databaseName)
    {
        if (!s3RadioButton.Checked)
        {
            return null;
        }

        var summary = backupComboBox.SelectedItem as DatabaseBackupSummary
            ?? throw new InvalidOperationException("Select a backup first.");
        var profile = GetSelectedS3Profile()
            ?? throw new InvalidOperationException("Select S3 profile first.");
        var credentials = new S3CredentialOptions(profile.AccessKey, _settingsService.UnprotectSecret(profile.ProtectedSecretKey));

        return new RestoreOptions(
            databaseName,
            BackupSource.S3,
            summary.LatestBackup.Url,
            false,
            credentials);
    }

    private async Task CreateAsync()
    {
        var completed = await _operation.RunAsync(async (token, progress) =>
        {
            ValidateInputs();
            var databaseName = DatabaseName;
            var restoreOptions = GetRestoreOptions(databaseName);
            var created = false;
            try
            {
                CreationAttempted = true;
                _log($"Creating database {databaseName}...");
                await DatabaseService.CreateDatabaseAsync(_connectionOptions, databaseName, token);
                created = true;
                if (restoreOptions is not null)
                    await DatabaseService.RestoreDatabaseAsync(_connectionOptions, restoreOptions, _log, token, progress);
                _log($"Database {databaseName} created.");
            }
            catch (OperationCanceledException ex) when (created)
            {
                throw new OperationCanceledException($"Operation cancelled. Database '{databaseName}' was created, but the restore did not finish.\r\n\r\n{ex.Message}", ex, token);
            }
            catch (Exception ex) when (created && ex is not OperationCanceledException)
            {
                throw new InvalidOperationException($"Database '{databaseName}' was created, but the restore did not finish.\r\n\r\n{ex.Message}", ex);
            }
        }, showProgress: s3RadioButton.Checked);

        if (completed)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }

    private async Task LoadSettingsAsync(CancellationToken cancellationToken, IProgress<OperationProgress> progress)
    {
        _settings = await _settingsService.LoadAsync(cancellationToken);
        var profiles = _settings.S3Credentials
            .OrderBy(profile => profile.Name)
            .Select(profile => profile.Name);
        _bindingS3Profiles = true;
        try
        {
            Ui.BindComboBox(s3ProfileComboBox, profiles, _settings.LastS3CredentialName);
        }
        finally
        {
            _bindingS3Profiles = false;
        }

        if (s3RadioButton.Checked && GetSelectedS3Profile() is not null)
            await LoadBackupsAsync(cancellationToken, progress);
    }

    private async Task RefreshBackupsIfReadyAsync()
    {
        if (_busy || _bindingS3Profiles || !s3RadioButton.Checked || GetSelectedS3Profile() is null)
        {
            return;
        }

        await _operation.RunAsync(LoadBackupsAsync);
    }

    private async Task LoadBackupsAsync(CancellationToken cancellationToken, IProgress<OperationProgress> progress)
    {
        var profile = GetSelectedS3Profile() ?? throw new InvalidOperationException("Select S3 profile first.");
        backupComboBox.Items.Clear();
        var credentials = new S3CredentialOptions(profile.AccessKey, _settingsService.UnprotectSecret(profile.ProtectedSecretKey));
        var summaries = await S3BackupService.GetLatestBackupsAsync(profile.S3Url, credentials, cancellationToken);
        Ui.BindComboBox(backupComboBox, summaries);
        ApplyBackupDatabaseName();
    }

    private void ApplyBackupDatabaseName()
    {
        if (s3RadioButton.Checked && backupComboBox.SelectedItem is DatabaseBackupSummary summary)
        {
            databaseNameTextBox.Text = summary.DatabaseName;
        }
    }

    private void ToggleModeFields()
    {
        var isS3 = s3RadioButton.Checked;
        databaseNameTextBox.ReadOnly = isS3;
        justNameRadioButton.Enabled = !_busy;
        s3RadioButton.Enabled = !_busy;
        s3ProfileLabel.Enabled = isS3;
        s3ProfileComboBox.Enabled = isS3 && !_busy;
        backupLabel.Enabled = isS3;
        backupComboBox.Enabled = isS3 && !_busy;

        if (isS3)
        {
            ApplyBackupDatabaseName();
        }
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        okButton.Enabled = !busy;
        databaseNameTextBox.Enabled = !busy;
        ToggleModeFields();
    }

    private S3CredentialProfile? GetSelectedS3Profile()
    {
        return _settings.FindS3Credential(s3ProfileComboBox.Text.Trim());
    }

    private void ValidateInputs()
    {
        if (!s3RadioButton.Checked)
        {
            Ui.Require(databaseNameTextBox, "Database name is required.");
            return;
        }

        if (GetSelectedS3Profile() is null)
        {
            throw new InvalidOperationException("Select S3 profile first.");
        }

        if (backupComboBox.SelectedItem is not DatabaseBackupSummary)
        {
            throw new InvalidOperationException("Select a backup first.");
        }
    }
}