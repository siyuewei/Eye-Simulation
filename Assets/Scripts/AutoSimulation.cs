using Obi;
using System.Collections.Generic;
using UnityEngine;

public class AutoSimulation : MonoBehaviour
{
    public ObiActor obiActor; // ����Ĥ��ObiActor��������������
    public Vector3 holePosition; // ����λ��
    public float holeAboveOffset; // �ƶ������Ϸ�ʱ��ƫ���������Զ��壩
    public float holeBelowOffset; // �����ƶ�������ʱ��ƫ���������Զ��壩

    // ���ڴ洢�ĸ��������ƶ���ƫ������ÿ��Ԫ�ض�Ӧһ���ǵ�ƫ����
    public List<Vector3> holeOutsideOffsets = new List<Vector3>()
    {
        new Vector3(1f, 0f, 0f), // ���Ͻ������ƶ�ƫ����ʾ�����ɸ�����Ҫ�޸�
        new Vector3(-1f, 0f, 0f), // ���Ͻ������ƶ�ƫ����ʾ�����ɸ�����Ҫ�޸�
        new Vector3(1f, 0f, 0f), // ���½������ƶ�ƫ����ʾ�����ɸ�����Ҫ�޸�
        new Vector3(-1f, 0f, 0f) // ���½������ƶ�ƫ����ʾ�����ɸ�����Ҫ�޸�
    };

    // Ԥ�ȶ�����ĸ������飬�ֱ��ӦĤ���ĸ��ǣ�����������Ѿ���Unity�༭�������ú����ǵ�id
    public int topLeftCornerGroupId;
    public int topRightCornerGroupId;
    public int bottomLeftCornerGroupId;
    public int bottomRightCornerGroupId;

    private bool isSimulating = false; // ����Ƿ����ڽ��з������
    private bool hasSimulationStarted = false; // ����Ƿ��Ѿ���ʼ������
    private int currentCornerIndex = 0; // ���ڼ�¼��ǰ���ڴ���Ľǵ�����

    void Update()
    {
        // ����Ƿ���F������ʼ����
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!hasSimulationStarted)
            {
                hasSimulationStarted = true;
            }

            isSimulating = true;
            currentCornerIndex = 0;
        }

        // ������ڷ��棬ÿһ֡������StartInsertingMembrane����
        if (isSimulating)
        {
            //StartInsertingMembrane();
        }
    }

    void StartInsertingMembrane()
    {
        // ��ȡԤ�ȶ���õ��������б�
        List<ObiParticleGroup> groups = obiActor.blueprint.groups;

        // ���ݵ�ǰ�ǵ����������Ӧ�Ľ�
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
                    // ���нǶ�������󣬽�������
                    isSimulating = false;
                }
                break;
            default:
                break;
        }
    }

    void MoveCornerParticlesIntoHole(ObiParticleGroup particleGroup)
    {
        // �ƶ������Ϸ�
        Vector3 targetAboveHole = holePosition + Vector3.forward * holeAboveOffset;
        bool aboveMovementComplete = MoveParticlesToTargetPositionByForce(particleGroup, targetAboveHole);

        // ȷ���ƶ������Ϸ��Ĳ�����ɺ󣬲������ƶ�������
        if (aboveMovementComplete)
        {
            // �����ƶ�������
            Vector3 targetBelowHole = holePosition - Vector3.forward * holeBelowOffset;
            bool belowMovementComplete = MoveParticlesToTargetPositionByForce(particleGroup, targetBelowHole);

            // ȷ�������ƶ��������Ĳ�����ɺ󣬲������ƶ�
            if (belowMovementComplete)
            {
                // �����ƶ������ݵ�ǰ�ǵ�������ȡ��Ӧ�������ƶ�ƫ������
                Vector3 targetOutsideHole = targetBelowHole + holeOutsideOffsets[currentCornerIndex];
                MoveParticlesToTargetPositionByForce(particleGroup, targetOutsideHole);
            }
        }
    }

    bool MoveParticlesToTargetPositionByForce(ObiParticleGroup particleGroup, Vector3 targetPosition)
    {
        float forceStrength = 5.0f; // ʩ������ǿ�ȣ��ɸ�����Ҫ����
        float tolerance = 0.01f; // �ɽ��ܵ���Χ�������ж������Ƿ񵽴�Ŀ��λ��
        bool allParticlesReached = false;

        while (!allParticlesReached)
        {
            allParticlesReached = true;

            foreach (int index in particleGroup.particleIndices)
            {
                Vector3 currentParticlePosition = obiActor.GetParticlePosition(index);
                Vector3 directionToTarget = targetPosition - currentParticlePosition;

                // ����ʩ�ӵ���������Ϊָ��Ŀ��λ�ã���СΪ�趨����ǿ��
                Vector3 force = directionToTarget.normalized * forceStrength;

                // ����Ӧ�õ�����
                Vector3 currentForce = obiActor.solver.externalForces[index];
                obiActor.solver.externalForces[index] = currentForce + force;

                // ��������λ�ã��������obiActor��ʩ����������Զ���������λ�ã�ʵ�ʿ�����Ҫ����Obi�Ļ��Ƶ�����ظ��º�����
                // ���磺obiActor.UpdatePhysics();�����庯���������Obi��ʵ�������

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
        float tolerance = 0.01f; // �ɽ��ܵ���Χ�������ж������Ƿ񵽴�Ŀ��λ��

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
        // ���ݵ�ǰ���ڴ���Ĳ���ȷ�����ӵ�Ŀ��λ��
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