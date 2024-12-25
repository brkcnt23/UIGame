using DG.Tweening;
using UnityEngine;

public class NavPanelAnimation : MonoBehaviour
{
    public void OpenPanel()
    {
        if (gameObject.activeSelf)
        {
            ClosePanel();
        }
        else
        {
            gameObject.SetActive(true);
            transform.DOScaleY(1, 0.5f).SetEase(Ease.OutBack);
        }
    }

    public void ClosePanel()
    {
        transform.DOScaleY(0, 0.5f).SetEase(Ease.InBack).OnComplete(() => gameObject.SetActive(false));
    }
}
