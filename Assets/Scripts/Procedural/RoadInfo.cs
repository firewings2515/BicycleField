using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class RoadInfo : MonoBehaviour
{
    NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //JobHandle.CompleteAll(jobHandleList);
        //jobHandleList.Clear();
        //jobHandleList = new NativeList<JobHandle>(Allocator.Temp);
    }

    public void addJob(JobHandle job)
    {
        jobHandleList.Add(job);
    }
}
