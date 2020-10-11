using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JellyMesh : MonoBehaviour
{
    public float Mass = 1f,Stiffness = 1f, Damping = 0.75f, Intensity = 1f;
    private Mesh OriginalMesh, MeshClone;
    private MeshRenderer renderer;
    private JellyVertex[] jv;
    private Vector3[] vertexArray;

    // Awake is called when the gameobject becomes active
    void Awake()
    {
        OriginalMesh = GetComponent<MeshFilter>().sharedMesh;
        MeshClone = Instantiate(OriginalMesh);
        GetComponent<MeshFilter>().sharedMesh = MeshClone;
        renderer = GetComponent<MeshRenderer>();
        jv = new JellyVertex[MeshClone.vertices.Length];
        for (int i = 0; i < MeshClone.vertices.Length; i++)
            jv[i] = new JellyVertex(i, transform.TransformPoint(MeshClone.vertices[i]));
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        vertexArray = OriginalMesh.vertices;
        for(int i = 0; i< jv.Length; i++)
        {
            Vector3 target = transform.TransformPoint(vertexArray[jv[i].ID]);
            float intensity = (1-(renderer.bounds.max.y - target.y) / renderer.bounds.size.y) * Intensity;
            jv[i].Shake(target, Mass, Stiffness, Damping);
            target = transform.InverseTransformPoint(jv[i].Postition);
            vertexArray[jv[i].ID] = Vector3.Lerp(vertexArray[jv[i].ID], target, intensity);
        }
        MeshClone.vertices = vertexArray;
    }

    public class JellyVertex
    {
        public int ID;
        public Vector3 Postition, Velocity, Force;

        public JellyVertex(int _id, Vector3 _pos)
        {
            ID = _id;
            Postition = _pos;
        }
        public void Shake(Vector3 target, float m, float s, float d)
        {
            Force = (target - Postition) * s;
            Velocity = (Velocity + Force / m) * d;
            Postition += Velocity;
            if((Velocity + Force + Force / m).magnitude < 0.001f)
            {
                Postition = target;
            }
        }

    }

}

