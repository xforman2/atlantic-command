using Godot;
using System;

public abstract partial class ResourceManager : GodotObject
{
    public int Wood { get; protected set; } = 100;
    public int Scrap { get; protected set; } = 100;
    public int Iron { get; protected set; } = 100;
    public int Tridentis { get; protected set; } = 100;

    public virtual void IncreaseResource(ResourceEnum resource, int amount)
    {
        if (amount <= 0) return;

        switch (resource)
        {
            case ResourceEnum.Wood:
                IncreaseWood(amount);
                break;
            case ResourceEnum.Scrap:
                IncreaseScrap(amount);
                break;
            case ResourceEnum.Iron:
                IncreaseIron(amount);
                break;
            case ResourceEnum.Tridentis:
                IncreaseTridentis(amount);
                break;
        }
    }

    public virtual void DecreaseResource(ResourceEnum resource, int amount)
    {
        if (amount <= 0) return;

        switch (resource)
        {
            case ResourceEnum.Wood:
                DecreaseWood(amount);
                break;
            case ResourceEnum.Scrap:
                DecreaseScrap(amount);
                break;
            case ResourceEnum.Iron:
                DecreaseIron(amount);
                break;
            case ResourceEnum.Tridentis:
                DecreaseTridentis(amount);
                break;
        }
    }

    protected virtual void IncreaseWood(int amount)
    {
        Wood += amount;
    }

    protected virtual void DecreaseWood(int amount)
    {
        Wood = Math.Max(0, Wood - amount);
    }

    protected virtual void IncreaseScrap(int amount)
    {
        Scrap += amount;
    }

    protected virtual void DecreaseScrap(int amount)
    {
        Scrap = Math.Max(0, Scrap - amount);
    }

    protected virtual void IncreaseIron(int amount)
    {
        Iron += amount;
    }

    protected virtual void DecreaseIron(int amount)
    {
        Iron = Math.Max(0, Iron - amount);
    }

    protected virtual void IncreaseTridentis(int amount)
    {
        Tridentis += amount;
    }

    protected virtual void DecreaseTridentis(int amount)
    {
        Tridentis = Math.Max(0, Tridentis - amount);
    }
}
