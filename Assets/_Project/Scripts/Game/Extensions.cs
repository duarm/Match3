namespace Match3
{
    using System;
    using UnityEngine;
    public static class Extensions
    {
        public static bool AdjacentOf (this Vector2Int a, Vector2Int b)
        {
            return (Math.Abs (a.x - b.x) + Math.Abs (a.y - b.y)) == 1;
        }
    }
}