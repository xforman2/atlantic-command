using Godot;
using System;

public abstract partial class ResourceManager : GodotObject
{
    public int Wood { get; protected set; } = 100;
    public int Coal { get; protected set; } = 100;
    public int Iron { get; protected set; } = 100;
    public int Copper { get; protected set; } = 100;

    public virtual void IncreaseWood(int amount)
    {
        if (amount <= 0) return;
        Wood += amount;
    }

    public virtual void DecreaseWood(int amount)
    {
        if (amount <= 0) return;
        Wood = Math.Max(0, Wood - amount);
    }

    public virtual void IncreaseCoal(int amount)
    {
        if (amount <= 0) return;
        Coal += amount;
    }

    public virtual void DecreaseCoal(int amount)
    {
        if (amount <= 0) return;
        Coal = Math.Max(0, Coal - amount);
    }

    public virtual void IncreaseIron(int amount)
    {
        if (amount <= 0) return;
        Iron += amount;
    }

    public virtual void DecreaseIron(int amount)
    {
        if (amount <= 0) return;
        Iron = Math.Max(0, Iron - amount);
    }

    public virtual void IncreaseCopper(int amount)
    {
        if (amount <= 0) return;
        Copper += amount;
    }

    public virtual void DecreaseCopper(int amount)
    {
        if (amount <= 0) return;
        Copper = Math.Max(0, Copper - amount);
    }
}
