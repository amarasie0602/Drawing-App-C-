using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.Json.Serialization;

// Small helper that represents one resize handle on a shape
public class ShapeHandle
{
    public int Index { get; set; }
    public Rectangle Rect { get; set; }
    public Cursor Cursor { get; set; }

    public ShapeHandle(int index, Point center, Cursor cursor)
    {
        Index = index;
        Cursor = cursor;
        Rect = new Rectangle(center.X - 5, center.Y - 5, 10, 10);
    }
}

// Base class for all shapes. Uses JSON polymorphism to allow saving/loading a list of different shape types.
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(RectangleShape), "rect")]
[JsonDerivedType(typeof(CircleShape), "circle")]
[JsonDerivedType(typeof(TriangleShape), "triangle")]
[JsonDerivedType(typeof(LineShape), "line")]
[JsonDerivedType(typeof(PolygonShape), "polygon")]

// Note: System.Text.Json does not support polymorphic deserialization of abstract classes, so we have to use a workaround by adding a dummy property in each derived class. See
public abstract class Shape
{
    // Storing as int because System.Text.Json cant serialize Color directly
    public int FillArgb { get; set; } = Color.LightBlue.ToArgb();
    public int BorderArgb { get; set; } = Color.Black.ToArgb();
    public int BorderWidth { get; set; } = 2;

    [JsonIgnore]
  
  // Expose Color properties for easier use in code, but ignore them in JSON since we are storing as ARGB ints
    public Color FillColor
    {
        get { return Color.FromArgb(FillArgb); }
        set { FillArgb = value.ToArgb(); }
    }

    [JsonIgnore]
    public Color BorderColor
    {
        get { return Color.FromArgb(BorderArgb); }
        set { BorderArgb = value.ToArgb(); }
    }

    public abstract Rectangle Bounds { get; }
    public abstract void Draw(Graphics g);
    public abstract bool HitTest(Point p);
    public abstract void Move(int dx, int dy);
    public abstract List<ShapeHandle> GetHandles();
    public abstract Point[] GetHandlePoints();
    public abstract void ApplyHandle(int index, Point newPoint, Point[] originalPoints);
    public abstract object SaveGeometry();
    public abstract void RestoreGeometry(object savedState);

    // Draws the dashed selection box and handles around the shape
    public void DrawSelection(Graphics g)
    {
        Rectangle bounds = Bounds;
        bounds.Inflate(3, 3);

        Pen dashPen = new Pen(Color.DodgerBlue, 1.5f);
        dashPen.DashStyle = DashStyle.Dash;
        g.DrawRectangle(dashPen, bounds);
        dashPen.Dispose();

        foreach (ShapeHandle handle in GetHandles())
        {
            g.FillRectangle(Brushes.White, handle.Rect);
            g.DrawRectangle(Pens.DodgerBlue, handle.Rect);
        }
    }
}