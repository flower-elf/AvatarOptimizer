using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Anatawa12.ApplyOnPlay
{
    [InitializeOnLoad]
    public static class ApplyOnPlayCallbackRegistry
    {
        internal static readonly List<IApplyOnPlayCallback> Callbacks = new List<IApplyOnPlayCallback>();
        internal const string ENABLE_EDITOR_PREFS_PREFIX = "com.anatawa12.apply-on-play.enabled.";

        static ApplyOnPlayCallbackRegistry()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                try
                {
                    var types = assembly.GetTypes();
                    foreach (var type in types)
                    {
                        if (!typeof(IApplyOnPlayCallback).IsAssignableFrom(type)) continue;
                        var constructor = type.GetConstructor(Type.EmptyTypes);
                        if (constructor == null) continue;
                        try
                        {
                            RegisterCallback((IApplyOnPlayCallback)constructor.Invoke(Array.Empty<object>()));
                        }
                        catch (Exception e)
                        {
                            LogException($"Instantiating {type.Name}", e);
                        }
                    }
                }
                catch (Exception e)
                {
                    try
                    {
                        LogException($"Discovering types in {assembly.GetName().Name}", e);
                    }
                    catch (Exception e2)
                    {
                        LogException("Discovering types in some assembly", e, e2);
                    }
                }
            }
        }

        [PublicAPI]
        public static void RegisterCallback(IApplyOnPlayCallback callback)
        {
            Callbacks.Add(callback);
        }

        internal static IApplyOnPlayCallback[] GetCallbacks()
        {
            var copied = Callbacks
                .Where(x => EditorPrefs.GetBool(ENABLE_EDITOR_PREFS_PREFIX + x.CallbackId, true))
                .ToArray();
            Array.Sort(copied, (a, b) => a.callbackOrder.CompareTo(b.callbackOrder));
            return copied;
        }

        private static void LogException(string message, params Exception[] exceptions)
        {
            message = $"[ApplyOnPlay] {message}";

            if (exceptions.Length == 1)
            {
                Debug.LogException(new Exception(message, exceptions[0]));
            }
            else
            {
                Debug.LogException(new AggregateException(message, exceptions));
            }
        }
    }
}