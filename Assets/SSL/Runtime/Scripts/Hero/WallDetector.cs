using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallDetector : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private Transform[] _detectionPoints;
    [SerializeField] private float _detectionLength = 0.1f;
    [SerializeField] private LayerMask _wallLayerMask;

    public bool DetectWallNearBy()
    {
        foreach (Transform detectionPoint in _detectionPoints)
        {
            RaycastHit2D hitResultRight = Physics2D.Raycast(detectionPoint.position, Vector2.right, _detectionLength, _wallLayerMask);

            if (hitResultRight.collider != null)
            {
                return true;
            }

            RaycastHit2D hitResultLeft = Physics2D.Raycast(detectionPoint.position, Vector2.left, _detectionLength, _wallLayerMask);

            if (hitResultLeft.collider != null)
            {
                return true;
            }
        }
        return false;
    }
}
