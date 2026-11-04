namespace DatabaseTools;

public partial class MainForm
{
    private readonly ToolStripButton _restoreAll = new("Restore all") { Visible = false };

    private void ConfigureRestoreAll()
    {
        _restoreAll.Image = Icons.Get(Icons.Restore);
        toolStrip.Items.Insert(toolStrip.Items.IndexOf(refreshToolStripButton), _restoreAll);
        _restoreAll.Click += async (_, _) =>
        {
            if (_busy || _connectionOptions is null)
                return;
            using var form = new RestoreAllForm(RequireConnectionOptions(), Log);
            form.ShowDialog(this);
            if (form.RestoreAttempted)
                await RunUiTaskAsync((_, _) => RefreshAfterChangeAsync());
        };
    }

    private async Task RefreshRestoreAllAvailabilityAsync()
    {
        try
        {
            var settings = await new SettingsService().LoadAsync(CancellationToken.None);
            if (IsDisposed || Disposing)
                return;
            _restoreAll.Visible = settings.S3Credentials.Count > 0;
            _restoreAll.Enabled = !_busy && _connectionOptions is not null;
        }
        catch (Exception ex)
        {
            if (!IsDisposed && !Disposing)
            {
                _restoreAll.Visible = false;
                Log("Unable to load S3 profiles: " + ex.Message);
            }
        }
    }
}
