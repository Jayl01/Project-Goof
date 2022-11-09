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
        movement *= 0.92f;
        rootBone.AddVelocity(movement * speed);
        rootBone.UpdateAll(accuracy, maxSpeed);
    }

    public void SetMovement(Vector3 newMove)
    {
        movement = newMove;
    }

    public void OnMove(InputValue value){
        if (controllerID == LobbyManager.self.clientID)
        {
            Vector3 moveVector = value.Get<Vector3>();
            if (moveVector.x < 0)
                movement = -transform.GetChild(0).right;
            else if (moveVector.x > 0)
                movement = transform.GetChild(0).right;

            if (moveVector.z > 0)
                movement = transform.GetChild(0).forward;
            else if (moveVector.z < 0)
                movement = -transform.GetChild(0).forward;
            movement *= 0.11f;
            //movement = value.Get<Vector3>();
            if (prevMovement != movement)
            {
                prevMovement = movement;
                SyncCall.SyncMovement(movement);
            }
        }
    }
}
