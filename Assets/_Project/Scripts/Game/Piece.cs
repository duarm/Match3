using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour
{
    public int type;
    public Vector2Int coordinates;
    public event Action<Vector2Int> MouseClicked;
    public event Action<Vector2Int> MouseEnter;
    public event Action<Vector2Int> MouseExit;
    public event Action<Vector2Int> MouseUp;

    
    private void OnMouseDown ()
    {
        MouseClicked.Invoke (coordinates);
    }

    private void OnMouseUp ()
    {
        MouseUp.Invoke (coordinates);
    }

    private void OnMouseEnter ()
    {
        MouseEnter.Invoke (coordinates);
    }

    private void OnMouseExit ()
    {
        MouseExit.Invoke (coordinates);
    }
}