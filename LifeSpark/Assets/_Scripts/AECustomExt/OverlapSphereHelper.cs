using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace AGE {

    public class OverlapSphereHelper : MonoBehaviour {

        public List<GameObject> GetCollisionSet() {

            Collider[] colliders = Physics.OverlapSphere(transform.position, 24);

            List<GameObject> result = new List<GameObject>();
            foreach (Collider col in colliders) {
                if (col != gameObject.collider) // add condition check for team here?
                    result.Add(col.gameObject);
            }
            return result;
        }

        //Dictionary<GameObject, float> collisionSet = new Dictionary<GameObject, float>();
    }
}
