using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScaler : MonoBehaviour
{
    [Range (.1f, 5)]
    [SerializeField] float multiplier = 1;
    [SerializeField] float referenceSize = 5;
    [SerializeField] Camera camera;

    void Start ()
    {
        ScaleCameraDistance ();
    }

    private void OnValidate ()
    {
        if (camera == null)
            camera = GetComponent<Camera> ();
    }

    [ContextMenu ("Recalculate Scaling")]
    public void ScaleCameraDistance ()
    {
        var aspectRatio = camera.aspect * multiplier;
        camera.orthographicSize = Mathf.Max (referenceSize * aspectRatio, 1);
    }
}