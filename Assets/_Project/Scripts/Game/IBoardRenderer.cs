using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match3
{
    public interface IBoardRenderer
    {
        void InitializeBoard (Piece[, ] pieces);
        void PiecesChanged (List<Piece> pieces);
        void OnPieceClicked (Vector2Int coor);
        void OnPieceSwap (Vector2Int coor);
        void OnPieceUp (Vector2Int coor);
        void OnPieceEnter (Vector2Int coor);
        void OnPieceExit (Vector2Int coor);
        IEnumerator GenerateGem (List<Piece> cords);
        IEnumerator FallBy (List<Piece> pieces, int by);
        IEnumerator PostFall (List<Piece> matches);
        IEnumerator Match (List<Piece> pieces);
        IEnumerator MovingPiece (Vector2Int from, Vector2Int to);
    }
}