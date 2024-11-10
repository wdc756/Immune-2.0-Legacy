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
        for (int i = 0; i < cells.Count; i++)
        {
            Cell cell = cells[i];
            cell.ActivateCell(cell.gameObject.transform.position);
            cell.NewTask(5, bacteria[i].gameObject);
            cell.NewTask(-1);
        }
        cells[0].NewTask(5, bacteria[4].gameObject);
        cells[0].NewTask(-1);
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
