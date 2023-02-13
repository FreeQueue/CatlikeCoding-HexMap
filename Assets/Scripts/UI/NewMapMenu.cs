using UnityEngine;

public class NewMapMenu : MonoBehaviour
{
	#region Serialized Fields
	public HexGrid hexGrid;
	#endregion

	public void Open() {
		gameObject.SetActive(true);
		HexMapCamera.Locked = true;
	}

	public void Close() {
		gameObject.SetActive(false);
		HexMapCamera.Locked = false;
	}

	public void CreateSmallMap() {
		CreateMap(20, 15);
	}

	public void CreateMediumMap() {
		CreateMap(40, 30);
	}

	public void CreateLargeMap() {
		CreateMap(80, 60);
	}

	private void CreateMap(int x, int z) {
		hexGrid.CreateMap(x, z);
		HexMapCamera.ValidatePosition();
		Close();
	}
}