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
        //把粒子的速度设定成当前值的viscosityFactor倍
        var allIndices = obiActor.solverIndices;
        for (int i = 0; i < allIndices.count; i++)
        {
            int particleIndex = allIndices[i];
            obiSolver.velocities[particleIndex] *= viscosityFactor;
        }
    }
}
