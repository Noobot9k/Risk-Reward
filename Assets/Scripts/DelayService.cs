using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class Delay {
    public delegate void DelegateType();
    static public IEnumerable Create(DelegateType function, float delay) {
        //StartCoroutine(fireFunctionWithDelay());
        yield return new WaitForSeconds(delay);
        function();
    }
    static IEnumerable fireFunctionWithDelay(DelegateType function, float delay) {
        yield return new WaitForSeconds(delay);
        function();
    }
}

public class DelayService : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
