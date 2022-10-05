using System.Collections.Generic;
using UnityEngine;

//this should be attached to the armature of playerModel
public class Ragdoll : MonoBehaviour
{
    GameObject root;
    Bone rootBone;

    [SerializeField]
    Bone[] bones;
    void Start(){
        root = transform.GetChild(0).gameObject;
        rootBone = root.GetComponent<Bone>();
        List<Bone> boneList = new List<Bone>(); 
        boneList.Add(rootBone);
        boneList.AddRange(rootBone.FindChildren());
        bones = boneList.ToArray();
        foreach (Bone bone in bones)
        {
            bone.transform.SetParent(transform);
        }
    }

    private void FixedUpdate()
    {
        foreach (Bone bone in bones)
        {
            bone.SetNetForce(new Vector3(Random.Range(-10,10), Random.Range(-10,10), Random.Range(-10,10)) * 0.01f   );
            bone.UpdateVelocity();
            bone.UpdatePosition();
            bone.Constrain();
        }
    }
}
