using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.Jobs;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;
using Random = UnityEngine.Random;

public class Testing2 : MonoBehaviour
{
    [SerializeField]
    private bool useJob;
    [SerializeField]
    private Transform pfZombie;

    private List<Zombie> zombieList; //= new List<Zombie>();

    public class Zombie
    {
        public Transform transform;
        public float moveY;
    }

    private void Start()
    {
        zombieList = new List<Zombie>();
        for (int i = 0; i < 1000; i++)
        {
            Transform zombieTransform = Instantiate(pfZombie, new Vector3(Random.Range(-8f, 8f), Random.Range(-5f, 5f)), Quaternion.identity);
            zombieList.Add(new Zombie
            {
                transform = zombieTransform,
                moveY = Random.Range(1f, 2f)
            });
        }
    }

    private void Update()
    {
        float startTime = Time.realtimeSinceStartup;

        if (useJob)
        {
            NativeArray<float3> positionArray = new NativeArray<float3>(zombieList.Count, Allocator.TempJob);
            NativeArray<float> MoveYArray = new NativeArray<float>(zombieList.Count, Allocator.TempJob);
            //TransformAccessArray transformAccessArray = new TransformAccessArray(zombieList.Count);

            for (int i = 0; i < zombieList.Count; i++)
            {
                positionArray[i] = zombieList[i].transform.position;
                MoveYArray[i] = zombieList[i].moveY;
                //transformAccessArray.Add(zombieList[i].transform);
            }
            
            ReallyToughParallelJob reallyToughParallelJob = new ReallyToughParallelJob { deltaTime = Time.deltaTime, moveYArray = MoveYArray, positionArray = positionArray };

            JobHandle jobHandle = reallyToughParallelJob.Schedule(zombieList.Count, 100);
            jobHandle.Complete();
            
            
            /*ReallyToughParallelJobTransforms reallyToughParallelJobTransforms = new ReallyToughParallelJobTransforms { deltaTime = Time.deltaTime, moveYArray = MoveYArray };

            JobHandle jobHandle = reallyToughParallelJobTransforms.Schedule(transformAccessArray);
            jobHandle.Complete();*/
            

            for (int i = 0; i < zombieList.Count; i++)
            {
                zombieList[i].transform.position = positionArray[i];
                zombieList[i].moveY = MoveYArray[i];
            }

            positionArray.Dispose();
            MoveYArray.Dispose();
            //transformAccessArray.Dispose();
        }
        else
        {
            for (int i = 0; i < zombieList.Count; i++)
            {
                zombieList[i].transform.position += new Vector3(0, zombieList[i].moveY * Time.deltaTime);

                if (zombieList[i].transform.position.y > 5f)
                    zombieList[i].moveY = -math.abs(zombieList[i].moveY);

                if (zombieList[i].transform.position.y < -5f)
                    zombieList[i].moveY = +math.abs(zombieList[i].moveY);

                float value = 0f;
                for (int k = 0; k < 1000; k++)
                    value = math.exp10(math.sqrt(value));
            }
        }



        /*
        NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);

        if (useJob)
        {
            for (int i = 0; i < 10; i++)
            {
                JobHandle jobHandle = ReallyToughTaskJob();
                jobHandleList.Add(jobHandle);
            }
            JobHandle.CompleteAll(jobHandleList);
            jobHandleList.Dispose();
        }
        else
        {
            for (int i = 0; i < 10; i++)
            {
                ReallyToughTask();
            }
        }
        */

        Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + " ms");
    }

    private void ReallyToughTask()
    {
        float value = 0f;

        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }

    private JobHandle ReallyToughTaskJob()
    {
        ReallyToughJob job = new ReallyToughJob();
        return job.Schedule();
    }

    [BurstCompile]
    public struct ReallyToughJob : IJob
    {
        public void Execute()
        {
            float value = 0f;

            for (int i = 0; i < 50000; i++)
            {
                value = math.exp10(math.sqrt(value));
            }
        }
    }

    [BurstCompile]
    public struct ReallyToughParallelJob : IJobParallelFor
    {
        public NativeArray<float3> positionArray;
        public NativeArray<float> moveYArray;

        public float deltaTime;

        public void Execute(int index)
        {
            positionArray[index] += new float3(0f, moveYArray[index] * deltaTime, 0f);

            if (positionArray[index].y > 5f)
                moveYArray[index] = -math.abs(moveYArray[index]);

            if (positionArray[index].y < -5f)
                moveYArray[index] = +math.abs(moveYArray[index]);

            float value = 0f;
            for (int i = 0; i < 1000; i++)
                value = math.exp10(math.sqrt(value));
        }
    }

    [BurstCompile]
    public struct ReallyToughParallelJobTransforms : IJobParallelForTransform
    {
        public NativeArray<float> moveYArray;

        public float deltaTime;
        public void Execute(int index, TransformAccess transform)
        {
            transform.position += new Vector3(0f, moveYArray[index] * deltaTime, 0f);

            if (transform.position.y > 5f)
                moveYArray[index] = -math.abs(moveYArray[index]);

            if (transform.position.y < -5f)
                moveYArray[index] = +math.abs(moveYArray[index]);

            float value = 0f;
            for (int i = 0; i < 1000; i++)
                value = math.exp10(math.sqrt(value));

        }
    }

}
