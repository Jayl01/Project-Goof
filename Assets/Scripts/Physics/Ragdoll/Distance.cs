using UnityEngine;

public class Distance : MonoBehaviour, Constraint {
    public Transform target;

    Bone targetBone, myBone;

    [SerializeField]
    float targetDistance =-1;

    [Range(0, 1)]
    public float dampening = 1;

    float myMass, targetMass, totalMass;

    void Awake()
    {
        if (target == null)
        {
            if (transform.childCount > 0)
            {
                target = transform.GetChild(0);
            }
            else
            {
                Debug.Log($"If you are going to add a Distance constraint to an end bone make sure you define the target transform");
            }
        }
        myBone = transform.GetComponent<Bone>();
        targetBone = target.GetComponent<Bone>();
        myMass = myBone.mass;
        targetMass = targetBone.mass;
        totalMass = myMass + targetMass;
        if(targetDistance == -1){
            targetDistance = (target.position - transform.position).magnitude;
        }
    }

// does not deal with mass right now
    public void Work(){
        Vector3 offset = targetBone.GetPredictedPosition() - myBone.GetPredictedPosition();
        Vector3 correction = offset - (offset.normalized * targetDistance);
        myBone.AddPosition(correction * targetBone.mass/totalMass);
        targetBone.AddPosition(correction * myBone.mass/totalMass * -1);
        Vector3 myVelInDir = Vector3.Dot(myBone.vel, offset.normalized) * offset.normalized;
        Vector3 targetVelInDir = Vector3.Dot(targetBone.vel, offset.normalized) * offset.normalized;
        Vector3 combinedVelInDir = (myMass * myVelInDir + targetMass * targetVelInDir) / (myMass + targetMass);
        myBone.AddVelocity(combinedVelInDir - myVelInDir);
        targetBone.AddVelocity(combinedVelInDir - targetVelInDir);

    }
}