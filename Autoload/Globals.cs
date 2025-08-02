using Godot;
using System;

public partial class Globals : Node
{

    public static int TILE_SIZE = 32;
    public static int MAX_MINING_DISTANCE = 5 * TILE_SIZE;
    public static Vector2 STARTING_POSITION = new Vector2(1080, 190);
}
