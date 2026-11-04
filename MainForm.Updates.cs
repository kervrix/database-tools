using Velopack;
using Velopack.Sources;

namespace DatabaseTools;

public partial class MainForm
{
    private const string UpdateRepository = "https://github.com/kervrix/database-tools";
    private readonly ToolStripMenuItem _checkUpdates = new("Check for updates...");
    private readonly ToolStripButton _restartUpdate = new("Restart to update") { Visible = false };
    private UpdateManager? _updateManager;
    private VelopackAsset? _pendingUpdate;
    private bool _checkingUpdates;
    private bool _closingForUpdates;

    private void ConfigureUpdates()
    {
        settingsToolStripDropDownButton.DropDownItems.Add(new ToolStripSeparator());
        settingsToolStripDropDownButton.DropDownItems.Add(_checkUpdates);
        toolStrip.Items.Add(_restartUpdate);
        _checkUpdates.Click += async (_, _) => await CheckForUpdatesAsync(manual: true);
        _restartUpdate.Click += (_, _) => RestartToUpdate();
        FormClosed += (_, _) => _closingForUpdates = true;
    }

    private async Task CheckForUpdatesAsync(bool manual)
    {
        if (_checkingUpdates || _closingForUpdates)
            return;

        _checkingUpdates = true;
        _checkUpdates.Enabled = false;
        try
        {
            _updateManager ??= new UpdateManager(new GithubSource(UpdateRepository, null, false));
            if (!_updateManager.IsInstalled)
            {
                if (manual)
                    MessageBox.Show(this, "Install DatabaseTools using the Setup installer from GitHub Releases to enable automatic updates.",
                        "Updates", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            _pendingUpdate = _updateManager.UpdatePendingRestart;
            if (_pendingUpdate is null)
            {
                var update = await _updateManager.CheckForUpdatesAsync();
                if (_closingForUpdates || IsDisposed)
                    return;
                if (update is null)
                {
                    if (manual)
                        MessageBox.Show(this, "You are using the latest version.", "Updates", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Log($"Downloading update {update.TargetFullRelease.Version}...");
                await _updateManager.DownloadUpdatesAsync(update);
                if (_closingForUpdates || IsDisposed)
                    return;
                _pendingUpdate = update.TargetFullRelease;
            }

            _restartUpdate.Text = $"Restart to update ({_pendingUpdate.Version})";
            _restartUpdate.Visible = true;
            UpdateRestartButton();
            Log("Update downloaded. Restart to update when database operations are finished.");
        }
        catch (Exception ex)
        {
            if (!_closingForUpdates && !IsDisposed)
            {
                Log("Unable to check or download updates: " + ex.Message);
                if (manual)
                    Ui.ShowError(this, "Unable to check or download updates. Please try again later.");
            }
        }
        finally
        {
            _checkingUpdates = false;
            if (!_closingForUpdates && !IsDisposed)
                _checkUpdates.Enabled = true;
        }
    }

    private void UpdateRestartButton() => _restartUpdate.Enabled = !_busy && _pendingUpdate is not null;

    private void RestartToUpdate()
    {
        // Modal backup/restore/create dialogs disable their owner. Never force them to exit.
        if (_busy || !Enabled || Application.OpenForms.Count != 1 || _pendingUpdate is null || _updateManager is null)
            return;
        try
        {
            using var currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            var instances = System.Diagnostics.Process.GetProcessesByName(currentProcess.ProcessName);
            try
            {
                if (instances.Length > 1)
                {
                    MessageBox.Show(this, "Close other DatabaseTools windows before applying the update.",
                        "Updates", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
            finally
            {
                foreach (var instance in instances)
                    instance.Dispose();
            }
            CancelSizeLoading();
            _log.Flush();
            _updateManager.ApplyUpdatesAndRestart(_pendingUpdate);
        }
        catch (Exception ex)
        {
            Log("Unable to apply update: " + ex.Message);
            Ui.ShowError(this, "Unable to apply the update. Please try again later.");
        }
    }
}
