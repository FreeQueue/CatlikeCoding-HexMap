using UnityEngine;

public class HexCell : MonoBehaviour
{
	#region Serialized Fields
	public HexCoordinates coordinates;
	public Color color;
	public RectTransform uiRect;
	[SerializeField] private HexCell[] neighbors;
	public HexGridChunk chunk;
	#endregion

	private int elevation = int.MinValue;
	public Vector3 Position => transform.localPosition;
	public int Elevation {
		get => elevation;
		set {
			if (elevation == value) return;
			elevation = value;
			Vector3 position = transform.localPosition;
			position.y = value * HexMetrics.elevationStep;
			position.y += (HexMetrics.SampleNoise(position).y * 2f - 1f) * HexMetrics.elevationPerturbStrength;
			transform.localPosition = position;

			Vector3 uiPosition = uiRect.localPosition;
			uiPosition.z = -position.y;
			uiRect.localPosition = uiPosition;
			Refresh();
		}
	}
	public Color Color {
		get => color;
		set {
			if (color == value) return;
			color = value;
			Refresh();
		}
	}

	private void Refresh() {
		if (chunk) {
			chunk.Refresh();
			for (int i = 0; i < neighbors.Length; i++) {
				HexCell neighbor = neighbors[i];
				if (neighbor != null && neighbor.chunk != chunk) neighbor.chunk.Refresh();
			}
		}
	}

	public HexCell GetNeighbor(HexDirection direction) => neighbors[(int)direction];

	public void SetNeighbor(HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
		cell.neighbors[(int)direction.Opposite()] = this;
	}

	public HexEdgeType GetEdgeType(HexDirection direction) =>
		HexMetrics.GetEdgeType(
			elevation, neighbors[(int)direction].elevation
		);

	public HexEdgeType GetEdgeType(HexCell otherCell) =>
		HexMetrics.GetEdgeType(
			elevation, otherCell.elevation
		);
}