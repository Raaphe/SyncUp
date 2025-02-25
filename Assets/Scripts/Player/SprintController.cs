using UnityEngine;
using UnityEngine.UI;

public class SprintController : MonoBehaviour
{

    private CanvasGroup sprintBarCG;
    public bool UseSprintBar = true;
    public bool HideBarWhenFull = true;
    public Image SprintBarBG;
    public Image SprintBar;
    public float SprintBarWidthPercent = .3f;
    public float SprintBarHeightPercent = .015f;
    private float sprintBarWidth;
    private float sprintBarHeight;

    public float WalkSpeed = 5f;
    public bool IsWalking = false;
    public bool IsSprinting = false;
    public bool EnableSprint = true;
    public bool UnlimitedSprint = false;
    public KeyCode SprintKey = KeyCode.LeftShift;
    public float SprintSpeed = 7f;
    public float SprintDuration = 5f;
    public float SprintCooldownInSeconds = 2f;
    public float SprintFOV = 80f;
    public float SprintFOVStepTime = 10f;
    public float MaxVelocityChange = 10f;

    private float sprintRemaining;
    private bool OnCooldown = false;

    private Vector3 targetVelocity;
    private Vector3 velocity;
    private Vector3 velocityChange;

    private PlayerCameraController playerCameraController;
    private JumpController jumpController;
    private PlayerController firstPersonController;
    private CrouchController crouchController;

    private void Awake()
    {
        if (!UnlimitedSprint)
            sprintRemaining = SprintDuration;
    }

    private void Start()
    {
        firstPersonController = GetComponent<PlayerController>();
        jumpController = GetComponent<JumpController>();
        playerCameraController = GetComponent<PlayerCameraController>();
        crouchController = GetComponent<CrouchController>();
        sprintBarCG = GetComponentInChildren<CanvasGroup>();
        SetUpSprintBar();
    }

    private void SetUpSprintBar()
    {
        if (UseSprintBar)
        {
            SprintBarBG.gameObject.SetActive(true);
            SprintBar.gameObject.SetActive(true);

            sprintBarWidth = Screen.width * SprintBarWidthPercent;
            sprintBarHeight = Screen.height * SprintBarHeightPercent;

            SprintBarBG.rectTransform.sizeDelta = new Vector3(sprintBarWidth, sprintBarHeight, 0f);
            SprintBar.rectTransform.sizeDelta = new Vector3(sprintBarWidth - 2, sprintBarHeight - 2, 0f);

            if (HideBarWhenFull) sprintBarCG.alpha = 0;
        }
        else
        {
            SprintBarBG.gameObject.SetActive(false);
            SprintBar.gameObject.SetActive(false);
        }
    }

    private void UpdateSprintBar()
    {
        if (UseSprintBar && !UnlimitedSprint)
            SprintBar.transform.localScale = new Vector3(sprintRemaining / SprintDuration, 1f, 1f);
    }

    private void ToSprintFov()
    {
        playerCameraController.playerCamera.fieldOfView = 
            Mathf.Lerp(playerCameraController.playerCamera.fieldOfView, SprintFOV, SprintFOVStepTime * Time.deltaTime);
    }

    private void UpdateSprintRemaining()
    {
        if (IsSprinting)
        {
            playerCameraController.isZoomed = false;
            ToSprintFov();
            if (!UnlimitedSprint)
            {
                sprintRemaining -= 1f * Time.deltaTime;
                if (sprintRemaining <= 0)
                {
                    IsSprinting = false;
                    OnCooldown = true;
                    StartCoroutine(TimingController.Time(TimeType.SCALEDTIME, SprintCooldownInSeconds, () => OnCooldown = false));
                }
            }
        }
        else sprintRemaining = Mathf.Clamp(sprintRemaining += 1 * Time.deltaTime, 0, SprintDuration);
    }

    private void SetIsWalking()
    {
        if (targetVelocity.x != 0 || targetVelocity.z != 0 && jumpController.IsGrounded)
            IsWalking = true;
        else
            IsWalking = false;
    }

    private bool CanSprint()
    {
        return EnableSprint && Input.GetKey(SprintKey) && sprintRemaining > 0f && !OnCooldown;
    }

    private void UpdateVelocityChange()
    {
        targetVelocity = transform.TransformDirection(targetVelocity) * SprintSpeed;
        velocity = firstPersonController._rigidBody.velocity;
        velocityChange = (targetVelocity - velocity);
        velocityChange.x = Mathf.Clamp(velocityChange.x, -MaxVelocityChange, MaxVelocityChange);
        velocityChange.z = Mathf.Clamp(velocityChange.z, -MaxVelocityChange, MaxVelocityChange);
        velocityChange.y = 0;
    }

    private void Sprint()
    {
        targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        SetIsWalking();
        if (CanSprint())
        {
            UpdateVelocityChange();
            if (velocityChange.x != 0 || velocityChange.z != 0)
            {
                IsSprinting = true;
                if (crouchController.IsCrouched) //uncrouch
                    crouchController.Crouch();
                if (HideBarWhenFull && !UnlimitedSprint)
                    sprintBarCG.alpha += 5 * Time.deltaTime;
            }
            firstPersonController._rigidBody.AddForce(velocityChange, ForceMode.VelocityChange);
        }
        else
        {
            IsSprinting = false;

            if (HideBarWhenFull && sprintRemaining == SprintDuration) sprintBarCG.alpha -= 3 * Time.deltaTime;

            targetVelocity = transform.TransformDirection(targetVelocity) * WalkSpeed;

            Vector3 velocity = firstPersonController._rigidBody.velocity;
            Vector3 velocityChange = (targetVelocity - velocity);
            velocityChange.x = Mathf.Clamp(velocityChange.x, -MaxVelocityChange, MaxVelocityChange);
            velocityChange.z = Mathf.Clamp(velocityChange.z, -MaxVelocityChange, MaxVelocityChange);
            velocityChange.y = 0;
            firstPersonController._rigidBody.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }

    public void UpdateSprintStates()
    {
        if (EnableSprint)
        {
            UpdateSprintRemaining();
            UpdateSprintBar();
        }
    }

    public void UpdateSprintMovementState()
    {
        if (EnableSprint)
            Sprint();
    }

}
