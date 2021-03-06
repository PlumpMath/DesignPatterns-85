﻿using System;
using System.Collections.Generic;

namespace BlackWasp.GenericObjectPool
{
    /// <summary>
    /// See http://www.blackwasp.co.uk/GenericObjectPool.aspx
    /// This is a generic class that represents a pool of objects.
    /// A default constructor is added 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Pool<T> where T : new()
    {
        private readonly List<T> _available = new List<T>(); 
        private readonly List<T> _inUse = new List<T>();

        private Action<T> _cleanUp;

        public int MaxPoolSize { get; private set; }

        public int PoolSize
        {
            get { return _available.Count + _inUse.Count; }
        }

        public Pool(Action<T> cleanUp, int maxPoolSize)
        {
            if (cleanUp == null)
            {
                throw new ArgumentNullException("cleanUp");
            }
            _cleanUp = cleanUp;
            MaxPoolSize = maxPoolSize;
        }

        public T Get()
        {
            lock (_available)
            {
                if (_available.Count == 0)
                {
                    return AddToPool();
                }

                T obj = _available[0];
                _inUse.Add(obj);
                _available.RemoveAt(0);

                return obj;
            }
        }

        private T AddToPool()
        {
            if (PoolSize == MaxPoolSize)
            {
                throw new InvalidOperationException("Pool exhausted");
            }

            T obj = new T();
            _inUse.Add(obj);

            return obj;
        }

        public void Release(T obj)
        {
            CleanUp(obj);
            
            lock (_available)
            {
                _available.Add(obj);
                _inUse.Remove(obj);
            }
        }

        private void CleanUp(T obj)
        {
            _cleanUp(obj);
        }
    }
}
