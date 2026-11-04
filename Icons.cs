using System.Drawing.Text;

namespace DatabaseTools;

/// <summary>
/// Renders Windows icon-font glyphs into small bitmaps so toolbars and menus can
/// carry icons without shipping binary image assets. Falls back to no icon when
/// no glyph font is installed, leaving the item text-only.
/// </summary>
internal static class Icons
{
    public const string Connect = "";   // Ethernet
    public const string Add = "";       // AddTo - circled; a bare thin plus reads as an afterthought
    public const string Refresh = "";   // Sync
    public const string Settings = "";  // Setting
    public const string Cloud = "";     // Cloud
    public const string Server = "";    // Server
    public const string Backup = "";    // Save
    public const string Restore = "";   // OpenFile
    public const string Optimize = "";  // Repair
    public const string Edit = "";      // Edit
    public const string Delete = "";    // Delete

    /// <summary>
    /// Bitmap size these icons are rendered at. Hosts must set <c>ImageScalingSize</c>
    /// to match via <see cref="Ui.UseIconScaling"/>, otherwise the 16px default rescales
    /// and blurs them.
    /// </summary>
    public const int Size = 20;

    /// <summary>
    /// Glyph is drawn smaller than the bitmap: at full box size the taller glyphs
    /// (trash, pencil) get clipped by a pixel or two. The 2px gap keeps the offset a
    /// whole number, which is what keeps rendering crisp.
    /// </summary>
    private const int GlyphSize = 16;

    /// <summary>Used for destructive actions, so they stand out from the rest of a menu.</summary>
    public static readonly Color DangerColor = Color.FromArgb(176, 32, 32);

    private static readonly Color DefaultColor = Color.FromArgb(48, 48, 48);

    // Segoe Fluent Icons is the Windows 11 icon font; Segoe MDL2 Assets is its
    // Windows 10 predecessor and covers the same code points for everything used here.
    private static readonly string? GlyphFontName = ResolveGlyphFont();

    private static readonly Dictionary<(string Glyph, int Argb), Image?> Cache = new();

    public static Image? Get(string glyph) => Get(glyph, DefaultColor);

    public static Image? Get(string glyph, Color color)
    {
        if (GlyphFontName is null)
        {
            return null;
        }

        var key = (glyph, color.ToArgb());
        if (Cache.TryGetValue(key, out var cached))
        {
            return cached;
        }

        var image = Render(glyph, color);
        Cache[key] = image;
        return image;
    }

    private static Image Render(string glyph, Color color)
    {
        var bitmap = new Bitmap(Size, Size);

        using var graphics = Graphics.FromImage(bitmap);
        // AntiAliasGridFit rather than ClearType: ClearType subpixel output smears
        // against the transparent background.
        graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

        // Regular weight only - these fonts have no bold face and GDI+'s synthesised
        // one smears the glyphs.
        using var font = new Font(GlyphFontName!, GlyphSize, GraphicsUnit.Pixel);
        using var brush = new SolidBrush(color);

        // Whole-pixel offset: a fractional one puts the glyph between pixels and it
        // renders visibly blurry.
        const float offset = (Size - GlyphSize) / 2f;
        graphics.DrawString(glyph, font, brush, new PointF(offset, offset), StringFormat.GenericTypographic);
        return bitmap;
    }

    private static string? ResolveGlyphFont()
    {
        foreach (var candidate in new[] { "Segoe Fluent Icons", "Segoe MDL2 Assets" })
        {
            if (IsFontInstalled(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static bool IsFontInstalled(string name)
    {
        using var fonts = new InstalledFontCollection();
        return fonts.Families.Any(family => family.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }
}
