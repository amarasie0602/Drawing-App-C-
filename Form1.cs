using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

public class Form1 : Form
{
    private DrawingCanvas canvas;

    // Toolbar buttons
    private ToolStrip toolbar;
    private ToolStripComboBox toolDropdown;
    private ToolStripButton btnUndo;
    private ToolStripButton btnRedo;
    private ToolStripButton btnDelete;
    private ToolStripButton btnNew;
    private ToolStripButton btnOpen;
    private ToolStripButton btnSave;

    // Property controls
    private Panel propertyPanel;
    private Label fillSwatch;
    private Label borderSwatch;
    private Button btnFillColor;
    private Button btnBorderColor;
    private NumericUpDown borderWidthInput;

    // Status bar
    private StatusStrip statusBar;
    private ToolStripLabel labelCoords;
    private ToolStripLabel labelInfo;

    public Form1()
    {
        this.Text = "Drawing App";
        this.Size = new Size(1000, 650);
        this.MinimumSize = new Size(700, 500);
        this.KeyPreview = true;
        this.KeyDown += Form1_KeyDown;

        SetupCanvas();
        SetupToolbar();
        SetupPropertyPanel();
        SetupStatusBar();
        SetupLayout();
        WireUpEvents();

        SetActiveTool(CanvasTool.Rectangle);
        UpdateTitle();
    }

    private void SetupCanvas()
    {
        canvas = new DrawingCanvas();
        canvas.Dock = DockStyle.Fill;
        canvas.TabStop = true;
    }

    private void SetupToolbar()
    {
        toolbar = new ToolStrip();

        btnNew    = new ToolStripButton("New");
        btnOpen   = new ToolStripButton("Open");
        btnSave   = new ToolStripButton("Save");
        btnUndo   = new ToolStripButton("Undo");
        btnRedo   = new ToolStripButton("Redo");
        btnDelete = new ToolStripButton("Delete");

        btnNew.ToolTipText    = "New drawing (Ctrl+N)";
        btnOpen.ToolTipText   = "Open file (Ctrl+O)";
        btnSave.ToolTipText   = "Save file (Ctrl+S)";
        btnUndo.ToolTipText   = "Undo (Ctrl+Z)";
        btnRedo.ToolTipText   = "Redo (Ctrl+Y)";
        btnDelete.ToolTipText = "Delete selected shape (Del)";

        // Shape tool dropdown
        toolDropdown = new ToolStripComboBox();
        toolDropdown.DropDownStyle = ComboBoxStyle.DropDownList;
        toolDropdown.Items.AddRange(new string[] { "Select", "Rectangle", "Circle", "Triangle", "Line", "Polygon" });
        toolDropdown.SelectedIndex = 1; // Rectangle by default
        toolDropdown.ToolTipText = "Choose drawing tool";
        toolDropdown.Width = 100;

        ToolStripLabel toolLabel = new ToolStripLabel("Tool:");

        toolbar.Items.AddRange(new ToolStripItem[]
        {
            btnNew, btnOpen, btnSave,
            new ToolStripSeparator(),
            toolLabel, toolDropdown,
            new ToolStripSeparator(),
            btnUndo, btnRedo,
            new ToolStripSeparator(),
            btnDelete
        });
    }

    private void SetupPropertyPanel()
    {
        propertyPanel = new Panel();
        propertyPanel.Height = 36;
        propertyPanel.Padding = new Padding(4, 4, 4, 4);

        fillSwatch = new Label();
        fillSwatch.Size = new Size(22, 22);
        fillSwatch.BackColor = canvas.FillColor;
        fillSwatch.BorderStyle = BorderStyle.FixedSingle;

        btnFillColor = new Button();
        btnFillColor.Text = "Fill Color";
        btnFillColor.Size = new Size(75, 24);

        borderSwatch = new Label();
        borderSwatch.Size = new Size(22, 22);
        borderSwatch.BackColor = canvas.BorderColor;
        borderSwatch.BorderStyle = BorderStyle.FixedSingle;

        btnBorderColor = new Button();
        btnBorderColor.Text = "Border Color";
        btnBorderColor.Size = new Size(85, 24);

        Label widthLabel = new Label();
        widthLabel.Text = "Width:";
        widthLabel.AutoSize = true;

        borderWidthInput = new NumericUpDown();
        borderWidthInput.Minimum = 1;
        borderWidthInput.Maximum = 30;
        borderWidthInput.Value = 2;
        borderWidthInput.Width = 50;

        FlowLayoutPanel flow = new FlowLayoutPanel();
        flow.Dock = DockStyle.Fill;
        flow.FlowDirection = FlowDirection.LeftToRight;
        flow.WrapContents = false;
        flow.Controls.Add(fillSwatch);
        flow.Controls.Add(btnFillColor);
        flow.Controls.Add(borderSwatch);
        flow.Controls.Add(btnBorderColor);
        flow.Controls.Add(widthLabel);
        flow.Controls.Add(borderWidthInput);

        propertyPanel.Controls.Add(flow);
    }

    private void SetupStatusBar()
    {
        statusBar = new StatusStrip();
        labelCoords = new ToolStripLabel("x: 0  y: 0");
        labelInfo   = new ToolStripLabel("") { Spring = true };
        labelInfo.TextAlign = ContentAlignment.MiddleRight;
        statusBar.Items.Add(labelCoords);
        statusBar.Items.Add(new ToolStripSeparator());
        statusBar.Items.Add(labelInfo);
    }

    private void SetupLayout()
    {
        Panel topArea = new Panel();
        topArea.Dock = DockStyle.Top;
        topArea.Height = 68;
        toolbar.Dock = DockStyle.Top;
        propertyPanel.Dock = DockStyle.Bottom;
        topArea.Controls.Add(propertyPanel);
        topArea.Controls.Add(toolbar);

        this.Controls.Add(canvas);
        this.Controls.Add(topArea);
        this.Controls.Add(statusBar);
    }

    private void WireUpEvents()
    {
        btnNew.Click    += (s, e) => NewFile();
        btnOpen.Click   += (s, e) => OpenFile();
        btnSave.Click   += (s, e) => SaveFile();
        btnUndo.Click   += (s, e) => { canvas.Document.Undo(); UpdateTitle(); };
        btnRedo.Click   += (s, e) => { canvas.Document.Redo(); UpdateTitle(); };
        btnDelete.Click += (s, e) => canvas.DeleteSelected();

        toolDropdown.SelectedIndexChanged += (s, e) =>
        {
            string item = toolDropdown.SelectedItem.ToString();
            if (item == "Select")    SetActiveTool(CanvasTool.Select);
            if (item == "Rectangle") SetActiveTool(CanvasTool.Rectangle);
            if (item == "Circle")    SetActiveTool(CanvasTool.Circle);
            if (item == "Triangle")  SetActiveTool(CanvasTool.Triangle);
            if (item == "Line")      SetActiveTool(CanvasTool.Line);
            if (item == "Polygon")   SetActiveTool(CanvasTool.Polygon);
        };

        btnFillColor.Click   += (s, e) => PickFillColor();
        btnBorderColor.Click += (s, e) => PickBorderColor();

        borderWidthInput.ValueChanged += (s, e) =>
        {
            canvas.BorderWidth = (int)borderWidthInput.Value;

            if (canvas.SelectedShape != null)
            {
                int oldWidth = canvas.SelectedShape.BorderWidth;
                int newWidth = canvas.BorderWidth;
                canvas.SelectedShape.BorderWidth = newWidth;

                canvas.Document.RecordCommand(new PropertyChangeCommand("Border Width",
                    () => canvas.SelectedShape.BorderWidth = newWidth,
                    () => canvas.SelectedShape.BorderWidth = oldWidth));

                canvas.Invalidate();
            }
        };

        canvas.SelectionChanged += (s, e) =>
        {
            Shape selected = canvas.SelectedShape;
            if (selected != null)
            {
                canvas.FillColor = selected.FillColor;
                canvas.BorderColor = selected.BorderColor;
                canvas.BorderWidth = selected.BorderWidth;
                fillSwatch.BackColor = selected.FillColor;
                borderSwatch.BackColor = selected.BorderColor;
                borderWidthInput.Value = selected.BorderWidth;
            }
            btnDelete.Enabled = selected != null;
            UpdateStatusInfo();
        };

        canvas.MouseMoved += (s, pt) =>
        {
            labelCoords.Text = "x: " + pt.X + "  y: " + pt.Y;
        };

        canvas.Document.Changed += (s, e) =>
        {
            btnUndo.Enabled = canvas.Document.CanUndo;
            btnRedo.Enabled = canvas.Document.CanRedo;

            if (canvas.Document.CanUndo)
                btnUndo.ToolTipText = "Undo: " + canvas.Document.NextUndoDescription;
            else
                btnUndo.ToolTipText = "Nothing to undo";

            if (canvas.Document.CanRedo)
                btnRedo.ToolTipText = "Redo: " + canvas.Document.NextRedoDescription;
            else
                btnRedo.ToolTipText = "Nothing to redo";

            UpdateTitle();
            UpdateStatusInfo();
        };
    }

    private void SetActiveTool(CanvasTool tool)
    {
        canvas.Tool = tool;
        canvas.SetSelection(null);

        // Keep the dropdown in sync when tool is changed via keyboard shortcut
        if (tool == CanvasTool.Select)    toolDropdown.SelectedItem = "Select";
        if (tool == CanvasTool.Rectangle) toolDropdown.SelectedItem = "Rectangle";
        if (tool == CanvasTool.Circle)    toolDropdown.SelectedItem = "Circle";
        if (tool == CanvasTool.Triangle)  toolDropdown.SelectedItem = "Triangle";
        if (tool == CanvasTool.Line)      toolDropdown.SelectedItem = "Line";
        if (tool == CanvasTool.Polygon)   toolDropdown.SelectedItem = "Polygon";

        if (tool == CanvasTool.Polygon)
            labelInfo.Text = "Click to add points, double-click or Escape to cancel, Enter/right-click to finish";
        else if (tool == CanvasTool.Select)
            labelInfo.Text = "Click to select, drag to move, use handles to resize, Del to delete";
        else
            labelInfo.Text = "Drag to draw";
    }

    private void PickFillColor()
    {
        ColorDialog dlg = new ColorDialog();
        dlg.Color = canvas.FillColor;

        if (dlg.ShowDialog() != DialogResult.OK) return;

        Color newColor = dlg.Color;
        canvas.FillColor = newColor;
        fillSwatch.BackColor = newColor;

        Shape selected = canvas.SelectedShape;
        if (selected == null) return;

        Color oldColor = selected.FillColor;
        selected.FillColor = newColor;

        canvas.Document.RecordCommand(new PropertyChangeCommand("Fill Color",
            () => { selected.FillColor = newColor; canvas.Invalidate(); },
            () => { selected.FillColor = oldColor; canvas.Invalidate(); }));

        canvas.Invalidate();
    }

    private void PickBorderColor()
    {
        ColorDialog dlg = new ColorDialog();
        dlg.Color = canvas.BorderColor;

        if (dlg.ShowDialog() != DialogResult.OK) return;

        Color newColor = dlg.Color;
        canvas.BorderColor = newColor;
        borderSwatch.BackColor = newColor;

        Shape selected = canvas.SelectedShape;
        if (selected == null) return;

        Color oldColor = selected.BorderColor;
        selected.BorderColor = newColor;

        canvas.Document.RecordCommand(new PropertyChangeCommand("Border Color",
            () => { selected.BorderColor = newColor; canvas.Invalidate(); },
            () => { selected.BorderColor = oldColor; canvas.Invalidate(); }));

        canvas.Invalidate();
    }

    private void NewFile()
    {
        if (!ConfirmClose()) return;
        canvas.Document.Clear();
        canvas.SetSelection(null);
        UpdateTitle();
    }

    private void OpenFile()
    {
        if (!ConfirmClose()) return;

        OpenFileDialog dlg = new OpenFileDialog();
        dlg.Filter = "Drawing files (*.drw)|*.drw|All files (*.*)|*.*";

        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            canvas.Document.Load(dlg.FileName);
            canvas.SetSelection(null);
            UpdateTitle();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not open the file:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void SaveFile()
    {
        string path = canvas.Document.FilePath;

        if (string.IsNullOrEmpty(path))
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Drawing files (*.drw)|*.drw|All files (*.*)|*.*";

            if (dlg.ShowDialog() != DialogResult.OK) return;
            path = dlg.FileName;
        }

        try
        {
            canvas.Document.Save(path);
            UpdateTitle();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Could not save the file:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool ConfirmClose()
    {
        if (!canvas.Document.IsDirty) return true;

        DialogResult result = MessageBox.Show(
            "You have unsaved changes. Do you want to save first?",
            "Unsaved Changes",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            SaveFile();
            return !canvas.Document.IsDirty;
        }

        return result == DialogResult.No;
    }

    private void UpdateTitle()
    {
        string filename = string.IsNullOrEmpty(canvas.Document.FilePath)
            ? "Untitled"
            : Path.GetFileName(canvas.Document.FilePath);

        string dirty = canvas.Document.IsDirty ? "*" : "";
        this.Text = "Drawing App - " + filename + dirty;
    }

    private void UpdateStatusInfo()
    {
        Shape selected = canvas.SelectedShape;
        if (selected != null)
        {
            Rectangle b = selected.Bounds;
            string name = selected.GetType().Name.Replace("Shape", "");
            labelInfo.Text = "Selected: " + name + "   x:" + b.X + " y:" + b.Y + " w:" + b.Width + " h:" + b.Height;
        }
    }

    private void Form1_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Control)
        {
            if (e.KeyCode == Keys.Z) { canvas.Document.Undo(); UpdateTitle(); e.Handled = true; }
            if (e.KeyCode == Keys.Y) { canvas.Document.Redo(); UpdateTitle(); e.Handled = true; }
            if (e.KeyCode == Keys.S) { SaveFile(); e.Handled = true; }
            if (e.KeyCode == Keys.O) { OpenFile(); e.Handled = true; }
            if (e.KeyCode == Keys.N) { NewFile();  e.Handled = true; }
            return;
        }

        if (e.KeyCode == Keys.Escape) { canvas.CancelPolygon(); e.Handled = true; }
        if (e.KeyCode == Keys.V) SetActiveTool(CanvasTool.Select);
        if (e.KeyCode == Keys.R) SetActiveTool(CanvasTool.Rectangle);
        if (e.KeyCode == Keys.C) SetActiveTool(CanvasTool.Circle);
        if (e.KeyCode == Keys.T) SetActiveTool(CanvasTool.Triangle);
        if (e.KeyCode == Keys.L) SetActiveTool(CanvasTool.Line);
        if (e.KeyCode == Keys.P) SetActiveTool(CanvasTool.Polygon);
        if (e.KeyCode == Keys.Delete) canvas.DeleteSelected();
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);
        if (!ConfirmClose())
            e.Cancel = true;
    }
}