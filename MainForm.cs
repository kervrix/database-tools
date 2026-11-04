using System.Diagnostics;
using System.ComponentModel;

namespace DatabaseTools;

public partial class MainForm : Form
{
    private DatabaseConnectionOptions? _connectionOptions;
    private bool _shown;
    private bool _busy;
    private CancellationTokenSource? _sizeLoading;
    private readonly OperationLog _log = new();
    private readonly OperationUi _operation;

    public MainForm()
    {
        InitializeComponent();
        components ??= new System.ComponentModel.Container();
        components.Add(_log);
        _operation = new OperationUi(this, SetBusy, Log);
        FormClosing += (_, _) => CancelSizeLoading();
        ConfigureUpdates();
        ConfigureRestoreAll();
        databasesGrid.KeyDown += databasesGrid_KeyDown;
        deleteDatabaseMenuItem.ShortcutKeyDisplayString = "Delete";
        if (_log.FileError is not null)
            Log("Unable to create log file: " + _log.FileError);

        var logTimer = new System.Windows.Forms.Timer(components) { Interval = 100 };
        logTimer.Tick += (_, _) => FlushLog();
        logTimer.Start();
        if (Icon.ExtractAssociatedIcon(Application.ExecutablePath) is { } icon)
        {
            Icon = icon;
        }

        Ui.StyleToolStrip(toolStrip);
        Ui.StyleDivider(toolStripDivider);
        ApplyIcons();
        Text = "Databases";
        SetConnection(null);
    }

    private void ApplyIcons()
    {
        Ui.UseIconScaling(databaseContextMenu);
        Ui.UseIconScaling(settingsToolStripDropDownButton.DropDown);

        connectToolStripButton.Image = Icons.Get(Icons.Connect);
        addDatabaseToolStripButton.Image = Icons.Get(Icons.Add);
        refreshToolStripButton.Image = Icons.Get(Icons.Refresh);
        settingsToolStripDropDownButton.Image = Icons.Get(Icons.Settings);
        s3ProfilesMenuItem.Image = Icons.Get(Icons.Cloud);
        connectionProfilesMenuItem.Image = Icons.Get(Icons.Server);
        backupMenuItem.Image = Icons.Get(Icons.Backup);
        restoreMenuItem.Image = Icons.Get(Icons.Restore);
        shrinkMenuItem.Image = Icons.Get(Icons.Optimize);
        deleteDatabaseMenuItem.Image = Icons.Get(Icons.Delete, Icons.DangerColor);
        deleteDatabaseMenuItem.ForeColor = Icons.DangerColor;
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        if (_shown)
        {
            return;
        }

        _shown = true;
        await RefreshRestoreAllAvailabilityAsync();
        _ = CheckForUpdatesAsync(manual: false);
        if (_connectionOptions is null && !ShowConnectDialog())
        {
            Close();
            return;
        }

        await LoadDatabasesAsync();
    }

    private async Task LoadDatabasesAsync()
    {
        if (_connectionOptions is null)
        {
            return;
        }

        await RunUiTaskAsync((token, _) => LoadDatabasesCoreAsync(token));
    }

    private async Task LoadDatabasesCoreAsync(CancellationToken cancellationToken)
    {
        var connectionOptions = RequireConnectionOptions();
        CancelSizeLoading();
        var stopwatch = Stopwatch.StartNew();
        Log("Loading databases...");
        var databases = await DatabaseService.GetDatabasesAsync(connectionOptions, cancellationToken);
        var rows = new BindingList<DatabaseGridRow>(databases
            .Select(database => new DatabaseGridRow(
                database.Name,
                database.State,
                database.Encoding,
                database.Collation,
                database.SizeMb))
            .ToList());
        databasesGrid.DataSource = rows;
        Log($"Loaded {databases.Count} database(s) in {stopwatch.Elapsed.TotalSeconds:F2}s. Loading sizes in background...");
        UpdateDatabaseContextMenuState();
        _ = LoadDatabaseSizesAsync(connectionOptions, rows);
    }

    private async Task LoadDatabaseSizesAsync(DatabaseConnectionOptions options, BindingList<DatabaseGridRow> rows)
    {
        using var cancellation = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        _sizeLoading = cancellation;
        var stopwatch = Stopwatch.StartNew();
        try
        {
            await Task.Yield();
            cancellation.Token.ThrowIfCancellationRequested();
            var sizes = await DatabaseService.GetDatabaseSizesAsync(options, cancellation.Token);
            if (cancellation.IsCancellationRequested || IsDisposed || Disposing || databasesGrid.DataSource != rows)
                return;

            for (var i = 0; i < rows.Count; i++)
                if (sizes.TryGetValue(rows[i].Name, out var size))
                    rows[i] = rows[i] with { SizeMb = size };
            Log($"Loaded database sizes in {stopwatch.Elapsed.TotalSeconds:F2}s.");
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            if (_sizeLoading == cancellation && !IsDisposed && !Disposing)
                Log("Database size loading timed out. The database list remains available.");
        }
        catch (Exception ex)
        {
            if (_sizeLoading == cancellation && !IsDisposed && !Disposing)
                Log("Unable to load database sizes: " + ex.Message);
        }
        finally
        {
            if (_sizeLoading == cancellation)
                _sizeLoading = null;
        }
    }

    private void CancelSizeLoading()
    {
        var cancellation = _sizeLoading;
        _sizeLoading = null;
        cancellation?.Cancel();
    }

    private async void refreshToolStripButton_Click(object sender, EventArgs e)
    {
        await LoadDatabasesAsync();
    }

    private async void addDatabaseToolStripButton_Click(object sender, EventArgs e)
    {
        using var form = new CreateDatabaseForm(RequireConnectionOptions(), Log);
        form.ShowDialog(this);
        if (form.CreationAttempted)
            await RunUiTaskAsync((_, _) => RefreshAfterChangeAsync());
    }

    private void backupMenuItem_Click(object sender, EventArgs e)
    {
        var databaseName = GetSelectedDatabase();
        if (databaseName is null)
            return;

        var defaultBackupFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (string.IsNullOrWhiteSpace(defaultBackupFolder))
            defaultBackupFolder = Environment.CurrentDirectory;

        using var form = new BackupForm(RequireConnectionOptions(), databaseName, defaultBackupFolder, Log);
        form.ShowDialog(this);
        _log.Flush();
    }

    private async void restoreMenuItem_Click(object sender, EventArgs e)
    {
        var databaseName = GetSelectedDatabase();
        if (databaseName is null)
        {
            return;
        }

        using var form = new RestoreForm(RequireConnectionOptions(), databaseName, Log);
        form.ShowDialog(this);
        if (form.RestoreAttempted)
            await RunUiTaskAsync((_, _) => RefreshAfterChangeAsync());
    }

    private async void shrinkMenuItem_Click(object sender, EventArgs e)
    {
        var databaseName = GetSelectedDatabase();
        if (databaseName is null)
        {
            return;
        }

        var result = MessageBox.Show(
            this,
            $"Run VACUUM on database '{databaseName}'?",
            "Database Tools",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result != DialogResult.Yes)
        {
            return;
        }

        await RunUiTaskAsync(async (cancellationToken, _) =>
        {
            var connectionOptions = RequireConnectionOptions();
            Log($"Running VACUUM on {databaseName}...");
            await DatabaseService.ShrinkDatabaseAsync(connectionOptions, databaseName, Log, cancellationToken);
            await LoadDatabasesCoreAsync(cancellationToken);
            Log($"VACUUM completed for {databaseName}.");
            MessageBox.Show(this, $"VACUUM completed for {databaseName}.", "Database Tools", MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
    }

    private async void deleteDatabaseMenuItem_Click(object sender, EventArgs e)
    {
        var databaseName = GetSelectedDatabase();
        if (databaseName is null)
        {
            return;
        }

        var result = MessageBox.Show(
            this,
            $"Permanently delete database '{databaseName}'?\r\n\r\n"
                + "All of its data will be lost and any open connections to it will be closed. "
                + "This cannot be undone.",
            "Database Tools",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (result != DialogResult.Yes)
        {
            return;
        }

        await RunUiTaskAsync(async (cancellationToken, _) =>
        {
            var connectionOptions = RequireConnectionOptions();
            await DatabaseService.DropDatabaseAsync(connectionOptions, databaseName, Log, cancellationToken);
            await LoadDatabasesCoreAsync(cancellationToken);
            Log($"Database {databaseName} deleted.");
            MessageBox.Show(this, $"Database '{databaseName}' deleted.", "Database Tools", MessageBoxButtons.OK, MessageBoxIcon.Information);
        });
    }

    private void databasesGrid_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyData != Keys.Delete)
            return;

        e.Handled = true;
        e.SuppressKeyPress = true;
        if (!_busy && _connectionOptions is not null && GetSelectedDatabaseRowOrNull() is not null)
            deleteDatabaseMenuItem.PerformClick();
    }

    private void databasesGrid_SelectionChanged(object sender, EventArgs e)
    {
        UpdateDatabaseContextMenuState();
    }

    private void databasesGrid_MouseDown(object sender, MouseEventArgs e)
    {
        Ui.SelectRowUnderMouse(databasesGrid, e);
    }

    private void UpdateDatabaseContextMenuState()
    {
        var connected = _connectionOptions is not null;
        var hasSelection = GetSelectedDatabaseRowOrNull() is not null;
        var enabled = connected && !_busy && hasSelection;
        backupMenuItem.Enabled = enabled;
        restoreMenuItem.Enabled = enabled;
        shrinkMenuItem.Enabled = enabled;
        deleteDatabaseMenuItem.Enabled = enabled;
    }

    private string? GetSelectedDatabase()
    {
        return GetSelectedDatabaseRow()?.Name;
    }

    private DatabaseGridRow? GetSelectedDatabaseRowOrNull()
    {
        return databasesGrid.CurrentRow?.DataBoundItem as DatabaseGridRow;
    }

    private DatabaseGridRow? GetSelectedDatabaseRow()
    {
        var row = GetSelectedDatabaseRowOrNull();
        if (row is null)
        {
            MessageBox.Show(this, "Select a database first.", "Database Tools", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        return row;
    }

    private async Task RunUiTaskAsync(Func<CancellationToken, IProgress<OperationProgress>, Task> action)
    {
        await _operation.RunAsync(action);
        _log.Flush();
    }

    private async Task RefreshAfterChangeAsync()
    {
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
        try
        {
            await LoadDatabasesCoreAsync(timeout.Token);
        }
        catch (Exception ex)
        {
            Log("Unable to refresh the database list: " + ex.Message);
        }
    }

    private void SetBusy(bool busy)
    {
        if (busy)
            CancelSizeLoading();
        _busy = busy;
        _restoreAll.Enabled = !busy && _connectionOptions is not null;
        UpdateRestartButton();
        var connected = _connectionOptions is not null;
        connectToolStripButton.Enabled = !busy;
        addDatabaseToolStripButton.Enabled = !busy && connected;
        refreshToolStripButton.Enabled = !busy && connected;
        settingsToolStripDropDownButton.Enabled = !busy;
        Cursor = busy ? Cursors.WaitCursor : Cursors.Default;
        UpdateDatabaseContextMenuState();
    }

    private async void s3ProfilesMenuItem_Click(object sender, EventArgs e)
    {
        using var form = new S3CredentialsForm();
        form.ShowDialog(this);
        Log("S3 profiles updated.");
        await RefreshRestoreAllAvailabilityAsync();
    }

    private void connectionProfilesMenuItem_Click(object sender, EventArgs e)
    {
        using var form = new ConnectionProfilesForm();
        form.ShowDialog(this);
        Log("Connection profiles updated.");
    }

    private async void connectToolStripButton_Click(object sender, EventArgs e)
    {
        if (ShowConnectDialog())
        {
            await LoadDatabasesAsync();
        }
    }

    private bool ShowConnectDialog()
    {
        using var form = new ConnectForm();
        if (form.ShowDialog(this) != DialogResult.OK || form.ConnectionOptions is not { } connectionOptions)
        {
            return false;
        }

        SetConnection(connectionOptions);
        Log($"Connected to {connectionOptions.Server}.");
        return true;
    }

    private void SetConnection(DatabaseConnectionOptions? connectionOptions)
    {
        CancelSizeLoading();
        _connectionOptions = connectionOptions;
        databasesGrid.DataSource = null;
        Text = connectionOptions is null ? "Databases" : $"Databases - {connectionOptions.Server}";
        SetBusy(false);
    }

    private DatabaseConnectionOptions RequireConnectionOptions()
    {
        return _connectionOptions ?? throw new InvalidOperationException("Connect to PostgreSQL first.");
    }

    private void Log(string message) => _log.Write(message);

    private void FlushLog()
    {
        var batch = _log.ReadBatch();
        if (batch.Length == 0)
            return;

        logTextBox.AppendText(batch);
        const int maxLogLength = 200_000;
        if (logTextBox.TextLength > maxLogLength)
        {
            logTextBox.Select(0, logTextBox.TextLength - maxLogLength);
            logTextBox.SelectedText = string.Empty;
            logTextBox.SelectionStart = logTextBox.TextLength;
            logTextBox.ScrollToCaret();
        }
    }

    private sealed record DatabaseGridRow(
        string Name,
        string State,
        string Encoding,
        string Collation,
        decimal? SizeMb);
}
