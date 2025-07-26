using Godot;
using System;

public abstract partial class ResourceManager : GodotObject
{
    public int Wood { get; protected set; } = 100;
    public int Scrap { get; protected set; } = 100;
    public int Iron { get; protected set; } = 100;
    public int Tridentis { get; protected set; } = 100;

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

    public virtual void IncreaseScrap(int amount)
    {
        if (amount <= 0) return;
        Scrap += amount;
    }

    public virtual void DecreaseScrap(int amount)
    {
        if (amount <= 0) return;
        Scrap = Math.Max(0, Scrap - amount);
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

    public virtual void IncreaseTridentis(int amount)
    {
        if (amount <= 0) return;
        Tridentis += amount;
    }

    public virtual void DecreaseTridentis(int amount)
    {
        if (amount <= 0) return;
        Tridentis = Math.Max(0, Tridentis - amount);
    }
}
