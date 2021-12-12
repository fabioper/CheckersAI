namespace Models
{
    public class Cell
    {
        public Piece Piece { get; set; }
        public CellCoordinates Position { get; set; }
        
        public bool IsEmpty() => Piece == null;

    }
}