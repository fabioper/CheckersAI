using System.Collections.Generic;
using UnityEngine;

public class BoardCell : MonoBehaviour
{
    [SerializeField] public Material selectedMaterial;
    [SerializeField] public Material defaultMaterial;
    
    public Piece Piece { get; set; }
    public CellCoordinates Position { get; set; }

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
                if (Piece == null)
                    return;

                if (GameController.Instance.IsTurn(Piece.Color))
                    BoardGrid.Instance.SelectedCell = this;
            }
            else
            {
                if (DeselectIfAlreadySelected()) return;
                var selectedCellPiece = BoardGrid.Instance.SelectedCell.Piece;

                if (Piece != null || !selectedCellPiece)
                    return;

                if (selectedCellPiece.CanMoveTo(this, out var move).Result && move.HasValue)
                {
                    selectedCellPiece.MoveTo(move.Value);
                    BoardGrid.Instance.SelectedCell.Piece = null;
                    BoardGrid.Instance.SelectedCell = null;
                }
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
            var selectedPiece = BoardGrid.Instance.SelectedCell.Piece;

            if (selectedPiece.CanMoveTo(this, out _).Result)
            {
                _objectRenderer.material = selectedMaterial;
            }
        }
    }

    public bool IsEmpty()
    {
        return Piece == null;
    }
}
