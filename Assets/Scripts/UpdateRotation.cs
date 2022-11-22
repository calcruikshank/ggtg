using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UpdateRotation : NetworkBehaviour
{
    private readonly NetworkVariable<Quaternion> _netRot = new(writePerm: NetworkVariableWritePermission.Owner);
    private void Update()
    {
        if (IsOwner)
        {
            _netRot.Value = transform.rotation;
        }
        else
        {
            transform.rotation = _netRot.Value;
        }
    }

}
