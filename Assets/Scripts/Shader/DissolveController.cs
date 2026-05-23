using UnityEngine;

public class DissolveController : MonoBehaviour
{
    private Material[] _materials;
    private float _currentDissolveAmount = 0f;
    private bool _isDissolving = false;

    [Header("Parametry animacji")]
    [Tooltip("Szybkoœæ, z jak¹ obiekt bêdzie znika³/pojawia³.")]
    public float dissolveSpeed = 0.5f;

    [Header("Parametryzacja wizualna shadera")]
    [Tooltip("Ustawia kolor krawêdzi wypalania bezpoœrednio w shaderze.")]
    public Color edgeColor = new Color(1.0f, 0.5f, 0.0f, 1.0f);

    [Tooltip("Ustawia gruboœæ krawêdzi wypalania.")]
    [Range(0.0f, 0.3f)]
    public float edgeThickness = 0.05f;

    void Start()
    {
        Renderer[] renderers = GetComponentsInChildren<Renderer>();
        _materials = new Material[renderers.Length];

        for (int i = 0; i < renderers.Length; i++)
        {
            _materials[i] = renderers[i].material;
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
    }

    public void StartDissolve()
    {
        _isDissolving = true;
    }

    public void ResetEffect()
    {
        _isDissolving = false;
        _currentDissolveAmount = 0f;

        foreach (var mat in _materials)
        {
            if (mat != null) mat.SetFloat("_DissolveAmount", _currentDissolveAmount);
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
        _currentDissolveAmount += dissolveSpeed * Time.deltaTime;
        _currentDissolveAmount = Mathf.Clamp01(_currentDissolveAmount);

        foreach (var mat in _materials)
        {
            if (mat != null) mat.SetFloat("_DissolveAmount", _currentDissolveAmount);
        }

        if (_currentDissolveAmount >= 1.0f)
        {
            _isDissolving = false;
            Destroy(gameObject);
        }
    }
}