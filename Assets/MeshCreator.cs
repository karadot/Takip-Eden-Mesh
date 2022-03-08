using System;
using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace
{
    public class MeshCreator : MonoBehaviour
    {
        [SerializeField] MeshFilter _meshFilter;

        [SerializeField] private Mover _mover;

        private List<Vector3> vertices = new List<Vector3>()
        {
            Vector3.down + Vector3.left + Vector3.back, //sol alt geri 0
            Vector3.up + Vector3.left + Vector3.back, //sol ust geri 1
            Vector3.up + Vector3.right + Vector3.back, //sag ust geri 2
            Vector3.down + Vector3.right + Vector3.back, //sag alt geri 3 
            Vector3.down + Vector3.left + Vector3.forward, //sol alt ileri 4
            Vector3.up + Vector3.left + Vector3.forward, //sol ust ileri  5
            Vector3.up + Vector3.right + Vector3.forward, //sag ust ileri  6
            Vector3.down + Vector3.right + Vector3.forward, //sag alt iler 7
        };

        private List<int> tris = new List<int>()
        {
            0, 4, 5,
            0, 5, 1,
            1, 5, 6,
            1, 6, 2,
            2, 6, 7,
            2, 7, 3,
            3, 7, 4,
            3, 4, 0
        };

        public List<Vector3> CalculatedVertices = new List<Vector3>();

        [SerializeField] private int cornerCount = 4;

        private void Start()
        {
            //TODO:Polygon Base
            //Angle Creates Points
            float angle = 360f / cornerCount;
            angle *= Mathf.Deg2Rad;
            for (int j = 0; j < cornerCount; j++)
            {
                CalculatedVertices.Add(new Vector3(Mathf.Sin((angle * j) + angle * .5f),
                    Mathf.Cos((angle * j) + angle * .5f), 0));
            }

            vertices.Clear();
            for (int i = 0; i < CalculatedVertices.Count; i++)
            {
                vertices.Add(CalculatedVertices[i] + Vector3.back);
            }

            for (int i = 0; i < CalculatedVertices.Count; i++)
            {
                vertices.Add(CalculatedVertices[i] + Vector3.forward);
            }

            tris.Clear();
            for (int i = 0; i < cornerCount - 1; i++)
            {
                tris.Add(i);
                tris.Add(i + cornerCount);
                tris.Add(i + cornerCount + 1);
                tris.Add(i);
                tris.Add(i + cornerCount + 1);
                tris.Add(i + 1);
            }

            tris.Add(cornerCount - 1);
            tris.Add(cornerCount - 1 + cornerCount);
            tris.Add(cornerCount);
            tris.Add(cornerCount - 1);
            tris.Add(cornerCount);
            tris.Add(0);

            string log = "";
            for (int i = 0; i <= tris.Count - 3; i += 3)
            {
                log += tris[i] + "," + tris[i + 1] + "," + tris[i + 2] + "\n";
            }

            Debug.Log(log);
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = tris.ToArray();
            _meshFilter.sharedMesh = mesh;
        }


        private int newPositionCounter = 0;

        public void OnNewPositionAdded(List<Vector3> obj)
        {
            //TODO:optimization 
            /*
            if (newPositionCounter >= 15)
            {
                for (int i = 0; i < obj.Count; i++)
                {
                    vertices.RemoveAt(0);
                }

                for (int i = 0; i < cornerCount * 6; i++)
                {
                    tris.RemoveAt(0);
                }
            }
*/
            foreach (var p in obj)
            {
                vertices.Add(p);
            }

            var currentTris = tris.Count;
            for (int i = tris.Count - (6 * cornerCount); i < currentTris; i++)
            {
                tris.Add(tris[i] + cornerCount);
            }


            Debug.Log(tris.Count);
            var tempMesh = _meshFilter.mesh;
            tempMesh.vertices = vertices.ToArray();
            tempMesh.triangles = tris.ToArray();
            _meshFilter.sharedMesh = tempMesh;
            newPositionCounter++;
        }

        public void OnLastVerticesUpdate(List<Vector3> updatedPos)
        {
            var tempMesh = _meshFilter.mesh;
            for (int i = 0; i < updatedPos.Count; i++)
            {
                vertices[^(updatedPos.Count - i)] = updatedPos[i];
            }

            tempMesh.vertices = vertices.ToArray();
            _meshFilter.sharedMesh = tempMesh;
        }
    }
}