using System;
using UnityEngine;

namespace Controllers
{
    public class PieceController : MonoBehaviour
    {

        public TeamColor Color { get; set; }
        public BoardCellController Cell { get; set; }

        public bool IsKing { get; set; }
    
        public bool IsTeam(TeamColor teamColor) => teamColor == Color;
    
        private int LastRow => Color switch
        {
            TeamColor.White => 7,
            TeamColor.Black => 0,
            _ => throw new Exception("")
        };

        public void SetPosition(BoardCellController cell)
        {
            Cell = cell;
            Cell.Piece = this;
            transform.position = Cell.transform.position;
        }

        public void PromoteKing()
        {
            var meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material.color = IsTeam(TeamColor.White) ? UnityEngine.Color.yellow : UnityEngine.Color.blue;
            IsKing = true;
        }
        public bool ReachedLastRow() => Cell.Position.Row == LastRow;

        private void Awake() => GameController.Instance.Pieces.Add(this);

        public void Remove()
        {
            Destroy(gameObject);
            GameController.Instance.Pieces.Remove(this);
            EventsStore.Instance.NotifyEvent(GameEventType.PieceAttacked);
        }

        public PieceController Clone() => Instantiate(this, null, false);
    }
}