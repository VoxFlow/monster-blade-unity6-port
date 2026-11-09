using System;
using UnityEngine;

public static class ValidationExtensions
{
	private static Validation Check<T>(this Validation validation, Func<bool> filterMethod, T exception) where T : Exception
	{
		if (filterMethod())
		{
			Validation validation2 = validation;
			if (validation2 == null)
			{
				Validation validation3 = new Validation();
				validation3.IsValid = true;
				validation2 = validation3;
			}
			return validation2;
		}
		throw exception;
	}

	public static Validation Check(this Validation validation, Func<bool> filterMethod)
	{
		return validation.Check(filterMethod, new Exception("Parameter InValid!"));
	}

	public static Validation NotNull(this Validation validation, UnityEngine.Object obj)
	{
		return validation.Check(() => obj != null, new ArgumentNullException(string.Format("The object {0} can't be null", obj)));
	}
}
