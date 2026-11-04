namespace DatabaseTools;

internal sealed class OperationUi
{
    private readonly Form _owner;
    private readonly Action<bool> _setBusy;
    private readonly Action<string>? _log;
    private Label? _status;
    private ProgressBar? _progress;
    private Button? _cancel;
    private CancellationTokenSource? _cancellation;
    private bool _closeRequested;

    public OperationUi(Form owner, Action<bool> setBusy, Action<string>? log = null)
    {
        _owner = owner;
        _setBusy = setBusy;
        _log = log;
        owner.FormClosing += (_, e) =>
        {
            if (_cancellation is null)
                return;

            e.Cancel = true;
            _closeRequested = true;
            Cancel();
        };
    }

    public void AttachProgress(Button action, Button? cancel = null)
    {
        var parent = action.Parent!;
        _cancel = cancel ?? new Button { Text = "Cancel", Size = new Size(82, action.Height) };
        _cancel.DialogResult = DialogResult.None;
        _cancel.Cursor = Cursors.Default;
        _cancel.Location = new Point(action.Right + 8, action.Top);
        _cancel.Visible = false;
        _cancel.Click += (_, _) => Cancel();
        parent.Controls.Add(_cancel);
        _status = new Label
        {
            Location = new Point(14, action.Bottom + 10), Size = new Size(parent.ClientSize.Width - 28, 24),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top, AutoEllipsis = true, Visible = false
        };
        _progress = new ProgressBar
        {
            Location = new Point(14, _status.Bottom + 2), Size = new Size(parent.ClientSize.Width - 28, 8),
            Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top, Style = ProgressBarStyle.Continuous, Visible = false
        };
        parent.Controls.AddRange([_status, _progress]);
        _owner.ClientSize = new Size(_owner.ClientSize.Width, _owner.ClientSize.Height + 48);
    }

    public async Task<bool> RunAsync(Func<CancellationToken, IProgress<OperationProgress>, Task> action, bool showProgress = false)
    {
        if (_cancellation is not null)
            return false;

        using var cancellation = new CancellationTokenSource();
        _cancellation = cancellation;
        _setBusy(true);
        if (_cancel is not null)
        {
            _cancel.Visible = showProgress;
            _cancel.Enabled = showProgress;
            _status!.Visible = showProgress;
            _status.Text = "Starting...";
            _progress!.Visible = false;
            _progress.Value = 0;
        }
        var progress = new Progress<OperationProgress>(value =>
        {
            if (!showProgress || _status is null || _cancellation != cancellation || cancellation.IsCancellationRequested)
                return;

            _status.Text = value.Stage;
            _progress!.Visible = value.BytesTransferred.HasValue && value.TotalBytes is > 0;
            if (value.BytesTransferred is { } bytes)
            {
                _status.Text += $" — {bytes / 1024d / 1024d:N1} MB";
                if (value.TotalBytes is > 0)
                {
                    var percent = (int)Math.Clamp(bytes * 100d / value.TotalBytes.Value, 0, 100);
                    _status.Text += $" / {value.TotalBytes.Value / 1024d / 1024d:N1} MB ({percent}%)";
                    _progress.Value = percent;
                    _progress.Visible = true;
                }
            }
        });

        try
        {
            await action(cancellation.Token, progress);
            if (_status is not null)
                _status.Text = "Completed";
            return true;
        }
        catch (OperationCanceledException ex) when (cancellation.IsCancellationRequested)
        {
            if (_status is not null)
                _status.Text = "Cancelled";
            _log?.Invoke(ex.Message);
            if (ex.InnerException is not null && !_closeRequested)
                Ui.ShowError(_owner, ex.Message);
            return false;
        }
        catch (Exception ex)
        {
            if (_status is not null)
                _status.Text = "Failed";
            _log?.Invoke("ERROR: " + ex.Message);
            Ui.ShowError(_owner, ex.Message);
            return false;
        }
        finally
        {
            _cancellation = null;
            if (_cancel is not null)
            {
                _cancel.Visible = false;
                _progress!.Visible = false;
            }
            _setBusy(false);
            if (_closeRequested)
            {
                _closeRequested = false;
                _owner.BeginInvoke(() => _owner.Close());
            }
        }
    }

    private void Cancel()
    {
        if (_cancel is not null)
        {
            _cancel.Enabled = false;
            _status!.Text = "Cancelling...";
        }
        _cancellation?.Cancel();
    }
}