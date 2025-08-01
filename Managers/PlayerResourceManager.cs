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
        ResourceMediator.NotifyResourceChanged(this, nameof(Scrap), Scrap);
        ResourceMediator.NotifyResourceChanged(this, nameof(Iron), Iron);
        ResourceMediator.NotifyResourceChanged(this, nameof(Tridentis), Tridentis);
    }

    protected override void IncreaseWood(int amount)
    {
        base.IncreaseWood(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Wood), Wood);
    }

    protected override void DecreaseWood(int amount)
    {
        base.DecreaseWood(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Wood), Wood);
    }

    protected override void IncreaseScrap(int amount)
    {
        base.IncreaseScrap(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Scrap), Scrap);
    }

    protected override void DecreaseScrap(int amount)
    {
        base.DecreaseScrap(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Scrap), Scrap);
    }

    protected override void IncreaseIron(int amount)
    {
        base.IncreaseIron(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Iron), Iron);
    }

    protected override void DecreaseIron(int amount)
    {
        base.DecreaseIron(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Iron), Iron);
    }

    protected override void IncreaseTridentis(int amount)
    {
        base.IncreaseTridentis(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Tridentis), Tridentis);
    }

    protected override void DecreaseTridentis(int amount)
    {
        base.DecreaseTridentis(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Tridentis), Tridentis);
    }
}
