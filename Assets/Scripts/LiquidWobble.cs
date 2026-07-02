using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Simulates liquid wobble and sloshing effects for a container based on movement and rotation.
/// Uses shader properties to animate the liquid surface.
/// </summary>
public class LiquidWobble : MonoBehaviour
{
    [Header("Wobble Settings")]
    [SerializeField] private float maxWobble = 0.03f;
    [SerializeField] private float maxAccumulatedWobble = 2f;
    [SerializeField] private float wobbleSpeedMove = 5f;
    [SerializeField] private float recovery = 1f;

    [Header("Velocity Adjustments")]
    [SerializeField] private float velocityDampener = 0.01f;
    [SerializeField] private float verticalWeight = 0.2f;

    [Header("References")]
    //[SerializeField] private Renderer rend;
    [SerializeField] private Transform liquidsParent;

    //private MaterialPropertyBlock propertyBlock;
    private Dictionary<Renderer, MaterialPropertyBlock> _propertyBlocks = new Dictionary<Renderer, MaterialPropertyBlock>();
    private Vector3 lastPos;
    private Quaternion lastRot;

    private float wobbleAmountToAddX;
    private float wobbleAmountToAddZ;
    private float sloshVariance;
    private float pulse;

    private float _fillAmount = 1f;

    private static readonly int WobbleXId = Shader.PropertyToID("_WobbleX");
    private static readonly int WobbleZId = Shader.PropertyToID("_WobbleZ");
    private static readonly int RandomOffsetId = Shader.PropertyToID("_RandomOffset");
    private static readonly int FillAmountId = Shader.PropertyToID("_FillAmount");

    void Start()
    {
        _fillAmount = 1f;

        Renderer[] renderers = liquidsParent.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(propBlock);

            _propertyBlocks.Add(renderer, propBlock);

            sloshVariance = Random.Range(0.4f, 1f);
            propBlock.SetFloat(RandomOffsetId, sloshVariance * 20f);
        }
    }

    void LateUpdate()
    {
        float deltaTime = Time.deltaTime;
        if (deltaTime <= 0f) return;

        // VELOCITY
        Vector3 velocity = (transform.position - lastPos) / deltaTime;
        Vector3 angularVelocity = GetAngularVelocity(deltaTime);

        // WOBBLE DECAY
        float decay = Mathf.Exp(-recovery * deltaTime);
        wobbleAmountToAddX *= decay;
        wobbleAmountToAddZ *= decay;

        // WOBBLE ADDITION
        float addX = (velocity.x + (velocity.y * verticalWeight) + angularVelocity.z + angularVelocity.y) * velocityDampener;
        float addZ = (velocity.z + (velocity.y * verticalWeight) + angularVelocity.x + angularVelocity.y) * velocityDampener;

        wobbleAmountToAddX += Mathf.Clamp(addX, -maxWobble, maxWobble);
        wobbleAmountToAddZ += Mathf.Clamp(addZ, -maxWobble, maxWobble);

        wobbleAmountToAddX = Mathf.Clamp(wobbleAmountToAddX, -maxAccumulatedWobble, maxAccumulatedWobble);
        wobbleAmountToAddZ = Mathf.Clamp(wobbleAmountToAddZ, -maxAccumulatedWobble, maxAccumulatedWobble);

        // SINE WAVE
        pulse = Mathf.Repeat(pulse + (deltaTime * wobbleSpeedMove), Mathf.PI * 2f);
        float sineWave = Mathf.Sin(pulse);

        // APPLY FINAL VALUES
        foreach (KeyValuePair<Renderer, MaterialPropertyBlock> kv in _propertyBlocks)
        {
            Renderer rend = kv.Key;
            MaterialPropertyBlock propBlock = kv.Value;

            propBlock.SetFloat(WobbleXId, wobbleAmountToAddX * sineWave * sloshVariance);
            propBlock.SetFloat(WobbleZId, wobbleAmountToAddZ * sineWave * sloshVariance);
            propBlock.SetFloat(FillAmountId, _fillAmount);
            rend.SetPropertyBlock(propBlock);
        }


        // SAVE POSITION AND ROTATION
        lastPos = transform.position;
        lastRot = transform.rotation;
    }

    /// <summary>
    /// Calculates the angular velocity of the object based on rotation changes.
    /// </summary>
    /// <param name="deltaTime">The time step for velocity calculation.</param>
    /// <returns>The angular velocity as a Vector3.</returns>
    Vector3 GetAngularVelocity(float deltaTime)
    {
        Quaternion deltaRot = transform.rotation * Quaternion.Inverse(lastRot);

        deltaRot.ToAngleAxis(out float angle, out Vector3 axis);

        if (angle > 180f)
            angle -= 360f;
        else if (Mathf.Approximately(angle, 0f))
            return Vector3.zero;

        return axis * angle * Mathf.Deg2Rad / deltaTime;
    }

    public void SetFillAmount(float value) => _fillAmount = Mathf.Clamp01(value);
}