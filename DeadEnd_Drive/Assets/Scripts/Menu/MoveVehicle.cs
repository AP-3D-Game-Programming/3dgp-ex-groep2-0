using UnityEngine;

public class MoveVehicle : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float speed = 3f;

    void Start()
    {
        transform.position = pointA.position;
    }

    void Update()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            pointB.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, pointB.position) < 0.1f)
        {
            transform.position = pointA.position;
        }
    }
}
