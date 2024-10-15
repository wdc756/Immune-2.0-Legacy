using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolingHelper : MonoBehaviour
{

    [SerializeField, Tooltip("The object to pool; must be assigned or else script will do nothing")]
    private GameObject poolObject;

    [SerializeField, Tooltip("The amount of objects to instatiate; must be greater than 1")]
    private int poolCount = 0;

    //used when GenerateOnOverflow is true to check if the current count is greater than 2 * the original
    private int originalCount;

    [SerializeField, Tooltip("The List of gameobjects in the pool")]
    private List<GameObject> pooledObjects = new List<GameObject>();

    [SerializeField, Tooltip("Will output a debug warning to the console when the list runs out of gameObjects")]
    private bool WarnOnOverflow = false;
    [SerializeField, Tooltip("Will generate a new object when the list runs out, expanding the list")]
    private bool GenerateOnOverflow = false;

    //determines if the script should do anything
    private bool isEnabled = true;

    void Start()
    {
        InitializeScriptVariables();
    }

    void InitializeScriptVariables()
    {
        if (poolObject == null || poolCount <= 1)
        {
            Debug.Log(gameObject.name + "'s ObjectPoolingHelper is missing inputs or the inputs are invalid. Disabling this script");
            isEnabled = false;
        }
        else
        {
            originalCount = poolCount;

            for (int i = 0; i < poolCount; i++)
            {
                pooledObjects.Add(Instantiate(poolObject));
                pooledObjects[i].SetActive(false);
            }
        }
    }

    //finds the next object not currently in use; in use here just means its active in the scene
    public GameObject GetNextObject()
    {
        if (isEnabled)
        {
            for (int i = 0; i < poolCount; i++)
            {
                if (!pooledObjects[i].activeInHierarchy)
                {
                    return pooledObjects[i];
                }
            }

            if (WarnOnOverflow)
            {
                Debug.LogWarning("Not enough objects in " + gameObject.name + "'s ObjectPoolingHelper List");
            }

            if (GenerateOnOverflow)
            {
                pooledObjects.Add(Instantiate(poolObject));
                pooledObjects[poolCount - 1].SetActive(true);
                poolCount++;

                if (poolCount > 2 * originalCount)
                {
                    Debug.LogWarning(gameObject.name + "'s ObjectPoolingHelper has generated double the number of objects originally stated");
                }
            }

            //if nothing found, then all the objects are being used
            return null;
        }
        else
        {
            Debug.LogWarning("Call was made on GetNextObject() in ObjectPoolingHelper attached to " + gameObject.name + " when the script is disabled");
            return null;
        }
    }

    public void SetGameObjectInList(GameObject newObject, int index)
    {
        if (isEnabled)
        {
            if (-1 < index && index < poolCount)
            {
                Destroy(pooledObjects[index]);
                pooledObjects[index] = newObject;
                pooledObjects[index].SetActive(false);
            }
            else
            {
                Debug.LogWarning("Call was made on SetGameObjectInList() in ObjectPoolingHelper attached to " + gameObject.name 
                    + " when index " + index + " is invalid");
            }
        }
        else
        {
            Debug.LogWarning("Call was made on SetGameObjectInList() in ObjectPoolingHelper attached to " + gameObject.name + " when the script is disabled");
        }
    }

    public void ResetGameObjectInList(int index)
    {
        if (isEnabled)
        {
            if (-1 < index && index < poolCount)
            {
                Destroy(pooledObjects[index]);
                pooledObjects[index] = Instantiate(poolObject);
                pooledObjects[index].SetActive(false);
            }
            else
            {
                Debug.LogWarning("Call was made on SetGameObjectInList() in ObjectPoolingHelper attached to " + gameObject.name
                    + " when index " + index + " is invalid");
            }
        }
        else
        {
            Debug.LogWarning("Call was made on ResetGameObjectInList() in ObjectPoolingHelper attached to " + gameObject.name + " when the script is disabled");
        }
    }



    public void SetPoolCount(int newCount, bool resetAll)
    {
        if (1 < newCount)
        {            
            if (resetAll)
            {
                poolCount = newCount;
                pooledObjects.Clear();
                InitializeScriptVariables();
            }
            else
            {
                if (poolCount < newCount)
                {
                    for (int i = 0; i < newCount - poolCount; i++)
                    {
                        pooledObjects.Add(Instantiate(poolObject));
                        pooledObjects[i].SetActive(false);
                    }
                }
                else
                {
                    pooledObjects.RemoveRange(newCount, poolCount - newCount);
                }

                poolCount = newCount;
            }
        }
        else
        {
            Debug.LogWarning("Invalid int for newCount: " + newCount + " in SetPoolCount on ObjectPoolingHelper attached to " + gameObject.name);
        }
    }

    //use this to re-enable the script when disabled
    public void ResetObjectPoolList()
    {
        if (isEnabled)
        {
            InitializeScriptVariables();
        }
        else
        {
            Debug.LogWarning("Call was made on ResetObjectPoolingHelper() in ObjectPoolingHelper attached to " + gameObject.name + " when the script is disabled");
        }
    }
}
