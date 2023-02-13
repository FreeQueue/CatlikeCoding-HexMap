using System.Collections.Generic;

public class HexCellPriorityQueue
{
	private readonly List<HexCell> list = new List<HexCell>();
	private int minimum = int.MaxValue;

	public int Count { get; private set; }

	public void Enqueue(HexCell cell) {
		Count += 1;
		int priority = cell.SearchPriority;
		if (priority < minimum) minimum = priority;
		while (priority >= list.Count) {
			list.Add(null);
		}
		cell.NextWithSamePriority = list[priority];
		list[priority] = cell;
	}

	public HexCell Dequeue() {
		Count -= 1;
		for (; minimum < list.Count; minimum++) {
			HexCell cell = list[minimum];
			if (cell != null) {
				list[minimum] = cell.NextWithSamePriority;
				return cell;
			}
		}
		return null;
	}

	public void Change(HexCell cell, int oldPriority) {
		HexCell current = list[oldPriority];
		HexCell next = current.NextWithSamePriority;
		if (current == cell) {
			list[oldPriority] = next;
		}
		else {
			while (next != cell) {
				current = next;
				next = current.NextWithSamePriority;
			}
			current.NextWithSamePriority = cell.NextWithSamePriority;
		}
		Enqueue(cell);
		Count -= 1;
	}

	public void Clear() {
		list.Clear();
		Count = 0;
		minimum = int.MaxValue;
	}
}