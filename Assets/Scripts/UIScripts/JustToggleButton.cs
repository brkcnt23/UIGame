using Unity;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class JustToggleButton : MonoBehaviour
{
    [HideInInspector]
    public Button Button;
    public bool panelState;

    public GameObject Panel;

    private void Start(){

        //Button = gameObject.GetComponent<Button>();
        Panel = gameObject.transform.GetChild(0).gameObject;

        if(Panel.activeInHierarchy)
            panelState = true;
        else
            panelState = false;
    }
    public void ToggleOpenClosePanel(){
        if(panelState){
            Panel.SetActive(false);
        }
        else if(!panelState){
            Panel.SetActive(true);
        }
    }
}