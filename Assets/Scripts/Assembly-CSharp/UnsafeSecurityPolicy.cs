using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;

internal class UnsafeSecurityPolicy
{
	public static bool Validator(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors policyErrors)
	{
		return true;
	}

	public static void Instate()
	{
		ServicePointManager.ServerCertificateValidationCallback = Validator;
	}
}
