namespace DatabaseTools;

public sealed partial class BackupForm : Form
{
    private readonly string _databaseName;
    private readonly DatabaseConnectionOptions _connectionOptions;
    private readonly Action<string> _log;
    private readonly OperationUi _operation;
    private bool _busy;
    private readonly SettingsService _settingsService = new();
    private AppSettings _settings = new();

    public BackupForm(DatabaseConnectionOptions connectionOptions, string databaseName, string defaultBackupFolder, Action<string> log)
    {
        _databaseName = databaseName;
        _connectionOptions = connectionOptions;
        _log = log;

        InitializeComponent();
        backupButton.DialogResult = DialogResult.None;
        CancelButton = null;
        _operation = new OperationUi(this, SetBusy, log);
        _operation.AttachProgress(backupButton, cancelButton);
        backupButton.Click += async (_, _) => await _operation.RunAsync(BackupAsync, showProgress: true);
        ShowIcon = false;
        Text = $"Backup - {_databaseName}";
        fileNameTextBox.Text = $"{SanitizeFileName(_databaseName)}-{DateTime.Now:yyyyMMdd-HHmmss}.dump";
        localFileTextBox.Text = Path.Combine(defaultBackupFolder, fileNameTextBox.Text);
        Shown += async (_, _) => await _operation.RunAsync((token, _) => LoadSettingsAsync(token));
        ToggleSourceFields();
    }

    public BackupOptions GetBackupOptions()
    {
        if (localRadioButton.Checked)
        {
            return new BackupOptions(_databaseName, BackupSource.LocalFile, localFileTextBox.Text.Trim(), initCheckBox.Checked);
        }

        var profile = GetSelectedS3Profile()
            ?? throw new InvalidOperationException("Select S3 profile first.");
        var backupPath = S3BackupService.BuildBackupUrl($"{_databaseName}/{fileNameTextBox.Text.Trim()}", profile.S3Url);
        var credentials = new S3CredentialOptions(profile.AccessKey, _settingsService.UnprotectSecret(profile.ProtectedSecretKey));
        return new BackupOptions(_databaseName, BackupSource.S3, backupPath, initCheckBox.Checked, credentials);
    }

    private async Task BackupAsync(CancellationToken cancellationToken, IProgress<OperationProgress> progress)
    {
        ValidateInputs();
        var options = GetBackupOptions();
        _log($"Starting backup for {_databaseName} to {options.BackupPath}...");
        await DatabaseService.BackupDatabaseAsync(_connectionOptions, options, _log, cancellationToken, progress);
        _log($"Backup completed for {_databaseName}.");
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        backupButton.Enabled = !busy;
        initCheckBox.Enabled = !busy;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        ToggleSourceFields();
    }

    private async Task LoadSettingsAsync(CancellationToken cancellationToken)
    {
        _settings = await _settingsService.LoadAsync(cancellationToken);
        var profiles = _settings.S3Credentials
            .OrderBy(profile => profile.Name)
            .Select(profile => profile.Name);
        Ui.BindComboBox(s3ProfileComboBox, profiles, _settings.LastS3CredentialName);
    }

    private void browseButton_Click(object sender, EventArgs e)
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "PostgreSQL dump (*.dump)|*.dump|All files (*.*)|*.*",
            FileName = fileNameTextBox.Text.Trim(),
            Title = "Select backup file"
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            localFileTextBox.Text = dialog.FileName;
            fileNameTextBox.Text = Path.GetFileName(dialog.FileName);
        }
    }

    private void sourceRadioButton_CheckedChanged(object sender, EventArgs e)
    {
        ToggleSourceFields();
    }

    private void ToggleSourceFields()
    {
        var isS3 = s3RadioButton.Checked;
        localFileTextBox.Enabled = !isS3 && !_busy;
        browseButton.Enabled = !isS3 && !_busy;
        s3RadioButton.Enabled = !_busy;
        localRadioButton.Enabled = !_busy;
        s3ProfileComboBox.Enabled = isS3 && !_busy;
        fileNameTextBox.Enabled = isS3 && !_busy;
    }

    private void ValidateInputs()
    {
        if (localRadioButton.Checked && string.IsNullOrWhiteSpace(localFileTextBox.Text))
        {
            throw new InvalidOperationException("Local backup file is required.");
        }

        if (s3RadioButton.Checked && GetSelectedS3Profile() is null)
        {
            throw new InvalidOperationException("Select S3 profile first.");
        }

        if (s3RadioButton.Checked && string.IsNullOrWhiteSpace(fileNameTextBox.Text))
        {
            throw new InvalidOperationException("Backup file name is required.");
        }
    }

    private S3CredentialProfile? GetSelectedS3Profile()
    {
        return _settings.FindS3Credential(s3ProfileComboBox.Text.Trim());
    }

    private static string SanitizeFileName(string value)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(value.Select(ch => invalid.Contains(ch) ? '_' : ch));
    }
}