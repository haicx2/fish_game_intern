using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelCondition : MonoBehaviour
{
    public event Action ConditionCompleteEvent = delegate { };

    protected Text m_txt;
    protected bool m_conditionCompleted = false;
    protected GameManager m_gameManager;

    public virtual void Setup(float value, Text txt)
    {
        m_txt = txt;
    }

    public virtual void Setup(float value, Text txt, GameManager mngr)
    {
        m_txt = txt;
        m_gameManager = mngr;
    }

    public virtual void Setup(float value, Text txt, BoardController board, GameManager mngr)
    {
        m_txt = txt;
        m_gameManager = mngr;
    }

    protected virtual void UpdateText() { }

    public virtual void Tick() { }

    protected void OnConditionComplete()
    {
        m_conditionCompleted = true;
        ConditionCompleteEvent();
    }

    protected virtual void OnDestroy()
    {
    }
}