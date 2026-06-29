using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class GamepadVRController : MonoBehaviour
{
    [Header("SteamVR Actions (assign in Inspector)")]
    public SteamVR_Action_Boolean moveUpAction;
    public SteamVR_Action_Boolean moveDownAction;
    public SteamVR_Action_Boolean moveLeftAction;
    public SteamVR_Action_Boolean moveRightAction;
    public SteamVR_Action_Vector2 handTranslateAction;
    public SteamVR_Action_Vector2 handRotateAction;
    public SteamVR_Action_Boolean grabGripAction;
    public SteamVR_Action_Boolean grabPinchAction;

    [Header("References")]
    public Transform head;
    public Transform rightHand;
    public CharacterController cc;

    [Header("Speeds")]
    public float moveSpeed = 3f;
    public float handMoveSpeed = 0.5f;
    public float handRotateSpeed = 45f;

    [Header("Grab Settings")]
    public float grabRadius = 0.25f;
    public LayerMask grabLayers;

    [Header("Pistol Attach Offset (local)")]
    public Vector3 pistolLocalPosition;
    public Vector3 pistolLocalEulerAngles;

    private Vector3 handOffset;
    private Hand handComponent;
    private Interactable currentGun;
    private Transform originalGunParent;
    private bool isGrabbing;

    private void Start()
    {
        handOffset = rightHand.localPosition;
        handComponent = rightHand.GetComponent<Hand>();
        if (handComponent == null)
            Debug.LogError("RightHand не содержит компонент Hand");
    }

    private void Update()
    {
        HandleMovement();
        HandleHandControl();
        HandleGrab();
        HandleShoot();
    }

    private void HandleMovement()
    {
        Vector2 mv = Vector2.zero;
        if (moveUpAction.GetState(SteamVR_Input_Sources.Any)) mv.y += 1f;
        if (moveDownAction.GetState(SteamVR_Input_Sources.Any)) mv.y -= 1f;
        if (moveRightAction.GetState(SteamVR_Input_Sources.Any)) mv.x += 1f;
        if (moveLeftAction.GetState(SteamVR_Input_Sources.Any)) mv.x -= 1f;

        Vector3 forward = Vector3.Scale(head.forward, new Vector3(1, 0, 1)).normalized;
        Vector3 right = Vector3.Scale(head.right, new Vector3(1, 0, 1)).normalized;
        cc.Move((forward * mv.y + right * mv.x) * moveSpeed * Time.deltaTime);
    }

    private void HandleHandControl()
    {
        Vector2 translate = handTranslateAction.axis;
        Vector2 rotate = handRotateAction.axis;

        Vector3 dPos = (head.right * translate.x + head.up * translate.y) * handMoveSpeed * Time.deltaTime;
        handOffset += dPos;
        rightHand.position = head.position + handOffset;

        Quaternion yaw = Quaternion.AngleAxis(rotate.x * handRotateSpeed * Time.deltaTime, Vector3.up);
        Quaternion pitch = Quaternion.AngleAxis(-rotate.y * handRotateSpeed * Time.deltaTime, Vector3.right);
        rightHand.rotation = rightHand.rotation * yaw * pitch;
    }

    private void HandleGrab()
    {
        if (grabGripAction.GetStateDown(SteamVR_Input_Sources.Any) && !isGrabbing)
        {
            TryGrabGun();
        }
        else if (grabGripAction.GetStateUp(SteamVR_Input_Sources.Any) && isGrabbing)
        {
            ReleaseGun();
        }
    }

    private void TryGrabGun()
    {
        if (currentGun != null) return;

        Collider[] hits = Physics.OverlapSphere(
            rightHand.position,
            grabRadius,
            grabLayers,
            QueryTriggerInteraction.Collide);

        Interactable nearest = null;
        float bestDist = float.MaxValue;
        foreach (var col in hits)
        {
            var inter = col.GetComponent<Interactable>();
            if (inter == null) continue;
            float dist = Vector3.Distance(rightHand.position, col.ClosestPoint(rightHand.position));
            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = inter;
            }
        }

        if (nearest != null)
        {
            handComponent.HoverLock(nearest);

            originalGunParent = nearest.transform.parent;

            if (nearest.TryGetComponent<Rigidbody>(out var rb))
                rb.isKinematic = true;

            nearest.transform.SetParent(rightHand);
            nearest.transform.localPosition = pistolLocalPosition;
            nearest.transform.localEulerAngles = pistolLocalEulerAngles;

            currentGun = nearest;
            isGrabbing = true;
        }
    }

    private void ReleaseGun()
    {
        if (currentGun == null) return;

        if (currentGun.TryGetComponent<Rigidbody>(out var rb))
            rb.isKinematic = false;

        currentGun.transform.SetParent(originalGunParent);
        handComponent.HoverUnlock(currentGun);

        currentGun = null;
        isGrabbing = false;
    }

    private void HandleShoot()
    {
        if (grabPinchAction.GetStateDown(SteamVR_Input_Sources.Any) && currentGun != null)
        {
            var pistolFire = currentGun.GetComponent<PistolFire>();
            pistolFire?.Fire();
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (rightHand == null) return;
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(rightHand.position, grabRadius);
    }
}