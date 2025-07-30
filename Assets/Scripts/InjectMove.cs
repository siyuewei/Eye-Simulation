using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InjectMove : MonoBehaviour
{
    public GameObject membraneAttach;
    public GameObject syringe;
    public GameObject injectionTubing;

    public Vector3 moveDistance;
    public float moveSpeed = 0.1f;
    public bool isMoving = false;
    public bool isMovingFinished = false;

    private Vector3 membraneAttachTarget;
    private Vector3 syringeTarget;
    private Vector3 injectionTubingTarget;

    bool isFirst = true;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.N) && !isMoving)
        {
            isMoving = true;
        }
        Move();
    }

    void Move()
    {
        if (!isMoving)
        {
            return;
        }

        if (isMovingFinished)
        {
            return;
        }

        if (isFirst)
        {
            membraneAttachTarget = membraneAttach.transform.position + moveDistance;
            syringeTarget = syringe.transform.position + moveDistance;
            injectionTubingTarget = injectionTubing.transform.position + moveDistance;
            isFirst = false;
        }

        membraneAttach.transform.position = Vector3.MoveTowards(membraneAttach.transform.position, membraneAttachTarget, moveSpeed);
        syringe.transform.position = Vector3.MoveTowards(syringe.transform.position, syringeTarget, moveSpeed);
        injectionTubing.transform.position = Vector3.MoveTowards(injectionTubing.transform.position, injectionTubingTarget, moveSpeed);
    
        if(membraneAttach.transform.position == membraneAttachTarget && syringe.transform.position == syringeTarget && injectionTubing.transform.position == injectionTubingTarget)
        {
            isMoving = false;
            isMovingFinished = true;
        }
    
    }
}
