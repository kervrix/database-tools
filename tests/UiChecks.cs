using System.Reflection;
using DatabaseTools;

internal static partial class Program
{
    private static void RenderForms()
    {
        var connection = new DatabaseConnectionOptions("localhost", false, "test", "");
        var forms = new Form[]
        {
            new MainForm(), new ConnectForm(), new CreateDatabaseForm(connection, _ => { }), new RestoreAllForm(connection, _ => { }),
            new BackupForm(connection, "example", TestFolder, _ => { }),
            new RestoreForm(connection, "example", _ => { }),
            new S3ProfileEditForm("S3", "https://example.test", "bucket/postgres", "", "", false)
        };
        foreach (var form in forms)
        {
            using (form)
            {
                var settings = form.GetType().GetField("_settingsService", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(form);
                if (settings is not null)
                    typeof(SettingsService).GetField("_settingsPath", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(settings, Path.Combine(TestFolder, "no-settings.json"));
                if (form is MainForm)
                    typeof(MainForm).GetField("_shown", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(form, true);
                form.ShowInTaskbar = false;
                form.Opacity = 0;
                Exception? failure = null;
                form.Shown += async (_, _) =>
                {
                    try
                    {
                        var operation = form.GetType().GetField("_operation", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(form);
                        while (operation?.GetType().GetField("_cancellation", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(operation) is not null)
                            await Task.Delay(10);
                        SaveForm(form, form.GetType().Name);
                        if (form is RestoreForm or BackupForm)
                        {
                            var task = RunOperation(operation!, async (token, progress) =>
                            {
                                progress.Report(new OperationProgress("Transferring backup", 512 * 1024, 1024 * 1024));
                                await Task.Delay(Timeout.Infinite, token);
                            }, true);
                            await Task.Delay(50);
                            SaveForm(form, form.GetType().Name + "-progress");
                            operation!.GetType().GetMethod("Cancel", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(operation, null);
                            await task;
                        }
                    }
                    catch (Exception ex)
                    {
                        failure = ex;
                    }
                    finally
                    {
                        form.Close();
                    }
                };
                Application.Run(form);
                if (failure is not null)
                    throw failure;
            }
        }
    }

    private static void SaveForm(Form form, string name)
    {
        using var bitmap = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
        var path = Path.Combine(TestFolder, name + ".png");
        bitmap.Save(path);
        Console.WriteLine(path);
    }

    private static Task<bool> RunOperation(object operation, Func<CancellationToken, IProgress<OperationProgress>, Task> action, bool showProgress)
        => (Task<bool>)operation.GetType().GetMethod("RunAsync")!.Invoke(operation, [action, showProgress])!;

    private static void PumpUntil(Task task)
    {
        var timeout = DateTime.UtcNow.AddSeconds(5);
        while (!task.IsCompleted && DateTime.UtcNow < timeout)
        {
            Application.DoEvents();
            Thread.Sleep(5);
        }
        if (!task.IsCompleted)
            throw new Exception("UI operation did not finish within five seconds.");
        task.GetAwaiter().GetResult();
    }

    private static void CheckOperationUi()
    {
        using var form = new Form { ClientSize = new Size(500, 200), ShowInTaskbar = false, Opacity = 0 };
        form.Show();
        var type = typeof(MainForm).Assembly.GetType("DatabaseTools.OperationUi")!;
        var busy = false;
        var operation = Activator.CreateInstance(type, form, (Action<bool>)(value => busy = value), null)!;
        Func<CancellationToken, IProgress<OperationProgress>, Task> action = async (token, progress) =>
        {
            progress.Report(new OperationProgress("Downloading", 512, 1024));
            await Task.Delay(Timeout.Infinite, token);
        };
        var ordinary = RunOperation(operation, action, false);
        Application.DoEvents();
        Assert(busy && form.Controls.Count == 0, "Ordinary operations add no progress or cancellation controls.");
        type.GetMethod("Cancel", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(operation, null);
        PumpUntil(ordinary);

        var restore = new Button { Text = "Restore", Location = new Point(120, 100) };
        form.Controls.Add(restore);
        type.GetMethod("AttachProgress")!.Invoke(operation, [restore, null]);
        var cancel = form.Controls.OfType<Button>().Single(button => button != restore);
        var progressBar = form.Controls.OfType<ProgressBar>().Single();
        var status = form.Controls.OfType<Label>().Single();
        IProgress<OperationProgress>? reporter = null;
        var task = RunOperation(operation, async (token, progress) =>
        {
            reporter = progress;
            progress.Report(new OperationProgress("Downloading", 512, 1024));
            await Task.Delay(Timeout.Infinite, token);
        }, true);
        Application.DoEvents();
        Assert(cancel.Visible && cancel.Top == restore.Top && cancel.Left > restore.Right, "Cancel sits next to the operation button.");
        Assert(progressBar.Visible && progressBar.Value == 50, "Transfer progress shows the measured percentage.");
        reporter!.Report(new OperationProgress("Restoring database"));
        Application.DoEvents();
        Assert(!progressBar.Visible && status.Text == "Restoring database", "Unknown progress shows a stage without a spinning bar.");
        cancel.PerformClick();
        PumpUntil(task);
        Assert(!task.Result && !busy && !cancel.Visible, "Cancel returns the UI to an idle state.");

        var next = RunOperation(operation, action, false);
        Application.DoEvents();
        Assert(!cancel.Visible && !status.Visible, "Ordinary actions hide backup controls.");
        typeof(Form).GetMethod("OnFormClosing", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, [new FormClosingEventArgs(CloseReason.UserClosing, false)]);
        PumpUntil(next);
        Assert(!next.Result, "Closing a window cancels and awaits its operation.");
        Application.DoEvents();

        using var restoreForm = new RestoreForm(new DatabaseConnectionOptions("localhost", false, "test", ""), "chat", _ => { });
        var clean = (CheckBox)typeof(RestoreForm).GetField("replaceCheckBox", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(restoreForm)!;
        Assert(clean.Checked, "Restore cleans existing schemas by default.");
        CheckRestoreAllPreview();
    }

    private static void CheckRestoreAllPreview()
    {
        using var form = new RestoreAllForm(new DatabaseConnectionOptions("localhost", false, "test", ""), _ => { });
        var type = typeof(RestoreAllForm);
        object Field(string name) => type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(form)!;
        void Set(string name, object value) => type.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(form, value);
        void Invoke(string name, params object[] args) => type.GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(form, args);
        var restore = (Button)Field("_restore");
        Assert(!restore.Enabled, "Bulk restore cannot start before preview is loaded.");
        Set("_previewProfile", new S3CredentialProfile { Name = "test" });
        Set("_existing", new HashSet<string>(["Existing", "app"], StringComparer.Ordinal));
        Set("_backups", new List<DatabaseBackupSummary>
        {
            new("Existing", new BackupHistoryItem("https://example.test/bucket/Existing/latest.dump", null, null)),
            new("existing", new BackupHistoryItem("https://example.test/bucket/existing/latest.dump", null, null)),
            new("app", new BackupHistoryItem("https://example.test/bucket/app/latest.dump", null, null))
        });
        Invoke("BuildPreview");
        string[] Actions() => ((System.Collections.IEnumerable)Field("_rows")).Cast<object>()
            .Select(row => (string)row.GetType().GetProperty("Action")!.GetValue(row)!).ToArray();
        Assert(Actions().SequenceEqual(["Overwrite", "Create", "Skip"]) && restore.Enabled,
            "All mode overwrites existing and creates missing databases using case-sensitive names.");
        ((ComboBox)Field("_mode")).SelectedIndex = 1;
        Assert(Actions().SequenceEqual(["Overwrite", "Skip", "Skip"]), "Existing-only mode skips missing databases and app.");
        Set("_existing", new HashSet<string>(["Existing"], StringComparer.Ordinal));
        ((ComboBox)Field("_mode")).SelectedIndex = 0;
        Assert(Actions().SequenceEqual(["Overwrite", "Create", "Skip"]), "All mode skips app even when it does not exist.");
        Invoke("SetBusy", true);
        Assert(!restore.Enabled && !((ComboBox)Field("_mode")).Enabled, "Bulk restore locks mode and start button while busy.");
        Invoke("SetBusy", false);
        Invoke("InvalidatePreview");
        Assert(!restore.Enabled && Actions().Length == 0, "Changing profile invalidates the previous restore plan.");
        Set("_previewProfile", new S3CredentialProfile { Name = "empty" });
        Invoke("BuildPreview");
        Assert(!restore.Enabled, "An empty S3 backup list cannot be restored.");
    }
}
