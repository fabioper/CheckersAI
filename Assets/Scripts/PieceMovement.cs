using System.Collections.Generic;

public class PieceMovement
{
    public List<BoardCell> Path { get; set; }

    public PieceMovement() => Path = new List<BoardCell>();
}
