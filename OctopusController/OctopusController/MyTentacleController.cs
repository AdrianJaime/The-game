using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;




namespace OctopusController
{

    
    internal class MyTentacleController

    //MAINTAIN THIS CLASS AS INTERNAL
    {

        TentacleMode tentacleMode;
        Transform[] _bones;
        Transform _endEffectorSphere;

        public Transform[] Bones { get => _bones; }

        //Exercise 1.
        public Transform[] LoadTentacleJoints(Transform root, TentacleMode mode)
        {
            //TODO: add here whatever is needed to find the bones forming the tentacle for all modes
            //you may want to use a list, and then convert it to an array and save it into _bones

            List<Transform> list = new List<Transform>();
            Transform iterator = root;

            tentacleMode = mode;

            switch (tentacleMode){
                case TentacleMode.LEG:
                    //TODO: in _endEffectorsphere you keep a reference to the base of the leg
                    iterator = iterator.GetChild(0);
                    for(int i = 0; i < 3; i++)
                    {
                        list.Add(iterator);
                        iterator = iterator.GetChild(1);
                    }

                    _endEffectorSphere = iterator;
                    list.Add(iterator);
                    _bones = list.ToArray();
                    Debug.Log(tentacleMode + " " + _bones.Length);
                    break;

                case TentacleMode.TAIL:
                    //TODO: in _endEffectorsphere you keep a reference to the red sphere 
                    for (int i = 0; i < 5; i++)
                    {
                        list.Add(iterator);
                        iterator = iterator.GetChild(1);
                    }
                    iterator = iterator.parent;
                    _endEffectorSphere = iterator;
                    list.Add(iterator);
                    _bones = list.ToArray();
                    Debug.Log(tentacleMode + " " + _bones.Length);
                    break;
                case TentacleMode.TENTACLE:
                    //TODO: in _endEffectorphere you  keep a reference to the sphere with a collider attached to the endEffector
                    iterator = iterator.GetChild(0).GetChild(0);

                    for(int i = 0; i < 51; i++)
                    {
                        iterator = iterator.GetChild(0);
                        list.Add(iterator);

                    }

                    _endEffectorSphere = iterator.GetChild(0);
                    _bones = list.ToArray();
                    Debug.Log(tentacleMode + " " + _bones.Length);
                    break;
            }
            return Bones;
        }
    }
}
