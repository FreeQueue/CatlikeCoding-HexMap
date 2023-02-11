using UnityEngine;

public class HexGridChunk : MonoBehaviour
{
	private HexCell[] cells;
	private Canvas gridCanvas;

	private HexMesh hexMesh;

	#region Event Functions
	private void Awake() {
		gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();

		cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
		ShowUI(false);
	}

	private void LateUpdate() {
		hexMesh.Triangulate(cells);
		enabled = false;
	}
	#endregion

	public void Refresh() {
		enabled = true;
	}

	public void ShowUI(bool visible) {
		gridCanvas.gameObject.SetActive(visible);
	}

	public void AddCell(int index, HexCell cell) {
		cells[index] = cell;
		cell.chunk = this;
		cell.transform.SetParent(transform, false);
		cell.uiRect.SetParent(gridCanvas.transform, false);
	}
}