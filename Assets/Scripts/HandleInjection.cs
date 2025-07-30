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

        // ���㵱ǰλ����Ŀ��λ��֮��ķ���
        Vector3 direction = injectTargectPosition - transform.position;

        // �����ǰλ����Ŀ��λ��֮��Ĳ��С��һ����ֵ����Ϊ�Ѿ�����Ŀ��
        if (direction.magnitude < 0.01f)
        {
            transform.position = injectTargectPosition; // ȷ��ֱ�ӵ���Ŀ��
            isInjecting = false;
            isInjectingFinished = true;
            return;
        }

        //// ����injectSpeed�����ٶȣ�����Ŀ��λ���ƶ�
        //Vector3 move = direction.normalized * injectSpeed * Time.deltaTime;

        //// ��������λ��
        //transform.position += move;

        //��ֱ�Ӹı�λ�ñ��ʩ��������
        Rigidbody rb = this.GetComponent<Rigidbody>();
        //rb.AddForce(direction.normalized * force, ForceMode.Impulse);
        rb.AddForce(Vector3.right * force, ForceMode.Force);

    }
}
