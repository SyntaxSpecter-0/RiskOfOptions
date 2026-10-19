using RiskOfOptions.Components.Panel;
using RiskOfOptions.Options;
using RoR2.UI;
using UnityEngine;
using UnityEngine.Events;

namespace RiskOfOptions.Components.Options
{
    public abstract class ModSetting : MonoBehaviour
    {
        protected static DecimalSeparator Separator => RiskOfOptionsPlugin.decimalSeparator!.Value;
        
        public string nameToken;
        public string settingToken;
        public LanguageTextMeshController nameLabel;
        public ModOptionPanelController optionController;

        protected BaseOption option;

        // The panel leaves left free on these instead of linking it back to the mod list
        internal virtual bool UsesLeftRight => false;

        // The B press that stops editing a field must not also take the panel back to the mod list
        internal static int editEndedFrame = -1;

        protected virtual void Awake()
        {
            if (nameLabel && !string.IsNullOrEmpty(nameToken))
                nameLabel.token = nameToken;
            
            if (string.IsNullOrEmpty(settingToken))
                return;

            option = ModSettingsManager.OptionCollection.GetOption(settingToken);
        }

        protected virtual void Start()
        {
            if (nameLabel && !string.IsNullOrEmpty(nameToken))
                nameLabel.token = nameToken;
        }

        // Several prefabs copied from the sliders kept left/right calls with no target, so they are replaced here
        protected void SetLeftRightActions(UnityAction left, UnityAction right)
        {
            var button = GetComponent<HGButton>();

            button.onFindSelectableLeft = new UnityEvent();
            button.onFindSelectableLeft.AddListener(left);

            button.onFindSelectableRight = new UnityEvent();
            button.onFindSelectableRight.AddListener(right);
        }

        // Moves to the next point on a grid of steps from origin, so values that start off the grid still reach round numbers.
        // Decimal maths keeps 0.1 - 0.1 at exactly 0, where float maths gives -1.5E-09.
        protected static float StepOnGrid(float value, float step, float origin, int direction)
        {
            if (Mathf.Abs(value) >= 1e9f || Mathf.Abs(origin) >= 1e9f)
                return value + direction * step;

            decimal offset = (decimal)value - (decimal)origin;
            decimal size = (decimal)step;
            decimal index = direction > 0 ? decimal.Floor(offset / size) + 1 : decimal.Ceiling(offset / size) - 1;

            return (float)(index * size + (decimal)origin);
        }

        public abstract bool HasChanged();

        public abstract void Revert();

        public abstract void CheckIfDisabled();

        public virtual void UpdateModifiedIndicator() { }
        
        protected abstract void Disable();

        protected abstract void Enable();
    }
}