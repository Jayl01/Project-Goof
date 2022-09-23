using UnityEngine;

public class testRotation : MonoBehaviour
{
    public float gravity, accel, vel;
    // Start is called before the first frame update
    void Start()
    {
        gravity = 9.8f;
        accel = 0;
        vel = 0;
    }

    // Update is called once per frame
    void Update()
    {
        accel = 1-Vector3.Dot(transform.TransformDirection(new Vector3(0,1,0)), Vector3.down);
        vel+=accel;
        // Spin the object around the target at 20 degrees/second.
        transform.RotateAround(transform.position, Vector3.Cross(transform.TransformDirection(new Vector3(0,1,0)), Vector3.down).normalized, vel * Time.deltaTime);
        // transform.rotation = Quaternion.Euler(transform.eulerAngles.x, 0, transform.eulerAngles.z);
    }
}
