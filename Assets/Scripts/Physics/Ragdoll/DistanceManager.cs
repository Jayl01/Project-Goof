using System.Collections.Generic;
using UnityEngine;
public class DistanceManager : MonoBehaviour, Constraint
{
    public Vector3 resultPos;
    int counter;

    List<Distance> distances = new List<Distance>();
    public void start()
    {
        
    }
    public DistanceManager RegisterConstraint(Distance toAdd){
        distances.Add(toAdd);
        return this;
    }
    public void RemoveConstraint(Distance toRem){
        distances.Remove(toRem);
    }

    public void Recommend(Vector3 rec){
        
    }
    public void Work(){
        
        for(int i = 0; i < distances.Count; i++){
            distances[i].Work();
        }
    }
}