using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

//this should be attached to the armature of playerModel
public class Ragdoll : MonoBehaviour
{
    Bone rootBone;

    [SerializeField]
    Vector3 movement;

    [Range(0.0f, 100.0f)]
    float accuracy = 100.0f;

    [SerializeField]
    Bone[] bones;
    void Start(){
        accuracy /= 100;
        movement = Vector3.zero;
        rootBone = transform.GetChild(0).gameObject.GetComponent<Bone>();
        rootBone.MakeTreeStructure(null);
        rootBone.SetParentToAll(transform);
    }

    private void FixedUpdate()
    {
        rootBone.SetNetForce(movement * 1000);
        rootBone.UpdateAll();
    }
    public void OnMove(InputValue value){
        movement = value.Get<Vector3>();
    }
}
