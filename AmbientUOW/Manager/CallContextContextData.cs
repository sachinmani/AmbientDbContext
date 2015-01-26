using System;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;

namespace AmbientDbContext.Manager
{
    internal class CallContextContextData
    {
        private static readonly string ObjectIdentifier = "AmbientPropertyIdentifer_" + Guid.NewGuid();

        //Enables compilers to dynamically attach object fields to managed objects.
        //The ConditionalWeakTable<TKey, TValue> class differs from other collection objects in its management of the object lifetime of keys stored in the collection.
        //Ordinarily, when an object is stored in a collection, its lifetime lasts until it is removed (and there are no additional references to the object) or until 
        //the collection object itself is destroyed. However, in the ConditionalWeakTable<TKey, TValue> class, adding a key/value pair to the table does not ensure that 
        //the key will persist, even if it can be reached directly from a value stored in the table (for example, if the table contains one key, A, with a value V1, and 
        //a second key, B, with a value P2 that contains a reference to A). Instead, ConditionalWeakTable<TKey, TValue> automatically removes the key/value entry as soon 
        //as no other references to a key exist outside the table.
        private static readonly ConditionalWeakTable<ContextKey, ContextData> ConditionalWeakTable = new ConditionalWeakTable<ContextKey, ContextData>();

        /// <summary>
        /// Add the contextData to the weak table with key as the context key. Also add the context key to the callcontext with ObjectIdentifier as key.
        /// </summary>
        internal static void AddContextData(ContextKey contextKey, ContextData contextData)
        {
            ContextData existingcontextData;
            CallContext.LogicalSetData(ObjectIdentifier, contextKey);
            if (ConditionalWeakTable.TryGetValue(contextKey, out existingcontextData))
            {
               return;
            }
            ConditionalWeakTable.Add(contextKey, contextData);
        }

        /// <summary>
        /// Check to see whether any contextdata exists in the callcontext else return null.
        /// </summary>
        /// <returns></returns>
        internal static ContextData GetContextData()
        {
            ContextData contextData= null;
            var contextKey = CallContext.LogicalGetData(ObjectIdentifier) as ContextKey;
            if (contextKey != null)
            {
                ConditionalWeakTable.TryGetValue(contextKey, out contextData);
            }
            return contextData;
        }

        /// <summary>
        /// Remove the context data from the CallContext and the WeakTable.
        /// </summary>
        internal static void RemoveContextData()
        {
            var contextKey2 = CallContext.LogicalGetData(ObjectIdentifier) as ContextKey;
            if (contextKey2 != null)
            {
                ConditionalWeakTable.Remove(contextKey2);
                CallContext.LogicalSetData(ObjectIdentifier, null);
            }
        }
    }
}
