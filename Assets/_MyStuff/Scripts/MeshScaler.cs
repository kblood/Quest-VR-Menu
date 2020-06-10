using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityGLTF;

public class MeshScaler : MonoBehaviour
{
    float timeToTest;
    GLTFComponent gltf;
    // Start is called before the first frame update
    void Start()
    {
        timeToTest = 15;
    }

    private void Awake()
    {
        gltf = GetComponent<GLTFComponent>();
    }

    // Update is called once per frame
    void Update()
    {
        //if (Time.time > timeToTest)
        //{
        //    timeToTest = Time.time + 15;
        //    print("Checking mesh size");
        //    ScaleMesh();
        //    SnapMeshToFloor();

        //}
    }

    public void SnapMeshToFloor()
    {
        var meshes = this.transform.GetComponentsInChildren<MeshFilter>();

        if (!meshes.Any())
        {
            Debug.Log("No gltf meshes found.");
            return;

        }
        // Recalculate min Y value now that the mesh has been rescaled
        float minY = meshes.SelectMany(m => m.mesh.vertices).Min(v => transform.TransformPoint(v).y);
        print("Lowest y: " + minY);
        // Snap the mesh to the floor
        transform.position = new Vector3(transform.position.x, transform.position.y - minY, transform.position.z);
    }

    public void ScaleMesh()
    {
        var meshes = this.transform.GetComponentsInChildren<MeshFilter>();

        Vector3 size = Vector3.zero;

        foreach(var mesh in meshes)
        {
            //if(mesh.mesh.bounds.size.x)
            //print(mesh.name + " is size: " + mesh.mesh.bounds.size);

            //Vector3 scale = Vector3.one;
            //Utils.GetAccumulatedLocalScale(transform, mesh.transform, ref scale);
            
            //if(scale != mesh.transform.lossyScale)
            //{
            //    print("Calculated scale: " + scale + " lossy scale: " + mesh.transform.lossyScale);
            //}

            size += Vector3.Scale(mesh.transform.lossyScale, mesh.mesh.bounds.size);
        }
        //print("Full mesh size is: " + size.magnitude);
        if (size.magnitude > 10 || size.magnitude < 1)
        {
            print("Scaling mesh with magnitude " + size.magnitude);
            transform.localScale = (transform.localScale / size.magnitude) * 10;
        }

        
    }
}
