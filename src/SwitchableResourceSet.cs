﻿using System;
using System.Collections.Generic;

namespace SimpleFuelSwitch
{
    /// <summary>
    /// Represents the full set of switchable resources available for a part.
    /// </summary>
    class SwitchableResourceSet
    {
        // The primary lookup that stores resources by unique ID.
        private readonly Dictionary<string, SwitchableResource[]> resourcesById = new Dictionary<string, SwitchableResource[]>();

        // Stores resource IDs in the order they're added to the set.
        private readonly List<string> orderedResourceIds = new List<string>();

        // Maps from resource IDs to user-friendly display names for them.
        private readonly Dictionary<string, string> displayNamesById = new Dictionary<string, string>();

        // The resourcesId of the default option, if known.
        private string defaultResourcesId = null;

        // How the "pick a resource set" field is labeled in the editor UI, e.g. "Fuel Type" or whatever.
        public readonly string selectorFieldName;

        /// <summary>
        /// Static map set up at program load time.  Key is part name, value is the set
        /// of switchable resource options available for the part.
        /// </summary>
        private static readonly Dictionary<string, SwitchableResourceSet> resourcesByPartName
            = new Dictionary<string, SwitchableResourceSet>();

        private SwitchableResourceSet(string selectorFieldName)
        {
            this.selectorFieldName = selectorFieldName;
        }

        /// <summary>
        /// Add a set of resources for the specified part.
        /// </summary>
        /// <param name="partName"></param>
        /// <param name="resourcesId"></param>
        /// <param name="displayName"></param>
        /// <param name="selectorFieldName"></param>
        /// <param name="isDefault"></param>
        /// <param name="resources"></param>
        public static void Add(
            string partName,
            string resourcesId,
            string displayName,
            string selectorFieldName,
            bool isDefault,
            SwitchableResource[] resources)
        {
            SwitchableResourceSet set = null;
            if (!resourcesByPartName.TryGetValue(partName, out set))
            {
                set = new SwitchableResourceSet(selectorFieldName);
                resourcesByPartName.Add(partName, set);
            }
            set.Add(resourcesId, displayName, resources);
            if (isDefault && (set.defaultResourcesId == null)) set.defaultResourcesId = resourcesId;
        }

        /// <summary>
        /// Get the resource set for the specified part, or null if there isn't one.
        /// </summary>
        /// <param name="partName"></param>
        /// <returns></returns>
        public static SwitchableResourceSet ForPart(string partName)
        {
            SwitchableResourceSet result;
            if (!resourcesByPartName.TryGetValue(partName, out result)) return null;
            return result;
        }

        private void Add(string resourcesId, string displayName, SwitchableResource[] resources)
        {
            if (resourcesById.ContainsKey(resourcesId))
            {
                throw new ArgumentException("Duplicate resourcesId " + resourcesId);
            }
            resourcesById[resourcesId] = resources;
            displayNamesById[resourcesId] = displayName;
            orderedResourceIds.Add(resourcesId);
        }

        /// <summary>
        /// Given a resources ID, get the corresponding set of resources.
        /// </summary>
        /// <param name="resourcesId"></param>
        /// <returns></returns>
        public SwitchableResource[] this[string resourcesId]
        {
            get
            {
                SwitchableResource[] results;
                if (!resourcesById.TryGetValue(resourcesId, out results))
                {
                    Logging.Warn("No resources found for " + resourcesId + ", returning null");
                    return null;
                }
                return results;
            }
        }

        /// <summary>
        /// Given a resources ID, get the user-friendly display name for it.
        /// </summary>
        /// <param name="resourcesId"></param>
        /// <returns></returns>
        public string DisplayNameOf(string resourcesId)
        {
            string result;
            if (!displayNamesById.TryGetValue(resourcesId, out result)) return null;
            return result;
        }

        /// <summary>
        /// Given a resources ID, get the ID of the "next" one (used for controlling
        /// the order in which things cycle in PAW's UI.
        /// </summary>
        /// <param name="resourcesId"></param>
        /// <returns></returns>
        public string NextResourcesId(string resourcesId)
        {
            int foundIndex = -1;
            for (int i = 0; i < orderedResourceIds.Count; ++i)
            {
                if (orderedResourceIds[i] == resourcesId)
                {
                    foundIndex = i;
                    break;
                }
            }
            if (foundIndex < 0) return null; // no such resourcesId
            foundIndex++; // move on to the next one
            if (foundIndex >= orderedResourceIds.Count) foundIndex = 0;
            return orderedResourceIds[foundIndex];
        }

        /// <summary>
        /// Gets the default resources ID (i.e. the first one displayed for a newly instantiated part).
        /// </summary>
        public string DefaultResourcesId
        {
            get
            {
                if (defaultResourcesId == null)
                {
                    // no explicit default was specified, so pick the first one
                    return (orderedResourceIds.Count > 0) ? orderedResourceIds[0] : null;
                }
                else
                {
                    return defaultResourcesId;
                }
            }
        }
    }
}
