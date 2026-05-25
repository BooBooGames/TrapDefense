using UnityEngine;
using TMPro;
using System.Collections;

public class CoinBillboard : MonoBehaviour
{
    public TextMeshProUGUI coinCounterLabel;
    [SerializeField][Min(0f)] private float upwardDistance = 0.65f;
    [SerializeField][Min(0.01f)] private float upwardDuration = 0.35f;
    [SerializeField][Min(0.01f)] private float scaleDownDuration = 0.35f;

    private Camera lookCamera;
    private Vector3 initialScale;

    private void Awake()
    {
        lookCamera = Camera.main;
        initialScale = transform.localScale;
    }

    public void Show(int coinReward)
    {
        coinCounterLabel.text = CoinFormatter.FormatCoins(coinReward);
        StopAllCoroutines();
        StartCoroutine(PlayRewardFeedback());
    }

    private IEnumerator PlayRewardFeedback()
    {
        Vector3 startPosition = transform.position;
        Vector3 endPosition = startPosition + Vector3.up * upwardDistance;
        float elapsed = 0f;

        while (elapsed < upwardDuration)
        {
            float t = Mathf.Clamp01(elapsed / upwardDuration);
            transform.position = Vector3.Lerp(startPosition, endPosition, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = endPosition;
        elapsed = 0f;

        while (elapsed < scaleDownDuration)
        {
            float t = Mathf.Clamp01(elapsed / scaleDownDuration);
            transform.localScale = Vector3.Lerp(initialScale, Vector3.zero, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.zero;
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        transform.LookAt(Camera.main.transform);
    }
#endif

    private void Update()
    {
        transform.LookAt(lookCamera.transform);
    }
}
