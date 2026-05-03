using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class DrawingDocument
{
    public List<Shape> Shapes { get; } = new List<Shape>();

    private Stack<ICommand> undoStack = new Stack<ICommand>();
    private Stack<ICommand> redoStack = new Stack<ICommand>();

    public bool IsDirty { get; private set; }
    public string FilePath { get; set; } = "";

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    public event EventHandler Changed;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    public bool CanUndo { get { return undoStack.Count > 0; } }
    public bool CanRedo { get { return redoStack.Count > 0; } }

    public string NextUndoDescription
    {
        get { return undoStack.Count > 0 ? undoStack.Peek().Description : ""; }
    }

    public string NextRedoDescription
    {
        get { return redoStack.Count > 0 ? redoStack.Peek().Description : ""; }
    }

    // Run a command and add it to the undo history
    public void Execute(ICommand cmd)
    {
        cmd.Execute();
        undoStack.Push(cmd);
        redoStack.Clear();
        IsDirty = true;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    // For commands that were already applied (like live drag), just record them
    public void RecordCommand(ICommand cmd)
    {
        undoStack.Push(cmd);
        redoStack.Clear();
        IsDirty = true;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Undo()
    {
        if (!CanUndo) return;
        ICommand cmd = undoStack.Pop();
        cmd.Undo();
        redoStack.Push(cmd);
        IsDirty = true;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Redo()
    {
        if (!CanRedo) return;
        ICommand cmd = redoStack.Pop();
        cmd.Execute();
        undoStack.Push(cmd);
        IsDirty = true;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        Shapes.Clear();
        undoStack.Clear();
        redoStack.Clear();
        IsDirty = false;
        FilePath = "";
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void MarkSaved()
    {
        IsDirty = false;
        Changed?.Invoke(this, EventArgs.Empty);
    }

    // Save and load using JSON
    private static JsonSerializerOptions jsonOptions = new JsonSerializerOptions { WriteIndented = true };

    public void Save(string path)
    {
        List<Shape> copy = new List<Shape>(Shapes);
        string json = JsonSerializer.Serialize(copy, jsonOptions);
        File.WriteAllText(path, json);
        FilePath = path;
        MarkSaved();
    }

    public void Load(string path)
    {
        string json = File.ReadAllText(path);
        List<Shape>? loaded = JsonSerializer.Deserialize<List<Shape>>(json, jsonOptions);

        Shapes.Clear();
        if (loaded != null)
            Shapes.AddRange(loaded);

        undoStack.Clear();
        redoStack.Clear();
        FilePath = path;
        IsDirty = false;
        Changed?.Invoke(this, EventArgs.Empty);
    }
}