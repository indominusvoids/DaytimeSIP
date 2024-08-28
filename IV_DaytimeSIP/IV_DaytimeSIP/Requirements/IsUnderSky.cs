using UnityEngine;

internal class IsUnderSky : RequirementBase
{
    public override bool IsValid(MinEventParams _params)
    {
        if (_params.Self == null)
        {
            Debug.LogError("[IsUnderSky] _params.Self is null.");
            return false;
        }

        Vector3 raycastOrigin = ((Component)_params.Self).transform.position + Vector3.up;
        bool flag = !Physics.Raycast(raycastOrigin, Vector3.up, 256f, 2130706431);

        return !this.invert ? flag : !flag;
    }
}