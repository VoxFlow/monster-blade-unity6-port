// The original runtime's (Mono 2.x) quicksort, so elements that compare equal keep the original order. Register CD-055.
using System;
using System.Collections.Generic;

public static class MBSort
{
	public static void Sort<T>(List<T> list)
	{
		Sort(list, Comparer<T>.Default.Compare);
	}

	public static void Sort<T>(List<T> list, Comparison<T> comparison)
	{
		if (comparison == null) throw new ArgumentNullException("comparison");
		if (list.Count < 2) return;
		T[] a = list.ToArray();
		QSort(a, 0, a.Length - 1, comparison);
		for (int i = 0; i < a.Length; i++) list[i] = a[i];
	}

	public static void Sort<T>(List<T> list, IComparer<T> comparer)
	{
		Sort(list, (comparer ?? Comparer<T>.Default).Compare);
	}

	public static void Sort<T>(T[] array)
	{
		Sort(array, Comparer<T>.Default.Compare);
	}

	public static void Sort<T>(T[] array, Comparison<T> comparison)
	{
		if (array == null) throw new ArgumentNullException("array");
		if (comparison == null) throw new ArgumentNullException("comparison");
		QSort(array, 0, array.Length - 1, comparison);
	}

	private static void QSort<T>(T[] a, int low0, int high0, Comparison<T> comparison)
	{
		if (low0 >= high0) return;
		int low = low0, high = high0;
		T pivot = a[low + (high - low) / 2];
		while (true)
		{
			while (low < high0 && comparison(a[low], pivot) < 0) low++;
			while (high > low0 && comparison(pivot, a[high]) < 0) high--;
			if (low <= high)
			{
				T t = a[low]; a[low] = a[high]; a[high] = t;
				low++;
				high--;
			}
			else break;
		}
		if (low0 < high) QSort(a, low0, high, comparison);
		if (low < high0) QSort(a, low, high0, comparison);
	}
}
