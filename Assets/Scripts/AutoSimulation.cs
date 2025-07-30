using Obi;
using System.Collections.Generic;
using UnityEngine;

public class AutoSimulation : MonoBehaviour
{
    public ObiActor obiActor; // 代表膜的ObiActor，包含若干粒子
    public Vector3 holePosition; // 洞的位置
    public float holeAboveOffset; // 移动到洞上方时的偏移量（可自定义）
    public float holeBelowOffset; // 往下移动穿过洞时的偏移量（可自定义）

    // 用于存储四个角向外移动的偏移量，每个元素对应一个角的偏移量
    public List<Vector3> holeOutsideOffsets = new List<Vector3>()
    {
        new Vector3(1f, 0f, 0f), // 左上角向外移动偏移量示例，可根据需要修改
        new Vector3(-1f, 0f, 0f), // 右上角向外移动偏移量示例，可根据需要修改
        new Vector3(1f, 0f, 0f), // 左下角向外移动偏移量示例，可根据需要修改
        new Vector3(-1f, 0f, 0f) // 右下角向外移动偏移量示例，可根据需要修改
    };

    // 预先定义的四个粒子组，分别对应膜的四个角，这里假设你已经在Unity编辑器中设置好它们的id
    public int topLeftCornerGroupId;
    public int topRightCornerGroupId;
    public int bottomLeftCornerGroupId;
    public int bottomRightCornerGroupId;

    private bool isSimulating = false; // 标记是否正在进行仿真操作
    private bool hasSimulationStarted = false; // 标记是否已经开始过仿真
    private int currentCornerIndex = 0; // 用于记录当前正在处理的角的索引

    void Update()
    {
        // 检测是否按下F键来开始仿真
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!hasSimulationStarted)
            {
                hasSimulationStarted = true;
            }

            isSimulating = true;
            currentCornerIndex = 0;
        }

        // 如果正在仿真，每一帧都调用StartInsertingMembrane函数
        if (isSimulating)
        {
            //StartInsertingMembrane();
        }
    }

    void StartInsertingMembrane()
    {
        // 获取预先定义好的粒子组列表
        List<ObiParticleGroup> groups = obiActor.blueprint.groups;

        // 根据当前角的索引处理对应的角
        switch (currentCornerIndex)
        {
            case 0:
                MoveCornerParticlesIntoHole(groups[topLeftCornerGroupId]);
                if (IsCornerMovementComplete(groups[topLeftCornerGroupId]))
                {
                    currentCornerIndex++;
                }
                break;
            case 1:
                MoveCornerParticlesIntoHole(groups[topRightCornerGroupId]);
                if (IsCornerMovementComplete(groups[topRightCornerGroupId]))
                {
                    currentCornerIndex++;
                }
                break;
            case 2:
                MoveCornerParticlesIntoHole(groups[bottomLeftCornerGroupId]);
                if (IsCornerMovementComplete(groups[bottomLeftCornerGroupId]))
                {
                    currentCornerIndex++;
                }
                break;
            case 3:
                MoveCornerParticlesIntoHole(groups[bottomRightCornerGroupId]);
                if (IsCornerMovementComplete(groups[bottomRightCornerGroupId]))
                {
                    // 所有角都处理完后，结束仿真
                    isSimulating = false;
                }
                break;
            default:
                break;
        }
    }

    void MoveCornerParticlesIntoHole(ObiParticleGroup particleGroup)
    {
        // 移动到洞上方
        Vector3 targetAboveHole = holePosition + Vector3.forward * holeAboveOffset;
        bool aboveMovementComplete = MoveParticlesToTargetPositionByForce(particleGroup, targetAboveHole);

        // 确保移动到洞上方的操作完成后，才往下移动穿过洞
        if (aboveMovementComplete)
        {
            // 往下移动穿过洞
            Vector3 targetBelowHole = holePosition - Vector3.forward * holeBelowOffset;
            bool belowMovementComplete = MoveParticlesToTargetPositionByForce(particleGroup, targetBelowHole);

            // 确保往下移动穿过洞的操作完成后，才向外移动
            if (belowMovementComplete)
            {
                // 向外移动（根据当前角的索引获取对应的向外移动偏移量）
                Vector3 targetOutsideHole = targetBelowHole + holeOutsideOffsets[currentCornerIndex];
                MoveParticlesToTargetPositionByForce(particleGroup, targetOutsideHole);
            }
        }
    }

    bool MoveParticlesToTargetPositionByForce(ObiParticleGroup particleGroup, Vector3 targetPosition)
    {
        float forceStrength = 5.0f; // 施加力的强度，可根据需要调整
        float tolerance = 0.01f; // 可接受的误差范围，用于判断粒子是否到达目标位置
        bool allParticlesReached = false;

        while (!allParticlesReached)
        {
            allParticlesReached = true;

            foreach (int index in particleGroup.particleIndices)
            {
                Vector3 currentParticlePosition = obiActor.GetParticlePosition(index);
                Vector3 directionToTarget = targetPosition - currentParticlePosition;

                // 计算施加的力，方向为指向目标位置，大小为设定的力强度
                Vector3 force = directionToTarget.normalized * forceStrength;

                // 将力应用到粒子
                Vector3 currentForce = obiActor.solver.externalForces[index];
                obiActor.solver.externalForces[index] = currentForce + force;

                // 更新粒子位置（这里假设obiActor在施加外力后会自动更新粒子位置，实际可能需要根据Obi的机制调用相关更新函数）
                // 例如：obiActor.UpdatePhysics();（具体函数名需根据Obi的实际情况）

                if (Vector3.Distance(currentParticlePosition, targetPosition) > tolerance)
                {
                    allParticlesReached = false;
                }
            }
        }

        return allParticlesReached;
    }

    bool IsCornerMovementComplete(ObiParticleGroup particleGroup)
    {
        float tolerance = 0.01f; // 可接受的误差范围，用于判断粒子是否到达目标位置

        foreach (int index in particleGroup.particleIndices)
        {
            Vector3 currentParticlePosition = obiActor.GetParticlePosition(index);
            Vector3 targetPosition = GetCurrentTargetPosition(index);

            if (Vector3.Distance(currentParticlePosition, targetPosition) > tolerance)
            {
                return false;
            }
        }

        return true;
    }

    Vector3 GetCurrentTargetPosition(int index)
    {
        // 根据当前正在处理的步骤确定粒子的目标位置
        switch (currentCornerIndex)
        {
            case 0:
                return holePosition + Vector3.forward * holeAboveOffset;
            case 1:
                return holePosition - Vector3.forward * holeBelowOffset;
            case 2:
                return holePosition - Vector3.forward * holeBelowOffset + holeOutsideOffsets[2];
            case 3:
                return holePosition - Vector3.forward * holeBelowOffset + holeOutsideOffsets[3];
            default:
                return Vector3.zero;
        }
    }
}