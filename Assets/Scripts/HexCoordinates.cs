using System;
using System.IO;
using UnityEngine;

[Serializable]
public struct HexCoordinates
{
	#region Serialized Fields
	[SerializeField] private int x, z;
	#endregion

	public HexCoordinates(int x, int z) {
		this.x = x;
		this.z = z;
	}

	public int X => x;

	public int Z => z;

	public int Y => -X - Z;

	public int DistanceTo(HexCoordinates other) =>
		((x < other.x ? other.x - x : x - other.x) +
		(Y < other.Y ? other.Y - Y : Y - other.Y) +
		(z < other.z ? other.z - z : z - other.z)) / 2;

	public static HexCoordinates FromOffsetCoordinates(int x, int z) => new HexCoordinates(x - z / 2, z);

	public static HexCoordinates FromPosition(Vector3 position) {
		float x = position.x / (HexMetrics.innerRadius * 2f);
		float y = -x;

		float offset = position.z / (HexMetrics.outerRadius * 3f);
		x -= offset;
		y -= offset;

		int iX = Mathf.RoundToInt(x);
		int iY = Mathf.RoundToInt(y);
		int iZ = Mathf.RoundToInt(-x - y);

		if (iX + iY + iZ != 0) {
			float dX = Mathf.Abs(x - iX);
			float dY = Mathf.Abs(y - iY);
			float dZ = Mathf.Abs(-x - y - iZ);

			if (dX > dY && dX > dZ)
				iX = -iY - iZ;
			else if (dZ > dY) iZ = -iX - iY;
		}

		return new HexCoordinates(iX, iZ);
	}

	public override string ToString() =>
		"(" +
		X + ", " + Y + ", " + Z + ")";

	public string ToStringOnSeparateLines() => X + "\n" + Y + "\n" + Z;

	public void Save(BinaryWriter writer) {
		writer.Write(x);
		writer.Write(z);
	}

	public static HexCoordinates Load(BinaryReader reader) {
		HexCoordinates c;
		c.x = reader.ReadInt32();
		c.z = reader.ReadInt32();
		return c;
	}
}