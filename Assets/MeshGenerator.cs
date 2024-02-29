using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Require a MeshFilter component to be attached to the GameObject this script is attached to.
[RequireComponent(typeof(MeshFilter))]
public class MeshGenerator : MonoBehaviour
    {
        private Mesh _mesh; // Mesh variable to hold the generated mesh.

        private Vector3[] _vertices; // Array to store the vertices of the mesh.
        private int[] _triangles; // Array to store the triangles of the mesh.
        private Color[] _colors; // Array to store the colors for each vertex.
        public int xSize = 20; // Public variable to set the size of the mesh along the x-axis.
        public int zSize = 20; // Public variable to set the size of the mesh along the z-axis.
        public Gradient gradient; // Public variable to set the gradient used for coloring the mesh based on height.

        private float _minTerrainHeight; // Tracks the minimum height of the terrain.
        private float _maxTerrainHeight; // Tracks the maximum height of the terrain.

        // Start is called before the first frame update
        void Start()
            {
                _mesh = new Mesh(); // Initializes the mesh.
                GetComponent<MeshFilter>().mesh = _mesh; // Assigns the created mesh to the MeshFilter component.

                CreateShape(); // Calls the function to create the shape of the mesh.
                UpdateMesh(); // Calls the function to update the mesh with the created shape.
            }


        void CreateShape()
            {
                // Initialize vertices array based on the size of the mesh to be generated.
                _vertices = new Vector3[(xSize + 1) * (zSize + 1)];

                // Generate vertices with Perlin noise for a more natural look.
                for (int i = 0, z = 0; z <= zSize; z++)
                    {
                        for (int x = 0; x <= xSize; x++)
                            {
                                float y = Mathf.PerlinNoise(x * 0.3f, z * 0.3f) * 2f;
                                _vertices[i] = new Vector3(x, y, z);

                                // Update the min and max terrain height for gradient calculation later.
                                if (y > _maxTerrainHeight)
                                    _maxTerrainHeight = y;

                                if (y < _minTerrainHeight)
                                    _minTerrainHeight = y;
                                i++;
                            }
                    }

                // Initialize triangles. Each square on the grid consists of 2 triangles, hence 6 vertices.
                _triangles = new int[xSize * zSize * 6];

                // Loop through grid squares to set up triangles.
                int vert = 0;
                int tris = 0;
                for (int z = 0; z < zSize; z++)
                    {
                        for (int x = 0; x < xSize; x++)
                            {
                                _triangles[tris + 0] = vert + 0;
                                _triangles[tris + 1] = vert + xSize + 1;
                                _triangles[tris + 2] = vert + 1;
                                _triangles[tris + 3] = vert + 1;
                                _triangles[tris + 4] = vert + xSize + 1;
                                _triangles[tris + 5] = vert + xSize + 2;

                                vert++;
                                tris += 6;
                            }

                        vert++;
                    }

                // Assign a color based on vertex height using the gradient.
                _colors = new Color[_vertices.Length];
                for (int i = 0, z = 0; z <= zSize; z++)
                    {
                        for (int x = 0; x <= xSize; x++)
                            {
                                float height = Mathf.InverseLerp(_minTerrainHeight, _maxTerrainHeight, _vertices[i].y);
                                _colors[i] = gradient.Evaluate(height);
                                i++;
                            }
                    }
            }

        private void UpdateMesh()
            {
                _mesh.Clear(); // Clears the current mesh.

                // Assigns the vertices, triangles, and colors to the mesh.
                _mesh.vertices = _vertices;
                _mesh.triangles = _triangles;
                _mesh.colors = _colors;

                _mesh.RecalculateNormals(); // Recalculates the normals of the mesh for proper lighting.
            }

        // Optional debugging feature to visualize vertices in the editor.
        // private void OnDrawGizmos()
        // {
        //     if (vertices == null) 
        //         return;
        //     
        //     for (int i = 0; i < vertices.Length; i++)
        //     {
        //         Gizmos.DrawSphere(vertices[i], 0.1f);
        //     }
        // }
    }