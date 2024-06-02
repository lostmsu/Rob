namespace RoboZZle.ViewModels;

using System.Net;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;

using RoboZZle.WebService;

public class LevelArchive(HttpClient? http = null) {
	readonly HttpClient http = http ?? new HttpClient(new HttpClientHandler {
		AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
	});

	static readonly DataContractSerializer Serializer = new(typeof(LevelInfo2[]));
	const string XML_URL = "https://github.com/lostmsu/RoboZZle.LevelArchive/raw/master/levels.xml";

	public async Task<LevelInfo2[]> GetLevelsAsync() {
		using var stream = await this.http.GetStreamAsync(XML_URL).ConfigureAwait(false);
		using var reader = XmlReader.Create(stream);
		return (LevelInfo2[])Serializer.ReadObject(reader);
	}
}