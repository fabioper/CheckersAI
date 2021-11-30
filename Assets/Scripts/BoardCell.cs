using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardCell : MonoBehaviour
{
    [SerializeField] public Material selectedMaterial;
    [SerializeField] public Material defaultMaterial;
    
    public Piece CurrentPiece { get; set; }
    public CellCoordinates CellCoordinates { get; set; }

    private Renderer _objectRenderer;

    private void Start()
    {
        _objectRenderer = GetComponent<MeshRenderer>();
    }

    private void OnMouseEnter()
    {
        _objectRenderer.material = selectedMaterial;
    }

    private void OnMouseExit()
    {
        _objectRenderer.material = defaultMaterial;
    }

    void OnMouseOver()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!BoardGrid.Instance.HasSelection())
            {
                if (CurrentPiece == null)
                    return;

                if (GameController.Instance.IsTurn(CurrentPiece.TeamColor))
                    BoardGrid.Instance.SelectedCell = this;
            }
            else
            {
                if (DeselectIfAlreadySelected()) return;
                var selectedCellPiece = BoardGrid.Instance.SelectedCell.CurrentPiece;

                if (CurrentPiece != null || !selectedCellPiece)
                    return;

                if (!selectedCellPiece.CanMoveTo(this, out var move))
                    return;

                var pathUntilThis = new List<BoardCell>();
                foreach (var cell in move.Path)
                {
                    pathUntilThis.Add(cell);

                    if (cell == this)
                        break;
                }
                move.Path = pathUntilThis;
                selectedCellPiece.MoveTo(move);
                BoardGrid.Instance.SelectedCell.CurrentPiece = null;
                BoardGrid.Instance.SelectedCell = null;
            }
        }
    }

    private bool DeselectIfAlreadySelected()
    {
        if (!BoardGrid.Instance.IsSelected(this))
            return false;

        BoardGrid.Instance.SelectedCell = null;
        return true;
    }

    private void Update()
    {
        _objectRenderer.material = BoardGrid.Instance.SelectedCell == this ? selectedMaterial : defaultMaterial;

        if (BoardGrid.Instance.SelectedCell != null && BoardGrid.Instance.SelectedCell != this)
        {
            var selectedPiece = BoardGrid.Instance.SelectedCell.CurrentPiece;

            if (selectedPiece.CanMoveTo(this, out var moves))
            {
                _objectRenderer.material = selectedMaterial;
            }
        }
    }

    public bool IsEmpty()
    {
        return CurrentPiece == null;
    }
}
