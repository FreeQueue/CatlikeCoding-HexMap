using UnityEngine;

public class HexCell : MonoBehaviour
{
	#region Serialized Fields
	/// <summary>
	///     Hexagonal coordinates unique to the cell.
	/// </summary>
	public HexCoordinates coordinates;
	public RectTransform uiRect;
	[SerializeField] private HexCell[] neighbors;
	public HexGridChunk chunk;
	#endregion

	private Color color;

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

			//check if the river is legal
			if (
				HasOutgoingRiver &&
				elevation < GetNeighbor(OutgoingRiver).elevation
			)
				RemoveOutgoingRiver();
			if (
				HasIncomingRiver &&
				elevation > GetNeighbor(IncomingRiver).elevation
			)
				RemoveIncomingRiver();

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

	public bool HasRiver => HasIncomingRiver || HasOutgoingRiver;
	public bool HasIncomingRiver { get; private set; }

	public bool HasOutgoingRiver { get; private set; }

	public HexDirection IncomingRiver { get; private set; }

	public HexDirection OutgoingRiver { get; private set; }
	public bool HasRiverBeginOrEnd => HasIncomingRiver != HasOutgoingRiver;

	public float StreamBedY =>
		(elevation + HexMetrics.streamBedElevationOffset) *
		HexMetrics.elevationStep;
	public float RiverSurfaceY =>
		(elevation + HexMetrics.riverSurfaceElevationOffset) *
		HexMetrics.elevationStep;

	private void Refresh() {
		if (chunk) {
			chunk.Refresh();
			for (int i = 0; i < neighbors.Length; i++) {
				HexCell neighbor = neighbors[i];
				if (neighbor != null && neighbor.chunk != chunk) neighbor.chunk.Refresh();
			}
		}
	}

	private void RefreshSelfOnly() {
		chunk.Refresh();
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

	public bool HasRiverThroughEdge(HexDirection direction) =>
		(HasIncomingRiver && IncomingRiver == direction) ||
		(HasOutgoingRiver && OutgoingRiver == direction);

	public void SetOutgoingRiver(HexDirection direction) {
		if (HasOutgoingRiver && OutgoingRiver == direction) return;
		HexCell neighbor = GetNeighbor(direction);
		if (!neighbor || elevation < neighbor.elevation) return;
		HasOutgoingRiver = true;
		OutgoingRiver = direction;
		RefreshSelfOnly();

		neighbor.RemoveIncomingRiver();
		neighbor.HasIncomingRiver = true;
		neighbor.IncomingRiver = direction.Opposite();
		neighbor.RefreshSelfOnly();
	}

	public void RemoveOutgoingRiver() {
		if (!HasOutgoingRiver) return;
		HasOutgoingRiver = false;
		RefreshSelfOnly();

		HexCell neighbor = GetNeighbor(OutgoingRiver);
		neighbor.HasIncomingRiver = false;
		neighbor.RefreshSelfOnly();
	}

	public void RemoveIncomingRiver() {
		if (!HasIncomingRiver) return;
		HasIncomingRiver = false;
		RefreshSelfOnly();

		HexCell neighbor = GetNeighbor(IncomingRiver);
		neighbor.HasOutgoingRiver = false;
		neighbor.RefreshSelfOnly();
	}

	public void RemoveRiver() {
		RemoveOutgoingRiver();
		RemoveIncomingRiver();
	}
}