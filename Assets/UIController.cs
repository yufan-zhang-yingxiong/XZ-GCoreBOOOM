using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    public static UIController instance { get; private set;}
    public GameObject lineLength;
    public Image lineLengthVal;
    private SquareController _player;
    private SquareController player
    {
        get
        {
            if (_player == null) _player = GameObject.FindWithTag("Square").GetComponent<SquareController>();
            return _player;
        }
    }
    private void Awake()
    {
        instance = this;
    }

    private void Update()
    {
        lineLength.SetActive(player.IsConnected);
        lineLengthVal.fillAmount = Mathf.Clamp(Mathf.Abs(player.maxDistance - player.Dj.distance) / player.maxDistance, 0, 1);
    }
}
