using UnityEngine;
using System.Collections;

public class AnimTest : MonoBehaviour
{
    Animator animator;

	// Use this for initialization
	void Start () 
    {
        animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update () 
    {       
        animator.SetBool("IsWalking", Input.GetKey(KeyCode.W));
	}
}
