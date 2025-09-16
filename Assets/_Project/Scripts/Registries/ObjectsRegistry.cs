using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace _Project.Scripts.Registries
{
    public class ObjectsRegistry
    {
        private readonly Dictionary<Type, IList> _typedCollections = new();

        public void Register<T>(T obj) where T : Component
        {
            var type = typeof(T);

            if (!_typedCollections.TryGetValue(type, out var list))
            {
                list = new ReactiveCollection<T>();
                _typedCollections[type] = list;
            }

            var typedList = (ReactiveCollection<T>)list;
            if (!typedList.Contains(obj))
            {
                typedList.Add(obj);
            }
        }

        public void Unregister<T>(T obj) where T : Component
        {
            var type = typeof(T);
            if (_typedCollections.TryGetValue(type, out var list))
            {
                var typedList = (ReactiveCollection<T>)list;
                if (typedList.Contains(obj))
                {
                    typedList.Remove(obj);
                }
            }
        }

        public ReactiveCollection<T> GetTypedList<T>()
        {
            var type = typeof(T);

            if (_typedCollections.TryGetValue(type, out var listObj))
            {
                return (ReactiveCollection<T>)listObj;
            }

            var newList = new ReactiveCollection<T>();
            _typedCollections[type] = newList;
            return newList;
        }
        
        public List<T> GetAllByInterface<T>() where T : class
        {
            var result = new List<T>();

            foreach (var list in _typedCollections.Values)
            {
                foreach (var obj in list)
                {
                    if (obj is T tObj)
                    {
                        result.Add(tObj);
                    }
                }
            }

            return result;
        }

        public void Clear()
        {
            foreach (var collection in _typedCollections)
            {
                collection.Value.Clear();
            }
        }
    }
}