using UnityEngine;

public class BezierCurve
{
	private Vector3[] controlPoints;

	public BezierCurve(Vector3[] waypointsArray)
	{
		controlPoints = new Vector3[waypointsArray.Length];
		for (int i = 0; i < waypointsArray.Length; i++)
		{
			controlPoints[i] = waypointsArray[i];
		}
	}

	public BezierCurve(Vector3[] waypointsArray, bool forGizmos)
	{
		controlPoints = new Vector3[waypointsArray.Length];
		if (forGizmos)
		{
			for (int i = 0; i < waypointsArray.Length; i++)
			{
				controlPoints[i] = waypointsArray[i];
			}
		}
		else
		{
			for (int j = 0; j < waypointsArray.Length; j++)
			{
				controlPoints[j] = waypointsArray[j];
			}
		}
	}

	public Vector3 GetPointAtTime(float t)
	{
		return CreateBenzierForPoint(t);
	}

	private Vector3 CreateBenzierForPoint(float t)
	{
		int num = (int)(t * (float)controlPoints.Length);
		if (num == controlPoints.Length)
		{
			num = 0;
		}
		float t2 = t * (float)controlPoints.Length - (float)num;
		Vector3 vector = controlPoints[CheckWithinArray(num, controlPoints.Length)];
		Vector3 vector2 = controlPoints[CheckWithinArray(num + 1, controlPoints.Length)];
		Vector3 vector3 = controlPoints[CheckWithinArray(num + 2, controlPoints.Length)];
		Vector3 vector4 = controlPoints[CheckWithinArray(num + 3, controlPoints.Length)];
		Vector3 vector5 = (vector3 - vector) * 0.166f;
		Vector3 vector6 = (vector4 - vector2) * 0.166f;
		return DoBenzierForNPoints(t2, new Vector3[4]
		{
			vector2,
			vector2 + vector5,
			vector3 - vector6,
			vector3
		});
	}

	private int CheckWithinArray(int x, int c)
	{
		if (x >= c)
		{
			return x % c;
		}
		return x;
	}

	private Vector3 DoBenzierForNPoints(float t, Vector3[] currentArray)
	{
		Vector3 zero = Vector3.zero;
		float f = 1f - t;
		int num = currentArray.Length - 1;
		for (int i = 0; i <= num; i++)
		{
			zero += BinomialCoefficient(num, i) * Mathf.Pow(f, num - i) * Mathf.Pow(t, i) * currentArray[i];
		}
		return zero;
	}

	public float CaculateTotalLength(float in_singleDeltaTime, float totalTime)
	{
		float num = 0f;
		float num2 = 0f;
		Vector3 zero = Vector3.zero;
		Vector3 vector = controlPoints[0];
		while (num2 < totalTime)
		{
			num2 += in_singleDeltaTime;
			zero = GetPointAtTime(num2 / totalTime);
			num += (zero - vector).magnitude;
			vector = zero;
		}
		return num;
	}

	private float BinomialCoefficient(int n, int k)
	{
		if (k < 0 || k > n)
		{
			return 0f;
		}
		k = ((k <= n / 2) ? k : (n - k));
		return (float)FallingPower(n, k) / (float)Factorial(k);
	}

	private int Factorial(int n)
	{
		if (n == 0)
		{
			return 1;
		}
		int num = n;
		while (n-- > 2)
		{
			num *= n;
		}
		return num;
	}

	private int FallingPower(int n, int p)
	{
		int num = 1;
		for (int i = 0; i < p; i++)
		{
			num *= n--;
		}
		return num;
	}
}
