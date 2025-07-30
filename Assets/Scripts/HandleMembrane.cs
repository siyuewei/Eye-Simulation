using Obi;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEngine;

public class HandleMembrane : MonoBehaviour
{
    // 基本参数
    public ObiActor obiActor;
    public ObiSolver obiSolver;

    //卷曲参数
    public float curlSpeed = 0.01f;
    public bool isCurling = false;
    public bool isCurlingFinished = false;

    //卷曲初始化参数
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

    //attach参数
    public GameObject curlAttachs;
    public int groupSize;
    public Dictionary<float, List<Vector3>> groups;
    public List<GameObject> attachObjects;
    public List<Vector3> attachTargetPosition;

    //移动参数
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
        

        // 遍历所有点，找到最小和最大坐标
        foreach (int index in allIndices)
        {
            Vector3 position = obiActor.GetParticlePosition(index);
            minPoint = Vector3.Min(minPoint, position);
            maxPoint = Vector3.Max(maxPoint, position);
        }

        midY = (minPoint.y + maxPoint.y) / 2;
        originZ = minPoint.z; //这里假设membrane是平行于XOY平面的

        // 计算长方形的长边和短边长度
        size = maxPoint - minPoint;
        length = size.x;  // 假设长边沿X轴
        width = size.y;   // 假设短边沿Y轴

        // 如果需要动态判断长边和短边的方向，比较大小
        if (length < width)
        {
            // 交换长边和短边
            float temp = length;
            length = width;
            width = temp;

            // 交换坐标轴方向
            Vector3 tempDirection = size;
            size.x = size.y;
            size.y = tempDirection.x;
        }


        // 按y值分组，考虑误差范围
        groups = new Dictionary<float, List<Vector3>>();
        float tolerance = 0.001f; // 误差容忍范围，允许一定的y误差

        foreach (int index in allIndices)
        {
            Vector3 position = obiActor.GetParticlePosition(index);
            bool foundGroup = false;

            // 查找是否有与当前粒子y值相近的组
            foreach (var group in groups)
            {
                if (Mathf.Abs(group.Key - position.y) < tolerance)
                {
                    group.Value.Add(position);
                    foundGroup = true;
                    break;
                }
            }

            // 如果找不到相近的组，就创建一个新的组
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

        // 为每个group创建一个新的空物体并绑定
        int groupIndex = 0;
        ObiActorBlueprint blueprint = this.gameObject.GetComponent<ObiSoftbody>().blueprint;

        foreach (var group in groups)
        {
            groupIndex++;
            List<Vector3> particles = group.Value;

            // 计算每组粒子中，x坐标最接近中间位置的粒子
            float midX = (minPoint.x + maxPoint.x) / 2;  // 计算x轴中间位置
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

            // 创建一个新的空物体并设置其位置
            GameObject groupObject = new GameObject("Group_" + groupIndex);
            attachObjects.Add(groupObject);
            groupObject.transform.position = new Vector3(this.transform.position.x, this.transform.position.y + closestParticle.y, this.transform.position.z);
            //groupObject.AddComponent<BoxCollider>();
            //groupObject.AddComponent<ObiCollider>();

            // 绑定该group的粒子
            groupObject.transform.parent = curlAttachs.transform;
            ObiParticleAttachment attachment = this.gameObject.AddComponent<ObiParticleAttachment>();
            attachment.particleGroup = blueprint.groups[groupSize-groupIndex];
            attachment.target = groupObject.transform;
            //attachment.attachmentType = ObiParticleAttachment.AttachmentType.Dynamic;
        }

        // 计算卷曲后圆柱的半径
        radius = width / (2 * Mathf.PI);

        //计算卷曲后每个attach物体的位置
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

        //// 计算每个点卷曲后的位置
        //for (int i = 0; i < allIndices.count; i++)
        //{
        //    int particleIndex = allIndices[i];
        //    Vector3 particlePosition = obiActor.GetParticlePosition(particleIndex);
        //    //Debug.Log("Particle " + particleIndex + " position: " + particlePosition);

        //    //计算卷曲后该点应该在的位置
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

        // 对每个attach物体施加作用力
        for (int i = 0; i < attachObjects.Count; ++i)
        {
            GameObject attachObject = attachObjects[i];
            Vector3 currentPosition = attachObject.transform.position;
            Vector3 targetPosition = attachTargetPosition[i];

            // 计算当前位置与目标位置之间的差距
            Vector3 direction = targetPosition - currentPosition;

            // 判断物体是否接近目标位置
            if (direction.magnitude < 0.01f)
            {
                // 如果接近目标位置，则跳过施加力，认为该物体已完成卷曲
                attachObject.transform.position = targetPosition;  // 确保物体到达目标位置
                continue;
            }

            // 如果还没有到达目标位置，则施加力
            isFinished = false;

            // 根据卷曲速度计算施加的力
            Vector3 force = direction.normalized * curlSpeed;

            // 施加力使物体向目标位置移动
            attachObject.transform.position += force * Time.deltaTime; // 按时间增量更新位置
        }

        // 如果所有物体都接近目标位置，表示卷曲完成
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

    //        //施加作用力
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
        //移动attach物体
        for (int i = 0; i < attachObjects.Count; ++i)
        {
            GameObject attachObject = attachObjects[i];
            Vector3 currentPosition = attachObject.transform.position;
            Vector3 targetPosition = moveTargetPositions[i];

            // 计算当前位置与目标位置之间的差距
            Vector3 direction = targetPosition - currentPosition;

            // 判断物体是否接近目标位置
            if (direction.magnitude < 0.01f)
            {
                // 如果接近目标位置，则跳过施加力，认为该物体已完成移动
                attachObject.transform.position = targetPosition;  // 确保物体到达目标位置
                continue;
            }
            else { isFinished = false; }

            // 如果还没有到达目标位置，则施加力
            // 根据移动速度计算施加的力
            Vector3 force = direction.normalized * moveSpeed;

            // 施加力使物体向目标位置移动
            attachObject.transform.position += force * Time.deltaTime; // 按时间增量更新位置
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
