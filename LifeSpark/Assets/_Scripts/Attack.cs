using UnityEngine;
using System.Collections;

public class Attack : MonoBehaviour {

    float attackRange;
    float attackRadius;
    float attackDuration;
    float remainingDuration;
    float speed;
    Player source;
    Vector3 target;

    public enum AttackType {
        Line,
        TargetArea,
        SelfArea
    };

    public AttackType attackType;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void Attack() {

    }
}
