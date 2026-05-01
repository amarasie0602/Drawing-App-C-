using System.Drawing;

abstract class Shape
{
    public Color FillColor { get; set; }
    public Color BorderColor { get; set; }
    public int BorderWidth { get; set; }
    public Rectangle Bounds { get; set; }

    public abstract void Draw(Graphics g);

    public virtual bool Contains(Point p)
    {
        return Bounds.Contains(p);
    }
}