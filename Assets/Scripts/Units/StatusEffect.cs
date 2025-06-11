using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusEffect : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI durationText;

    [Header("Icons")]
    public Sprite poisonedIcon;
    public Sprite stunnedIcon;
    public Sprite strengthenedIcon;
    public Sprite protectedIcon;
    public Sprite hastedIcon;

    private StatusEffectType effectType;
    private int duration;

    public void Initialize(StatusEffectType type, int initialDuration)
    {
        effectType = type;
        duration = initialDuration;

        // Set icon based on effect type
        if (iconImage != null)
        {
            switch (effectType)
            {
                case StatusEffectType.Poisoned:
                    iconImage.sprite = poisonedIcon;
                    iconImage.color = Color.green;
                    break;
                case StatusEffectType.Stunned:
                    iconImage.sprite = stunnedIcon;
                    iconImage.color = Color.yellow;
                    break;
                case StatusEffectType.Strengthened:
                    iconImage.sprite = strengthenedIcon;
                    iconImage.color = Color.red;
                    break;
                case StatusEffectType.Protected:
                    iconImage.sprite = protectedIcon;
                    iconImage.color = Color.blue;
                    break;
                case StatusEffectType.Hasted:
                    iconImage.sprite = hastedIcon;
                    iconImage.color = Color.cyan;
                    break;
            }
        }

        UpdateDuration(duration);
    }

    public void UpdateDuration(int newDuration)
    {
        duration = newDuration;

        if (durationText != null)
        {
            durationText.text = duration.ToString();
        }
    }

    public void DecreaseDuration()
    {
        duration--;
        UpdateDuration(duration);

        if (duration <= 0)
        {
            // Effect has expired
            Destroy(gameObject);
        }
    }

    private void LateUpdate()
    {
        // Always face the camera
        if (Camera.main != null)
        {
            transform.LookAt(transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up);
        }
    }
}
