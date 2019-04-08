using System;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Scripts.Utils
{
    public static class Extensions
    {
        public static Color SetAlpha(this Color color, float value)
        {
            return new Color(color.r, color.g, color.b, value);
        }

        public static ISet<T> ToSet<T>(this IEnumerable<T> iEnumerable)
        {
            return new HashSet<T>(iEnumerable);
        }

        public static T GetOrAddComponent<T>(this Component c) where T : Component
        {
            return c.gameObject.GetOrAddComponent<T>();
        }

        public static Component GetOrAddComponent(this GameObject go, Type componentType)
        {
            var result = go.GetComponent(componentType);
            return result == null ? go.AddComponent(componentType) : result;
        }

        public static T GetOrAddComponent<T>(this GameObject go) where T : Component
        {
            return GetOrAddComponent(go, typeof(T)) as T;
        }
    }
}