using UnityEngine;

public class Distance : MonoBehaviour, Constraint {
    public Transform target;

    Bone aBone, bBone;

    DistanceManager aMan, bMan;

    [SerializeField]
    float targetDistance =-1;

    [Range(0, 1)]
    public float dampening = 1;

    float aMass, bMass, totalMass;

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
        aBone = transform.GetComponent<Bone>();
        bBone = target.GetComponent<Bone>();
        aMan = aBone.transform.GetComponent<DistanceManager>().RegisterConstraint(this);
        bMan = bBone.transform.GetComponent<DistanceManager>().RegisterConstraint(this);
        aMass = aBone.mass;
        bMass = bBone.mass;
        totalMass = aMass + bMass;
        if(targetDistance == -1){
            targetDistance = (target.position - transform.position).magnitude;
        }
    }

    // without mass
    public void Work(){
        Vector3 correction, direction = correction = bBone.snapshot - aBone.snapshot;
        direction.Normalize();
        // keep in mind in this case the original value of correction is not the direction but the offset vector
        // when initialized both correction and direction were the offset vector, just that direction is normalized in the next line
        // this prevents an extra variable of using both offset and direction and also prevents calling .normalized to get direction from offset
        // correction -=  (direction * targetDistance);
        aBone.Translate(direction * (targetDistance - correction.magnitude)* -0.5f);
        bBone.Translate(direction * (correction.magnitude - targetDistance)* -0.5f);
        // Vector3 myVelInDir = Vector3.Dot(myBone.vel, direction.normalized) * direction.normalized;
        // Vector3 targetVelInDir = Vector3.Dot(targetBone.vel, direction.normalized) * direction.normalized;
        // Vector3 combinedVelInDir = (myMass * myVelInDir + targetMass * targetVelInDir) / (myMass + targetMass);
        // myBone.AddVelocity(combinedVelInDir - myVelInDir);
        // targetBone.AddVelocity(combinedVelInDir - targetVelInDir);

    }


    // with mass
    // public void Work(){
    //     Vector3 offset = targetBone.GetPredictedPosition() - myBone.GetPredictedPosition();
    //     Vector3 correction = offset - (offset.normalized * targetDistance);
    //     myBone.Translate(correction * targetBone.mass/totalMass);
    //     targetBone.Translate(correction * myBone.mass/totalMass * -1);
    //     Vector3 myVelInDir = Vector3.Dot(myBone.vel, offset.normalized) * offset.normalized;
    //     Vector3 targetVelInDir = Vector3.Dot(targetBone.vel, offset.normalized) * offset.normalized;
    //     Vector3 combinedVelInDir = (myMass * myVelInDir + targetMass * targetVelInDir) / (myMass + targetMass);
    //     myBone.AddVelocity(combinedVelInDir - myVelInDir);
    //     targetBone.AddVelocity(combinedVelInDir - targetVelInDir);
    // }
}