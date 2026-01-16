using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace INab.Demo
{
    public class TestSceneManager : MonoBehaviour
    {
        public List<GameObject> objectsToShow = new List<GameObject>();

        public Text text;

        private int currentObjectActiveID = 0;

        // Start is called before the first frame update
        void Start()
        {
            foreach (var obj in objectsToShow)
            {
                obj.SetActive(false);
            }

            if (objectsToShow.Count > 0) objectsToShow[0].SetActive(true);
        }

        // Update is called once per frame
        void Update()
        {
            if (objectsToShow.Count < 1) return;

            text.text = (currentObjectActiveID + 1).ToString() + " / " + objectsToShow.Count.ToString();

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                Previous();
            }

            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                Next();
            }
        }

        public void Next()
        {
            objectsToShow[currentObjectActiveID].SetActive(false);
            currentObjectActiveID++;

            currentObjectActiveID = currentObjectActiveID % objectsToShow.Count;

            objectsToShow[currentObjectActiveID].SetActive(true);
        }

        public void Previous()
        {
            objectsToShow[currentObjectActiveID].SetActive(false);
            currentObjectActiveID--;
            if (currentObjectActiveID < 0) currentObjectActiveID = objectsToShow.Count - 1;

            objectsToShow[currentObjectActiveID].SetActive(true);
        }
    }
}