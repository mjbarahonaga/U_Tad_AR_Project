using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MEC;

public interface IPausable
{
    public void Pause(bool pause);
    abstract void ActivateUpdate();
    abstract IEnumerator<float> UpdateCoroutine();
}
