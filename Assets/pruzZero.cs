using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class pruzZero : MonoBehaviour {
	public KMAudio audio;
	public KMBombModule module;
	static int moduleIdCounter = 1;
	int moduleId;
	private bool moduleSolved;

	// Use this for initialization
	public TextMesh[] screen;
	public KMSelectable[] keypad;
	public KMSelectable submit;
	public KMSelectable clear;
	public KMSelectable screenToggle;
	public TextMesh colorBlindText;
	private Color[] textColors =
	{
		Color.green,
		Color.cyan,
		Color.magenta,
		new Color(1f, 0.6f, 0f)
	};
	private string colorOrder = "GCMO";
	private int[][] table =
	{
		new int[]{3, 2, 1, 0, 8, 2, 3, 8, 1},
		new int[]{1, 3, 5, 9, 7, 5, 4, 9, 0},
		new int[]{9, 7, 0, 4, 9, 6, 9, 5, 0},
		new int[]{4, 5, 3, 2, 1, 2, 2, 7, 8}
	};
	private bool screenType;
	private int TPscore;
	private bool isSubmit;
	private bool animating;
	private string submitScreen;
	private string number;
	private ArrayList solutions;
	private ArrayList colorList;
	private int currentStage;
	void Awake()
	{
		moduleId = moduleIdCounter++;
		moduleSolved = false;
		keypad[0].OnInteract += delegate () { pressedKey(0); return false; };
		keypad[1].OnInteract += delegate () { pressedKey(1); return false; };
		keypad[2].OnInteract += delegate () { pressedKey(2); return false; };
		keypad[3].OnInteract += delegate () { pressedKey(3); return false; };
		keypad[4].OnInteract += delegate () { pressedKey(4); return false; };
		keypad[5].OnInteract += delegate () { pressedKey(5); return false; };
		keypad[6].OnInteract += delegate () { pressedKey(6); return false; };
		keypad[7].OnInteract += delegate () { pressedKey(7); return false; };
		keypad[8].OnInteract += delegate () { pressedKey(8); return false; };
		keypad[9].OnInteract += delegate () { pressedKey(9); return false; };
		clear.OnInteract += delegate () { pressClear(); return false; };
		submit.OnInteract += delegate () { pressSubmit(); return false; };
		screenToggle.OnInteract += delegate () { pressScreen(); return false; };
	}
	void Start () 
	{
		Debug.LogFormat("[0 #{0}] Starting up module", moduleId);
		TPscore = 0;
		isSubmit = false;
		screenType = false;
		submitScreen = "";
		generatePuzzle();
	}
	void generatePuzzle()
	{
		number = UnityEngine.Random.Range(1, 10) + "";
		int n = UnityEngine.Random.Range(0, 4);
		string colors = colorOrder[n] + "";
		screen[0].color = textColors[n];
		screen[0].text = number[0] + "";
		for (int aa = 1; aa < screen.Length; aa++)
		{
			number =  number + "" + UnityEngine.Random.Range(0, 10);
			n = UnityEngine.Random.Range(0, 4);
			colors = colors + "" + colorOrder[n];
			screen[aa].color = textColors[n];
			screen[aa].text = number[aa] + "";
		}
		solutions = new ArrayList();
		colorList = new ArrayList();
		currentStage = 0;
		colorList.Add(colors.ToUpperInvariant());
		solutions.Add(getSolution(number, colors, 1));
		
		while(!((string)solutions[solutions.Count - 1]).Equals("0"))
		{
			colors = "";
			for (int aa = 0; aa < ((string)solutions[solutions.Count - 1]).Length; aa++)
			{
				n = UnityEngine.Random.Range(0, 4);
				colors = colors + "" + colorOrder[n];
			}
			colorList.Add(colors.ToUpperInvariant());
			solutions.Add(getSolution((string)solutions[solutions.Count - 1], colors, solutions.Count + 1));
		}
		getScreen();	
	}
	string getSolution(string number, string colors, int stage)
	{
		Debug.LogFormat("[0 #{0}] Stage #{1}", moduleId, stage);
		Debug.LogFormat("[0 #{0}] Generated Number: {1}", moduleId, number);
		Debug.LogFormat("[0 #{0}] Generated Colors: {1}", moduleId, colors);
		string number2 = "";
		for (int aa = 0; aa < number.Length; aa++)
			number2 = number2 + "" + table[colorOrder.IndexOf(colors[aa])][aa];
		Debug.LogFormat("[0 #{0}] Number made from colors: {1}", moduleId, number2);
		string newNumber = number + "";
		for (int aa = 0; aa < number.Length; aa++)
		{
			if (number[aa] == number2[aa])
			{
				newNumber = number.Substring(aa);
				break;
			}
		}
		Debug.LogFormat("[0 #{0}] Number Received: {1}", moduleId, newNumber);
		while(number2.Length > 1)
		{
			int sum = 0;
			for (int aa = 0; aa < number2.Length; aa++)
				sum += (number2[aa] - '0');
			number2 = sum + "";
		}
		Debug.LogFormat("[0 #{0}] Digital Root of colors: {1}", moduleId, number2);
		long num = (long.Parse(newNumber) * long.Parse(number2)) / 10;
		Debug.LogFormat("[0 #{0}] {1} * 0.{2} = {3}", moduleId, newNumber, number2, num);
		string solution = num + "";
		return solution;
	}
	void getScreen()
	{
		if(screenType)
		{
			for (int aa = 0; aa < screen.Length; aa++)
			{
				if (aa < number.Length)
				{
					screen[aa].color = Color.white;
					screen[aa].text = number[aa] + "";
					colorBlindText.text = colorBlindText.text + "" + ((string)colorList[currentStage])[aa];
				}
				else
					screen[aa].text = "";
			}
		}
		else
		{
			for (int aa = 0; aa < screen.Length; aa++)
			{
				if (aa < number.Length)
				{
					screen[aa].color = textColors[colorOrder.IndexOf(((string)colorList[currentStage])[aa])];
					screen[aa].text = number[aa] + "";
				}
				else
					screen[aa].text = "";
			}
		}
	}
	void pressScreen()
	{
		if (!(moduleSolved) && !animating)
		{
			audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, screenToggle.transform);
			if (!(isSubmit))
			{
				if (screenType)
				{
					for (int aa = 0; aa < screen.Length; aa++)
					{
						if (aa < number.Length)
						{
							screen[aa].color = textColors[colorOrder.IndexOf(((string)colorList[currentStage])[aa])];
						}
						else
							screen[aa].text = "";
					}
					colorBlindText.text = "";
				}
				else
				{
					for (int aa = 0; aa < screen.Length; aa++)
					{
						if (aa < number.Length)
						{
							screen[aa].color = Color.white;
							colorBlindText.text = colorBlindText.text + "" + ((string)colorList[currentStage])[aa];
						}
						else
							screen[aa].text = "";
					}
				}
				screenType = !(screenType);
			}
		}
	}
	IEnumerator nextStage()
	{
		animating = true;
		string temp = "CORRECT!!";
		for (int aa = 0; aa < screen.Length; aa++)
		{
			screen[aa].text = temp[aa] + "";
			screen[aa].color = Color.green;
		}
		yield return new WaitForSeconds(1.0f);
		number = (string)solutions[currentStage];
		currentStage++;
		getScreen();
		submitScreen = "";
		isSubmit = false;
		animating = false;
	}
	IEnumerator strike()
	{
		animating = true;
		yield return new WaitForSeconds(1.0f);
		getScreen();
		isSubmit = false;
		submitScreen = "";
		animating = false;
	}
	void pressedKey(int n)
	{
		if(!(moduleSolved) && !animating)
		{
			audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, keypad[n].transform);
			if (!(isSubmit))
			{
				colorBlindText.text = "";
				isSubmit = true;
				for (int aa = 0; aa < screen.Length; aa++)
				{
					screen[aa].color = Color.white;
					screen[aa].text = "";
				}
			}
			if (submitScreen.Length < 9)
			{
				submitScreen = submitScreen + "" + n;
				screen[submitScreen.Length - 1].text = n + "";
			}
		}
	}
	void pressClear()
	{
		if(!(moduleSolved) && !animating)
		{
			if(isSubmit)
            {
				audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, clear.transform);
				getScreen();
				isSubmit = false;
				submitScreen = "";
			}
		}
	}
	void pressSubmit()
	{
		if(!(moduleSolved) && !animating)
		{
			if(isSubmit)
			{
				audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, submit.transform);
				Debug.LogFormat("[0 #{0}] You entered: {1}", moduleId, submitScreen);
				if (submitScreen.Equals((string)solutions[currentStage]))
				{
					audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.CorrectChime, transform);
					Debug.LogFormat("[0 #{0}] That is correct!", moduleId);
					TPscore += ((number.Length * 34) / 100) + 1;
					if (submitScreen.Equals("0"))
					{
						submitScreen = "SOLVED!!!";
						for (int aa = 0; aa < screen.Length; aa++)
						{
							screen[aa].text = submitScreen[aa] + "";
							screen[aa].color = Color.white;
						}
						module.HandlePass();
						moduleSolved = true;
					}
					else
						StartCoroutine(nextStage());
				}
				else
				{
					Debug.LogFormat("[0 #{0}] I was expecting {1}", moduleId, (string)solutions[currentStage]);
					string temp = "INCORRECT";
					for (int aa = 0; aa < screen.Length; aa++)
					{
						screen[aa].text = temp[aa] + "";
						screen[aa].color = Color.red;
					}
					module.HandleStrike();
					StartCoroutine(strike());
				}
			}
		}
	}

	#pragma warning disable 414
	private readonly string TwitchHelpMessage = @"!{0} screen to toggle color blind mode. !{0} submit|sub|s 1234567890 to submit the number.";
	#pragma warning restore 414
	IEnumerator ProcessTwitchCommand(string command)
	{
		string[] param = command.Split(' ');
		if (param.Length == 1 && (Regex.IsMatch(param[0], @"^\s*screen\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)))
		{
			yield return new WaitForSeconds(0.1f);
			screenToggle.OnInteract();
		}
		else if ((Regex.IsMatch(param[0], @"^\s*submit\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(param[0], @"^\s*sub\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) || Regex.IsMatch(param[0], @"^\s*s\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)) && param.Length > 1)
		{
			string check = "";
			for (int aa = 1; aa < param.Length; aa++)
				check = check + "" + param[aa];
			bool flag = true;
			for(int aa = 0; aa < check.Length; aa++)
			{
				if("0123456789".IndexOf(check[aa]) < 0)
				{
					flag = false;
					break;
				}
			}
			if (check.Length > 9)
				flag = false;
			if (flag)
			{
				yield return null;
				for (int aa = 0; aa < check.Length; aa++)
				{
					keypad[check[aa] - '0'].OnInteract();
					yield return new WaitForSeconds(0.1f);
				}
				if (submitScreen.Equals("0") && ((string)solutions[currentStage]).Equals("0"))
                {
					TPscore += ((number.Length * 34) / 100) + 1;
					yield return "awardpointsonsolve " + TPscore;
				}
				submit.OnInteract();
			}
			else
				yield break;
		}
		else
			yield break;
	}

	IEnumerator TwitchHandleForcedSolve()
    {
		while (!moduleSolved)
        {
			while (animating) yield return true;
			if (submitScreen.Length > ((string)solutions[currentStage]).Length)
			{
				clear.OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
			else
			{
				for (int i = 0; i < submitScreen.Length; i++)
				{
					if (i == ((string)solutions[currentStage]).Length)
						break;
					if (submitScreen[i] != ((string)solutions[currentStage])[i])
					{
						clear.OnInteract();
						yield return new WaitForSeconds(0.1f);
						break;
					}
				}
			}
			string sub = ((string)solutions[currentStage]).Substring(submitScreen.Length);
			for (int aa = 0; aa < sub.Length; aa++)
			{
				keypad[sub[aa] - '0'].OnInteract();
				yield return new WaitForSeconds(0.1f);
			}
			submit.OnInteract();
		}
	}
}
