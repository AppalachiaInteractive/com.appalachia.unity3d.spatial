#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Appalachia.Utility.Extensions;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

#endregion

namespace Appalachia.Spatial
{
    public class SimpleGridPositioner : MonoBehaviour
    {
        public enum GridObjectTypes
        {
            All,
            Tree,
            LODGroup,
            MeshFilter,
            Camera,
            Light,
            Grid
        }

        [DisableIf(nameof(locked))]
        public bool adoptGroundNormal = true;

        [DisableIf(nameof(locked))]
        public bool centered = true;

        [DisableIf(nameof(locked))]
        public bool childrenOnly = true;

        [DisableIf(nameof(locked))]
        public bool findLOD;

        [DisableIf(nameof(locked))]
        public bool ignoreSelf = true;

        public bool locked;

        [DisableIf(nameof(locked))]
        public bool lockedToGround = true;

        [DisableIf(nameof(locked))]
        public bool oneLevelOnly = true;

        [DisableIf(nameof(locked))]
        public bool randomYRotation = true;

        [DisableIf(nameof(locked))]
        public bool randomYRotationIsConsumable = true;

        [DisableIf(nameof(locked))]
        public bool yToZero = true;

        [Range(.01F, 1000F)]
        [DisableIf(nameof(locked))]
        public float gridCellSize = 15f;

        [ShowIf(nameof(findLOD))]
        [DisableIf(nameof(locked))]
        public float lodGridSize = 10f;

        [ListDrawerSettings(HideAddButton = true)]
        [SceneObjectsOnly]
        [DisableIf(nameof(locked))]
        public GameObject[] objectsToCenter = new GameObject[0];

        [DisableIf(nameof(locked))]
        public SortMode sortMode = SortMode.Name;

        [DisableIf(nameof(locked))]
        public Vector3 localOffset;

        #region Menu Items

#if UNITY_EDITOR

        [UnityEditor.MenuItem(
            PKG.Menu.GameObjects.Base + "Add SimpleGridPositioner",
            priority = PKG.Priority - 10
        )]
        [UnityEditor.MenuItem(
            PKG.Menu.Appalachia.Components.Base + "Add SimpleGridPositioner",
            priority = PKG.Priority - 10
        )]
        public static void AddSimpleGridPositioner(UnityEditor.MenuCommand menuCommand)
        {
            //Create the spawner
            var gridGo = new GameObject("Simple Grid");
            gridGo.AddComponent<SimpleGridPositioner>();

            var parent = menuCommand.context as GameObject;
            if (parent != null)
            {
                // Reparent it
                UnityEditor.GameObjectUtility.SetParentAndAlign(gridGo, parent);
            }

            // Register the creation in the undo system
            UnityEditor.Undo.RegisterCreatedObjectUndo(gridGo, "Created " + gridGo.name);

            //Make it active
            UnityEditor.Selection.activeObject = gridGo;
        }
#endif

        #endregion

        #region Event Functions

        private void Awake()
        {
            /*
            if (PrefabUtility.IsPartOfPrefabAsset(this))
            {
                for (var i = 0; i < objectsToCenter.Length; i++)
                {
                    var objectToCenter = objectsToCenter[i];

                    if (PrefabUtility.IsAnyPrefabInstanceRoot(objectToCenter))
                    {
                        objectsToCenter[i] = AssetDatabaseManager.GetPrefabAsset(objectToCenter);
                    }
                }
            }
            else
            {
                for (var i = 0; i < objectsToCenter.Length; i++)
                {
                    var objectToCenter = objectsToCenter[i];

                    if (PrefabUtility.IsPartOfPrefabAsset(objectToCenter))
                    {
                        objectsToCenter[i] = objectToCenter.InstantiatePrefab();
                    }
                }
                
                BuildGrid();
            }
            */
        }

        #endregion

        [Button]
        [DisableIf(nameof(locked))]
        public void BuildGrid()
        {
            GameObject[][] centeringCollection;

            if (findLOD)
            {
                var centering = new List<List<GameObject>>();

                foreach (var obj in objectsToCenter.OrderBy(obj => obj.name))
                {
                    var lowerName = obj.name.ToLowerInvariant();
                    var styles = new[] {"lod{0}", "lod_{0}", "lod-{0}"};
                    var matched = false;
                    var matchingLod = 0;
                    var matchingStyle = 0;

                    for (var i = 0; i < styles.Length; i++)
                    {
                        for (var j = 0; j < 5; j++)
                        {
                            var stringToCheck = string.Format(styles[i], j.ToString());

                            if (lowerName.Contains(stringToCheck))
                            {
                                matched = true;
                                matchingLod = j;
                                matchingStyle = i;
                                break;
                            }
                        }

                        if (matched)
                        {
                            break;
                        }
                    }

                    if (!matched || (matched && (matchingLod == 0)))
                    {
                        centering.Add(new List<GameObject> {obj});
                        continue;
                    }

                    for (var i = 0; i < centering.Count; i++)
                    {
                        var stringToCheck = string.Format(styles[matchingStyle], matchingLod.ToString());
                        var lod0String = string.Format(styles[matchingStyle],    0);

                        var searchTerm = lowerName.Replace(stringToCheck, lod0String).ToLower();
                        if ((centering[i] != null) && (centering[i][0].name.ToLower() == searchTerm))
                        {
                            centering[i].Add(obj);
                        }
                    }
                }

                centeringCollection = new GameObject[centering.Count][];

                for (var i = 0; i < centering.Count; i++)
                {
                    centeringCollection[i] = new GameObject[centering[i].Count];
                    for (var j = 0; j < centering[i].Count; j++)
                    {
                        centeringCollection[i][j] = centering[i][j];
                    }
                }
            }
            else
            {
                centeringCollection = new GameObject[objectsToCenter.Length][];
                for (var i = 0; i < objectsToCenter.Length; i++)
                {
                    centeringCollection[i] = new[] {objectsToCenter[i]};
                }
            }

            var cubeSize = 1;

            while ((cubeSize * cubeSize) < centeringCollection.Length)
            {
                cubeSize += 1;
            }

            var startingX = centered ? (-cubeSize * gridCellSize) / 2 : 0;
            var startingY = 0f;
            var startingZ = centered ? (-cubeSize * gridCellSize) / 2 : 0;

            var currentX = startingX;
            var currentZ = startingZ;

            var breakout = false;

            for (var i = 0; i < cubeSize; i++)
            {
                if (breakout)
                {
                    break;
                }

                for (var j = 0; j < cubeSize; j++)
                {
                    var iteration = (i * cubeSize) + j;

                    if (iteration >= centeringCollection.Length)
                    {
                        breakout = true;
                        break;
                    }

                    var originalY = centeringCollection[iteration][0].transform.localPosition.y;

                    var position = new Vector3(currentX, yToZero ? startingY : originalY, currentZ);

                    var transform1 = transform;
                    var finalPosition = transform1.position + position;
                    var finalRotation = transform1.rotation;

                    if (lockedToGround)
                    {
                        var rayOrigin = finalPosition;

                        rayOrigin.y = 1024;

                        var ray = new Ray(rayOrigin, Vector3.down);

                        var hits = new RaycastHit[12];

                        var hitCount = Physics.RaycastNonAlloc(ray, hits);

                        for (var h = 0; h < hitCount; h++)
                        {
                            var hit = hits[h];

                            if (hit.collider is TerrainCollider t)
                            {
                                finalPosition.y = hit.point.y;
                                var terrainNormal = hit.normal;

                                if (adoptGroundNormal)
                                {
                                    Vector3 lookForward;

                                    if (terrainNormal != Vector3.up)
                                    {
                                        var terrainRight = math.cross(terrainNormal, Vector3.up);
                                        lookForward = math.cross(terrainNormal, terrainRight);
                                        finalRotation = Quaternion.LookRotation(lookForward, terrainNormal);
                                    }
                                }
                            }
                        }
                    }

                    if (randomYRotation)
                    {
                        finalRotation *= Quaternion.Euler(0.0f, Random.Range(-180f, 180f), 0.0f);
                    }

                    centeringCollection[iteration][0].transform.position = finalPosition + localOffset;
                    centeringCollection[iteration][0].transform.rotation = finalRotation;

                    var rb = centeringCollection[iteration][0].transform.GetComponent<Rigidbody>();

                    if (rb != null)
                    {
                        rb.velocity = Vector3.zero;
                        rb.angularVelocity = Vector3.zero;
                    }

                    if (centeringCollection[iteration].Length > 1)
                    {
                        for (var k = 1; k < centeringCollection[iteration].Length; k++)
                        {
                            var pos = finalPosition + new Vector3(lodGridSize * k, 0, lodGridSize * k);

                            centeringCollection[iteration][k].transform.position = pos;

                            rb = centeringCollection[iteration][k].transform.GetComponent<Rigidbody>();

                            if (rb != null)
                            {
                                rb.velocity = Vector3.zero;
                                rb.angularVelocity = Vector3.zero;
                            }
                        }
                    }

                    currentZ += gridCellSize;
                }

                currentZ = startingZ;
                currentX += gridCellSize;
            }

            if (randomYRotationIsConsumable)
            {
                randomYRotation = false;
            }
        }

        [Button(Expanded = true, Name = "Clear Objects", Style = ButtonStyle.Box)]
        [DisableIf(nameof(locked))]
        public void ClearObjectsOfType(GridObjectTypes type)
        {
            switch (type)
            {
                case GridObjectTypes.All:
                    objectsToCenter = new GameObject[0];
                    break;
                case GridObjectTypes.Tree:
                    objectsToCenter = objectsToCenter.Where(obj => obj.GetType() != typeof(Tree)).ToArray();
                    break;
                case GridObjectTypes.LODGroup:
                    objectsToCenter = objectsToCenter.Where(obj => obj.GetType() != typeof(LODGroup))
                                                     .ToArray();
                    break;
                case GridObjectTypes.MeshFilter:
                    objectsToCenter = objectsToCenter.Where(obj => obj.GetType() != typeof(MeshFilter))
                                                     .ToArray();
                    break;
                case GridObjectTypes.Camera:
                    objectsToCenter = objectsToCenter.Where(obj => obj.GetType() != typeof(Camera)).ToArray();
                    break;
                case GridObjectTypes.Light:
                    objectsToCenter = objectsToCenter.Where(obj => obj.GetType() != typeof(Light)).ToArray();
                    break;
                case GridObjectTypes.Grid:
                    objectsToCenter = objectsToCenter
                                     .Where(obj => obj.GetType() != typeof(SimpleGridPositioner))
                                     .ToArray();
                    break;
            }
        }

        [Button(Name = "Clear, Search, Build")]
        [DisableIf(nameof(locked))]
        public void ClearSearchBuild()
        {
            if (locked)
            {
                return;
            }

            ClearObjectsOfType(GridObjectTypes.All);
            SearchForObjects(GridObjectTypes.All);
            BuildGrid();
        }

        [Button(Name = "Clear, Search, Build (All In Scene)")]
        [DisableIf(nameof(locked))]
        public void ClearSearchBuildAll()
        {
            var comps = FindObjectsOfType<SimpleGridPositioner>();

            foreach (var obj in comps)
            {
                if (obj.locked)
                {
                    continue;
                }

                obj.ClearSearchBuild();
            }
        }

        [Button(Name = "Clear, Search, Sort, Build")]
        [DisableIf(nameof(locked))]
        public void ClearSearchSortBuild()
        {
            if (locked)
            {
                return;
            }

            ClearObjectsOfType(GridObjectTypes.All);
            SearchForObjects(GridObjectTypes.All);
            SortChildren();
            BuildGrid();
        }

        [Button(Name = "Clear, Search, Sort, Build (All In Scene)")]
        [DisableIf(nameof(locked))]
        public void ClearSearchSortBuildAll()
        {
            var comps = FindObjectsOfType<SimpleGridPositioner>();

            foreach (var obj in comps)
            {
                if (obj.locked)
                {
                    continue;
                }

                obj.ClearSearchSortBuild();
            }
        }

        [Button(Expanded = true, Name = "Search For Objects", Style = ButtonStyle.Box)]
        [DisableIf(nameof(locked))]
        public void SearchForObjects(GridObjectTypes type)
        {
            GameObject[] gameObjects = null;

            switch (type)
            {
                case GridObjectTypes.All:
                    gameObjects = childrenOnly
                        ? transform.GetComponentsInChildren<Transform>().Select(t => t.gameObject).ToArray()
                        : FindObjectsOfType<Transform>().Select(t => t.gameObject).ToArray();
                    break;
                case GridObjectTypes.Tree:
                    gameObjects = childrenOnly
                        ? transform.GetComponentsInChildren<Tree>().Select(t => t.gameObject).ToArray()
                        : FindObjectsOfType<Tree>().Select(t => t.gameObject).ToArray();
                    break;
                case GridObjectTypes.LODGroup:
                    gameObjects = childrenOnly
                        ? transform.GetComponentsInChildren<LODGroup>().Select(t => t.gameObject).ToArray()
                        : FindObjectsOfType<LODGroup>().Select(t => t.gameObject).ToArray();
                    break;
                case GridObjectTypes.MeshFilter:
                    gameObjects = childrenOnly
                        ? transform.GetComponentsInChildren<MeshFilter>().Select(t => t.gameObject).ToArray()
                        : FindObjectsOfType<MeshFilter>().Select(t => t.gameObject).ToArray();
                    break;
                case GridObjectTypes.Camera:
                    gameObjects = childrenOnly
                        ? transform.GetComponentsInChildren<Camera>().Select(t => t.gameObject).ToArray()
                        : FindObjectsOfType<Camera>().Select(t => t.gameObject).ToArray();
                    break;
                case GridObjectTypes.Light:
                    gameObjects = childrenOnly
                        ? transform.GetComponentsInChildren<Light>().Select(t => t.gameObject).ToArray()
                        : FindObjectsOfType<Light>().Select(t => t.gameObject).ToArray();
                    break;
                case GridObjectTypes.Grid:
                    gameObjects = childrenOnly
                        ? transform.GetComponentsInChildren<SimpleGridPositioner>()
                                   .Select(t => t.gameObject)
                                   .ToArray()
                        : FindObjectsOfType<SimpleGridPositioner>().Select(t => t.gameObject).ToArray();
                    break;
            }

            if (gameObjects == null)
            {
                return;
            }

            var hash = new HashSet<GameObject>();

            foreach (var obj in objectsToCenter)
            {
                if ((obj == null) ||
                    (oneLevelOnly && (obj.transform.parent != transform)) ||
                    (ignoreSelf && (obj.transform == transform)))
                {
                    continue;
                }

                hash.Add(obj);
            }

            foreach (var obj in gameObjects)
            {
                if ((obj == null) ||
                    (oneLevelOnly && (obj.transform.parent != transform)) ||
                    (ignoreSelf && (obj.transform == transform)))
                {
                    continue;
                }

                hash.Add(obj);
            }

            objectsToCenter = hash.ToArray();
        }

        [Button]
        [DisableIf(nameof(locked))]
        public void SortChildren()
        {
            switch (sortMode)
            {
                case SortMode.Name:
                    SortChildrenAlphabetically();
                    break;
                case SortMode.Volume:
                    SortChildrenByVolume();
                    break;

                case SortMode.Bounds:
                case SortMode.Width:
                case SortMode.Height:
                    SortChildrenByBounds(sortMode);
                    break;
                case SortMode.Mass:
                    SortChildrenByComponentsDetail<Rigidbody, float>(
                        rigidbody1 => rigidbody1.mass,
                        (a, b) => a + b
                    );
                    break;
                case SortMode.Components:
                    SortChildrenByComponents<Component>();
                    break;
                case SortMode.Colliders:
                    SortChildrenByComponents<Collider>();
                    break;
                case SortMode.Renderers:
                    SortChildrenByComponents<Renderer>();
                    break;
                case SortMode.LODLevels:
                    SortChildrenByComponentsDetail<LODGroup, int>(
                        lod => lod.GetLODs().Length,
                        (a, b) => a + b
                    );
                    break;
                case SortMode.Vertices:
                    SortChildrenByComponentsDetail<MeshFilter, int>(
                        meshFilter => meshFilter.sharedMesh.vertexCount,
                        Mathf.Max
                    );
                    break;
                case SortMode.Triangles:
                    SortChildrenByComponentsDetail<MeshFilter, int>(
                        meshFilter => meshFilter.sharedMesh.triangles.Length / 3,
                        Mathf.Max
                    );
                    break;
                case SortMode.VerticesLOD0:
                    SortChildrenByComponentsDetail<LODGroup, int>(
                        lodGroup => lodGroup.GetLODs()[0].renderers[0].GetSharedMesh().vertexCount,
                        Mathf.Max
                    );
                    break;
                case SortMode.TrianglesLOD0:
                    SortChildrenByComponentsDetail<LODGroup, int>(
                        lodGroup => lodGroup.GetLODs()[0].renderers[0].GetSharedMesh().triangles.Length / 3,
                        Mathf.Max
                    );
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string NumericsOnly(string value, out string leftover)
        {
            var newString = new StringBuilder();

            var lastWasNumber = false;

            for (var i = 0; i < value.Length; i++)
            {
                var testChar = value[i];

                if (char.IsNumber(testChar))
                {
                    lastWasNumber = true;
                    newString.Append(testChar);
                }
                else if (char.IsPunctuation(testChar) ||
                         char.IsSeparator(testChar) ||
                         char.IsSymbol(testChar))
                {
                    if (lastWasNumber || (i == 0))
                    {
                        newString.Append('.');
                    }

                    newString.Clear();

                    lastWasNumber = false;
                }
            }

            var ns = newString.ToString();

            if (string.IsNullOrWhiteSpace(ns))
            {
                leftover = null;
                return string.Empty;
            }

            leftover = value.Replace(newString.ToString(), string.Empty);
            return newString.ToString();
        }

        private void SortChildrenAlphabetically()
        {
            var children = new List<Transform>();

            var numericSort = false;

            for (var i = objectsToCenter.Length - 1; i >= 0; i--)
            {
                var child = objectsToCenter[i].transform;

                children.Add(child);

                if (i == 0)
                {
                    var testName = NumericsOnly(child.name, out _);

                    if (float.TryParse(testName, out _))
                    {
                        numericSort = true;
                    }
                }

                child.parent = null;
            }

            children.Sort(
                (t1, t2) =>
                {
                    if (numericSort)
                    {
                        var testName1 = NumericsOnly(t1.name, out var leftover1);
                        var testName2 = NumericsOnly(t2.name, out var leftover2);

                        var firstSuccess = float.TryParse(testName1,  out var value1);
                        var secondSuccess = float.TryParse(testName2, out var value2);

                        if (firstSuccess && secondSuccess && (leftover1 == leftover2))
                        {
                            return value1.CompareTo(value2);
                        }

                        return string.Compare(t1.name, t2.name, StringComparison.Ordinal);
                    }

                    return string.Compare(t1.name, t2.name, StringComparison.Ordinal);
                }
            );

            foreach (var child in children)
            {
                child.parent = transform;
            }
        }

        private void SortChildrenByBounds(SortMode sort)
        {
            var children = new List<Transform>();
            var boundsCollection = new Dictionary<int, Bounds>();

            for (var i = objectsToCenter.Length - 1; i >= 0; i--)
            {
                var child = objectsToCenter[i].transform;

                children.Add(child);

                child.parent = null;
                var hash = child.GetHashCode();

                var childRenderers = child.GetComponentsInChildren<Renderer>();

                if ((childRenderers != null) && (childRenderers.Length > 0))
                {
                    var b = childRenderers[0].bounds;

                    for (var j = 1; j < childRenderers.Length; j++)
                    {
                        b.Encapsulate(childRenderers[j].bounds);
                    }

                    boundsCollection.Add(hash, b);
                }
                else
                {
                    var childCollider = child.GetComponentsInChildren<Collider>();

                    if ((childCollider != null) && (childCollider.Length > 0))
                    {
                        var b = childCollider[0].bounds;

                        for (var j = 1; j < childCollider.Length; j++)
                        {
                            b.Encapsulate(childCollider[j].bounds);
                        }

                        boundsCollection.Add(hash, b);
                    }
                    else
                    {
                        boundsCollection.Add(child.GetHashCode(), new Bounds());
                    }
                }
            }

            children.Sort(
                (t1, t2) =>
                {
                    var hash1 = t1.GetHashCode();
                    var hash2 = t2.GetHashCode();

                    var bounds1 = boundsCollection[hash1];
                    var bounds2 = boundsCollection[hash2];

                    int sortVal;

                    switch (sort)
                    {
                        case SortMode.Bounds:
                            sortVal = bounds2.size.sqrMagnitude.CompareTo(bounds1.size.sqrMagnitude);
                            break;

                        case SortMode.Width:

                            var xz1 = bounds1.size.xz();
                            var xz2 = bounds2.size.xz();

                            sortVal = xz2.sqrMagnitude.CompareTo(xz1.sqrMagnitude);
                            break;

                        case SortMode.Height:
                            sortVal = bounds2.size.y.CompareTo(bounds1.size.y);
                            break;

                        default:
                            throw new ArgumentOutOfRangeException(nameof(sort), sort, null);
                    }

                    if (sortVal == 0)
                    {
                        return string.Compare(t2.name, t1.name, StringComparison.Ordinal);
                    }

                    return sortVal;
                }
            );

            foreach (var child in children)
            {
                child.parent = transform;
            }
        }

        private void SortChildrenByComponents<T>()
            where T : Component
        {
            var children = new List<Transform>();
            var componentCollection = new Dictionary<int, int>();

            for (var i = objectsToCenter.Length - 1; i >= 0; i--)
            {
                var child = objectsToCenter[i].transform;

                children.Add(child);

                child.parent = null;
                var hash = child.GetHashCode();

                var components = child.GetComponentsInChildren<T>();

                componentCollection.Add(hash, components.Length);
            }

            children.Sort(
                (t1, t2) =>
                {
                    var hash1 = t1.GetHashCode();
                    var hash2 = t2.GetHashCode();

                    var count1 = componentCollection[hash1];
                    var count2 = componentCollection[hash2];

                    var sort = count2.CompareTo(count1);

                    if (sort == 0)
                    {
                        return string.Compare(t2.name, t1.name, StringComparison.Ordinal);
                    }

                    return sort;
                }
            );

            foreach (var child in children)
            {
                child.parent = transform;
            }
        }

        private void SortChildrenByComponentsDetail<T, TV>(Func<T, TV> valueGenerator, Func<TV, TV, TV> add)
            where TV : IComparable<TV>
        {
            var children = new List<Transform>();

            var componentCollection = new Dictionary<int, TV>();

            for (var i = objectsToCenter.Length - 1; i >= 0; i--)
            {
                var child = objectsToCenter[i].transform;

                children.Add(child);

                child.parent = null;
                var hash = child.GetHashCode();

                var components = child.GetComponentsInChildren<T>();

                TV sum = default;

                for (var j = 0; j < components.Length; j++)
                {
                    try
                    {
                        var newValue = valueGenerator(components[j]);
                        sum = add(sum, newValue);
                    }
                    catch
                    {
                        // ignored
                    }
                }

                componentCollection.Add(hash, sum);
            }

            children.Sort(
                (t1, t2) =>
                {
                    var hash1 = t1.GetHashCode();
                    var hash2 = t2.GetHashCode();

                    var count1 = componentCollection[hash1];
                    var count2 = componentCollection[hash2];

                    var sort = count2.CompareTo(count1);

                    if (sort == 0)
                    {
                        return string.Compare(t2.name, t1.name, StringComparison.Ordinal);
                    }

                    return sort;
                }
            );

            foreach (var child in children)
            {
                child.parent = transform;
            }
        }

        //private Dictionary<int, float> _storedVolumes = new Dictionary<int, float>();

        private void SortChildrenByVolume()
        {
            //if (_storedVolumes == null) _storedVolumes = new Dictionary<int, float>();

            var children = new List<Transform>();
            var volumeCollection = new Dictionary<int, float>();

            for (var i = objectsToCenter.Length - 1; i >= 0; i--)
            {
                var child = objectsToCenter[i].transform;

                children.Add(child);

                child.parent = null;
                var hash = child.GetHashCode();

                /*if (_storedVolumes.ContainsKey(hash))
                {
                    volumeCollection.Add(hash, _storedVolumes[hash]);
                }
                else*/
                {
                    var childRenderers = child.GetComponentsInChildren<Renderer>();

                    if ((childRenderers != null) && (childRenderers.Length > 0))
                    {
                        var volume = childRenderers.GetVolume();

                        volumeCollection.Add(hash, volume);

                        //_storedVolumes.Add(hash, volume);
                    }
                    else
                    {
                        volumeCollection.Add(hash, 0f);
                    }
                }
            }

            children.Sort(
                (t1, t2) =>
                {
                    var hash1 = t1.GetHashCode();
                    var hash2 = t2.GetHashCode();

                    var volume1 = volumeCollection[hash1];
                    var volume2 = volumeCollection[hash2];

                    var sort = volume2.CompareTo(volume1);

                    if (sort == 0)
                    {
                        return string.Compare(t2.name, t1.name, StringComparison.Ordinal);
                    }

                    return sort;
                }
            );

            foreach (var child in children)
            {
                child.parent = transform;
            }
        }
    }
}