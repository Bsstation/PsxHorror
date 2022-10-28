using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKFeetController : MonoBehaviour
{
    #region Var
    Animator anim;
    Vector3 rightFootPosition, leftFootPosition, rightFootIKPosition, leftFootIKPosition;
    Quaternion rightFootIKRootation, leftFootIKRotation;
    float lastPelvisPositionY, lastLeftFootPositionY, lastRightFootPositionY;

    public bool enableIK;
    [Range (0,2)] public float heightFromGroundRay = 1.14f;
    [Range(0, 2)] public float raycastDownDistance = 1.5f;
    public LayerMask layers;

    public float pelvisOffset;
    [Range(0, 1)] public float pelvisUpAndDownSpeed = 0.28f;
    [Range(0, 1)] public float feetToIkPositionSpeed = 0.5f;

    public string leftFootCurve = "IKFootL", rightFootCurve = "IKFootR";
    public bool useProIkFeature = false;
    public bool showSolverResolver = true;
    #endregion

    #region
    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (!enableIK) return;

        AdjustFeetTarget(ref leftFootPosition, HumanBodyBones.LeftFoot);
        AdjustFeetTarget(ref rightFootPosition, HumanBodyBones.RightFoot);

        FeetPosSolver(rightFootPosition, ref rightFootIKPosition, ref rightFootIKRootation);
        FeetPosSolver(leftFootPosition, ref leftFootIKPosition, ref leftFootIKRotation);
    }

    private void OnAnimatorIK(int layerIndex)
    {
        if (!enableIK) return;
        MovePelvisHeight();

        anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, 1f);

        if (useProIkFeature)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.RightFoot, anim.GetFloat(rightFootCurve));
        }
        MoveFeetToIkPoint(AvatarIKGoal.RightFoot, rightFootIKPosition, rightFootIKRootation, ref lastRightFootPositionY);

        anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, 1f);

        if (useProIkFeature)
        {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot, anim.GetFloat(leftFootCurve));
        }
        MoveFeetToIkPoint(AvatarIKGoal.LeftFoot, leftFootIKPosition, leftFootIKRotation, ref lastLeftFootPositionY);
    }
    #endregion

    #region
    void MoveFeetToIkPoint(AvatarIKGoal foot, Vector3 positionIkHolder, Quaternion rotationIkHolder, ref float lastFootPosY)
    {
        Vector3 targetIkPos = anim.GetIKPosition(foot);

        if(positionIkHolder != Vector3.zero)
        {
            targetIkPos = transform.InverseTransformPoint(targetIkPos);
            positionIkHolder = transform.InverseTransformPoint(positionIkHolder);

            float yVariable = Mathf.Lerp(lastLeftFootPositionY, positionIkHolder.y, feetToIkPositionSpeed);
            targetIkPos.y += yVariable;

            lastLeftFootPositionY = yVariable;

            targetIkPos = transform.TransformPoint(targetIkPos);
            anim.SetIKRotation(foot, rotationIkHolder);
        }
        anim.SetIKPosition(foot, targetIkPos);
    }

    void MovePelvisHeight()
    {
        if(rightFootIKPosition == Vector3.zero || leftFootIKPosition == Vector3.zero || lastPelvisPositionY == 0)
        {
            lastPelvisPositionY = anim.bodyPosition.y;
            return;
        }

        float lOffsetPos = leftFootIKPosition.y - transform.position.y;
        float rOffsetPos = rightFootIKPosition.y - transform.position.y;

        float totalOffset = (lOffsetPos < rOffsetPos) ? lOffsetPos : rOffsetPos;

        Vector3 newPelvisPos = anim.bodyPosition + Vector3.up * totalOffset;

        newPelvisPos.y = Mathf.Lerp(lastPelvisPositionY, newPelvisPos.y, pelvisUpAndDownSpeed);
        anim.bodyPosition = newPelvisPos;
        lastPelvisPositionY = anim.bodyPosition.y;
    }

    void FeetPosSolver(Vector3 fromSkyPos, ref Vector3 feetIkPosition, ref Quaternion feetIkRotation)
    {
        RaycastHit hit;
        if (showSolverResolver)
            Debug.DrawLine(fromSkyPos, fromSkyPos + Vector3.down * (raycastDownDistance + heightFromGroundRay), Color.yellow); 

        if (Physics.Raycast(fromSkyPos, Vector3.down, out hit, raycastDownDistance + heightFromGroundRay, layers))
        {
            feetIkPosition = fromSkyPos;
            feetIkPosition.y = hit.point.y + pelvisOffset;
            feetIkRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * transform.rotation;
            return;
        }
        feetIkPosition = Vector3.zero;
    }

    void AdjustFeetTarget(ref Vector3 feetPosition, HumanBodyBones foot)
    {
        feetPosition = anim.GetBoneTransform(foot).position;
        feetPosition.y = transform.position.y + heightFromGroundRay;
    }
    #endregion
}