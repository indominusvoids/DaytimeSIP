using UnityEngine;

internal class IsUnderSky : RequirementBase
{
    public override bool IsValid(MinEventParams _params)
    {
        bool flag = !Physics.Raycast(((Component)_params.Self).transform.position + Vector3.up, Vector3.up, 256f, 2130706431);
        Log.Out($"Is Under Sky: {flag}");
        return !this.invert ? flag : !flag;
    }
}