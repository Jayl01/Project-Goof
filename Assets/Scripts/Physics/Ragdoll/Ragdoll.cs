using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.Rendering.DebugUI;

//this should be attached to the armature of playerModel
public class Ragdoll : MonoBehaviour
{
    Bone rootBone;

    [SerializeField]
    Vector3 movement;
    Vector3 prevMovement;

    public byte controllerID;

    [SerializeField]
    int accuracy = 5;

    [SerializeField]
    float speed = 1, maxSpeed = 100;

    void Start(){
        movement = Vector3.zero;
        rootBone = transform.GetChild(0).gameObject.GetComponent<Bone>();
        rootBone.MakeTreeStructure(null);
        rootBone.SetParentToAll(transform);
    }

    private void FixedUpdate()
    {
        rootBone.AddVelocity(movement * speed);
        rootBone.UpdateAll(accuracy, maxSpeed);
    }

    public void SetMovement(Vector3 newMove)
    {
        movement = newMove;
    }

    public void OnMove(InputValue value){
        // if (controllerID == LobbyManager.self.clientID)
        // {
            movement = value.Get<Vector3>();
            if (prevMovement != movement)
            {
                prevMovement = movement;
                SyncCall.SyncMovement(movement);
            }
        // }
    }
}
