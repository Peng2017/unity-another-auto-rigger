﻿using System.Collections.Generic;
using UnityEngine;

namespace AnotherAutoRigger
{
    [System.Serializable]
    public class YawPitchRoll : MonoBehaviour
    {
        // mapping options
        public enum MappingOptions { ZXY=0, XYZ=1, XZY=2, YXZ=3, YZX=4, ZYX=5 };
        public MappingOptions mapping;

        // transforms
        public string origin;
        public string insertion;
        public Transform originTransform;
        public Transform insertionTransform;

        // euler offset
        public float _parentOffsetX;
        public float _parentOffsetY;
        public float _parentOffsetZ;

        // private
        private bool isValid;
        private Matrix4x4 parentOffsetMatrix;
        private int yawIndex = 0;
        private int pitchIndex = 1;
        private int rollIndex = 2;
        private Vector3 yawPitchRoll;

        private Quaternion GetLocalRotation()
        {
            // validate transforms
            if (originTransform == null || insertionTransform == null)
                return Quaternion.identity;

            // convert transforms to 4x4 matrices
            Matrix4x4 parentMatrix = Matrix4x4.TRS(
                originTransform.position, 
                originTransform.rotation, 
                originTransform.localScale
            );
            Matrix4x4 childMatrix = Matrix4x4.TRS(
                insertionTransform.position, 
                insertionTransform.rotation, 
                insertionTransform.localScale
            );

            // get local transformation matrix of child
            Matrix4x4 localMatrix =  (parentOffsetMatrix * parentMatrix).inverse * childMatrix;
            return localMatrix.rotation;
        }

        private Vector3 CalculateYawPitchRoll()
        {
            // calculate yaw pitch roll
            Quaternion rotation = GetLocalRotation();
            float yaw = Mathf.Asin(2 * rotation.x * rotation.y + 2 * rotation.z * rotation.w);
            float pitch = Mathf.Atan2(2 * rotation.x * rotation.w - 2 * rotation.y * rotation.z, 1 - 2 * rotation.x * rotation.x - 2 * rotation.z * rotation.z);
            float roll = Mathf.Atan2(2 * rotation.y * rotation.w - 2 * rotation.x * rotation.z, 1 - 2 * rotation.y * rotation.y - 2 * rotation.z * rotation.z);

            // populate yaw pitch roll vector
            Vector3 yawPitchRoll = new Vector3();
            yawPitchRoll[yawIndex] = yaw * Mathf.Rad2Deg;
            yawPitchRoll[pitchIndex] = pitch * Mathf.Rad2Deg * -1;
            yawPitchRoll[rollIndex] = roll * Mathf.Rad2Deg * -1;

            return yawPitchRoll;
        }

        // -------------------------------------------------------------------------

        public Vector3 GetYawPitchRoll()
        {
            return yawPitchRoll;
        }

        // -------------------------------------------------------------------------

        void Awake()
        {
            // populate transforms
            if (originTransform == null)
                originTransform = this.GetComponentInGameObjectFromString<Transform>(origin);
            if (insertionTransform == null)
                insertionTransform = this.GetComponentInGameObjectFromString<Transform>(insertion);

            // validate
            isValid = (originTransform == null || insertionTransform == null) ? false : true;
        }

        void Start()
        {
            // only continue when yaw pitch roll is valid
            if (!isValid)
                return;

            // construct offset rotation on parent
            Quaternion offsetQuat = Quaternion.Euler(_parentOffsetX, _parentOffsetY, _parentOffsetZ);
            parentOffsetMatrix = Matrix4x4.Rotate(offsetQuat);

            // construct mapping list to get index 
            List<string> mappingList = new List<string>();
            mappingList.Add("X");
            mappingList.Add("Y");
            mappingList.Add("Z");

            // get yaw pitch roll indices
            string mappingString = mapping.ToString();
            yawIndex = mappingList.IndexOf(mappingString[0].ToString());
            pitchIndex = mappingList.IndexOf(mappingString[1].ToString());
            rollIndex = mappingList.IndexOf(mappingString[2].ToString());
        }

        void Update()
        {
            // only continue when yaw pitch roll is valid
            if (!isValid)
                return;

            // calculate yaw pitch roll
            yawPitchRoll = CalculateYawPitchRoll();
        }
    }
}