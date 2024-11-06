using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempCellManager : MonoBehaviour
{
    public List<Cell> cells;
    public List<Cell> bacteria;


    // Start is called before the first frame update
    void Start()
    {
        foreach (Cell cell in cells)
        {
            cell.ActivateCell(cell.gameObject.transform.position);
        }
        cells[0].NewTask(5, bacteria[0].gameObject);
        cells[1].NewTask(5, bacteria[1].gameObject);
        cells[2].NewTask(-3);
        foreach (Cell cell in bacteria)
        {
            cell.ActivateCell(cell.gameObject.transform.position);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
