﻿using System.Collections.Generic;
using UnityEngine;

namespace OpenTween
{
    internal static class TweenPool<T> where T : TweenBase<T>, new()
    {
        private static List<T> _activeTweens;
        private static Stack<T> _poolTweens;

        public static T GetNew()
        {
            if (_poolTweens == null)
            {
                _activeTweens = new List<T>(64000);
                _poolTweens = new Stack<T>(10);
                TweenUpdater.RegisterUpdate(Update);
            }

            T t = _poolTweens.Count > 0 ? _poolTweens.Pop() : new T();
            
            if(OpenTweenSettings.CaptureCreationStacktrace)
                t.CreationStacktrace = StackTraceUtility.ExtractStackTrace();

            _activeTweens.Add(t);
            return t;
        }

        public static void Return(T instance)
        {
            _activeTweens.Remove(instance);
            _poolTweens.Push(instance);
        }

        private static void Update(float dt)
        {
            if (_activeTweens == null)
                return;
            int count = _activeTweens.Count;
            for (int i = 0; i < count; i++)
            {
                _activeTweens[i].Update(dt, false);
            }
        }
    }
}