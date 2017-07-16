using UnityEngine;

namespace MovingSofaProblem
{
    public class BreakFall : MonoBehaviour
    {
        void OnCollisionEnter(Collision col)
        {
            var rigidBody = gameObject.GetComponent<Rigidbody>();
            if(rigidBody != null)
            {
                Destroy(rigidBody);
            }
            Destroy(this);
        }
    }
}
