using UnityEngine;

namespace VTools.Grid
{
    [CreateAssetMenu(fileName = "GridObjectTemplate", menuName = "Grid/GridObjectTemplate", order = 0)]
    public class GridObjectTemplate : ScriptableObject
    {
        [Header("Definition")]
        [SerializeField] protected string _name;
        [SerializeField] protected GridObjectController _view;
        [SerializeField] protected Sprite _sprite;

        public string Name => _name;
        public GridObjectController View => _view;

        public Sprite Sprite => _sprite;

        public virtual GridObject CreateInstance()
        {
            return new GridObject(this);
        }
    }
}