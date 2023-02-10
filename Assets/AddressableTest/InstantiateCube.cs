using System.Collections;
using System.Collections.Generic;
using UnityEditor.AddressableAssets.Build;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class InstantiateCube : MonoBehaviour
{
    private Button button;

    [SerializeField] private AssetReference assetReference;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(CreateCube);
    }

    private void CreateCube()
    {
        assetReference.InstantiateAsync(new Vector3(0, 0, 0), Quaternion.identity);
    }
}
