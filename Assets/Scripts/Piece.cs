using UnityEngine;

public class Piece : MonoBehaviour
{
    public TeamColor TeamColor { get; set; }

    public void MoveTo(BoardCell cell)
    {
        if (cell.CurrentPiece != null)
            return;

        cell.CurrentPiece = this;
        transform.position = cell.transform.position;
    }
}
