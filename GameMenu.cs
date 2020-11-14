using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameMenu : MonoBehaviour
{
    //if there is a menu it might have a camera
    public GameObject menuUI;
    public GameObject menuCamera;

    //the character as it appears in the menu
    //it can be a variant of the avatar prefab, make sure the interactive components are disabled
    public GameObject menuAvatar;
    public Appearance menuAvatarAppearance;

    //the button on the interface that fires the join event
    public Button joinButton;

    //using text mesh pro for better text rendering 
    public TMP_InputField nickNameField;

    //two example sliders
    public Slider s0;
    public Slider s1;



    // Start is called before the first frame update
    void Start()
    {
        //simple character creator using a copy of the avatar prefab
        if (menuAvatar != null)
        {
            //to preview the avatar appearance I save the script
            menuAvatarAppearance = menuAvatar.GetComponent<Appearance>();

            s0.onValueChanged.AddListener(OnS0);
            s1.onValueChanged.AddListener(OnS1);

            menuAvatarAppearance.changeHead(s0.value);
            menuAvatarAppearance.changeSkinColor(s1.value);
        }

        OpenMenu();

    }

    // Update is called once per frame
    void Update()
    {

        joinButton.interactable = Net.connected;
    }

    //take the data and submit it
    public void Join()
    {
        if (Net.connected)
        {
            AvatarData d = new AvatarData();
            d.nickName = nickNameField.text;

            //the avatar appearance is defined by the "DNA", an array of floats
            d.DNA = new float[2];

            d.DNA[0] = s0.value;
            d.DNA[1] = s1.value;

            //I have a global reference of netmanager in Net
            Net.manager.SendAvatarData(d);
            
        }
        else
        {
            Debug.Log("Error: Not connected");
        }
    }

    public void OpenMenu()
    {
        //make sure the cursor is available
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        nickNameField.Select();
        nickNameField.ActivateInputField();

        menuCamera.SetActive(true);

        if(menuAvatar != null)
           menuAvatar.SetActive(true);

        menuUI.SetActive(true);
    }

    public void CloseMenu()
    {
        menuCamera.SetActive(false);
        menuAvatar.SetActive(false);
        menuUI.SetActive(false);
    }

    //sliders listeners
    public void OnS0(float value)
    {
        menuAvatarAppearance.changeSkinColor(value);
    }

    public void OnS1(float value)
    {
        menuAvatarAppearance.changeHead(value);
        menuAvatarAppearance.changeSkinColor(s0.value);
    }
}
