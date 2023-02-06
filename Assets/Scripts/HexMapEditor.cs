using UnityEngine;
using UnityEngine.EventSystems;

public class HexMapEditor : MonoBehaviour
{
	#region Serialized Fields
	public Color[] colors;

	public HexGrid hexGrid;
	#endregion

	private Color activeColor;

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
		if (Physics.Raycast(inputRay, out hit)) hexGrid.ColorCell(hit.point, activeColor);
	}

	public void SelectColor(int index) {
		activeColor = colors[index];
	}
}