namespace DatabaseTools;

public partial class ConnectionProfilesForm : Form
{
    private readonly SettingsService _settingsService = new();
    private AppSettings _settings = new();

    public ConnectionProfilesForm()
    {
        InitializeComponent();
        ShowIcon = false;
        Ui.StyleToolStrip(toolStrip);
        Ui.StyleDivider(toolStripDivider);
        Ui.UseIconScaling(profileContextMenu);
        newToolStripButton.Image = Icons.Get(Icons.Add);
        editProfileMenuItem.Image = Icons.Get(Icons.Edit);
        deleteProfileMenuItem.Image = Icons.Get(Icons.Delete, Icons.DangerColor);
        deleteProfileMenuItem.ForeColor = Icons.DangerColor;
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await Ui.RunAsync(this, LoadSettingsAsync);
    }

    private async Task LoadSettingsAsync()
    {
        _settings = await _settingsService.LoadAsync(CancellationToken.None);
        BindGrid();
    }

    private void BindGrid()
    {
        profilesGrid.DataSource = _settings.SqlConnections
            .OrderBy(profile => profile.Name)
            .ToList();
        UpdateMenuState();
    }

    private void profilesGrid_SelectionChanged(object sender, EventArgs e)
    {
        UpdateMenuState();
    }

    private async void profilesGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0)
        {
            await Ui.RunAsync(this, EditSelectedProfileAsync);
        }
    }

    private void profilesGrid_MouseDown(object sender, MouseEventArgs e)
    {
        Ui.SelectRowUnderMouse(profilesGrid, e);
    }

    private void UpdateMenuState()
    {
        var hasSelection = GetSelectedProfile() is not null;
        editProfileMenuItem.Enabled = hasSelection;
        deleteProfileMenuItem.Enabled = hasSelection;
    }

    private SqlConnectionProfile? GetSelectedProfile()
    {
        return profilesGrid.CurrentRow?.DataBoundItem as SqlConnectionProfile;
    }

    private async void newToolStripButton_Click(object sender, EventArgs e)
    {
        await Ui.RunAsync(this, CreateProfileAsync);
    }

    private async void editProfileMenuItem_Click(object sender, EventArgs e)
    {
        await Ui.RunAsync(this, EditSelectedProfileAsync);
    }

    private async void deleteProfileMenuItem_Click(object sender, EventArgs e)
    {
        await Ui.RunAsync(this, DeleteSelectedProfileAsync);
    }

    private async Task CreateProfileAsync()
    {
        using var form = new ConnectionProfileEditForm(string.Empty, string.Empty, string.Empty, string.Empty, isNew: true);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        if (_settings.FindSqlConnection(form.ProfileName) is not null)
        {
            MessageBox.Show(this, $"Profile \"{form.ProfileName}\" already exists.", "Database Tools", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var profile = new SqlConnectionProfile
        {
            Name = form.ProfileName,
            Server = form.Server,
            UserName = form.UserName,
            ProtectedPassword = _settingsService.ProtectSecret(form.Password)
        };

        _settings.SqlConnections.Add(profile);
        _settings.LastSqlConnectionName = profile.Name;

        await _settingsService.SaveAsync(_settings, CancellationToken.None);
        BindGrid();
        SelectProfile(profile.Name);
        MessageBox.Show(this, $"Profile \"{profile.Name}\" saved.", "Database Tools", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task EditSelectedProfileAsync()
    {
        var profile = GetSelectedProfile();
        if (profile is null)
        {
            return;
        }

        using var form = new ConnectionProfileEditForm(
            profile.Name,
            profile.Server,
            profile.UserName,
            _settingsService.UnprotectSecret(profile.ProtectedPassword),
            isNew: false);

        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var conflict = _settings.SqlConnections.Any(item =>
            !ReferenceEquals(item, profile) && item.Name.Equals(form.ProfileName, StringComparison.OrdinalIgnoreCase));
        if (conflict)
        {
            MessageBox.Show(this, $"Profile \"{form.ProfileName}\" already exists.", "Database Tools", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var wasLast = _settings.LastSqlConnectionName?.Equals(profile.Name, StringComparison.OrdinalIgnoreCase) == true;

        profile.Name = form.ProfileName;
        profile.Server = form.Server;
        profile.UserName = form.UserName;
        profile.ProtectedPassword = _settingsService.ProtectSecret(form.Password);

        if (wasLast)
        {
            _settings.LastSqlConnectionName = profile.Name;
        }

        await _settingsService.SaveAsync(_settings, CancellationToken.None);
        BindGrid();
        SelectProfile(profile.Name);
        MessageBox.Show(this, $"Profile \"{profile.Name}\" saved.", "Database Tools", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async Task DeleteSelectedProfileAsync()
    {
        var profile = GetSelectedProfile();
        if (profile is null)
        {
            return;
        }

        var confirmation = MessageBox.Show(
            this,
            $"Delete connection profile \"{profile.Name}\"?",
            "Database Tools",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        _settings.SqlConnections.Remove(profile);
        if (_settings.LastSqlConnectionName?.Equals(profile.Name, StringComparison.OrdinalIgnoreCase) == true)
        {
            _settings.LastSqlConnectionName = null;
        }

        await _settingsService.SaveAsync(_settings, CancellationToken.None);
        BindGrid();
    }

    private void SelectProfile(string name)
    {
        foreach (DataGridViewRow row in profilesGrid.Rows)
        {
            if (row.DataBoundItem is SqlConnectionProfile item && item.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                profilesGrid.ClearSelection();
                row.Selected = true;
                profilesGrid.CurrentCell = row.Cells[0];
                break;
            }
        }
    }
}