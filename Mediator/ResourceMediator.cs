using System;
using Godot;

public partial class ResourceMediator : Node
{
    public static event Action<object, string, int> OnResourceChanged;

    public static event Action<object> OnResourceWanted;

    public static void NotifyResourceChanged(object sender, string resourceName, int newAmount)
    {
        OnResourceChanged?.Invoke(sender, resourceName, newAmount);
    }

    public static void NotifyResourceWanted(object sender)
    {
        OnResourceWanted?.Invoke(sender);
    }
}
