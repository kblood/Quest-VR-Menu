using OculusSampleFramework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AddGrabbable : MonoBehaviour
{
    public void MakeGrabbable()
    {
        //var meshes = this.transform.GetComponentsInChildren<MeshFilter>();
        gameObject.layer = 8;

        AdvancedMerge();

        if (!TryGetComponent<MeshCollider>(out var collider))
            collider = gameObject.AddComponent<MeshCollider>();

        collider.sharedMesh = GetComponent<MeshFilter>().sharedMesh;

        int polygons = collider.sharedMesh.triangles.Length / 3;
        print("polygons " + polygons);
        if (polygons > 250)
        {
            float quality = 250f / polygons;

            print("Simplify Mesh " + quality);
            //SimplifyMesh(quality);
            SimplifyMesh(0.5f);
        }

        var child = transform.GetChild(0);
        //var children = transform.GetComponentsInChildren<Transform>().ToList();
        //children.Remove(transform);
        //transform.DetachChildren();
        //foreach (var child in children)
        //    Destroy(child.gameObject);
        transform.DetachChildren();
        child.parent = null;
        CenterMesh();
        child.gameObject.SetActive(false);
        //child.parent = transform;
        collider.sharedMesh = GetComponent<MeshFilter>().sharedMesh;
        polygons = collider.sharedMesh.triangles.Length / 3;
        print("polygons " + polygons);

        collider.convex = true;

        if (!TryGetComponent<Rigidbody>(out var rigidbody))
            rigidbody = gameObject.AddComponent<Rigidbody>();

        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        
        var grab = this.gameObject.AddComponent<DistanceGrabbable>();

        grab.m_materialColorField = "_OutlineColor";

        //this.gameObject.AddComponent<JellyMesh>();
    }

    public void AdvancedMerge()
    {
        // All our children (and us)
        MeshFilter[] filters = GetComponentsInChildren<MeshFilter>(false);

        // All the meshes in our children (just a big list)
        var materials = new List<Material>();
        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>(false); // <-- you can optimize this
        foreach (MeshRenderer renderer in renderers)
        {
            if (renderer.transform == transform)
                continue;
            Material[] localMats = renderer.sharedMaterials;
            foreach (Material localMat in localMats)
                if (!materials.Contains(localMat))
                    materials.Add(localMat);
        }

        // Each material will have a mesh for it.
        var submeshes = new List<Mesh>();
        foreach (Material material in materials)
        {
            // Make a combiner for each (sub)mesh that is mapped to the right material.
            var combiners = new List<CombineInstance>();
            foreach (MeshFilter filter in filters)
            {
                if (filter.transform == transform) continue;
                // The filter doesn't know what materials are involved, get the renderer.
                MeshRenderer renderer = filter.GetComponent<MeshRenderer>();  // <-- (Easy optimization is possible here, give it a try!)
                if (renderer == null)
                {
                    Debug.LogError(filter.name + " has no MeshRenderer");
                    continue;
                }

                // Let's see if their materials are the one we want right now.
                Material[] localMaterials = renderer.sharedMaterials;
                for (int materialIndex = 0; materialIndex < localMaterials.Length; materialIndex++)
                {
                    if (localMaterials[materialIndex] != material)
                        continue;
                    // This submesh is the material we're looking for right now.
                    CombineInstance ci = new CombineInstance();
                    ci.mesh = filter.sharedMesh;
                    ci.subMeshIndex = materialIndex;
                    ci.transform = filter.transform.localToWorldMatrix;
                    //ci.transform = Matrix4x4.identity;
                    combiners.Add(ci);
                }
            }
            // Flatten into a single mesh.
            Mesh mesh = new Mesh();
            mesh.CombineMeshes(combiners.ToArray(), true);
            submeshes.Add(mesh);
        }

        // The final mesh: combine all the material-specific meshes as independent submeshes.
        var finalCombiners = new List<CombineInstance>();
        foreach (Mesh mesh in submeshes)
        {
            CombineInstance ci = new CombineInstance();
            ci.mesh = mesh;
            ci.subMeshIndex = 0;
            ci.transform = Matrix4x4.identity;
            finalCombiners.Add(ci);
        }
        Mesh finalMesh = new Mesh();
        finalMesh.CombineMeshes(finalCombiners.ToArray(), false);
        if (!TryGetComponent<MeshRenderer>(out var myMeshRenderer))
            myMeshRenderer = gameObject.AddComponent<MeshRenderer>();

        if (!TryGetComponent<MeshFilter>(out var myMeshFilter))
            myMeshFilter = gameObject.AddComponent<MeshFilter>();
        myMeshFilter.sharedMesh = finalMesh;
        myMeshRenderer.materials = materials.ToArray();

        Debug.Log("Final mesh has " + submeshes.Count + " materials.");
    }

    public void CombineMeshesAndMaterialsFromChildren()
    {
        var meshes = GetComponentsInChildren<MeshFilter>().ToList();
        if (TryGetComponent<MeshFilter>(out var meshFilter))
        {
            //meshes.Add(meshFilter);
        }
        else
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        Vector3[] newMesh = new Vector3[0];

        if (meshes.Any())
        {
            //meshes.SelectMany(m => m.mesh.vertices).Select(v => transform.TransformPoint(v));
            newMesh = meshes.SelectMany
            (
                m => 
                    {
                        var meshTransform = m.transform;
                        return m.mesh.vertices.Select(v => meshTransform.TransformPoint(v)).ToArray();
                    }
            ).ToArray();
        }

        for(int i= 0; i < meshFilters.Length; i++)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
        }

        meshFilter.mesh = new Mesh();
        meshFilter.mesh.CombineMeshes(combine);
        meshFilter.mesh.vertices = newMesh;

        var meshRenderers = this.transform.GetComponentsInChildren<MeshRenderer>().ToList();
        if (TryGetComponent<MeshRenderer>(out var meshRenderer))
        {
            //meshRenderers.Add(meshRenderer);
        }
        else
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        Material[] materials = meshRenderers.SelectMany(m => m.sharedMaterials).ToArray();
        print("Found " + materials.Length + "Materials");
        meshRenderer.materials = materials;
        foreach(var c in GetComponentsInChildren<MeshFilter>())
        {
            c.gameObject.SetActive(false);
        }
        transform.gameObject.SetActive(true);
    }

    public void CenterMesh()
    {
        var meshFilter = GetComponent<MeshFilter>();
        var theMesh = meshFilter.sharedMesh;
        var originalVerts = theMesh.vertices;
        var centeredVerts = new Vector3[originalVerts.Length];
        Vector3 center = theMesh.bounds.center;
        Vector3 offset = center - meshFilter.transform.position;

        //Quaternion qAngle = Quaternion.AngleAxis(rotatedAngle, axis);
        for (int vert = 0; vert < originalVerts.Length; vert++)
        {
            centeredVerts[vert] = originalVerts[vert] - offset;
        }

        theMesh.vertices = centeredVerts;
    }

    public void RotateMesh(MeshFilter meshFilter, Vector3 axis, float rotatedAngle = 90)
    {
        var theMesh = meshFilter.sharedMesh;
        var originalVerts = theMesh.vertices;
        var rotatedVerts = new Vector3[originalVerts.Length];

        Quaternion qAngle = Quaternion.AngleAxis(rotatedAngle, axis);
        for (int vert  = 0; vert < originalVerts.Length; vert++)
        {
            rotatedVerts[vert] = qAngle * originalVerts[vert];
        }

        theMesh.vertices = rotatedVerts;
    }

    public void SimplifyMesh(float quality = 0.5f)
    {
        var meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
            meshFilter = GetComponentInChildren<MeshFilter>();
        var sourceMesh = meshFilter.sharedMesh;
        var meshSimplifier = new UnityMeshSimplifier.MeshSimplifier();
        meshSimplifier.Initialize(sourceMesh);
        meshSimplifier.SimplifyMesh(quality);
        var destMesh = meshSimplifier.ToMesh();
        meshFilter.sharedMesh = destMesh;
    }

    public void CombineMeshes()
    {
        print("Combining meshes");

        var filter = gameObject.AddComponent<MeshFilter>();
        var renderer = gameObject.AddComponent<MeshRenderer>();

        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            combine[i].mesh = meshFilters[i].sharedMesh;
            combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);

            i++;
        }
        filter.mesh = new Mesh();
        filter.mesh.CombineMeshes(combine);

        var meshRenderers = this.transform.GetComponentsInChildren<MeshRenderer>();
        Material[] materials = meshRenderers.SelectMany(m => m.sharedMaterials).ToArray();

        print("Found " + materials.Length + "Materials");

        renderer.materials = materials;

        materials = meshRenderers.Select(m => m.material).ToArray();
        renderer.materials = materials;

        transform.gameObject.SetActive(true);
    }
}
