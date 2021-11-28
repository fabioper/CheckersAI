using System.Collections.Generic;

public class PieceMovement
{
    public BoardCell Start { get; set; }
    public BoardCell Destination { get; set; }
    public List<Piece> AttackedPieces { get; set; }

    public PieceMovement()
    {
        AttackedPieces = new List<Piece>();
    }
}
