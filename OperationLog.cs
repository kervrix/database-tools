using System.ComponentModel;
using System.Text;

namespace DatabaseTools;

public sealed class OperationLog : Component
{
    private const int MaxPendingLines = 1000;
    private const int MaxDisplayLineLength = 4000;
    private readonly object _sync = new();
    private readonly Queue<string> _pending = new();
    private StreamWriter? _writer;
    private int _omitted;
    private bool _reportedFileError;

    public string? FilePath { get; private set; }
    public string? FileError { get; private set; }

    public OperationLog(string? folder = null)
    {
        try
        {
            folder ??= Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Kervrix", "DatabaseTools", "Logs");
            Directory.CreateDirectory(folder);
            FilePath = Path.Combine(folder, $"{DateTime.Now:yyyyMMdd-HHmmss}-{Guid.NewGuid():N}.log");
            _writer = new StreamWriter(new FileStream(FilePath, FileMode.CreateNew, FileAccess.Write, FileShare.Read), new UTF8Encoding(false), 16 * 1024);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            FileError = ex.Message;
            FilePath = null;
        }
    }

    public void Write(string message)
    {
        var line = $"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}";
        lock (_sync)
        {
            try
            {
                _writer?.Write(line);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                FileError = ex.Message;
            }

            if (_pending.Count == MaxPendingLines)
            {
                _pending.Dequeue();
                _omitted++;
            }
            _pending.Enqueue(line.Length <= MaxDisplayLineLength ? line : line[..MaxDisplayLineLength] + "…" + Environment.NewLine);
        }
    }

    public string ReadBatch()
    {
        lock (_sync)
        {
            var batch = new StringBuilder();
            if (FileError is not null && !_reportedFileError)
            {
                batch.AppendLine("Unable to write the full log: " + FileError);
                _reportedFileError = true;
            }
            if (_omitted > 0)
            {
                batch.AppendLine($"[{_omitted} earlier messages omitted from the display. See the log file for full output.]");
                _omitted = 0;
            }
            for (var i = 0; i < 500 && _pending.TryDequeue(out var line); i++)
                batch.Append(line);
            return batch.ToString();
        }
    }

    public void Flush()
    {
        lock (_sync)
        {
            try
            {
                _writer?.Flush();
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                FileError = ex.Message;
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            lock (_sync)
            {
                try
                {
                    _writer?.Dispose();
                }
                catch (IOException ex)
                {
                    FileError = ex.Message;
                }
                _writer = null;
            }
        }
        base.Dispose(disposing);
    }
}