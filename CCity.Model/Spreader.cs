using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class Spreader
    {

        #region Fields

        private HashSet<Field> _modifiedFields;
        private Func<Placeable, List<Placeable>> _neighbours;
        private Func<Placeable, Placeable, bool> _spreadRule;
        private HashSet<Placeable> _roots;
        private readonly SpreadType _type;
        private bool _weighted;

        #endregion

        #region Constructors

        public Spreader(SpreadType type, bool weighted, Func<Placeable, Placeable, bool> spreadRule, Func<Placeable, List<Placeable>> neighbours)
        {
            _type = type;
            _weighted = weighted;
            _spreadRule = spreadRule;
            _neighbours = neighbours;
            _roots = new HashSet<Placeable>();
            _modifiedFields = new HashSet<Field>();
        }

        #endregion

        #region Public methods

        public void Refresh(Placeable placeable)
        {
            if (placeable.IsDemolished)
            {
                if (IsRoot(placeable)) _roots.Remove(placeable);
                bool hadRoot = GetRoot(placeable) != null;
                List<Placeable> possibleEntries = Unspread(placeable);
                foreach(Placeable possibleEntry in possibleEntries) Spread(possibleEntry);
                if (_weighted && hadRoot) RefreshRoots();
            }
            else
            {
                if (IsRoot(placeable)) _roots.Add(placeable);
                Spread(placeable);
                if (_weighted && !IsIsolated(placeable)) RefreshRoots();
            }
        }

        public List<Field> GetAndClearModifiedFields()
        {
            List<Field> returnValue = _modifiedFields.ToList();
            _modifiedFields.Clear();
            return returnValue;
        }

        #endregion

        #region Private methods

        #region Spread related

        private void Spread(Placeable placeable)
        {
            Queue<Placeable> BFSQueue = new Queue<Placeable>();
            BFSQueue.Enqueue(placeable);
            Spread(BFSQueue);
        }

        private void Spread(IEnumerable<Placeable> placeables)
        {
            Queue<Placeable> BFSQueue = new Queue<Placeable>();
            foreach(Placeable placeable in placeables) BFSQueue.Enqueue(placeable);
            Spread(BFSQueue);
        }

        //From the given points, starts Spreading the current SpreadType using BFS
        private void Spread(Queue<Placeable> BFSQueue)
        {
            Queue<Placeable> toOptimize = new Queue<Placeable>();
            while (BFSQueue.Count > 0)
            {
                Placeable current = BFSQueue.Dequeue();

                //If the currently visited placeable is not spreaded, it tries to spread itself by
                //looking for a neighbor that is spreaded.
                //If there are more spreaded neighbours available, it chooses the strongest
                if (!Check(current))
                {
                    Placeable? maxNeighbor = GetNeighbours(current).Where(e => Check(e) && CanConnect(e, current)).MaxBy(e => GetRootCapacity(e));
                    if (maxNeighbor != null)
                    {
                        Connect(maxNeighbor, current);
                    }
                }

                //If the currently visited placeable is spreaded (by default, or just got spreaded some lines before)
                if (Check(current))
                {
                    //Queuing placeables that are not spreaded.
                    foreach (Placeable neighbor in GetNeighbours(current).Where(e => !Check(e))) BFSQueue.Enqueue(neighbor);
                }
            }

            if (_weighted)
            {
                HashSet<Placeable> alreadyOptimized = new HashSet<Placeable>();
                while (toOptimize.Count > 0)
                {
                    Placeable current = toOptimize.Dequeue();
                    Placeable? strongerNeighbour = GetNeighbours(current).Where(e => Check(e) && CanConnect(e, current) && WouldWorthChanging(current, e)).MaxBy(e => GetRootCapacity(e));
                    if (strongerNeighbour != null)
                    {
                        Connect(strongerNeighbour, current);

                        //The root of the currently visited placeable changed, so we have to change
                        //the root of every placeable that directly or indireclty gets the spread from current
                        RootChanged(current);

                        //We have a set of already optimized placeables so we not optimize a placeable twice in one Spread
                        //in order to prevent passing back and forth edge case among 2 roots
                        alreadyOptimized.Add(current);
                    }
                    //Queuing placeables that could have a stronger neighbor
                    foreach (Placeable neighbor in GetNeighbours(current).Where(e => Check(e) && WouldWorthChanging(e, current) && !alreadyOptimized.Contains(e))) toOptimize.Enqueue(neighbor);
                }
            }
        }

        //Unspread from one point
        //Returns the strongest possible entry after finishing the unspreading
        private List<Placeable> Unspread(Placeable placeable)
        {
            Queue<Placeable> BFSQueue = new Queue<Placeable>();
            BFSQueue.Enqueue(placeable);
            return Unspread(BFSQueue);
        }

        //Unspread from two or more points (parallel BFS)
        private void Unspread(IEnumerable<Placeable> placeables)
        {
            Queue<Placeable> BFSQueue = new Queue<Placeable>();
            foreach (Placeable placeable in placeables) BFSQueue.Enqueue(placeable);
            Unspread(BFSQueue);
        }

        //General unspread
        private List<Placeable> Unspread(Queue<Placeable> BFSQueue)
        {
            List<Placeable> possibleEntries = new List<Placeable>();
            while (BFSQueue.Count > 0)
            {
                Placeable current = BFSQueue.Dequeue();

                //Disconnecting the currently visited placeable => it is not spreaded anymore
                Disconnect(current);

                //Looking for potential sources - placeables that are spreaded at the moment
                //(this property could change during the unspread, so we have to filter them after the bfs)
                List<Placeable> potentialSources = GetNeighbours(current).FindAll(e => Check(e) && !ArePairs(current, e));
                foreach (Placeable potentialSource in potentialSources) possibleEntries.Add(potentialSource);

                //Queuing every neighbor that gets the spread from the currently visited placeable
                List<Placeable> queuedNeighbours = GetNeighbours(current).FindAll(e => ArePairs(current, e));
                foreach (Placeable queuedPlaceable in queuedNeighbours) BFSQueue.Enqueue(queuedPlaceable);
            }
            //Returning the strongest possible entry
            //return possibleEntries.Where(e => Check(e)).MaxBy(e => GetRootCapacity(e));
            return possibleEntries.Where(e => Check(e)).ToList();
        }

        #endregion

        #region Root related

        //If the root of a placeable changes, we have to refresh the root of every placeables
        //that directly or indireclty gets the spread from it
        private void RootChanged(Placeable placeable)
        {
            Queue<Placeable> BFSQueue = new Queue<Placeable>();
            BFSQueue.Enqueue(placeable);
            while (BFSQueue.Count > 0)
            {
                Placeable current = BFSQueue.Dequeue();
                current.GetsSpreadFrom[_type] = (current.GetsSpreadFrom[_type].direct, placeable.GetsSpreadFrom[_type].root);
                foreach (Placeable spreaded in GetNeighbours(current).Where(e => ArePairs(current, e))) BFSQueue.Enqueue(spreaded);
            }
        }

        //Refreshing roots with parallel BFS
        public void RefreshRoots()
        {
            List<Action> rootEnablingActions = new List<Action>();
            
            //Disabling roots so they are not considered spreaded anymore, and then unspread them
            rootEnablingActions = _roots.Select(e => DisableRootTemporarly(e)).ToList();
            Unspread(_roots);

            //Reenable and spread roots
            foreach (Action rootEnablingAction in rootEnablingActions) rootEnablingAction();
            Spread(_roots);

            //During the spreading, some roots could have obtained some capacity, because an other root could have
            //taken some placeables from it.
            //So there's a chance that there are unspreaded placeables despite the fact that
            //there are available roots.
            //Optimizing roots by looking for unspreaded placeable available from them, and trying to
            //spread them.
            OptimizeRoots();
        }

        private void OptimizeRoots()
        {
            Queue<Placeable> BFSQueue = new Queue<Placeable>();
            Queue<Placeable> UnspreadedPlaceables = new Queue<Placeable>();
            foreach (Placeable root in _roots) BFSQueue.Enqueue(root);
            while (BFSQueue.Count > 0)
            {
                Placeable current = BFSQueue.Dequeue();

                if (!Check(current)) UnspreadedPlaceables.Enqueue(current);

                foreach (Placeable neighbor in GetNeighbours(current).Where(e => ArePairs(current, e))) BFSQueue.Enqueue(neighbor);
            }
            while (UnspreadedPlaceables.Count > 0)
            {
                Placeable current = UnspreadedPlaceables.Dequeue();

                if (!Check(current)) Spread(new List<Placeable>() { current });
            }
        }

        #endregion

        #region Helpers

        private bool Check(Placeable placeable)
            => (placeable.CurrentSpreadValue[_type] == placeable.MaxSpreadValue[_type]() && placeable.GetsSpreadFrom[_type].root != null)
            || IsRoot(placeable);

        private void Connect(Placeable source, Placeable target)
        {
            if (!CanConnect(source, target))
            {
                throw new Exception();
            }

            Disconnect(target);

            Placeable root = GetRoot(source)!;
            target.GetsSpreadFrom[_type] = (source, root);
            int val = Math.Min(target.MaxSpreadValue[_type](), GetRootCapacity(source));
            root.CurrentSpreadValue[_type] += val;
            target.CurrentSpreadValue[_type] = val;

            _modifiedFields.Add(target.Owner!);
            if (target is IMultifield multifield)
            {
                foreach (Filler filler in multifield.Occupies) _modifiedFields.Add(filler.Owner!);
            }
        }

        private void Disconnect(Placeable placeable)
        {
            if (GetRoot(placeable) == null) return;

            Placeable root = GetRoot(placeable)!;
            root.CurrentSpreadValue[_type] -= placeable.CurrentSpreadValue[_type];
            placeable.CurrentSpreadValue[_type] = 0;
            placeable.GetsSpreadFrom[_type] = (null, null);

            _modifiedFields.Add(placeable.Owner!);
            if (placeable is IMultifield multifield)
            {
                foreach (Filler filler in multifield.Occupies) _modifiedFields.Add(filler.Owner!);
            }
        }


        //Disableing root temporarly and returning a function that reenables it
        private Action DisableRootTemporarly(Placeable root)
        {
            root.GetsSpreadFrom[_type] = (null, null);
            return () => root.GetsSpreadFrom[_type] = (null, root);
        }

        #endregion

        #region Aliases

        private bool CanConnect(Placeable source, Placeable target) => _spreadRule(source, target);
        private List<Placeable> GetNeighbours(Placeable placeable) => _neighbours(placeable);
        private bool ArePairs(Placeable source, Placeable target) => target.GetsSpreadFrom[_type].direct == source;
        private Placeable? GetRoot(Placeable placeable) => placeable.GetsSpreadFrom[_type].root;
        private bool IsRoot(Placeable placeable) => GetRoot(placeable) != null ? GetRoot(placeable) == placeable : false;
        private int GetRootCapacity(Placeable placeable) => GetRoot(placeable)!.MaxSpreadValue[_type]() - GetRoot(placeable)!.CurrentSpreadValue[_type];
        private bool WouldWorthChanging(Placeable examined, Placeable potentialSource) => GetRootCapacity(potentialSource) > (GetRootCapacity(examined) + examined.CurrentSpreadValue[_type]) && GetRoot(examined) != GetRoot(potentialSource) && CanConnect(potentialSource, examined) && GetRootCapacity(potentialSource) >= examined.MaxSpreadValue[_type]();
        private bool IsIsolated(Placeable placeable) => GetNeighbours(placeable).Count == 0 || GetNeighbours(placeable).All(e => GetRoot(e) == null);

        #endregion

        #endregion
    }
}
