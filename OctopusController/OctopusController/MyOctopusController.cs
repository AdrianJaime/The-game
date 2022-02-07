using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{
    public enum TentacleMode { LEG, TAIL, TENTACLE };

    public class MyOctopusController 
    {
        
        MyTentacleController[] _tentacles =new  MyTentacleController[4];

        Transform _currentRegion;
        Transform _target;

        Transform[] _randomTargets;// = new Transform[4];


        float _twistMin, _twistMax;
        float _swingMin, _swingMax;

        #region public methods
        //DO NOT CHANGE THE PUBLIC METHODS!!

        public float TwistMin { set => _twistMin = value; }
        public float TwistMax { set => _twistMax = value; }
        public float SwingMin {  set => _swingMin = value; }
        public float SwingMax { set => _swingMax = value; }
        

        public void TestLogging(string objectName)
        {

           
            Debug.Log("hello, I am working "+objectName);

            
        }

        public void Init(Transform[] tentacleRoots, Transform[] randomTargets)
        {
            _tentacles = new MyTentacleController[tentacleRoots.Length];

            // foreach (Transform t in tentacleRoots)
            for(int i = 0;  i  < tentacleRoots.Length; i++)
            {

                _tentacles[i] = new MyTentacleController();
                _tentacles[i].LoadTentacleJoints(tentacleRoots[i],TentacleMode.TENTACLE);
                //TODO: initialize any variables needed in ccd
            }
            _randomTargets = randomTargets;
            _theta = new float[_tentacles[0].Bones.Length];
            _cos = new float[_tentacles[0].Bones.Length];
            _sin = new float[_tentacles[0].Bones.Length];
            tpos = _randomTargets[0].position;
            //TODO: use the regions however you need to make sure each tentacle stays in its region
        }

              
        public void NotifyTarget(Transform target, Transform region)
        {
            _currentRegion = region;
            _target = target;
        }

        public void NotifyShoot() {
            //TODO. what happens here?
            shoot = true;
            Debug.Log("Shoot");
             
        }


        public void UpdateTentacles()
        {
            //TODO: implement logic for the correct tentacle arm to stop the ball and implement CCD method
            update_ccd();
        }




        #endregion


        #region private and internal methods
        //todo: add here anything that you need
        float[] _theta, _sin, _cos;
        bool shoot = false;
        float _epsilon = 0.1f;
        Vector3 tpos;
        bool done = false;
        int Mtries = 10;
        int tries = 0;

        double SimpleAngle(double theta)
        {
            theta = theta % (2.0 * Mathf.PI);
            if (theta < -Mathf.PI)
                theta += 2.0 * Mathf.PI;
            else if (theta > Mathf.PI)
                theta -= 2.0 * Mathf.PI;
            return theta;
        }
        void update_ccd()
        {
            Vector3 r1, r2;
            if (!done)
            {
                for (int t = 0; t < _tentacles.Length; t++)
                {
                    if (tries <= Mtries)
                    {
                        for (int i = _tentacles[t].Bones.Length - 2; i >= 0; i--)
                        {
                            r1 = _tentacles[t].Bones[_tentacles[t].Bones.Length - 1].transform.position - _tentacles[t].Bones[i].transform.position;

                            if (shoot)
                                r2 = _target.position - _tentacles[t].Bones[i].position;
                            else
                                r2 = _randomTargets[t].position - _tentacles[t].Bones[i].position;

                            // to avoid small numbers
                            if (r1.magnitude * r2.magnitude <= 0.001f)
                            {
                                _cos[i] = 1;
                                _sin[i] = 0;
                            }
                            else
                            {
                                _cos[i] = Vector3.Dot(r1, r2) / (r1.magnitude * r2.magnitude);
                                _sin[i] = (Vector3.Cross(r1, r2)).magnitude / (r1.magnitude * r2.magnitude);

                            }

                            // The axis of rotation 
                            Vector3 axisBase = (Vector3.Cross(r1, r2)).normalized;


                            // angle between r1 and r2 
                            _theta[i] = Mathf.Acos(Mathf.Max(-1, Mathf.Min(1, _cos[i])));

                            // check if sin component is negative then invert angle
                            if (_sin[i] < 0.0f)
                                _theta[i] = -_theta[i];


                            // obtain an angle value between -pi and pi, and then convert to degrees
                            _theta[i] = (float)SimpleAngle(_theta[i]) * Mathf.Rad2Deg;

                            Quaternion prevRotation = _tentacles[t].Bones[i].transform.localRotation;

                            // rotate joint.

                                Quaternion rot = Quaternion.AngleAxis(_theta[i], axisBase);
                                rot.x = 0;
                                rot.y = 0;
                                _tentacles[t].Bones[i].rotation = rot * _tentacles[t].Bones[i].rotation;

                        }

                        tries++;
                    }

                    Vector3 dist = _randomTargets[t].transform.position - _tentacles[t].Bones[_tentacles[t].Bones.Length - 1].transform.position;

                    if (dist.magnitude < _epsilon)
                        done = true;
                    else
                        done = false;
                    if (_randomTargets[t].position != tpos)
                    {
                        tries = 0;
                        tpos = _randomTargets[t].transform.position;
                    }
                }

            }



        }



            #endregion


    }
}
