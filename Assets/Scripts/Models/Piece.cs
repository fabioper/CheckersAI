using System;

namespace Models
{
    public class Piece
    {
        public TeamColor Color { get; set; }
        public Cell Cell { get; set; }

        public bool IsKing { get; set; }
    
        public bool IsTeam(TeamColor teamColor) => teamColor == Color;
    
        private int LastRow => Color switch
        {
            TeamColor.White => 7,
            TeamColor.Black => 0,
            _ => throw new Exception("")
        };
        
        public void PromoteKing()
        {
            IsKing = true;
        }
        public bool ReachedLastRow() => Cell.Position.Row == LastRow;
        
        public void Remove()
        {
            Cell.Piece = null;
        }
    }
}