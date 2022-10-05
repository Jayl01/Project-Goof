using System.Collections.Generic;
using UnityEngine;


/* notes:
can make limbs in a set of two bones or three mass circles with 2 lines connecting them.
these lines have an orientation and their forwards vector will be in the direction of head - root. 
with head being the mass circle outermost 
(so like shoulder, elbow, hand in a connection between elbow and hand the hand is the head and the tail is the elbow)
keep in mind that it is the same for hip, knee, foot
then the upwards vector will be the cross of hand - elbow and shoulder - elbow. this will give a vector perpendicular to the plane formed by the limb
sideways vector is then implied or can be calculated with cross of up and forward vector

we can also use local space in order to calculate the rotation of each bone along its forward axis. 
this will make sure that we can place limits on to how much a bone can rotate along its forward axis.
using same idea we can make sure that limbs don't rotate past 180 degrees as that would be broken bones in a normal human.

could also implement same three mass circle implementation for neck and head (lower neck, place where neck meets head, top of head)
however for the head since it can rotate in any direction can implement just making sure it is a certain angle from a vector the is going to be around 45 degrees from the horizon facing forwards and up. can be found using handy dandy formula found here https://stackoverflow.com/questions/5188561/signed-angle-between-two-3d-vectors-with-same-origin-within-the-same-plane
honestly I think since we don't need it signed we could just use dot product without the need for a cross product but well see once we actually implement it

same thing that was said about the head can be said about the spine
the rotation for the upper spine can be determined from the locations of the shoulders
if you do Rshoulder - Lshoulder then that should give you the right orientation vector

what we could then do is cross product of Rshoulder - neck and Lshoulder- neck which would give you the forward vector of spine
although wait just do a simple neck minus spine and thats the forward direction of the spine.... yeah def doing that instead


want to make it constraint based. so this bone class will only be used to apply forces to the bone and then call all the other constraint classes.
then there will be a distance constraint between two bones (used on every bone), hinge constraint between three bones (used on elbows and knees). a rotation around an axis constraint (used for making sure bones don't rotate too far around their forward axis in order to prevent flesh from rotating too far around a bone, aka every bone will have this) and then ball and socket constraint between two bones (used to make sure bones don't rotate too far around their root bone)

that way the bone class will just call all of these constraints that are attached to the bone object and each constraint can have its parameters in the inspector.

*/


//this should be attached to each moving bone in armature
public class Bone : MonoBehaviour
{
    Vector3 netForce, vel;

    [SerializeField]
    float mass = 1;
    float massInv;

    Component[] childBones;


    // Start is called before the first frame update
    void Start()
    {
        // childBones = FindComponentInChildren(typeof(Bone));
        netForce = Vector3.zero;
        vel = Vector3.zero;

        // this is done in order to multiply by 1/mass for f=ma (a = f/m = f * 1/m) since multiplication is cheaper than division
        // also checking that if mass is 0 then we will treat is as a kinematic rigidbody. aka an immovable object via normal forces
        massInv = (mass != 0) ? 1/mass : 0;


    }

    // Update is called once per frame
    void FixedUpdate()
    {
        // vel = (((transform.position + vel) - root).normalized * length + root) - transform.position;
        transform.position +=vel;
        // parentT.rotation = Quaternion.LookRotation(transform.position - parentT.position, Vector3.Cross());
        // PhysicsManager.GRAVITY = new Vector3(Random.Range(-10,10), Random.Range(-10,10), Random.Range(-10,10)) * 0.1f;
    }


    public void AddForce(Vector3 force){
        netForce += force;
    }

    public void SetNetForce(Vector3 force){
        netForce = force;
    }

// this should only be called once per frame after all the force calculations have been calculated
    public void UpdateVelocity(){
        vel += netForce * massInv;
    }



// call this from the root bone in order to find every single Bone code in all of its children and their children and so on
    public List<Bone> FindChildren(){ 
        // Debug.Log($"Starting find Children from " + ToString());
        if (transform.childCount > 0)
        {
            // Debug.Log($"Found " + transform.childCount + " children from " + ToString());
            List<Bone> children = new List<Bone>();
            for (int i = 0; i < transform.childCount; i++)
            {
                children.Add(transform.GetChild(i).gameObject.GetComponent<Bone>());
                if (children[children.Count -1] != null)
                {
                    // Debug.Log(children[children.Count -1].ToString() + " HAS BONE CODE (from " + ToString() + ")");
                    List<Bone> temp = children[children.Count -1].FindChildren();
                    if (temp != null)
                    {
                        children.AddRange(temp);
                    }
                }
                else
                {
                    // Debug.Log($"uhm you probably messed up because "+ transform.GetChild(i).ToString() + "does not have a Bone component.");
                }
            }

            return children;
        }
        // Debug.Log($"this " + ToString() + "mfer is lonely had no one to make children with");
        return null;
    }

    //@Todo: THIS IS WRONG RIGHT NOW BUT GO FIX THIS
    
    // void invokeMethodAcrossAllBones(Action<> method){
    //     foreach (Bone bone in childBones){
    //         bone.method;
    //     }
    // }

    public void UpdatePosition()
    {
        transform.position = transform.position + vel * Time.fixedDeltaTime;
    }
    public Vector3 GetPredictedPosition(){
        return transform.position + vel * Time.fixedDeltaTime;
    }


    /// <summary> only keeping this if I ever want to go back to rotational physics
    // void RotatePhysicsAttempt(){
        // PhysicsManager.GRAVITY = Quaternion.Euler(-Time.fixedDeltaTime * 10,1,1) * PhysicsManager.GRAVITY;
        // localForward = transform.TransformDirection(new Vector3(0,1,0));
        // accel = Mathf.Sin(Mathf.Acos(Vector3.Dot(localForward, PhysicsManager.GRAVITY.normalized))) * PhysicsManager.GRAVITY;
        // accel += Mathf.Sin(Mathf.Acos(Vector3.Dot(localForward, initial))) * initial / flex;
        // vel = (vel + accel) * 0.99f;
        // // Spin the object around the target at 20 degrees/second.
        // transform.RotateAround(transform.position, Vector3.Cross(localForward, vel).normalized, vel.magnitude * Time.fixedDeltaTime);

        // // transform.rotation = Quaternion.Euler(transform.eulerAngles.x, 0, transform.eulerAngles.z);
    // }
}