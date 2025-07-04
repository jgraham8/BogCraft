namespace BogCraft.UI.Services;

public class PlayerUpdateEventArgs : EventArgs
{
    public int Count { get; set; }
    public List<string> Players { get; set; } = [];
}