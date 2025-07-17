using System;
using Godot;

public partial class ResourceMediator : Node
{
	public static event Action<object, string, int> OnResourceChanged;

	public static void NotifyResourceChanged(object sender, string resourceName, int newAmount)
	{
		OnResourceChanged?.Invoke(sender, resourceName, newAmount);
	}

}
