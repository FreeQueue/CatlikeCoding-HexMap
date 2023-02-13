using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class HexCell : MonoBehaviour
{
	#region Serialized Fields
	public HexCoordinates coordinates;

	public RectTransform uiRect;

	public HexGridChunk chunk;

	[SerializeField] private HexCell[] neighbors;

	[SerializeField] private bool[] roads;
	#endregion

	private int elevation = int.MinValue;

	private bool explored;

	private int specialIndex;

	private int terrainTypeIndex;

	private int urbanLevel, farmLevel, plantLevel;

	private int visibility;

	private bool walled;
	private int waterLevel;

	public int Index { get; set; }

	public int Elevation {
		get => elevation;
		set {
			if (elevation == value) return;
			int originalViewElevation = ViewElevation;
			elevation = value;
			if (ViewElevation != originalViewElevation) ShaderData.ViewElevationChanged();
			RefreshPosition();
			ValidateRivers();

			for (int i = 0; i < roads.Length; i++) {
				if (roads[i] && GetElevationDifference((HexDirection)i) > 1) SetRoad(i, false);
			}

			Refresh();
		}
	}

	public int WaterLevel {
		get => waterLevel;
		set {
			if (waterLevel == value) return;
			int originalViewElevation = ViewElevation;
			waterLevel = value;
			if (ViewElevation != originalViewElevation) ShaderData.ViewElevationChanged();
			ValidateRivers();
			Refresh();
		}
	}

	public int ViewElevation => elevation >= waterLevel ? elevation : waterLevel;

	public bool IsUnderwater => waterLevel > elevation;

	public bool HasIncomingRiver { get; private set; }

	public bool HasOutgoingRiver { get; private set; }

	public bool HasRiver => HasIncomingRiver || HasOutgoingRiver;

	public bool HasRiverBeginOrEnd => HasIncomingRiver != HasOutgoingRiver;

	public HexDirection RiverBeginOrEndDirection => HasIncomingRiver ? IncomingRiver : OutgoingRiver;

	public bool HasRoads {
		get {
			for (int i = 0; i < roads.Length; i++) {
				if (roads[i]) return true;
			}
			return false;
		}
	}

	public HexDirection IncomingRiver { get; private set; }

	public HexDirection OutgoingRiver { get; private set; }

	public Vector3 Position => transform.localPosition;

	public float StreamBedY =>
		(elevation + HexMetrics.streamBedElevationOffset) *
		HexMetrics.elevationStep;

	public float RiverSurfaceY =>
		(elevation + HexMetrics.waterElevationOffset) *
		HexMetrics.elevationStep;

	public float WaterSurfaceY =>
		(waterLevel + HexMetrics.waterElevationOffset) *
		HexMetrics.elevationStep;

	public int UrbanLevel {
		get => urbanLevel;
		set {
			if (urbanLevel != value) {
				urbanLevel = value;
				RefreshSelfOnly();
			}
		}
	}

	public int FarmLevel {
		get => farmLevel;
		set {
			if (farmLevel != value) {
				farmLevel = value;
				RefreshSelfOnly();
			}
		}
	}

	public int PlantLevel {
		get => plantLevel;
		set {
			if (plantLevel != value) {
				plantLevel = value;
				RefreshSelfOnly();
			}
		}
	}

	public int SpecialIndex {
		get => specialIndex;
		set {
			if (specialIndex != value && !HasRiver) {
				specialIndex = value;
				RemoveRoads();
				RefreshSelfOnly();
			}
		}
	}

	public bool IsSpecial => specialIndex > 0;

	public bool Walled {
		get => walled;
		set {
			if (walled != value) {
				walled = value;
				Refresh();
			}
		}
	}

	public int TerrainTypeIndex {
		get => terrainTypeIndex;
		set {
			if (terrainTypeIndex != value) {
				terrainTypeIndex = value;
				ShaderData.RefreshTerrain(this);
			}
		}
	}

	public bool IsVisible => visibility > 0 && Explorable;

	public bool IsExplored { get => explored && Explorable; private set => explored = value; }

	public bool Explorable { get; set; }

	public int Distance { get; set; }

	public HexUnit Unit { get; set; }

	public HexCell PathFrom { get; set; }

	public int SearchHeuristic { get; set; }

	public int SearchPriority => Distance + SearchHeuristic;

	public int SearchPhase { get; set; }

	public HexCell NextWithSamePriority { get; set; }

	public HexCellShaderData ShaderData { get; set; }

	public void IncreaseVisibility() {
		visibility += 1;
		if (visibility == 1) {
			IsExplored = true;
			ShaderData.RefreshVisibility(this);
		}
	}

	public void DecreaseVisibility() {
		visibility -= 1;
		if (visibility == 0) ShaderData.RefreshVisibility(this);
	}

	public void ResetVisibility() {
		if (visibility > 0) {
			visibility = 0;
			ShaderData.RefreshVisibility(this);
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

	public bool HasRiverThroughEdge(HexDirection direction) =>
		(HasIncomingRiver && IncomingRiver == direction) ||
		(HasOutgoingRiver && OutgoingRiver == direction);

	public void RemoveIncomingRiver() {
		if (!HasIncomingRiver) return;
		HasIncomingRiver = false;
		RefreshSelfOnly();

		HexCell neighbor = GetNeighbor(IncomingRiver);
		neighbor.HasOutgoingRiver = false;
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

	public void RemoveRiver() {
		RemoveOutgoingRiver();
		RemoveIncomingRiver();
	}

	public void SetOutgoingRiver(HexDirection direction) {
		if (HasOutgoingRiver && OutgoingRiver == direction) return;

		HexCell neighbor = GetNeighbor(direction);
		if (!IsValidRiverDestination(neighbor)) return;

		RemoveOutgoingRiver();
		if (HasIncomingRiver && IncomingRiver == direction) RemoveIncomingRiver();
		HasOutgoingRiver = true;
		OutgoingRiver = direction;
		specialIndex = 0;

		neighbor.RemoveIncomingRiver();
		neighbor.HasIncomingRiver = true;
		neighbor.IncomingRiver = direction.Opposite();
		neighbor.specialIndex = 0;

		SetRoad((int)direction, false);
	}

	public bool HasRoadThroughEdge(HexDirection direction) => roads[(int)direction];

	public void AddRoad(HexDirection direction) {
		if (
			!roads[(int)direction] && !HasRiverThroughEdge(direction) &&
			!IsSpecial && !GetNeighbor(direction).IsSpecial &&
			GetElevationDifference(direction) <= 1
		)
			SetRoad((int)direction, true);
	}

	public void RemoveRoads() {
		for (int i = 0; i < neighbors.Length; i++) {
			if (roads[i]) SetRoad(i, false);
		}
	}

	public int GetElevationDifference(HexDirection direction) {
		int difference = elevation - GetNeighbor(direction).elevation;
		return difference >= 0 ? difference : -difference;
	}

	private bool IsValidRiverDestination(HexCell neighbor) =>
		neighbor && (
			elevation >= neighbor.elevation || waterLevel == neighbor.elevation
		);

	private void ValidateRivers() {
		if (
			HasOutgoingRiver &&
			!IsValidRiverDestination(GetNeighbor(OutgoingRiver))
		)
			RemoveOutgoingRiver();
		if (
			HasIncomingRiver &&
			!GetNeighbor(IncomingRiver).IsValidRiverDestination(this)
		)
			RemoveIncomingRiver();
	}

	private void SetRoad(int index, bool state) {
		roads[index] = state;
		neighbors[index].roads[(int)((HexDirection)index).Opposite()] = state;
		neighbors[index].RefreshSelfOnly();
		RefreshSelfOnly();
	}

	private void RefreshPosition() {
		Vector3 position = transform.localPosition;
		position.y = elevation * HexMetrics.elevationStep;
		position.y +=
			(HexMetrics.SampleNoise(position).y * 2f - 1f) *
			HexMetrics.elevationPerturbStrength;
		transform.localPosition = position;

		Vector3 uiPosition = uiRect.localPosition;
		uiPosition.z = -position.y;
		uiRect.localPosition = uiPosition;
	}

	private void Refresh() {
		if (chunk) {
			chunk.Refresh();
			for (int i = 0; i < neighbors.Length; i++) {
				HexCell neighbor = neighbors[i];
				if (neighbor != null && neighbor.chunk != chunk) neighbor.chunk.Refresh();
			}
			if (Unit) Unit.ValidateLocation();
		}
	}

	private void RefreshSelfOnly() {
		chunk.Refresh();
		if (Unit) Unit.ValidateLocation();
	}

	public void Save(BinaryWriter writer) {
		writer.Write((byte)terrainTypeIndex);
		writer.Write((byte)elevation);
		writer.Write((byte)waterLevel);
		writer.Write((byte)urbanLevel);
		writer.Write((byte)farmLevel);
		writer.Write((byte)plantLevel);
		writer.Write((byte)specialIndex);
		writer.Write(walled);

		if (HasIncomingRiver)
			writer.Write((byte)(IncomingRiver + 128));
		else
			writer.Write((byte)0);

		if (HasOutgoingRiver)
			writer.Write((byte)(OutgoingRiver + 128));
		else
			writer.Write((byte)0);

		int roadFlags = 0;
		for (int i = 0; i < roads.Length; i++) {
			if (roads[i]) roadFlags |= 1 << i;
		}
		writer.Write((byte)roadFlags);
		writer.Write(IsExplored);
	}

	public void Load(BinaryReader reader, int header) {
		terrainTypeIndex = reader.ReadByte();
		ShaderData.RefreshTerrain(this);
		elevation = reader.ReadByte();
		RefreshPosition();
		waterLevel = reader.ReadByte();
		urbanLevel = reader.ReadByte();
		farmLevel = reader.ReadByte();
		plantLevel = reader.ReadByte();
		specialIndex = reader.ReadByte();
		walled = reader.ReadBoolean();

		byte riverData = reader.ReadByte();
		if (riverData >= 128) {
			HasIncomingRiver = true;
			IncomingRiver = (HexDirection)(riverData - 128);
		}
		else {
			HasIncomingRiver = false;
		}

		riverData = reader.ReadByte();
		if (riverData >= 128) {
			HasOutgoingRiver = true;
			OutgoingRiver = (HexDirection)(riverData - 128);
		}
		else {
			HasOutgoingRiver = false;
		}

		int roadFlags = reader.ReadByte();
		for (int i = 0; i < roads.Length; i++) {
			roads[i] = (roadFlags & (1 << i)) != 0;
		}

		IsExplored = header >= 3 && reader.ReadBoolean();
		ShaderData.RefreshVisibility(this);
	}

	public void SetLabel(string text) {
		var label = uiRect.GetComponent<Text>();
		label.text = text;
	}

	public void DisableHighlight() {
		var highlight = uiRect.GetChild(0).GetComponent<Image>();
		highlight.enabled = false;
	}

	public void EnableHighlight(Color color) {
		var highlight = uiRect.GetChild(0).GetComponent<Image>();
		highlight.color = color;
		highlight.enabled = true;
	}
}