using System.ComponentModel;

namespace DatabaseTools;

public sealed class RestoreAllForm : Form
{
    private readonly DatabaseConnectionOptions _connection;
    private readonly Action<string> _log;
    private readonly SettingsService _settingsService = new();
    private readonly ComboBox _profiles = new() { DropDownStyle = ComboBoxStyle.DropDownList, Left = 130, Top = 14, Width = 590 };
    private readonly ComboBox _mode = new() { DropDownStyle = ComboBoxStyle.DropDownList, Left = 130, Top = 48, Width = 590 };
    private readonly DataGridView _grid = new() { Left = 14, Top = 90, Width = 706, Height = 280, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false };
    private readonly Button _preview = new() { Text = "Load preview", Left = 14, Top = 385, Width = 110 };
    private readonly Button _restore = new() { Text = "Restore all", Left = 135, Top = 385, Width = 110 };
    private readonly OperationUi _operation;
    private readonly BindingList<RestoreRow> _rows = new();
    private AppSettings _settings = new();
    private IReadOnlyList<DatabaseBackupSummary> _backups = [];
    private HashSet<string> _existing = new(StringComparer.Ordinal);
    private S3CredentialProfile? _previewProfile;
    private bool _busy;

    public bool RestoreAttempted { get; private set; }

    public RestoreAllForm(DatabaseConnectionOptions connection, Action<string> log)
    {
        _connection = connection;
        _log = log;
        Text = "Restore all from S3";
        ClientSize = new Size(734, 425);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowIcon = false;
        StartPosition = FormStartPosition.CenterParent;
        Controls.AddRange([new Label { Text = "S3 profile", Left = 14, Top = 18, Width = 110 }, _profiles,
            new Label { Text = "Restore mode", Left = 14, Top = 52, Width = 110 }, _mode, _grid, _preview, _restore]);
        _mode.Items.AddRange(["All backups: overwrite existing and create missing databases", "Existing databases only: overwrite; skip missing databases"]);
        _mode.SelectedIndex = 0;
        _grid.DataSource = _rows;
        _operation = new OperationUi(this, SetBusy, log);
        _operation.AttachProgress(_restore);
        _profiles.SelectedIndexChanged += (_, _) => InvalidatePreview();
        _mode.SelectedIndexChanged += (_, _) => BuildPreview();
        _preview.Click += async (_, _) => await LoadPreviewAsync();
        _restore.Click += async (_, _) => await RestoreAsync();
        Shown += async (_, _) =>
        {
            await _operation.RunAsync(async (token, _) =>
            {
                _settings = await _settingsService.LoadAsync(token);
                Ui.BindComboBox(_profiles, _settings.S3Credentials.OrderBy(p => p.Name).Select(p => p.Name), _settings.LastS3CredentialName);
            });
        };
        SetBusy(false);
    }

    private void InvalidatePreview()
    {
        _previewProfile = null;
        _backups = [];
        _rows.Clear();
        SetBusy(_busy);
    }

    private async Task LoadPreviewAsync()
    {
        InvalidatePreview();
        await _operation.RunAsync(async (token, _) =>
        {
            var profile = _settings.FindS3Credential(_profiles.Text) ?? throw new InvalidOperationException("Select S3 profile first.");
            var credentials = Credentials(profile);
            var backups = await S3BackupService.GetLatestBackupsAsync(profile.S3Url, credentials, token);
            var databases = await DatabaseService.GetDatabasesAsync(_connection, token);
            _existing = databases.Select(d => d.Name).ToHashSet(StringComparer.Ordinal);
            _backups = backups;
            _previewProfile = profile;
            BuildPreview();
            _log($"Restore all preview: {backups.Count} S3 backup(s) on {_connection.Server}.");
        });
    }

    private void BuildPreview()
    {
        _rows.Clear();
        foreach (var backup in _backups)
        {
            var action = backup.DatabaseName == "app" ? "Skip"
                : _existing.Contains(backup.DatabaseName) ? "Overwrite" : _mode.SelectedIndex == 0 ? "Create" : "Skip";
            _rows.Add(new RestoreRow(backup.DatabaseName, action, backup.LatestBackup.FinishedAt, action == "Skip" ? "Skipped" : "Pending"));
        }
        SetBusy(_busy);
    }

    private async Task RestoreAsync()
    {
        if (_busy || _previewProfile is null || !_rows.Any(r => r.Action != "Skip"))
            return;
        var overwrite = _rows.Count(r => r.Action == "Overwrite");
        var create = _rows.Count(r => r.Action == "Create");
        if (MessageBox.Show(this,
                $"Server: {_connection.Server}\r\nS3 profile: {_previewProfile.Name}\r\n\r\nOverwrite {overwrite} database(s) and create {create} database(s) shown in the preview?\r\n\r\nAll user schemas and their data in databases marked Overwrite will be removed. The operation stops on the first error; completed restores are not rolled back.",
                "Confirm restore all", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes)
            return;

        var profile = _previewProfile;
        var completed = 0;
        var current = -1;
        var success = await _operation.RunAsync(async (token, progress) =>
        {
            try
            {
                var credentials = Credentials(profile);
                var currentDatabases = (await DatabaseService.GetDatabasesAsync(_connection, token)).Select(d => d.Name).ToHashSet(StringComparer.Ordinal);
                if (_backups.Any(b => currentDatabases.Contains(b.DatabaseName) != _existing.Contains(b.DatabaseName)))
                    throw new InvalidOperationException("The database list changed. Load the preview again before restoring.");
                for (var i = 0; i < _rows.Count; i++)
                {
                    if (_rows[i].Action == "Skip")
                        continue;
                    token.ThrowIfCancellationRequested();
                    current = i;
                    var row = _rows[i];
                    _rows[i] = row with { Status = "Restoring" };
                    RestoreAttempted = true;
                    _log($"Restore all: {row.Action} {row.Database} ({completed + 1}/{overwrite + create}).");
                    if (row.Action == "Create")
                        await DatabaseService.CreateDatabaseAsync(_connection, row.Database, token);
                    var itemProgress = new ForwardProgress(p => progress.Report(p with { Stage = $"{completed + 1}/{overwrite + create} {row.Database}: {p.Stage}" }));
                    await DatabaseService.RestoreDatabaseAsync(_connection,
                        new RestoreOptions(row.Database, BackupSource.S3, _backups[i].LatestBackup.Url, row.Action == "Overwrite", credentials), _log, token, itemProgress);
                    _rows[i] = row with { Status = "Completed" };
                    completed++;
                    current = -1;
                }
            }
            catch (Exception ex)
            {
                if (current >= 0)
                    _rows[current] = _rows[current] with { Status = ex is OperationCanceledException ? "Cancelled; review database" : "Failed; review database" };
                _log($"Restore all stopped: {completed} completed. Remaining databases were not processed.");
                throw;
            }
            finally
            {
                // Require a fresh preview before any retry, especially after partial creation.
                _previewProfile = null;
            }
        }, showProgress: true);
        if (success)
            MessageBox.Show(this, $"Restored {completed} database(s).", "Restore all", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private S3CredentialOptions Credentials(S3CredentialProfile profile) => new(profile.AccessKey, _settingsService.UnprotectSecret(profile.ProtectedSecretKey));

    private void SetBusy(bool busy)
    {
        _busy = busy;
        _profiles.Enabled = !busy;
        _mode.Enabled = !busy;
        _preview.Enabled = !busy && _profiles.SelectedIndex >= 0;
        _restore.Enabled = !busy && _previewProfile is not null && _rows.Any(r => r.Action != "Skip");
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
    }

    private sealed record RestoreRow(string Database, string Action, DateTime? BackupDate, string Status);
    private sealed class ForwardProgress(Action<OperationProgress> report) : IProgress<OperationProgress>
    {
        public void Report(OperationProgress value) => report(value);
    }
}
