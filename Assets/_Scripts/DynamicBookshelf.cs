using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class DynamicBookshelf : MonoBehaviour {
    [Range(0, 30)] public int numberOfRows = 5;
    [Range(0, 30)] public int numberOfColumns = 3;

    [Range(0.01f, 30)] public float shelfWidth = 8f;
    [Range(0.01f, 30)] public float shelfHeight = 1f;
    [Range(0.01f, 10)] public float shelfDepth = 0.5f;

    [Range(0.01f, 1f)] public float shelfWoodThickness = 0.1f; // Thickness of the wood used for shelves
    [Range(0.01f, 1f)] public float sideWoodThickness = 0.1f; // Thickness of the wood used for sides
    [Range(0.01f, 1f)] public float backWoodThickness = 0.1f; // Thickness of the wood used for the back
    [Range(0.01f, 1f)] public float columnWoodThickness = 0.1f; // Thickness of the wood used for vertical columns
    [Range(0.01f, 1f)] public float columnDepth = 1f; // Percentage of the shelf depth that the columns should occupy

    public bool hasBack = true;
    public bool hasSides = true;

    [Range(0f, 1f)] public float backPosition = 0f; // Position of the back from 0 (back) to 1 (front)
    [Range(0f, 1f)] public float columnPosition = 0f; // Position of the column from 0 (back) to 1 (front)

    private Mesh mesh;

    private List<Vector3> vertices;
    private List<int> triangles;
    private List<Vector2> uvs;

    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    public bool updateBookshelf; // Whether we need to update the bookshelf or not

    void OnValidate() {
        if (!Application.isPlaying) updateBookshelf = true;
    }

    void PrepareMesh() {
        if (mesh == null) mesh = new Mesh();
        if (meshCollider == null) meshCollider = GetComponent<MeshCollider>();
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
        
        mesh.Clear();
        
        meshCollider.sharedMesh = mesh;
        meshFilter.mesh = mesh;
    }

    void Awake() {
        GenerateBookshelf();
    }

    void GenerateBookshelf() {
        PrepareMesh();

        vertices = new List<Vector3>();
        triangles = new List<int>();
        uvs = new List<Vector2>();

        float currentHeight = 0;
        for (int row = 0; row < numberOfRows; row++) {
            AddShelf(currentHeight);
            currentHeight += shelfHeight + shelfWoodThickness;
        }

        for (int col = 0; col < numberOfColumns; col++) AddVerticalSeparator(col);

        if (hasBack) AddBack(currentHeight);

        if (hasSides) AddSides(currentHeight);

        AddTopCover(currentHeight);

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
    }

    // TODO: Needs to account for back position
    void AddTopCover(float totalHeight) {
        // to fix top to account for back position, we need to follow the depth of the back based on the back position. That means we also have to change the top position based on the back position and the back thickness
        Vector3 topBaseCorner = new Vector3((hasSides ? -sideWoodThickness : 0), totalHeight, 0);
        AddBox(topBaseCorner, shelfWidth - (hasSides ? -sideWoodThickness * 2 : 0), shelfWoodThickness, shelfDepth + (hasBack ? backWoodThickness : 0));
    }

    void AddVerticalSeparator(int column) {
        float columnSpacing = shelfWidth / (numberOfColumns + 1);

        for (int row = 0; row < numberOfRows; row++) {
            float yPos = row * shelfHeight + ((row + 1) * shelfWoodThickness);
            float xPos = (column + 1) * columnSpacing - columnWoodThickness / 2;
            float zPos = -shelfDepth + (columnPosition * (shelfDepth - shelfDepth * columnDepth)) + (shelfDepth * columnDepth);

            Vector3 baseCorner = new Vector3(xPos, yPos, zPos);
            AddBox(baseCorner, columnWoodThickness, shelfHeight, shelfDepth * columnDepth);
        }
    }

    void AddBack(float totalHeight) {
        float backZPos = -shelfDepth + (backPosition * (shelfDepth + backWoodThickness));
        Vector3 baseCorner = new Vector3((hasSides ? -sideWoodThickness : 0), 0, backZPos);
        AddBox(baseCorner, shelfWidth + (hasSides ? sideWoodThickness * 2 : 0), totalHeight, backWoodThickness);
    }

    void AddSides(float totalHeight) {
        // Left side
        Vector3 leftBaseCorner = new Vector3(shelfWidth, 0, 0);
        AddBox(leftBaseCorner, sideWoodThickness, totalHeight, shelfDepth);

        // Right side
        Vector3 rightBaseCorner = new Vector3(-sideWoodThickness, 0, 0);
        AddBox(rightBaseCorner, sideWoodThickness, totalHeight, shelfDepth);
    }

    void AddShelf(float yPos) {
        Vector3 baseCorner = new Vector3(0, yPos, 0);
        AddBox(baseCorner, shelfWidth, shelfWoodThickness, shelfDepth);
    }


    void AddBox(Vector3 baseCorner, float width, float height, float depth) {
        Vector3 bottomLeftFront = baseCorner;
        Vector3 bottomRightFront = baseCorner + new Vector3(width, 0, 0);
        Vector3 topLeftFront = baseCorner + new Vector3(0, height, 0);
        Vector3 topRightFront = baseCorner + new Vector3(width, height, 0);

        Vector3 bottomLeftBack = baseCorner + new Vector3(0, 0, -depth);
        Vector3 bottomRightBack = baseCorner + new Vector3(width, 0, -depth);
        Vector3 topLeftBack = baseCorner + new Vector3(0, height, -depth);
        Vector3 topRightBack = baseCorner + new Vector3(width, height, -depth);

        AddQuad(bottomLeftFront, bottomRightFront, topLeftFront, topRightFront); // Front face
        AddQuad(bottomRightBack, bottomLeftBack, topRightBack, topLeftBack); // Back face

        AddQuad(topLeftFront, topRightFront, topLeftBack, topRightBack); // Top face
        AddQuad(bottomLeftBack, bottomRightBack, bottomLeftFront, bottomRightFront); // Bottom face

        AddQuad(bottomRightFront, bottomRightBack, topRightFront, topRightBack); // Left face
        AddQuad(bottomLeftBack, bottomLeftFront, topLeftBack, topLeftFront); // Right face
    }

    void AddQuad(Vector3 bl, Vector3 br, Vector3 tl, Vector3 tr) {
        int startVertex = vertices.Count;
        vertices.Add(bl);
        vertices.Add(br);
        vertices.Add(tr);
        vertices.Add(tl);

        triangles.Add(startVertex);
        triangles.Add(startVertex + 1);
        triangles.Add(startVertex + 2);
        triangles.Add(startVertex);
        triangles.Add(startVertex + 2);
        triangles.Add(startVertex + 3);

        uvs.Add(new Vector2(0, 0));
        uvs.Add(new Vector2(1, 0));
        uvs.Add(new Vector2(1, 1));
        uvs.Add(new Vector2(0, 1));
    }
    
    public void ForceUpdate() {
        mesh = new Mesh();
        GenerateBookshelf();
    }
}