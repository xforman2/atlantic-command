
using Godot;

public partial class SharedOverlay : Control
{
    private Label woodLabel;
    private Label scrapLabel;
    private Label ironLabel;
    private Label tridentisLabel;

    public override void _Ready()
    {

        woodLabel = GetNode<Label>("ResourceBar/WoodBox/WoodLabel");
        scrapLabel = GetNode<Label>("ResourceBar/ScrapBox/ScrapLabel");
        ironLabel = GetNode<Label>("ResourceBar/IronBox/IronLabel");
        tridentisLabel = GetNode<Label>("ResourceBar/TridentisBox/TridentisLabel");


        ResourceMediator.OnResourceChanged += OnResourceChanged;
        ResourceMediator.NotifyResourceWanted(this);
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
            case nameof(PlayerResourceManager.Scrap):
                scrapLabel.Text = $"{newAmount}";
                break;
            case nameof(PlayerResourceManager.Iron):
                ironLabel.Text = $"{newAmount}";
                break;
            case nameof(PlayerResourceManager.Tridentis):
                tridentisLabel.Text = $"{newAmount}";
                break;
        }
    }
}
