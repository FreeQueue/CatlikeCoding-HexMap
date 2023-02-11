using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
	#region Serialized Fields
	public Color[] colors;

	public HexGrid hexGrid;
	#endregion

	private Color activeColor;

	private int activeElevation;
	private bool applyColor;
	private bool applyElevation = true;
	private int brushSize;

	#region Event Functions
	private void Awake() {
		SelectColor(0);
	}

	private void Update() {
		if (Input.GetMouseButtonDown(0) &&
			!EventSystem.current.IsPointerOverGameObject())
			HandleInput();
	}
	#endregion

	private void HandleInput() {
		Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
		RaycastHit hit;
		if (Physics.Raycast(inputRay, out hit)) EditCells(hexGrid.GetCell(hit.point));
	}

	public void SetElevation(float elevation) {
		activeElevation = (int)elevation;
	}

	public void SelectColor(int index) {
		applyColor = index >= 0;
		if (applyColor) activeColor = colors[index];
	}

	public void SetApplyElevation(bool toggle) {
		applyElevation = toggle;
	}

	public void SetBrushSize(float size) {
		brushSize = (int)size;
	}

	public void ShowUI(bool visible) {
		hexGrid.ShowUI(visible);
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
			if (applyColor) cell.Color = activeColor;
			if (applyElevation) cell.Elevation = activeElevation;
		}
	}
}