using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OctopusController;
using UnityEngine.UI;

public class IK_Scorpion : MonoBehaviour
{
    MyScorpionController _myController = new MyScorpionController();

    public IK_tentacles _myOctopus;

    [Header("Body")]
    float animTime;
    public float animDuration = 5;
    bool animPlaying = false;
    public Transform Body;
    public Transform StartPos;
    public Transform EndPos;

    [Header("Tail")]
    public Transform tailTarget;
    public Transform tail;

    [Header("Legs")]
    public Transform[] legs;
    public Transform[] legTargets;
    public Transform[] futureLegBases;
    public GameObject[] legBases;

    private Vector3 originalPosition;
    private MovingBall ball;
    private Slider strengthSlider;
    private float sliderSpeed = 20f;
    private bool up = true;

    RaycastHit hit;


    // Start is called before the first frame update
    void Start()
    {
        _myController.InitLegs(legs, futureLegBases, legTargets);
        _myController.InitTail(tail);
        originalPosition = transform.GetChild(0).position;
        strengthSlider = GameObject.Find("Strength").GetComponentInChildren<Slider>();
        ball = GameObject.Find("Ball").GetComponent<MovingBall>();
    }

    // Update is called once per frame
    void Update()
    {
        if (animPlaying)
            animTime += Time.deltaTime;

        NotifyTailTarget();

        if (Input.GetKeyUp(KeyCode.Space))
        {
            ball.shootSpeed = strengthSlider.value;
            NotifyStartWalk();
            animTime = 0;
            animPlaying = true;
            up = true;
        }
        else if (Input.GetKey(KeyCode.Space))
        {
            if (up)
                strengthSlider.value += sliderSpeed * Time.deltaTime;
            else
                strengthSlider.value -= sliderSpeed * Time.deltaTime;
            if (strengthSlider.value == strengthSlider.maxValue || strengthSlider.value == strengthSlider.minValue)
                up = up ? false : true;
        }

        if (animTime < animDuration)
        {
            Body.position = Vector3.Lerp(StartPos.position, EndPos.position, animTime / animDuration);
        }
        else if (animTime >= animDuration && animPlaying)
        {
            Body.position = EndPos.position;
            animPlaying = false;
        }

        _myController.UpdateIK();
        EnvironmentReacting();
        if (Input.GetKeyDown(KeyCode.R))
        {
            strengthSlider.value = strengthSlider.minValue;
            ResetPosition();
            ball.ResetPosition();
        }

    }

    void ResetPosition()
    {
        transform.GetChild(0).position = originalPosition;
    }

    //Function to send the tail target transform to the dll
    public void NotifyTailTarget()
    {
        _myController.NotifyTailTarget(tailTarget);
    }

    //Trigger Function to start the walk animation
    public void NotifyStartWalk()
    {

        _myController.NotifyStartWalk();
    }

    public void EnvironmentReacting()
    {
        for (int i = 0; i < futureLegBases.Length; i++)
        {
            Physics.Raycast(futureLegBases[i].transform.position + new Vector3(0, 1, 0), futureLegBases[i].transform.TransformDirection(Vector3.down), out hit, 2);
            futureLegBases[i].transform.position = hit.point;
        }

        Body.GetChild(1).transform.up = Vector3.Cross(futureLegBases[1].transform.position - futureLegBases[4].transform.position, futureLegBases[0].transform.position - futureLegBases[5].transform.position).normalized;
    }
}
