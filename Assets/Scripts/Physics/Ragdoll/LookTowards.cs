using UnityEngine;
public class LookTowards : MonoBehaviour, Constraint{
    public Transform lookTarget, rollTarget;

    [Range(0,1)]
    public float lookDampening, rollDampening;

    void Awake(){
        if (lookTarget == null)
        {
            if (transform.childCount > 0)
            {
                lookTarget = transform.GetChild(0);
            }
            else{
                Debug.Log($"If you are going to add a look constraint to an end bone make sure you define the target transform");
            }
        }
        if(rollTarget == null){
            rollTarget = lookTarget;
        }
    }

    public void Work(){
        transform.rotation = Quaternion.LookRotation(lookTarget.position - transform.position, rollTarget.TransformVector(Vector3.up));
    }
}