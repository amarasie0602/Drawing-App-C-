using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

public partial class Form1 : Form
{
    private List<Shape> shapes = new List<Shape>();
    private Stack<Shape> undoStack = new Stack<Shape>();
    private Stack<Shape> redoStack = new Stack<Shape>();
    private Shape currentShape = null;
    private Shape selectedShape = null;
    private Point startPoint;

    private ComboBox shapeSelector;
    private Button fillColorButton;
    private Button borderColorButton;
    private NumericUpDown borderWidthControl;
    private Button undoButton;
    private Button redoButton;
    private Panel canvas;

    private Color selectedFillColor = Color.LightBlue;
    private Color selectedBorderColor = Color.Black;
    private int selectedBorderWidth = 2;

    public Form1()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        this.Text = "Shape Drawing App (with Bonus)";
        this.Size = new Size(900, 600);

        shapeSelector = new ComboBox();
        shapeSelector.Items.AddRange(new string[] { "Rectangle", "Circle" });
        shapeSelector.SelectedIndex = 0;
        shapeSelector.Location = new Point(10, 10);

        fillColorButton = new Button();
        fillColorButton.Text = "Fill Color";
        fillColorButton.Location = new Point(150, 10);
        fillColorButton.Click += (s, e) =>
        {
            using var dlg = new ColorDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                selectedFillColor = dlg.Color;
                if (selectedShape != null)
                {
                    selectedShape.FillColor = dlg.Color;
                    canvas.Invalidate();
                }
            }
        };

        borderColorButton = new Button();
        borderColorButton.Text = "Border Color";
        borderColorButton.Location = new Point(250, 10);
        borderColorButton.Click += (s, e) =>
        {
            using var dlg = new ColorDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                selectedBorderColor = dlg.Color;
                if (selectedShape != null)
                {
                    selectedShape.BorderColor = dlg.Color;
                    canvas.Invalidate();
                }
            }
        };

        borderWidthControl = new NumericUpDown();
        borderWidthControl.Minimum = 1;
        borderWidthControl.Maximum = 10;
        borderWidthControl.Value = selectedBorderWidth;
        borderWidthControl.Location = new Point(370, 10);
        borderWidthControl.Width = 50;
        borderWidthControl.ValueChanged += (s, e) =>
        {
            selectedBorderWidth = (int)borderWidthControl.Value;
            if (selectedShape != null)
            {
                selectedShape.BorderWidth = selectedBorderWidth;
                canvas.Invalidate();
            }
        };

        undoButton = new Button() { Text = "Undo", Location = new Point(450, 10) };
        redoButton = new Button() { Text = "Redo", Location = new Point(530, 10) };

        undoButton.Click += (s, e) =>
        {
            if (shapes.Count > 0)
            {
                var shape = shapes.Last();
                shapes.RemoveAt(shapes.Count - 1);
                undoStack.Push(shape);
                selectedShape = null;
                canvas.Invalidate();
            }
        };

        redoButton.Click += (s, e) =>
        {
            if (undoStack.Count > 0)
            {
                var shape = undoStack.Pop();
                shapes.Add(shape);
                selectedShape = null;
                canvas.Invalidate();
            }
        };

        canvas = new Panel();
        canvas.Location = new Point(10, 50);
        canvas.Size = new Size(860, 480);
        canvas.BackColor = Color.White;
        canvas.BorderStyle = BorderStyle.FixedSingle;
        canvas.Paint += Canvas_Paint;
        canvas.MouseDown += Canvas_MouseDown;
        canvas.MouseMove += Canvas_MouseMove;
        canvas.MouseUp += Canvas_MouseUp;

        this.Controls.AddRange(new Control[] {
            shapeSelector, fillColorButton, borderColorButton, borderWidthControl,
            undoButton, redoButton, canvas
        });
    }

    private void Canvas_Paint(object sender, PaintEventArgs e)
    {
        foreach (var shape in shapes)
        {
            shape.Draw(e.Graphics);

            if (shape == selectedShape)
            {
                using var pen = new Pen(Color.Red, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash };
                e.Graphics.DrawRectangle(pen, shape.Bounds);
            }
        }

        if (currentShape != null)
            currentShape.Draw(e.Graphics);
    }

    private void Canvas_MouseDown(object sender, MouseEventArgs e)
    {
        selectedShape = null;

        foreach (var shape in shapes.AsEnumerable().Reverse())
        {
            if (shape.Contains(e.Location))
            {
                selectedShape = shape;
                selectedFillColor = shape.FillColor;
                selectedBorderColor = shape.BorderColor;
                selectedBorderWidth = shape.BorderWidth;
                borderWidthControl.Value = selectedBorderWidth;
                break;
            }
        }

        if (selectedShape == null)
            startPoint = e.Location;

        canvas.Invalidate();
    }

    private void Canvas_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left && selectedShape == null)
        {
            var rect = GetRectangle(startPoint, e.Location);

            currentShape = shapeSelector.SelectedItem.ToString() == "Rectangle"
                ? new RectangleShape() as Shape
                : new CircleShape() as Shape;

            currentShape.FillColor = selectedFillColor;
            currentShape.BorderColor = selectedBorderColor;
            currentShape.BorderWidth = selectedBorderWidth;
            currentShape.Bounds = rect;

            canvas.Invalidate();
        }
    }

    private void Canvas_MouseUp(object sender, MouseEventArgs e)
    {
        if (currentShape != null)
        {
            shapes.Add(currentShape);
            redoStack.Clear(); // Clear redo when new shape added
            currentShape = null;
            canvas.Invalidate();
        }
    }

    private Rectangle GetRectangle(Point p1, Point p2)
    {
        return new Rectangle(
            Math.Min(p1.X, p2.X),
            Math.Min(p1.Y, p2.Y),
            Math.Abs(p1.X - p2.X),
            Math.Abs(p1.Y - p2.Y)
        );
    }
}