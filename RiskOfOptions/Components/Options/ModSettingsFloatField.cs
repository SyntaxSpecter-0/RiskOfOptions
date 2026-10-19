using System;
using System.Globalization;
using UnityEngine;

namespace RiskOfOptions.Components.Options;

public class ModSettingsFloatField : ModSettingsNumericField<float>
{
    internal override bool UsesLeftRight => true;

    protected override void Awake()
    {
        base.Awake();

        SetLeftRightActions(() => Step(-1), () => Step(1));
    }

    private void Step(int direction)
    {
        // Float fields have no step setting, so use 1% of the range when one is set, otherwise 0.1
        bool hasRange = min > float.MinValue && max < float.MaxValue;
        float step = hasRange ? (max - min) / 100f : 0.1f;

        SubmitValue(Clamp(StepOnGrid(GetCurrentValue(), step, hasRange ? min : 0f, direction)));
    }

    protected override float Clamp(float value) => Mathf.Clamp(value, min, max);

    protected override bool TryParse(string text, NumberStyles styles, IFormatProvider provider, out float num) =>
        float.TryParse(text, styles, provider, out num);
}