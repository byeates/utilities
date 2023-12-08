using Newtonsoft.Json.Linq;

namespace Code.Common.Storage
{
	public class PersistentJsonData : PersistentData<JObject>
	{
		/// <inheritdoc/>
		public override void HandledLoadedFile()
		{
			data = JObject.Parse(rawData);
		}

		/// <inheritdoc/>
		public override void HandleFileNotFound()
		{
			data = new JObject();
		}
	}
}