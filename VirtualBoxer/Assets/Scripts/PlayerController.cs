using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public int health = 1000;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //transform.position = Camera.main.transform.position;
	}

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.root.tag == "Enemy")
        {
            health -= 50 + (int)(Random.value * 50);
            if(health < 0)
            {
                //End game, lose
            }
        }
    }
}
