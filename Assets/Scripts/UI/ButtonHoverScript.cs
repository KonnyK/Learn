using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonHoverScript : MonoBehaviour,ISelectHandler, IDeselectHandler, IPointerEnterHandler, IPointerExitHandler{

    [SerializeField] private Text ButtonText;
    [SerializeField] private float NormSize = 1;
    [SerializeField] private float SelectSize = 2;

    private void ChangeScale(bool Selected)
    {
        if (Selected) transform.localScale = Vector3.one * SelectSize;
        else transform.localScale = Vector3.one * NormSize;
    }

    public void Start(){ ChangeScale(false); }

    public void OnDeselect(BaseEventData eventData){ ChangeScale(false); }

    public void OnSelect(BaseEventData eventData){ ChangeScale(true); }

    public void OnPointerEnter(PointerEventData eventData){ ChangeScale(true); }

    public void OnPointerExit(PointerEventData eventData){ ChangeScale(false); }
}
