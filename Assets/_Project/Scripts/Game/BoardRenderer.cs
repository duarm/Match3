using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using NaughtyAttributes;
using UnityEngine;
using UnityEngine.U2D;

namespace Match3
{
    // UI must display the game state
    public class BoardRenderer : MonoBehaviour, IBoardRenderer
    {
        [Tooltip ("The speed in which the pieces swap when they move.")]
        [SerializeField] public float pieceSwappingSpeed = 0.3f;
        [System.Serializable]
        public class PieceSprite
        {
            [ShowAssetPreview]
            public Sprite sprite;
        }

        [SerializeField] Color mouseHoverPieceColor;
        [SerializeField] Color mouseClickPieceColor;

        [SerializeField] Board board;
        [SerializeField] PieceSprite[] sprites;
        SpriteRenderer[, ] renderers;
        float distanceBetweenTiles;

        private void OnValidate ()
        {
            if (board == null)
                board = FindObjectOfType<Board> ();
        }

        void IBoardRenderer.InitializeBoard ()
        {
            var size = board.Size;
            renderers = new SpriteRenderer[size.x, size.y];
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    var renderer = board.transform.GetChild ((y * size.x) + x)
                        .GetComponent<SpriteRenderer> ();
                    renderers[x, y] = renderer;
                }
            }

            distanceBetweenTiles = Mathf.Abs ((renderers[0, 0].transform.localPosition - renderers[0, 1].transform.position).y);
        }

        void IBoardRenderer.RenderBoard (Piece[, ] pieces)
        {
            for (int y = 0; y < board.Size.y; y++)
            {
                for (int x = 0; x < board.Size.x; x++)
                {
                    if (pieces[x, y].type != -1)
                        GetRenderer (x, y).sprite = sprites[pieces[x, y].type].sprite;
                    else
                        GetRenderer (x, y).sprite = null;
                }
            }
        }

        void IBoardRenderer.PiecesChanged (params Piece[] pieces)
        {
            for (int i = 0; i < pieces.Length; i++)
            {
                var piece = pieces[i];
                var x = piece.coordinates.x;
                var y = piece.coordinates.y;
                if (piece.type != -1)
                    GetRenderer (x, y).sprite = sprites[piece.type].sprite;
                else
                    GetRenderer (x, y).sprite = null;
            }
        }

        public void OnPieceClicked (Vector2Int coor)
        {
            GetRenderer (coor.x, coor.y).color = mouseClickPieceColor;
        }

        public void OnPieceUp (Vector2Int coor)
        {
            GetRenderer (coor.x, coor.y).color = Color.white;
        }

        public void OnPieceEnter (Vector2Int coor)
        {
            GetRenderer (coor.x, coor.y).color = mouseHoverPieceColor;
        }

        public void OnPieceExit (Vector2Int coor)
        {
            GetRenderer (coor.x, coor.y).color = Color.white;
        }

        public IEnumerator MovingPiece (Vector2Int from, Vector2Int to)
        {
            var fromPiece = GetRenderer (from.x, from.y);
            var toPiece = GetRenderer (to.x, to.y);

            fromPiece.transform.DOMove (toPiece.transform.position, pieceSwappingSpeed)
                .SetEase (Ease.InOutQuad);
            yield return toPiece.transform.DOMove (fromPiece.transform.position, pieceSwappingSpeed)
                .SetEase (Ease.InOutQuad).WaitForCompletion ();
            //after the movement, we swap the physical pieces back
            var oldToPos = fromPiece.transform.position;
            fromPiece.transform.position = toPiece.transform.position;
            toPiece.transform.position = oldToPos;
        }

        /// <summary>
        /// Translate cords by the value of by
        /// </summary>
        /// <param name="cords">the pieces to translate down</param>
        /// <param name="by">How much to translate down</param>
        /// <returns></returns>
        public IEnumerator Fall (List<Piece> pieces, List<Piece> to)
        {
            foreach (var match in to)
            {
                GetRenderer (match.coordinates.x, match.coordinates.y).enabled = false;
            }

            var finished = 1;
            for (int i = 0; i < pieces.Count; i++)
            {
                //make the piece falls to match position
                Debug.Log ($"x: {pieces[i].coordinates.x}, y: {pieces[i].coordinates.y} to x: {pieces[i].coordinates.x}, y: {pieces[i].coordinates.y - to.Count}");
                var piece = GetRenderer (pieces[i].coordinates.x, pieces[i].coordinates.y);
                var matchPiece = GetRenderer (pieces[i].coordinates.x, pieces[i].coordinates.y - to.Count);
                var matchPos = matchPiece.transform.localPosition;
                var initialPos = piece.transform.localPosition;
                if (i == pieces.Count - 1)
                    yield return piece.transform.DOMove (matchPos, 0.2f)
                        .SetEase (Ease.InOutQuad)
                        .OnComplete (() =>
                        {
                            to[finished].type = pieces[finished].type;
                            matchPiece.sprite = sprites[to[finished].type].sprite;
                            piece.transform.localPosition = initialPos;
                            finished++;
                        })
                        .WaitForCompletion ();
                else
                    piece.transform.DOMove (matchPos, 0.2f)
                    .SetEase (Ease.InOutQuad)
                    .OnComplete (() =>
                    {
                        to[finished].type = pieces[finished].type;
                        matchPiece.sprite = sprites[to[finished].type].sprite;
                        piece.transform.localPosition = initialPos;
                        finished++;
                    });
            }

            foreach (var match in to)
            {
                GetRenderer (match.coordinates.x, match.coordinates.y).enabled = true;
            }
        }

        public IEnumerator GenerateGemVertical (List<Piece> pieces)
        {
            for (int i = 0; i < pieces.Count; i++)
            {
                var renderer = GetRenderer (pieces[i].coordinates.x, pieces[i].coordinates.y);
                var transf = renderer.transform;
                var initialPos = transf.localPosition;
                renderer.sprite = sprites[pieces[i].type].sprite;
                //Debug.Log($"moving {pieces[i].coordinates} to {}");
                transf.localPosition = new Vector3 (transf.localPosition.x, transf.localPosition.y + (pieces.Count * distanceBetweenTiles));
                if (i == pieces.Count - 1)
                    yield return transf.DOMove (initialPos, 0.2f)
                        .SetEase (Ease.InOutQuad)
                        .WaitForCompletion ();
                else
                    transf.DOMove (initialPos, 0.2f)
                    .SetEase (Ease.InOutQuad);
            }
        }

        public IEnumerator GenerateGemHorizontal (List<Piece> cords)
        {
            throw new NotImplementedException ();
        }

        SpriteRenderer GetRenderer (int x, int y)
        {
            return renderers[x, y];
        }
    }
}