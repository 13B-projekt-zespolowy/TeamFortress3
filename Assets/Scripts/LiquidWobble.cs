using UnityEngine;

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
    [SerializeField] private Renderer rend;

    private MaterialPropertyBlock propertyBlock;
    private Vector3 lastPos;
    private Quaternion lastRot;

    private float wobbleAmountToAddX;
    private float wobbleAmountToAddZ;
    private float sloshVariance;
    private float pulse;

    private static readonly int WobbleXId = Shader.PropertyToID("_WobbleX");
    private static readonly int WobbleZId = Shader.PropertyToID("_WobbleZ");
    private static readonly int RandomOffsetId = Shader.PropertyToID("_RandomOffset");

    void Start()
    {
        if (rend == null)
            rend = GetComponent<Renderer>();

        propertyBlock = new MaterialPropertyBlock();
        rend.GetPropertyBlock(propertyBlock);

        sloshVariance = Random.Range(0.4f, 1f);
        propertyBlock.SetFloat(RandomOffsetId, sloshVariance * 20f);
        Debug.Log(sloshVariance + " || " + propertyBlock.GetFloat(RandomOffsetId));
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
        propertyBlock.SetFloat(WobbleXId, wobbleAmountToAddX * sineWave * sloshVariance);
        propertyBlock.SetFloat(WobbleZId, wobbleAmountToAddZ * sineWave * sloshVariance);
        rend.SetPropertyBlock(propertyBlock);


        // SAVE POSITION AND ROTATION
        lastPos = transform.position;
        lastRot = transform.rotation;
    }

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
}