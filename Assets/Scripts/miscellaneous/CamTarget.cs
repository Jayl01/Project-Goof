using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamTarget : MonoBehaviour
{
    [SerializeField]
    Transform player;
    Bone playerBone;

    [SerializeField]
    float lookAhead, angle, offsetAngle, dist, dampening;
    
    [SerializeField]
    Vector3 target;

    void Awake()
    {
        playerBone = player.gameObject.GetComponent<Bone>();
        transform.position = player.position;
        target = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
        updateCam();
    }

    void updateCam(){
        target = player.position + playerBone.vel * lookAhead * Time.fixedDeltaTime;
        transform.position = transform.position + (target-transform.position)/dampening + new Vector3(0, Mathf.Sin(angle * Mathf.Deg2Rad), -Mathf.Cos(angle * Mathf.Deg2Rad)) * dist;
        transform.rotation = Quaternion.Euler(new Vector3(angle + offsetAngle, 0, 0));
    }


}