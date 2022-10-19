using UnityEngine;
using UnityEngine.InputSystem;

public class MovingCube : MonoBehaviour
{
    public float speed = 6.5f;

    public void FixedUpdate()
    {
        
    }

    public void OnMove(InputValue value)
    {
        Vector3 moveVector = value.Get<Vector3>();
        if (moveVector.x < 0)
            transform.position += -transform.right * speed;
        else if (moveVector.x > 0)
            transform.position += transform.right * speed;

        if (moveVector.z > 0)
            transform.position += transform.forward * speed;
        else if (moveVector.z < 0)
            transform.position += -transform.forward * speed;
    }
}
