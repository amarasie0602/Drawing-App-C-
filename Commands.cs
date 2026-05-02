using System;
using System.Collections.Generic;

public interface ICommand
{
    string Description { get; }
    void Execute();
    void Undo();
}

// Command for when a new shape is added to the canvas
public class AddShapeCommand : ICommand
{
    private List<Shape> shapeList;
    private Shape shape;

    public string Description { get; }

    public AddShapeCommand(List<Shape> list, Shape s)
    {
        shapeList = list;
        shape = s;
        Description = "Add " + s.GetType().Name.Replace("Shape", "");
    }

    public void Execute()
    {
        shapeList.Add(shape);
    }

    public void Undo()
    {
        shapeList.Remove(shape);
    }
}

// Command for when a shape is deleted
public class DeleteShapeCommand : ICommand
{
    private List<Shape> shapeList;
    private Shape shape;
    private int indexInList;

    public string Description { get; }

    public DeleteShapeCommand(List<Shape> list, Shape s)
    {
        shapeList = list;
        shape = s;
        Description = "Delete " + s.GetType().Name.Replace("Shape", "");
    }

    public void Execute()
    {
        indexInList = shapeList.IndexOf(shape);
        shapeList.Remove(shape);
    }

    public void Undo()
    {
        if (indexInList >= 0 && indexInList <= shapeList.Count)
            shapeList.Insert(indexInList, shape);
        else
            shapeList.Add(shape);
    }
}

// Command for move and resize operations - saves geometry before and after
public class MoveResizeCommand : ICommand
{
    private Shape shape;
    private object stateBefore;
    private object stateAfter;

    public string Description { get; }

    public MoveResizeCommand(Shape s, object before, object after, string desc)
    {
        shape = s;
        stateBefore = before;
        stateAfter = after;
        Description = desc;
    }

    public void Execute()
    {
        shape.RestoreGeometry(stateAfter);
    }

    public void Undo()
    {
        shape.RestoreGeometry(stateBefore);
    }
}

// General purpose command for property changes like color or border width
public class PropertyChangeCommand : ICommand
{
    private Action doAction;
    private Action undoAction;

    public string Description { get; }

    public PropertyChangeCommand(string description, Action execute, Action undo)
    {
        Description = description;
        doAction = execute;
        undoAction = undo;
    }

    public void Execute()
    {
        doAction();
    }

    public void Undo()
    {
        undoAction();
    }
}