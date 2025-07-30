using Obi;
using UnityEngine;

public class HandleMembraneViscosity : MonoBehaviour
{
    public ObiActor obiActor;
    private ObiSolver obiSolver;
    public float viscosityFactor;
    private bool isViscosity = false;


    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.F))
        {
            isViscosity = true;
        }
        Viscosity();
    }

    void Viscosity()
    {
        if (!isViscosity)
        {
            return;
        }

        obiSolver = obiActor.solver;
        //�����ӵ��ٶ��趨�ɵ�ǰֵ��viscosityFactor��
        var allIndices = obiActor.solverIndices;
        for (int i = 0; i < allIndices.count; i++)
        {
            int particleIndex = allIndices[i];
            obiSolver.velocities[particleIndex] *= viscosityFactor;
        }
    }
}
