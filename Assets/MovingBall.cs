using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingBall : MonoBehaviour
{
    [SerializeField]
    IK_tentacles _myOctopus;

    //movement speed in units per second
    [Range(-1.0f, 1.0f)]
    [SerializeField]
    private float _movementSpeed = 5f;
    private float mass = 1f;
    private bool shoot = false;
    private bool stopped = false;
    private bool goal = true;
    private GameObject target;
    private Vector3 dirVec, originalPosition;

    public float shootSpeed;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("BlueTarget");
        dirVec = Vector3.zero;
        originalPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.identity;

        if (shoot && !stopped)
        {
            ShootBall();
        }
    }

    void ShootBall()
    {
        dirVec = target.transform.position - transform.position;
        dirVec.Normalize();

        gameObject.transform.Translate(dirVec * shootSpeed * Time.deltaTime);
        Debug.Log("Shot with " + shootSpeed);
    }

    public void ResetPosition()
    {
        dirVec = Vector3.zero;
        goal = goal == true ? false : true;
        stopped = false;
        shoot = false;
        transform.position = originalPosition;
    }
    private void OnCollisionEnter(Collision collision)
    {
        _myOctopus.NotifyShoot();
        shoot = true;
        if (collision.gameObject.transform.name == "BallRegion")
            stopped = true;
    }
}
