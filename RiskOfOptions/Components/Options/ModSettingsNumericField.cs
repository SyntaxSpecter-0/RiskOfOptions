using System;
using System.Globalization;
using RiskOfOptions.OptionConfigs;
using RiskOfOptions.Options;
using RoR2.UI;
using TMPro;
using UnityEngine;

namespace RiskOfOptions.Components.Options;

public abstract class ModSettingsNumericField<TNumeric> : ModSettingsControl<TNumeric, NumericFieldConfig<TNumeric>>
    where TNumeric : struct, IComparable, IComparable<TNumeric>, IConvertible, IEquatable<TNumeric>, IFormattable
{
    public TMP_InputField valueText;
    public TNumeric min;
    public TNumeric max;
    public string formatString;
    
    protected NumericFieldConfig<TNumeric>.NumericFieldTryParse? _numericFieldTryParse;

    private MPEventSystemLocator _eventSystemLocator;
    private int _editStartFrame = -1;
    private int _editEndFrame = -1;

    protected override void Awake()
    {
        base.Awake();

        _numericFieldTryParse = Config?.TryParse;
        
        valueText.onEndEdit.AddListener(OnTextEdited);
        valueText.onSubmit.AddListener(OnTextEdited);

        _eventSystemLocator = GetComponent<MPEventSystemLocator>();
        GetComponent<HGButton>().onClick.AddListener(StartEditing);
    }

    private void Update()
    {
        var eventSystem = _eventSystemLocator.eventSystem;

        if (!eventSystem || eventSystem.currentSelectedGameObject != valueText.gameObject)
            return;

        if (valueText.isFocused)
        {
            // A focused text box swallows menu input, so read the confirm and back buttons directly
            var player = eventSystem.player;
            if (Time.frameCount > _editStartFrame && (player.GetButtonDown(RewiredConsts.Action.UISubmit) || player.GetButtonDown(RewiredConsts.Action.UICancel)))
                StopEditing();

            return;
        }

        // Editing ended some other way (e.g. Enter), so give the gamepad its place in the list back
        if (eventSystem.currentInputSource == MPEventSystem.InputSource.Gamepad && Time.frameCount > _editStartFrame + 1)
            eventSystem.SetSelectedGameObject(gameObject);
    }

    private void StartEditing()
    {
        var eventSystem = _eventSystemLocator.eventSystem;

        // The A press that confirmed an edit can reach the row in the same frame, which would reopen it
        if (!eventSystem || eventSystem.currentInputSource != MPEventSystem.InputSource.Gamepad || Time.frameCount <= _editEndFrame + 1)
            return;

        _editStartFrame = Time.frameCount;
        eventSystem.SetSelectedGameObject(valueText.gameObject);
        valueText.ActivateInputField();
    }

    private void StopEditing()
    {
        _editEndFrame = Time.frameCount;
        editEndedFrame = Time.frameCount;

        // Deactivating raises onEndEdit, which saves the value through OnTextEdited
        valueText.DeactivateInputField();
        _eventSystemLocator.eventSystem.SetSelectedGameObject(gameObject);
    }

    protected override void Disable()
    {
        foreach (var button in GetComponentsInChildren<HGButton>())
            button.interactable = false;
    }

    protected override void Enable()
    {
        foreach (var button in GetComponentsInChildren<HGButton>())
            button.interactable = true;
    }
    
    protected override void OnUpdateControls()
    {
        base.OnUpdateControls();

        var num = Clamp(GetCurrentValue());
            
        if (valueText)
            valueText.text = string.Format(Separator.GetCultureInfo(), formatString, num);
    }

    private void OnTextEdited(string newText)
    {
        SubmitValue(InternalTryParse(newText, Separator.GetCultureInfo(), out var num)
            ? Clamp(num)
            : GetCurrentValue());
    }
    
    private bool InternalTryParse(string input, CultureInfo cultureInfo, out TNumeric value)
    {
        return _numericFieldTryParse is not null
            ? _numericFieldTryParse(input, cultureInfo, out value)
            : TryParse(input, NumberStyles.Any, cultureInfo, out value);
    }

    protected abstract TNumeric Clamp(TNumeric value);

    protected abstract bool TryParse(string text, NumberStyles styles, IFormatProvider provider, out TNumeric num);
}