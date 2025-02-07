﻿using Unity.Burst;
using Unity.Burst.CompilerServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
#if UNITY_COLLECTIONS
using NativeListInt = Unity.Collections.NativeList<int>;

#else
using NativeListInt = OpenTween.Jobs.NativeList<int>;
#endif

namespace OpenTween.Jobs
{
    [BurstCompile]
    internal struct ColorLerp : IJobParallelFor
    {
        [NativeDisableParallelForRestriction] [ReadOnly]
        public NativeArray<TweenOptions<Color>> Options;

        [ReadOnly] [NativeDisableParallelForRestriction]
        public NativeListInt Indices;

        [NativeDisableParallelForRestriction] public NativeArray<TweenInternal<Color>> Array;

        public unsafe void Execute(int i)
        {
            int index = Indices[i];
            
            ref TweenInternal<Color> t = ref UnsafeUtility.ArrayElementAsRef<TweenInternal<Color>>(Array.GetUnsafePtr(), index);

            if (Hint.Unlikely(!t.IsUpdatedInLastFrame))
                return;

            ref TweenOptions<Color> options = ref UnsafeUtility.ArrayElementAsRef<TweenOptions<Color>>(Options.GetUnsafePtr(), index);
            
            Color s = options.Start;
            Color e = options.End;
            var start = new float4(s.r, s.g, s.b, s.a);
            var end = new float4(e.r, e.g, e.b, e.a);

            end = options.IsRelative ? start + end : end;

            float4 result = options.IsFrom
                ? math.lerp(end, start, t.LerpParameter)
                : math.lerp(start, end, t.LerpParameter);

            t.CurrentValue = new Color(result.x, result.y, result.z, result.w);
        }
    }
}