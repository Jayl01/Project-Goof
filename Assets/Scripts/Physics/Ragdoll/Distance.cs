using UnityEngine;

public class Distance : MonoBehaviour, Constraint {
    public Transform target;

    Bone targetBone, myBone;

    [SerializeField]
    float targetDistance =-1;

    [Range(0, 1)]
    public float dampening;

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
        if(targetDistance == -1){
            targetDistance = (target.position - transform.position).magnitude;
        }
    }

// does not deal with mass right now
    public void Work(){
        Vector3 offset = target.position - transform.position;
        float distance = offset.magnitude;
        float correction = (targetDistance - distance)/2;
        myBone.AddVelocity(offset * correction * -1);
        targetBone.AddVelocity(offset * correction);


    }
}