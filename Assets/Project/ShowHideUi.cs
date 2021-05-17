using UnityEngine;

namespace Project
{
    public class ShowHideUi : MonoBehaviour
    {
        [SerializeField] private KeyCode keyCode = KeyCode.H;
        [SerializeField] private GameObject container;

        private void Update()
        {
            if (Input.GetKeyDown(keyCode))
            {
                container.SetActive(!container.activeSelf);
            }
        }
    }
}