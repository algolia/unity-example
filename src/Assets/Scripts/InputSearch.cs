using System.Collections;
using Algolia.Search.Clients;
using Algolia.Search.Models.Search;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine.Windows.Speech;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;

namespace Assets.Scripts
{
    public class InputSearch : MonoBehaviour
    {
        public InputField SearchInput;
        private SearchClient _searchClient;
        private SearchIndex _searchIndex;
        private readonly List<GameObject> _planets = new List<GameObject>();
        private GameObject _viewPort;
        DictationRecognizer dictationRecognizer;

        // Start is called before the first frame update
        void Start()
        {
            var appId = System.Environment.GetEnvironmentVariable("ALGOLIA_APPLICATION_ID");
            var apiKey = System.Environment.GetEnvironmentVariable("ALGOLIA_SEARCH_KEY");

            _searchClient = new SearchClient(appId, apiKey);
            _searchIndex = _searchClient.InitIndex("Planets");
            _viewPort = GameObject.Find("Viewport");

            dictationRecognizer = new DictationRecognizer();
            dictationRecognizer.DictationHypothesis += (text) =>
            {
                if (text.Equals("empty"))
                {
                    SearchInput.text = "";
                    doSearch("");
                    return;
                }

                SearchInput.text = text;
                doSearch(text);
            };

            dictationRecognizer.Start();
        }
        void OnDestroy()
        {
            dictationRecognizer.Stop();
            dictationRecognizer.Dispose();
        }

        // Invoked when the value of the text field changes.
        public void ValueChangeCheck()
        {
            var search = SearchInput.text;

            if (string.IsNullOrWhiteSpace(search))
            {
                _planets?.ForEach(Destroy);
                return;
            }

            doSearch(search);
        }

        public async void doSearch(string text)
        {
            var results = await _searchIndex.SearchAsync<Planet>(new Query(SearchInput.text)
            {
                HitsPerPage = 8,
            });

            if (results.Hits != null)
            {
                LoadResults(results.Hits);
            }
        }

        void LoadResults(List<Planet> planets)
        {
            _planets?.ForEach(Destroy);

            int positionX = 60;
            int paddingX = 190;
            int positionY = -120;
            int j = 0;

            for (int i = 0; i < planets.Count(); i++)
            {
                j++;

                // Create the planet image object
                var planetObject = new GameObject($"PlanetImage{i}");
                var image = planetObject.AddComponent<RawImage>();
                image.color = Color.clear;

                StartCoroutine(LoadTexture(image, i));

                planetObject.transform.SetParent(_viewPort.transform, false);
                _planets?.Add(planetObject);

                // Text under the planet object
                var textplanetObject = new GameObject($"TextPlanet{i}");
                var text = textplanetObject.AddComponent<Text>();
                text.text = $"{planets[i].Name} \n {planets[i].Price} €";
                text.font = Resources.Load<Font>("Jupiter");
                text.fontSize = 16;
                text.alignment = TextAnchor.MiddleCenter;
                text.color = new Color(50f / 255f, 50f / 255f, 50f / 255f);
                textplanetObject.transform.SetParent(_viewPort.transform, false);
                _planets?.Add(textplanetObject);

                // Set position of the objects
                planetObject.transform.localPosition = new Vector3(positionX + paddingX * (j - 1), positionY);
                textplanetObject.transform.localPosition = new Vector3(positionX + paddingX * (j - 1), positionY - 65);
                textplanetObject.transform.GetComponent<RectTransform>().sizeDelta = new Vector2(110, 100);

                if (j == 4)
                {
                    j = 0;
                    positionY += positionY - 50;
                }
            }

            IEnumerator LoadTexture(RawImage image, int i)
            {
                var resourceRequest = Resources.LoadAsync<Texture2D>($"{planets[i].Path}");
                while (!resourceRequest.isDone)
                {
                    yield return 0;
                }

                if (image != null)
                {
                    image.texture = resourceRequest.asset as Texture2D;
                    image.color = Color.white;
                }
            }
        }
    }
}