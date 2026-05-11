using System;
using UnityEngine;

public class CameraFollow:MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    [Header("Follow Setting")]
    public float smoothSpeed = 5f;
    public Vector2 offset = new Vector2(2f,1f);

    [Header("Dead Zone")]
    public float deadZoneX = 0.5f;
    public float deadZoneY = 0.2f;

    [Header("Range")]
    public bool useBounds = false;
    public float minX,maxX;
    public float minY,maxY;


    private Vector3 targetPos;
    private float currentOffsetX;

    void LateUpdate()
    {
        if (target == null) return;

        CalculateOffset();
        FollowTarget();
    }
    void CalculateOffset()
    {
        SpriteRenderer sr = target.GetComponentInChildren<SpriteRenderer>();
        float direcetion = sr!=null && sr.flipX ? -1f : 1f;

        currentOffsetX = Mathf.Lerp(currentOffsetX,offset.x * direcetion, Time.deltaTime);
    }
    void FollowTarget()
    {
        Vector3 current = transform.position;
        Vector3 desired = new Vector3(
            target.position.x + currentOffsetX,
            target.position.y + offset.y ,
            current.z);

        float distX = Mathf.Abs(desired.x - current.x);
        float distY = Mathf.Abs(desired.y - current.y);

        if(distX > deadZoneX || distY > deadZoneY)
        {
            Vector3 smoothed = Vector3.Lerp(current,desired,smoothSpeed * Time.deltaTime);
            transform.position = smoothed;

        }
        if (useBounds)
        {
            transform.position = new Vector3(
                Mathf.Clamp(transform.position.x, minX, maxX)
                ,Mathf.Clamp(transform.position.y, minY,maxY)
            );
        }

    }
    
    
}
