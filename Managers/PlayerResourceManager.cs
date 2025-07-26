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

    public override void IncreaseScrap(int amount)
    {
        base.IncreaseScrap(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Scrap), Scrap);
    }

    public override void DecreaseScrap(int amount)
    {
        base.DecreaseScrap(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Scrap), Scrap);
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

    public override void IncreaseTridentis(int amount)
    {
        base.IncreaseTridentis(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Tridentis), Tridentis);
    }

    public override void DecreaseTridentis(int amount)
    {
        base.DecreaseTridentis(amount);
        ResourceMediator.NotifyResourceChanged(this, nameof(Tridentis), Tridentis);
    }
}
