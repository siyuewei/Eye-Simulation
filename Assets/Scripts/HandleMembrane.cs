using Obi;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class HandleMembrane : MonoBehaviour
{
    // ��������
    public ObiActor obiActor;
    public ObiSolver obiSolver;

    //��������
    public float curlSpeed = 0.01f;
    public bool isCurling = false;
    public bool isCurlingFinished = false;

    //������ʼ������
    Vector3 minPoint = Vector3.positiveInfinity;
    Vector3 maxPoint = Vector3.negativeInfinity;

    float midY;
    float originZ;
    Vector3 size;
    float length;
    float width;
    float radius;
    List<Vector3> newPositions = new List<Vector3>();

    bool isCurlInitial = false;

    //attach����
    public GameObject curlAttachs;
    public int groupSize;
    public Dictionary<float, List<Vector3>> groups;
    public List<GameObject> attachObjects;
    public List<Vector3> attachTargetPosition;

    //�ƶ�����
    public float moveSpeed = 0.01f;
    public bool isMoving = false;
    public bool isMovingFinished = false;
    public Vector3 moveTargetPosition;
    bool isMovingInitial = false;
    public List<Vector3> moveTargetPositions;

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C)  )
        {
            isCurling = true;
        }
        Curl();

        if (Input.GetKeyDown(KeyCode.M)  && isCurlingFinished)
        {
            isMoving = true;
        }
        Move();

        if(Input.GetKeyDown(KeyCode.J) && isCurlingFinished && isMovingFinished)
        {
            DetachAttach();
        }
    }

    void CurlInitial()
    {
        //Debug.Log("Initial membrane");


        obiSolver = obiActor.solver;

        var allIndices = obiActor.solverIndices;
        

        // �������е㣬�ҵ���С���������
        foreach (int index in allIndices)
        {
            Vector3 position = obiActor.GetParticlePosition(index);
            minPoint = Vector3.Min(minPoint, position);
            maxPoint = Vector3.Max(maxPoint, position);
        }

        midY = (minPoint.y + maxPoint.y) / 2;
        originZ = minPoint.z; //�������membrane��ƽ����XOYƽ���

        // ���㳤���εĳ��ߺͶ̱߳���
        size = maxPoint - minPoint;
        length = size.x;  // ���賤����X��
        width = size.y;   // ����̱���Y��

        // �����Ҫ��̬�жϳ��ߺͶ̱ߵķ��򣬱Ƚϴ�С
        if (length < width)
        {
            // �������ߺͶ̱�
            float temp = length;
            length = width;
            width = temp;

            // ���������᷽��
            Vector3 tempDirection = size;
            size.x = size.y;
            size.y = tempDirection.x;
        }


        // ��yֵ���飬������Χ
        groups = new Dictionary<float, List<Vector3>>();
        float tolerance = 0.001f; // ������̷�Χ������һ����y���

        foreach (int index in allIndices)
        {
            Vector3 position = obiActor.GetParticlePosition(index);
            bool foundGroup = false;

            // �����Ƿ����뵱ǰ����yֵ�������
            foreach (var group in groups)
            {
                if (Mathf.Abs(group.Key - position.y) < tolerance)
                {
                    group.Value.Add(position);
                    foundGroup = true;
                    break;
                }
            }

            // ����Ҳ���������飬�ʹ���һ���µ���
            if (!foundGroup)
            {
                groups[position.y] = new List<Vector3> { position };
            }
        }
        if(groupSize != groups.Count)
        {
            Debug.LogError("Group size is not correct, change the tolerance");
            return;
        }

        // Ϊÿ��group����һ���µĿ����岢��
        int groupIndex = 0;
        ObiActorBlueprint blueprint = this.gameObject.GetComponent<ObiSoftbody>().blueprint;

        foreach (var group in groups)
        {
            groupIndex++;
            List<Vector3> particles = group.Value;

            // ����ÿ�������У�x������ӽ��м�λ�õ�����
            float midX = (minPoint.x + maxPoint.x) / 2;  // ����x���м�λ��
            Vector3 closestParticle = particles[0];
            float minDistance = Mathf.Abs(particles[0].x - midX);

            foreach (var particle in particles)
            {
                float distance = Mathf.Abs(particle.x - midX);
                if (distance < minDistance)
                {
                    closestParticle = particle;
                    minDistance = distance;
                }
            }

            // ����һ���µĿ����岢������λ��
            GameObject groupObject = new GameObject("Group_" + groupIndex);
            attachObjects.Add(groupObject);
            groupObject.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + closestParticle.y, this.transform.position.z);
            //groupObject.AddComponent<BoxCollider>();
            //groupObject.AddComponent<ObiCollider>();

            // �󶨸�group������
            groupObject.transform.parent = curlAttachs.transform;
            ObiParticleAttachment attachment = this.gameObject.AddComponent<ObiParticleAttachment>();
            attachment.particleGroup = blueprint.groups[groupSize-groupIndex];
            attachment.target = groupObject.transform;
            //attachment.attachmentType = ObiParticleAttachment.AttachmentType.Dynamic;
        }

        // ���������Բ���İ뾶
        radius = width / (2 * Mathf.PI);

        //���������ÿ��attach�����λ��
        for(int i = 0; i < attachObjects.Count; i++)
        {
            GameObject attachObject = attachObjects[i];
            Vector3 position = attachObject.transform.position;

            float distance = midY - position.y;
            float theta = distance / radius;
            float newY = midY - radius * Mathf.Sin(theta);
            float newZ = originZ + radius * (1 - Mathf.Cos(theta));

            attachTargetPosition.Add(new Vector3(position.x, newY, newZ));
        }

        //// ����ÿ����������λ��
        //for (int i = 0; i < allIndices.count; i++)
        //{
        //    int particleIndex = allIndices[i];
        //    Vector3 particlePosition = obiActor.GetParticlePosition(particleIndex);
        //    //Debug.Log("Particle " + particleIndex + " position: " + particlePosition);

        //    //���������õ�Ӧ���ڵ�λ��
        //    float distance = midY - particlePosition.y;
        //    float theta = distance / radius;
        //    float newY = midY - radius * Mathf.Sin(theta);
        //    float newZ = originZ + radius * (1 - Mathf.Cos(theta));

        //    newPositions.Add(new Vector3(particlePosition.x, newY, newZ));
        //}


        ////Debug
        //for (int i = 0; i < allIndices.count; i++)
        //{
        //    Debug.Log("Particle " + allIndices[i] + " position: " + obiActor.GetParticlePosition(allIndices[i]).ToString("f4") + " new position: " + newPositions[i]);
        //}
    }

    void Curl()
    {
        if (!isCurling)
        {
            return;
        }

        if(!isCurlInitial)
        {
            CurlInitial();
            isCurlInitial = true;
        }

        bool isFinished = true;

        // ��ÿ��attach����ʩ��������
        for (int i = 0; i < attachObjects.Count; ++i)
        {
            GameObject attachObject = attachObjects[i];
            Vector3 currentPosition = attachObject.transform.position;
            Vector3 targetPosition = attachTargetPosition[i];

            // ���㵱ǰλ����Ŀ��λ��֮��Ĳ��
            Vector3 direction = targetPosition - currentPosition;

            // �ж������Ƿ�ӽ�Ŀ��λ��
            if (direction.magnitude < 0.01f)
            {
                // ����ӽ�Ŀ��λ�ã�������ʩ��������Ϊ����������ɾ���
                attachObject.transform.position = targetPosition;  // ȷ�����嵽��Ŀ��λ��
                continue;
            }

            // �����û�е���Ŀ��λ�ã���ʩ����
            isFinished = false;

            // ���ݾ����ٶȼ���ʩ�ӵ���
            Vector3 force = direction.normalized * curlSpeed;

            // ʩ����ʹ������Ŀ��λ���ƶ�
            attachObject.transform.position += force * Time.deltaTime; // ��ʱ����������λ��
        }

        // ����������嶼�ӽ�Ŀ��λ�ã���ʾ�������
        if (isFinished)
        {
            isCurling = false;
            isCurlingFinished = true;
        }
    }


    //void Curl()
    //{
    //    if(isCurling == false)
    //    {
    //        return;
    //    }
    //    //Debug.Log("Curling membrane");

    //    var allIndices = obiActor.solverIndices;

    //    bool isCurlingFinished = true;
    //    for (int i = 0; i < allIndices.count; i++)
    //    {
    //        int particleIndex = allIndices[i];

    //        //ʩ��������
    //        Vector3 targetPosition = newPositions[i];
    //        Vector3 direction = (targetPosition - obiActor.GetParticlePosition(particleIndex));

    //        if (direction.magnitude < 0.01f)
    //        {
    //            continue;
    //        }
    //        else
    //        {
    //            isCurlingFinished = false;
    //        }

    //        Vector3 force = direction.normalized * curlSpeed;
    //        obiSolver.externalForces[particleIndex] = force;

    //        Debug.Log("Particle " + particleIndex + " position: " + obiActor.GetParticlePosition(particleIndex) + " target position: " + targetPosition + " force: " + force);

    //    }

    //    if (isCurlingFinished)
    //    {
    //        isCurling = false;
    //    }
    //}


    void MoveInitial()
    {
        Vector3 moveDirection = moveTargetPosition - this.transform.position;
        for(int i = 0; i < attachObjects.Count; i++)
        {
            moveTargetPositions.Add(attachTargetPosition[i] + moveDirection);
        }
    }

    void Move()
    {
        if (isMoving == false)
        {
            return;
        }

        if (!isMovingInitial)
        {
            MoveInitial();
            isMovingInitial = true;
        }
        bool isFinished = true;
        //�ƶ�attach����
        for (int i = 0; i < attachObjects.Count; ++i)
        {
            GameObject attachObject = attachObjects[i];
            Vector3 currentPosition = attachObject.transform.position;
            Vector3 targetPosition = moveTargetPositions[i];

            // ���㵱ǰλ����Ŀ��λ��֮��Ĳ��
            Vector3 direction = targetPosition - currentPosition;

            // �ж������Ƿ�ӽ�Ŀ��λ��
            if (direction.magnitude < 0.01f)
            {
                // ����ӽ�Ŀ��λ�ã�������ʩ��������Ϊ������������ƶ�
                attachObject.transform.position = targetPosition;  // ȷ�����嵽��Ŀ��λ��
                continue;
            }
            else { isFinished = false; }

            // �����û�е���Ŀ��λ�ã���ʩ����
            // �����ƶ��ٶȼ���ʩ�ӵ���
            Vector3 force = direction.normalized * moveSpeed;

            // ʩ����ʹ������Ŀ��λ���ƶ�
            attachObject.transform.position += force * Time.deltaTime; // ��ʱ����������λ��
        }

        if(isFinished)
        {
            isMoving = false;
            isMovingFinished = true;

            //DetachAttach();
        }
    }

    void DetachAttach()
    {
        ObiParticleAttachment[] attachments = this.gameObject.GetComponents<ObiParticleAttachment>();
        for(int i = 0; i < attachments.Length; i++)
        {
            attachments[i].enabled = false;
        }

    }
}
