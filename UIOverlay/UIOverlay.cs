
using Godot;

public partial class UIOverlay : Control
{
    private Label woodLabel;
    private Label coalLabel;
    private Label ironLabel;
    private Label copperLabel;

    public override void _Ready()
    {

        woodLabel = GetNode<Label>("ResourceBar/WoodBox/WoodLabel");
        coalLabel = GetNode<Label>("ResourceBar/CoalBox/CoalLabel");
        ironLabel = GetNode<Label>("ResourceBar/IronBox/IronLabel");
        copperLabel = GetNode<Label>("ResourceBar/CopperBox/CopperLabel");


        ResourceMediator.OnResourceChanged += OnResourceChanged;
        ResourceMediator.NotifyResourceWanted(this); //
    }

    public override void _ExitTree()
    {
        ResourceMediator.OnResourceChanged -= OnResourceChanged;
    }

    private void OnResourceChanged(object sender, string resourceName, int newAmount)
    {
        switch (resourceName)
        {
            case nameof(PlayerResourceManager.Wood):
                woodLabel.Text = $"{newAmount}";
                break;
            case nameof(PlayerResourceManager.Coal):
                coalLabel.Text = $"{newAmount}";
                break;
            case nameof(PlayerResourceManager.Iron):
                ironLabel.Text = $"{newAmount}";
                break;
            case nameof(PlayerResourceManager.Copper):
                copperLabel.Text = $"{newAmount}";
                break;
        }
    }
}
