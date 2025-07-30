using Obi;
using System.Collections.Generic;
using UnityEngine;

public class HandleMembraneDrag : MonoBehaviour
{
    public ObiActor obiActor;                       // ObiActor, �洢membrane������
    public float dragRadius = 1.0f;                 // �϶��뾶�����Ƹ������ӵķ�Χ
    public float dragStrength = 1.0f;               // �϶�����
    public float forceStrength = 5.0f;              // ʩ�ӵ����ӵĶ�������ǿ�ȣ��Ҽ�ʩ�ӵ�����
    public LayerMask membraneLayer;                 // �������߼��Ĳ㣬ȷ������ֻ��membrane�ཻ
    public bool isDragging = false;                 // �Ƿ������϶�membrane
    private List<int> draggedParticles = new List<int>(); // ���϶�������
    private Vector3 lastMousePosition;              // ��һ�����λ��
    private Vector3 seleceCenter;                   // ѡ�е����ĵ�

    private Vector3 initialDragPoint;               // ��ʼ�϶��㣨��갴��ʱ�����е㣩
    private Vector3 initialMousePosition;           // ��갴��ʱ��λ�ã���������ƫ����
    private Plane dragPlane;                        // �������߼����϶�ƽ��

    void Update()
    {
        // �������������º��ɿ�
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopDrag();
        }

        // ��������Ҽ����²�ʩ����ƽ��ǰ����
        if (Input.GetMouseButtonDown(1))
        {
            ApplyForceToSelectedParticles();
        }

        // ��������ƶ�ʱ�϶�����
        if (isDragging)
        {
            DragMembrane();
        }
    }

    // ��ʼ�϶�
    void StartDrag()
    {
        // ���߼�⣬�������ߴ���Ļ�ռ䵽����ռ�
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, membraneLayer))
        {
            // �������membrane����ȡ���е�
            Vector3 hitPoint = hit.point;

            // �ҵ����е���Χ�����ӣ����϶��뾶�ڵ����ӣ�
            draggedParticles.Clear();  // ���֮ǰ���϶�����

            var allIndices = obiActor.solverIndices;
            foreach (int index in allIndices)
            {
                Vector3 particlePosition = obiActor.GetParticlePosition(index);

                // �������������е�ľ���
                if (Vector3.Distance(particlePosition, hitPoint) < dragRadius)
                {
                    draggedParticles.Add(index);  // ��ӵ����϶��������б�
                }
            }

            // ��ʼ���϶�ƽ�棨ƽ�������������һ��ƽ�棬���Ϊ hitPoint������Ϊ������ĳ���
            dragPlane = new Plane(Camera.main.transform.forward, hitPoint);

            isDragging = true;  // ��ʼ�϶�
            initialDragPoint = hitPoint;  // ��¼��ʼ���е�
            initialMousePosition = Input.mousePosition;  // ��¼��갴��ʱ��λ��
        }
    }

    // ֹͣ�϶�
    void StopDrag()
    {
        isDragging = false;
        draggedParticles.Clear();  // ����϶������б�
    }

    // �϶�membrane������ƶ�ʱ��
    void DragMembrane()
    {
        // ��������ƽ������
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distanceToPlane;

        if (dragPlane.Raycast(ray, out distanceToPlane))
        {
            // ���������ƽ��Ľ���
            Vector3 mousePositionOnPlane = ray.GetPoint(distanceToPlane);
            Vector3 offset = mousePositionOnPlane - initialDragPoint;  // ��������ƶ���ƫ����

            // ��ÿ�����϶�������ʩ������
            foreach (int index in draggedParticles)
            {
                Vector3 particlePosition = obiActor.GetParticlePosition(index);
                Vector3 direction = offset;  // ƫ������Ϊ�϶��ķ���

                // ʩ���϶���
                Vector3 dragForce = direction.normalized * dragStrength;

                // ����Ӧ�õ�����
                Vector3 currentForce = obiActor.solver.externalForces[index];
                obiActor.solver.externalForces[index] = dragForce + currentForce;  // ʹ�� += ʹ�������ۻ���
            }

            // �����϶���ʼ��
            initialDragPoint = mousePositionOnPlane;
        }
    }

    // ��ѡ�е�����ʩ����ƽ�淨�߷������
    void ApplyForceToSelectedParticles()
    {
        if(!isDragging) return;  // ֻ�����϶�ʱ��ʩ����

        foreach (int index in draggedParticles)
        {
            Vector3 particlePosition = obiActor.GetParticlePosition(index);
            Vector3 forceDirection = dragPlane.normal; // ʹ��ƽ��ķ��߷���
            Vector3 force = forceDirection * forceStrength; // ����ʩ�ӵ���

            // ����Ӧ�õ�����
            Vector3 currentForce = obiActor.solver.externalForces[index];
            obiActor.solver.externalForces[index] = currentForce + force; // ʹ�� += ʹ�������ۻ���
        }
    }

    void OnDrawGizmos()
    {
        if (isDragging)
        {
            Gizmos.color = Color.green;

            // ����ÿ�����϶�������
            foreach (int index in draggedParticles)
            {
                Vector3 particlePosition = obiActor.GetParticlePosition(index);
                Gizmos.DrawSphere(particlePosition, 0.001f);  // ��ÿ����ѡ�е�����λ�û���һ��С��
            }
        }
    }
}
