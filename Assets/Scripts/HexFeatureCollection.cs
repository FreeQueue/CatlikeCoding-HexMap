using System;
using UnityEngine;

[Serializable]
public struct HexFeatureCollection
{
	#region Serialized Fields
	public Transform[] prefabs;
	#endregion

	public Transform Pick(float choice) => prefabs[(int)(choice * prefabs.Length)];
}