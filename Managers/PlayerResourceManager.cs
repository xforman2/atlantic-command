using Godot;
using System;

public partial class PlayerResourceManager : ResourceManager
{

    public PlayerResourceManager()
    {
        ResourceMediator.OnResourceWanted += HandleResourceRequest;
    }

    private void HandleResourceRequest(object requester)
    {
        ResourceMediator.NotifyResourceChanged(this, nameof(Wood), Wood);
        ResourceMediator.NotifyResourceChanged(this, nameof(Coal), Coal);
        ResourceMediator.NotifyResourceChanged(this, nameof(Iron), Iron);
        ResourceMediator.NotifyResourceChanged(this, nameof(Copper), Copper);
    }

    public override void IncreaseWood(int amount)
    {
        base.IncreaseWood(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Wood), Wood);
    }

    public override void DecreaseWood(int amount)
    {
        base.DecreaseWood(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Wood), Wood);
    }

    public override void IncreaseCoal(int amount)
    {
        base.IncreaseCoal(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Coal), Coal);
    }

    public override void DecreaseCoal(int amount)
    {
        base.DecreaseCoal(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Coal), Coal);
    }

    public override void IncreaseIron(int amount)
    {
        base.IncreaseIron(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Iron), Iron);
    }

    public override void DecreaseIron(int amount)
    {
        base.DecreaseIron(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Iron), Iron);
    }

    public override void IncreaseCopper(int amount)
    {
        base.IncreaseCopper(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Copper), Copper);
    }

    public override void DecreaseCopper(int amount)
    {
        base.DecreaseCopper(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Copper), Copper);
    }
}
