using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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

        private List<Vector2> uv = new List<Vector2>();

        public List<Vector3> CalculatedVertices = new List<Vector3>();

        [SerializeField] private int cornerCount = 4;

        private int initialTris, initialVertexCount;
        private int newPositionCounter = 0;

        private List<int> forwardIndices = new List<int>(), backIndices = new List<int>();
        [SerializeField] private bool useDiscrete;

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
            if (useDiscrete)
                DiscreteVertexInit();
            else
                ContinuousVertexInit();
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = tris.ToArray();
            mesh.uv = uv.ToArray();
            _meshFilter.sharedMesh = mesh;
        }

        public void OnNewPositionAdded(List<Vector3> obj)
        {
            if (useDiscrete)
                DiscreteOnNewPositionAdded(obj);
            else
                ContinuousOnNewPositionAdded(obj);
            var tempMesh = _meshFilter.mesh;
            tempMesh.vertices = vertices.ToArray();
            tempMesh.triangles = tris.ToArray();
            tempMesh.uv = uv.ToArray();
            tempMesh.RecalculateNormals();
            tempMesh.RecalculateBounds();
            _meshFilter.sharedMesh = tempMesh;
            newPositionCounter++;
        }

        public void OnLastVerticesUpdate(List<Vector3> updatedPos)
        {
            _meshFilter.sharedMesh = useDiscrete
                ? DiscreteOnLastVerticesUpdate(updatedPos)
                : ContinuousOnLastVerticesUpdated(updatedPos);
        }

        #region Continouos

        void ContinuousVertexInit()
        {
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

            initialTris = tris.Count;
            string log = "";
            for (int i = 0; i <= tris.Count - 3; i += 3)
            {
                log += tris[i] + "," + tris[i + 1] + "," + tris[i + 2] + "\n";
            }
        }


        void ContinuousOnNewPositionAdded(List<Vector3> obj)
        {
            var startCount = vertices.Count;

            for (int i = obj.Count; i > 0; i--)
            {
                vertices.Add(vertices[startCount - i]);
            }

            foreach (var p in obj)
            {
                vertices.Add(p);
            }

            var currentTris = tris.Count;

            for (int i = currentTris - (6 * cornerCount); i < currentTris; i++)
            {
                tris.Add(tris[i] + cornerCount + cornerCount);
            }


            if (newPositionCounter >= 15)
            {
                for (int i = 0; i < obj.Count * 2; i++)
                {
                    vertices.RemoveAt(0);
                }

                for (int i = 0; i < cornerCount * 6; i++)
                {
                    tris.RemoveAt(0);
                }

                for (int i = 0; i < tris.Count; i++)
                {
                    tris[i] -= cornerCount * 2;
                }
            }
        }


        private List<Vector3> lastUpdatedVertices = new List<Vector3>();

        Mesh ContinuousOnLastVerticesUpdated(List<Vector3> updatedPos)
        {
            lastUpdatedVertices = new List<Vector3>(updatedPos);
            var tempMesh = _meshFilter.mesh;
            for (int i = 0; i < updatedPos.Count; i++)
            {
                vertices[^(updatedPos.Count - i)] = updatedPos[i];
            }

            tempMesh.vertices = vertices.ToArray();
            tempMesh.RecalculateBounds();
            tempMesh.RecalculateNormals();
            return tempMesh;
        }

        #endregion

        #region Discrete

        private int count;

        List<Vector3> newVertices = new List<Vector3>();
        List<int> tempTris = new List<int>();
        private List<Vector2> newUv = new List<Vector2>();

        void DiscreteVertexInit()
        {
            List<Vector3> tempStartVertices = new List<Vector3>();
            List<Vector2> tempUv = new List<Vector2>();
            for (int i = 0; i < CalculatedVertices.Count; i++)
            {
                tempStartVertices.Add(CalculatedVertices[i] + Vector3.back);
                tempUv.Add(new Vector2((float) i / (CalculatedVertices.Count - 1), 0));
            }

            for (int i = 0; i < CalculatedVertices.Count; i++)
            {
                tempStartVertices.Add(CalculatedVertices[i] + Vector3.forward);
                tempUv.Add(new Vector2((float) i / (CalculatedVertices.Count - 1), 1));
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

            CalculatedVertices = new List<Vector3>(vertices);
            for (int i = 0; i < tris.Count; i++)
            {
                var temp = tempStartVertices[tris[i]];
                if (temp.z == 1)
                {
                    temp.z = 0;
                    forwardIndices.Add(i);
                    CalculatedVertices.Add(temp);
                }
                else
                {
                    backIndices.Add(i);
                }

                uv.Add(tempUv[tris[i]]);
                vertices.Add(tempStartVertices[tris[i]]);
            }

            tris.Clear();
            for (int i = 0; i < vertices.Count; i++)
            {
                tris.Add(i);
            }

            initialTris = tris.Count;
            initialVertexCount = vertices.Count;
        }

        void DiscreteOnNewPositionAdded(List<Vector3> obj)
        {
            tempTris.Clear();
            newVertices = new List<Vector3>(obj.Count * 2);
            newUv = new List<Vector2>();
            for (int i = 0; i < obj.Count * 2; i++)
            {
                newVertices.Add(Vector3.zero);
            }

            for (int i = 0; i < backIndices.Count; i++)
            {
                newVertices[backIndices[i]] = lastUpdatedVertices[(i + 2) % backIndices.Count];
            }

            for (int i = 0; i < forwardIndices.Count; i++)
            {
                newVertices[forwardIndices[i]] = obj[i];
            }

            for (int i = 0; i < newVertices.Count; i++)
            {
                uv.Add(uv[i]);
                vertices.Add(newVertices[i]);
            }

            for (int i = 0; i < initialTris; i++)
            {
                tempTris.Add(tris[^(initialTris - i)] + initialTris);
            }

            for (int i = 0; i < tempTris.Count; i++)
            {
                tris.Add(tempTris[i]);
            }

            //For optimization
            if (count >= 15)
            {
                for (int i = 0; i < initialVertexCount; i++)
                {
                    vertices.RemoveAt(0);
                }

                for (int i = 0; i < initialTris; i++)
                {
                    tris.RemoveAt(tris.Count - 1);
                }

                for (int i = 0; i < initialVertexCount; i++)
                {
                    uv.RemoveAt(0);
                }
            }

            count++;
        }

        Mesh DiscreteOnLastVerticesUpdate(List<Vector3> updatedPos)
        {
            var tempMesh = _meshFilter.mesh;
            lastUpdatedVertices = new List<Vector3>(updatedPos);

            for (int i = 0; i < forwardIndices.Count; i++)
            {
                vertices[^(initialVertexCount - forwardIndices[i])] = updatedPos[i];
            }

            tempMesh.vertices = vertices.ToArray();
            tempMesh.RecalculateBounds();
            tempMesh.RecalculateNormals();

            return tempMesh;
        }

        #endregion
    }
}