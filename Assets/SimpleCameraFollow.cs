using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;

    private Vector3 initPos;

    private void Awake()
    {
        initPos = transform.position;
    }

    private void LateUpdate()
    {
        initPos.z = target.position.z;
        transform.position = initPos;
    }
}