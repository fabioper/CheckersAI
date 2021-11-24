using UnityEngine;

public class BoardCell : MonoBehaviour
{
    private Renderer _objectRenderer;
    public Piece CurrentPiece { get; set; }
    public BoardMovementControl boardControl;

    private void Start()
    {
        _objectRenderer = GetComponent<MeshRenderer>();
        boardControl = BoardMovementControl.Instance;
    }

    void OnMouseEnter() => _objectRenderer.enabled = true;

    void OnMouseExit() => _objectRenderer.enabled = false;

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (boardControl.SelectedCell is null)
            {
                boardControl.SelectedCell = this;
            }
            else if (CurrentPiece is null)
            {
                var piece = boardControl.SelectedCell.CurrentPiece;
                if (!piece) return;
                MovePiece(piece);
                boardControl.SelectedCell.CurrentPiece = null;
                boardControl.SelectedCell = null;
            }
        }
    }

    public void MovePiece(Piece piece)
    {
        CurrentPiece = piece;
        piece.transform.position = transform.position;
    }
}
