public class BoardMovementControl
{
    public BoardCell SelectedCell { get; set; }
    
    private BoardMovementControl()
    {
    }
    
    public static BoardMovementControl Instance { get; } = new BoardMovementControl();
}
