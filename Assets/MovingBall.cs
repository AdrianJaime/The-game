using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private Text rotationVelocityText;

    public float shootSpeed, rotationSpeed, effect, actualRotation;

    // Start is called before the first frame update
    void Start()
    {
        target = GameObject.Find("BlueTarget");
        dirVec = Vector3.zero;
        originalPosition = transform.position;
        rotationVelocityText = GameObject.Find("Rotation Velocity Text").GetComponent<Text>();
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
        Vector3 newPos;

        newPos.x = transform.position.x + 
        gameObject.transform.Translate(dirVec * shootSpeed * Time.deltaTime);

        actualRotation += effect * shootSpeed * rotationSpeed * Time.deltaTime;
        rotationVelocityText.text = "Rotation Velocity: " + (effect * shootSpeed * rotationSpeed) + " deg/s";
        transform.Rotate(Vector3.up, actualRotation, Space.World);
        Debug.Log("Shot with " + actualRotation);

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

        if (collision.gameObject.transform.name == "BallRegion" && !goal)
            stopped = true;
        else if (goal && !shoot)
        {
        _myOctopus.NotifyShoot();
        shoot = true;
        dirVec = target.transform.position - transform.position;
        dirVec.Normalize();
        }
    }
}
