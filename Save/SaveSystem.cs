using Godot;
using System;
using System.Text.Json;

public static class SaveSystem
{
    private const string SavePath = "user://save.json";

    public static void SaveGame(ShipSaveData data)
    {
        try
        {
            string json = JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });

            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Write);
            file.StoreString(json);

            GD.Print($"Game saved to {SavePath}");
        }
        catch (Exception e)
        {
            GD.PrintErr($"Failed to save game: {e}");
        }
    }

    public static ShipSaveData LoadShip()
    {
        if (!FileAccess.FileExists(SavePath))
        {
            GD.Print("No save file found at ", SavePath);
            return null;
        }

        try
        {
            using var file = FileAccess.Open(SavePath, FileAccess.ModeFlags.Read);
            var json = file.GetAsText();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            ShipSaveData data = JsonSerializer.Deserialize<ShipSaveData>(json, options);

            GD.Print("Ship loaded successfully.");
            return data;
        }
        catch (Exception e)
        {
            GD.PrintErr("Failed to load ship: ", e.Message);
            return null;
        }
    }

    public static void LoadFromSave(PlayerShip ship, ShipSaveData saveData)
    {
        if (saveData == null) return;

        ship.playerResourceManager.SetResource(ResourceEnum.Wood, saveData.Wood);
        ship.playerResourceManager.SetResource(ResourceEnum.Scrap, saveData.Scrap);
        ship.playerResourceManager.SetResource(ResourceEnum.Iron, saveData.Iron);
        GD.Print(saveData.Tridentis);
        ship.playerResourceManager.SetResource(ResourceEnum.Tridentis, saveData.Tridentis);

        ship.Position = saveData.Position.ToVector2();
        ship.RotationDegrees = saveData.RotationDegrees;

        foreach (var (floor, collision) in ship.Floors.Values)
        {
            floor.QueueFree();
            collision.QueueFree();
        }

        ship.Floors.Clear();

        foreach (var structure in ship.StructuresOrigin.Values)
        {
            structure.QueueFree();
        }
        ship.StructuresOrigin.Clear();

        foreach (var floorData in saveData.Floors)
        {
            var floorPos = floorData.Position.ToVector2I();
            ship.AddFloor(floorPos, floorData.Type);
            ship.UpdateBounds(floorPos);
            if (ship.Floors.TryGetValue(floorData.Position.ToVector2I(), out var floorTuple))
            {
                floorTuple.Item1.TakeDamage(floorTuple.Item1.HP - floorData.HP);
            }
        }

        foreach (var structureData in saveData.Structures)
        {
            ship.PlaceStructure(
                    structureData.Origin.ToVector2I(),
                    structureData.Type,
                    structureData.RotationDegrees
                    );
        }
    }
    public static void DeleteSave()
    {
        try
        {
            if (FileAccess.FileExists(SavePath))
            {
                Error result = DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(SavePath));
                if (result == Error.Ok)
                {
                    GD.Print($"Save file deleted: {SavePath}");
                }
                else
                {
                    GD.PrintErr($"Failed to delete save file. Error: {result}");
                }
            }
            else
            {
                GD.Print("No save file to delete.");
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Exception while deleting save: {e}");
        }
    }

    public static bool HasSave()
    {
        return FileAccess.FileExists(SavePath);
    }
}
