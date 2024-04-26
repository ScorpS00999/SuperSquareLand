﻿using UnityEngine;

public class CameraProfile : MonoBehaviour
{
    [Header("Type")]
    [SerializeField] private CameraProfileType _profileType = CameraProfileType.Static;

    [Header("Follow")]
    [SerializeField] private CameraFollowable _targetToFollow = null;



    public CameraProfileType ProfileType => _profileType;

    public CameraFollowable TargetToFollow => _targetToFollow;

    private Camera _camera;

    public float CameraSize => _camera.orthographicSize;

    public Vector3 Position => _camera.transform.position;

    public enum CameraProfileType
    {
        Static = 0,
        FollowTarget
    }

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        if ( _camera != null)
        {
            _camera.enabled = false;
        }
    }
}

