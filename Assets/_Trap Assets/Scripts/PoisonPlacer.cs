using UnityEngine;
using UnityEngine.UI;

public class PoisonPlacer : MonoBehaviour
{
    [Header("Poison Settings")]
    public float poisonRadius = 5f;
    public float poisonDuration = 5f;
    public float fadeDuration = 1.5f;
    public GameObject poisonEffectPrefab;
    public LayerMask groundLayer;

    private bool poisonModeActive = false;

    [SerializeField] Button poisonBtn;

    private void Start()
    {
        poisonBtn.onClick.AddListener(() => 
        {
            ArmPoison();
            poisonBtn.gameObject.SetActive(false);
        });
    }

    void Update()
    {
       

        if (!poisonModeActive) return;

        // --- PC: Mouse click ---
        if (Input.GetMouseButtonDown(0))
        {
            TryPlacePoison(Input.mousePosition);
        }

        // --- Mobile: Touch input ---
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                TryPlacePoison(touch.position);
            }
        }
    }

    void ArmPoison()
    {
        poisonModeActive = true;
        Debug.Log("Poison mode armed — tap the ground!");

        // Optional: show UI indicator to player that poison is armed
        // UIManager.instance.ShowPoisonArmedIcon(true);
    }

    void TryPlacePoison(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
        {
            PlacePoison(hit.point);
            poisonModeActive = false;
            poisonBtn.gameObject.SetActive(true);
            // Optional: hide UI indicator
            // UIManager.instance.ShowPoisonArmedIcon(false);
        }
        else
        {
            Debug.Log("No ground hit — aim at the ground.");
        }
    }

    void PlacePoison(Vector3 position)
    {
        GameObject zone;

        if (poisonEffectPrefab != null)
        {
            zone = Instantiate(poisonEffectPrefab, position, Quaternion.identity);

            // Scale the prefab according to radius
            float diameter = poisonRadius * 2f;
            zone.transform.localScale = new Vector3(diameter, zone.transform.localScale.y, diameter);
        }
        else
        {
            zone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            zone.transform.position = position + Vector3.up * 0.05f;

            // Scale X and Z by diameter, keep Y flat
            float diameter = poisonRadius * 2f;
            zone.transform.localScale = new Vector3(diameter, 0.05f, diameter);

            Material mat = new Material(Shader.Find("Standard"));
            mat.color = new Color(0.2f, 1f, 0.2f, 0.5f);
            mat.SetFloat("_Mode", 3);
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.renderQueue = 3000;
            zone.GetComponent<Renderer>().material = mat;

            Destroy(zone.GetComponent<Collider>());
        }

        PoisonZone pz = zone.AddComponent<PoisonZone>();
        pz.Init(poisonRadius, poisonDuration, fadeDuration);
    }
}