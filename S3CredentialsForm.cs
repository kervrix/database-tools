namespace DatabaseTools;

public partial class S3CredentialsForm : Form
{
    private readonly SettingsService _settingsService = new();
    private AppSettings _settings = new();

    public S3CredentialsForm()
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
        profilesGrid.DataSource = _settings.S3Credentials
            .OrderBy(profile => profile.Name)
            .Select(profile => new S3ProfileRow(profile))
            .ToList();
        UpdateButtonState();
    }

    private void profilesGrid_SelectionChanged(object sender, EventArgs e)
    {
        UpdateButtonState();
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

    private void UpdateButtonState()
    {
        var hasSelection = GetSelectedProfile() is not null;
        editProfileMenuItem.Enabled = hasSelection;
        deleteProfileMenuItem.Enabled = hasSelection;
    }

    private S3CredentialProfile? GetSelectedProfile()
    {
        return profilesGrid.CurrentRow?.DataBoundItem is S3ProfileRow row ? row.Profile : null;
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
        using var form = new S3ProfileEditForm(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty, isNew: true);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        if (_settings.FindS3Credential(form.ProfileName) is not null)
        {
            MessageBox.Show(this, $"Profile \"{form.ProfileName}\" already exists.", "Database Tools", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var profile = new S3CredentialProfile
        {
            Name = form.ProfileName,
            S3Url = form.BuildS3Url(),
            AccessKey = form.AccessKey,
            ProtectedSecretKey = _settingsService.ProtectSecret(form.SecretKey)
        };

        _settings.S3Credentials.Add(profile);
        _settings.LastS3CredentialName = profile.Name;

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

        var location = ParseS3ProfileUrl(profile.S3Url);
        using var form = new S3ProfileEditForm(
            profile.Name,
            location.EndpointUrl,
            location.Bucket,
            profile.AccessKey,
            _settingsService.UnprotectSecret(profile.ProtectedSecretKey),
            isNew: false);

        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var conflict = _settings.S3Credentials.Any(item =>
            !ReferenceEquals(item, profile) && item.Name.Equals(form.ProfileName, StringComparison.OrdinalIgnoreCase));
        if (conflict)
        {
            MessageBox.Show(this, $"Profile \"{form.ProfileName}\" already exists.", "Database Tools", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var wasLast = _settings.LastS3CredentialName?.Equals(profile.Name, StringComparison.OrdinalIgnoreCase) == true;

        profile.Name = form.ProfileName;
        profile.S3Url = form.BuildS3Url();
        profile.AccessKey = form.AccessKey;
        profile.ProtectedSecretKey = _settingsService.ProtectSecret(form.SecretKey);

        if (wasLast)
        {
            _settings.LastS3CredentialName = profile.Name;
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
            $"Delete S3 profile \"{profile.Name}\"?",
            "Database Tools",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (confirmation != DialogResult.Yes)
        {
            return;
        }

        _settings.S3Credentials.Remove(profile);
        if (_settings.LastS3CredentialName?.Equals(profile.Name, StringComparison.OrdinalIgnoreCase) == true)
        {
            _settings.LastS3CredentialName = null;
        }

        await _settingsService.SaveAsync(_settings, CancellationToken.None);
        BindGrid();
    }

    private void SelectProfile(string name)
    {
        foreach (DataGridViewRow row in profilesGrid.Rows)
        {
            if (row.DataBoundItem is S3ProfileRow item && item.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                profilesGrid.ClearSelection();
                row.Selected = true;
                profilesGrid.CurrentCell = row.Cells[0];
                break;
            }
        }
    }

    private static S3ProfileUrlParts ParseS3ProfileUrl(string s3Url)
    {
        if (!Uri.TryCreate(s3Url.TrimEnd('/'), UriKind.Absolute, out var uri))
        {
            return new S3ProfileUrlParts(s3Url, string.Empty);
        }

        var pathParts = uri.AbsolutePath
            .Trim('/')
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var bucket = Uri.UnescapeDataString(string.Join("/", pathParts));
        var endpointUrl = $"{uri.Scheme}://{uri.Authority}";

        return new S3ProfileUrlParts(endpointUrl, bucket);
    }

    private sealed record S3ProfileUrlParts(string EndpointUrl, string Bucket);

    private sealed class S3ProfileRow
    {
        public S3ProfileRow(S3CredentialProfile profile)
        {
            Profile = profile;
            Bucket = ParseS3ProfileUrl(profile.S3Url).Bucket;
        }

        public S3CredentialProfile Profile { get; }
        public string Name => Profile.Name;
        public string Bucket { get; }
    }
}