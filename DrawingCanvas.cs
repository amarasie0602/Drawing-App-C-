using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public enum CanvasTool { Select, Rectangle, Circle, Triangle, Line, Polygon }

public class DrawingCanvas : Panel
{
    public DrawingDocument Document { get; } = new DrawingDocument();
    public CanvasTool Tool { get; set; } = CanvasTool.Rectangle;

    public Color FillColor { get; set; } = Color.LightBlue;
    public Color BorderColor { get; set; } = Color.Black;
    public int BorderWidth { get; set; } = 2;

    public Shape? SelectedShape { get; private set; }

    public event EventHandler? SelectionChanged;
    public event EventHandler<Point>? MouseMoved;

    // State for drawing a new shape
    private Point drawStart;
    private Shape? previewShape;

    // State for moving a selected shape
    private bool isMoving;
    private Point lastMousePos;
    private object? geometryBeforeDrag;

    // State for resizing via handles
    private int activeHandleIndex = -1;
    private Point[]? handlePointsBeforeDrag;

    // State for polygon drawing (click to add points)
    private PolygonShape? polygonInProgress;
    private Point currentMousePos;

    public DrawingCanvas()
    {
        DoubleBuffered = true;
        BackColor = Color.White;
        BorderStyle = BorderStyle.FixedSingle;

        Document.Changed += (s, e) => Invalidate();

        KeyDown += Canvas_KeyDown;
    }

    public void SetSelection(Shape? shape)
    {
        SelectedShape = shape;
        SelectionChanged?.Invoke(this, EventArgs.Empty);
        Invalidate();
    }

    public void DeleteSelected()
    {
        if (SelectedShape == null) return;
        Document.Execute(new DeleteShapeCommand(Document.Shapes, SelectedShape));
        SetSelection(null);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        Graphics g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        foreach (Shape shape in Document.Shapes)
        {
            shape.Draw(g);
            if (shape == SelectedShape)
                shape.DrawSelection(g);
        }

        // Draw shape preview while user is dragging to create one
        if (previewShape != null)
            previewShape.Draw(g);

        // Draw the polygon that's still being built
        if (polygonInProgress != null)
        {
            polygonInProgress.Draw(g);

            // Draw a ghost line from the last vertex to the mouse
            Point[] pts = polygonInProgress.GetPoints();
            if (pts.Length > 0)
            {
                Pen ghostPen = new Pen(Color.Gray, 1);
                ghostPen.DashStyle = DashStyle.Dot;
                g.DrawLine(ghostPen, pts[pts.Length - 1], currentMousePos);
                ghostPen.Dispose();
            }

            // Draw small dots on each vertex so the user can see them
            foreach (Point pt in polygonInProgress.GetPoints())
                g.FillEllipse(Brushes.DodgerBlue, pt.X - 4, pt.Y - 4, 8, 8);
        }
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        this.Focus();

        drawStart = e.Location;
        lastMousePos = e.Location;

        // Polygon tool uses click-by-click mode instead of drag
        if (Tool == CanvasTool.Polygon)
        {
            HandlePolygonClick(e);
            return;
        }

        if (Tool == CanvasTool.Select)
        {
            HandleSelectMouseDown(e);
            return;
        }

        // Start drawing a new shape
        previewShape = CreateShapeForTool();
        previewShape.FillColor = FillColor;
        previewShape.BorderColor = BorderColor;
        previewShape.BorderWidth = BorderWidth;
        UpdatePreviewShape(e.Location);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        currentMousePos = e.Location;
        MouseMoved?.Invoke(this, e.Location);

        if (polygonInProgress != null)
        {
            Invalidate();
            return;
        }

        if (e.Button == MouseButtons.Left)
        {
            if (isMoving && SelectedShape != null)
            {
                int dx = e.X - lastMousePos.X;
                int dy = e.Y - lastMousePos.Y;
                SelectedShape.Move(dx, dy);
                lastMousePos = e.Location;
                Invalidate();
            }
            else if (activeHandleIndex >= 0 && SelectedShape != null)
            {
                SelectedShape.ApplyHandle(activeHandleIndex, e.Location, handlePointsBeforeDrag);
                Invalidate();
            }
            else if (previewShape != null)
            {
                UpdatePreviewShape(e.Location);
                Invalidate();
            }
        }
        else
        {
            UpdateCursor(e.Location);
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        // Finish move - record command for undo
        if (isMoving && SelectedShape != null && geometryBeforeDrag != null)
        {
            object stateAfter = SelectedShape.SaveGeometry();
            Document.RecordCommand(new MoveResizeCommand(SelectedShape, geometryBeforeDrag, stateAfter, "Move"));
        }
        isMoving = false;

        // Finish resize - record command for undo
        if (activeHandleIndex >= 0 && SelectedShape != null && geometryBeforeDrag != null)
        {
            object stateAfter = SelectedShape.SaveGeometry();
            Document.RecordCommand(new MoveResizeCommand(SelectedShape, geometryBeforeDrag, stateAfter, "Resize"));
        }
        activeHandleIndex = -1;
        geometryBeforeDrag = null;

        // Finish drawing a shape
        if (previewShape != null)
        {
            Rectangle b = previewShape.Bounds;
            bool bigEnough = b.Width >= 4 && b.Height >= 4;

            // Lines just need to be long enough, not have area
            if (previewShape is LineShape ls)
                bigEnough = Math.Abs(ls.X2 - ls.X1) >= 4 || Math.Abs(ls.Y2 - ls.Y1) >= 4;

            if (bigEnough)
            {
                Document.Execute(new AddShapeCommand(Document.Shapes, previewShape));
                SetSelection(null);
            }

            previewShape = null;
            Invalidate();
        }
    }

    protected override void OnMouseDoubleClick(MouseEventArgs e)
    {
        base.OnMouseDoubleClick(e);
        if (Tool == CanvasTool.Polygon)
            FinishPolygon();
    }

    private void Canvas_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter) FinishPolygon();
        if (e.KeyCode == Keys.Delete) DeleteSelected();
    }

    private void HandlePolygonClick(MouseEventArgs e)
    {
        if (e.Button == MouseButtons.Left)
        {
            if (polygonInProgress == null)
            {
                polygonInProgress = new PolygonShape();
                polygonInProgress.FillColor = FillColor;
                polygonInProgress.BorderColor = BorderColor;
                polygonInProgress.BorderWidth = BorderWidth;
            }

            // If the user clicks near the first point, close the polygon
            Point[] pts = polygonInProgress.GetPoints();
            if (pts.Length >= 3 && Distance(e.Location, pts[0]) < 12)
            {
                FinishPolygon();
            }
            else
            {
                polygonInProgress.AddPoint(e.Location);
                Invalidate();
            }
        }
        else if (e.Button == MouseButtons.Right)
        {
            FinishPolygon();
        }
    }

    private void FinishPolygon()
    {
        if (polygonInProgress == null) return;
        if (polygonInProgress.PX.Count >= 3)
            Document.Execute(new AddShapeCommand(Document.Shapes, polygonInProgress));
        polygonInProgress = null;
        Invalidate();
    }

    public void CancelPolygon()
    {
        polygonInProgress = null;
        Invalidate();
    }

    private void HandleSelectMouseDown(MouseEventArgs e)
    {
        // Check if user clicked on a resize handle of the selected shape
        if (SelectedShape != null)
        {
            ShapeHandle clickedHandle = GetHandleAt(SelectedShape, e.Location);
            if (clickedHandle != null)
            {
                activeHandleIndex = clickedHandle.Index;
                handlePointsBeforeDrag = SelectedShape.GetHandlePoints();
                geometryBeforeDrag = SelectedShape.SaveGeometry();
                return;
            }

            // Check if clicking on the selected shape to move it
            if (SelectedShape.HitTest(e.Location))
            {
                isMoving = true;
                geometryBeforeDrag = SelectedShape.SaveGeometry();
                return;
            }
        }

        // Try to select a different shape (check topmost first)
        Shape? clicked = null;
        for (int i = Document.Shapes.Count - 1; i >= 0; i--)
        {
            if (Document.Shapes[i].HitTest(e.Location))
            {
                clicked = Document.Shapes[i];
                break;
            }
        }

        SetSelection(clicked);

        if (clicked != null)
        {
            isMoving = true;
            geometryBeforeDrag = clicked.SaveGeometry();
        }
    }

    private void UpdateCursor(Point mousePos)
    {
        if (Tool == CanvasTool.Select && SelectedShape != null)
        {
            ShapeHandle handle = GetHandleAt(SelectedShape, mousePos);
            if (handle != null)
                Cursor = handle.Cursor;
            else if (SelectedShape.HitTest(mousePos))
                Cursor = Cursors.SizeAll;
            else
                Cursor = Cursors.Default;
        }
        else if (Tool != CanvasTool.Select)
        {
            Cursor = Cursors.Cross;
        }
        else
        {
            Cursor = Cursors.Default;
        }
    }

    private Shape CreateShapeForTool()
    {
        if (Tool == CanvasTool.Rectangle) return new RectangleShape();
        if (Tool == CanvasTool.Circle)    return new CircleShape();
        if (Tool == CanvasTool.Triangle)  return new TriangleShape();
        if (Tool == CanvasTool.Line)      return new LineShape();
        return new RectangleShape();
    }

    private void UpdatePreviewShape(Point current)
    {
        int x = Math.Min(drawStart.X, current.X);
        int y = Math.Min(drawStart.Y, current.Y);
        int w = Math.Abs(current.X - drawStart.X);
        int h = Math.Abs(current.Y - drawStart.Y);

        if (previewShape is RectangleShape rs)
        {
            rs.X = x; rs.Y = y; rs.W = w; rs.H = h;
        }
        else if (previewShape is TriangleShape ts)
        {
            ts.PX = new int[] { x + w / 2, x, x + w };
            ts.PY = new int[] { y, y + h, y + h };
        }
        else if (previewShape is LineShape ls)
        {
            ls.X1 = drawStart.X; ls.Y1 = drawStart.Y;
            ls.X2 = current.X;  ls.Y2 = current.Y;
        }
    }

    private ShapeHandle GetHandleAt(Shape shape, Point p)
    {
        foreach (ShapeHandle handle in shape.GetHandles())
        {
            if (handle.Rect.Contains(p))
                return handle;
        }
        return null;
    }

    private double Distance(Point a, Point b)
    {
        return Math.Sqrt(Math.Pow(a.X - b.X, 2) + Math.Pow(a.Y - b.Y, 2));
    }
}