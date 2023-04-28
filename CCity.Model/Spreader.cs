using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace CCity.Model
{
    public class Spreader
    {
        public HashSet<Field> ModifiedFields { get; private set; }
        private Func<Placeable, List<Placeable>> _neighbours;
        private Func<Placeable, Placeable, bool> _spreadRule;
        private readonly SpreadType _type;

        public Spreader(SpreadType type, Func<Placeable, Placeable, bool> spreadRule, Func<Placeable, List<Placeable>> neighbours)
        {
            _type = type;
            _spreadRule = spreadRule;
            _neighbours = neighbours;
            ModifiedFields = new HashSet<Field>();
        }

        public void Refresh(Placeable placeable)
        {   
            if (placeable.IsDemolished)
            {
                bool rootRefreshWillBeNeeded = false;
                Placeable? root = GetRoot(placeable);
                if (root != null && IsRootFullyUtilized(root)) rootRefreshWillBeNeeded = true;
                List<Placeable> spreadedNeighbours = GetNeighbours(placeable).FindAll(e => ArePairs(placeable, e));
                Disconnect(placeable);
                foreach (Placeable neighbor in spreadedNeighbours)
                {
                    Modify(neighbor);
                }
                if (rootRefreshWillBeNeeded) RefreshRoot(root!);
            }
            else
            {
                Spread(placeable);
                if (IsNotFullyWeighted(placeable))
                {
                    RefreshRoot(placeable);
                }
            }
        }

        public void RefreshRoot(Placeable placeable)
        {
            Placeable? root = GetRoot(placeable);
            if (root == null) return;
            List<Placeable> spreadedNeighbours = GetNeighbours(root).FindAll(e => ArePairs(root, e));
            Action<Placeable> reenableRoot = DisableRootTemporarly(root);
            foreach (Placeable neighbor in spreadedNeighbours)
            {
                Modify(neighbor);
            }
            reenableRoot(root);
            Spread(root);
        }

        private void Spread(Placeable placeable)
        {
            Queue<Placeable> levelOrderQueue = new Queue<Placeable>();
            levelOrderQueue.Enqueue(placeable);
            while(levelOrderQueue.Count > 0)
            {
                Placeable current = levelOrderQueue.Dequeue();
                List<Placeable> neighbours = GetNeighbours(current);
                if (!Check(current))
                {
                    Placeable? maxNeighbor = neighbours.Where(e => Check(e) && CanConnect(e, current)).MaxBy(e => GetRootCapacity(e));
                    if (maxNeighbor != null) Connect(maxNeighbor, current);
                }

                if (Check(current))
                {
                    Placeable? strongerNeighbour = neighbours.Where(e => Check(e) && CanConnect(e, current) && GetRootCapacity(e) > GetRootCapacity(current)).MaxBy(e => GetRootCapacity(e));
                    if (strongerNeighbour != null)
                    {
                        Connect(strongerNeighbour, current);
                    }
                    List<Placeable> targetNeighbours = GetNeighbours(current).FindAll(e => !Check(e));
                    foreach (Placeable neighbor in targetNeighbours) levelOrderQueue.Enqueue(neighbor);
                }
            }
        }

        private void Modify(Placeable placeable)
        {
            Queue<Placeable> levelOrderQueue = new Queue<Placeable>();
            Queue<Placeable> possibleEntries = new Queue<Placeable>();
            levelOrderQueue.Enqueue(placeable);
            while(levelOrderQueue.Count > 0)
            {
                Placeable current = levelOrderQueue.Dequeue();

                Disconnect(current);
                List<Placeable> potentialSources = GetNeighbours(current).FindAll(e => Check(e) && !ArePairs(current, e));
                foreach (Placeable potentialSource in potentialSources) possibleEntries.Enqueue(potentialSource);

                List<Placeable> queuedNeighbours = GetNeighbours(current).FindAll(e => ArePairs(current, e));
                foreach (Placeable queuedPlaceable in queuedNeighbours) levelOrderQueue.Enqueue(queuedPlaceable);
            }
            bool found = false;
            while(possibleEntries.Count > 0 && !found)
            {
                Placeable possibleEntry = possibleEntries.Dequeue();
                if (Check(possibleEntry))
                {
                    Spread(possibleEntry);
                    found = true;
                }
            }
            
        }

        public List<Field> GetAndClearModifiedFields()
        {
            List<Field> returnValue = ModifiedFields.ToList();
            ModifiedFields = new HashSet<Field>();
            return returnValue;
        }


        private bool Check(Placeable placeable)
            => (placeable.CurrentSpreadValue[_type] == placeable.MaxSpreadValue[_type]() && placeable.GetsSpreadFrom[_type].root != null)
            || placeable.GetsSpreadFrom[_type].root == placeable;

        private void Connect(Placeable source, Placeable target)
        {
            Disconnect(target);

            Placeable root = GetRoot(source)!;
            target.GetsSpreadFrom[_type] = (source, root);
            int val = Math.Min(target.MaxSpreadValue[_type](), GetRootCapacity(source));
            root.CurrentSpreadValue[_type] += val;
            target.CurrentSpreadValue[_type] = val;

            ModifiedFields.Add(target.Owner!);
            if(target is IMultifield multifield)
            {
                foreach (Filler filler in multifield.Occupies) ModifiedFields.Add(filler.Owner!);
            }
        }

        private void Disconnect(Placeable placeable)
        {
            if (GetRoot(placeable) == null) return;

            Placeable root = GetRoot(placeable)!;
            root.CurrentSpreadValue[_type] -= placeable.CurrentSpreadValue[_type];
            placeable.CurrentSpreadValue[_type] = 0;
            placeable.GetsSpreadFrom[_type] = (null, null);

            ModifiedFields.Add(placeable.Owner!);
            if(placeable is IMultifield multifield)
            {
                foreach (Filler filler in multifield.Occupies) ModifiedFields.Add(filler.Owner!);
            }
        }

        private bool CanConnect(Placeable source, Placeable target) => _spreadRule(source, target);
        private List<Placeable> GetNeighbours(Placeable placeable) => _neighbours(placeable);
        private bool ArePairs(Placeable source, Placeable target) => target.GetsSpreadFrom[_type].direct == source;
        private Placeable? GetRoot(Placeable placeable) => placeable.GetsSpreadFrom[_type].root;
        //TESTING:
        //private int GetRootCapacity(Placeable placeable) => GetRoot(placeable) != null ? GetRoot(placeable)!.MaxSpreadValue[_type] - GetRoot(placeable)!.CurrentSpreadValue[_type] : 0;
        private int GetRootCapacity(Placeable placeable) => GetRoot(placeable)!.MaxSpreadValue[_type]() - GetRoot(placeable)!.CurrentSpreadValue[_type];
        private bool IsRootFullyUtilized(Placeable placeable) => GetRoot(placeable)!.MaxSpreadValue[_type]() == GetRoot(placeable)!.CurrentSpreadValue[_type];
        private bool IsNotFullyWeighted(Placeable placeable) => placeable.MaxSpreadValue[_type]() > placeable.CurrentSpreadValue[_type];
        private Action<Placeable> DisableRootTemporarly(Placeable root)
        {
            root.GetsSpreadFrom[_type] = (null, null);
            return (root) => root.GetsSpreadFrom[_type] = (null, root);
        }
    }
}
