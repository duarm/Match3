using System.Collections;
using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Match3
{
    public class BoardUI : MonoBehaviour
    {
        [SerializeField] GameObject menu;
        [SerializeField] TextMeshProUGUI titleText;
        [HideInInspector]
        [SerializeField] Board board;

        private void OnValidate ()
        {
            if (board == null)
                board = FindObjectOfType<Board> ();
        }

        private void Start ()
        {
            EventManager.StartListening (Board.GameOver, GameOver);
        }

        public void GameOver ()
        {
            menu.SetActive (true);
        }

        public void StartGame ()
        {
            StartCoroutine (StartGameRoutine (false));
        }

        public void StartRushGame ()
        {
            StartCoroutine (StartGameRoutine (true));
        }

        IEnumerator StartGameRoutine (bool rush)
        {
            menu.SetActive (false);
            board.StartGame (rush);
            yield return null;
        }

        public void Quit ()
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit ();
#endif
        }
    }
}