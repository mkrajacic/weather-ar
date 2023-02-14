using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine.UI;
using UnityEngine.Networking;
using SimpleJSON;
using TMPro;
using UnityEngine.SceneManagement;

public class WebCamTextureToCloudVision : MonoBehaviour {

	public string url = "https://vision.googleapis.com/v1/images:annotate?key=";
	public string apiKey = "";
	public float captureIntervalSeconds = 0.02f;
	public int requestedWidth = 1280;
	public int requestedHeight = 960;
	public FeatureType featureType = FeatureType.TEXT_DETECTION;
	public int maxResults = 9;
	public GameObject resPanel;
	List<GameObject> weather_elements = new List<GameObject>();
	public Button pause_button;
	public Text pause_text;
	public Button menu_button;

	public GameObject weather_element;
	bool paused = false;

	WebCamTexture webcamTexture;
	Texture2D texture2D;
	Dictionary<string, string> headers;

	[System.Serializable]
	public class AnnotateImageRequests {
		public List<AnnotateImageRequest> requests;
	}

	[System.Serializable]
	public class AnnotateImageRequest {
		public Image image;
		public List<Feature> features;
	}

	[System.Serializable]
	public class Image {
		public string content;
	}

	[System.Serializable]
	public class Feature {
		public string type;
		public int maxResults;
	}

	public enum FeatureType {
		TYPE_UNSPECIFIED,
		FACE_DETECTION,
		LANDMARK_DETECTION,
		LOGO_DETECTION,
		LABEL_DETECTION,
		TEXT_DETECTION,
		SAFE_SEARCH_DETECTION,
		IMAGE_PROPERTIES
	}

	void Start () {

		pause_button.onClick.AddListener(Pause);
		menu_button.onClick.AddListener(ReturnToMenu);

		headers = new Dictionary<string, string>();
		headers.Add("Content-Type", "application/json; charset=UTF-8");

		apiKey = "";
		weather_element.SetActive(false);

		if (apiKey == null || apiKey == "")
			Debug.LogError("No API key. Please set your API key into the \"Web Cam Texture To Cloud Vision(Script)\" component.");
		
		WebCamDevice[] devices = WebCamTexture.devices;
		for (var i = 0; i < devices.Length; i++) {
			Debug.Log (devices [i].name);
		}
		if (devices.Length > 0) {

			webcamTexture = new WebCamTexture(devices[0].name, requestedWidth,requestedHeight);
			Renderer r = GetComponent<Renderer> ();
			if (r != null) {
				Material m = r.material;
				if (m != null) {
					m.mainTexture = webcamTexture;
				}
			}
			
			webcamTexture.Play();
			StartCoroutine("Capture");
		}
	}
	
	void Update () {

	}

	private IEnumerator Capture() {
		while (true) {

			while(paused){
				yield return null;
			}

			if (this.apiKey == null)
				yield return null;

		bool weatherEmpty = !weather_elements.Any();

			yield return new WaitForSeconds(captureIntervalSeconds);

						if(!weatherEmpty){
											foreach(var e in weather_elements) {
												Destroy(e);
											}
			}


			Color[] pixels = webcamTexture.GetPixels();
			if (pixels.Length == 0)
				yield return null;
			if (texture2D == null || webcamTexture.width != texture2D.width || webcamTexture.height != texture2D.height) {
				texture2D = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.RGBA32, false);
			}

			texture2D.SetPixels(pixels);
			byte[] jpg = texture2D.EncodeToJPG();
			string base64 = System.Convert.ToBase64String(jpg);

			AnnotateImageRequests requests = new AnnotateImageRequests();
			requests.requests = new List<AnnotateImageRequest>();

			AnnotateImageRequest request = new AnnotateImageRequest();
			request.image = new Image();
			request.image.content = base64;
			request.features = new List<Feature>();
			Feature feature = new Feature();
			feature.type = this.featureType.ToString();
			feature.maxResults = this.maxResults;
			request.features.Add(feature); 
			requests.requests.Add(request);

			string jsonData = JsonUtility.ToJson(requests, false);

			if (jsonData != string.Empty) {
				string url = this.url + this.apiKey;
				byte[] postData = System.Text.Encoding.Default.GetBytes(jsonData);
				using(WWW www = new WWW(url, postData, headers)) {
					yield return www;
					if (string.IsNullOrEmpty(www.error)) {
						string responses = www.text.Replace("\n", "").Replace(" ", "");

						JSONNode res = JSON.Parse(responses);
						string fullText = res["responses"][0]["textAnnotations"][0]["description"].ToString().Trim('"');
						if (fullText != ""){
							Debug.Log("OCR Response: " + fullText);

							resPanel.SetActive(true);
							fullText = fullText.Replace("\\n", ";");
							string[] texts = fullText.Split(';');

							GameObject g;

							for(int i=0;i<texts.Length;i++){

								if((texts[i].Any(char.IsDigit))){
									texts[i] = RemoveDigits(texts[i]);
								}

								if((texts[i].Contains("/"))){
									string[] temp = texts[i].Split('/');
									texts[i] = temp[0];
								}
								
								if((texts[i].Contains("\\"))) {
									string[] temp = texts[i].Split('\\');
									texts[i] = temp[0];
								}

								if((texts[i].Contains("-"))) {
									string[] temp = texts[i].Split('-');
									texts[i] = temp[0];
								}
								
								
								 if((Regex.Matches(texts[i], @"[čćžšđáéóöőúüű]").Count)>0){
												texts[i] = Regex.Replace(texts[i], "č", "c", RegexOptions.IgnoreCase);
												texts[i] = Regex.Replace(texts[i], "ć", "c", RegexOptions.IgnoreCase);
												texts[i] = Regex.Replace(texts[i], "š", "s", RegexOptions.IgnoreCase);
												texts[i] = Regex.Replace(texts[i], "dž", "dz", RegexOptions.IgnoreCase);
												texts[i] = Regex.Replace(texts[i], "ž", "z", RegexOptions.IgnoreCase);
												texts[i] = Regex.Replace(texts[i], "á", "a", RegexOptions.IgnoreCase);
												texts[i] = Regex.Replace(texts[i], "é", "e", RegexOptions.IgnoreCase);
												texts[i] = Regex.Replace(texts[i], "ó", "o", RegexOptions.IgnoreCase);
												texts[i] = Regex.Replace(texts[i], "ö", "o", RegexOptions.IgnoreCase);
												texts[i] = Regex.Replace(texts[i], "ő", "o", RegexOptions.IgnoreCase);
												texts[i] = Regex.Replace(texts[i], "ú", "u", RegexOptions.IgnoreCase);
												texts[i] = Regex.Replace(texts[i], "ü", "u", RegexOptions.IgnoreCase);
												texts[i] = Regex.Replace(texts[i], "ű", "u", RegexOptions.IgnoreCase);
								}

								if((texts[i].ToCharArray().Where(c => Char.IsUpper(c)).Count())>1){
									string[] temp = Regex.Split(texts[i], @"(?<!^)(?=[A-Z])");

									var f = 0;
									foreach(string s in temp) {
										if((s.Length)==1) {
											temp[f] = "";
										}else if(s == "HR"){
											temp[f] = "";
										}
										f++;
									}

									texts[i] = string.Join(" ", temp);
									texts[i] = texts[i].Trim();
							}
									Debug.Log("to weather api ");
									Debug.Log(texts[i]);


										string weatherURL = "http://api.weatherapi.com/v1/current.json?key=key&q=" + texts[i] + "&aqi=no";

										var delay = new WaitForSecondsRealtime(0.08f);
										var t = Task.Run(async () => await GetRequest(weatherURL));
										yield return new WaitUntil(() => t.IsCompleted);
										string weatherText = t.Result;
										yield return delay;					

										JSONNode weatherRes = JSON.Parse(weatherText);
										string extracted_weather_text;
										string extracted_weather_temp;

										Debug.Log(weatherRes);

										if(weatherRes!=null){
											extracted_weather_text = weatherRes["current"]["condition"]["text"].ToString().Trim('"');
											extracted_weather_temp = weatherRes["current"]["temp_c"];
											g = Instantiate(weather_element,resPanel.transform) as GameObject;
											g.SetActive(true);

											string iconURL = weatherRes["current"]["condition"]["icon"];

											delay = new WaitForSecondsRealtime(0.02f);
											var tex = Task.Run(async () => await GetTextureRequest(iconURL));
											yield return new WaitUntil(() => tex.IsCompleted);
											Texture2D icon = tex.Result;
											yield return delay;					

											Sprite sprite = Sprite.Create(icon, (new Rect(0,0,icon.width,icon.height)),new Vector2( 0.5f, 0.5f ));
											g.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>().sprite = sprite;
											g.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = texts[i];
											g.transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = extracted_weather_text + " - " + extracted_weather_temp + " °C";
											weather_elements.Add(g);

										if(i == texts.Length - 1){
											extracted_weather_temp = "";
											extracted_weather_text = "";
											texts[i] = "";
										}}

							}
						}
					} else {
						Debug.Log("Error: " + www.error);
					}
				}
			}
		}
	}

	async Task<string> GetRequest(string url) {
		using var www = UnityWebRequest.Get(url);
		www.SetRequestHeader("Content-Type","application/json");

		var operation = www.SendWebRequest();

		while(!operation.isDone){
			await Task.Yield();
		}

		if(www.result == UnityWebRequest.Result.Success) {
			return www.downloadHandler.text;
		}else{
			return www.error;
		}
	}

		async Task<Texture2D> GetTextureRequest(string url) {
		using var www = UnityWebRequestTexture.GetTexture(url);
		www.SetRequestHeader("Content-Type","application/json");

		var operation = www.SendWebRequest();

		while(!operation.isDone){
			await Task.Yield();
		}

		if(www.result == UnityWebRequest.Result.Success) {
			return ((DownloadHandlerTexture)www.downloadHandler).texture;
		}else{
			return null;
		}
	}

	 public static string RemoveDigits(string key)
    {
        return Regex.Replace(key, @"[^A-Z]+", String.Empty);
    }

	public void Pause() {

		if(!paused) {
			paused = true;
			pause_text.text = "Start";

			StopCoroutine("Capture");

		}else{
			paused = false;
			pause_text.text = "Pause";

			StartCoroutine("Capture");
		}

	}

	public void ReturnToMenu() {
        SceneManager.LoadScene("Menu");
	}

}
