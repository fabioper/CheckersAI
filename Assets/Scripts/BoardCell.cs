using UnityEngine;

public class BoardCell : MonoBehaviour
{
    [SerializeField] public Material selectedMaterial;
    [SerializeField] public Material defaultMaterial;
    
    public Piece CurrentPiece { get; set; }
    public CellCoordinates CellCoordinates { get; set; }

    private Renderer _objectRenderer;
    private BoardMovementControl _boardControl;

    private void Start()
    {
        _objectRenderer = GetComponent<MeshRenderer>();
        _boardControl = BoardMovementControl.Instance;
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
            if (_boardControl.SelectedCell is null)
            {
                if (CurrentPiece != null)
                {
                    _boardControl.SelectedCell = this;
                }
            }
            else
            {
                if (_boardControl.SelectedCell == this)
                {
                    _boardControl.SelectedCell = null;
                    return;
                }
                var selectedCellPiece = _boardControl.SelectedCell.CurrentPiece;
                
                if (CurrentPiece != null || !selectedCellPiece || !selectedCellPiece.CanMoveTo(this))
                    return;
                
                selectedCellPiece.MoveTo(this);
                _boardControl.SelectedCell.CurrentPiece = null;
                _boardControl.SelectedCell = null;
            }
        }
    }

    private void Update()
    {
        _objectRenderer.material = _boardControl.SelectedCell == this ? selectedMaterial : defaultMaterial;

        if (_boardControl.SelectedCell != null && _boardControl.SelectedCell != this)
        {
            var selectedPiece = _boardControl.SelectedCell.CurrentPiece;
            var possibleMoves = selectedPiece.GetPossibleMoves();
            
            if (possibleMoves.Contains(this))
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
