using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;
using Pamux.Lib.Procedural.Models;

namespace Pamux.Lib.Procedural.Utilities
{
    public class ThreadedDataRequester : MonoBehaviour
    {
        private static ThreadedDataRequester instance;
        private Queue<ThreadInfo> dataQueue = new Queue<ThreadInfo>();

        void Awake()
        {
            instance = FindObjectOfType<ThreadedDataRequester>();
        }

        public static void RequestData(Func<object> generateData, Action<object> callback)
        {
            ThreadStart threadStart = delegate
            {
                instance.DataThread(generateData, callback);
            };

            new Thread(threadStart).Start();
        }

        void DataThread(Func<object> generateData, Action<object> callback)
        {
            var data = generateData();
            lock (dataQueue)
            {
                dataQueue.Enqueue(new ThreadInfo(callback, data));
            }
        }

        void Update()
        {
            if (dataQueue.Count > 0)
            {
                for (var i = 0; i < dataQueue.Count; ++i)
                {
                    var threadInfo = dataQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
            }
        }        
    }
}