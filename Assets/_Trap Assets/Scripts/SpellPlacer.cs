using UnityEngine;
using UnityEngine.UI;

public enum SpellType { Poison, Ice, Fire, Shock }

[System.Serializable]
public class SpellClass
{
    [Header("UI")]
    public Button spellBtn;

    [Header("Identity")]
    public SpellType spellType = SpellType.Poison;

    [Header("Visuals")]
    public ParticleSystem spellParticle;
    public GameObject effectPrefab;

    [Header("Zone — General")]
    public float spellRadius = 5f;
    public float spellDuration = 5f;
    public float fadeDuration = 1.5f;

    [Header("Poison / Fire — Damage")]
    public float damagePerTick = 2f;
    public float tickInterval = 0.5f;

    [Header("Ice — Freeze")]
    public float iceDuration = 4f;

    [Header("Shock — Stutter")]
    public float shockFreezeTime = 0.3f;
    public float shockRunTime = 0.4f;

    [HideInInspector] public bool isArmed;
}

public class SpellPlacer : MonoBehaviour
{
    [Header("Spells")]
    [SerializeField] private SpellClass[] spells;

    [Header("Raycasting")]
    [SerializeField] private LayerMask groundLayer;

    private int armedIndex = -1;

    private void Start()
    {
        for (int i = 0; i < spells.Length; i++)
        {
            int idx = i;
            if (spells[idx].spellBtn == null)
            {
                Debug.LogWarning($"[SpellPlacer] Spell [{idx}] has no Button assigned.");
                continue;
            }
            spells[idx].spellBtn.onClick.AddListener(() => ArmSpell(idx));
        }
    }

    private void Update()
    {
        if (armedIndex < 0) return;

#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            TryPlace(Input.mousePosition);
            return;
        }
#endif
        if (Input.touchCount > 0)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase == TouchPhase.Began)
                TryPlace(t.position);
        }
    }

    private void ArmSpell(int index)
    {
        if (armedIndex >= 0 && armedIndex != index)
            Disarm(armedIndex);

        armedIndex = index;
        spells[index].isArmed = true;
        spells[index].spellBtn.gameObject.SetActive(false);
        Debug.Log($"[SpellPlacer] {spells[index].spellType} armed – tap the ground!");
    }

    private void Disarm(int index)
    {
        spells[index].isArmed = false;
        spells[index].spellBtn.gameObject.SetActive(true);
    }

    private void TryPlace(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            PlaceSpell(armedIndex, hit.point);
            Disarm(armedIndex);
            armedIndex = -1;
        }
        else
        {
            Debug.Log("[SpellPlacer] No ground hit.");
        }
    }

    private void PlaceSpell(int index, Vector3 pos)
    {
        SpellClass s = spells[index];

        if (s.effectPrefab != null)
        {
            GameObject zone = Instantiate(s.effectPrefab, pos, Quaternion.identity);

            // Ensure a SphereCollider exists — the zone Init() will configure it
            if (zone.GetComponent<SphereCollider>() == null)
                zone.AddComponent<SphereCollider>();

            switch (s.spellType)
            {
                case SpellType.Poison:
                    zone.AddComponent<PoisonZone>()
                        .Init(s.spellRadius, s.spellDuration, s.fadeDuration,
                              s.damagePerTick, s.tickInterval);
                    break;

                case SpellType.Ice:
                    zone.AddComponent<IceZone>()
                        .Init(s.spellRadius, s.iceDuration, s.fadeDuration);
                    break;

                case SpellType.Fire:
                    zone.AddComponent<FireZone>()
                        .Init(s.spellRadius, s.spellDuration, s.fadeDuration,
                              s.damagePerTick, s.tickInterval);
                    break;

                case SpellType.Shock:
                    zone.AddComponent<ShockZone>()
                        .Init(s.spellRadius, s.spellDuration, s.fadeDuration,
                              s.shockFreezeTime, s.shockRunTime);
                    break;
            }
        }

        // One-shot particle burst
        if (s.spellParticle != null)
        {
            ParticleSystem fx = Instantiate(s.spellParticle, pos, Quaternion.identity);
            fx.Play();
            Destroy(fx.gameObject, fx.main.duration + fx.main.startLifetime.constantMax);
        }

        Debug.Log($"[SpellPlacer] {s.spellType} placed at {pos}");
    }
}