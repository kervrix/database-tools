namespace DatabaseTools;

internal static class Ui
{
    public static async Task RunAsync(Form owner, Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            ShowError(owner, ex.Message);
        }
    }

    public static void ShowError(Form owner, string message)
    {
        MessageBox.Show(owner, message, "Database Tools", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    public static void BindComboBox<T>(ComboBox comboBox, IEnumerable<T> items, object? preferredSelection = null)
        where T : notnull
    {
        BindItems(comboBox.Items, item => comboBox.SelectedItem = item, index => comboBox.SelectedIndex = index, items, preferredSelection);
    }

    public static void BindListBox<T>(ListBox listBox, IEnumerable<T> items, object? preferredSelection = null)
        where T : notnull
    {
        BindItems(listBox.Items, item => listBox.SelectedItem = item, index => listBox.SelectedIndex = index, items, preferredSelection);
    }

    public static void Require(Control control, string message)
    {
        if (string.IsNullOrWhiteSpace(control.Text))
        {
            throw new InvalidOperationException(message);
        }
    }

    public static void StyleToolStrip(ToolStrip toolStrip)
    {
        toolStrip.RenderMode = ToolStripRenderMode.System;
        toolStrip.GripStyle = ToolStripGripStyle.Hidden;
        toolStrip.BackColor = SystemColors.Control;
        UseIconScaling(toolStrip);
    }

    /// <summary>
    /// Sizes a strip's image slot to match <see cref="Icons.Size"/>. Without this the
    /// 16px default rescales the icons and blurs them.
    /// </summary>
    public static void UseIconScaling(ToolStrip strip)
    {
        strip.ImageScalingSize = new Size(Icons.Size, Icons.Size);
    }

    /// <summary>
    /// A 1px rule that keeps the toolbar from bleeding into the content below it.
    /// </summary>
    public static void StyleDivider(Panel divider)
    {
        divider.BackColor = Color.FromArgb(170, 170, 170);
    }

    /// <summary>
    /// Selects the row under the cursor on right-click so context menu actions
    /// always target the row the user actually pointed at.
    /// </summary>
    public static void SelectRowUnderMouse(DataGridView grid, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
        {
            return;
        }

        var hit = grid.HitTest(e.X, e.Y);
        if (hit.RowIndex < 0)
        {
            return;
        }

        grid.ClearSelection();
        grid.Rows[hit.RowIndex].Selected = true;
        grid.CurrentCell = grid.Rows[hit.RowIndex].Cells[Math.Max(hit.ColumnIndex, 0)];
    }

    private static void BindItems<T>(
        System.Collections.IList itemsCollection,
        Action<object> selectItem,
        Action<int> selectIndex,
        IEnumerable<T> items,
        object? preferredSelection)
        where T : notnull
    {
        itemsCollection.Clear();
        foreach (var item in items)
        {
            itemsCollection.Add(item);
        }

        if (preferredSelection is not null && itemsCollection.Contains(preferredSelection))
        {
            selectItem(preferredSelection);
        }
        else if (itemsCollection.Count > 0)
        {
            selectIndex(0);
        }
    }
}