using System;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class MeshDataVo
    {
        public int Resolution;
        public float Size;
        public Vector3[][] Vertices;
        public int[] Triangles;
    }
}