using UnityEngine;
using UnityEngine.Animations.Rigging;

public class LeftHandIKBinder : MonoBehaviour
{
    [SerializeField] private TwoBoneIKConstraint leftArmIK;

    const string TargetName = "LeftHandIK";
    const string HintName = "LeftElbowHint";

    public void BindToWeapon(GameObject weaponInstance)
    {
        Transform target = FindDeepChild(weaponInstance.transform, "LeftHandIK");
        Transform hint = FindDeepChild(weaponInstance.transform, "LeftElbowHint");

        if (!target || !hint)
        {
            leftArmIK.weight = 0f;
            return;
        }

        var data = leftArmIK.data;
        data.target = target;
        data.hint = hint;
        leftArmIK.data = data;

        leftArmIK.weight = 1f;
    }
    static Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name) return child;
            var result = FindDeepChild(child, name);
            if (result) return result;
        }
        return null;
    }
}