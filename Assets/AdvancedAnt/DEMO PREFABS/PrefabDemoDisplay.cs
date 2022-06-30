using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PrefabDemoDisplay : MonoBehaviour {

    public Text uiText;
    public GameObject BikeTrainerButtons;

    // Use this for initialization
    void Start() {
      
    }

    // Update is called once per frame
    void Update() {
        uiText.text = "";

        if (GameObject.Find("CadenceDisplay")) {
            uiText.text = "Cadence sensor - is connected ? " + GameObject.Find("CadenceDisplay").GetComponent<CadenceDisplay>().connected + "\n";
            uiText.text += "cadence: " + GameObject.Find("CadenceDisplay").GetComponent<CadenceDisplay>().cadence + "\n";
            uiText.text += "----------------------------------------------\n";
        }
        if (GameObject.Find("SpeedCadenceDisplay")) {
            uiText.text += "SpeedCadence sensor - is connected ? " + GameObject.Find("SpeedCadenceDisplay").GetComponent<SpeedCadenceDisplay>().connected + "\n";
            uiText.text += "cadence = " + GameObject.Find("SpeedCadenceDisplay").GetComponent<SpeedCadenceDisplay>().cadence +"\n";
            uiText.text += "speed = " + GameObject.Find("SpeedCadenceDisplay").GetComponent<SpeedCadenceDisplay>().speed + "\n";
            uiText.text += "distance = " + GameObject.Find("SpeedCadenceDisplay").GetComponent<SpeedCadenceDisplay>().distance + "\n";
            uiText.text += "----------------------------------------------\n";
        }
        if (GameObject.Find("HeartRateDisplay")) {
            uiText.text += "Heart rate sensor - is connected ? " + GameObject.Find("HeartRateDisplay").GetComponent<HeartRateDisplay>().connected + "\n";
            uiText.text += "HR = " + GameObject.Find("HeartRateDisplay").GetComponent<HeartRateDisplay>().heartRate + "\n";
            uiText.text += "----------------------------------------------\n";
        }
        if (GameObject.Find("SpeedDisplay")) {
            uiText.text += "Speed sensor - is connected ? " + GameObject.Find("SpeedDisplay").GetComponent<SpeedDisplay>().connected + "\n";
            uiText.text += "speed = " + GameObject.Find("SpeedDisplay").GetComponent<SpeedDisplay>().speed + "\n";
            uiText.text += "distance = " + GameObject.Find("SpeedDisplay").GetComponent<SpeedDisplay>().distance + "\n";
            uiText.text += "----------------------------------------------\n";
        }
        if (GameObject.Find("PowerMeterDisplay")) {
            uiText.text += "Power sensor - is connected ? " + GameObject.Find("PowerMeterDisplay").GetComponent<PowerMeterDisplay>().connected + "\n";
            uiText.text += "power = " + GameObject.Find("PowerMeterDisplay").GetComponent<PowerMeterDisplay>().instantaneousPower + "\n";
            uiText.text += "cadence = " + GameObject.Find("PowerMeterDisplay").GetComponent<PowerMeterDisplay>().instantaneousCadence + "\n";
           
            uiText.text += "----------------------------------------------\n";
        }
        if (GameObject.Find("FitnessEquipmentDisplay")) {
            BikeTrainerButtons.SetActive(true);
            uiText.text += "Fitness E: - is connected ? " + GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>().connected + "\n";
            uiText.text += "power = " + GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>().instantaneousPower + "\n";
            uiText.text += "speed= " + GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>().speed + "\n";
            uiText.text += "elapsedTime= " + GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>().elapsedTime + "\n";
            uiText.text += "heartRate= " + GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>().heartRate + "\n";
            uiText.text += "distanceTraveled= " + GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>().distanceTraveled + "\n";
            uiText.text += "cadence= " + GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>().cadence + "\n";

            uiText.text += "----------------------------------------------\n";
        }

    }

    public void FitnessButton1() {
        GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>().SetTrainerResistance(0);
    }
    public void FitnessButton2() {
        GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>().SetTrainerResistance(50);
    }
    public void FitnessButton3() {
        GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>().SetTrainerResistance(100);
    }
    public void FitnessButton4() {
        GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>().RequestTrainerCapabilities();
    }
    public void FitnessButton5() {
        GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>().SetTrainerUserConfiguration(10, 80); //set the user weight
    }
    public void FitnessButton6() {
        GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>().SetTrainerTargetPower(200); // set the target power in watt
    }
    public void FitnessButton7() {
        GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>().SetTrainerSlope(5); //set the trainer simulation slope in % (grade) between -200 & 200%
    }
    public void FitnessButton8() {
        GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>().RequestCommandStatus();
    }
    public void FitnessButton9() {
        GameObject.Find("FitnessEquipmentDisplay").GetComponent<FitnessEquipmentDisplay>().RequestUserConfig();
    }

}

