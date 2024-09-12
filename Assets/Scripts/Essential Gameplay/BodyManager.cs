using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.UIElements;

public class BodyManager : MonoBehaviour
{
    /*
    This is the initialization and main manager script
    - think of it as the main() function in a console app
     */

    [SerializeField] private float cameraFocusZoom;
    [SerializeField] private float cameraMapZoom;

    public bool Focused = false;
    public List<BodySection> BodySections = new List<BodySection>();
    public LayerMask BodyLayerMask;

    [SerializeField] private BodySection focusedSection;

    void Start()
    {
        focusedSection = null;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) OnClick();    // TODO: use a keydown event somehow (TJL)
        if (Input.GetKeyDown(KeyCode.Escape)) UnfocusBodySection();
    }

    /*
     * DOC: OnClick occurs every time the player clicks the left mouse button. 
     * Currently it focuses a body part, if not already focused (by calling FocusBodySection).
     * (TJL)
     */
    private void OnClick()
    {
        //Debug.Log("Click");

        if (Focused) return;        // Saves us from doing raycasts if we're already focused
                                    // TODO: delegate this to the individual sections if perf is a concern (TJL)
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero);

        if (hit)
        {
            //Debug.Log("Hit");
            BodySection section = hit.transform.GetComponent<BodySection>();
            if (section == null) return;
            FocusBodySection(section);
        }
    }


    /*
     * DOC: FocusBodySection occurs when the player clicks on a valid body section.
     * TODO: focusing anim
     * (TJL)
     */
    private void FocusBodySection(BodySection section)
    {
        focusedSection = section;
        section.Focus();
        Focused = true;

        Vector3 cameraPosition = focusedSection.transform.position;
        cameraPosition.z = -10f;    // Default camera distance, do not change (TJL)
        Camera.main.transform.position = cameraPosition;
        Camera.main.orthographicSize = cameraFocusZoom;
    }


    /*
     * DOC: UnfocusBodySection occurs when the player presses escape.
     *  UnfocusBodySection does nothing if unfocused already.
     *  TODO: defocusing anim
     * (TJL)
     */
    private void UnfocusBodySection()
    {
        if (!Focused) return;
        focusedSection.Unfocus();
        focusedSection = null;
        Focused = false;

        Vector3 cameraPosition = new Vector3(0f, 0f, -10f);     // Default cam position, do not change (TJL)
        Camera.main.transform.position = cameraPosition;
        Camera.main.orthographicSize = cameraMapZoom;
    }
    
}
