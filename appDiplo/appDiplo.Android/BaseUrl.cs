using Xamarin.Forms;
using appDiplo.Droid;

[assembly: Dependency(typeof(BaseUrl_Android))]
namespace appDiplo.Droid
{
	public class BaseUrl_Android : IBaseUrl
	{
		public string Get()
		{
			return "file:///android_asset/";
		}
	}
}