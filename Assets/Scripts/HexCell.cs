using UnityEngine;

public class HexCell : MonoBehaviour
{
	#region Serialized Fields
	public HexCoordinates coordinates;
	public Color color;
	[SerializeField] private HexCell[] neighbors;
	#endregion

	public HexCell GetNeighbor(HexDirection direction) => neighbors[(int)direction];

	public void SetNeighbor(HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
		cell.neighbors[(int)direction.Opposite()] = this;
	}
}