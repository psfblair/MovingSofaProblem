using System;
using UnityEngine;

namespace MovingSofaProblem
{
    public class BreakFall : MonoBehaviour
    {
        public delegate void MeasureHasLandedHandler(object sender, EventArgs e);
        public event MeasureHasLandedHandler MeasureHasLanded;

        void OnCollisionEnter(Collision col)
        {
            var rigidBody = gameObject.GetComponent<Rigidbody>();
            if(rigidBody != null)
            {
                Destroy(rigidBody);
            }
            MeasureHasLanded(this, EventArgs.Empty);
            MeasureHasLanded = null;
            Destroy(this);
        }
    }
}
