using UnityEngine;

public class IKFootManager : MonoBehaviour
{
    public IKFootSolver leftFoot;
    public IKFootSolver rightFoot;

    private bool leftTurn = true;

    private void Update()
    {
        if (leftTurn && leftFoot.IsReadyToStep && !rightFoot.IsStepping)
        {
            leftFoot.TryStep();
            leftTurn = false;
        }
        else if (!leftTurn && rightFoot.IsReadyToStep && !leftFoot.IsStepping)
        {
            rightFoot.TryStep();
            leftTurn = true;
        }
    }
}
