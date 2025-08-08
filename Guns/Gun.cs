using Godot;
using System;

public abstract partial class Gun : BuildableStructure
{
    public abstract void Shoot(Vector2? target = null);
}
