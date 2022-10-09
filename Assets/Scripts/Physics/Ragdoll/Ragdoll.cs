using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//this should be attached to the armature of playerModel
public class Ragdoll : MonoBehaviour
{
    GameObject root;
    Bone rootBone;

    [SerializeField]
    Vector3 movement;

    [SerializeField]
    Bone[] bones;
    void Start(){
        movement = Vector3.zero;
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
            // bone.SetNetForce(new Vector3(Random.Range(-10,10), Random.Range(-10,10), Random.Range(-10,10)) * 0.01f);
            // bone.SetNetForce(PhysicsManager.GRAVITY * 0.01f);
            bone.SetNetForce(Vector3.zero);
            // bone.AddVelocity(PhysicsManager.GRAVITY * 0.01f);
            // bone.UpdateVelocity();
            // bone.UpdatePosition();
            // bone.Constrain();
        }
        bones[11].AddForce(movement * 2000);
        foreach (Bone bone in bones)
        {
            // bone.SetNetForce(new Vector3(Random.Range(-10,10), Random.Range(-10,10), Random.Range(-10,10)) * 0.01f);
            // bone.SetNetForce(PhysicsManager.GRAVITY * 0.01f);
            // bone.SetNetForce(Vector3.zero);
            // bone.AddVelocity(PhysicsManager.GRAVITY * 0.01f);
            bone.Constrain();
            bone.UpdateVelocity();
            bone.UpdatePosition();
        }
    }
    public void OnMove(InputValue value){
        Vector2 movement2 = value.Get<Vector2>();
        movement.x = movement2.x;
        movement.z = movement2.y;
    }
}
