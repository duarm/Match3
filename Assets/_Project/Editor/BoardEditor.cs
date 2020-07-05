using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Match3
{
    [CustomEditor (typeof (Board))]
    public class BoardEditor : Editor
    {
        Board board;
        private void OnEnable ()
        {
            board = (Board) target;
        }

        public override void OnInspectorGUI ()
        {
            base.OnInspectorGUI ();

        }

        private void OnSceneGUI ()
        {

        }
    }
}