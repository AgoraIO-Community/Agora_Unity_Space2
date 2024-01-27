using System;
using System.Runtime.InteropServices;
using UnityEngine;
namespace Agora.Demo.Meta.Model
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public class TransformData
    {
        public string UserId;
        public Vector3 LocalPosition;
        public Vector3 EulerAngles;
        public Vector3 LocalScale;
        public Vector3 Forward;

        public TransformData() { }

        public TransformData(Transform transform)
        {
            LocalPosition = transform.localPosition;
            EulerAngles = transform.localRotation.eulerAngles;
            LocalScale = transform.localScale;
            Forward = transform.forward;
        }
        public string ToJSON()
        {
            return JsonUtility.ToJson(this);
        }

        public override string ToString()
        {
            string str = string.Format("pos:{0} angle:{1} scale:{2} {3}", LocalPosition, EulerAngles, LocalScale, Forward);
            return str;
        }
    }
}

