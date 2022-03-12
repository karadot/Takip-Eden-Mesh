using System;
using System.Collections;
using System.Collections.Generic;
using DefaultNamespace;
using UnityEngine;

public class Mover : MonoBehaviour
{
    private float _timer = 0;

    private List<Vector3> _corners = new List<Vector3>()
    {
        Vector3.down + Vector3.left, //sol alt
        Vector3.up + Vector3.left, //sol ust
        Vector3.up + Vector3.right, //sag ust
        Vector3.down + Vector3.right, //sag alt
    };

    [SerializeField] private MeshCreator _creator;

    private IEnumerator Start()
    {
        yield return null;
        yield return null;
        yield return null;
        _corners = _creator.CalculatedVertices;
        for (int i = 0; i < _corners.Count; i++)
        {
            var temp = _corners[i];
            temp.z = 0;
            _corners[i] = temp;
        }

        StartCoroutine(UpdatePositions());
    }

    IEnumerator UpdatePositions()
    {
        _timer = 0;
        var positions = new List<Vector3>();
        while (true)
        {
            positions.Clear();
            for (int i = 0; i < _corners.Count; i++)
            {
                positions.Add(transform.TransformPoint(_corners[i]));
            }

            if (_timer < .1f)
            {
                _creator.OnLastVerticesUpdate(positions);
            }
            else
            {
                _creator.OnNewPositionAdded(positions);
                _timer = 0;
            }

            _timer += Time.deltaTime;
            yield return null;
        }
    }

    private Vector3 currentMovement = Vector3.forward;

    private void Update()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");
        v = Mathf.Clamp(v, -.5f, .5f);
        h = Mathf.Clamp(h, -.5f, .5f);
        currentMovement = Vector3.Lerp(currentMovement, new Vector3(h, v, 1), Time.deltaTime);
        transform.position += currentMovement * Time.deltaTime * 10;
        transform.rotation = Quaternion.LookRotation(currentMovement);
    }
}