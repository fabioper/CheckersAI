using System;
using UnityEngine;

namespace Controllers
{
    public class PieceController : MonoBehaviour
    {
        private void Awake() => GameController.Instance.Pieces.Add(this);

        public PieceController Clone() => Instantiate(this, null, false);
    }
}