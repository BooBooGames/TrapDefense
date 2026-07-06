using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class ModularProgressBar : MonoBehaviour
{
    [SerializeField] private RectMask2D _fillMask;

    [Range(0f, 1f)]
    [SerializeField] private float _progress = 1f;

    private float _maxWidth = -1f;

    private IEnumerator Start()
    {
        yield return new WaitForEndOfFrame();

        _maxWidth = _fillMask.rectTransform.rect.width;
    }

    public async void SetProgress(float pValue)
    {
        while(_maxWidth < 0f)
        {
            await Awaitable.EndOfFrameAsync();
        }

        _progress = Mathf.Clamp01(pValue);

        Vector4 padding = _fillMask.padding;
        padding.z = _maxWidth - (_maxWidth * _progress);
        _fillMask.padding = padding;
    }
}