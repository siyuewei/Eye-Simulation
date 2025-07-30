using Obi;
using System.Collections.Generic;
using UnityEngine;

public class HandleMembraneDrag : MonoBehaviour
{
    public ObiActor obiActor;                       // ObiActor, 存储membrane的粒子
    public float dragRadius = 1.0f;                 // 拖动半径，控制附近粒子的范围
    public float dragStrength = 1.0f;               // 拖动力度
    public float forceStrength = 5.0f;              // 施加到粒子的额外力的强度（右键施加的力）
    public LayerMask membraneLayer;                 // 用于射线检测的层，确保射线只与membrane相交
    public bool isDragging = false;                 // 是否正在拖动membrane
    private List<int> draggedParticles = new List<int>(); // 被拖动的粒子
    private Vector3 lastMousePosition;              // 上一次鼠标位置
    private Vector3 seleceCenter;                   // 选中的中心点

    private Vector3 initialDragPoint;               // 初始拖动点（鼠标按下时的命中点）
    private Vector3 initialMousePosition;           // 鼠标按下时的位置，用来计算偏移量
    private Plane dragPlane;                        // 用于射线检测的拖动平面

    void Update()
    {
        // 处理鼠标左键按下和松开
        if (Input.GetMouseButtonDown(0))
        {
            StartDrag();
        }

        if (Input.GetMouseButtonUp(0))
        {
            StopDrag();
        }

        // 处理鼠标右键按下并施加向平面前的力
        if (Input.GetMouseButtonDown(1))
        {
            ApplyForceToSelectedParticles();
        }

        // 处理鼠标移动时拖动物体
        if (isDragging)
        {
            DragMembrane();
        }
    }

    // 开始拖动
    void StartDrag()
    {
        // 射线检测，发射射线从屏幕空间到世界空间
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, membraneLayer))
        {
            // 如果命中membrane，获取命中点
            Vector3 hitPoint = hit.point;

            // 找到命中点周围的粒子（在拖动半径内的粒子）
            draggedParticles.Clear();  // 清空之前的拖动粒子

            var allIndices = obiActor.solverIndices;
            foreach (int index in allIndices)
            {
                Vector3 particlePosition = obiActor.GetParticlePosition(index);

                // 计算粒子与命中点的距离
                if (Vector3.Distance(particlePosition, hitPoint) < dragRadius)
                {
                    draggedParticles.Add(index);  // 添加到被拖动的粒子列表
                }
            }

            // 初始化拖动平面（平行于世界坐标的一个平面，起点为 hitPoint，法线为摄像机的朝向）
            dragPlane = new Plane(Camera.main.transform.forward, hitPoint);

            isDragging = true;  // 开始拖动
            initialDragPoint = hitPoint;  // 记录初始命中点
            initialMousePosition = Input.mousePosition;  // 记录鼠标按下时的位置
        }
    }

    // 停止拖动
    void StopDrag()
    {
        isDragging = false;
        draggedParticles.Clear();  // 清空拖动粒子列表
    }

    // 拖动membrane（鼠标移动时）
    void DragMembrane()
    {
        // 计算鼠标的平面坐标
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float distanceToPlane;

        if (dragPlane.Raycast(ray, out distanceToPlane))
        {
            // 计算鼠标与平面的交点
            Vector3 mousePositionOnPlane = ray.GetPoint(distanceToPlane);
            Vector3 offset = mousePositionOnPlane - initialDragPoint;  // 计算鼠标移动的偏移量

            // 对每个被拖动的粒子施加外力
            foreach (int index in draggedParticles)
            {
                Vector3 particlePosition = obiActor.GetParticlePosition(index);
                Vector3 direction = offset;  // 偏移量作为拖动的方向

                // 施加拖动力
                Vector3 dragForce = direction.normalized * dragStrength;

                // 将力应用到粒子
                Vector3 currentForce = obiActor.solver.externalForces[index];
                obiActor.solver.externalForces[index] = dragForce + currentForce;  // 使用 += 使得力是累积的
            }

            // 更新拖动起始点
            initialDragPoint = mousePositionOnPlane;
        }
    }

    // 给选中的粒子施加向平面法线方向的力
    void ApplyForceToSelectedParticles()
    {
        if(!isDragging) return;  // 只有在拖动时才施加力

        foreach (int index in draggedParticles)
        {
            Vector3 particlePosition = obiActor.GetParticlePosition(index);
            Vector3 forceDirection = dragPlane.normal; // 使用平面的法线方向
            Vector3 force = forceDirection * forceStrength; // 计算施加的力

            // 将力应用到粒子
            Vector3 currentForce = obiActor.solver.externalForces[index];
            obiActor.solver.externalForces[index] = currentForce + force; // 使用 += 使得力是累积的
        }
    }

    void OnDrawGizmos()
    {
        if (isDragging)
        {
            Gizmos.color = Color.green;

            // 绘制每个被拖动的粒子
            foreach (int index in draggedParticles)
            {
                Vector3 particlePosition = obiActor.GetParticlePosition(index);
                Gizmos.DrawSphere(particlePosition, 0.001f);  // 在每个被选中的粒子位置绘制一个小球
            }
        }
    }
}
