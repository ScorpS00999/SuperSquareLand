﻿using UnityEngine;

public class CameraChangeProfileTrigger : MonoBehaviour
{
    private const string TARGET_TAG = "CameraTriggerTarget";

    [Header("Camera Profile")]
    [SerializeField] private CameraProfile _cameraProfile;
    
    [Header("Trigger Box")]
    [SerializeField] private BoxCollider2D _boxCollider;

    private void OnDrawGizmos()
    {
        if (_boxCollider == null) return;
        Color gizmosColor = Color.green;
        gizmosColor.a = 0.05f;
        Gizmos.color = gizmosColor;
        Vector3 boxCenter = _boxCollider.transform.position + (Vector3)_boxCollider.offset;
        Vector3 boxSize = _boxCollider.size;
        Gizmos.DrawCube(boxCenter, boxSize);
        Gizmos.color = Color.white;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(TARGET_TAG))
        {
            return;
        }
        CameraManager.Instance.EnterProfile(_cameraProfile);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag(TARGET_TAG))
        {
            return;
        }
        CameraManager.Instance.ExitProfile(_cameraProfile);
    }
}