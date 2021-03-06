﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityGLTF;

public static class Utils
{

    public static void DuplicateAndCombine3DObject(GameObject obj, int amount)
    {
        //[Range(0, 100)]
        //public int amount;

        GameObject newObj = new GameObject();

        Mesh mainMesh = obj.GetComponent<MeshFilter>().sharedMesh;

        Mesh meshToCopy = new Mesh();
        meshToCopy.vertices = mainMesh.vertices;
        meshToCopy.triangles = mainMesh.triangles;
        meshToCopy.normals = mainMesh.normals;
        meshToCopy.uv = mainMesh.uv;

        Matrix4x4[] matrix = new Matrix4x4[amount];
        for (int i = 0; i < amount; i++)
        {
            matrix[i].SetTRS(new Vector3(i * 10, 0, 0), Quaternion.Euler(new Vector3(Random.Range(-5, 5), Random.Range(0, 360), Random.Range(-5, 5))), Vector3.one);
        }

        CombineInstance[] ci = new CombineInstance[amount];
        for (int i = 0; i < amount; i++)
        {
            ci[i] = new CombineInstance();
            ci[i].mesh = meshToCopy;
            ci[i].transform = matrix[i];
        }

        Material[] materials = obj.GetComponent<MeshRenderer>().sharedMaterials;

        Mesh batchedMesh = newObj.AddComponent<MeshFilter>().mesh = new Mesh();
        batchedMesh.CombineMeshes(ci);
        newObj.AddComponent<MeshRenderer>().materials = materials;

        int[] mainSubTri;
        int[] newSubTri;
        batchedMesh.subMeshCount = mainMesh.subMeshCount;

        for (int i = 0; i < mainMesh.subMeshCount; i++)
        {
            mainSubTri = mainMesh.GetTriangles(i);

            newSubTri = new int[mainSubTri.Length * amount];

            for (int ii = 0; ii < amount; ii++)
            {
                for (int iii = 0; iii < mainSubTri.Length; iii++)
                {
                    newSubTri[(ii * mainSubTri.Length) + iii] = mainSubTri[iii] + (ii * mainMesh.vertexCount);
                }
            }
            batchedMesh.SetTriangles(newSubTri, i);
        }
    }

    // Gets an axis aligned bound box around an array of game objects
    public static Bounds GetBounds(GameObject[] objs)
    {
        if (objs == null || objs.Length == 0)
        {
            return new Bounds(Vector3.zero, Vector3.zero);
        }

        float minX = Mathf.Infinity;
        float maxX = -Mathf.Infinity;
        float minY = Mathf.Infinity;
        float maxY = -Mathf.Infinity;
        float minZ = Mathf.Infinity;
        float maxZ = -Mathf.Infinity;

        Vector3[] points = new Vector3[8];

        foreach (GameObject go in objs)
        {
            GetBoundsPointsNoAlloc(go, points);
            foreach (Vector3 v in points)
            {
                if (v.x < minX) minX = v.x;
                if (v.x > maxX) maxX = v.x;
                if (v.y < minY) minY = v.y;
                if (v.y > maxY) maxY = v.y;
                if (v.z < minZ) minZ = v.z;
                if (v.z > maxZ) maxZ = v.z;
            }
        }

        float sizeX = maxX - minX;
        float sizeY = maxY - minY;
        float sizeZ = maxZ - minZ;

        Vector3 center = new Vector3(minX + sizeX / 2.0f, minY + sizeY / 2.0f, minZ + sizeZ / 2.0f);

        return new Bounds(center, new Vector3(sizeX, sizeY, sizeZ));
    }

    // Gets an axis aligned bound box around an array of game objects
    public static void GetChildren(GameObject parent, ref List<GameObject> list)
    {
        foreach(Transform child in parent.transform)
        {
            list.Add(child.gameObject);
            GetChildren(child.gameObject, ref list);
        }
    }

    public static void GetAccumulatedLocalScale(Transform parent, Transform child, ref Vector3 scale)
    {
        scale = Vector3.Scale(child.localScale, scale);
        if (parent != child)
            GetAccumulatedLocalScale(parent, child.parent, ref scale);
    }

    // Pass in a game object and a Vector3[8], and the corners of the mesh.bounds in 
    //   in world space are returned in the passed array;
    public static void GetBoundsPointsNoAlloc(GameObject go, Vector3[] points)
    {
        if (points == null || points.Length < 8)
        {
            Debug.Log("Bad Array");
            return;
        }
        MeshFilter mf = go.GetComponent<MeshFilter>();
        if (mf == null)
        {
            //Debug.Log("No MeshFilter on object");
            for (int i = 0; i < points.Length; i++)
                points[i] = go.transform.position;
            return;
        }

        Transform tr = go.transform;

        Vector3 v3Center = mf.mesh.bounds.center;
        Vector3 v3ext = mf.mesh.bounds.extents;

        points[0] = tr.TransformPoint(new Vector3(v3Center.x - v3ext.x, v3Center.y + v3ext.y, v3Center.z - v3ext.z));  // Front top left corner
        points[1] = tr.TransformPoint(new Vector3(v3Center.x + v3ext.x, v3Center.y + v3ext.y, v3Center.z - v3ext.z));  // Front top right corner
        points[2] = tr.TransformPoint(new Vector3(v3Center.x - v3ext.x, v3Center.y - v3ext.y, v3Center.z - v3ext.z));  // Front bottom left corner
        points[3] = tr.TransformPoint(new Vector3(v3Center.x + v3ext.x, v3Center.y - v3ext.y, v3Center.z - v3ext.z));  // Front bottom right corner
        points[4] = tr.TransformPoint(new Vector3(v3Center.x - v3ext.x, v3Center.y + v3ext.y, v3Center.z + v3ext.z));  // Back top left corner
        points[5] = tr.TransformPoint(new Vector3(v3Center.x + v3ext.x, v3Center.y + v3ext.y, v3Center.z + v3ext.z));  // Back top right corner
        points[6] = tr.TransformPoint(new Vector3(v3Center.x - v3ext.x, v3Center.y - v3ext.y, v3Center.z + v3ext.z));  // Back bottom left corner
        points[7] = tr.TransformPoint(new Vector3(v3Center.x + v3ext.x, v3Center.y - v3ext.y, v3Center.z + v3ext.z));  // Back bottom right corner
    }
}
