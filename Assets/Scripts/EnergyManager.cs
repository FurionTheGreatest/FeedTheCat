 using UnityEngine;
 using System;
 using System.Globalization;
 using TMPro;
 using UnityEngine.UI;

 public class EnergyManager : MonoBehaviour
 {
     public Slider energySlider;
     public TMP_Text energyText;
     private const int MaxEnergy = 20;
 
     private static TimeSpan _newLifeInterval = new TimeSpan(0,10,0);

     private DateTime _lostLifeTimeStamp;
     private int _livesLeft = MaxEnergy;
     private int _amountOfIntervalsPassed;
     private const string LivesLeftPrefsString = "LivesLeft";
     private const string LostLifeTimeStampPrefsString = "LifeLostTimeStamp";

     public static EnergyManager instance;
     private void Start()
     {
         if (instance == null)
             instance = this;
         
         DontDestroyOnLoad(gameObject);

         energySlider = GameObject.Find("Energy").GetComponentInChildren<Slider>();
         energyText = energySlider.gameObject.GetComponentInChildren<TMP_Text>();
         
         if (PlayerPrefs.HasKey(LostLifeTimeStampPrefsString))
         {
             _livesLeft = PlayerPrefs.GetInt(LivesLeftPrefsString);
             PlayerPrefs.GetString(LostLifeTimeStampPrefsString);
             
             _lostLifeTimeStamp = Convert.ToDateTime(PlayerPrefs.GetString(LostLifeTimeStampPrefsString));
             
             CalculateTimeDifference();
             
             ChangeEnergyUi();
         }
     }

     private void Update() 
     {
         if (_livesLeft < MaxEnergy)
         {
            CalculateTimeDifference();
         }
     }
     [ContextMenu ("Lose Life")]
     public void LostLife()
     {
         _livesLeft -= 5;
         if (_livesLeft < MaxEnergy)
         {
             // mark the timestamp only when lives drop from MAX to MAX -1
             _lostLifeTimeStamp = DateTime.Now;
 
            SaveTimeStamp();
         }
         ChangeEnergyUi();
     }
     public void SaveTimeStamp()
     {
         PlayerPrefs.SetInt(LivesLeftPrefsString, _livesLeft);
         PlayerPrefs.SetString(LostLifeTimeStampPrefsString, _lostLifeTimeStamp.ToString(CultureInfo.CurrentCulture));
     }

     private void CalculateTimeDifference()
     {
         TimeSpan timeDifference = DateTime.Now - _lostLifeTimeStamp;
 
         try 
         {
             // round down or we get a new life whenever over half of interval has passed
             var intervalD = Math.Floor(timeDifference.TotalSeconds / _newLifeInterval.TotalSeconds);
             _amountOfIntervalsPassed = Convert.ToInt32(intervalD);
         }
         catch (OverflowException) 
         {
             // something has probably gone wrong. give full lives. normalize the situation
             _livesLeft = MaxEnergy;
             ChangeEnergyUi();
         }   
 
         if (_amountOfIntervalsPassed + _livesLeft >= MaxEnergy)
         {
             _livesLeft = MaxEnergy;
             ChangeEnergyUi();
         }
 
         Debug.Log("it's been " + timeDifference + " since lives dropped from full (now "+_livesLeft+"). " + _amountOfIntervalsPassed + " reloads passed. Lives Left: " + GetAmountOfLives() );
     }
     private int GetAmountOfLives()
     {
         return Mathf.Min(_livesLeft + _amountOfIntervalsPassed, MaxEnergy);
     }

     private void ChangeEnergyUi()
     {
         if (energyText != null)
         {
             energySlider.maxValue = MaxEnergy;
             energySlider.value = _livesLeft;
             energyText.text = _livesLeft + "/" + MaxEnergy;
         }
         else Debug.Log("no text field for energy");
     }

     private void OnDestroy()
     {
         SaveTimeStamp();
     }
 }