using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Switches the three profile tabs and swaps their artwork.
///
/// Unity's own SpriteSwap transition is tied to pointer state, so a selected
/// tab reverts the moment focus moves. Selection here is explicit: one tab is
/// current, and only that one wears the selected sprite.
/// </summary>
public class ProfileTabController : MonoBehaviour
{
    [SerializeField] private Button[] tabButtons;
    [SerializeField] private GameObject[] tabPages;
    [SerializeField] private Sprite[] normalSprites;
    [SerializeField] private Sprite[] selectedSprites;

    [SerializeField] private int startingTab = 0;

    private int _current = -1;

    private void Awake()
    {
        for (int i = 0; i < tabButtons.Length; i++)
        {
            int index = i;   // captured per iteration, not shared
            if (tabButtons[i] == null) continue;

            tabButtons[i].onClick.RemoveAllListeners();
            tabButtons[i].onClick.AddListener(() => Select(index));
        }
    }

    private void OnEnable() => Select(_current < 0 ? startingTab : _current);

    public void Select(int index)
    {
        if (tabPages == null || tabPages.Length == 0) return;

        index = Mathf.Clamp(index, 0, tabPages.Length - 1);
        _current = index;

        for (int i = 0; i < tabPages.Length; i++)
        {
            if (tabPages[i] != null)
                tabPages[i].SetActive(i == index);

            SetTabSprite(i, i == index);
        }
    }

    private void SetTabSprite(int index, bool selected)
    {
        if (tabButtons == null || index >= tabButtons.Length) return;

        var button = tabButtons[index];
        if (button == null) return;

        var image = button.GetComponent<Image>();
        if (image == null) return;

        var sprite = selected
            ? Get(selectedSprites, index)
            : Get(normalSprites, index);

        if (sprite != null)
            image.sprite = sprite;
    }

    private static Sprite Get(Sprite[] array, int index)
    {
        if (array == null || index < 0 || index >= array.Length) return null;
        return array[index];
    }
}
