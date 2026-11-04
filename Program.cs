namespace DatabaseTools;

static class Program
{
    [STAThread]
    static void Main()
    {
        Velopack.VelopackApp.Build().SetAutoApplyOnStartup(false).Run();
        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
