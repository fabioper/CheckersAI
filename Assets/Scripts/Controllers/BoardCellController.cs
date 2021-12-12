using UnityEngine;

namespace Controllers
{
    public class BoardCellController : MonoBehaviour
    {
        [SerializeField] public Material selectedMaterial;
        [SerializeField] public Material defaultMaterial;
    
        private Renderer _objectRenderer;

        private void Start() => _objectRenderer = GetComponent<MeshRenderer>();

        private void OnMouseEnter() => _objectRenderer.material = selectedMaterial;

        private void OnMouseExit() => _objectRenderer.material = defaultMaterial;

        void OnMouseOver()
        {
            if (Input.GetMouseButtonDown(0))
                Select();
        }

        private void Select()
        {
            if (!BoardGridController.Instance.HasSelection())
            {
                if (Piece == null)
                    return;

                if (GameController.Instance.IsTurn(Piece.Color))
                    BoardGridController.Instance.SelectedCell = this;
            }
            else
            {
                if (DeselectIfAlreadySelected()) return;
                var selectedCellPiece = BoardGridController.Instance.SelectedCell.Piece;

                if (Piece != null || !selectedCellPiece)
                    return;

                if (BoardGridController.Instance.CanMoveTo(selectedCellPiece, this, out var move).Result && move.HasValue)
                {
                    BoardGridController.Instance.MoveTo(selectedCellPiece, move.Value);
                    BoardGridController.Instance.SelectedCell.Piece = null;
                    BoardGridController.Instance.SelectedCell = null;
                }
            }
        }

        private bool DeselectIfAlreadySelected()
        {
            if (!BoardGridController.Instance.IsSelected(this))
                return false;

            BoardGridController.Instance.SelectedCell = null;
            return true;
        }

        private void Update()
        {
            _objectRenderer.material = BoardGridController.Instance.SelectedCell == this ? selectedMaterial : defaultMaterial;

            if (BoardGridController.Instance.SelectedCell != null && BoardGridController.Instance.SelectedCell != this)
            {
                var selectedPiece = BoardGridController.Instance.SelectedCell.Piece;
                if (selectedPiece == null) return;
                
                if (BoardGridController.Instance.CanMoveTo(selectedPiece, this, out _).Result)
                    _objectRenderer.material = selectedMaterial;
            }
        }
        
        public BoardCellController Clone()
        {
            var boardCell = (BoardCellController)MemberwiseClone();
            boardCell.Piece = Piece.Clone();

            return boardCell;
        }
    }
}