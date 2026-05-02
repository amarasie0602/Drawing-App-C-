using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

public class LineShape : Shape
{
    public int X1 { get; set; }
    public int Y1 { get; set; }
    public int X2 { get; set; }
    public int Y2 { get; set; }

    public override Rectangle Bounds
    {
        get
        {
            return new Rectangle(
                Math.Min(X1, X2),
                Math.Min(Y1, Y2),
                Math.Abs(X2 - X1),
                Math.Abs(Y2 - Y1));
        }
    }

    public override void Draw(Graphics g)
    {
        Pen pen = new Pen(BorderColor, BorderWidth);
        pen.StartCap = LineCap.Round;
        pen.EndCap = LineCap.Round;
        g.DrawLine(pen, X1, Y1, X2, Y2);
        pen.Dispose();
    }

    // Click anywhere close to the line, not just its bounding box
    public override bool HitTest(Point p)
    {
        double threshold = Math.Max(BorderWidth / 2.0 + 4, 5);
        return DistanceToLine(p) <= threshold;
    }

    public override void Move(int dx, int dy)
    {
        X1 += dx; Y1 += dy;
        X2 += dx; Y2 += dy;
    }

    // Two handles, one on each end of the line
    public override List<ShapeHandle> GetHandles()
    {
        List<ShapeHandle> handles = new List<ShapeHandle>();
        handles.Add(new ShapeHandle(0, new Point(X1, Y1), Cursors.Cross));
        handles.Add(new ShapeHandle(1, new Point(X2, Y2), Cursors.Cross));
        return handles;
    }

    public override Point[] GetHandlePoints()
    {
        return new Point[] { new Point(X1, Y1), new Point(X2, Y2) };
    }

    public override void ApplyHandle(int index, Point newPoint, Point[] originalPoints)
    {
        if (index == 0) { X1 = newPoint.X; Y1 = newPoint.Y; }
        else            { X2 = newPoint.X; Y2 = newPoint.Y; }
    }

    public override object SaveGeometry()
    {
        return (X1, Y1, X2, Y2);
    }

    public override void RestoreGeometry(object savedState)
    {
        var state = ((int, int, int, int))savedState;
        X1 = state.Item1;
        Y1 = state.Item2;
        X2 = state.Item3;
        Y2 = state.Item4;
    }

    // Gets perpendicular distance from point p to the line segment
    private double DistanceToLine(Point p)
    {
        double dx = X2 - X1;
        double dy = Y2 - Y1;

        if (dx == 0 && dy == 0)
        {
            // Line is actually just a point
            return Math.Sqrt(Math.Pow(p.X - X1, 2) + Math.Pow(p.Y - Y1, 2));
        }

        double t = ((p.X - X1) * dx + (p.Y - Y1) * dy) / (dx * dx + dy * dy);
        t = Math.Max(0, Math.Min(1, t));

        double nearestX = X1 + t * dx;
        double nearestY = Y1 + t * dy;

        return Math.Sqrt(Math.Pow(p.X - nearestX, 2) + Math.Pow(p.Y - nearestY, 2));
    }
}