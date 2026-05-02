using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class TriangleShape : Shape
{
    public int[] PX { get; set; } = new int[3];
    public int[] PY { get; set; } = new int[3];

    public Point[] GetPoints()
    {
        return new Point[]
        {
            new Point(PX[0], PY[0]),
            new Point(PX[1], PY[1]),
            new Point(PX[2], PY[2])
        };
    }

    public override Rectangle Bounds
    {
        get
        {
            int minX = Math.Min(PX[0], Math.Min(PX[1], PX[2]));
            int minY = Math.Min(PY[0], Math.Min(PY[1], PY[2]));
            int maxX = Math.Max(PX[0], Math.Max(PX[1], PX[2]));
            int maxY = Math.Max(PY[0], Math.Max(PY[1], PY[2]));
            return new Rectangle(minX, minY, maxX - minX, maxY - minY);
        }
    }

    public override void Draw(Graphics g)
    {
        Point[] pts = GetPoints();

        SolidBrush brush = new SolidBrush(FillColor);
        Pen pen = new Pen(BorderColor, BorderWidth);

        g.FillPolygon(brush, pts);
        g.DrawPolygon(pen, pts);

        brush.Dispose();
        pen.Dispose();
    }

    public override bool HitTest(Point p)
    {
        Point[] pts = GetPoints();
        return PointInTriangle(p, pts[0], pts[1], pts[2]);
    }

    public override void Move(int dx, int dy)
    {
        for (int i = 0; i < 3; i++)
        {
            PX[i] += dx;
            PY[i] += dy;
        }
    }

    // Each vertex gets its own handle
    public override List<ShapeHandle> GetHandles()
    {
        Point[] pts = GetPoints();
        List<ShapeHandle> handles = new List<ShapeHandle>();
        for (int i = 0; i < pts.Length; i++)
            handles.Add(new ShapeHandle(i, pts[i], Cursors.SizeAll));
        return handles;
    }

    public override Point[] GetHandlePoints()
    {
        return GetPoints();
    }

    public override void ApplyHandle(int index, Point newPoint, Point[] originalPoints)
    {
        PX[index] = newPoint.X;
        PY[index] = newPoint.Y;
    }

    public override object SaveGeometry()
    {
        return ((int[])PX.Clone(), (int[])PY.Clone());
    }

    public override void RestoreGeometry(object savedState)
    {
        var state = ((int[], int[]))savedState;
        PX = (int[])state.Item1.Clone();
        PY = (int[])state.Item2.Clone();
    }

    // Standard point-in-triangle test using cross products
    private bool PointInTriangle(Point p, Point a, Point b, Point c)
    {
        double d1 = CrossSign(p, a, b);
        double d2 = CrossSign(p, b, c);
        double d3 = CrossSign(p, c, a);

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }

    private double CrossSign(Point p1, Point p2, Point p3)
    {
        return (p1.X - p3.X) * (double)(p2.Y - p3.Y)
             - (p2.X - p3.X) * (double)(p1.Y - p3.Y);
    }
}