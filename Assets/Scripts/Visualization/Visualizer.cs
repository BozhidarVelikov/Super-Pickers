using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SuperPickers {
    public class Visualizer : MonoBehaviour {
        private static Visualizer instance;

        [SerializeField] private GameObject binPrefab;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private TextAsset jsonFile;
        [SerializeField] private TextMeshProUGUI productNameText;
        [SerializeField] private TextMeshProUGUI binNameText;
        [SerializeField] private TextMeshProUGUI instructionsText;
        [SerializeField] private Button startButton;
        [SerializeField] private Material oldItemMaterial;

        private List<Ordering> orderings;
        private Camera mainCamera;

        void Start() {
            if (instance == null) {
                instance = this;
            } else {
                Destroy(this.gameObject);
            }

            mainCamera = Camera.main;
            // StartCoroutine(LoadDataAndVizualize());
        }

        public static void StartVisualization() {
            instance.startButton.gameObject.SetActive(false);

            instance.productNameText.gameObject.SetActive(true);
            instance.binNameText.gameObject.SetActive(true);
            instance.instructionsText.gameObject.SetActive(true);

            instance.StartCoroutine(instance.LoadDataAndVizualize());
        }

        IEnumerator LoadDataAndVizualize() {
            // Load data
            yield return StartCoroutine(JsonUtilityWrapper.LoadContent());
            orderings = JsonUtilityWrapper.FromJsonList();

            // Visualize results
            yield return StartCoroutine(VisualizeContainers());

            startButton.gameObject.SetActive(true);

            instance.productNameText.gameObject.SetActive(false);
            instance.binNameText.gameObject.SetActive(false);
            instance.instructionsText.gameObject.SetActive(false);

            yield return null;
        }

        IEnumerator VisualizeContainers() {
            foreach (var ordering in orderings) {
                Bin bin = ordering.bin;
                // Create and scale container box
                GameObject binGO = Instantiate(binPrefab);
                binGO.name = bin.name;
                binNameText.text = bin.name;

                // Set size (scale) to match dimensions
                binGO.transform.localScale = new Vector3(bin.width / 100f, bin.height / 100f, bin.depth / 100f);
                binGO.transform.position = new Vector3(bin.width / 200f, bin.height / 200f, bin.depth / 200f);

                mainCamera.transform.position = new Vector3(bin.width / 200f, bin.height / 200f + 2f, -1.1f);
                mainCamera.transform.rotation = Quaternion.Euler(40f, 0f, 0f);

                // Sort by product bottom Y
                // var sortedProducts = container.items.OrderBy(p => p.position[1]).ToList();
                var sortedItems = ordering.items.OrderBy(p => p.position[1] - (p.height / 2)).ToList();

                List<GameObject> itemGOs = new List<GameObject>();
                foreach (var item in sortedItems) {
                    // TODO: Change material of last item
                    UpdateMaterials(itemGOs);

                    GameObject itemGO = Instantiate(
                        itemPrefab,
                        new Vector3(binGO.transform.localScale.x / 2f, bin.height / 200f + 1f, binGO.transform.localScale.z / 2f),
                        Quaternion.identity
                    );

                    // Set size
                    itemGO.transform.localScale = new Vector3(item.width / 100f - 0.005f, item.height / 100f - 0.005f, item.depth / 100f - 0.005f);

                    // Add item to list, so that we can destroy it later
                    itemGOs.Add(itemGO);

                    // Update UI
                    productNameText.text = item.name;
                    instructionsText.text = "";

                    // Animate rotation and position
                    yield return StartCoroutine(AnimateItem(itemGO, item));

                    // Wait for key press
                    yield return new WaitUntil(() => Input.anyKeyDown);
                }

                // Add instruction to close the box
                instructionsText.text = "Fill the empty space and close the current box.";

                // Wait for key press
                yield return new WaitUntil(() => Input.anyKeyDown);

                Destroy(binGO);
                itemGOs.ForEach(itemGO => Destroy(itemGO));
            }
        }

        IEnumerator AnimateItem(GameObject itemGO, Item item) {
            float duration = 1f;
            float elapsed = 0f;

            // Make first rotation
            Quaternion targetRotation = Quaternion.Euler(item.eulerRotations[0].x, item.eulerRotations[0].y, item.eulerRotations[0].z);
            AddRotationInstruction(
                Vector3.zero,
                new Vector3(item.eulerRotations[0].x, item.eulerRotations[0].y, item.eulerRotations[0].z)
            );

            Vector3 initialPosition = itemGO.transform.position;
            Quaternion initialRotation = itemGO.transform.rotation;

            while (elapsed < duration) {
                float t = elapsed / duration;
                itemGO.transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            itemGO.transform.rotation = targetRotation;
            initialRotation = itemGO.transform.rotation;

            // Make second rotation
            elapsed = 0f;
            targetRotation = Quaternion.Euler(item.eulerRotations[1].x, item.eulerRotations[1].y, item.eulerRotations[1].z);
            AddRotationInstruction(
                new Vector3(item.eulerRotations[0].x, item.eulerRotations[0].y, item.eulerRotations[0].z),
                new Vector3(item.eulerRotations[1].x, item.eulerRotations[1].y, item.eulerRotations[1].z)
            );
            while (elapsed < duration) {
                float t = elapsed / duration;
                itemGO.transform.rotation = Quaternion.Slerp(initialRotation, targetRotation, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            itemGO.transform.rotation = targetRotation;

            // Move item
            Vector3 targetPosition = CalculateTargetPosition(item);
            AddMoveInstruction(new Vector3(item.position[0], item.position[1], item.position[2]));
            elapsed = 0f;
            while (elapsed < duration) {
                float t = elapsed / duration;
                itemGO.transform.position = Vector3.Lerp(initialPosition, targetPosition, t);

                elapsed += Time.deltaTime;
                yield return null;
            }

            itemGO.transform.position = targetPosition;
        }

        private Vector3 CalculateTargetPosition(Item item) {
            switch (item.rotation) {
                case 0:
                    return new Vector3(
                        item.position[0] + item.width / 2f,
                        item.position[1] + item.height / 2f,
                        item.position[2] + item.depth / 2f
                    ) / 100f;
                case 1:
                    return new Vector3(
                        item.position[0] + item.height / 2f,
                        item.position[1] + item.width / 2f,
                        item.position[2] + item.depth / 2f
                    ) / 100f;
                case 2:
                    return new Vector3(
                        item.position[0] + item.depth / 2f,
                        item.position[1] + item.height / 2f,
                        item.position[2] + item.width / 2f
                    ) / 100f;
                case 3:
                    return new Vector3(
                        item.position[0] + item.height / 2f,
                        item.position[1] + item.depth / 2f,
                        item.position[2] + item.width / 2f
                    ) / 100f;
                case 4:
                    return new Vector3(
                        item.position[0] + item.width / 2f,
                        item.position[1] + item.depth / 2f,
                        item.position[2] + item.height / 2f
                    ) / 100f;
                case 5:
                    return new Vector3(
                        item.position[0] + item.depth / 2f,
                        item.position[1] + item.width / 2f,
                        item.position[2] + item.height / 2f
                    ) / 100f;
            }

            return Vector3.zero;
        }

        private void AddRotationInstruction(Vector3 oldRotation, Vector3 newRotation) {
            if (oldRotation.Equals(newRotation)) {
                return;
            }

            if (oldRotation.x != newRotation.x) {
                instructionsText.text += "\nRotate the item 90 degrees on the X axis.";
                return;
            }

            if (oldRotation.y != newRotation.y) {
                instructionsText.text += "\nRotate the item 90 degrees on the Y axis.";
                return;
            }

            if (oldRotation.z != newRotation.z) {
                instructionsText.text += "\nRotate the item 90 degrees on the Z axis.";
            }
        }

        private void AddMoveInstruction(Vector3 targetPosition) {
            instructionsText.text += "\nPlace the item at " + targetPosition + ".";
        }

        private void UpdateMaterials(List<GameObject> items) {
            if (items.Count == 0) {
                return;
            }

            items[items.Count - 1].GetComponent<MeshRenderer>().materials = new Material[] { oldItemMaterial };
        }
    }
}