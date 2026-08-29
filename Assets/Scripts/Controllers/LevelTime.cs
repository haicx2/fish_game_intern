using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelTime : LevelCondition
{
    private float m_time;
    private GameObject m_bombIcon;

    public override void Setup(float value, Text txt, GameManager mngr)
    {
        base.Setup(value, txt, mngr);

        m_time = 60f;

        CreateBombIcon();
        UpdateText();
    }

    private void CreateBombIcon()
    {
        m_bombIcon = new GameObject("BombIcon");
        m_bombIcon.transform.SetParent(m_txt.transform, false);

        Image img = m_bombIcon.AddComponent<Image>();
        img.sprite = Resources.Load<Sprite>("12");

        img.rectTransform.sizeDelta = new Vector2(50, 50);
        img.rectTransform.anchoredPosition = new Vector2(0, 60);

        m_bombIcon.SetActive(false);
    }

    public override void Tick()
    {
        if (m_conditionCompleted) return;
        if (m_gameManager.State != GameManager.eStateGame.GAME_STARTED) return;

        m_time -= Time.deltaTime;

        // Show the bomb when 10 seconds or less remain
        if (m_time <= 10f && m_time > 0f)
        {
            if (m_bombIcon != null && !m_bombIcon.activeSelf)
            {
                m_bombIcon.SetActive(true);
            }
        }

        UpdateText();

        if (m_time <= 0f)
        {
            m_time = 0f;
            if (m_bombIcon != null) m_bombIcon.SetActive(false);

            UpdateText();
            m_gameManager.ShowLoseScreen();
            OnConditionComplete();
        }
    }

    protected override void UpdateText()
    {
        if (m_time < 0f) return;

        if (m_txt != null)
        {
            m_txt.text = string.Format("TIME:\n{0:00}", Mathf.Ceil(m_time));

            // Optional bonus: Make the timer text turn red when 10 seconds are left!
            if (m_time <= 10f)
            {
                m_txt.color = Color.red;
            }
        }
    }
}