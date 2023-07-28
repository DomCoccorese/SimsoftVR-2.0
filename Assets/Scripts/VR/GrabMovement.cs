// Copyright 2022-2023 Herobots Srl
// https://www.herobots.eu/

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabMovement : MonoBehaviour
{
    [SerializeField] private InputActionAsset actionAsset;

    private Vector3 startPoint;

    public float defaultGrabSpeedFactor = 10.0f;
    public static float GrabSpeedFactor { get;  set; }


    [SerializeField] private XRRayInteractor leftControllerInteractor;
    [SerializeField] private XRRayInteractor rightControllerInteractor;
    private XRRayInteractor currentInteractor;

    private InputAction callGrabMoveLeft;
    private InputAction callGrabMoveRight;

    private bool isGrabbing =false;

    private void Start()
    {
        callGrabMoveLeft = actionAsset.FindActionMap("XRI LeftHand Locomotion").FindAction("Grab Move");
        callGrabMoveRight = actionAsset.FindActionMap("XRI RightHand Locomotion").FindAction("Grab Move");

        callGrabMoveLeft.Enable();
        callGrabMoveRight.Enable();

        GrabSpeedFactor = defaultGrabSpeedFactor;
    }

    // Update is called once per frame
    void Update()
    {
        if (callGrabMoveLeft.WasPerformedThisFrame())
            ActivateGrabMove(true, leftControllerInteractor);

        if (callGrabMoveRight.WasPerformedThisFrame())
            ActivateGrabMove(true,rightControllerInteractor);

        if (callGrabMoveLeft.WasReleasedThisFrame())
            ActivateGrabMove(false,leftControllerInteractor);

        if (callGrabMoveRight.WasReleasedThisFrame())
            ActivateGrabMove(false,rightControllerInteractor);

        if (!isGrabbing)
            return;

        Vector3 translation = currentInteractor.transform.position - startPoint;
        currentInteractor.transform.parent.Translate(-translation * GrabSpeedFactor * Time.fixedDeltaTime);
    }

    private void ActivateGrabMove(bool activate, XRRayInteractor interactor)
    {
        currentInteractor = interactor;
        if (activate)
            startPoint = interactor.transform.position;

        isGrabbing = activate;
    }
}
