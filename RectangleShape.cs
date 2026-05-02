using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

public class RectangleShape : Shape
{
    public int X { get; set; }
    public int Y { get; set; }
    public int W { get; set; }
    public int H { get; set; }

    public override Rectangle Bounds
    {
        get { return new Rectangle(X, Y, W, H); }
    }

    public override void Draw(Graphics g)
    {
        if (W < 1 || H < 1) return;

        SolidBrush brush = new SolidBrush(FillColor);
        Pen pen = new Pen(BorderColor, BorderWidth);

        g.FillRectangle(brush, Bounds);
        g.DrawRectangle(pen, Bounds);

        brush.Dispose();
        pen.Dispose();
    }

    public override bool HitTest(Point p)
    {
        return Bounds.Contains(p);
    }

    public override void Move(int dx, int dy)
    {
        X += dx;
        Y += dy;
    }

    // 8 handles around the rectangle
    // 0=TopLeft  1=TopMid  2=TopRight
    // 3=MidLeft            4=MidRight
    // 5=BotLeft  6=BotMid  7=BotRight
    public override List<ShapeHandle> GetHandles()
    {
        int cx = X + W / 2;
        int cy = Y + H / 2;

        List<ShapeHandle> handles = new List<ShapeHandle>();
        handles.Add(new ShapeHandle(0, new Point(X,     Y),     Cursors.SizeNWSE));
        handles.Add(new ShapeHandle(1, new Point(cx,    Y),     Cursors.SizeNS));
        handles.Add(new ShapeHandle(2, new Point(X + W, Y),     Cursors.SizeNESW));
        handles.Add(new ShapeHandle(3, new Point(X,     cy),    Cursors.SizeWE));
        handles.Add(new ShapeHandle(4, new Point(X + W, cy),    Cursors.SizeWE));
        handles.Add(new ShapeHandle(5, new Point(X,     Y + H), Cursors.SizeNESW));
        handles.Add(new ShapeHandle(6, new Point(cx,    Y + H), Cursors.SizeNS));
        handles.Add(new ShapeHandle(7, new Point(X + W, Y + H), Cursors.SizeNWSE));
        return handles;
    }

    public override Point[] GetHandlePoints()
    {
        return new Point[] { new Point(X, Y), new Point(X + W, Y + H) };
    }

    public override void ApplyHandle(int index, Point pt, Point[] orig)
    {
        int left   = orig[0].X;
        int top    = orig[0].Y;
        int right  = orig[1].X;
        int bottom = orig[1].Y;

        if (index == 0) { left = pt.X; top = pt.Y; }
        else if (index == 1) { top = pt.Y; }
        else if (index == 2) { right = pt.X; top = pt.Y; }
        else if (index == 3) { left = pt.X; }
        else if (index == 4) { right = pt.X; }
        else if (index == 5) { left = pt.X; bottom = pt.Y; }
        else if (index == 6) { bottom = pt.Y; }
        else if (index == 7) { right = pt.X; bottom = pt.Y; }

        X = Math.Min(left, right);
        Y = Math.Min(top, bottom);
        W = Math.Max(4, Math.Abs(right - left));
        H = Math.Max(4, Math.Abs(bottom - top));
    }

    public override object SaveGeometry()
    {
        return (X, Y, W, H);
    }

    public override void RestoreGeometry(object savedState)
    {
        var state = ((int, int, int, int))savedState;
        X = state.Item1;
        Y = state.Item2;
        W = state.Item3;
        H = state.Item4;
    }
}