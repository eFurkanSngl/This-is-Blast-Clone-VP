using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

public class AudioManager : MonoBehaviour
{
    [Inject] SignalBus _signalBus;
    [SerializeField] private AudioSource _mergeAudio;
    [SerializeField] private AudioSource _clickAudio;
    [SerializeField] private AudioSource _swipeAudio;
    [SerializeField] private AudioSource _destroyAudio;


    private void Awake()
    {
        _signalBus.Subscribe<MergeSignal>(MergeSound);
        _signalBus.Subscribe<SwipeSignalBus>(SwipeSound);
        _signalBus.Subscribe<ClickSignalBus>(ClickSound);
        _signalBus.Subscribe<DestorySignal>(DestroySound);
    }

    private void DestroySound() => _destroyAudio.Play();

    private void SwipeSound()=>_swipeAudio.Play();
    private void MergeSound()
    {
        _mergeAudio.Play();
    }

    private void ClickSound() => _clickAudio.Play();


    private void OnDestroy()
    {
        _signalBus.Unsubscribe<MergeSignal>(MergeSound);
        _signalBus.Unsubscribe<ClickSignalBus>(ClickSound);
        _signalBus.Unsubscribe<SwipeSignalBus>(SwipeSound);
        _signalBus.Unsubscribe<DestorySignal>(DestroySound);
    }
}
