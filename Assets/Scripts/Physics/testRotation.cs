
using UnityEngine;

public class testRotation : MonoBehaviour
{
    Vector3 gravity, accel, vel, offset, root;
    Transform oldParent;
    [SerializeField]
    Transform newParent;
    float length;
    // Start is called before the first frame update
    void Start()
    {
        oldParent = transform.parent;
        accel = Vector3.zero;
        vel = Vector3.zero;
        gravity = new Vector3(0,-9.8f,0);
        root = oldParent.position;
        offset = transform.position - root;
        length = offset.magnitude;
        // Debug.Log(transform.rotation.EulerAngles);
        transform.SetParent(newParent);


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        root = oldParent.position;
        accel = gravity;
        vel += accel * Time.fixedDeltaTime;
        vel = (((transform.position + vel) - root).normalized * length + root) - transform.position;
        transform.position +=vel;
        oldParent.rotation = Quaternion.LookRotation(transform.position - oldParent.position);
        gravity = new Vector3(Random.Range(-10,10), Random.Range(-10,10), Random.Range(-10,10)) * 0.1f;
    }


    /// <summary> only keeping this if I ever want to go back to rotational physics
    void rotatePhysicsAttempt(){
        // gravity = Quaternion.Euler(-Time.fixedDeltaTime * 10,1,1) * gravity;
        // localForward = transform.TransformDirection(new Vector3(0,1,0));
        // accel = Mathf.Sin(Mathf.Acos(Vector3.Dot(localForward, gravity.normalized))) * gravity;
        // accel += Mathf.Sin(Mathf.Acos(Vector3.Dot(localForward, initial))) * initial / flex;
        // vel = (vel + accel) * 0.99f;
        // // Spin the object around the target at 20 degrees/second.
        // transform.RotateAround(transform.position, Vector3.Cross(localForward, vel).normalized, vel.magnitude * Time.fixedDeltaTime);

        // // transform.rotation = Quaternion.Euler(transform.eulerAngles.x, 0, transform.eulerAngles.z);
    }
}
