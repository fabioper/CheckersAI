using System.Collections.Generic;
using System.Linq;

namespace Models
{
    public class Game
    {
        public Game()
        {
            Board = new Board(this);
            ActiveTeam = TeamColor.White;
        }
        public TeamColor ActiveTeam { get; set; }
        public Board Board { get; set; }

        public bool IsTurn(TeamColor teamColor) => ActiveTeam == teamColor;

        public void ChangeTurn() => ActiveTeam = ActiveTeam == TeamColor.White ? TeamColor.Black : TeamColor.White;
        
        public HashSet<Piece> Pieces { get; set; }
        public TeamColor? Winner { get; set; }

        public HashSet<Piece> WhitePieces
            => new HashSet<Piece>(Pieces.Where(x => x.IsTeam(TeamColor.White)));
        public HashSet<Piece> BlackPieces
            => new HashSet<Piece>(Pieces.Where(x => x.IsTeam(TeamColor.Black)));
        public HashSet<Piece> WhiteKings
            => new HashSet<Piece>(Pieces.Where(x => x.IsTeam(TeamColor.Black) && x.IsKing));
        public HashSet<Piece> BlackKings
            => new HashSet<Piece>(Pieces.Where(x => x.IsTeam(TeamColor.Black) && x.IsKing));
    }
}