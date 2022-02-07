using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace OctopusController
{
  
    public class MyScorpionController
    {
        //TAIL
        Transform tailTarget;
        Transform tailEndEffector;
        MyTentacleController _tail;
        float animationRange;

        //LEGS
        Transform[] legTargets;
        Transform[] legFutureBases;
        MyTentacleController[] _legs = new MyTentacleController[6];

        
        #region public
        public void InitLegs(Transform[] LegRoots,Transform[] LegFutureBases, Transform[] LegTargets)
        {
            _legs = new MyTentacleController[LegRoots.Length];
            //Legs init
            initPos = new Vector3[_legs.Length];
            moving = new bool[_legs.Length];
            alpha = new float[_legs.Length];
            InitLegOffsets(0.0f, 0.2f, 0.2f, 0.4f, 0.4f, 0.0f);
            legTargets = new Transform[LegTargets.Length];
            legFutureBases = new Transform[LegFutureBases.Length];
            for (int i = 0; i < LegRoots.Length; i++)
            {
                _legs[i] = new MyTentacleController();
                _legs[i].LoadTentacleJoints(LegRoots[i], TentacleMode.LEG);

                //TODO: initialize anything needed for the FABRIK implementation
                legTargets[i] = LegTargets[i];
                legFutureBases[i] = LegFutureBases[i];
            }
            aux = new Vector3[_legs[0].Bones.Length];
            dist = new float[_legs[0].Bones.Length - 1];

            for (int i = 0; i < dist.Length; i++)
            {
                Vector3 vec = _legs[0].Bones[i + 1].position - _legs[0].Bones[i].position;
                dist[i] = vec.magnitude;
            }
        }

        public void InitTail(Transform TailBase)
        {
            //TODO: Initialize anything needed for the Gradient Descent implementation
            _tail = new MyTentacleController();
            _tail.LoadTentacleJoints(TailBase, TentacleMode.TAIL);
            Solution = new float[_tail.Bones.Length];
            InitTailJoints();
            tailEndEffector = _tail.Bones[_tail.Bones.Length - 1];
        }

        //TODO: Check when to start the animation towards target and implement Gradient Descent method to move the joints.
        public void NotifyTailTarget(Transform target)
        {
            tailTarget = target;
            tailEndEffector = _tail.Bones[_tail.Bones.Length - 1];
            if ((tailEndEffector.position - tailTarget.position).magnitude <= distance)
            {
                updateTail();
            }
        }

        //TODO: Notifies the start of the walking animation
        public void NotifyStartWalk()
        {
            startWalking = true;
        }

        //TODO: create the apropiate animations and update the IK from the legs and tail

        public void UpdateIK()
        {
            if (startWalking)
            {
                updateLegPos();
                updateLegs();
            }
        }
        #endregion


        #region private

        float[] Solution = null;
        TailJoints[] _tailJoints;
        float StopThreshold = 0.1f;
        float DeltaGradient = 0.1f;
        float LearningRate = 50f;
        float distance = 4f;
        Vector3 target = new Vector3(0, 0, 0);


        bool startWalking = false;
        float legThreshold = 0.5f;
        Vector3[] offsets, initPos, aux;
        bool[] moving;
        float[] alpha, dist;




        //TODO: Implement the leg base animations and logic
        private void updateLegPos()
        {
            //check for the distance to the futureBase, then if it's too far away start moving the leg towards the future base position
            for(int i = 0; i < _legs.Length; i++)
            {
                if((legFutureBases[i].position - _legs[i].Bones[0].position).magnitude >= legThreshold - offsets[i].x && !moving[i])
                {
                    moving[i] = true;
                    initPos[i] = _legs[i].Bones[0].position;
                    alpha[i] = 0;
                }
                else if (moving[i])
                {

                    if (alpha[i] < 1)
                    {
                        Debug.Log(i);
                        alpha[i] += Time.deltaTime * 2;
                        if (alpha[i] + Time.deltaTime * 2 > 1) alpha[i] = 1;
                        _legs[i].Bones[0].position = initPos[i] + (legFutureBases[i].position - offsets[i] - initPos[i]) * alpha[i];

                    }
                    else
                        moving[i] = false;
                }

            }
        }
        //TODO: implement Gradient Descent method to move tail if necessary
        private void updateTail()
        {
            target = tailTarget.position;
            if (DistanceFromTarget(target, Solution) > StopThreshold)
                ApproachTarget(target);
        }
        //TODO: implement fabrik method to move legs 
        private void updateLegs()
        {
            for (int j = 0; j < _legs.Length; j++)
            {
                for (int i = 0; i < aux.Length; i++)
                {
                    aux[i] = _legs[j].Bones[i].position;
                }
                if (Vector3.Distance(aux[0], legTargets[j].position) <= this.dist.Sum())
                {
                    int iterator = 4;

                    while (iterator != 0)
                    {
                        aux[aux.Length - 1] = legTargets[j].position;
                        for (int i = aux.Length - 2; i >= 0; i--)
                            aux[i] = aux[i + 1] + ((_legs[j].Bones[i].position - aux[i + 1]).normalized * this.dist[i]);
                        aux[0] = _legs[j].Bones[0].position;
                        for (int i = 1; i <= aux.Length - 1; i++)
                            aux[i] = aux[i - 1] + ((aux[i] - aux[i - 1]).normalized * this.dist[i - 1]);
                        iterator--;
                    }
                }

                for (int i = 0; i <= _legs[j].Bones.Length - 2; i++)
                {
                    _legs[j].Bones[i].position = aux[i];
                    _legs[j].Bones[i].LookAt(aux[i + 1]);
                    _legs[j].Bones[i].Rotate(90, 0, 0, Space.Self);
                }
                _legs[j].Bones[_legs[j].Bones.Length - 1].position = aux[_legs[j].Bones.Length - 1];
            }
        }

        public struct PositionRotation
        {
            Vector3 position;
            Quaternion rotation;

            public PositionRotation(Vector3 position, Quaternion rotation)
            {
                this.position = position;
                this.rotation = rotation;
            }

            // PositionRotation to Vector3
            public static implicit operator Vector3(PositionRotation pr)
            {
                return pr.position;
            }
            // PositionRotation to Quaternion
            public static implicit operator Quaternion(PositionRotation pr)
            {
                return pr.rotation;
            }
        }
        private struct TailJoints
        {
            public Vector3 Axis, StartOffset;
            public float Min, Max;
        }
        private void InitTailJoints()
        {

            _tailJoints = new TailJoints[_tail.Bones.Length];

            //TAIL JOINT 0            
            _tailJoints[0].Axis = new Vector3(0, 0, 1);
            _tailJoints[0].Min = -110;
            _tailJoints[0].Max = 110;
            _tailJoints[0].StartOffset = _tail.Bones[0].localPosition;
            Solution[0] = _tail.Bones[0].localRotation.z;

            //TAIL JOINT 1
            _tailJoints[1].Axis = new Vector3(1, 0, 0);
            _tailJoints[1].Min = -100;
            _tailJoints[1].Max = 30;
            _tailJoints[1].StartOffset = _tail.Bones[1].localPosition;
            Solution[1] = _tail.Bones[1].localRotation.x;

            //TAIL JOINT 2
            _tailJoints[2].Axis = new Vector3(0, 1, 0);
            _tailJoints[2].Min = -60;
            _tailJoints[2].Max = 15;
            _tailJoints[2].StartOffset = _tail.Bones[2].localPosition;
            Solution[2] = _tail.Bones[2].localRotation.y;

            //TAIL JOINT 3
            _tailJoints[3].Axis = new Vector3(0, 1, 0);
            _tailJoints[3].Min = -150;
            _tailJoints[3].Max = 150;
            _tailJoints[3].StartOffset = _tail.Bones[3].localPosition;
            Solution[3] = _tail.Bones[3].localRotation.y;

            //TAIL JOINT 4
            _tailJoints[4].Axis = new Vector3(1, 0, 0);
            _tailJoints[4].Min = -180;
            _tailJoints[4].Max = 180;
            _tailJoints[4].StartOffset = _tail.Bones[4].localPosition;
            Solution[4] = _tail.Bones[4].localRotation.x;

            //TAIL JOINT END EFFECTOR
            _tailJoints[5].Axis = new Vector3(1, 0, 0);
            _tailJoints[5].Min = -180;
            _tailJoints[5].Max = 180;
            _tailJoints[5].StartOffset = _tail.Bones[5].localPosition;
            Solution[5] = _tail.Bones[5].localRotation.x;
        }
        public PositionRotation ForwardKinematics(float[] Solution)
        {
            Vector3 prevPoint = _tail.Bones[0].position;

            // Takes object initial rotation into account
            Quaternion rotation = _tail.Bones[0].parent.parent.rotation;
            for (int i = 1; i < _tail.Bones.Length - 1; i++)
            {
                // Rotates around a new axis

                rotation *= Quaternion.AngleAxis(Solution[i - 1], _tailJoints[i - 1].Axis);

                Vector3 nextPoint = prevPoint + rotation * _tailJoints[i].StartOffset * 0.32622f;

                //Debug.DrawLine(prevPoint, nextPoint, Color.green);
                prevPoint = nextPoint;
            }

            // The end of the effector
            return new PositionRotation(prevPoint, rotation);
        }
        public float DistanceFromTarget(Vector3 target, float[] Solution)
        {
            Vector3 point = ForwardKinematics(Solution);
            return Vector3.Distance(point, target);
        }
        public void ApproachTarget(Vector3 target)
        {

            float p_gradient;

            for (int i = _tail.Bones.Length - 1; i >= 0; i--)
            {
                p_gradient = CalculateGradient(target, Solution, i, DeltaGradient);
                Solution[i] -= LearningRate * p_gradient;
                Solution[i] = Mathf.Clamp(Solution[i], _tailJoints[i].Min, _tailJoints[i].Max);
                
                if (_tailJoints[i].Axis.x == 1) _tail.Bones[i].localEulerAngles = new Vector3(Solution[i], 0, 0);
                else
                if (_tailJoints[i].Axis.y == 1) _tail.Bones[i].localEulerAngles = new Vector3(0, Solution[i], 0);
                else
                if (_tailJoints[i].Axis.z == 1) _tail.Bones[i].localEulerAngles = new Vector3(0, 0, Solution[i]);


                if (DistanceFromTarget(target, Solution) < StopThreshold) return;
            }
        }
        public float CalculateGradient(Vector3 target, float[] Solution, int i, float delta)
        {
            float solutionAngle = Solution[i];


            float f_x = DistanceFromTarget(target, Solution);

            Solution[i] += delta;
            float f_x_plus_h = DistanceFromTarget(target, Solution);

            float gradient = (f_x_plus_h - f_x) / delta;

            Solution[i] = solutionAngle;

            return gradient;
        }
        void InitLegOffsets(float a, float b, float c, float d, float e, float f)
        {
            offsets = new Vector3[_legs.Length];
            offsets[0] = new Vector3(a, 0, 0);
            offsets[1] = new Vector3(b, 0, 0);
            offsets[2] = new Vector3(c, 0, 0);
            offsets[3] = new Vector3(d, 0, 0);
            offsets[4] = new Vector3(e, 0, 0);
            offsets[5] = new Vector3(f, 0, 0);
        }

        #endregion
    }
}
