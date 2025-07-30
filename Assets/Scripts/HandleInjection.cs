using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandleInjection : MonoBehaviour
{
    //public float injectSpeed = 0.1f;
    public float force = 0.1f;
    public Vector3 injectDistance;
    public Vector3 injectTargectPosition;

    public bool isInjecting = false;
    public bool isInjectingFinished = false;
    bool isFirst = true;
   


    void Update()
    {
        if(Input.GetKeyDown(KeyCode.J))
        {
            isInjecting = true;
        }
        Inject();
    }

    void Inject()
    {
        if (!isInjecting)
        {
            return;
        }

        if (isInjectingFinished)
        {
            return;
        }


        if (isFirst)
        {
            injectTargectPosition = transform.position + injectDistance;
            isFirst = false;
        }

        //Debug.Log("Injecting");

        // 计算当前位置与目标位置之间的方向
        Vector3 direction = injectTargectPosition - transform.position;

        // 如果当前位置与目标位置之间的差距小于一个阈值，认为已经到达目标
        if (direction.magnitude < 0.01f)
        {
            transform.position = injectTargectPosition; // 确保直接到达目标
            isInjecting = false;
            isInjectingFinished = true;
            return;
        }

        //// 根据injectSpeed控制速度，逐渐向目标位置移动
        //Vector3 move = direction.normalized * injectSpeed * Time.deltaTime;

        //// 更新物体位置
        //transform.position += move;

        //从直接改变位置变成施加作用力
        Rigidbody rb = this.GetComponent<Rigidbody>();
        //rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        rb.AddForce(Vector3.right * force, ForceMode.Force);

    }
}
