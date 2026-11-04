namespace DatabaseTools;

public partial class RestoreForm : Form
{
    private readonly DatabaseConnectionOptions _connectionOptions;
    private readonly string _databaseName;
    private readonly Action<string> _log;
    private readonly SettingsService _settingsService = new();
    private readonly ComboBox _s3ProfileComboBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill, Margin = new Padding(0, 1, 6, 0) };
    private AppSettings _settings = new();
    private bool _busy;
    private bool _bindingS3Profiles;
    private readonly OperationUi _operation;

    public bool RestoreAttempted { get; private set; }

    public RestoreForm(DatabaseConnectionOptions connectionOptions, string databaseName, Action<string> log)
    {
        _connectionOptions = connectionOptions;
        _databaseName = databaseName;
        _log = log;

        InitializeComponent();
        _operation = new OperationUi(this, SetBusy, Log);
        _operation.AttachProgress(restoreButton);
        ShowIcon = false;
        ConfigureS3ProfileControls();
        WireEvents();
        s3SourceRadioButton.Checked = true;
        replaceCheckBox.Text = "Clean schemas before restore";
        replaceCheckBox.Checked = true;
        Text = $"Restore - {_databaseName}";
    }

    private void ConfigureS3ProfileControls()
    {
        s3ProfileLabel.Text = "S3 profile";

        s3ProfilePanel.ColumnCount = 2;
        s3ProfilePanel.ColumnStyles.Clear();
        s3ProfilePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        s3ProfilePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 0F));
        s3ProfilePanel.Controls.Add(_s3ProfileComboBox, 0, 0);
    }

    private void WireEvents()
    {
        Shown += async (_, _) => await _operation.RunAsync((token, _) => LoadInitialDataAsync(token));
        browseLocalFileButton.Click += (_, _) => BrowseLocalBackupFile();
        s3SourceRadioButton.CheckedChanged += (_, _) => ToggleSourceFields();
        localSourceRadioButton.CheckedChanged += (_, _) => ToggleSourceFields();
        _s3ProfileComboBox.SelectedIndexChanged += async (_, _) => await RefreshBackupsIfReadyAsync();
        restoreButton.Click += async (_, _) => await StartRestoreAsync();
        ToggleSourceFields();
    }

    private async Task LoadInitialDataAsync(CancellationToken cancellationToken)
    {
        _settings = await _settingsService.LoadAsync(cancellationToken);
        BindS3Profiles();
        if (GetSelectedS3CredentialProfile() is not null)
            await LoadS3BackupsAsync(cancellationToken);
    }

    private void BindS3Profiles()
    {
        _bindingS3Profiles = true;
        try
        {
            var profiles = _settings.S3Credentials
                .OrderBy(profile => profile.Name)
                .Select(profile => profile.Name);
            Ui.BindComboBox(_s3ProfileComboBox, profiles, _settings.LastS3CredentialName);
        }
        finally
        {
            _bindingS3Profiles = false;
        }
    }

    private async Task RefreshBackupsIfReadyAsync()
    {
        if (_busy || _bindingS3Profiles || !s3SourceRadioButton.Checked || GetSelectedS3CredentialProfile() is null)
        {
            return;
        }

        await _operation.RunAsync((token, _) => LoadS3BackupsAsync(token));
    }

    private async Task LoadS3BackupsAsync(CancellationToken cancellationToken)
    {
        var profile = GetSelectedS3CredentialProfile() ?? throw new InvalidOperationException("Select S3 profile first.");
        var s3Credentials = GetS3CredentialOptions();
        var previousBackup = GetSelectedBackup();
        backupComboBox.Items.Clear();

        Log($"Loading S3 backups for {_databaseName}...");
        var backups = await S3BackupService.GetBackupsAsync(_databaseName, profile.S3Url, s3Credentials, cancellationToken);
        var selectedBackup = backups.FirstOrDefault(item => item.Url.Equals(previousBackup, StringComparison.OrdinalIgnoreCase));
        Ui.BindComboBox(backupComboBox, backups, selectedBackup);
        Log($"Loaded {backups.Count} backup(s).");
    }

    private void ToggleSourceFields()
    {
        var isS3 = s3SourceRadioButton.Checked;
        s3ProfileLabel.Enabled = isS3;
        s3ProfilePanel.Enabled = isS3 && !_busy;
        backupFileLabel.Enabled = isS3;
        backupPanel.Enabled = isS3 && !_busy;
        localFileLabel.Enabled = !isS3;
        localFilePanel.Enabled = !isS3 && !_busy;
    }

    private void BrowseLocalBackupFile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "PostgreSQL dump (*.dump)|*.dump|All files (*.*)|*.*",
            Title = "Select PostgreSQL dump file"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            localFileTextBox.Text = dialog.FileName;
        }
    }

    private async Task StartRestoreAsync()
    {
        await Ui.RunAsync(this, async () =>
        {
            ValidateRestoreInputs();
            var options = GetRestoreOptions();
            if (options.Replace && MessageBox.Show(this,
                    $"Delete all user schemas in '{_databaseName}' before restoring? Existing data in those schemas will be removed.",
                    "Clean before restore", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            {
                return;
            }

            var restored = await _operation.RunAsync(async (token, progress) =>
            {
                RestoreAttempted = true;
                await DatabaseService.RestoreDatabaseAsync(_connectionOptions, options, Log, token, progress);
                Log("Restore completed.");
            }, showProgress: true);

            if (restored)
                MessageBox.Show(this, $"Database '{_databaseName}' restored successfully.", "Database Tools", MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
    }

    private RestoreOptions GetRestoreOptions()
    {
        return new RestoreOptions(
            _databaseName,
            GetBackupSource(),
            GetBackupPath(),
            replaceCheckBox.Checked,
            localSourceRadioButton.Checked ? null : GetS3CredentialOptions());
    }

    private BackupSource GetBackupSource()
    {
        return localSourceRadioButton.Checked ? BackupSource.LocalFile : BackupSource.S3;
    }

    private string GetBackupPath()
    {
        return localSourceRadioButton.Checked
            ? localFileTextBox.Text.Trim()
            : S3BackupService.BuildBackupUrl(GetSelectedBackup(), GetSelectedS3CredentialProfile()?.S3Url ?? string.Empty);
    }

    private string GetSelectedBackup()
    {
        return backupComboBox.SelectedItem is BackupHistoryItem backup
            ? backup.Url
            : backupComboBox.Text.Trim();
    }

    private S3CredentialProfile? GetSelectedS3CredentialProfile()
    {
        return _settings.FindS3Credential(_s3ProfileComboBox.Text.Trim());
    }

    private S3CredentialOptions GetS3CredentialOptions()
    {
        var profile = GetSelectedS3CredentialProfile()
            ?? throw new InvalidOperationException("Select S3 credential profile first.");

        return new S3CredentialOptions(
            profile.AccessKey,
            _settingsService.UnprotectSecret(profile.ProtectedSecretKey));
    }

    private void ValidateRestoreInputs()
    {
        if (localSourceRadioButton.Checked)
        {
            Ui.Require(localFileTextBox, "Local backup file is required.");
            return;
        }

        if (GetSelectedS3CredentialProfile() is null)
        {
            throw new InvalidOperationException("Select S3 credential profile first.");
        }

        Ui.Require(backupComboBox, "S3 backup is required.");
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        restoreButton.Enabled = !busy;
        replaceCheckBox.Enabled = !busy;
        s3SourceRadioButton.Enabled = !busy;
        localSourceRadioButton.Enabled = !busy;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        ToggleSourceFields();
    }

    private void Log(string message)
    {
        _log(message);
    }
}