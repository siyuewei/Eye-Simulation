using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleNeedle : MonoBehaviour
{
    public bool isNeedleIn = false;
    public float needleSpeed = 0.1f;
    public Vector3 targetPosition;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space) && !isNeedleIn)
        {
            isNeedleIn = true;
        }

        if(isNeedleIn)
        {
            this.transform.position = Vector3.MoveTowards(this.transform.position, targetPosition, needleSpeed);
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Test();
        }
    }


    void Test()
    {
        //给物体施加一个x轴正方向的冲量
        Debug.Log("Test");
        Rigidbody rb = this.GetComponent<Rigidbody>();
        float force = 0.1f;
        rb.AddForce(Vector3.right * force, ForceMode.Impulse);

    }
}
