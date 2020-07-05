using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using NaughtyAttributes;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Match3
{
    // controls the Game State
    public class Board : MonoBehaviour
    {
        [Header ("Piece")]
        [SerializeField] GameObject piecePrefab;
        [SerializeField] Vector3 pivot;
        [SerializeField] Vector2 spacing = Vector2.one;
        [SerializeField] float scale = 1;
        [Header ("Board")]
        [SerializeField] int pieceCount = 7;
        [SerializeField] Vector2Int size = new Vector2Int (6, 6);

        public Vector2Int Size => size;
        public IBoardRenderer renderer;
        bool updatingBoard = false;

        Vector2Int clickedPiece = new Vector2Int (-1, -1);
        Vector2Int hoverPiece = new Vector2Int (-1, -1);
        readonly Vector2Int noPiece = new Vector2Int (-1, -1);
        Piece[, ] pieces;

        public Piece[, ] Pieces => pieces;
        public Vector2Int HoverPiece => hoverPiece;

        void Start ()
        {
            renderer = GetComponent<IBoardRenderer> ();
            GenerateBoard (Size);
        }

        public void GenerateBoard (Vector2Int size)
        {
            pieces = new Piece[size.x, size.y];
            for (int y = 0; y < size.y; y++)
            {
                for (int x = 0; x < size.x; x++)
                {
                    GameObject go = Instantiate (piecePrefab);
                    go.name = $"x: {x} | y: {y}";
                    go.transform.parent = transform;
                    go.transform.localScale = new Vector2 (scale, scale);
                    go.transform.localPosition = pivot + new Vector3 (x * spacing.x, y * spacing.y, 0);
                    var piece = go.GetComponent<Piece> ();
                    piece.MouseClicked += OnMouseClickPiece;
                    piece.MouseUp += OnMouseUpPiece;
                    piece.MouseEnter += OnMouseEnterPiece;
                    piece.MouseExit += OnMouseExitPiece;
                    piece.coordinates = new Vector2Int (x, y);
                    piece.type = UnityEngine.Random.Range (0, pieceCount);
                    pieces[x, y] = piece;
                }
            }

            renderer.InitializeBoard ();
            renderer.RenderBoard (pieces);
        }

        IEnumerator MovePiece (Vector2Int to)
        {
            updatingBoard = true;
            var fromPiece = pieces[clickedPiece.x, clickedPiece.y];
            var toPiece = pieces[to.x, to.y];
            var toType = toPiece.type;
            //swap the pieces
            toPiece.type = fromPiece.type;
            fromPiece.type = toType;
            ResetMovement ();
            //move the pieces visually
            yield return renderer.MovingPiece (fromPiece.coordinates, to);

            //Debug.Log ($"[MOVING] From {clickedPiece.x}:{clickedPiece.y} t:{pieces[clickedPiece.x,clickedPiece.y].type} to {to.x}:{to.y} t:{pieces[to.x,to.y].type}");
            renderer.RenderBoard (pieces);

            StartCoroutine (UpdateBoard ());
        }

        IEnumerator UpdateBoard ()
        {
            // search pattern
            //1 . # . # . #
            //2 # . # . # .
            //3 . # . # . #
            //4 # . # . # .
            //5 . # . # . #
            //6 # . # . # .

            for (int y = 0; y < size.y; y++)
            {
                for (int x = y % 2; x < size.x; x += 2)
                {
                    var piece = pieces[x, y];

                    var hMatches = new List<Piece> (6);
                    var vMatches = new List<Piece> (6);
                    hMatches.Add (piece);
                    vMatches.Add (piece);
                    SearchForMatches (piece, new Vector2Int (-1, 0), ref hMatches);
                    SearchForMatches (piece, new Vector2Int (1, 0), ref hMatches);
                    SearchForMatches (piece, new Vector2Int (0, -1), ref vMatches);
                    SearchForMatches (piece, new Vector2Int (0, 1), ref vMatches);

                    vMatches = vMatches.OrderBy (f => f.coordinates.y).ToList ();
                    //matched 3 or more
                    if (vMatches.Count >= 3)
                    {
                        var topMatch = vMatches[vMatches.Count - 1];
                        if (PieceAt (topMatch.coordinates.x, topMatch.coordinates.y + 1))
                        {
                            var pieceBuffer = new List<Piece> (6);
                            GetAbove (topMatch.coordinates, ref pieceBuffer);
                            for (int i = 0; i < pieceBuffer.Count; i++)
                            {
                                var newPos = pieceBuffer[i].coordinates;
                                newPos.y -= vMatches.Count;
                                var pieceToFall = GetPiece (pieceBuffer[i].coordinates.x, pieceBuffer[i].coordinates.y);
                                var fallToPiece = GetPiece (newPos.x, newPos.y);
                                fallToPiece.type = pieceToFall.type;
                            }

                            Debug.Break ();
                            yield return renderer.Fall (pieceBuffer, vMatches);
                            int emptyPieces = Mathf.Abs (vMatches.Count - pieceBuffer.Count);
                            for (int i = vMatches.Count - 1; i > vMatches.Count - emptyPieces - 1; i--)
                            {
                                pieceBuffer.Add (vMatches[i]);
                            }

                            for (int i = 0; i < pieceBuffer.Count; i++)
                            {
                                pieceBuffer[i].type = UnityEngine.Random.Range (0, pieceCount);
                            }
                            yield return renderer.GenerateGemVertical (pieceBuffer);
                            // Down pieces.Count by vMatches.Count, and generate pieces.Count - vMatches.Count
                        }
                        else
                        {
                            for (int i = 0; i < vMatches.Count; i++)
                            {
                                vMatches[i].type = UnityEngine.Random.Range (0, pieceCount);
                            }
                            
                            //Generate vMatches.Count
                            yield return renderer.GenerateGemVertical (vMatches);
                        }

                        yield return new WaitForSeconds (0.5f);
                    }

                    if (hMatches.Count >= 3)
                    {
                        hMatches.Add (piece);
                        Debug.Break ();
                        var pieceBuffer = new List<Piece> (6);
                        for (int i = 1; i < hMatches.Count; i++)
                        {
                            /*
                                                        foreach (var item in vMatches)
                            {
                                Debug.Log ("++" + item.coordinates);
                            }
                            int emptyPieces = Mathf.Abs (vMatches.Count - pieceBuffer.Count);
                            for (int i = vMatches.Count - 1; i > vMatches.Count - emptyPieces - 1; i--)
                            {
                                Debug.Log ("i=" + i);
                                Debug.Log ("until:" + (vMatches.Count - emptyPieces - 1));
                                pieceBuffer.Add (vMatches[i]);
                            }
                            
                            foreach (var item in pieceBuffer)
                            {
                                Debug.Log ("--" + item.coordinates);
                            }*/


                            GetAbove (hMatches[i].coordinates, ref pieceBuffer);
                            StartCoroutine (renderer.Fall (pieceBuffer, vMatches));
                            Debug.Break ();
                            for (int t = 0; t < pieceBuffer.Count; t++)
                            {
                                pieceBuffer[t].type = UnityEngine.Random.Range (0, pieceCount);
                            }
                            StartCoroutine (renderer.GenerateGemVertical (hMatches));
                            pieceBuffer.Clear ();
                        }

                        GetAbove (hMatches[0].coordinates, ref pieceBuffer);
                        hMatches[0].type = UnityEngine.Random.Range (0, pieceCount);
                        yield return renderer.Fall (pieceBuffer, vMatches);
                        yield return renderer.GenerateGemVertical (hMatches);
                    }

                }
            }
            updatingBoard = false;
        }

        /// <summary>
        /// returns all the pieces above the given one
        /// </summary>
        void GetAbove (Vector2Int cor, ref List<Piece> pieces)
        {
            if (TryGetPiece (cor.x, cor.y + 1, out var piece))
            {
                pieces.Add (piece);
                GetAbove (piece.coordinates, ref pieces);
            }
        }

        void SearchForMatches (Piece piece, Vector2Int direction, ref List<Piece> matches)
        {
            Piece nextPiece;
            if (TryGetPiece (piece.coordinates.x + direction.x, piece.coordinates.y + direction.y, out nextPiece))
            {
                if (piece.type == nextPiece.type)
                {
                    matches.Add (nextPiece);
                    SearchForMatches (nextPiece, direction, ref matches);
                }
            }
        }

        /// <summary>
        /// Returns the piece at the given position, otherwise null if out of bounds
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="piece"></param>
        /// <returns>True if there was a piece at the given position</returns>
        bool TryGetPiece (int x, int y, out Piece piece)
        {
            piece = null;
            if (x < 0 || y < 0 || x >= size.x || y >= size.y)
                return false;
            piece = pieces[x, y];
            return true;
        }

        Piece GetPiece (int x, int y)
        {
            if (x < 0 || y < 0 || x >= size.x || y >= size.y)
                return null;
            return pieces[x, y];
        }

        bool PieceAt (int x, int y)
        {
            if (x < 0 || y < 0 || x >= size.x || y >= size.y)
                return false;
            return true;
        }

        //events
        void OnMouseClickPiece (Vector2Int coor)
        {
            if (updatingBoard)
                return;

            if (clickedPiece == noPiece)
                clickedPiece = coor;
            else if (clickedPiece == coor)
                clickedPiece = noPiece;
            else
                StartCoroutine (MovePiece (coor));

            renderer.OnPieceClicked (coor);
        }

        void ResetMovement ()
        {
            clickedPiece = noPiece;
        }

        void OnMouseUpPiece (Vector2Int coor)
        {
            renderer.OnPieceUp (coor);
        }
        void OnMouseEnterPiece (Vector2Int coor)
        {
            renderer.OnPieceEnter (coor);
            hoverPiece = coor;
        }
        void OnMouseExitPiece (Vector2Int coor)
        {
            renderer.OnPieceExit (coor);
            hoverPiece = noPiece;
        }
    }
}