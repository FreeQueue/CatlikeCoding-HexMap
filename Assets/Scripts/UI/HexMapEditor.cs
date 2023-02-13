﻿using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
	#region Serialized Fields
	public HexGrid hexGrid;

	public Material terrainMaterial;
	#endregion

	private int activeElevation;

	private int activeTerrainTypeIndex;

	private int activeUrbanLevel, activeFarmLevel, activePlantLevel, activeSpecialIndex;
	private int activeWaterLevel;

	private bool applyElevation = true;

	private bool applyUrbanLevel, applyFarmLevel, applyPlantLevel, applySpecialIndex;
	private bool applyWaterLevel = true;

	private int brushSize;
	private HexDirection dragDirection;

	private bool isDrag;
	private HexCell previousCell;

	private OptionalToggle riverMode, roadMode, walledMode;

	#region Event Functions
	private void Awake() {
		terrainMaterial.DisableKeyword("GRID_ON");
		SetEditMode(false);
	}

	private void Update() {
		if (!EventSystem.current.IsPointerOverGameObject()) {
			if (Input.GetMouseButton(0)) {
				HandleInput();
				return;
			}
			if (Input.GetKeyDown(KeyCode.U)) {
				if (Input.GetKey(KeyCode.LeftShift))
					DestroyUnit();
				else
					CreateUnit();
				return;
			}
		}
		previousCell = null;
	}
	#endregion

	public void SetTerrainTypeIndex(int index) {
		activeTerrainTypeIndex = index;
	}

	public void SetApplyElevation(bool toggle) {
		applyElevation = toggle;
	}

	public void SetElevation(float elevation) {
		activeElevation = (int)elevation;
	}

	public void SetApplyWaterLevel(bool toggle) {
		applyWaterLevel = toggle;
	}

	public void SetWaterLevel(float level) {
		activeWaterLevel = (int)level;
	}

	public void SetApplyUrbanLevel(bool toggle) {
		applyUrbanLevel = toggle;
	}

	public void SetUrbanLevel(float level) {
		activeUrbanLevel = (int)level;
	}

	public void SetApplyFarmLevel(bool toggle) {
		applyFarmLevel = toggle;
	}

	public void SetFarmLevel(float level) {
		activeFarmLevel = (int)level;
	}

	public void SetApplyPlantLevel(bool toggle) {
		applyPlantLevel = toggle;
	}

	public void SetPlantLevel(float level) {
		activePlantLevel = (int)level;
	}

	public void SetApplySpecialIndex(bool toggle) {
		applySpecialIndex = toggle;
	}

	public void SetSpecialIndex(float index) {
		activeSpecialIndex = (int)index;
	}

	public void SetBrushSize(float size) {
		brushSize = (int)size;
	}

	public void SetRiverMode(int mode) {
		riverMode = (OptionalToggle)mode;
	}

	public void SetRoadMode(int mode) {
		roadMode = (OptionalToggle)mode;
	}

	public void SetWalledMode(int mode) {
		walledMode = (OptionalToggle)mode;
	}

	public void SetEditMode(bool toggle) {
		enabled = toggle;
	}

	public void ShowGrid(bool visible) {
		if (visible)
			terrainMaterial.EnableKeyword("GRID_ON");
		else
			terrainMaterial.DisableKeyword("GRID_ON");
	}

	private HexCell GetCellUnderCursor() => hexGrid.GetCell(Camera.main.ScreenPointToRay(Input.mousePosition));

	private void CreateUnit() {
		HexCell cell = GetCellUnderCursor();
		if (cell && !cell.Unit)
			hexGrid.AddUnit(
				Instantiate(HexUnit.unitPrefab), cell, Random.Range(0f, 360f)
			);
	}

	private void DestroyUnit() {
		HexCell cell = GetCellUnderCursor();
		if (cell && cell.Unit) hexGrid.RemoveUnit(cell.Unit);
	}

	private void HandleInput() {
		HexCell currentCell = GetCellUnderCursor();
		if (currentCell) {
			if (previousCell && previousCell != currentCell)
				ValidateDrag(currentCell);
			else
				isDrag = false;
			EditCells(currentCell);
			previousCell = currentCell;
		}
		else {
			previousCell = null;
		}
	}

	private void ValidateDrag(HexCell currentCell) {
		for (
			dragDirection = HexDirection.NE;
			dragDirection <= HexDirection.NW;
			dragDirection++
		) {
			if (previousCell.GetNeighbor(dragDirection) == currentCell) {
				isDrag = true;
				return;
			}
		}
		isDrag = false;
	}

	private void EditCells(HexCell center) {
		int centerX = center.coordinates.X;
		int centerZ = center.coordinates.Z;

		for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++) {
			for (int x = centerX - r; x <= centerX + brushSize; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}
		for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++) {
			for (int x = centerX - brushSize; x <= centerX + r; x++) {
				EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
			}
		}
	}

	private void EditCell(HexCell cell) {
		if (cell) {
			if (activeTerrainTypeIndex >= 0) cell.TerrainTypeIndex = activeTerrainTypeIndex;
			if (applyElevation) cell.Elevation = activeElevation;
			if (applyWaterLevel) cell.WaterLevel = activeWaterLevel;
			if (applySpecialIndex) cell.SpecialIndex = activeSpecialIndex;
			if (applyUrbanLevel) cell.UrbanLevel = activeUrbanLevel;
			if (applyFarmLevel) cell.FarmLevel = activeFarmLevel;
			if (applyPlantLevel) cell.PlantLevel = activePlantLevel;
			if (riverMode == OptionalToggle.No) cell.RemoveRiver();
			if (roadMode == OptionalToggle.No) cell.RemoveRoads();
			if (walledMode != OptionalToggle.Ignore) cell.Walled = walledMode == OptionalToggle.Yes;
			if (isDrag) {
				HexCell otherCell = cell.GetNeighbor(dragDirection.Opposite());
				if (otherCell) {
					if (riverMode == OptionalToggle.Yes) otherCell.SetOutgoingRiver(dragDirection);
					if (roadMode == OptionalToggle.Yes) otherCell.AddRoad(dragDirection);
				}
			}
		}
	}

	#region Nested type: ${0}
	private enum OptionalToggle
	{
		Ignore, Yes, No,
	}
	#endregion
}