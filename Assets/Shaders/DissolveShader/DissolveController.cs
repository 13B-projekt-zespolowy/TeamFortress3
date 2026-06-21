using UnityEngine;

public class DissolveController : MonoBehaviour
{
    private Material[] _materials;
    private Renderer[] _renderers;

    [Header("Parametry animacji")]
    [Tooltip("Szybkoœæ, z jak¹ obiekt bêdzie znika³/pojawia³.")]
    public float dissolveSpeed = 0.5f;

    [Header("Testowanie w edytorze")]
    [Tooltip("Rêczne sterowanie rozpuszczaniem.")]
    [Range(0f, 1f)]
    public float currentDissolveAmount = 0f;

    private bool _isDissolving = false;

    [Header("Parametryzacja wizualna shadera")]
    [Tooltip("Ustawia kolor krawêdzi wypalania bezpoœrednio w shaderze.")]
    [ColorUsage(true, true)]
    public Color edgeColor = new Color(1.0f, 0.5f, 0.0f, 1.0f);

    [Tooltip("Ustawia gruboœæ krawêdzi wypalania.")]
    [Range(0.0f, 0.3f)]
    public float edgeThickness = 0.05f;

    void Start()
    {
        _renderers = GetComponentsInChildren<Renderer>();
        _materials = new Material[_renderers.Length];

        for (int i = 0; i < _renderers.Length; i++)
        {
            _materials[i] = _renderers[i].material;
        }

        SyncShaderParameters();
    }

    void Update()
    {
        SyncShaderParameters();

        if (_isDissolving)
        {
            UpdateDissolveProgress();
        }
        else
        {
            foreach (var mat in _materials)
            {
                if (mat != null) mat.SetFloat("_DissolveAmount", currentDissolveAmount);
            }
        }
    }

    [ContextMenu("Start animacji")]
    public void StartDissolve()
    {
        SetRenderersVisible(true);
        _isDissolving = true;
    }

    [ContextMenu("Zresetuj")]
    public void ResetEffect()
    {
        _isDissolving = false;
        currentDissolveAmount = 0f;

        SetRenderersVisible(true);

        foreach (var mat in _materials)
        {
            if (mat != null) mat.SetFloat("_DissolveAmount", currentDissolveAmount);
        }
    }

    private void SyncShaderParameters()
    {
        if (_materials == null) return;

        foreach (var mat in _materials)
        {
            if (mat != null)
            {
                mat.SetColor("_EdgeColor", edgeColor);
                mat.SetFloat("_EdgeThickness", edgeThickness);
            }
        }
    }

    private void UpdateDissolveProgress()
    {
        currentDissolveAmount += dissolveSpeed * Time.deltaTime;
        currentDissolveAmount = Mathf.Clamp01(currentDissolveAmount);

        foreach (var mat in _materials)
        {
            if (mat != null) mat.SetFloat("_DissolveAmount", currentDissolveAmount);
        }

        if (currentDissolveAmount >= 1.0f)
        {
            _isDissolving = false;
            SetRenderersVisible(false);
        }
    }

    private void SetRenderersVisible(bool isVisible)
    {
        if (_renderers == null) return;

        foreach (var rend in _renderers)
        {
            if (rend != null)
            {
                rend.enabled = isVisible;
            }
        }
    }
   private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            StartDissolve();
        }
    }
}